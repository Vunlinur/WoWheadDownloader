using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WoWheadDownloader {
	internal class MP3Processor {
		// Combine downloaded files into single MP3 per set
		public static void CombineMP3sBySound(IEnumerable<Sound> sounds) {
			foreach (Sound sound in sounds) {
				using var outputStream = File.Create(sound.Name);

				foreach (SoundFile file in sound.Files) {
					Mp3FileReader reader = new (file.LocalFile);
					if ((outputStream.Position == 0) && (reader.Id3v2Tag != null)) {
						outputStream.Write(reader.Id3v2Tag.RawData,
								 0,
								 reader.Id3v2Tag.RawData.Length);
					}
					Mp3Frame frame;
					while ((frame = reader.ReadNextFrame()) != null) {
						outputStream.Write(frame.RawData, 0, frame.RawData.Length);
					}
				}
			}
		}
	}
}
