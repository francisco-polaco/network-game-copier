﻿using System;
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
            GetBlizzardGameDetails();
            List<string> list = new List<string>();
            foreach (var installedGamesValue in InstalledGames.Values)
            {
                list.Add(installedGamesValue.PathName);
            }
            FtpManager.GetInstance().AddLinks(list.ToArray());
        }

        private void GetBlizzardGameDetails()
        {
            foreach (var gameName in new[]{ "Overwatch" , "Heroes of the Storm" ,
                "Hearthstone", "World of Warcraft" , "Diablo 3" , "Starcraft 2"})
            {
                try
                {
                    string pathname = RegistryManager.FindInstallLocationByName(gameName);
                    InstalledGames[gameName] =
                        new PathnameSizePair
                        {
                            PathName = pathname,
                            Size = Utilities.DirSize(new DirectoryInfo(pathname)).ToString()
                        };
                }
                catch (NoKeyFoundException)
                {
                }
            }
        }


        public override void RetrieveGame(string gameName, NetworkManager clienta, string selectedComputer, AsyncPack asyncPack)
        {
            throw new System.NotImplementedException();
        }
    }
}