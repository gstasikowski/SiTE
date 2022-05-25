using SiTE.Interfaces;
using System.Windows;

namespace SiTE.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsView : IPageViewModel
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

            foreach (string language in Logic.Refs.dataBank.LanguageList)
            { cboLanguageList.Items.Add(language.Substring(0, language.IndexOf('[') - 1)); }
            
            cboLanguageList.SelectedIndex = Logic.Refs.dataBank.LanguageIndex(Logic.Refs.dataBank.GetSetting("languageID"));
            SelectLanguage();

            // AutoSave
            chkAutoSaveEnable.IsChecked = System.Convert.ToBoolean(Logic.Refs.dataBank.GetSetting("autoSave"));
            ToggleAutoSave();

            // Encryption
            chkEncryption.IsChecked = System.Convert.ToBoolean(Logic.Refs.dataBank.GetSetting("encryption"));
            ToggleEncryption();
            ToggleSettingsStatus(false);
        }

        private void SelectLanguage()
        {
            string currentLanguage = Logic.Refs.dataBank.LanguageList[cboLanguageList.SelectedIndex];
            int codePosition = currentLanguage.IndexOf('[') + 1;
            string cultureCode = currentLanguage.Substring(codePosition, currentLanguage.Length - (codePosition + 1));
            Logic.Refs.dataBank.SetSetting("languageID", cultureCode);
            Logic.Refs.localizationHandler.SwitchLanguage(cultureCode);
        }

        private void ToggleEncryption()
        {
            txtEncryptionPassword.IsEnabled = (bool)chkEncryption.IsChecked;

            if (!txtEncryptionPassword.IsEnabled)
                txtEncryptionPassword.Text = "";
            else
                txtEncryptionPassword.Text = Logic.Refs.dataBank.GetSetting("password"); // temporary for testing
            
            ToggleSettingsStatus(true);
        }

        private void ToggleAutoSave()
        {
            txtAutoSaveDelay.IsEnabled = (bool)chkAutoSaveEnable.IsChecked;

            if (!txtAutoSaveDelay.IsEnabled)
                txtAutoSaveDelay.Text = "";
            else
                txtAutoSaveDelay.Text = Logic.Refs.dataBank.GetSetting("autoSaveDelay");

            ToggleSettingsStatus(true);
        }

        private void ApplySettings()
        {
            SelectLanguage();
            Logic.Refs.dataBank.SetSetting("autoSave", chkAutoSaveEnable.IsChecked.ToString());
            
            if (int.TryParse(txtAutoSaveDelay.Text, out _))
                Logic.Refs.dataBank.SetSetting("autoSaveDelay", txtAutoSaveDelay.Text);
            else
                Logic.Refs.dataBank.SetSetting("autoSaveDelay", "5");

            Logic.Refs.dataBank.SetSetting("encryption", chkEncryption.IsChecked.ToString());

            if (txtEncryptionPassword.Text == string.Empty)
            {
                chkEncryption.IsChecked = false;
                ToggleEncryption();
            }

            Logic.Refs.dataBank.SetSetting("password", txtEncryptionPassword.Text);

            Logic.FileOperations.SaveSettings();
            ToggleSettingsStatus(false);
        }

       private void CloseSettingsView()
        {
            LoadSettings();
            Logic.Refs.viewControl.CurrentPageViewModel = Logic.Refs.viewControl.PageViewModels[0]; // switch to binding
        }

        private void ToggleSettingsStatus(bool modified)
        {
            if (Logic.Refs.viewControl == null)
                return;

            Logic.Refs.viewControl.SettingsModified = modified;
        }
        #endregion Methods

        #region UI Events

        private void CBLanguageList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ToggleSettingsStatus(true);
        }

        private void ChkbAutoSaveEnable_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleAutoSave();
        }

        private void TBAutoSaveDelay_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ToggleSettingsStatus(true);
        }

        private void ChkbEncryption_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleEncryption();
        }

        private void TBEncryptionPassword_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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