using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;

namespace SiTE.Views
{
    public partial class SaveReminderView : Window
    {
        private int _dialogChoice = -1;

        public SaveReminderView()
        {
            InitializeComponent();
            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.IsEnabled = false;
            this.Closing += ClosePrompt;
        }

        private void SetDialogChoice(int choice)
        {
            _dialogChoice = choice;
            Close(_dialogChoice);
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

        private void ClosePrompt(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.IsEnabled = true;
        }
    }
}