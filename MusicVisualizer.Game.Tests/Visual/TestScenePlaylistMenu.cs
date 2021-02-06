using MusicVisualizer.Game.UI;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;

namespace MusicVisualizer.Game.Tests.Visual
{
    public class TestScenePlaylistMenu : TestScene
    {
        private readonly PlaylistMenu menu;

        public TestScenePlaylistMenu()
        {
            Add(menu = new PlaylistMenu
            {
                RelativeSizeAxes = Axes.Y
            });

            menu.SetPlaylist("PLwBnYkSZTLgIGr1_6l5pesUY0TZZFIy_b");
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
