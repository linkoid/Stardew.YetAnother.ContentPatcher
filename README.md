# YAML Adapter for Content Patcher

[![YAML Adapter for Content Patcher | YAML](https://custom-icon-badges.demolab.com/badge/YAML_Adapter_for_Content_Patcher-YAML-CB171E.svg?logo=smapi)](https://github.com/linkoid/Stardew.YetAnother.ContentPatcher)
[![GitHub](https://img.shields.io/badge/GitHub-%23121011.svg?logo=github&logoColor=white)](https://github.com/linkoid/Stardew.YetAnother.ContentPatcher)
[![License](https://img.shields.io/github/license/linkoid/Stardew.YetAnother.ContentPatcher)](https://github.com/linkoid/Stardew.YetAnother.ContentPatcher/tree/main?tab=MIT-1-ov-file)

[![Stardew Valley | Modding](https://custom-icon-badges.demolab.com/badge/Stardew_Valley-Modding-6B3407.svg?logo=stardewvalley&labelColor=FFD58E)](https://stardewvalleywiki.com/Modding)
[![SMAPI | C#](https://custom-icon-badges.demolab.com/badge/SMAPI-C%23-%23239120.svg?logo=smapi)](https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started)
[![Content Patcher | JSON]( https://custom-icon-badges.demolab.com/badge/Content_Patcher-JSON-000.svg?logo=smapi)](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher)



YAML Adapter for Content Patcher is a framework that allows creating [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) content packs using human-friendly [YAML](https://yaml.org/) instead of JSON.

### Installation
1. Install the [latest version of SMAPI](https://smapi.io/).
2. Install the [latest version of Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915).
3. Install this mod from [Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/0).
4. Unzip any YACP content packs into `Mods` to install them.
5. Run the game using SMAPI.


## For Mod Authors
YACP works exactly the same as Content Patcher. The only things that change
are the use of `content.yaml` files instead of `content.json`, and the YAML syntax.

For details on using Content Patcher, refer to the [Content Patcher Author Guide](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide.md).

For details on using YAML with Content Patcher, refer to the example [content.yaml](https://github.dev/linkoid/Stardew.YetAnother.ContentPatcher/blob/main/YetAnother.ContentPatcher.Tests.Content/content.yaml) file.

### Migrating from JSON
An existing `content.json` file can quickly be converted to a `content.yaml` file
using [YACP's online JSON converter](https://linkoid.github.io/Stardew.YetAnother.ContentPatcher/converter).

### Examples
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


## Links
* [Source Code](https://github.com/linkoid/Stardew.YetAnother.ContentPatcher)
* [JSON to YAML Converter](https://linkoid.github.io/Stardew.YetAnother.ContentPatcher/converter)
