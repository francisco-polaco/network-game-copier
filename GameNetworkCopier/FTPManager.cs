using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentFTP;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using FubarDev.FtpServer.AuthTls;
using FubarDev.FtpServer.FileSystem.DotNet;
using NLog;

namespace GameNetworkCopier
{
    class FtpManager
    {
        private static readonly int FtpPort = 21;

        private Logger logger = LogManager.GetLogger("Example");
        private FtpServer _ftpServer;
        private static readonly FtpManager Instance = new FtpManager();

        public static FtpManager GetInstance()
        {
            return Instance;
        }

        public void LaunchFtpServer(string pathToServe)
        {
            // allow only anonymous logins
            var membershipProvider = new AnonymousMembershipProvider();


            // use %TEMP%/TestFtpServer as root folder
            var fsProvider = new DotNetFileSystemProvider(pathToServe, false);

            // Use all commands from the FtpServer assembly and NOT the one(s) from the AuthTls assembly
            var commandFactory = new AssemblyFtpCommandHandlerFactory(typeof(FtpServer).Assembly);


            // Initialize the FTP server
            _ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1", FtpPort, commandFactory)
            {
                DefaultEncoding = Encoding.ASCII, // This can cause trouble.
                LogManager = new FtpLogManager(),
            };

            // Start the FTP server
            _ftpServer.Start();

        }

        public void StopFtpServer()
        {
            _ftpServer?.Stop();
        }

        public void ftpClient()
        {
            // create an FTP client
            FtpClient client = new FtpClient("127.0.0.1") {Port = FtpPort};

            // if you don't specify login credentials, we use the "anonymous" user account
            client.Credentials = new NetworkCredential("anonymous", "anonymous@batata.com");

            // begin connecting to the server
            client.Connect();

            // get a list of files and directories in the "/htdocs" folder
            foreach (FtpListItem item in client.GetListing("/htdocs"))
            {

                // if this is a file
                if (item.Type == FtpFileSystemObjectType.File)
                {

                    // get the file size
                    long size = client.GetFileSize(item.FullName);

                }

                // get modified date/time of the file or folder
                DateTime time = client.GetModifiedTime(item.FullName);

                // calculate a hash for the file on the server side (default algorithm)
                //maybe need o import tls commands
                //FtpHash hash = client.GetHash(item.FullName);

            }

            // upload a file
            client.UploadFile(@"C:\teste\asdf.txt", "/htdocs/big.txt");

            // rename the uploaded file
            //client.Rename("/htdocs/big.txt", "/htdocs/big2.txt");

            // download the file again
            client.DownloadFile(@"C:\\teste\\lel.txt", "/htdocs/teste.txt");

            // delete the file
            //client.DeleteFile("/htdocs/big2.txt");

            // delete a folder recursively
            //client.DeleteDirectory("/htdocs/extras/");

            // check if a file exists
            if (client.FileExists("/htdocs/big2.txt")) { }

            // check if a folder exists
            if (client.DirectoryExists("/htdocs/extras/")) { }

            // upload a file and retry 3 times before giving up
            client.RetryAttempts = 3;
          //  client.UploadFile(@"C:\MyVideo.mp4", "/htdocs/big.txt", FtpExists.Overwrite, false, FtpVerify.Retry);

            // disconnect! good bye!
            client.Disconnect();
        }
    }
}
