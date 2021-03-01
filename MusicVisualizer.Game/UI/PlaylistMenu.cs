using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using YoutubeExplode;
using YoutubeExplode.Playlists;

namespace MusicVisualizer.Game.UI
{
    public class PlaylistMenu : FocusedOverlayContainer
    {
        [Resolved]
        private YoutubeClient youtube { get; set; }

        [Resolved(canBeNull: true)]
        private QueueManager queue { get; set; }

        private const int width = 430;

        private FillFlowContainer itemFlow;

        [BackgroundDependencyLoader]
        private void load()
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

        public void SetPlaylist(IEnumerable<PlaylistVideo> videos)
        {
            itemFlow.Clear();

            foreach (var video in videos)
            {
                itemFlow.Add(new PlaylistMenuItem(video)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Action = () => queue?.PlayVideo(video.Id)
                });
            }
        }
    }
}
