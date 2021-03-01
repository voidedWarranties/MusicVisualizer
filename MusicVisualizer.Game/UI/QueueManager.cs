using System;
using System.Collections.Generic;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using YoutubeExplode.Playlists;

namespace MusicVisualizer.Game.UI
{
    public class QueueManager : Component
    {
        public readonly Bindable<Track> Track = new Bindable<Track>();
        public readonly Bindable<string> Video = new Bindable<string>();

        private readonly List<PlaylistVideo> playlist = new List<PlaylistVideo>();

        public Action<string> PlayYoutube;

        private bool shouldBePlaying;
        private int playingIndex;

        public void SetPlaylist(IEnumerable<PlaylistVideo> videos)
        {
            playlist.Clear();
            playlist.AddRange(videos);
        }

        protected override void Update()
        {
            base.Update();

            if (Track.Value == null) return;

            if (Track.Value.IsRunning && !shouldBePlaying)
            {
                shouldBePlaying = true;
                playingIndex = playlist.FindIndex(i => i.Id == Video.Value);
            }
            else if (Track.Value.HasCompleted && shouldBePlaying)
            {
                playingIndex = (playingIndex + 1) % playlist.Count;
                PlayYoutube?.Invoke(playlist[playingIndex].Id);
            }
        }
    }
}
