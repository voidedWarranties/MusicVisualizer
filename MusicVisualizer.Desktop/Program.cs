using osu.Framework.Platform;
using osu.Framework;
using MusicVisualizer.Game;

namespace MusicVisualizer.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableHost(@"MusicVisualizer"))
            using (osu.Framework.Game game = new MusicVisualizerGame())
                host.Run(game);
        }
    }
}
