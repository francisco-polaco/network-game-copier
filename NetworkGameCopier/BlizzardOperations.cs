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
           // GetBlizzardGameDetails();
        }

        private void GetBlizzardGameDetails()
        {
            try
            {
                InstalledGames["ow"] =
                    new PathnameSizePair {PathName = RegistryManager.FindInstallLocationByName("Overwatch"), Size = "0MB"};
            }
            catch (NoKeyFoundException e)
            {
                Console.WriteLine(e);
            }
            //try
            //{
            //    InstalledGames["heroes"] = 
            //            new PathnameSizePair{PathName = RegistryManager.FindInstallLocationByName("Heroes of the Storm")};
            //}
            //catch (NoKeyFoundException e)
            //{
            //    Console.WriteLine(e);
            //}
            //try
            //{
            //    InstalledGames["hs"] =
            //        new PathnameSizePair { PathName = RegistryManager.FindInstallLocationByName("Hearthstone") };
            //}
            //catch (NoKeyFoundException e)
            //{
            //    Console.WriteLine(e);
            //}
            //try
            //{
            //    InstalledGames["wow"] = new
            //        PathnameSizePair
            //        { PathName = RegistryManager.FindInstallLocationByName("World of Warcraft") };
            //}
            //catch (NoKeyFoundException e)
            //{
            //    Console.WriteLine(e);
            //}
            //try
            //{
            //    InstalledGames["d3"] =
            //        new PathnameSizePair { PathName = RegistryManager.FindInstallLocationByName("Diablo 3") };
            //}
            //catch (NoKeyFoundException e)
            //{
            //    Console.WriteLine(e);
            //}
            //try
            //{
            //    InstalledGames["sc2"] =
            //        new PathnameSizePair { PathName = RegistryManager.FindInstallLocationByName("Starcraft 2") };
            //}
            //catch (NoKeyFoundException e)
            //{
            //    Console.WriteLine(e);
            //}
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