using MusicVisualizer.Game.UI;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;

namespace MusicVisualizer.Game.Tests.Visual
{
    public class TestSceneProgressOverlay : TestScene
    {
        private readonly ProgressOverlay overlay;

        public TestSceneProgressOverlay()
        {
            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Gray
                },
                overlay = new ProgressOverlay()
            });

            var bar = overlay.AddItem("bar 1");

            AddSliderStep("bar 1", 0, 1, 0.5, val =>
            {
                if (!bar.IsAlive)
                {
                    bar = overlay.AddItem("another bar");
                }

                bar.Progress = val;
            });
        }

        [Test]
        public void TestShow()
        {
            AddStep("Show", overlay.Show);
        }

        [Test]
        public void TestHide()
        {
            AddStep("Hide", overlay.Hide);
        }
    }
}
