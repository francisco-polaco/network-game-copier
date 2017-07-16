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

        public List<NameSizePair> GetGamesNamesList()
        {
            return GameProviderSingleton.GetInstance().Active.GetGameNamesList();
        }

        public string GetDirFromGameName(string gameName)
        {
            return GameProviderSingleton.GetInstance().Active.GetDirFromGameName(gameName);
        }

    }
}
