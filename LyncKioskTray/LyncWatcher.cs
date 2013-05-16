using System;
using System.Management;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Lync.Model;

namespace LyncKioskTray
{
    /// <summary>
    ///     The Lync client may not be running when the app starts, or it may be restarted.
    ///     This class starts the Lync logic if necessary, and restarts it if the app restarts.
    /// </summary>
    public class LyncWatcher
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
                    MethodBase.GetCurrentMethod().DeclaringType);

        private Action<LyncClient> _lyncStarted = client => { };
        private readonly Control _invokerControl = new Control();

        public LyncWatcher()
        {
            _invokerControl.CreateControl();
        }

        public void Start(Action<LyncClient> lyncStarted)
        {
            _lyncStarted = lyncStarted;

            WatchForProcessStart("lync.exe", LyncProcessStarted);

            var client = TryToGetLyncClient();
            if (client == null)
                Log.Info("Lync client must not be running. Waiting for the process to start.");
            else
                _lyncStarted(client);
            
        }

        private void WatchForProcessStart(string processName, Action a)
        {
            string queryString =
                "SELECT TargetInstance" +
                "  FROM __InstanceCreationEvent " +
                "WITHIN  10 " +
                " WHERE TargetInstance ISA 'Win32_Process' " +
                "   AND TargetInstance.Name = '" + processName + "'";

            // The dot in the scope means use the current machine
            const string scope = @"\\.\root\CIMV2";

            // Create a watcher and listen for events
            var watcher = new ManagementEventWatcher(scope, queryString);
            watcher.EventArrived += (sender, args) =>
                {
                    //Give Lync some time to start
                    //I tried getting fancy with checking the Lync state, but we end up getting into
                    //threading hell. It was just easier to give Lync some time after the process starts.
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    _invokerControl.Invoke(a);
                };
            watcher.Start();
        }

        private static LyncClient TryToGetLyncClient()
        {
            try
            {
                return LyncClient.GetClient();
            }
            catch (ClientNotFoundException)
            {
                return null;
            }
        }

        private void LyncProcessStarted()
        {
            var lyncClient = TryToGetLyncClient();
            if (lyncClient == null)
            {
                Log.Info("Lync.exe appears to have started, but a LyncClient could not be attached");
            }
            else
            {
                _lyncStarted(lyncClient);
            }
        }
    }
}
