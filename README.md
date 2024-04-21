# FloppyVPN
### The full-strack and cross-platform VPN company software. Very private and 100% open source.

## Download
 Please use the official website to register, download and use FloppyVPN - http://floppy.jp.net. If you want to compile it yourself or learn more about the project - continue reading.

## Stack
- Servers: C# (ASP.NET Core 6)
- Website: C#; HTML, CSS, JS (ASP.NET Core 6 MVC)
- Database: MySQL
- Windows App & Installer: C# (WinForms)
- Android App: 

## Project logic

Wireguard protocol (AmneziaWG) and related software is taken as the protocol and drivers of FloppyVPN. It is simple, fast and safe. All the apps use it.

VPN servers use ports differ from standard WG ones to because in some networks you simply could not connect to a web resource on certain ports. Also, connections to these ports look a bit more legit.

Servers (orchestrator one, website one and VPN ones) use an authorization key called "master_key" to communicate with each other via API. Without a same key no server-server communication is possible. The key is an alphanumeric string of any length (recommended length - from 16 to 64), is also used to perform administrative actions.

Storing and provisioning of configs (unique connection strings) on severs are on behalf of the orchestrator server; The reason of such logic lies in instability of VPN servers - they may be deleted unexpectedly.

Expanding a new VPN server on a new Linux box is easy and is being automated as possible though a kernel module installation is required.


## Deploying
### How to fully deploy the app.