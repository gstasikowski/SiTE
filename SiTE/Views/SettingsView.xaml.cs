﻿using SiTE.Interfaces;
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

            // Encryption
            chkb_Encryption.IsChecked = System.Convert.ToBoolean(Logic.Refs.dataBank.GetSetting("encryption"));
            ToggleEncryption();
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
        }

        private void ApplySettings()
        {
            SelectLanguage();
            Logic.Refs.dataBank.SetSetting("encryption", chkb_Encryption.IsChecked.ToString());
            Logic.Refs.dataBank.SetSetting("password", tb_EncryptionPassword.Text); // TODO prevent user from setting empty password if encryption is enabled

            Logic.Refs.fileOperations.SaveSettings();
        }

       private void CloseSettingsView()
        {
            Logic.Refs.viewControl.CurrentPageViewModel = Logic.Refs.viewControl.PageViewModels[0]; // switch to binding
        }

        #endregion Methods

        #region UI Events

        private void ChkbEncryption_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleEncryption();
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