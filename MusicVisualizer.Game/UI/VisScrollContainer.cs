using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace MusicVisualizer.Game.UI
{
    public class VisScrollContainer : ScrollContainer<Drawable>
    {
        public VisScrollContainer()
        {
        }

        public VisScrollContainer(Direction direction)
            : base(direction)
        {
        }

        protected override ScrollbarContainer CreateScrollbar(Direction direction) =>
            new VisScrollbar(direction);

        private class VisScrollbar : ScrollbarContainer
        {
            private const float dim_size = 8;

            public VisScrollbar(Direction direction)
                : base(direction)
            {
                Masking = true;
                CornerRadius = dim_size / 2;

                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Gray
                };
            }

            public override void ResizeTo(float val, int duration = 0, Easing easing = Easing.None)
            {
                Vector2 size = new Vector2(dim_size)
                {
                    [(int)ScrollDirection] = val
                };
                this.ResizeTo(size, duration, easing);
            }
        }
    }
}
