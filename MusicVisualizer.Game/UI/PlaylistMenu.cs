using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using YoutubeExplode;

namespace MusicVisualizer.Game.UI
{
    public class PlaylistMenu : FocusedOverlayContainer
    {
        [Resolved]
        private YoutubeClient youtube { get; set; }

        private const int width = 430;

        private readonly FillFlowContainer itemFlow;

        public Action<string> PlayYoutube;

        public PlaylistMenu()
        {
            Width = width;
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            RelativePositionAxes = Axes.X;

            X = 1;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.DarkGray.Darken(2),
                    Depth = int.MaxValue
                },
                new PlaylistScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = itemFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(5)
                    }
                }
            };
        }

        protected override void PopIn() => this.MoveToX(0, 300, Easing.OutExpo);

        protected override void PopOut() => this.MoveToX(1, 300, Easing.InQuint);

        public void SetPlaylist(string id) => Schedule(() =>
        {
            itemFlow.Clear();

            Task.Run(async () =>
            {
                var videos = await youtube.Playlists.GetVideosAsync(id);

                foreach (var video in videos)
                {
                    var item = new PlaylistMenuItem(video)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Action = () => PlayYoutube?.Invoke(video.Id)
                    };

                    _ = LoadComponentAsync(item, itemFlow.Add);
                }
            });
        });

        private class PlaylistScrollContainer : ScrollContainer<Drawable>
        {
            protected override ScrollbarContainer CreateScrollbar(Direction direction) =>
                new PlaylistScrollbar(direction);

            private class PlaylistScrollbar : ScrollbarContainer
            {
                private const float dim_size = 8;

                public PlaylistScrollbar(Direction direction)
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
}
