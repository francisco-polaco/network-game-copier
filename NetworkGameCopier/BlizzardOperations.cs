using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetworkGameCopier
{
    public class BlizzardOperations : GameProviderOperationsBase
    {

        private static readonly BlizzardOperations Instance = new BlizzardOperations();

        public static BlizzardOperations GetInstance()
        {
            return Instance;
        }

        private BlizzardOperations()
        {
            List<string> list = GetBlizzardGameDetails();
            FtpManager.GetInstance().AddLinks(list.ToArray());
        }

        private List<string> GetBlizzardGameDetails()
        {
            List<string> gamesPathnames = new List<string>();

            foreach (var gameName in new[]{ "Overwatch" , "Heroes of the Storm" ,
                "Hearthstone", "World of Warcraft" , "Diablo 3" , "Starcraft 2"})
            {
                try
                {
                    string pathname = RegistryManager.FindInstallLocationByName(gameName);
                    gamesPathnames.Add(pathname);
                    string[] directoryPathSplitted = pathname.Split('\\');
                    InstalledGames[gameName] =
                        new PathnameSizePair
                        {
                            PathName = directoryPathSplitted[directoryPathSplitted.Length - 1],
                            Size = Utilities.DirSize(new DirectoryInfo(pathname)).ToString()
                        };
                }
                catch (NoKeyFoundException)
                {
                }
            }
            return gamesPathnames;
        }


        public override void RetrieveGame(string gameName, NetworkManager clienta, string selectedComputer, AsyncPack asyncPack)
        {
            string remotePath = clienta.GetDirFromGameNameSteam(gameName);
            Console.WriteLine(remotePath);
            //FtpManager.GetInstance()
            //  .RetrieveGame(Path.Combine(_steamappsPath, "common"), remotePath, selectedComputer, asyncPack);
            FtpManager.GetInstance().RetrieveGame("C:\\teste", remotePath, selectedComputer, asyncPack);
        }

        public override NameSizePair[] GetRemoteGamesNamesList(NetworkManager client)
        {
            return client.GetGamesNamesListBlizzard().ToArray();
        }
    }
}