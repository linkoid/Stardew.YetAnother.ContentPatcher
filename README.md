﻿# Yet Another Content Patcher

Yet Another Content Patcher is a framework that allows creating [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) content packs using human-friendly [YAML](https://yaml.org/) instead of JSON.

### Installation
1. Install the [latest version of SMAPI](https://smapi.io/).
2. Install the [latest version of Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915).
3. Install this mod from [Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/0).
4. Unzip any Yet Another Content Patcher content packs into `Mods` to install them.
5. Run the game using SMAPI.


### For Mod Authors
YACP works exactly the same as Content Patcher. The only things that change
are the use of **content.yaml** files instead of **content.json**, and the YAML syntax.

Example mod file structure:
```
📁 Mods/
    📁 [YACP] YourModName/
        🗎 content.yaml
        ﻿﻿🗎 manifest.json
        📁 assets/
            🗎 example.png
```

Example content.yaml file:
```yaml
Format: 2.3.0
Changes:
- Action: Load
    Target: Portraits/Abigail
    FromFile: "assets/example.png"
```

For details on using Content Patcher, refer to the [Content Patcher Author Guide](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide.md).

For details on using YAML with Content Patcher, refer to the example [content.yaml](https://github.dev/linkoid/Stardew.YetAnother.ContentPatcher/blob/main/YetAnother.ContentPatcher.Tests.Content/content.yaml) file.


### Links
[Source Code](https://github.com/linkoid/Stardew.YetAnother.ContentPatcher)
