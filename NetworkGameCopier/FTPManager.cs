using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.DotNet;
using NLog;

namespace NetworkGameCopier
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

        public void RetrieveGame(string destGamePath, string sourceGamePath, string targetIpServer, AsyncPack asyncPack)
        {
            new Thread(() =>
            {
                // create an FTP client
                FtpClient client = new FtpClient(targetIpServer)
                {
                    Port = FtpPort,
                    Credentials = new NetworkCredential("anonymous", "anonymous@batata.com")
                };

                // if you don't specify login credentials, we use the "anonymous" user account

                // begin connecting to the server
                client.Connect();
            
                // upload a file and retry 3 times before giving up
                client.RetryAttempts = 3;

                string sourcePath = client.GetWorkingDirectory() + sourceGamePath;
                // check if a folder exists
                if (client.DirectoryExists(sourcePath))
                {
                    long totalSize = 0;
                    List<FtpListItem> filesToDownloadList = new List<FtpListItem>();
                    BuildListOfFilesToDownload(client, sourcePath, filesToDownloadList, ref totalSize);
                    LogManager.GetCurrentClassLogger().Info("Size to be transfered: " + totalSize);

                    long alreadyDownloaded = 0;
                    // download the files without any sort of verification and error handling YET!
                    // It should be used a bigger number of threads, but for some reason the server can't handle it.
                    Parallel.For(0, filesToDownloadList.Count, new ParallelOptions { MaxDegreeOfParallelism = 1 },
                        i =>
                        {
                            // TODO: Revisit this code to show some statistics
                            //Stopwatch sw = new Stopwatch();
                            //sw.Start();
                            client.DownloadFile(destGamePath + filesToDownloadList[i].FullName, 
                                filesToDownloadList[i].FullName);
                            //sw.Stop();
                            //long elapsedTime = sw.ElapsedMilliseconds / 1000;
                            //long speed;
                            //if (elapsedTime != 0)
                            //    speed = filesToDownloadList[i].Size / elapsedTime;
                            //else
                            //    speed = 1;
                            alreadyDownloaded += filesToDownloadList[i].Size;
                            //long eta = (filesToDownloadList[i].Size - alreadyDownloaded) / speed;
                            // LogManager.GetCurrentClassLogger().Warn("Time: " + elapsedTime + "\nSpeed: " + speed / 1024 + "KB/s\nETA: " + eta);
                            asyncPack.Window.Dispatcher.Invoke(asyncPack.ToExecute,
                                Convert.ToDouble(alreadyDownloaded) / Convert.ToDouble(totalSize) * 100);
                        });
                }
                else
                {
                    throw new DirectoryNotFoundException();
                }

                // disconnect! good bye!
                client.Disconnect();
            }).Start();
        }

        private void BuildListOfFilesToDownload(FtpClient client, string sourcePath, List<FtpListItem> filesToDownloadList, ref long sizeToBeTransfered)
        {
            // get a list of files and directories in the folder
            foreach (FtpListItem item in client.GetListing(sourcePath))
            {
                // if this is a file
                if (item.Type == FtpFileSystemObjectType.File)
                {
                    sizeToBeTransfered += item.Size;
                    filesToDownloadList.Add(item);
                }
                else if (item.Type == FtpFileSystemObjectType.Directory)
                {
                    // going deeper!
                    BuildListOfFilesToDownload(client, sourcePath + "/" + item.Name, 
                        filesToDownloadList, ref sizeToBeTransfered);
                }
            }
        }
    }
    
}
