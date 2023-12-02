# Timberborn-MirrorBuildings
Add the ability to flip a building when placing it by pressing F. 

# Usage
Download the mod in your preferred way. Then the mod is good to go. Press F when constructing a building to flip it. The key to flip the bilding can be changed in the keybind options in the game.

# Known issues
This mod might cause fps issues while placing down a building. Especially is the building allows for multiple to be placed at a time. 
This is hopefully addresses in the future.

# Installing
Recommended way to install this mod is through [Thunderstore](https://timberborn.thunderstore.io/). You can install this plugin manually by cloning the repo, building it
and adding the dll to your bepinex plugins folder. This plugin is dependent on the magnificent [TimberAPI](https://github.com/Timberborn-Modding-Central/TimberAPI).

# Changelog

## v4.0.0 - 2.12.2023
- Changed flipping logic from flipping the mesh into changing scale to -1
- Necessary changes to work with game v.0.5.*
- Should work with modded buildings

## v3.2.1 - 27.5.2023
- Fixed so the mod doesn't crash with modded buildings

## v3.2.0 - 27.5.2023
- Updated to work with Update 4!
- Fixed mirrored buildings not having a powerconnection on where the entrance used to be

## v3.1.0 - 16.12.2022
- Updated TimberAPI to 0.5.3
- Minor changed caused by game version 0.3.4.3

## v3.0.1 - 14.10.2022
- Improved performance when placing down a building

## v3.0.0 - 23.9.2022
- Updated to work with TimberAPI v0.5
- Reset flipstate to false when exiting any tool

## v2.0.0 - 9.9.2022
- Fixed normals so flipped buildings shouldn't look off anymore
- Minor change to output package folder
- Hopefully fixed Papermills and Deep Water Pumps animators

## v1.1.1 - 14.5.2022
- Bugfixes:
	- Some buidings animations looked silly, like waterpump, papermill and large water tank
	- Large Water Tanks entrance was blocked when flipped
	- Now flip the BlockSpecifications too, so occupations and textures match

## v1.0.0 - 13.5.2022
- Initial release
- Added ability to flip buildings when constructing by pressing F