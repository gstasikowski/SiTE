using System;
using System.Collections.Generic;

namespace SiTE.Models
{
    public class DataBank
    {
        #region Variables
        
        string defaultNotePath = AppDomain.CurrentDomain.BaseDirectory + "Notes\\";
        string defaultConfigPath = AppDomain.CurrentDomain.BaseDirectory;
        string defaultLanguagePath = AppDomain.CurrentDomain.BaseDirectory + "Languages\\";
        string tempFileExtension = ".site";
        Dictionary<string, string> settings = new Dictionary<string, string>();

        List<string> languageList = new List<string>();

        string noteCurrentOpen = string.Empty;
        string noteLastSaveTime = string.Empty;

        #endregion Variables

        #region Getters/setters

        public string DefaultNotePath
        {
            get { return defaultNotePath; }
        }

        public string DefaultConfigPath
        {
            get { return defaultConfigPath; }
        }

        public string DefaultLanguagePath
        {
            get { return defaultLanguagePath; }
        }

        public string TempFileExtension
        {
            get { return tempFileExtension; }
        }
        #endregion Getters/setters

        #region Methods

        public void RestoreDefaultSettings()
        {
            SetSetting("languageID", "en-US");
            SetSetting("encryption", "true");
            SetSetting("password", "abcde1234"); // for testing only! TODO hash and move into a secure place so it's not widely available when app is running
        }

        public Dictionary<string, string> GetAllSettings()
        {
            return settings;
        }

        public string GetSetting(string key)
        {
            return settings[key];
        }

        public void SetSetting(string key, string value)
        {
            if (settings.ContainsKey(key))
            {
                if (key == "password")
                    Logic.Refs.fileOperations.UpdateEncryptionPassword(GetSetting("password"), value);

                settings[key] = value; 
            }
            else
            { settings.Add(key, value); }
        }

        public string NoteCurrentOpen
        {
            get { return noteCurrentOpen; }
            set { noteCurrentOpen = value; }
        }

        public string NoteLastSaveTime
        {
            get { return noteLastSaveTime; }
            set { noteLastSaveTime = value; }
        }

        public void AddAvailableLanguage(string languageCode)
        {
            if (!languageList.Contains(languageCode))
            { languageList.Add(languageCode); }
        }

        public List<string> LanguageList
        {
            get { return languageList; }
        }

        public int LanguageIndex(string languageCode)
        {
            return languageList.FindIndex(x => x.Contains(languageCode));
        }

        #endregion Methods
    }
}
