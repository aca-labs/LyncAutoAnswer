using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;

namespace LyncKioskTray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);

        private Settings _settingWindow;
        private LyncWatcher _lyncWatcher;
        private NotifyIcon _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            AllowOneInstance();
            base.OnStartup(e);
            InitLogging();
            NeverCrash();
            StartTray();
            StartWatchingLync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _trayIcon.Visible = false;
        }

        private static void AllowOneInstance()
        {
            // Get Reference to the current Process
            var me = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(me.ProcessName).Length > 1)
            {
                Current.Shutdown();
            }
        }

        private void StartTray()
        {
            //this.WindowState = WindowState.;
            _trayIcon = new NotifyIcon();
            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "TrayIcon.ico");
            Debug.Assert(iconStream != null, "iconStream != null");
            _trayIcon.Icon = new Icon(iconStream);
            _trayIcon.Visible = true;

            _trayIcon.Click += (sender, args) =>
                {
                    if(_settingWindow == null)
                        _settingWindow = new Settings();
                    _settingWindow.Show();
                    _settingWindow.Activate();
                };

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        public void InitLogging()
        {
            var logConfig = new FileInfo("Logging.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(logConfig);
            Log.Info("Lync Kiosk Starting");
        }

        private void StartWatchingLync()
        {
            _lyncWatcher = new LyncWatcher();
            
            _lyncWatcher.Start(client =>
                {
                    var answer = new LyncAnswer(client);
                    answer.Start();
                });
        }

        private void NeverCrash()
        {
            DispatcherUnhandledException += (sender, args) => Log.Error("Dispatcher unhandled exception", args.Exception);
        }
    }
}
