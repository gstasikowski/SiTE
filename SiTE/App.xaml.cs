using SiTE.Models;
using System.Windows;

namespace SiTE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow app = new MainWindow();
            ApplicationViewModel context = new ApplicationViewModel();
            Logic.Refs.viewControl = context;
            app.DataContext = context;
            app.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Logic.FileOperations.CheckForPlainDatabaseFiles();
        }
    }
}
