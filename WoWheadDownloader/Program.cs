using HtmlAgilityPack;
using Kurukuru;
using NAudio.Wave;
using System.Linq;
using System.Text.RegularExpressions;


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
                int fileCount = sound.Files.Count;

                await Spinner.StartAsync($"  Downloading: {sound.Name} ({fileCount} file/s)", async spinner => {
                    int errorCount = await DownloadSoundFiles(sound, client, targetFolder);

                    if (errorCount == 0)
                        spinner.Succeed($"Completed: {sound.Name} ({fileCount} file/s)");
                    else if (errorCount == fileCount)
                        spinner.Fail($"Failed: {sound.Name} (0/{fileCount} file/s)");
                    else
                        spinner.Warn($"Partially completed: {sound.Name} ({fileCount - errorCount}/{fileCount} file/s)");
                });
            }

            //// Combine downloaded files into single MP3 per set
            //foreach (SoundSet set in sSets) {
			//	using var outputStream = File.Create(set.Name);
            //    foreach (SoundFile file in set.Files) {
            //        Mp3FileReader reader = new Mp3FileReader(file.MetaData.LocalFile);
            //        if ((outputStream.Position == 0) && (reader.Id3v2Tag != null)) {
            //            outputStream.Write(reader.Id3v2Tag.RawData,
            //                     0,
            //                     reader.Id3v2Tag.RawData.Length);
            //        }
            //        Mp3Frame frame;
            //        while ((frame = reader.ReadNextFrame()) != null) {
            //            outputStream.Write(frame.RawData, 0, frame.RawData.Length);
            //        }
            //    }
            //}
        }

        private static async Task<int> DownloadSoundFiles(Sound sound, HttpClient client, string targetFolder) {
            int errorCount = 0;
            foreach (var file in sound.Files) {
				string fileName = file.FileName;
				string setDir = Path.Combine(targetFolder, sound.Name);
				Directory.CreateDirectory(setDir);
                file.MetaData.LocalFile = Path.Combine(targetFolder, setDir, fileName);

                await Spinner.StartAsync($"  Downloading: {fileName}...", async spinner => {
                    try {
                        await Downloader.DownloadFileAsync(client, file.Url, file.MetaData.LocalFile);
                        file.MetaData.Downloaded = true;
                        spinner.Succeed($"  Saved: {file.MetaData.LocalFile}");
                    }
                    catch (Exception ex) {
                        errorCount++;
                        spinner.Fail($"  Failed: {fileName} ({ex.Message})");
                    }
                });
            }
            return errorCount;
        }
    }
}
