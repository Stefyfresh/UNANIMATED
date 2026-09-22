# UNANIMATED

**NOTE: THIS IS CURRENTLY AN ALPHA WORK IN PROGRESS- if you would like to try it, please DM me on Discord! (my username is Stefyfresh)**

A custom animation system mod for the hit rhythm game UNBEATABLE! 

This mod makes charting for Arcade Mode much more visually interesting than the base game.

## Features

This list of features is currently implemented in the mod!

- Full control over the camera, its position, rotation, FOV, easing, etc.
- Background video support in any stage
- Swappable stages
- Swappable characters

## Planned Features

These features are planned, but I have no guarantee that I will implement all of them!

- Custom colours in gameplay
- Coloured background and foreground overlays
- SV/scroll speed control
- More..?

## Configuration

The format of the mod is still being finalized, check back later!

---

## Mod Installation Instructions

- Download the latest release of the mod from the releases page, and extract the DLL file from inside the zip
- Download BepInEx from [here](https://github.com/BepInEx/BepInEx/releases) and extract the BepInEx folder from the zip into the main UNBEATABLE game code folder (the one that contains UNBEATABLE.exe). You must extract ALL the files from that zip into the main UNBEATABLE folder (do NOT make a new folder!)
- Run the game once and close it
- Put the mod DLL into the BepInEx\plugins folder

The structure should then be:

<pre>
UNBEATABLE
├─── UNBEATABLE.exe
├─── UNBEATABLE_Data
├─── {some other folders and files}
├─── .doorstop_version
├─── changelog.txt
├─── doorstop_config.ini
├─── winhttp.dll
└─── BepInEx
    ├─── cache
    ├─── config
    ├─── core
    ├─── patchers
    └─── plugins
        ├─── SomeMod.dll
        └─── SomeOtherMod.dll
</pre>

Once the mod is in the folder, restart the game and it should load.
