using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Framework.Testing;

namespace MusicVisualizer.Game.IO
{
    [ExcludeFromDynamicCompile]
    public class VisConfigManager : IniConfigManager<VisSetting>
    {
        protected override void InitialiseDefaults()
        {
            Set(VisSetting.Playlist, "PLwBnYkSZTLgIGr1_6l5pesUY0TZZFIy_b");
        }

        public VisConfigManager(Storage storage)
            : base(storage)
        {
        }
    }

    public enum VisSetting
    {
        Playlist
    }
}
