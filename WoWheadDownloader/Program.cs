using System.Linq;


namespace WoWheadDownloader {
    internal class Program {
        static void Main(string[] args) {
            string targetFolder = "DownloadedMP3s";
            string soundPage = "https://www.wowhead.com/sound=147772/mus-80-warfrontsbattle-f#comments";

            Downloader.DownloadSoundPage(targetFolder, soundPage);
        }
    }
}
