using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using FubarDev.FtpServer.AuthTls;
using FubarDev.FtpServer.FileSystem.DotNet;
using NLog;

public delegate void WaitCallback(object state);


namespace GameNetworkCopier
{
    class FtpManager
    {
        private static readonly int FtpPort = 21;

        private readonly Logger _logger = LogManager.GetLogger(typeof(FtpManager).Name);
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

        public void RetrieveGame(string destGamePath, string sourceGamePath)
        {
            // create an FTP client
            FtpClient client = new FtpClient("127.0.0.1") {Port = FtpPort};

            // if you don't specify login credentials, we use the "anonymous" user account
            client.Credentials = new NetworkCredential("anonymous", "anonymous@batata.com");

            // begin connecting to the server
            client.Connect();
            
            // upload a file and retry 3 times before giving up
            client.RetryAttempts = 3;

            string sourcePath = client.GetWorkingDirectory() + sourceGamePath;
            // check if a folder exists
            if (client.DirectoryExists(sourcePath))
            {
                List<FtpListItem> filesToDownloadList = new List<FtpListItem>();
                BuildListOfFilesToDownload(client, sourcePath, filesToDownloadList);

                // download the files without any sort of verification and error handling YET!
                // It should be used a bigger number of threads, but for some reason the server can't handle it.
                Parallel.For(0, filesToDownloadList.Count, new ParallelOptions { MaxDegreeOfParallelism = 1 },
                    i =>
                    {
                        client.DownloadFile(destGamePath + filesToDownloadList[i].FullName, 
                            filesToDownloadList[i].FullName);
                    });
            }
            else
            {
                throw new DirectoryNotFoundException();
            }

            // disconnect! good bye!
            client.Disconnect();
        }

        public void RunDownloadFile(object threadPack)
        {
            ThreadPack tp = threadPack as ThreadPack;
            tp?.Client.DownloadFile(tp.DestGamePath + tp.Item.FullName, tp.Item.FullName);
        }

        private void BuildListOfFilesToDownload(FtpClient client, string sourcePath, List<FtpListItem> filesToDownloadList)
        {
            // get a list of files and directories in the folder
            foreach (FtpListItem item in client.GetListing(sourcePath))
            {
                // if this is a file
                if (item.Type == FtpFileSystemObjectType.File)
                {
                    filesToDownloadList.Add(item);
                }
                else if (item.Type == FtpFileSystemObjectType.Directory)
                {
                    // going deeper!
                    BuildListOfFilesToDownload(client, sourcePath + "/" + item.Name, filesToDownloadList);
                }
            }
        }
    }

    class ThreadPack
    {
        public FtpClient Client { get; set; }
        public FtpListItem Item { get; set; }
        public String DestGamePath { get; set; }
    }
    
}
