using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using RestSharp;
using Ionic.Zip;
using Microsoft.Win32;

namespace UpdateService
{
    public partial class UpdateService1 : ServiceBase
    {
        public UpdateService1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //#if DEBUG
            //    System.Diagnostics.Debugger.Launch();
            //#endif
            // https://api.github.com/repos/francisco-polaco/network-game-copier/releases/latest
            var client = 
                new RestClient("https://api.github.com/repos/francisco-polaco/network-game-copier/releases/latest");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            string[] lines = response.Content.Split(',');
            foreach (var line in lines)
            {
                if (line.Contains("browser_") && line.Contains(".zip"))
                {
                    string[] downloadUrlArray = line.Split(':');
                    string downloadUrl = (downloadUrlArray[1] + ":" + downloadUrlArray[2])
                        .Replace("\"", "")
                        .Replace("}", "")
                        .Replace("]", "");
                    ulong newVersion;
                    if (VersionIsUpdatable(downloadUrl, out newVersion))
                    {
                        var dirName = Path.Combine(Path.GetTempPath(), "NetworkGameCopier_update");
                        var updateDir = new DirectoryInfo(dirName);
                        updateDir.Create();
                        var fileName = Path.Combine(dirName, "update.zip");
                        new WebClient().DownloadFile(downloadUrl, fileName);
                        Process[] pname = Process.GetProcessesByName("NetworkGameCopier");
                        if (pname.Length != 0)
                        {
                            pname[0].WaitForExit();
                        }
                        using (ZipFile zip = ZipFile.Read(fileName))
                        {
                            zip.ExtractAll(ReadExecutionLocation(), ExtractExistingFileAction.OverwriteSilently);
                        }
                        using (var streamWriter = new StreamWriter(Path.Combine(ReadExecutionLocation(), "version.dat")))
                        {
                            streamWriter.Write(newVersion.ToString());
                        }
                        // Cleaning up the trash
                        updateDir.Delete(true);
                    }
                    break;
                }
            }
            Stop();
        }

        private bool VersionIsUpdatable(string downloadUrl, out ulong mostRecentVersion)
        {
            StreamReader sr = new StreamReader(
                Path.Combine(ReadExecutionLocation(), "version.dat"));
            ulong version = UInt64.Parse(sr.ReadToEnd());
            // get downloaded version
            mostRecentVersion = 
                ulong.Parse(downloadUrl.Substring(downloadUrl.IndexOf("2", downloadUrl.Length - 1 - 15))
                .Replace(".zip", ""));
            Console.WriteLine("Version: " + version + " VS " + mostRecentVersion);
            return version < mostRecentVersion;
        }

        private string ReadExecutionLocation()
        {
            //return @"C:\Users\franc\Source\Repos\game-network-copier\NetworkGameCopier\bin\Debug";
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software");
            key = key?.OpenSubKey("NetworkGameCopier");
            //key = key?.OpenSubKey("InstalledPath")
            return key?.GetValue("InstalledPath").ToString();
        }

        protected override void OnStop()
        {
        }
    }
}
