using MusicVisualizer.Game.IO;
using MusicVisualizer.Resources;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;
using YoutubeExplode;

namespace MusicVisualizer.Game
{
    public class MusicVisualizerGameBase : osu.Framework.Game
    {
        // Anything in this class is shared between the test browser and the game implementation.
        // It allows for caching global dependencies that should be accessible to tests, or changing
        // the screen scaling for all components including the test browser and framework overlays.

        protected override Container<Drawable> Content { get; }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        protected FileStore Store { get; private set; }

        protected YoutubeClient Youtube { get; private set; }

        protected MusicVisualizerGameBase()
        {
            Name = "MusicVisualizer";

            // Ensure game and tests scale with window size and screen DPI.
            base.Content.Add(Content = new DrawSizePreservingFillContainer
            {
                // You may want to change TargetDrawSize to your "default" resolution, which will decide how things scale and position when using absolute coordinates.
                TargetDrawSize = new Vector2(1366, 768)
            });
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            Resources.AddStore(new DllResourceStore(typeof(MusicVisualizerResources).Assembly));

            AddFont(Resources, @"Fonts/Noto-Sans-CJK-JP");

            dependencies.CacheAs(Youtube = new YoutubeClient());

            dependencies.Cache(Store = new FileStore(storage, Youtube));

            dependencies.CacheAs(Audio.GetTrackStore(Store.Store));

            Audio.Tracks.AddAdjustment(AdjustableProperty.Volume, new BindableDouble(0.25));
        }
    }
}
