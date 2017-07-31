using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using NLog;

namespace NetworkGameCopier
{
    internal class SteamOperations : GameProviderOperationsBase
    {
        private static readonly SteamOperations Instance = new SteamOperations();

        public List<string> LibraryPaths { get; }
        public string SteamappsPath { get; private set; }

        public static SteamOperations GetInstance()
        {
            return Instance;
        }

        private SteamOperations()
        {
            FindSteamappsPath();
            DirectoryInfo di = new DirectoryInfo(SteamappsPath);
            foreach (var file in FullDirList(di, "*.acf"))
            {
                string[] lines = File.ReadAllLines(SteamappsPath + "\\" + file);
                GetSteamGameDetails(lines);
            }
            LibraryPaths = GetLibraryPathsFromSteam(File.ReadAllLines(Path.Combine(SteamappsPath, "libraryfolders.vdf")));
            string[] files =
                Directory.GetFileSystemEntries(Path.Combine(SteamappsPath, "common"), "*",
                    SearchOption.TopDirectoryOnly);
            FtpManager.GetInstance().AddLinks(files);
        }

        public override void RetrieveGame(string gameName, NetworkManager clienta,
            string selectedComputer, AsyncPack asyncPack)
        {
            string remotePath = clienta.GetDirFromGameNameSteam(gameName);
            LogManager.GetCurrentClassLogger().Warn(remotePath);
            FtpManager.GetInstance()
                .RetrieveGame(SettingsManager.GetInstance().GetDefaultSteamLibrary(),
                    remotePath, selectedComputer, asyncPack);
        }

        public override NameSizePair[] GetRemoteGamesNamesList(NetworkManager client)
        {
            return client.GetGamesNamesListSteam().ToArray();
        }

        public List<string> GetLibraryPaths()
        {
            var libs = new List<string>(LibraryPaths) {Path.Combine(SteamappsPath, "common")};
            return libs;
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
                        InstalledGames[futureKey] =
                            new PathnameSizePair {PathName = pathname, Size = GetValuesFromLine(line)[1]};
                    break;
                }
            }
        }

        private List<string> GetLibraryPathsFromSteam(string[] lines)
        {
            List<string> paths = new List<string>();
            foreach (var line in lines)
            {
                if (line.Contains("\"1\""))
                {
                    var path = GetValuesFromLine(line)[1];
                    do
                    {
                        path = path.Replace("\\\\", "\\");
                    }
                    while (path.Contains("\\\\"));
                    paths.Add(path);
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

        private ArrayList FullDirList(DirectoryInfo dir, string searchPattern)
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
            string steamPath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam",
                "SourceModInstallPath", String.Empty) as string;
            if (steamPath != null && !steamPath.Equals(String.Empty))
            {
                SteamappsPath = steamPath.Replace("sourcemods", "");
            }
            else
            {
                throw new Exception("No steamapps path found.");
            }
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
                return string.Compare(Name, otherNameSizePair.Name, StringComparison.Ordinal);
            }
            throw new InvalidCastException();
        }

        public override String ToString()
        {
            return "GamePair: " + Name + " - " + Size;
        }
    }
}
