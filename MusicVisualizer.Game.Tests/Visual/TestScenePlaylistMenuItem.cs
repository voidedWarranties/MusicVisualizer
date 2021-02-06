using System.Threading.Tasks;
using MusicVisualizer.Game.UI;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using YoutubeExplode;

namespace MusicVisualizer.Game.Tests.Visual
{
    public class TestScenePlaylistMenuItem : TestScene
    {
        public TestScenePlaylistMenuItem()
        {
            Task.Run(async () =>
            {
                var video = await new YoutubeClient().Videos.GetAsync("1I2p5nknSYM");
                Schedule(() =>
                {
                    Add(new PlaylistMenuItem(video)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });
                });
            });
        }
    }
}
