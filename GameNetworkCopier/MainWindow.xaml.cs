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

namespace GameNetworkCopier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BackendSingleton.GetInstance();
            Console.WriteLine(BackendSingleton.GetInstance()._aClient.ping());
            string[] list = BackendSingleton.GetInstance()._aClient.GetGamesNamesList().ToArray();
            Array.Sort(list);
            SteamOperations.printArray(list);
            Console.WriteLine(BackendSingleton.GetInstance()._aClient.GetDirFromGameName(list[0]));
            //FtpManager.GetInstance().ftpClient();
        }
    }
}
