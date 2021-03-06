﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FFMpegCore;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MusicVisualizer.Game.IO
{
    public class FileStore
    {
        public readonly Storage Storage;

        public readonly IResourceStore<byte[]> Store;

        private readonly YoutubeClient youtube;

        public FileStore(Storage storage, YoutubeClient youtube)
        {
            Storage = storage.GetStorageForDirectory(@"songs");
            Store = new StorageBackedResourceStore(Storage);

            this.youtube = youtube;
        }

        public async Task<(string, string)> GetYoutubeFiles(string id, Action<double> videoProgress = null, Action<double> audioProgress = null)
        {
            var audioPath = $"{id}/audio.mp3";
            var videoPath = $"{id}/video.webm";

            bool audioExists = Storage.Exists(audioPath);
            bool videoExists = Storage.Exists(videoPath);

            if (!videoExists || !audioExists)
            {
                var youtubeVideo = await youtube.Videos.Streams.GetManifestAsync(id);

                var videoStreamInfo = youtubeVideo.GetVideoOnly().WithHighestVideoQuality();
                var audioStreamInfo = youtubeVideo.GetAudioOnly().WithHighestBitrate();

                List<Task> tasks = new List<Task>();

                if (!videoExists && videoStreamInfo != null)
                {
                    Logger.Log($"Downloading {videoPath}");

                    var progress = new Progress<double>(videoProgress ?? (d => { }));

                    tasks.Add(youtube.Videos.Streams.DownloadAsync(videoStreamInfo, Storage.GetFullPath(videoPath, true), progress));
                }

                if (!audioExists && audioStreamInfo != null)
                {
                    Logger.Log($"Downloading {audioPath}");

                    TimeSpan? length = null;

                    tasks.Add(FFMpegArguments.FromUrlInput(new Uri(audioStreamInfo.Url))
                                             .OutputToFile(Storage.GetFullPath(audioPath, true))
                                             .NotifyOnProgress(time =>
                                             {
                                                 Trace.Assert(length.HasValue);

                                                 audioProgress?.Invoke(time.TotalMilliseconds / length.Value.TotalMilliseconds);
                                             })
                                             .NotifyOnOutput((l, _) =>
                                             {
                                                 l = l.Trim();
                                                 const string token = "Duration: ";

                                                 if (!l.StartsWith(token)) return;

                                                 var timeStr = l.Substring(token.Length).Split(", ")[0];

                                                 length = TimeSpan.Parse(timeStr);
                                             })
                                             .ProcessAsynchronously());
                }

                await Task.WhenAll(tasks);
            }

            return (videoPath, audioPath);
        }
    }
}
