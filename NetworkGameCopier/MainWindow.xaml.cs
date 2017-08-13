using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MaterialDesignThemes.Wpf;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NetworkGameCopier
{

    public delegate void DelAddComputer(string computer);
    public delegate void DelProgress(double percentage);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int NetworkManagerPort = 8086;

        private NetworkManager _server;
        private NetworkManager _client;
        private string _targetClientIp;
        //private NetworkPerformanceReporter _network;

        public MainWindow()
        {
            Init();
            InitializeComponent();
            MySnackbar.MessageQueue = 
                new SnackbarMessageQueue(TimeSpan.FromSeconds(Properties.Resources.SnackbarSecondsDuration));
            GameProviderSingleton.GetInstance();
            CheckBoxShutdownAfter.IsChecked = SettingsManager.GetInstance().GetShutdownAfterDownloads();
            Refresh_Button_Click(null, null);
            LaunchUpdater();
            //_network = NetworkPerformanceReporter.Create();
            //Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(v =>
            //{
            //    Console.WriteLine(_network.GetNetworkPerformanceData().BytesReceived);
            //});
        }

        private void LaunchUpdater()
        {
                                                           // "Network Game Copier Updater"
            ServiceController service = new ServiceController("Network Game Copier Updater");
            service.Start();
        }


        private void Init()
        {
            ConfigLog();
            DiscoverService.GetInstance().StartListening();
            _server = new NetworkManager(NetworkManagerPort);
        }

        private NameSizePair SizeFromBytesToMBytes(NameSizePair pair)
        {
            string size = pair.Size;
            long parsed;
            if (Int64.TryParse(size, out parsed))
            {
                long sizeInMb = parsed / 1024 / 1024;
                return new NameSizePair {Name = pair.Name, Size = sizeInMb.ToString()};
            }
            return new NameSizePair {Name = pair.Name, Size = "0"};
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
            fileTarget.FileName = "${basedir}/log.txt";
            fileTarget.Layout = "${message}";

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            // Example usage
            //Logger logger = LogManager.GetLogger("Example");
            //logger.Trace("trace log message");
            //logger.Debug("debug log message");
            //logger.Info("info log message");
            //logger.Warn("warn log message");
            //logger.Error("error log message");
            //logger.Fatal("fatal log message");
        }

        private void GamesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LogManager.GetCurrentClassLogger().Debug("Selected: {0}", e.AddedItems[0]);
            }
            catch (IndexOutOfRangeException)
            {
                // Avoid crashing after selecting a game and changing provider
                return;
            }
            NameSizePair gamePair = e.AddedItems[0] as NameSizePair;
            if (gamePair != null)
            {
                DownloadTaskQueue.GetInstance().QueueJob(
                    GameProviderSingleton.GetInstance().Active, 
                    gamePair.Name, _client, _targetClientIp, 
                    new AsyncPack { ToExecute = new DelProgress(Progress), Window = this });
                    //GameProviderSingleton.GetInstance().Active
                    //    .RetrieveGame(gamePair.Name, _client, _targetClientIp,
                    //        new AsyncPack {ToExecute = new DelProgress(Progress), Window = this});
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
            FillList();
        }

        private void FillList()
        {
            if(_client == null) return;
            GamesList.SelectedItems.Clear();
            NameSizePair[] list = GameProviderSingleton.GetInstance().Active.GetRemoteGamesNamesList(_client);
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

        private void ButtonBlizzard_OnClick(object sender, RoutedEventArgs e)
        {
            GameProviderSingleton.GetInstance().Active = 
                BlizzardOperations.GetInstance();
            FillList();
        }

        private void ButtonSteam_OnClick(object sender, RoutedEventArgs e)
        {
            GamesList.SelectedIndex = -1;
            GameProviderSingleton.GetInstance().Active =
                SteamOperations.GetInstance();
            FillList();
        }


        private void ButtonAcceptSettings_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsManager.GetInstance().SetDefaultBlizzardPath(SettingsBlizzardPath.Text);
            SettingsManager.GetInstance().SetDefaultSteamLibrary(SteamLibsComboBox.SelectedItem.ToString());
            new Thread(() =>
                SettingsManager.GetInstance().ForceSave()).Start();
        }

        private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsBlizzardPath.Text = SettingsManager.GetInstance().GetDefaultBlizzardPath();
            SteamLibsComboBox.Items.Clear();
            List<string> libs = SteamOperations.GetInstance().GetLibraryPaths();
            if(!libs.Contains(SettingsManager.GetInstance().GetDefaultSteamLibrary()))
                libs.Add(SettingsManager.GetInstance().GetDefaultSteamLibrary());
            libs.Sort();
            foreach (var libraryPath in libs)
            {
                SteamLibsComboBox.Items.Add(libraryPath);
            }
            SteamLibsComboBox.SelectedItem = SettingsManager.GetInstance().GetDefaultSteamLibrary();
            /* The second command in the array represents the close drawer command.
            * Check XAML Toolkit Sources, namely a constructor at line 77
            * https://github.com/ButchersBoy/MaterialDesignInXamlToolkit/blob/master/MaterialDesignThemes.Wpf/DrawerHost.cs
            */
            //if(!(sender == null && e == null)) Drawer.CommandBindings[1].Command.Execute(null);
        }

        private void ButtonBrowseSteam_OnClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    if (!SteamLibsComboBox.Items.Contains(dialog.SelectedPath))
                    {
                        SteamLibsComboBox.Items.Clear();
                        List<string> libs = SteamOperations.GetInstance().GetLibraryPaths();
                        libs.Add(dialog.SelectedPath);
                        libs.Sort();
                        foreach (var libraryPath in libs)
                        {
                            SteamLibsComboBox.Items.Add(libraryPath);
                        }
                    }
                    SteamLibsComboBox.SelectedItem = dialog.SelectedPath;
                }
            }
        }

        private void ButtonBrowseBlizzard_OnClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    SettingsBlizzardPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void ButtonDefaultSettings_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsManager.GetInstance().LoadDefaultValues();
            ButtonSettings_OnClick(null, null);

            MySnackbar.MessageQueue.Enqueue("Settings reset", "UNDO", HandleSettingsUndoMethod);
        }

        private void HandleSettingsUndoMethod()
        {
            SettingsManager.GetInstance().LoadSettings();
            ButtonSettings_OnClick(null, null);
        }

        private void CheckBoxShutdownAfter_OnClick(object sender, RoutedEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug(CheckBoxShutdownAfter.IsChecked);
            SettingsManager.GetInstance().SetShutdownAfterDownload(CheckBoxShutdownAfter.IsChecked);
            new Thread(SettingsManager.GetInstance().ForceSave).Start();
        }
    }

   
}
