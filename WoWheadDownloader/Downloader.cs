using HtmlAgilityPack;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WoWheadDownloader {
    internal partial class Downloader {
        public static async Task<Sound[]> DownloadSoundPage(string targetFolder, string soundPage) {
            Directory.CreateDirectory(targetFolder);

			Sound[] sounds = GetSoundsFromPage(soundPage);

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

		/**
         * Downloads all sounds from the given page.
         * Accepts either a search page like:
         * https://www.wowhead.com/sounds/zone-music/name:boralus
         * Or single sound page like:
         * https://www.wowhead.com/sound=22846/mus-southbarrensgreen
         */
        public static Sound[] GetSoundsFromPage(string url) {
            LinkType type = GetLinkType(url);
            if (type == LinkType.Unknown)
				throw new ArgumentException("Unsupported URL format.");

			var doc = GetPage(url);

            string jsonOutput = type switch {
				LinkType.Search => GetSearchedSoundsJson(doc),
				LinkType.Sound => GetSoundJson(doc),
				_ => ""
			};

			return type switch {
				LinkType.Search => ParseSearchedSoundsJson(jsonOutput).Sounds,
				LinkType.Sound => ParseSoundJson(jsonOutput),
				_ => []
			};
		}

		public static async Task DownloadFileAsync(HttpClient client, string url, string filePath) {
			try {
				byte[] fileBytes = await client.GetByteArrayAsync(url);
				await File.WriteAllBytesAsync(filePath, fileBytes);
			}
			catch (Exception ex) {
				Console.WriteLine($"Error downloading {url}: {ex.Message}");
			}
		}

		private static LinkType GetLinkType(string url) {
			if (url.Contains("/sound="))
				return LinkType.Sound;
			if (url.Contains("/sounds/"))
				return LinkType.Search;
			return LinkType.Unknown;
		}

		private enum LinkType {
            Sound,
            Search,
            Unknown
        }

		private static HtmlDocument GetPage(string url) {
            var web = new HtmlWeb();
            return web.Load(url);
		}

		private static string GetSoundJson(HtmlDocument? doc)
			=> GetJson(doc, "//script[contains(text(), 'WH.Gatherer.addData')]", SoundScriptRegex());

		private static string GetSearchedSoundsJson(HtmlDocument? doc)
			=> GetJson(doc, "//script[contains(text(), 'new Listview')]", SearchedSoundsScriptRegex());

		private static string GetJson(HtmlDocument? doc, string scriptXpath, Regex jsonRegex) {
            // Find script tag
            var scriptNode = doc.DocumentNode.SelectSingleNode(scriptXpath);

            if (scriptNode == null)
				throw new ArgumentException("No relevant script tag found.");

            // Extract JSON using Regex
            string scriptText = scriptNode.InnerText;
            var match = jsonRegex.Match(scriptText);

            if (!match.Success)
				throw new ArgumentException("No JSON data found.");

            var output = match.Groups[1].Value;
            output = output.Replace("\\/", "/"); // Fix escaped slashes
            return output;
		}

		private static Search ParseSearchedSoundsJson(string jsonOutput) {
			jsonOutput = Regex.Replace(jsonOutput,
				@",?\s*""extraCols""\s*:\s*\[[^\]]*\]\s*,?",
		        string.Empty, RegexOptions.Singleline
            );

			var search = JsonSerializer.Deserialize<Search>(jsonOutput);
			if (search is null)
				throw new InvalidDataException("Failed to deserialize JSON.");

			return search;
		}

		internal static Sound[] ParseSoundJson(string jsonOutput) {
            var soundsDict = JsonSerializer.Deserialize<Dictionary<string, Sound>>(jsonOutput);
            if (soundsDict is null)
                throw new InvalidDataException("Failed to deserialize JSON.");

            foreach (var kvp in soundsDict)
                kvp.Value.Id = int.Parse(kvp.Key);

            return soundsDict.Values.ToArray();
        }

		[GeneratedRegex(@"WH\.Gatherer\.addData\(19,\s*\d+,\s*(\{.*\})\);")]
		private static partial Regex SoundScriptRegex();

		[GeneratedRegex(@"new Listview\((\{.*\})\);")]
		private static partial Regex SearchedSoundsScriptRegex();
	}
}
