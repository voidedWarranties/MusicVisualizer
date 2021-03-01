using MusicVisualizer.Game.IO;
using MusicVisualizer.Game.UI;
using MusicVisualizer.Game.UI.Visualizers;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using YoutubeExplode;

namespace MusicVisualizer.Game
{
    public class MainScreen : Screen
    {
        private BackgroundVideo backgroundVideo;

        [Resolved]
        private YoutubeClient youtube { get; set; }

        [Resolved]
        private FileStore store { get; set; }

        [Resolved]
        private ITrackStore tracks { get; set; }

        private readonly Bindable<Track> track = new Bindable<Track>();
        private readonly Bindable<string> video = new Bindable<string>();

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        private ProgressOverlay progress;

        [BackgroundDependencyLoader]
        private void load()
        {
            PlaylistMenu menu;

            track.ValueChanged += ev =>
            {
                ev.OldValue?.Stop();
                ev.OldValue?.Dispose();
            };

            dependencies.Cache(backgroundVideo = new BackgroundVideo
            {
                Track = { BindTarget = track }
            });

            QueueManager queue;
            dependencies.Cache(queue = new QueueManager
            {
                Track = { BindTarget = track },
                Video = { BindTarget = video },
                PlayYoutube = playVideo
            });

            InternalChildren = new Drawable[]
            {
                queue,
                new VisualizerContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Track = { BindTarget = track },
                    Children = new Drawable[]
                    {
                        backgroundVideo,
                        new TriangleVisualizer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new BarVisualizer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new ProjectileVisualizer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        progress = new ProgressOverlay(),
                        menu = new PlaylistMenu
                        {
                            RelativeSizeAxes = Axes.Y,
                            PlayYoutube = playVideo
                        }
                    }
                },
                new SongMenu(Direction.Vertical)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    PlayPause = () =>
                    {
                        var t = track.Value;
                        if (t == null) return;

                        if (t.IsRunning)
                            t.Stop();
                        else
                            t.Start();
                    },
                    OpenPlaylist = menu.Show,
                    ClosePlaylist = menu.Hide
                }
            };
        }

        private async void playVideo(string id)
        {
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

            video.Value = id;

            track.Value = await tracks.GetAsync(audioPath);
            track.Value.Start();

            Schedule(() =>
            {
                backgroundVideo.Play(videoPath);
            });
        }
    }
}
