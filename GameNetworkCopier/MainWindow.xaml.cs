using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace GameNetworkCopier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SteamOperations _steam;
        private NetworkManager _server;
        private NetworkManager _aClient;

        public MainWindow()
        {
            Init();
            InitializeComponent();
            _steam.ReadyFtpServer();
        }

        private void Init()
        {
            _steam = SteamOperations.GetInstance();
            _server = new NetworkManager(8086);
            _aClient = (NetworkManager) Activator.GetObject(
                typeof(NetworkManager),
                "tcp://localhost:8086/NetworkManager");
            ConfigLog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(_aClient.ping());
            string[] list = _aClient.GetGamesNamesList().ToArray();
            //Array.Sort(list);
            //SteamOperations.printArray(list);
            //Console.WriteLine(_aClient.GetDirFromGameName(list[0]));
            FtpManager.GetInstance().ftpClient();
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
    }
}
