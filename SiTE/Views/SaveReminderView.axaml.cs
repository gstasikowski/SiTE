using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;

namespace SiTE.Views
{
    public partial class SaveReminderView : Window
    {
        public int dialogChoice = -1;

        public SaveReminderView()
        {
            InitializeComponent();
            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.IsEnabled = false;
        }

        private void SetDialogChoice(int choice)
        {
            dialogChoice = choice;
            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.IsEnabled = true;
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