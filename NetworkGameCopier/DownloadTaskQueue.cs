using System;
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

        private readonly Queue<Thread> _queue = new Queue<Thread>();
        private readonly Thread _workerThread;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        private DownloadTaskQueue()
        {
            _workerThread = new Thread(() =>
            {
                while (true)
                {
                    if (_queue.Count == 0)
                        _manualResetEvent.WaitOne();
                    else
                    {
                        Monitor.Enter(_queue);
                        Thread toRun = _queue.Dequeue();
                        Monitor.Exit(_queue);
                        toRun.Start();
                        toRun.Join();
                        if (_queue.Count == 0)
                        {
                            // Work is over
                            var shutdownAfterDownloads = SettingsManager.GetInstance().GetShutdownAfterDownloads();
                            if (shutdownAfterDownloads != null && (bool) shutdownAfterDownloads)
                            {
                                ShutdownComputer();
                            }
                        }
                    }
                }
            });
            _workerThread.Start();
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

        public void QueueJob(GameProviderOperationsBase provider, string gameName, NetworkManager client,
            string selectedComputer, AsyncPack asyncPack)
        {
            Thread toQueue = new Thread(() =>
            {
                asyncPack.Window.Dispatcher.Invoke(asyncPack.ToExecute, new object[]{0.0});
                provider.RetrieveGame(gameName, client, selectedComputer, asyncPack);
            });
            Monitor.Enter(_queue);
            _queue.Enqueue(toQueue);
            Monitor.Exit(_queue);
            _manualResetEvent.Set();
        }

        //public void SoftStop()
        //{
        //    _queue.Clear();
        //}

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
