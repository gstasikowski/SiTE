using System.Windows;

namespace SiTE.Views
{
    /// <summary>
    /// Interaction logic for ErrorMessage.xaml
    /// </summary>
    public partial class ErrorMessage : Window
    {
        public ErrorMessage(string message, string errorDetails)
        {
            InitializeComponent();
            Application.Current.MainWindow.IsEnabled = false;
            lblErrorContent.Content = message;
            lblErrorDetails.Content = errorDetails;

            expErrorDetails.Visibility = (errorDetails == string.Empty) ? Visibility.Hidden : Visibility.Visible;
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.MainWindow.IsEnabled = true;
        }
    }
}
