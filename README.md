# Utility Truck Interactions: Control utility truck buckets in GTA V
## Created by PNWParksFan

This is a RagePluginHook plugin for single-player GTA V. 
It enables players to move the bucket on utility trucks up/down and rotate using simple controls.

### Requirements: 
 - RagePluginHook is required to load this script. FiveM is not currently supported, but the 
   mod is open source. If you are interested in porting it to FiveM, please let me know you 
   are working on it. 

 - You **MUST** install the included `v_boomtruck.ycd` clipset file into a streaming resource 
   for your game. You can either load it using an LML streaming folder, or by putting it in 
   any vehicles.rpf folder from an addon DLC or other location of your choosing. If you install 
   the enhanced utility truck model that is provided as a demo/companion vehicle mod, the YCD 
   is included with that package and you do not need to install it separately. 

### Installation Instructions:

 - To install the plugin, copy the DLL and INI files to your `Plugins` folder

 - Load the plugin using RPH, either automatically at startup or with the `LoadPlugin` command

 - Approach a truck with a utility bucket. The companion mod provides updates to the default utillitruck 
   model, but this also works with any addon or replacement model that uses the necessary model setup. 
   You do not need to register models in an INI or anything like that, they are detected automatically. 

 - Press and hold "X" on the keyboard or "A" on the controller to enable 
   the utility truck interaction mode. You will see a spinner in the lower 
   right corner of the screen for ~3 seconds while holding, then the mode 
   will be activated. 
   
 - Climb into the bucket by climbing on the model, or by standing behind the 
   truck and pressing the climb/jump key
   
 - Raise/lower the bucket using 7/9 on the numpad or LB/RB on the controller
 
 - Rotate the bucket using 4/6 on the numpad or left stick on the controller
 
 - Press INSERT on the keyboard or R3 on the controller to reset the bucket 
   (boom will automatically rotate back to center and lower to bottom)
   
 - If the model has outrigger legs (which the model provided does if 
   installed properly using LML), press and hold H on the keyboard or 
   right DPAD to raise/lower the outriggers. 
 
 - When the bucket is moving you will be attached to it and unable to move, 
   when it is stopped you can enter/exit/move around.
   
 - You can put NPC peds in the bucket as well and they will also be moved.

 
This script automatically detects and works with any vehicle with the required bones. However, some 
functionality may not work as expected if the relative position of the relevant dummies has not been retained. 
The recommended approach is to import the default `utillitruck` model and copy over the exact dummy positions 
for the following bones: `rotating_base`, `arm_1`, `arm_2`, `bucket`. 

If you create a utility truck model intended for use with this mod, please consider linking to this mod in 
your readme or credits file, and link to this page. Feel free to send me a link to your mod when it's released 
so I can see what you created using this plugin! If you found this plugin particularly useful, you 
can [donate to me](https://parksmods.com/donate/) to support my development projects and get member-exclusive benefits. 

[Download the latest version from the releases tab](https://github.com/pnwparksfan/utility-truck-interaction/releases)

[![Latest Version](https://img.shields.io/github/release/pnwparksfan/utility-truck-interaction)](https://github.com/pnwparksfan/utility-truck-interaction/releases)  
[![Download Count](https://img.shields.io/github/downloads/pnwparksfan/utility-truck-interaction/total)](https://github.com/pnwparksfan/utility-truck-interaction/releases)    

If you encounter any bugs, please [submit an issue](https://github.com/pnwparksfan/utility-truck-interaction/issues) or contact me on Discord if we share a mutual server.

[![Issues](https://img.shields.io/github/issues/pnwparksfan/utility-truck-interaction)](https://github.com/pnwparksfan/utility-truck-interaction/issues)  
[![Issues](https://img.shields.io/github/issues-closed/pnwparksfan/utility-truck-interaction)](https://github.com/pnwparksfan/utility-truck-interaction/issues)


### Credits:
 - Script by PNWParksFan
 - Thanks to Dexyfex for adding YCD (animation clipset) editing to CodeWalker
