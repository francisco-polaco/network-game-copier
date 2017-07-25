using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace NetworkGameCopier
{
    public class NetworkManager : MarshalByRefObject
    {
        public NetworkManager(int port)
        {
            ChannelServices.RegisterChannel(new TcpChannel(port), false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(NetworkManager),
                "NetworkManager",
                WellKnownObjectMode.Singleton);
        }

        public NetworkManager()
        {
        }

        public string Ping()
        {
            Console.WriteLine("Ping!");
            return "Pong!";
        }

        public List<NameSizePair> GetGamesNamesListSteam()
        {
            return SteamOperations.GetInstance().GetLocalGamesNamesList();
        }

        public string GetDirFromGameNameSteam(string gameName)
        {
            return SteamOperations.GetInstance().GetDirFromGameName(gameName);
        }

        public List<NameSizePair> GetGamesNamesListBlizzard()
        {
            return BlizzardOperations.GetInstance().GetLocalGamesNamesList();
        }

        public string GetDirFromGameNameBlizzard(string gameName)
        {
            return BlizzardOperations.GetInstance().GetDirFromGameName(gameName);
        }

    }
}
