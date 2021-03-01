using System.Collections.Generic;
using System.Threading.Tasks;
using MusicVisualizer.Game.IO;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using YoutubeExplode;
using YoutubeExplode.Playlists;

namespace MusicVisualizer.Game.UI
{
    public class QueueManager : Component
    {
        [Resolved]
        private YoutubeClient youtube { get; set; }

        [Resolved]
        private FileStore store { get; set; }

        [Resolved]
        private ITrackStore tracks { get; set; }

        [Resolved(canBeNull: true)]
        private BackgroundVideo backgroundVideo { get; set; }

        [Resolved(canBeNull: true)]
        private ProgressOverlay progress { get; set; }

        [Resolved]
        private PlaylistMenu playlistMenu { get; set; }

        [Resolved(canBeNull: true)]
        private VisConfigManager config { get; set; }

        public readonly Bindable<Track> Track = new Bindable<Track>();
        public readonly Bindable<string> Video = new Bindable<string>();

        private readonly List<PlaylistVideo> playlist = new List<PlaylistVideo>();

        private bool shouldBePlaying;
        private int playingIndex;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (config == null) return;

            config.GetBindable<string>(VisSetting.Playlist).ValueChanged += e =>
            {
                SetPlaylist(e.NewValue);
            };

            SetPlaylist(config.Get<string>(VisSetting.Playlist));
        }

        public void TogglePause()
        {
            var t = Track.Value;
            if (t == null) return;

            if (t.IsRunning)
                t.Stop();
            else
                t.Start();
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

        public void SetPlaylist(string id) => Schedule(() => // schedule due to dependency on youtube
        {
            Task.Run(async () =>
            {
                var videos = await youtube.Playlists.GetVideosAsync(id);

                playlist.Clear();
                playlist.AddRange(videos);

                Schedule(() =>
                {
                    playlistMenu.SetPlaylist(videos);
                });
            });
        });

        public async void PlayVideo(string id)
        {
            shouldBePlaying = false; // prevent infinite loop of downloading every video in case the track does not start immediately

            ProgressOverlay.ProgressBar progressVideo = null, progressAudio = null;

            var (videoPath, audioPath) = await store.GetYoutubeFiles(id, d =>
            {
                if (progress == null) return;

                Schedule(() =>
                {
                    progressVideo ??= progress.AddItem($"{id} - Video");
                    progressVideo.Progress = d;
                });
            }, d =>
            {
                if (progress == null) return;

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
                backgroundVideo?.Play(videoPath);
            });
        }
    }
}
