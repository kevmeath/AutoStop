# AutoStop
This plugin stops the server after a configured delay when the last player leaves. It is intended to be used on servers with automatic startup and shutdown already configured on the system, otherwise this only automates one step and power savings are minimal.

## Installation
1. Set up a [Terraria](https://www.terraria.org/) server with [TShock](https://github.com/Pryaxis/TShock)
2. Download AutoStop from the [releases page](https://github.com/kevmeath/AutoStop/releases). (Make sure it matches your TShock version)
3. Place AutoStop.dll in `<your_server_folder>/ServerPlugins`

## Configuration
After the first run a configuration file will be created at `<your_server_folder>/tshock/AutoStop.json`. Here you can set the delay in **milliseconds** before the server stops. The default is 10 minutes.
```
{
  "Settings": {
    "Delay": 600000
  }
}
```
