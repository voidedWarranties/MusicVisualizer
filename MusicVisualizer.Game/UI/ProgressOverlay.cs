using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osuTK;

namespace MusicVisualizer.Game.UI
{
    public class ProgressOverlay : OverlayContainer
    {
        private const int width = 400;
        private const int height = 250;

        private const int header_height = 30;

        protected override void PopIn() => this.MoveToY(0, 300, Easing.OutExpo);

        protected override void PopOut() => this.MoveToY(1, 300, Easing.InQuint);

        private readonly FillFlowContainer<ProgressBar> progressFlow;

        public ProgressOverlay()
        {
            Width = width;
            Height = height;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            RelativePositionAxes = Axes.Y;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black.Opacity(0.6f)
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.Black.Opacity(0.9f)
                        },
                        new SpriteText
                        {
                            Text = "Download Progress",
                            Font = FontUsage.Default.With(size: header_height - 5),
                            Padding = new MarginPadding { Left = 5 },
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = header_height },
                    Child = new VisScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = progressFlow = new FillFlowContainer<ProgressBar>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5)
                        }
                    }
                }
            };
        }

        private void checkComplete()
        {
            Purge();

            using (BeginDelayedSequence(250))
            {
                if (progressFlow.Any() && progressFlow.Children.All(p => p.IsComplete))
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
        }

        public void Purge()
        {
            foreach (var child in progressFlow)
            {
                if (child.IsComplete)
                {
                    child.OnProgress = null;
                    child.FadeOut(200, Easing.OutQuint).Expire();
                }
            }
        }

        public ProgressBar AddItem(string label)
        {
            ProgressBar bar = new ProgressBar(label)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                OnProgress = checkComplete
            };

            Schedule(() => progressFlow.Add(bar));
            Show();

            return bar;
        }

        public class ProgressBar : CompositeDrawable
        {
            private const int bar_height = 8;

            public Action OnProgress;

            public bool IsComplete;

            public double Progress
            {
                set
                {
                    bar.ClearTransforms();

                    bar.ResizeWidthTo((float)value, 200, Easing.OutQuad);

                    IsComplete = Precision.AlmostEquals(value, 1, 0.1);

                    OnProgress?.Invoke();
                }
            }

            private readonly Box bar;

            public ProgressBar(string label)
            {
                RelativeSizeAxes = Axes.X;
                Height = bar_height * 3;

                Padding = new MarginPadding(5);

                InternalChildren = new Drawable[]
                {
                    new SpriteText
                    {
                        RelativeSizeAxes = Axes.X,
                        Font = FontUsage.Default.With(size: bar_height * 2 - 2),
                        Text = label
                    },
                    bar = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Y = bar_height * 2,
                        Colour = Colour4.Blue,
                        Height = bar_height
                    }
                };
            }
        }
    }
}
