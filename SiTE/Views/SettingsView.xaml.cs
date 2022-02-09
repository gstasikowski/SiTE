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
            cb_LanguageList.Items.Clear();

            foreach (string language in Logic.Refs.dataBank.LanguageList)
            { cb_LanguageList.Items.Add(language.Substring(0, language.IndexOf('[') - 1)); }
            
            cb_LanguageList.SelectedIndex = Logic.Refs.dataBank.LanguageIndex(Logic.Refs.dataBank.GetSetting("languageID"));
            SelectLanguage();

            // AutoSave
            chkb_AutoSaveEnable.IsChecked = System.Convert.ToBoolean(Logic.Refs.dataBank.GetSetting("autoSave"));
            ToggleAutoSave();

            // Encryption
            chkb_Encryption.IsChecked = System.Convert.ToBoolean(Logic.Refs.dataBank.GetSetting("encryption"));
            ToggleEncryption();
            ToggleSettingsStatus(false);
        }

        private void SelectLanguage()
        {
            string currentLanguage = Logic.Refs.dataBank.LanguageList[cb_LanguageList.SelectedIndex];
            int codePosition = currentLanguage.IndexOf('[') + 1;
            string cultureCode = currentLanguage.Substring(codePosition, currentLanguage.Length - (codePosition + 1));
            Logic.Refs.dataBank.SetSetting("languageID", cultureCode);
            Logic.Refs.localizationHandler.SwitchLanguage(cultureCode);
        }

        private void ToggleEncryption()
        {
            tb_EncryptionPassword.IsEnabled = (bool)chkb_Encryption.IsChecked;

            if (!tb_EncryptionPassword.IsEnabled)
                tb_EncryptionPassword.Text = "";
            else
                tb_EncryptionPassword.Text = Logic.Refs.dataBank.GetSetting("password"); // temporary for testing
            
            ToggleSettingsStatus(true);
        }

        private void ToggleAutoSave()
        {
            tb_AutoSaveDelay.IsEnabled = (bool)chkb_AutoSaveEnable.IsChecked;

            if (!tb_AutoSaveDelay.IsEnabled)
                tb_AutoSaveDelay.Text = "";
            else
                tb_AutoSaveDelay.Text = Logic.Refs.dataBank.GetSetting("autoSaveDelay");

            ToggleSettingsStatus(true);
        }

        private void ApplySettings()
        {
            SelectLanguage();
            Logic.Refs.dataBank.SetSetting("autoSave", chkb_AutoSaveEnable.IsChecked.ToString());
            
            if (int.TryParse(tb_AutoSaveDelay.Text, out _))
                Logic.Refs.dataBank.SetSetting("autoSaveDelay", tb_AutoSaveDelay.Text);
            else
                Logic.Refs.dataBank.SetSetting("autoSaveDelay", "5");

            Logic.Refs.dataBank.SetSetting("encryption", chkb_Encryption.IsChecked.ToString());
            Logic.Refs.dataBank.SetSetting("password", tb_EncryptionPassword.Text); // TODO prevent user from setting empty password if encryption is enabled

            Logic.Refs.fileOperations.SaveSettings();
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