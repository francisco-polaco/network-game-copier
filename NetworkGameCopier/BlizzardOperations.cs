using System;
using GameNetworkCopier;

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
        }

        private void GetBlizzardGameDetails()
        {
            try
            {
                InstalledGames["Overwatch"] =
                    new PathnameSizePair {PathName = RegistryManager.FindInstallLocationByName("Overwatch"), Size = "0"};
            }
            catch (NoKeyFoundException e)
            {
                Console.WriteLine(e);
            }
            try
            {
                InstalledGames["Heroes of the Storm"] =
                        new PathnameSizePair { PathName = RegistryManager.FindInstallLocationByName("Heroes of the Storm")
                            , Size = "0" };
            }
            catch (NoKeyFoundException e)
            {
                Console.WriteLine(e);
            }
            try
            {
                InstalledGames["Hearthstone"] =
                    new PathnameSizePair
                    {
                        PathName = RegistryManager.FindInstallLocationByName("Hearthstone"),
                        Size = "0"
                    };
            }
            catch (NoKeyFoundException e)
            {
                Console.WriteLine(e);
            }
            try
            {
                InstalledGames["World of Warcraft"] = new
                    PathnameSizePair
                    {
                        PathName = RegistryManager.FindInstallLocationByName("World of Warcraft") 
                        , Size = "0"

                };
            }
            catch (NoKeyFoundException e)
            {
                Console.WriteLine(e);
            }
            try
            {
                InstalledGames["Diablo 3"] =
                    new PathnameSizePair
                    {
                        PathName = RegistryManager.FindInstallLocationByName("Diablo 3")
                        , Size = "0"
                    };
            }
            catch (NoKeyFoundException e)
            {
                Console.WriteLine(e);
            }
            try
            {
                InstalledGames["Starcraft 2"] =
                    new PathnameSizePair
                    {
                        PathName = RegistryManager.FindInstallLocationByName("Starcraft 2")
                        , Size = "0"
                    };
            }
            catch (NoKeyFoundException e)
            {
                Console.WriteLine(e);
            }
            PrintHelper.PrintList(GetGameNamesList());
        }

        public override void ReadyFtpServer()
        {
            throw new System.NotImplementedException();
        }

        public override void RetrieveGame(string gameName, NetworkManager clienta, string selectedComputer, AsyncPack asyncPack)
        {
            throw new System.NotImplementedException();
        }
    }
}