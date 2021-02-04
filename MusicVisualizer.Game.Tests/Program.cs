using osu.Framework;
using osu.Framework.Platform;

namespace MusicVisualizer.Game.Tests
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableHost("MusicVisualizer"))
            using (var game = new MusicVisualizerTestBrowser())
                host.Run(game);
        }
    }
}
