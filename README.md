# Project LGiM

## Table of contents
* [General info](#general-info)
* [Technologies](#technologies)
* [Setup](#setup)
* [Controls](#controls)
  * [General](#general)
  * [Moving mode](#moving-mode)
  * [Scaling mode](#scaling-mode)
  * [Rotating mode](#rotating-mode)

## General info
The project is 3D engine containing basic functionalities such as loading 3D models in .obj format, loading textures for models, rotating, moving and scaling models. It's also possible to move and rotate the camera.


## Technologies
* [C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/)
* [Monodevelop](https://www.monodevelop.com/)
* [Mono](https://www.mono-project.com/)
* [GtkSharp](https://www.mono-project.com/docs/gui/gtksharp/)
* [Math.Net](https://www.mathdotnet.com/)

## Setup
To run this project:
1. [Download](https://www.monodevelop.com/download/) and install Monodevelop.
1. Open Monodevelop.
2. Press Control + O and choose 3dEngine.sln.
3. Press Control + F5 to run the program.


## Controls


### General
| Key | Function |
|-----|----------|
| W   | Move the camera forward |
| S | Move the camera backward |
| A | Move the camera to the left |
| D | Move the camera to the right |
| Space | Move the camera up |
| Left Control | Move the camera down |
| LPM + Mouse move | Rotate the camera in the x-axis and y-axis |
| Left Shift + LPM + Mouse move | Rotate the camera in the z-axis |
| Mouse Scroll | Zoom |
| 1 | Change mode to moving |
| 2 | Change mode to scaling | 
| 3 | Change mode to rotating |

### Moving mode
| Key | Function |
|-----|----------|
| PPM + Horizontal mouse move | Move the object to the left/right |
| PPM + Vertical mouse move | Move the object up/down |
| Left Shift + PPM + Veritcal mouse move | Move the object to/from yourself |

### Scaling mode
| Key | Function |
|-----|----------|
| PPM + Horizontal mouse move | Scale the object horizontally  |
| PPM + Vertical mouse move | Scale the object vertically |
| Left Shift + PPM + Veritcal mouse move | Scale the object horizontally and vertically |

### Rotating mode
| Key | Function |
|-----|----------|
| PPM + Horizontal mouse move | Rotate the object horizontally  |
| PPM + Vertical mouse move | Rotate the object vertically |
| Left Shift + PPM + Veritcal mouse move | Rotate the object in the z-axis |
