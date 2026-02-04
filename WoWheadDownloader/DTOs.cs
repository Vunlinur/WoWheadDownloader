using System.Text.Json.Serialization;

namespace WoWheadDownloader {
    /**
     * Represents a script json from the Search page
     */
	internal class Search {
		[JsonPropertyName("sort")]
		public string[] Sort { get; set; }

		[JsonPropertyName("maxPopularity")]
		public int MaxPopularity { get; set; }

		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("template")]
		public string Template { get; set; }

		[JsonPropertyName("data")]
		public Sound[] Sounds { get; set; }

        // ExtraCols gets removed because it's unquoted and bug-prone
	}

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

		[JsonPropertyName("popularity")]
		public int Popularity { get; set; }
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
