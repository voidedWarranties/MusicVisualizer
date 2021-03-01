using MusicVisualizer.Game.UI;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;

namespace MusicVisualizer.Game.Tests.Visual
{
    public class TestScenePlaylistMenu : TestScene
    {
        [Cached]
        private readonly QueueManager queue = new();

        [Cached]
        private readonly PlaylistMenu menu;

        public TestScenePlaylistMenu()
        {
            AddRange(new Drawable[]
            {
                queue,
                menu = new PlaylistMenu
                {
                    RelativeSizeAxes = Axes.Y
                }
            });

            queue.SetPlaylist("PLwBnYkSZTLgIGr1_6l5pesUY0TZZFIy_b");
        }

        [Test]
        public void TestShow()
        {
            AddStep("Show", menu.Show);
        }

        [Test]
        public void TestHide()
        {
            AddStep("Hide", menu.Hide);
        }
    }
}
