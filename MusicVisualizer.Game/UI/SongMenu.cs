using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicVisualizer.Game.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using YoutubeExplode;

namespace MusicVisualizer.Game.UI
{
    public class SongMenu : Menu
    {
        [Resolved]
        private YoutubeClient youtube { get; set; }

        public Action PlayPause { get; set; }

        public Action<string> PlayYoutube;

        private List<MenuItem> songSubmenuItems;

        public SongMenu(Direction direction, bool topLevelMenu = true)
            : base(direction, topLevelMenu)
        {
            BackgroundColour = FrameworkColour.Blue;
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, FileStore store)
        {
            Items = new[]
            {
                new MenuItem("Open Folder", () => host.OpenFileExternally(store.Storage.GetFullPath("."))),
                new MenuItem("Play/Pause", PlayPause),
                new MenuItem("Songs")
                {
                    Items = songSubmenuItems = new List<MenuItem>()
                }
            };
        }

        public async Task SetPlaylist(string id)
        {
            songSubmenuItems.Clear();

            await foreach (var video in youtube.Playlists.GetVideosAsync(id))
            {
                songSubmenuItems.Add(new MenuItem(video.Title, () => PlayYoutube?.Invoke(video.Id)));
            }
        }

        protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new SongMenuItem(item);

        protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) => new BasicScrollContainer(direction);

        protected override Menu CreateSubMenu() => new SongMenu(Direction.Vertical);

        public class SongMenuItem : DrawableMenuItem
        {
            public SongMenuItem(MenuItem item)
                : base(item)
            {
                BackgroundColour = FrameworkColour.BlueGreen;
                BackgroundColourHover = FrameworkColour.Green;
            }

            protected override Drawable CreateContent() => new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Padding = new MarginPadding(2),
                Font = FrameworkFont.Condensed
            };
        }
    }
}
