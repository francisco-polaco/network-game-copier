using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkGameCopier
{
    class DownloadTaskQueue
    {
        private static readonly DownloadTaskQueue Instance = new DownloadTaskQueue();

        public static DownloadTaskQueue GetInstance()
        {
            return Instance;
        }

        private class TaskContainer
        {
            public GameProviderOperationsBase Provider { get; }
            public string GameName { get; }
            public NetworkManager Client { get; }
            public string SelectedComputer { get; }
            public AsyncPack AsyncPack { get; }

            public TaskContainer(GameProviderOperationsBase provider, string gameName, NetworkManager client, string selectedComputer, AsyncPack asyncPack)
            {
                Provider = provider;
                GameName = gameName;
                Client = client;
                SelectedComputer = selectedComputer;
                AsyncPack = asyncPack;
            }
        }

        private readonly ConcurrentQueue<TaskContainer> _queue = new ConcurrentQueue<TaskContainer>();
        private readonly Thread _workerThread;
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        private DownloadTaskQueue()
        {
            _workerThread = new Thread(() =>
            {
                while (true)
                {

                    if (_queue.Count == 0)
                    {
                        _autoResetEvent.WaitOne();
                    }
                    else
                    {
                        TaskContainer toRun;
                        if (_queue.TryDequeue(out toRun))
                        {
                            DoWork(toRun);
                            if (_queue.Count == 0)
                            {
                                // Work is over
                                var shutdownAfterDownloads = SettingsManager.GetInstance().GetShutdownAfterDownloads();
                                if (shutdownAfterDownloads != null && (bool)shutdownAfterDownloads)
                                {
                                    ShutdownComputer();
                                }
                            }
                        }
                    }
                }
            });
            _workerThread.Start();
        }

        public void QueueJob(GameProviderOperationsBase provider, string gameName, NetworkManager client,
            string selectedComputer, AsyncPack asyncPack)
        {
            _queue.Enqueue(new TaskContainer(provider, gameName, client, selectedComputer, asyncPack));
            _autoResetEvent.Set();
        }

        private void DoWork(TaskContainer container)
        {
            container.AsyncPack.Window.Dispatcher.Invoke(container.AsyncPack.ToExecute, new object[] { 0.0 });
            container.Provider.RetrieveGame(container.GameName, container.Client,
                container.SelectedComputer, container.AsyncPack);
        }

        private void ShutdownComputer()
        {
            var psi = new ProcessStartInfo("shutdown", "/s /t 60")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(psi);
        }

        public void ForceStop()
        {
            // Force Stop
            try
            {
                _workerThread.Abort();
            }
            catch
            {
                // ignored
            }
        }

    }

}
