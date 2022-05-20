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
        
        string defaultNotePath = AppDomain.CurrentDomain.BaseDirectory + "Notes\\";
        string defaultDatabasePath = AppDomain.CurrentDomain.BaseDirectory + "Notes\\Journal.data";
        string defaultPIndexPath = AppDomain.CurrentDomain.BaseDirectory + "Notes\\Journal.pixd";
        string defaultSIndexPath = AppDomain.CurrentDomain.BaseDirectory + "Notes\\Journal.sidx";
        string defaultConfigPath = AppDomain.CurrentDomain.BaseDirectory;
        string defaultLanguagePath = AppDomain.CurrentDomain.BaseDirectory + "Languages\\";

        Dictionary<string, string> settings = new Dictionary<string, string>();

        List<string> languageList = new List<string>();
        ObservableCollection<NoteModel> noteList = new();
        
        string noteCurrentOpen = string.Empty;
        string noteLastSaveTime = string.Empty;
        #endregion Variables

        #region Properties
        public string DefaultDBPath
        {
            get { return defaultDatabasePath; }
        }

        public string DefaultPIndexPath
        {
            get { return defaultPIndexPath; }
        }

        public string DefaultSIndexPath
        {
            get { return defaultSIndexPath; }
        }

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
        #endregion Properties

        #region Methods
        public void RestoreDefaultSettings()
        {            
            SetSetting("languageID", "en-US");
            SetSetting("autoSave", "true");
            SetSetting("autoSaveDelay", "5");
            SetSetting("encryption", "true");
            SetSetting("editorView", "1");
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

        public ObservableCollection<NoteModel> NoteList
        {
            get { return noteList; }
            set { noteList = value; }
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
