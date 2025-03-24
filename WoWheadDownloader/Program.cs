using HtmlAgilityPack;
using Kurukuru;
using System.Linq;


namespace WoWheadDownloader {
    internal class Program {
        static async Task Main(string[] args) {
            string targetFolder = "DownloadedMP3s";
            string soundPage = "https://www.wowhead.com/sound=22271/mus-southbarrens-gn";

            Directory.CreateDirectory(targetFolder);


            // Step 1: Get the Page
            HtmlDocument doc = null;
            bool pageSuccess = await Spinner.StartAsync("Fetching sound page...", async spinner =>
            {
                doc = Downloader.GetPage(soundPage);
                if (doc == null) {
                    spinner.Fail("Failed to fetch sound page.");
                    return false;
                }
                spinner.Succeed("Page fetched.");
                return true;
            });

            if (!pageSuccess) return;

            // Step 2: Extract JSON
            string jsonOutput = "";
            bool jsonSuccess = await Spinner.StartAsync("Extracting sound information...", async spinner => {
                if (!Downloader.GetSoundJson(doc, out jsonOutput)) {
                    spinner.Fail("Failed to extract sound information.");
                    return false;
                }
                spinner.Succeed("Sound information extracted.");
                return true;
            });

            if (!jsonSuccess) return;

            // Step 3: Parse Sounds
            Sound[] sounds = [];
            bool parseSuccess = await Spinner.StartAsync("Parsing sound data...", async spinner => {
                sounds = Downloader.ParseSoundJson(jsonOutput);
                if (sounds.Length == 0) {
                    spinner.Fail("No sounds found.");
                    return false;
                }
                spinner.Succeed($"Found {sounds.SelectMany(s => s.Files).Count()} sounds.");
                return true;
            });
            if (!parseSuccess) return;

            // Step 4: Download Files
            using HttpClient client = new();
            foreach (var sound in sounds) {
                foreach (var file in sound.Files) {
                    string fileName = Path.GetFileName(new Uri(file.Url).LocalPath);
                    string filePath = Path.Combine(targetFolder, fileName);

                    await Spinner.StartAsync($"  Downloading: {fileName}...", async spinner =>
                    {
                        try {
                            await Downloader.DownloadFileAsync(client, file.Url, filePath);
                            spinner.Succeed($"  Saved: {filePath}");
                        }
                        catch (Exception ex) {
                            spinner.Fail($"  Failed: {fileName} ({ex.Message})");
                        }
                    });
                }
            }
        }
    }
}
