using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace GameNetworkCopier
{

    class SteamOperations
    {
        private Dictionary<string, string> installedGames;
        private List<string> libraryPaths;
        public SteamOperations()
        {
            String steamappsPath = GetSteamappsPath();
            DirectoryInfo di = new DirectoryInfo(@steamappsPath);
            foreach (var file in FullDirList(di, "*.acf"))
            {
                string[] lines = File.ReadAllLines(@steamappsPath + "\\" + file);
                installedGames = GetSteamGameDetails(lines);
            }
            libraryPaths = GetLibraryPaths(File.ReadAllLines(@steamappsPath + "\\" + "libraryfolders.vdf"));

        }

       Dictionary<string, string> GetSteamGameDetails(string[] lines)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("\"name\"") || line.Contains("\"installdir\""))
                {
                    var aux = GetValuesFromLine(line);
                    result[aux[0]] = aux[1];
                }
            }
            return result;
        }

        List<string> GetLibraryPaths(string[] lines)
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

        private String GetSteamappsPath()
        {
            string steamPath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SourceModInstallPath", String.Empty) as string;
            if (steamPath != null && !steamPath.Equals(String.Empty))
            {
                return steamPath.Replace("sourcemods", "");
            }
            throw new Exception("No steamapps path found.");
        }

        public static void printList(List<string> l)
        {
            String res = "[ ";
            foreach (var el in l)
            {
                res += el + ", ";
            }
            Console.WriteLine(res + "]");
        }
        public static void printArray(object[] l)
        {
            String res = "[ ";
            foreach (var el in l)
            {
                res += el + ", ";
            }
            Console.WriteLine(res + "]");
        }
    }
}
