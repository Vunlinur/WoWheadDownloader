using System.Text.Json.Serialization;

namespace WoWheadDownloader {
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

    internal class SoundFile {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
