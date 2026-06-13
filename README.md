# BepInExPack Goblin Cleanup
[![Thunderstore Badge](https://modding.resonite.net/assets/available-on-thunderstore.svg)](https://thunderstore.io/c/goblincleanup/)

BepInEx 5.4.23.5, preconfigured and ready to use for [Goblin Cleanup](https://store.steampowered.com/app/2748340/Goblin_Cleanup/).

Includes unstripped corlib files to replace the game's stripped Mono runtime assemblies, and patches `doorstop_config.ini` to load from `UnstrippedCorlib` before the game's `Managed` directory.

## Installation (Manual)
1. Install the latest release via [Thunderstore](https://thunderstore.io/c/goblincleanup/) or download the ZIP from the [Releases](https://github.com/hazre/BepInExPack-GoblinCleanup/releases) page.
2. Extract the ZIP and copy the `BepInExPack` directory to your Goblin Cleanup installation folder:
   - **Default location:** `C:\Program Files (x86)\Steam\steamapps\common\Goblin Cleanup\`
3. Start the game. BepInEx will load automatically. Mods can be added to `BepInEx/plugins/`.
