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