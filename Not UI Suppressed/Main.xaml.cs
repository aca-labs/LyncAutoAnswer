using System;
using System.IO;
using System.Windows;
using Microsoft.Lync.Model;

namespace SuperSimpleLyncKiosk
{
    public partial class Main : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Main()
        {
            InitializeComponent();
            InitLogging();

            this.WindowState = System.Windows.WindowState.Maximized;

            var lync = LyncClient.GetClient();

            lync.ConversationManager.AutoAnswerIncomingVideoCalls();

            Log.Debug("Kiosk Started");
        }

        public void InitLogging()
        {
            var logConfig = new FileInfo("Logging.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(logConfig);
            Log.Info("Lync Kiosk Starting");
        }
    }
}
