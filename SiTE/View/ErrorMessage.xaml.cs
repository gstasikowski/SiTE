using System.Windows;

namespace SiTE.View
{
    /// <summary>
    /// Interaction logic for ErrorMessage.xaml
    /// </summary>
    public partial class ErrorMessage : Window
    {
        public ErrorMessage(string message)
        {
            InitializeComponent();
            lbl_errorContent.Content = message;
        }
    }
}
