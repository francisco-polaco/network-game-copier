using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace GameNetworkCopier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            setup();
            InitializeComponent();
        }

        private void setup()
        {
            String steamappsPath = getSteamappsPath();
            //ArrayList list = getRemainingLibrariesPath(steamappsPath);
            getInstalledGamesNames(steamappsPath);
        }

        private ArrayList getRemainingLibrariesPath(string steamappsPath)
        {
            //JsonConvert.DeserializeObject
            throw new NotImplementedException();
        }

        private ArrayList getInstalledGamesNames(string steamappsPath)
        {
            DirectoryInfo di = new DirectoryInfo(@steamappsPath);
            foreach (var file in FullDirList(di, "*.acf"))
            {
                string contents = File.ReadAllText(@steamappsPath + "\\" + file);
                //AppState astate = JsonConvert.DeserializeObject<AppState>(contents);
                //Console.WriteLine(astate.name);
            }
            return null;
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

        private String getSteamappsPath()
        {
            string steamPath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SourceModInstallPath", String.Empty) as string;
            if (steamPath != null && !steamPath.Equals(String.Empty))
            {
                return steamPath.Replace("sourcemods", "");
            }
            throw new Exception("No steamapps path found.");
        }

        public static void printList(ArrayList l)
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
