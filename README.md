# TTG Tools

A utility for modifying files from Telltale Games, including texts (.landb, .langdb, .dlog, .prop), textures (.d3dtx) for PC, Xbox 360, PS4, PS3, PS Vita, Nintendo Switch, Nintendo Wii, iOS and Android, fonts (.font), as well as extracting and creating archive files (.ttarch, .ttarch2, .obb). Supports decryption of .lua and .lenc files.

Based on [TTG Tools by Den Em and Pashok6798](https://github.com/zenderovpaulo95/TTG-Tools), with additional modifications.

## Recent Changes

### Archive Unpacker Improvements (2026-04)
- **Ttarch2 Scanner**: New tool for scanning ttarch2 archives — browse file listings without full extraction
- **Oodle Kraken Support**: Added detection and decompression for Oodle Kraken (0x8C 0x06) algorithm, previously only LZHLW was supported
- **Last Chunk Padding Detection**: Combined approach — try full chunkSize first (padded archives), fall back to file offset calculation (unpadded archives). Displays padding status in Archive Info panel
- **Archive Info Panel**: Now shows compression algorithm name (Deflate / Oodle LZHLW / Oodle Kraken), TTArch2 version (3ATT/4ATT), and last chunk padding status
- **Cross-Thread Fix**: Fixed InvalidOperationException when extracting files with search filter or format filter enabled
- **Lua Encryption Detection**: Improved — only marks .lenc files as encrypted by default, checks .lua files by reading actual content after decompression

### Font Editor Enhancements (2026)
- **UI Optimization**: Improved user interface with better layout and usability
- **Font Detection**: Added automatic font detection capability for easier file handling
- **Missing Character Generation**: Implemented automatic generation of missing characters for comprehensive font support
- **Texture Management**: Enhanced DDS texture file handling with automatic copying during save operations
- **Multi-page Support**: Improved support for fonts with multiple texture pages
- **Default Font Fix**: Set default font to Tahoma to prevent layout issues on non-English systems

### Archive Packer Improvements (2026)
- **Padding Control**: Added option to control last chunk padding (compatible mode pads to full chunk size)
- **Oodle Compression**: Corrected function signature (10 parameters + StdCall) for OodleLZ_Compress
- **Header Accuracy**: Fixed zCTT header field ordering and chunksFirstOffset calculation for Oodle compressed archives

## Screenshots

### Archive Unpacker
![Archive Unpacker](images/Archive_Unpacker.png)

### Archive Packer
![Archive Packer](images/Archive_Packer.png)

### Font Editor
![Font Editor](images/FontEditor-Envolved.png)
![Font Editor Settings](images/FontEditorSettings.png)

## Features

TTG Tools makes it easier to translate and modify Telltale Games and Skunkape Games. It currently supports:

- Telltale Texas Hold'em
- Bone: Out from Boneville / The Great Cow Race
- Sam & Max: Save the World / Beyond Time and Space / The Devil's Playhouse
- Sam & Max: Save the World - Remastered / Beyond Time and Space - Remastered / The Devil's Playhouse - Remastered
- Strong Bad's Cool Game for Attractive People
- Wallace & Gromit's Grand Adventures
- Tales of Monkey Island
- Hector: Badge of Carnage
- Puzzle Agent 1 & 2
- Poker Night at the Inventory / Poker Night 2
- Poker Night at the Inventory - Remastered
- Back to the Future: The Game
- Jurassic Park: The Game
- Law & Order: Legacies
- The Walking Dead: Season One / Season Two / A New Frontier / The Final Season / The Telltale Definitive Series / Michonne
- The Wolf Among Us
- Tales from the Borderlands (2015 & 2021)
- Game of Thrones: The Telltale Series
- Minecraft: Story Mode - Season One / Season Two
- Batman: The Telltale Series / The Enemy Within
- Marvel's Guardians of the Galaxy: The Telltale Series

## Special Thanks

- Den Em and Pashok6798 for the original TTG Tools
- Aluigi for the source code of `ttarchext`
- Taylor Hornby for the C# source code of Blowfish encryption
- Gdkchan, Stella/AboodXD for the Nintendo Switch swizzle method
- Daemon1 and tge for the PS4 swizzle algorithm
- Josh Tamely for the Oodle wrapper
- Hajin Jang for the Zlib wrapper
- [Nemiroff](https://github.com/Nemiroff/TTG-Tools) for fixing a bug in the Font Editor
- Krisp for adding Xbox and Wii textures support and font editing with Swizzle
- [Benny](https://quickandeasysoftware.net) for sending the encryption key for Poker Night at the Inventory - Remastered
