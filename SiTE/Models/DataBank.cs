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

		private readonly string _defaultConfigFile = "SiTE.DefaultConfig.xml";
		private readonly string _configFile = AppDomain.CurrentDomain.BaseDirectory + "Config.xml";
		private readonly string _defaultNotePath = AppDomain.CurrentDomain.BaseDirectory + "Notes/";
		private readonly string _defaultMasterKeyFile = "master.key";
		private readonly string _defaultDatabaseFile = "Journal.data";
		private readonly string _defaultPIndexFile = "Journal.pixd";
		private readonly string _defaultSIndexFile = "Journal.sidx";
		private readonly string _defaultLanguagePath = AppDomain.CurrentDomain.BaseDirectory + "Languages/";
		private readonly string _encryptionExtention = ".aes";

		private string _userPassword = string.Empty;

		private Dictionary<string, string> _settings = new Dictionary<string, string>();
		private List<string> _languageList = new List<string>();
		private ObservableCollection<NoteModel> _noteList = new();
		private NoteModel _activeNote = new NoteModel();
		private bool _isNoteModified;
		#endregion Variables

		#region Properties
		public string DefaultConfigFile
		{
			get { return _defaultConfigFile; }
		}
		public string ConfigFile
		{
			get { return _configFile; }
		}

		public string DefaultNotePath
		{
			get { return _defaultNotePath; }
		}

		public string DefaultMasterKeyFile
		{
			get { return _defaultNotePath + _defaultMasterKeyFile; }
		}

		public string DefaultDBPath
		{
			get { return _defaultNotePath + _defaultDatabaseFile; }
		}

		public string DefaultPIndexPath
		{
			get { return _defaultNotePath + _defaultPIndexFile; }
		}

		public string DefaultSIndexPath
		{
			get { return _defaultNotePath + _defaultSIndexFile; }
		}

		public string DefaultLanguagePath
		{
			get { return _defaultLanguagePath; }
		}

		public string EncryptionExtention
		{ 
			get { return _encryptionExtention; } 
		}

		public string UserPassword
		{ 
			get { return _userPassword; } 
		}

		public ObservableCollection<NoteModel> NoteList
		{
			get { return _noteList; }
			set { _noteList = value; }
		}

		public NoteModel ActiveNote
		{
			get { return _activeNote; }
			set { _activeNote = value; }
		}

		public bool IsNoteModified
		{
			get { return _isNoteModified; }
			set { _isNoteModified = value; }
		}
		#endregion Properties

		#region Methods
		public void UpdatePassword(string newPassword, bool onStart)
		{
			_userPassword = newPassword;

			if (!onStart)
			{
				SiTE.Core.Instance.encryptionOperations.UpdateEncryption();
			}
		}

		public Dictionary<string, string> GetAllSettings()
		{
			return _settings;
		}

		public string GetSetting(string key)
		{
			return _settings[key];
		}

		public void SetSetting(string key, string value)
		{
			if (_settings.ContainsKey(key))
			{
				_settings[key] = value;
			}
			else
			{
				_settings.Add(key, value);
			}
		}

		public string GetNoteTitle(Guid noteID)
		{
			return NoteList.FirstOrDefault(n => n.ID == noteID).Title;
		}

		public int GetNoteIndex(Guid noteID)
		{
			NoteModel note = NoteList.FirstOrDefault(n => n.ID.Equals(noteID));
			return NoteList.IndexOf(note);
		}

		public int GetNoteIndexFromTitle(string noteTitle)
		{
			NoteModel note = NoteList.FirstOrDefault(n => n.Title.Equals(noteTitle));
			return NoteList.IndexOf(note);
		}

		public void NewNote()
		{
			ActiveNote.ID = System.Guid.Empty;
			ActiveNote.Title = string.Empty;
			ActiveNote.Content = string.Empty;
			ActiveNote.Created = new DateTime();
			ActiveNote.Modified = new DateTime();
		}

		public void OpenNote(System.Guid noteID)
		{
			var tempNote = SiTE.Core.Instance.databaseOperations.LoadNote(noteID);
			
			ActiveNote.ID = tempNote.ID;
			ActiveNote.Title = tempNote.Title;
			ActiveNote.Content = tempNote.Content;
			ActiveNote.Created = tempNote.Created;
			ActiveNote.Modified = tempNote.Modified;
		}

		public void SaveNote()
		{
			SiTE.Core.Instance.databaseOperations.SaveNote(ActiveNote.ID, ActiveNote.Title, ActiveNote.Content);
		}

		public void DeleteNote()
		{
			SiTE.Core.Instance.databaseOperations.DeleteNote(ActiveNote.ID);
		}
		#endregion Methods
	}
}
