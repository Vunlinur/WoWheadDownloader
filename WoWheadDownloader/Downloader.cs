using HtmlAgilityPack;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WoWheadDownloader {
    internal class Downloader {
        public static async Task<Sound[]> DownloadSoundPage(string targetFolder, string soundPage) {
            Directory.CreateDirectory(targetFolder);

            var doc = GetPage(soundPage);
            if (!GetSoundJson(doc, out string jsonOutput)) {
                Console.WriteLine($"Failed to get sound JSON: {jsonOutput}");
                return [];
            }

            Sound[] sounds = ParseSoundJson(jsonOutput);
            if (sounds.Length == 0) {
                Console.WriteLine("No sounds found to download.");
                return [];
            }

            HttpClient client = new();
            foreach (var sound in sounds) {
                foreach (var file in sound.Files) {
                    string fileName = Path.GetFileName(new Uri(file.Url).LocalPath);
                    string filePath = Path.Combine(targetFolder, fileName);

                    Console.WriteLine($"Downloading: {fileName}");
                    await DownloadFileAsync(client, file.Url, filePath);
                    Console.WriteLine($"Saved: {filePath}");
                }
            }
            return sounds;
        }

        internal static HtmlDocument GetPage(string url) {
            var web = new HtmlWeb();
            return web.Load(url);
        }

        internal static bool GetSoundJson(HtmlDocument? doc, out string output) {
            // Find script tag
            var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), 'WH.Gatherer.addData')]");

            if (scriptNode == null) {
                output = "No relevant script tag found.";
                return false;
            }

            // Extract JSON using Regex
            string scriptText = scriptNode.InnerText;
            var match = Regex.Match(scriptText, @"WH\.Gatherer\.addData\(19,\s*\d+,\s*(\{.*\})\);");

            if (!match.Success) {
                output = "No JSON data found.";
                return false;
            }

            output = match.Groups[1].Value;
            output = output.Replace("\\/", "/"); // Fix escaped slashes
            return true;
        }

        internal static Sound[] ParseSoundJson(string jsonOutput) {
            var soundsDict = JsonSerializer.Deserialize<Dictionary<string, Sound>>(jsonOutput);
            if (soundsDict is null)
                throw new InvalidDataException("Failed to deserialize JSON.");

            foreach (var kvp in soundsDict)
                kvp.Value.Id = int.Parse(kvp.Key);

            return soundsDict.Values.ToArray();
        }

        internal static async Task DownloadFileAsync(HttpClient client, string url, string filePath) {
            try {
                byte[] fileBytes = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(filePath, fileBytes);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error downloading {url}: {ex.Message}");
            }
        }
    }
}
