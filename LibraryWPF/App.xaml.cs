using System.Configuration;
using System.Data;
using System.Windows;

namespace LibraryWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.ApplyTheme("Light"); // Set default theme on startup
        }
    }

}
