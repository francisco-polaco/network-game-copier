using System.Windows;

namespace NetworkGameCopier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
       
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Perform tasks at application exit
            FtpManager.GetInstance().StopFtpServer();
        }

    }
    

}
