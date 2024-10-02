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
			Models.DataBank.Instance.UpdatePassword(txtPassword.Text, true);
			// DialogResult = true;
			Close(true);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// if (DialogResult == null)
			// {
			//	 Models.DataBank.Instance.UpdatePassword("-1", true); 
			//	 App.Current.Shutdown();
			// }
		}
	}
}