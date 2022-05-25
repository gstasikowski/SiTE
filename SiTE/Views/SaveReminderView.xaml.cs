using System.Windows;

namespace SiTE.Views
{
    /// <summary>
    /// Interaction logic for SaveReminderView.xaml
    /// </summary>
    public partial class SaveReminderView : Window
    {
        public int dialogChoice = -1;

        public SaveReminderView()
        {
            InitializeComponent();
            Application.Current.MainWindow.IsEnabled = false;
        }

        private void SetDialogChoice(int choice)
        {
            dialogChoice = choice;
            Application.Current.MainWindow.IsEnabled = true;
            Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SetDialogChoice(0);
        }

        private void BtnProceed_Click(object sender, RoutedEventArgs e)
        {
            SetDialogChoice(1);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            SetDialogChoice(-1);
        }
    }
}
