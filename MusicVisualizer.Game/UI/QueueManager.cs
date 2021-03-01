using System.Collections.Generic;
using MusicVisualizer.Game.IO;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using YoutubeExplode.Playlists;

namespace MusicVisualizer.Game.UI
{
    public class QueueManager : Component
    {
        [Resolved]
        private FileStore store { get; set; }

        [Resolved]
        private ITrackStore tracks { get; set; }

        [Resolved]
        private BackgroundVideo backgroundVideo { get; set; }

        [Resolved]
        private ProgressOverlay progress { get; set; }

        public readonly Bindable<Track> Track = new Bindable<Track>();
        public readonly Bindable<string> Video = new Bindable<string>();

        private readonly List<PlaylistVideo> playlist = new List<PlaylistVideo>();

        private bool shouldBePlaying;
        private int playingIndex;

        public void SetPlaylist(IEnumerable<PlaylistVideo> videos)
        {
            playlist.Clear();
            playlist.AddRange(videos);
        }

        protected override void Update()
        {
            base.Update();

            if (Track.Value == null) return;

            if (Track.Value.IsRunning && !shouldBePlaying)
            {
                shouldBePlaying = true;
                playingIndex = playlist.FindIndex(i => i.Id == Video.Value);
            }
            else if (Track.Value.HasCompleted && shouldBePlaying)
            {
                playingIndex = (playingIndex + 1) % playlist.Count;
                PlayVideo(playlist[playingIndex].Id);
            }
        }

        public async void PlayVideo(string id)
        {
            shouldBePlaying = false; // prevent infinite loop of downloading every video in case the track does not start immediately

            ProgressOverlay.ProgressBar progressVideo = null, progressAudio = null;

            var (videoPath, audioPath) = await store.GetYoutubeFiles(id, d =>
            {
                Schedule(() =>
                {
                    progressVideo ??= progress.AddItem($"{id} - Video");
                    progressVideo.Progress = d;
                });
            }, d =>
            {
                Schedule(() =>
                {
                    progressAudio ??= progress.AddItem($"{id} - Audio");
                    progressAudio.Progress = d;
                });
            });

            Video.Value = id;

            Track.Value = await tracks.GetAsync(audioPath);
            Track.Value.Start();

            Schedule(() =>
            {
                backgroundVideo.Play(videoPath);
            });
        }
    }
}
