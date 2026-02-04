using Kurukuru;
using System.Linq;


namespace WoWheadDownloader {
    internal class Program {
        static async Task Main(string[] args) {
            string targetFolder = "DownloadedMP3s";
            string soundPage = "https://www.wowhead.com/sounds/zone-music/name:kultir";
            Directory.CreateDirectory(targetFolder);

			// Step 1: Get the sounds from the page
            Sound[] sounds = [];
            bool parseSuccess = await Spinner.StartAsync("Fetching sound data...", async spinner => {
                try {
					sounds = Downloader.GetSoundsFromPage(soundPage);
				}
                catch (Exception e) {
					spinner.Fail(e.Message);
					return false;
				}
                if (sounds.Length == 0) {
                    spinner.Fail("No sounds found.");
                    return false;
                }
                spinner.Succeed($"Found {sounds.SelectMany(s => s.Files).Count()} sounds.");
                return true;
            });

            if (!parseSuccess) return;

            // Step 2: Download Files
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
        }

        private static async Task<int> DownloadSoundFiles(Sound sound, HttpClient client, string targetFolder) {
            int errorCount = 0;
            foreach (var file in sound.Files) {
				string fileName = file.FileName;
				string setDir = Path.Combine(targetFolder, sound.Name);
				Directory.CreateDirectory(setDir);
                file.LocalFile = Path.Combine(setDir, fileName);

                await Spinner.StartAsync($"  Downloading: {fileName}...", async spinner => {
                    if (File.Exists(file.LocalFile)) {
                        spinner.Warn($"  Skipped (already exists): {file.LocalFile}");
                        return;
                    }
                    try {
                        await Downloader.DownloadFileAsync(client, file.Url, file.LocalFile);
                        spinner.Succeed($"  Saved: {file.LocalFile}");
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
