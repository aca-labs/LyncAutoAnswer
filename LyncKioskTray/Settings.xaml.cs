using System.Diagnostics;
using System.Windows;

namespace LyncKioskTray
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();

            //Hide the window, don't destroy it. This will let us show it again later.
            Closing += (sender, args) =>
                {
                    args.Cancel = true;
                    Visibility = Visibility.Hidden;

                    //Persist the settings to disk
                    Properties.Settings.Default.Save();
                };
        }

        private void Hyperlink_OnRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
