using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Input;

namespace SiTE.Views
{
	public partial class SettingsView : UserControl
	{
		public SettingsView()
		{
			InitializeComponent();
		}

		#region Methods
		private void ToggleEncryption()
		{
			ToggleSettingsStatus(true);
		}

		private void ApplySettings()
		{
			((ViewModels.SettingsViewModel)this.DataContext).AppSettings.SaveSettings();
		}

	   private void CloseSettingsView()
		{
			((ViewModels.SettingsViewModel)this.DataContext).AppSettings.LoadSettings();
			// Logic.Refs.viewControl.CurrentPageViewModel = Logic.Refs.viewControl.PageViewModels[0]; // TODO: Use methods in MainWindowViewModel
		}

		private void ToggleSettingsStatus(bool modified)
		{
			// if (Logic.Refs.viewControl == null)
			//	 return;

			// Logic.Refs.viewControl.SettingsModified = modified;
		}
		#endregion Methods

		#region UI Events
		private void BtnApply_Click(object sender, RoutedEventArgs e)
		{
			ApplySettings();
		}

		private void BtnBack_Click(object sender, RoutedEventArgs e)
		{
			CloseSettingsView();
		}
		#endregion UI Events
	}
}