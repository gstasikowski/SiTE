using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;

namespace SiTE.Views
{
    public partial class PasswordInputView : Window
    {
        public PasswordInputView()
        {
            InitializeComponent();
        }

        private void SubmitPassword(object sender, RoutedEventArgs e)
        {
            Logic.Refs.dataBank.UpdatePassword(txtPassword.Text, true);
            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.IsEnabled = true;
            // DialogResult = true;
            Close(true);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // if (DialogResult == null)
            // {
            //     Logic.Refs.dataBank.UpdatePassword("-1", true); 
            //     App.Current.Shutdown();
            // }
        }
    }
}