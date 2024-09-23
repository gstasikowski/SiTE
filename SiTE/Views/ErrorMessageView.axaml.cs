using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace SiTE.Views
{
    public partial class ErrorMessageView : Window
    {
        public ErrorMessageView(string message, string errorDetails)
        {
            InitializeComponent();
            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.IsEnabled = false;
            lblErrorContent.Text = message;
            lblErrorDetails.Content = errorDetails;

            expErrorDetails.IsVisible = (errorDetails == string.Empty) ? false : true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // App.Current.MainWindow.IsEnabled = true; // TODO: replace with a proper call for Avalonia
        }
    }
}