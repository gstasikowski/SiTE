using System.Collections.ObjectModel;
using Avalonia.Controls.ApplicationLifetimes;
using SiTE.Logic;

namespace SiTE.ViewModels
{
	public class SettingsViewModel : ViewModelBase
	{
		public ObservableCollection<string> LanguageList
		{
			get { return Localizer.Instance.LanguageList; }
		}

		public Settings AppSettings
		{
			get { return Settings.Instance; }
		}

		public void SaveSettings()
		{
			AppSettings.SaveSettings();
		}

		public void SwitchToEditorView()
		{
			AppSettings.LoadSettings();
			((ViewModels.MainWindowViewModel)((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.DataContext).ToggleActiveScreen();
		}
	}
}