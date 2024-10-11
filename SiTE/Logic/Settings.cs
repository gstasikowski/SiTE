using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using SiTE.Models;

namespace SiTE.Logic
{
	public class Settings : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private Core CoreApp
		{
			get { return Core.Instance; }
		}

		private bool _settingsModified = false;

		public static Settings Instance { get; set; } = new Settings();

		public int SelectedLanguage
		{
			get { return Localizer.Instance.SelectedLanguageIndex(); }
			set
			{
				Localizer.Instance.LoadLanguage(Localizer.Instance.LanguageList[value]);
				SetSetting("LanguageID", Localizer.Instance.LanguageList[value]);
			}
		}

		public int SelectedTheme
		{
			get { return GetThemeIndex(); }
			set { ChangeTheme(value); }
		}

		public bool AutoSave
		{
			get { return GetSetting("AutoSave") == "True"; }
			set { SetSetting("AutoSave", value.ToString()); }
		}

		public int AutoSaveDelay
		{
			get { return HelperMethods.ParseIntSetting("AutoSaveDelay"); }
			set { SetSetting("AutoSaveDelay", value.ToString()); }
		}

		public bool EncryptDatabase
		{
			get { return GetSetting("EncryptDatabase") == "True"; }
			set { SetSetting("EncryptDatabase", value.ToString()); }
		}

		public string DatabasePassword // temp, gotta hash and salt
		{
			get { return GetSetting("DatabasePassword"); }
			set { SetSetting("DatabasePassword", value.ToString()); }
		}

		public int EditorMode
		{
			get { return HelperMethods.ParseIntSetting("EditorMode"); }
			set { SetSetting("EditorMode", value.ToString()); }
		}

		public bool SettingsModified
		{
			get { return _settingsModified; }
			private set
			{
				_settingsModified = value;
				OnPropertyChanged();
			}
		}

		public void ApplyUserSettings()
		{
			Localizer.Instance.LoadLanguage(GetSetting("LanguageID"));
			ApplyLoadedTheme();
			
			SettingsModified = false;
		}

		public void SaveSettings()
		{
			CoreApp.fileOperations.SaveSettings();
			SettingsModified = false;
		}

		public void LoadSettings()
		{
			CoreApp.fileOperations.LoadSettings();
			ApplyUserSettings();
		}

		public void RestoreDefaultSettings()
		{
			CoreApp.fileOperations.LoadDefaultSettings(CoreApp.dataBank.DefaultConfigFile);
			Localizer.Instance.LoadLanguage(GetSetting("LanguageID"));
			OnPropertyChanged("SelectedLanguage");
			ApplyLoadedTheme();
			
			foreach (var setting in CoreApp.dataBank.GetAllSettings())
			{
				OnPropertyChanged(setting.Key);
			}
		}

		public string GetSetting(string settingID)
		{
			return CoreApp.dataBank.GetSetting(settingID);
		}

		public void SetSetting(string settingID, string value)
		{
			CoreApp.dataBank.SetSetting(settingID, value);
			OnPropertyChanged(settingID);
			SettingsModified = true;
		}

		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void ApplyLoadedTheme()
		{
			int themeIndex = 0;
			int.TryParse(GetSetting("Theme"), out themeIndex);
			SelectedTheme = themeIndex;
		}

		private void ChangeTheme(int index)
		{
			switch (index)
			{
				case 1:
					Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
					break;

				case 2:
					Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
					break;

				default:
					Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;
					break;
			}

			SetSetting("Theme", index.ToString());
		}

		private int GetThemeIndex()
		{
			switch (Avalonia.Application.Current.RequestedThemeVariant.ToString())
			{
				case "Light":
					return 1;

				case "Dark":
					return 2;

				default:
					return 0;
			}
		}
	}
}