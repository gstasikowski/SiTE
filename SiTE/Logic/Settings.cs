using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using SiTE.Models;

namespace SiTE.Logic
{
	public class Settings : INotifyPropertyChanged
	{
		private Core CoreApp
		{
			get { return Core.Instance; }
		}

		public static Settings Instance { get; set; } = new Settings();
		public event PropertyChangedEventHandler PropertyChanged;

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

		public void ApplyUserSettings()
		{
			Localizer.Instance.LoadLanguage(GetSetting("LanguageID"));

			int themeIndex = 0;
			int.TryParse(GetSetting("Theme"), out themeIndex);
			SelectedTheme = themeIndex;
		}

		public void SaveSettings()
		{
			CoreApp.fileOperations.SaveSettings();
		}

		public void LoadSettings()
		{
			CoreApp.fileOperations.LoadSettings();
		}

		public void RestoreDefaultSettings()
		{
			CoreApp.fileOperations.LoadDefaultSettings(CoreApp.dataBank.DefaultConfigFile);
			Localizer.Instance.LoadLanguage(GetSetting("LanguageID"));
			OnPropertyChanged("AutoSave");
			OnPropertyChanged("AutoSaveDelay");
			OnPropertyChanged("EncryptDatabase");
			OnPropertyChanged("DatabasePassword");
		}

		public string GetSetting(string settingID)
		{
			return CoreApp.dataBank.GetSetting(settingID);
		}

		public void SetSetting(string settingID, string value)
		{
			CoreApp.dataBank.SetSetting(settingID, value);
		}

		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

			SetSetting("theme", index.ToString());
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