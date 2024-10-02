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
		}

		private void SetDialogChoice(int choice)
		{
			_dialogChoice = choice;
			Close(_dialogChoice);
		}

		private void ConfirmNoteSave(object sender, RoutedEventArgs e)
		{
			SetDialogChoice(0);
		}

		private void DenyNoteSave(object sender, RoutedEventArgs e)
		{
			SetDialogChoice(1);
		}

		private void CancelNoteSwitch(object sender, RoutedEventArgs e)
		{
			SetDialogChoice(-1);
		}
	}
}