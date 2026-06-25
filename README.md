<table>
  <tr>
    <td><img src="https://raw.githubusercontent.com/hazre/BepInExPack-GoblinCleanup/refs/heads/main/icon.png" width="96" alt="BepInExPack Goblin Cleanup icon"></td>
    <td>
      <h1>BepInExPack Goblin Cleanup</h1>
      <p>BepInEx modloader, preconfigured and ready to use for <a href="https://store.steampowered.com/app/2748340/Goblin_Cleanup/">Goblin Cleanup</a></p>
    </td>
  </tr>
</table>

[![Thunderstore Badge](https://modding.resonite.net/assets/available-on-thunderstore.svg)](https://thunderstore.io/c/goblin-cleanup/)

This is [BepInEx 5.4.23.5](https://github.com/BepInEx/BepInEx) pack for [Goblin Cleanup](https://store.steampowered.com/app/2748340/Goblin_Cleanup/).

The exact version is `BepInEx_win_x64_5.4.23.5.zip` from https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5

BepInEx is a general purpose framework for Unity modding.
BepInEx includes tools and libraries to

* load custom code (hereafter *plugins*) into the game on launch;
* patch in-game methods, classes and even entire assemblies without touching original game files;
* configure plugins and log game to desired outputs like console or file;
* manage plugin dependencies.

BepInEx is currently [one of the most popular modding tools for Unity on GitHub](https://github.com/topics/modding?o=desc&s=stars).

## This pack's contents

* `BepInExPack\UnstrippedCorlib\`'s corlib files sourced from Unity Editor 2023.2.22f1: `UnityEngine*.dll` from the non-dev player Managed directory, system assemblies from `MonoBleedingEdge\unityjit-win32`
* `BepInExPack\doorstop_config.ini`'s `dllSearchPathOverride` set to `UnstrippedCorlib`
* `BepInExPack\BepInEx\config\BepInEx.cfg`'s `HideManagerGameObject` set to true

## Installation (Manual)
1. Install the latest release via [Thunderstore](https://thunderstore.io/c/goblin-cleanup/) or download the ZIP from the [Releases](https://github.com/hazre/BepInExPack-GoblinCleanup/releases) page.
2. Extract the ZIP and copy the contents of `BepInExPack` directory to your Goblin Cleanup installation folder:
   - **Default location:** `C:\Program Files (x86)\Steam\steamapps\common\Goblin Cleanup\`
3. Start the game. BepInEx will load automatically. Mods can be added to `BepInEx/plugins/`.

## Development
 
Install [mise](https://mise.jdx.dev/getting-started.html) and run `mise install` to set up tools.
 
```bash
mise run prepare-unstripped   # Generate UnstrippedCorlib from Unity build support
mise run build                # Build the Thunderstore package
```
 
Run `mise tasks` to list all available tasks.

## License

This project is licensed under MIT License. See [LICENSE](LICENSE) for details.