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
            Close();
        }
    }
}
