using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using NLog;

namespace NetworkGameCopier
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
        private const string Message = "GameNetworkCopier-DiscoverService-YouThere?";
        private const string Ack_Message = "GameNetworkCopier-DiscoverService-Kappa";
        private HashSet<string> _ipsRetrieved;
        private Window _window;
        private DelAddComputer _delAddComputer;

        private DiscoverService()
        {
            _ipsRetrieved = new HashSet<string>();
            _udpListener = new UdpClient(Port);
        }

        public void StartListening()
        {
            _ar = _udpListener.BeginReceive(Receive, new object());
        }
        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Port);
            byte[] bytes = _udpListener.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            LogManager.GetCurrentClassLogger().Info("From {0} received: {1} ", ip.Address, message);
            // To prevent to add ourselves to the list, uncomment below.
            // if (Dns.GetHostEntry(ip.Address).HostName.Equals(Dns.GetHostName() + ".lan")) return;
            if (message.Equals(Message))
            {
                LogManager.GetCurrentClassLogger().Info("Request from discover service received.");
                if(_ipsRetrieved.Add(ip.Address.ToString()))
                    _window.Dispatcher.Invoke(_delAddComputer, ip.Address.ToString());
                AckLiveServer(ip.Address);
            }
            else if (message.Equals(Ack_Message))
            {
                LogManager.GetCurrentClassLogger().Info("Ack from other server received.");
                if(_ipsRetrieved.Add(ip.Address.ToString()))
                    _window.Dispatcher.Invoke(_delAddComputer, ip.Address.ToString());
            } //Dns.GetHostEntry(ip.Address).HostName
            StartListening();
        }

        public void RetrieveLiveServers(Window window, DelAddComputer delAddComputer)
        {
            _window = window;
            _delAddComputer = delAddComputer;
            _ipsRetrieved.Clear();
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, Port);
            byte[] bytes = Encoding.ASCII.GetBytes(Message);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }

        public void AckLiveServer(IPAddress clientAddress)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(clientAddress, Port);
            byte[] bytes = Encoding.ASCII.GetBytes(Ack_Message);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }

        public void PrintListKnownIps()
        {
            PrintHelper.PrintList(new List<string>(_ipsRetrieved));
        }
    }
}
