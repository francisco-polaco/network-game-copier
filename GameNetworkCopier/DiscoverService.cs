using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameNetworkCopier
{
    class DiscoverService
    {
        private static readonly DiscoverService Instance = new DiscoverService();

        public static DiscoverService GetInstance()
        {
            return Instance;
        }

        private UdpClient _udpListener;
        IAsyncResult _ar = null;
        private const int Port = 11000;
        private HashSet<string> _ipsRetrieved;
        private DiscoverService()
        {
            _ipsRetrieved = new HashSet<string>();
            _udpListener = new UdpClient(Port);
        }

        public void StartListening()
        {
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, Port);

            _ar = _udpListener.BeginReceive(Receive, new object());
        }
        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Port);
            byte[] bytes = _udpListener.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            Console.WriteLine("From {0} received: {1} ", ip.Address.ToString(), message);
            StartListening();
        }

        public void Broadcast()
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, Port);
            byte[] bytes = Encoding.ASCII.GetBytes("Hello!");
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }
    }
}
