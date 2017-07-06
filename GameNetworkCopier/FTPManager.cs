using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.DotNet;

namespace GameNetworkCopier
{
    class FtpManager
    {
        private FtpServer _ftpServer;
        private static FtpManager instance = new FtpManager();

        public static FtpManager getInstance()
        {
            return instance;
        }

        public void LaunchFtpServer(string pathToServe)
        {
            // allow only anonymous logins
            var membershipProvider = new AnonymousMembershipProvider();

            // use %TEMP%/TestFtpServer as root folder
            var fsProvider = new DotNetFileSystemProvider(pathToServe, false);

            // Initialize the FTP server
            _ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1");

            // Start the FTP server
            _ftpServer.Start();

        }

        public void Stop()
        {
            _ftpServer?.Stop();
        }
    }
}
