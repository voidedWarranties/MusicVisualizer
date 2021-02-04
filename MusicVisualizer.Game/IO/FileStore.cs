using System.Collections.Generic;
using System.IO;
using System.Linq;
using IniParser;
using IniParser.Model;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace MusicVisualizer.Game.IO
{
    public class FileStore
    {
        public readonly Storage Storage;

        public readonly IResourceStore<byte[]> Store;

        public FileStore(Storage storage)
        {
            Storage = storage.GetStorageForDirectory(@"songs");
            Store = new StorageBackedResourceStore(Storage);
        }

        public IniData GetIni(string path)
        {
            var parser = new StreamIniDataParser();
            return parser.ReadData(new StreamReader(Storage.GetStream(path)));
        }

        public IEnumerable<SongConfig> GetInis()
        {
            var inis = new List<SongConfig>();
            var directories = Storage.GetDirectories(".");

            foreach (var directory in directories)
            {
                var files = Storage.GetFiles(directory);
                inis.AddRange(files.Where(f => f.EndsWith(".ini")).Select(f => new SongConfig(directory, GetIni(f))));
            }

            return inis;
        }
    }
}
