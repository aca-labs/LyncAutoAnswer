using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace LyncKioskTray
{
    public class Tray
    {
        public NotifyIcon TrayControl { get; private set; }

        public event EventHandler Shutdown = (sender, args) => { };

        private Settings _settingWindow;

        public Tray(NotifyIcon notifyIcon)
        {
            TrayControl = notifyIcon;
        }

        public void Show()
        {
            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "TrayIcon.ico");
            Debug.Assert(iconStream != null, "iconStream != null");
            TrayControl.Icon = new Icon(iconStream);
            TrayControl.Visible = true;

            TrayControl.DoubleClick += (sender, args) => ShowSettings();

            AddMenu();
        }
        
        private void AddMenu()
        {
            var menuItems = new List<MenuItem>();

            var settingsMenuItem = new MenuItem("Settings");
            settingsMenuItem.Click += (sender, args) => ShowSettings();
            menuItems.Add(settingsMenuItem);

            var exitMenuItem = new MenuItem("Exit");
            exitMenuItem.Click += (sender, args) => Shutdown(this, EventArgs.Empty);
            menuItems.Add(exitMenuItem);

            TrayControl.ContextMenu = new ContextMenu(menuItems.ToArray());
        }

        private void ShowSettings()
        {
            if (_settingWindow == null)
                    _settingWindow = new Settings();
                _settingWindow.Show();
                _settingWindow.Activate();
        }
    }
}
