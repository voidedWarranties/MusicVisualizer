﻿using System;
using System.Collections.Generic;
using MusicVisualizer.Game.IO;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;

namespace MusicVisualizer.Game
{
    public class SongMenu : Menu
    {
        public Action<SongConfig> PlayFile;

        private List<MenuItem> songSubmenuItems;

        public SongMenu(Direction direction, bool topLevelMenu = true)
            : base(direction, topLevelMenu)
        {
            BackgroundColour = FrameworkColour.Blue;
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, FileStore store)
        {
            Items = new MenuItem[]
            {
                new MenuItem("Open Folder", () => host.OpenFileExternally(store.Storage.GetFullPath("."))),
                new MenuItem("Songs")
                {
                    Items = songSubmenuItems = new List<MenuItem>()
                }
            };
        }

        public void UpdateItems(IEnumerable<SongConfig> files)
        {
            songSubmenuItems.Clear();
            files.ForEach(file =>
            {
                songSubmenuItems.Add(new MenuItem(file.Name, () => PlayFile?.Invoke(file)));
            });
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