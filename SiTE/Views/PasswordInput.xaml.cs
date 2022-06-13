using System.Windows;

namespace SiTE.Views
{
    /// <summary>
    /// Interaction logic for PasswordInput.xaml
    /// </summary>
    public partial class PasswordInput : Window
    {
        public PasswordInput()
        {
            InitializeComponent();
        }

        private void SubmitPassword(object sender, RoutedEventArgs e)
        {
            Logic.Refs.dataBank.UpdatePassword(txtPassword.Password, true);
            Application.Current.MainWindow.IsEnabled = true;
            DialogResult = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult == null)
            {
                Logic.Refs.dataBank.UpdatePassword("-1", true); 
                App.Current.Shutdown();
            }
        }
    }
}
