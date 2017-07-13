using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace NetworkGameCopier
{
    class NetworkManager : MarshalByRefObject
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
            return SteamOperations.GetInstance().GetGameNamesList();
        }

        public string GetDirFromGameName(string gameName)
        {
            return SteamOperations.GetInstance().GetDirFromGameName(gameName);
        }

    }
}
