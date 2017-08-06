using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using RestSharp;

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
                    string downloadUrl = (downloadUrlArray[1] + downloadUrlArray[2])
                        .Replace("\"", "")
                        .Replace("}", "")
                        .Replace("]", "");
                    if (VersionIsUpdatable(downloadUrl))
                    {
                        var fileName = Path.Combine(Path.GetTempPath(), "NetworkGameCopier_update", "update.zip");
                        new WebClient().DownloadFile(downloadUrl, fileName);
                        Process[] pname = Process.GetProcessesByName("NetworkGameCopier");
                        if (pname.Length != 0)
                        {
                            pname[0].WaitForExit();
                        }
                        ZipFile.ExtractToDirectory(fileName, ReadExecutionLocation());
                        // Cleaning up the trash
                        //new DirectoryInfo(Path.Combine(Path.GetTempPath(), "NetworkGameCopier_update")).Delete(true);
                    }
                    break;
                }
            }
            Stop();
        }

        private bool VersionIsUpdatable(string downloadUrl)
        {
            StreamReader sr = new StreamReader(
                Path.Combine(ReadExecutionLocation(), "version.dat"));
            UInt64 version = UInt64.Parse(sr.ReadToEnd());
            // get downloaded version
            UInt64 downloadVersion = 50;
            return version < downloadVersion;
        }

        private string ReadExecutionLocation()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\NetworkGameCopier\InstalledPath");
            return key?.GetValue("InstalledPath").ToString();
        }

        protected override void OnStop()
        {
        }
    }
}
