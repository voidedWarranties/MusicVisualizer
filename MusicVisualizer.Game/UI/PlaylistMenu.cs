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
                new VisScrollContainer
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
    }
}
