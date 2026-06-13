<table>
  <tr>
    <td><img src="icon.png" width="96" alt="More Players icon"></td>
    <td>
      <h1>BepInExPack Goblin Cleanup</h1>
      <p>BepInEx 5.4.23.5, preconfigured and ready to use for <a href="https://store.steampowered.com/app/2750010">Goblin Cleanup</a></p>
    </td>
  </tr>
</table>

[![Thunderstore Badge](https://modding.resonite.net/assets/available-on-thunderstore.svg)](https://thunderstore.io/c/goblincleanup/)

Includes unstripped corlib files to replace the game's stripped Mono runtime assemblies, and patches `doorstop_config.ini` to load from `UnstrippedCorlib` before the game's `Managed` directory.

## Installation (Manual)
1. Install the latest release via [Thunderstore](https://thunderstore.io/c/goblincleanup/) or download the ZIP from the [Releases](https://github.com/hazre/BepInExPack-GoblinCleanup/releases) page.
2. Extract the ZIP and copy the `BepInExPack` directory to your Goblin Cleanup installation folder:
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