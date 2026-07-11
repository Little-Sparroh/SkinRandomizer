# Skin Randomizer

A BepInEx client-side mod for Mycopunk that randomly equips your favorited skins when a mission starts.

## Features

- On drop pod countdown / mission start, randomly equips favorited skins
- Applies to your **character**, **drop pod**, and **both equipped weapons**
- Favorites are split into independent pools so you still get one of each type when available:
  - Main skins
  - Gun crabs
  - Constellations (VFX crabs)
- Toggle on/off via config

## Getting Started

### Dependencies

* Mycopunk (base game)
* [BepInEx](https://github.com/BepInEx/BepInEx) - Version 5.4.2403 or compatible (BepInExPack_Mycopunk)

### Installing

**Via Thunderstore (Recommended)**:
1. Download and install via Thunderstore Mod Manager / r2modman
2. The mod will be installed automatically

**Manual Installation**:
1. Place `SkinRandomizer.dll` in your `<Mycopunk Directory>/BepInEx/plugins/` folder

### Usage

1. Favorite the skins you want in the random pool (character, drop pod, and/or weapons)
2. Start a mission
3. When the drop pod countdown begins, a random favorite is equipped from each available pool for each gear piece

## Configuration

Config file: `<Mycopunk Directory>/BepInEx/config/sparroh.skinrandomizer.cfg`

| Setting | Default | Description |
|---------|---------|-------------|
| `Enabled` | `true` | If true, randomly equips favorite skins on mission start |

Config changes are hot-reloaded while the game is running — edit and save the `.cfg` file, and the next mission start will use the new values (no restart needed).


## Help

* **Nothing changing?** Make sure you have skins favorited for the character, drop pod, and/or equipped weapons
* **Weapon skins not changing?** Favorite skins on the specific weapons you have equipped (slots 1 and 2)
* **Mod not loading?** Verify BepInEx is installed and check the BepInEx console/log
* **Want the old look back?** Disable the mod in config, or re-equip skins manually

## Authors

- Sparroh

## License

This project is licensed under the MIT License - see the LICENSE file for details
