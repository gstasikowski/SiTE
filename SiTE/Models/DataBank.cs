using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SiTE.Models
{
    public class DataBank
    {
        #region Variables
        public readonly string projectUrl = "https://github.com/gstasikowski/SiTE";
        
        readonly string defaultConfigPath = AppDomain.CurrentDomain.BaseDirectory + "Config.xml";
        readonly string defaultNotePath = AppDomain.CurrentDomain.BaseDirectory + "Notes\\";
        readonly string defaultDatabaseFile = "Journal.data";
        readonly string defaultPIndexFile = "Journal.pixd";
        readonly string defaultSIndexFile = "Journal.sidx";
        readonly string defaultLanguagePath = AppDomain.CurrentDomain.BaseDirectory + "Languages\\";
        readonly string encryptionExtention = ".aes";

        Dictionary<string, string> settings = new Dictionary<string, string>();
        List<string> languageList = new List<string>();
        ObservableCollection<NoteModel> noteList = new();
        #endregion Variables

        #region Properties
        public string DefaultConfigPath
        {
            get { return defaultConfigPath; }
        }

        public string DefaultNotePath
        {
            get { return defaultNotePath; }
        }

        public string DefaultDBPath
        {
            get { return defaultNotePath + defaultDatabaseFile; }
        }

        public string DefaultPIndexPath
        {
            get { return defaultNotePath + defaultPIndexFile; }
        }

        public string DefaultSIndexPath
        {
            get { return defaultNotePath + defaultSIndexFile; }
        }

        public string DefaultLanguagePath
        {
            get { return defaultLanguagePath; }
        }

        public string EncryptionExtention
        { 
            get { return encryptionExtention; } 
        }

        public ObservableCollection<NoteModel> NoteList
        {
            get { return noteList; }
            set { noteList = value; }
        }

        #endregion Properties

        #region Methods
        public void RestoreDefaultSettings()
        {            
            SetSetting("languageID", "en-US");
            SetSetting("autoSave", "true");
            SetSetting("autoSaveDelay", "5");
            SetSetting("encryption", "true");
            SetSetting("editorMode", "1");
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
                settings[key] = value;

                if (key == "password")
                    Logic.FileOperations.UpdateEncryption();
            }
            else
            { settings.Add(key, value); }
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

        public string GetNoteTitle(Guid noteID)
        {
            return NoteList.FirstOrDefault(n => n.ID == noteID).Title;
        }

        public int GetNoteIndex(string noteTitle)
        {
            NoteModel note = NoteList.FirstOrDefault(n => n.Title.Equals(noteTitle));
            return NoteList.IndexOf(note);
        }

        public int LanguageIndex(string languageCode)
        {
            return languageList.FindIndex(x => x.Contains(languageCode));
        }
        #endregion Methods
    }
}
