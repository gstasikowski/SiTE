using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using SiTE.Models;

namespace SiTE.Logic
{
	public class FileOperations
	{
		private Core _coreApp;
		private DataBank _dataBank;

		public FileOperations(Core coreApp, DataBank dataBank)
		{
			_coreApp = coreApp;
			_dataBank = dataBank;
		}

		public void InitialSetup()
		{
			CheckAppDirectories();
			LoadSettings();
			ClearTempDatabaseFiles(); // remove remnant temp files on startup (in case of crash)
			_coreApp.encryptionOperations.PrepareEncryptedFiles();
		}

		void CheckAppDirectories()
		{
			if (!Directory.Exists(_coreApp.dataBank.DefaultNotePath))
			{
				Directory.CreateDirectory(_coreApp.dataBank.DefaultNotePath);
			}
		}

		public bool CheckDatabaseFilesExist(bool encrypted)
		{
			string encryptionExtention = encrypted ? _coreApp.dataBank.EncryptionExtention : string.Empty;

			return (File.Exists(_coreApp.dataBank.DefaultDBPath + encryptionExtention)
				&& File.Exists(_coreApp.dataBank.DefaultPIndexPath + encryptionExtention)
				&& File.Exists(_coreApp.dataBank.DefaultSIndexPath + encryptionExtention));
		}

		public void DeleteFile(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return;
			}
			
			File.Delete(filePath);
		}

		public void ClearTempDatabaseFiles()
		{
			if (_coreApp.dataBank.GetSetting("EncryptDatabase") == "False")
			{
				return;
			}

			if (File.Exists(_coreApp.dataBank.DefaultMasterKeyFile + _coreApp.dataBank.EncryptionExtention))
			{
				DeleteFile(_coreApp.dataBank.DefaultMasterKeyFile);
			}

			DeleteFile(_coreApp.dataBank.DefaultDBPath);
			DeleteFile(_coreApp.dataBank.DefaultPIndexPath);
			DeleteFile(_coreApp.dataBank.DefaultSIndexPath);
		}

		#region Settings
		public void LoadSettings()
		{
			string configFilePath = _coreApp.dataBank.ConfigFile;

			if (File.Exists(configFilePath))
			{
				string configFile = File.ReadAllText(configFilePath);
				ParseSettings(configFile);
			}
			else
			{
				LoadDefaultSettings(_coreApp.dataBank.DefaultConfigFile);
				SaveSettings();
			}
		}

		public void LoadDefaultSettings(string assemblyConfigFile)
		{
			try
			{
				var assembly = Assembly.GetExecutingAssembly();

				using (Stream stream = assembly.GetManifestResourceStream(assemblyConfigFile))
				using (StreamReader reader = new StreamReader(stream))
				{
					string? configContent = reader.ReadToEnd();
					ParseSettings(configContent);

					reader.Close();
				}
			}
			catch (FileNotFoundException e)
			{
				new ErrorHandler("ErrorFileNotFound", e.InnerException.ToString());
			}
		}

		public void SaveSettings()
		{
			Dictionary<string, string> appSettings = _coreApp.dataBank.GetAllSettings();

			FileStream fileStream;
			fileStream = new FileStream(_coreApp.dataBank.ConfigFile, FileMode.Create);

			XElement rootElement = new XElement("Config", appSettings.Select(kv => new XElement(kv.Key, kv.Value)));
			XmlSerializer serializer = new XmlSerializer(rootElement.GetType());
			serializer.Serialize(fileStream, rootElement);

			fileStream.Close();
		}

		public void ParseSettings(string configContent)
		{
			XElement rootElement = XElement.Parse(configContent);

			foreach (var element in rootElement.Elements())
			{
				_coreApp.dataBank.SetSetting(element.Name.LocalName, element.Value);
			}
		}
		#endregion Settings
	}
}
