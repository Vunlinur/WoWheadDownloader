using System.Text.Json.Serialization;

namespace WoWheadDownloader {
	/**
     * Represetns a set of sound files, e.g.:
     * https://www.wowhead.com/sound=22846/mus-southbarrensgreen
     * Contains one or more sound files
     */
	internal class Sound {
        [JsonIgnore] // Ignore this when serializing/deserializing as the key is dynamic
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("files")]
        public List<SoundFile> Files { get; set; }
    }

    /**
     * A single sound file within a sound set
     */
    internal class SoundFile {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        public string FileName => Path.GetFileName(new Uri(Url).LocalPath);

        public FileData MetaData { get; set; } = new();
    }

    internal class FileData {
        public string LocalFile { get; set; }
        public bool Downloaded { get; set; }
    }
}
