using MusicVisualizer.Game.IO;
using MusicVisualizer.Game.UI;
using MusicVisualizer.Game.UI.Visualisers;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osuTK.Graphics;

namespace MusicVisualizer.Game
{
    public class MainScreen : Screen
    {
        private SongMenu songMenu;

        private BackgroundVideo backgroundVideo;

        private Bindable<SongConfig> config = new Bindable<SongConfig>();

        private Bindable<Track> track = new Bindable<Track>();

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(ITrackStore tracks, FileStore store)
        {
            track.ValueChanged += ev =>
            {
                ev.OldValue?.Stop();
                ev.OldValue?.Dispose();
            };

            dependencies.Cache(backgroundVideo = new BackgroundVideo
            {
                Config = { BindTarget = config },
                Track = { BindTarget = track }
            });

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
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
                        }
                    }
                },
                songMenu = new SongMenu (Direction.Vertical)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    PlayFile = file =>
                    {
                        config.Value = file;
                        config.TriggerChange();

                        track.Value = tracks.Get(file.Audio);
                        track.Value.Start();
                    }
                }
            };

            songMenu.UpdateItems(store.GetInis());
        }
    }
}
