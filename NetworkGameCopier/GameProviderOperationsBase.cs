using System;
using System.Collections.Generic;
using NLog;

namespace NetworkGameCopier
{
    public abstract class GameProviderOperationsBase
    {
        protected Dictionary<string, PathnameSizePair> InstalledGames 
            = new Dictionary<string, PathnameSizePair>();

        public List<NameSizePair> GetLocalGamesNamesList()
        {
            List<NameSizePair> list = new List<NameSizePair>();
            foreach (KeyValuePair<string, PathnameSizePair> entry in InstalledGames)
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
            return InstalledGames[gameName].PathName;
        }

        public abstract void RetrieveGame(string gameName, NetworkManager clienta, string selectedComputer, AsyncPack asyncPack);

        public abstract NameSizePair[] GetRemoteGamesNamesList(NetworkManager client);
    }

    public class GameProviderSingleton
    {

        private static readonly GameProviderSingleton Instance = new GameProviderSingleton();

        public static GameProviderSingleton GetInstance()
        {
            return Instance;
        }

        private GameProviderOperationsBase _active;

        public GameProviderOperationsBase Active
        {
            get => _active;
            set
            {
                _active = value;
                LogManager.GetCurrentClassLogger().Warn("GameProvider is now " + value.GetType().Name);
            }
        }

        private GameProviderSingleton()
        {
            Active = SteamOperations.GetInstance();
        }
    }
}