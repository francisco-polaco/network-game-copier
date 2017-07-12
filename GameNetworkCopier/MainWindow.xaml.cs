using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace GameNetworkCopier
{

    public delegate void DelAddComputer(string computer);
    public delegate void DelProgress(double percentage);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int NetworkManagerPort = 8086;

        private SteamOperations _steam;
        private NetworkManager _server;
        private NetworkManager _client;
        private string _targetClientIp;

        public MainWindow()
        {
            Init();
            InitializeComponent();
            _steam.ReadyFtpServer();
            Refresh_Button_Click(null, null);
        }

        private void Init()
        {
            ConfigLog();
            DiscoverService.GetInstance().StartListening();
            _steam = SteamOperations.GetInstance();
            _server = new NetworkManager(NetworkManagerPort);
        }

        private NameSizePair SizeFromBytesToMBytes(NameSizePair pair)
        {
            string size = pair.Size;
            long sizeInMb = Int64.Parse(size) / 1024 / 1024;
            return new NameSizePair {Name = pair.Name, Size = sizeInMb.ToString()};
        }

        private void ConfigLog()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";
            fileTarget.FileName = "${basedir}/file.txt";
            fileTarget.Layout = "${message}";

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            // Example usage
            Logger logger = LogManager.GetLogger("Example");
            logger.Trace("trace log message");
            logger.Debug("debug log message");
            logger.Info("info log message");
            logger.Warn("warn log message");
            logger.Error("error log message");
            logger.Fatal("fatal log message");
        }

        private void GamesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("Selected: {0}", e.AddedItems[0]);
            NameSizePair gamePair = e.AddedItems[0] as NameSizePair;
            if (gamePair != null)
            {
                Progress(0);
                SteamOperations.GetInstance()
                    .RetrieveGame(gamePair.Name, _client, _targetClientIp, 
                        new AsyncPack{ToExecute = new DelProgress(Progress), Window = this});
            }
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            ComputerComboBox.Items.Clear();
            DiscoverService.GetInstance().RetrieveLiveServers(this, AddComputer);
        }

        public void AddComputer(string computer)
        {
            if(!ComputerComboBox.IsEditable) ComputerComboBox.IsEnabled = true;
            ComputerComboBox.Items.Add(computer);
        }

        public void Progress(double percentage)
        {
            if (Math.Abs(percentage - 100) < 0.1)
            {
                ProgressBar.Opacity = 0;
                Percentage.Opacity = 0;
                StateText.Text = "Idle";
            }
            else
            {
                if (Math.Abs(ProgressBar.Opacity - 100) > 1)
                {
                    ProgressBar.Opacity = 100;
                    Percentage.Opacity = 100;
                    StateText.Text = "Listing";

                }else
                    StateText.Text = "Downloading";
                ProgressBar.Value = percentage;
                Percentage.Text = percentage.ToString("0.00") + "%";
            }
        }

        private void ComputerComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _targetClientIp = e.AddedItems[0].ToString();
            //string ip = Dns.GetHostAddresses(_targetClientIp)[0].ToString();
            LogManager.GetCurrentClassLogger().Info("Current client selected: " + _targetClientIp);
            _client = (NetworkManager)Activator.GetObject(
                typeof(NetworkManager),
                BuildTcpRemoteEndpoint(_targetClientIp));
            LogManager.GetCurrentClassLogger().Info(_client.Ping());
            NameSizePair[] list = _client.GetGamesNamesList().ToArray();
            Array.Sort(list);
            GamesList.Items.Clear();
            foreach (NameSizePair game in list)
            {
                GamesList.Items.Add(SizeFromBytesToMBytes(game));
            }
        }

        private static string BuildTcpRemoteEndpoint(string ip)
        {
            return "tcp://" + ip + ":" + NetworkManagerPort + "/NetworkManager";
        }
    }

   
}
