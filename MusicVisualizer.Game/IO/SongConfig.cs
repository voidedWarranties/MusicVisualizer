using IniParser.Model;

namespace MusicVisualizer.Game.IO
{
    public class SongConfig
    {
        private readonly IniData data;

        private readonly string directory;

        public SongConfig(string directory, IniData data)
        {
            this.data = data;
            this.directory = directory;
        }

        public bool HasVideo => data["Files"]["video"] != null;

        public string Video => HasVideo ? $"{directory}/{data["Files"]["video"]}" : null;

        public string Audio => $"{directory}/{data["Files"]["audio"]}";

        public string Name => data["Metadata"]["name"] ?? directory;
    }
}
