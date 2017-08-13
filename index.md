# Network Game Copier
A tool that allows users to copy their games over the local network.
## Motivation
If you have a slow connection to the internet like me, you only have patience to wait for one computer to download your game. One could argue that using an USB external storage you would have this problem solved, but sometimes having a solution that works over the network, using your local network full speed, is easier and pratical.<br>
Plus, if a friend is coming over, you can transfer a game to him using this tool.
## Current State
It has a cute but far from perfect GUI that allows users to list the Steam and Blizzard games from a computer selected and download them using FTP.
### Next Planned Features
* GUI reflecting operations' queue;
* Integrity verification of updates;
* Resume aborted downloads;
* Deletion of explicit stopped downloads.
### Security
Every message are susceptible to attacks and are in plain text. Since this tool will be used in a supposed friendly LAN and it will be used to copy games, introduce SFTP or SSL will bring an overhead that we do not need.
### Support
Right now, I'm only fully considering Steam and partially Blizzard. Other platforms should be easy to integrate, but since this project is done on my spare time and I cannot compromise to it fully, it will stay this way. New platforms and features will be announced through a tbd channel. 
