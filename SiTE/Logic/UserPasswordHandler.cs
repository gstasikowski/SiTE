using Avalonia.Controls.ApplicationLifetimes;
using System.Threading;
using System.Threading.Tasks;

namespace SiTE.Logic
{
	public class UserPasswordHandler // TODO: rewrite
	{
		public bool canUnlockDatabase = false;

		public UserPasswordHandler()
		{
			OpenPasswordDialog();
		}

		public async Task OpenPasswordDialog()
		{
			Views.PasswordInputView passwordWindow = new Views.PasswordInputView();
			// passwordWindow.ShowDialog(ApplicationLifetime.MainWindow); // TODO: ShowDialog needs an owner window for the dialog
			canUnlockDatabase = await passwordWindow.ShowDialog<bool>(((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow);
		}
	}
}
