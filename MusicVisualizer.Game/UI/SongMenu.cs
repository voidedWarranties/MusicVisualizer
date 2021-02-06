using System;
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

        public Action PlayPause;

        public Action OpenPlaylist;

        public Action ClosePlaylist;

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
                new MenuItem("Open Playlist", OpenPlaylist),
                new MenuItem("Close Playlist", ClosePlaylist)
            };
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
