# ONI Mod Launcher
### A standalone mod manager and launcher for Oxygen Not Included.

<img src="doc/Screenshot.png"/>

## System Requirements
- 64-bit Windows
- .NET 8.0

## Features
- Organize your mod list outside of the game with the ability to save and load backups of your mod list.
- Easy access to the game log and mod folders.
- Toggle between Vanilla and DLC outside of the game.
- Enable Debug Mode with a simple checkbox.
- Enable a hidden testing feature that skips the main menu and immediately loads the last save file.  
**Note:** This setting is saved in a file beside the game EXE. It will still have an effect even if launching from Steam.
- Mark mods as "keep" to ensure they stay enabled even when using "Disable All" button.
- Mark mods as "broken" to ensure they stay disabled even when using "Enable All" button.
- Open and edit JSON config files for mods.
- "Smart" mod sorting:
    - Mods marked "keep" go to the top.
    - Mods marked "broken" go to the bottom.
    - Mods in dev or local folders go to the top below keep.
    - Mods from steam sorted by workshop ID (and therefore by release date too)

