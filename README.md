# WoWhead Downloader

A command-line tool for downloading sound files from WoWhead.com.
The tool automatically parses WoWhead pages to extract sound data and downloads all associated audio files, organizing them into folders by sound name.
Keeps audio metadata if available at source.

## Features

- 🎵 Download sounds from WoWhead search pages or individual sound pages
- 📁 Automatic folder organization by sound name
- ⏭️ Skip already downloaded files (prevents re-downloading)
- 📊 Progress tracking for each sound set and individual files

## Requirements

- .NET 8.0 SDK or Runtime

## Usage

```bash
WoWheadDownloader [soundPageUrl] [targetFolder]
```

### Arguments

- **soundPageUrl** (required): A WoWhead URL pointing to either:
  - A search page (e.g., `https://www.wowhead.com/sounds/zone-music/name:boralus`)
  - A single sound page (e.g., `https://www.wowhead.com/sound=22846/mus-southbarrensgreen`)

- **targetFolder** (optional): The directory where downloaded files will be saved. Defaults to `DownloadedMP3s` if not specified.

### Examples

Download all sounds from a search page:
```bash
WoWheadDownloader "https://www.wowhead.com/sounds/zone-music/name:kultir"
```

Download to a custom folder:
```bash
WoWheadDownloader "https://www.wowhead.com/sounds/zone-music/name:boralus" "MyMusic"
```

Download from a single sound page:
```bash
WoWheadDownloader "https://www.wowhead.com/sound=22846/mus-southbarrensgreen"
```

## File Organization

Downloaded files are organized as follows:

```
DownloadedMP3s/
├── MUS_80_DGN_SiegeOfBoralus_Ashvane_Intro/
│   └── MUS_80_HouseAshvane_H.mp3
├── MUS_80_DGN_SiegeOfBoralus_Kraken/
│   ├── MUS_80_ShrineofStorms_1_H.mp3
│   └── MUS_80_ShrineofStorms_2_A.mp3
└── ...
```

Each sound set gets its own folder, and all files belonging to that sound set are placed inside.

## Dependencies

- **HtmlAgilityPack** (1.12.0): For parsing HTML and extracting sound data
- **Kurukuru** (1.4.2): For animated CLI spinners
