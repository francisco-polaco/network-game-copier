using System.Collections.Generic;

namespace NetworkGameCopier
{
    public abstract class GameProviderOperationsBase
    {
        protected Dictionary<string, PathnameSizePair> InstalledGames 
            = new Dictionary<string, PathnameSizePair>();

        public List<NameSizePair> GetGameNamesList()
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

        public abstract void ReadyFtpServer();

        public abstract void RetrieveGame(string gameName, NetworkManager clienta, string selectedComputer, AsyncPack asyncPack);

    }
}