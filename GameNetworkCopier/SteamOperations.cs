using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Win32;

namespace GameNetworkCopier
{

    class SteamOperations
    {
        private static readonly SteamOperations Instance = new SteamOperations();

        public static SteamOperations GetInstance()
        {
            return Instance;
        }

        private Dictionary<string, PathnameSizePair> _installedGames = new Dictionary<string, PathnameSizePair>();
        private List<string> _libraryPaths;
        private string _steamappsPath;

        private SteamOperations()
        {
            FindSteamappsPath();
            DirectoryInfo di = new DirectoryInfo(_steamappsPath);
            foreach (var file in FullDirList(di, "*.acf"))
            {
                string[] lines = File.ReadAllLines(_steamappsPath + "\\" + file);
                GetSteamGameDetails(lines);
            }
            _libraryPaths = GetLibraryPaths(File.ReadAllLines(_steamappsPath + "\\" + "libraryfolders.vdf"));
        }

        public void ReadyFtpServer()
        {
            FtpManager.GetInstance().LaunchFtpServer(Path.Combine(_steamappsPath, "common"));
        }

        public List<NameSizePair> GetGameNamesList()
        {
            List<NameSizePair> list = new List<NameSizePair>();
            foreach (KeyValuePair<string, PathnameSizePair> entry in _installedGames)
            {
                list.Add(new NameSizePair
                    {
                        Name = entry.Key, Size = entry.Value.Size
                    });
            }
            return list;
        }

        public string GetDirFromGameName(string gameName)
        {
            return _installedGames[gameName].PathName;
        }

        private void GetSteamGameDetails(string[] lines)
        {
            string futureKey = null;
            string pathname = "";
            foreach (var line in lines)
            {
                if (line.Contains("\"name\"") )
                {
                    futureKey = GetValuesFromLine(line)[1];
                }
                else if (line.Contains("\"installdir\""))
                {
                    pathname = GetValuesFromLine(line)[1];
                }
                else if (line.Contains("\"SizeOnDisk\""))
                {
                    if (futureKey != null)
                        _installedGames[futureKey] =
                            new PathnameSizePair {PathName = pathname, Size = GetValuesFromLine(line)[1]};
                    break;
                }
            }
        }

        private List<string> GetLibraryPaths(string[] lines)
        {
            List<string> paths = new List<string>();
            foreach (var line in lines)
            {
                if (line.Contains("\"1\""))
                {
                    var aux = GetValuesFromLine(line);
                    paths.Add(aux[1]);
                }
            }
            return paths;
        }

        private List<string> GetValuesFromLine(string line)
        {
            string[] strings = line.Split('\t');
            List<string> aux = new List<string>();
            foreach (string s in strings)
            {
                if (s.StartsWith("\"") && s.EndsWith("\""))
                {
                    aux.Add(s.Replace("\"", ""));
                }
            }
            return aux;
        }

        ArrayList FullDirList(DirectoryInfo dir, string searchPattern)
        {
            ArrayList files = new ArrayList();
            // Console.WriteLine("Directory {0}", dir.FullName);
            // list the files
            try
            {
                foreach (FileInfo f in dir.GetFiles(searchPattern))
                {
                    //Console.WriteLine("File {0}", f.FullName);
                    files.Add(f);
                }
            }
            catch
            {
                Console.WriteLine("Directory {0}  \n could not be accessed!!!!", dir.FullName);
                throw new AccessViolationException();  // We alredy got an error trying to access dir so dont try to access it again
            }

            // process each directory
            // If I have been able to see the files in the directory I should also be able 
            // to look at its directories so I dont think I should place this in a try catch block
            //foreach (DirectoryInfo d in dir.GetDirectories())
            //{
            //    folders.Add(d);
            //    FullDirList(d, searchPattern);
            //}
            return files;
        }

        private void FindSteamappsPath()
        {
            string steamPath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SourceModInstallPath", String.Empty) as string;
            if (steamPath != null && !steamPath.Equals(String.Empty))
            {
                _steamappsPath = steamPath.Replace("sourcemods", "");
            }
            else
            {
                throw new Exception("No steamapps path found.");
            }
        }

        public void RetrieveGame(string gameName, NetworkManager clienta, string selectedComputer)
        {
            string remotePath = clienta.GetDirFromGameName(gameName);
            Console.WriteLine(remotePath);
            FtpManager.GetInstance().RetrieveGame(Path.Combine(_steamappsPath, "common"), remotePath, selectedComputer);
        }
    }


    public class PathnameSizePair
    {
        public string PathName { get; set; }
        public string Size { get; set; }
    }

    [System.Serializable]
    public class NameSizePair : IComparable
    {

        public string Name { get; set; }
        public string Size { get; set; }
        public int CompareTo(object obj)
        {
            if(obj != null && obj.GetType() == typeof(NameSizePair) )
            {
                var otherNameSizePair = obj as NameSizePair;
                return String.Compare(Name, otherNameSizePair.Name, StringComparison.Ordinal);
            }
            throw new InvalidCastException();
        }

        public override String ToString()
        {
            return "GamePair: " + Name + " - " + Size;
        }
    }
}
