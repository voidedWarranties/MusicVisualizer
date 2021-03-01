using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace MusicVisualizer.Game.UI
{
    public class PlaylistMenuItem : Button
    {
        private const int width = 400;
        private const int height = 75;

        private const int padding_vertical = 5;
        private const int padded_height = height - 2 * padding_vertical;
        private const int thumbnail_width = (int)(padded_height * (16 / 9f));

        private Container thumbnailContainer;

        private readonly string title;
        private readonly string author;
        private readonly string thumbnailUrl;

        public PlaylistMenuItem(Video video)
        {
            title = video.Title;
            author = video.Author;
            thumbnailUrl = video.Thumbnails.MediumResUrl;
        }

        public PlaylistMenuItem(PlaylistVideo video)
        {
            title = video.Title;
            author = video.Author;
            thumbnailUrl = video.Thumbnails.MediumResUrl;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            Width = width;
            Height = height;

            CornerRadius = 9;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Gray.Darken(3)
                },
                thumbnailContainer = new Container
                {
                    Masking = true,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = thumbnail_width,
                    X = padding_vertical
                },
                new FillFlowContainer
                {
                    Padding = new MarginPadding { Left = padding_vertical + thumbnail_width + 10, Vertical = height / 4f },
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Font = FrameworkFont.Regular.With(size: 0.55f * height / 2),
                            RelativeSizeAxes = Axes.X,
                            Truncate = true,
                            Text = title
                        },
                        new SpriteText
                        {
                            Font = FrameworkFont.Regular.With(size: 0.45f * height / 2),
                            RelativeSizeAxes = Axes.X,
                            Truncate = true,
                            Text = author
                        }
                    }
                }
            };

            thumbnailContainer.Add(new DelayedLoadUnloadWrapper(() =>
            {
                var thumbnail = new Thumbnail(thumbnailUrl)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0
                };

                thumbnail.OnLoadComplete += d => d.FadeIn(200, Easing.OutQuad);

                return thumbnail;
            }).With(d =>
            {
                d.Anchor = Anchor.Centre;
                d.Origin = Anchor.Centre;
            }));
        }

        [LongRunningLoad]
        private class Thumbnail : Sprite
        {
            private readonly string url;

            public Thumbnail(string url)
            {
                this.url = url;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                Texture = textures.Get(url);
                Scale = new Vector2((float)thumbnail_width / Texture.Width);
            }
        }
    }
}
