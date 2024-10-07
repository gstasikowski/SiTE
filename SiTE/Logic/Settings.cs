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
				SetSetting("languageID", Localizer.Instance.LanguageList[value]);
			}
		}

		public int SelectedTheme
		{
			get { return GetThemeIndex(); }
			set { ChangeTheme(value); }
		}

		public bool AutoSave
		{
			get { return GetSetting("autoSave") == "True"; }
			set { SetSetting("autoSave", value.ToString()); }
		}

		public int AutoSaveDelay
		{
			get { return HelperMethods.ParseIntSetting("autoSaveDelay"); }
			set { SetSetting("autoSaveDelay", value.ToString()); }
		}

		public bool EncryptDatabase
		{
			get { return GetSetting("encryptDatabase") == "True"; }
			set { SetSetting("encryptDatabase", value.ToString()); }
		}

		public string DatabasePassword // temp, gotta hash and salt
		{
			get { return GetSetting("databasePassword"); }
			set { SetSetting("databasePassword", value.ToString()); }
		}

		public int EditorMode
		{
			get { return HelperMethods.ParseIntSetting("editorMode"); }
			set { SetSetting("editorMode", value.ToString()); }
		}

		public void ApplyUserSettings()
		{
			Localizer.Instance.LoadLanguage(GetSetting("languageID"));

			int themeIndex = 0;
			int.TryParse(GetSetting("theme"), out themeIndex);
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
			Localizer.Instance.LoadLanguage(GetSetting("languageID"));
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