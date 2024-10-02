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
			LoadSettings();
		}

		#region Methods
		private void LoadSettings()
		{
			// Language
			cboLanguageList.Items.Clear();

			foreach (string language in Models.DataBank.Instance.LanguageList) // TODO: replace with binding
			{
				cboLanguageList.Items.Add(language.Substring(0, language.IndexOf('(') - 1));
			}
			
			cboLanguageList.SelectedIndex = Models.DataBank.Instance.LanguageIndex(Models.DataBank.Instance.GetSetting("languageID"));
			SelectLanguage();

			// AutoSave
			chkAutoSaveEnable.IsChecked = System.Convert.ToBoolean(Models.DataBank.Instance.GetSetting("autoSave"));
			ToggleAutoSave();

			// Encryption
			chkEncryption.IsChecked = System.Convert.ToBoolean(Models.DataBank.Instance.GetSetting("encryption"));
			ToggleEncryption();
			chkPassword.IsChecked = (Models.DataBank.Instance.UserPassword != string.Empty);
			TogglePasswordProtection();
			ToggleSettingsStatus(false);
		}

		private void SelectLanguage()
		{
			string currentLanguage = Models.DataBank.Instance.LanguageList[cboLanguageList.SelectedIndex];
			int codePosition = currentLanguage.IndexOf('[') + 1;
			string cultureCode = currentLanguage.Substring(codePosition, currentLanguage.Length - (codePosition + 1));
			Models.DataBank.Instance.SetSetting("languageID", cultureCode);
			// Logic.Refs.localizationHandler.SwitchLanguage(cultureCode);
		}

		private void ToggleEncryption()
		{
			ToggleSettingsStatus(true);
		}

		private void TogglePasswordProtection()
		{
			txtEncryptionPassword.IsEnabled = (bool)chkPassword.IsChecked;
			
			if (!(bool)chkPassword.IsChecked)
			{ Models.DataBank.Instance.UpdatePassword(string.Empty, false); }

			txtEncryptionPassword.Text = Models.DataBank.Instance.UserPassword;

			ToggleSettingsStatus(true);
		}

		private void ToggleAutoSave()
		{
			txtAutoSaveDelay.IsEnabled = (bool)chkAutoSaveEnable.IsChecked;

			if (!txtAutoSaveDelay.IsEnabled)
				txtAutoSaveDelay.Text = "";
			else
				txtAutoSaveDelay.Text = Models.DataBank.Instance.GetSetting("autoSaveDelay");

			ToggleSettingsStatus(true);
		}

		private void ApplySettings()
		{
			SelectLanguage();
			Models.DataBank.Instance.SetSetting("autoSave", chkAutoSaveEnable.IsChecked.ToString());
			
			if (int.TryParse(txtAutoSaveDelay.Text, out _))
				Models.DataBank.Instance.SetSetting("autoSaveDelay", txtAutoSaveDelay.Text);
			else
				Models.DataBank.Instance.SetSetting("autoSaveDelay", "5");

			Models.DataBank.Instance.SetSetting("encryption", chkEncryption.IsChecked.ToString());

			if (txtEncryptionPassword.Text == string.Empty)
			{
				chkPassword.IsChecked = false;
				TogglePasswordProtection();
			}

			Models.DataBank.Instance.UpdatePassword(txtEncryptionPassword.Text, false);

			Logic.FileOperations.SaveSettings();
			ToggleSettingsStatus(false);
		}

	   private void CloseSettingsView()
		{
			LoadSettings();
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
		private void CBLanguageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ToggleSettingsStatus(true);
		}

		private void ChkAutoSaveEnable_Toggled(object sender, RoutedEventArgs e)
		{
			ToggleAutoSave();
		}

		private void TBAutoSaveDelay_TextChanged(object sender, TextInputEventArgs e)
		{
			ToggleSettingsStatus(true);
		}

		private void ChkEncryption_Toggled(object sender, RoutedEventArgs e)
		{
			ToggleEncryption();
		}

		private void ChkPasswordProtection_Toggled(object sender, RoutedEventArgs e)
		{
			TogglePasswordProtection();
		}

		private void TBEncryptionPassword_TextChanged(object sender, TextInputEventArgs e)
		{
			ToggleSettingsStatus(true);
		}

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