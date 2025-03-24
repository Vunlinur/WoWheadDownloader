using HtmlAgilityPack;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;


namespace WoWheadDownloader {
    internal class Program {
        static void Main(string[] args) {
            string targetFolder = "DownloadedMP3s";
            Directory.CreateDirectory(targetFolder);

            var doc = GetPage("https://www.wowhead.com/sound=22271/mus-southbarrens-gn");
            if (!GetSoundJson(doc, out string jsonOutput)) {
                Console.WriteLine($"Failed to get sound JSON: {jsonOutput}");
                return;
            }

            var aaa = doc.DocumentNode.OuterHtml;
            Sound[] sounds = ParseSoundJson(jsonOutput);
            if (sounds.Length == 0) {
                Console.WriteLine("No sounds found to download.");
                return;
            }

            using HttpClient client = new();
            foreach (var sound in sounds) {
                foreach (var file in sound.Files) {
                    string fileName = Path.GetFileName(new Uri(file.Url).LocalPath);
                    string filePath = Path.Combine(targetFolder, fileName);

                    Console.WriteLine($"Downloading: {fileName}");
                    DownloadFileAsync(client, file.Url, filePath).Wait();
                    Console.WriteLine($"Saved: {filePath}");
                }
            }
        }

        static HtmlDocument? GetPage(string url) {
            var web = new HtmlWeb();
            try {
                return web.Load(url);
            }
            catch (AggregateException aggregateException) {
                if (!aggregateException.Message.Contains("(404) Not Found"))
                    throw;
            }
            return null;
        }

        private static bool GetSoundJson(HtmlDocument? doc, out string output) {

            // Find script tag
            var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), 'WH.Gatherer.addData')]");

            if (scriptNode == null) {
                output = "No relevant script tag found.";
                return false;
            }

            // Extract JSON using Regex
            string scriptText = scriptNode.InnerText;
            var match = Regex.Match(scriptText, @"WH\.Gatherer\.addData\(\d+,\s*\d+,\s*(\{.*\})\);");

            if (!match.Success) {
                output = "No JSON data found.";
                return false;
            }

            output = match.Groups[1].Value;
            output = output.Replace("\\/", "/"); // Fix escaped slashes
            return true;
        }

        private static Sound[] ParseSoundJson(string jsonOutput) {
            var soundsDict = JsonSerializer.Deserialize<Dictionary<string, Sound>>(jsonOutput);
            if (soundsDict is null)
                throw new InvalidDataException("Failed to deserialize JSON.");

            foreach (var kvp in soundsDict)
                kvp.Value.Id = int.Parse(kvp.Key);

            return soundsDict.Values.ToArray();
        }

        static async Task DownloadFileAsync(HttpClient client, string url, string filePath) {
            try {
                byte[] fileBytes = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(filePath, fileBytes);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error downloading {url}: {ex.Message}");
            }
        }

        public class Sound {
            [JsonIgnore] // Ignore this when serializing/deserializing as the key is dynamic
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("type")]
            public int Type { get; set; }

            [JsonPropertyName("files")]
            public List<SoundFile> Files { get; set; }
        }

        public class SoundFile {
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
}
