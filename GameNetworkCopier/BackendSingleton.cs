using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNetworkCopier
{
    class BackendSingleton
    {
        private static readonly BackendSingleton Instance = new BackendSingleton();

        public static BackendSingleton getInstance()
        {
            return Instance;
        }

        public SteamOperations _steam = new SteamOperations();
        public NetworkManager _server = new NetworkManager(8086);
        public NetworkManager _aClient;


        public BackendSingleton()
        {
            _aClient = (NetworkManager)Activator.GetObject(
                    typeof(NetworkManager),
                    "tcp://localhost:8086/NetworkManager");
        }
    }
}
