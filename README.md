# Wheel Addon

A plugin for OTD that add support for the touch wheel present in some Wacom tablets

## Supported Versions

- OpenTabletDriver 0.5.3.1 to 0.5.3.3

## Dependencies

- [.NET 5 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/5.0#:~:text=x86-,.NET%20Desktop%20Runtime%205.0.17,-The%20.NET%20Desktop) (for OpenTabletDriver 0.5.x)
- [.NET 7 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0#:~:text=x86-,.NET%20Desktop%20Runtime) (for Wheel Addon's interface)
- [OpenTabletDriver 0.5.3.x](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/tag/v0.5.3.3)
- [OTD.Backport](https://github.com/Mrcubix/OTD.Backport)
- [OTD.EnhancedOutputMode](https://github.com/Mrcubix/OTD.EnhancedOutputMode)

## Installation

### Plugin

1. Download the latest release .zip file for your version of OpenTabletDriver from [here](https://github.com/Mrcubix/WheelAddon/releases/latest),
2. Open the plugin manager in OpenTabletDriver,
3. Drag and drop the downloaded .zip file into the plugin manager,
4. Restart OpenTabletDriver,
5. Head to the filter tab,
6. Click on 'Wheel Addon' in the filter list,
7. Enable the filter.

### Interface

1. Download the binary (executable) for your specific OS from [here](https://github.com/Mrcubix/WheelAddon/releases/latest)

Windows: `Wheel-Addon.UX-win-x64.exe`
Linux: `Wheel-Addon.UX-linux-x64`
~~MacOS: `Wheel-Addon.UX-osx-x64`~~

2. Run the binary

If everything went as intended, you should see the state 'Connected' at the bottom right of the interface.
From there, you can start tweaking your settings.

## Usage

### OpenTabletDriver UX

In the filter section of OpenTabletDriver, you'll find a new filter called 'Wheel Addon'.
This filter is used to configure numerical values of the plugin.
You can hover on any properties's boxes if you need any explanations on what each does.

But here is the copy pasted content for each of those properties:

- Toggle Mode: 

    - When disabled, the wheel will act in Simple Mode, which mean that a clockwise or counter-clockwise rotation will trigger an action.
    - When enabled, the wheel will act in Advanced Mode, where each slices defined by the user will trigger an action when the finger stop touching the wheel. 

(Default: Simple Mode (Unchecked))

- Rotation Threshold: /!\ Only in Simple Mode /!\ The threshold before the designated action is performed. 

(Default: 20 au)

- Large Rotation Threshold: /!\ Only in Simple Mode /!\ The threshold at which a rotation is considered large, for exemple, when going from the lowest value, to the highest after a full rotation. 

(Default: 30 au)

- Debounce During Touch: It happens that sometimes, the touch wheel will stop detecting your finger for a short moment.
This will prevent such issues from happening if set properly.
Its value need to be under `amount` * 15ms < The value of `Debounce After Touch`ing the wheel. 

(Default: 3 packets)

- Debounce After Touch: The debounce mechanism forcing the handler to recognize whenever the user stop touching the wheel. 

(Default: 120 ms)