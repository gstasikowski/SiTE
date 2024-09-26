using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SiTE.Logic
{
    public static class FileOperations
    {
        public static void InitialSetup()
        {
            CheckAppDirectories();
            // LoadTranslations();
            LoadSettings();
            ClearTempDatabaseFiles(); // remove remnant temp files on startup (in case of crash)
            EncryptionOperations.PrepareEncryptedFiles();
        }

        static void CheckAppDirectories()
        {
            if (!Directory.Exists(Refs.dataBank.DefaultNotePath))
            {
                Directory.CreateDirectory(Refs.dataBank.DefaultNotePath);
            }
        }

        public static bool CheckDatabaseFilesExist(bool encrypted)
        {
            string encryptionExtention = encrypted ? Refs.dataBank.EncryptionExtention : string.Empty;

            return (File.Exists(Refs.dataBank.DefaultDBPath + encryptionExtention)
                && File.Exists(Refs.dataBank.DefaultPIndexPath + encryptionExtention)
                && File.Exists(Refs.dataBank.DefaultSIndexPath + encryptionExtention));
        }

        public static void DeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            
            File.Delete(filePath);
        }

        public static void ClearTempDatabaseFiles()
        {
            if (Refs.dataBank.GetSetting("encryption") == "False")
            {
                return;
            }

            if (File.Exists(Refs.dataBank.DefaultMasterKeyFile + Refs.dataBank.EncryptionExtention))
            {
                DeleteFile(Refs.dataBank.DefaultMasterKeyFile);
            }

            DeleteFile(Refs.dataBank.DefaultDBPath);
            DeleteFile(Refs.dataBank.DefaultPIndexPath);
            DeleteFile(Refs.dataBank.DefaultSIndexPath);
        }
        
        public static void LoadTranslations() // TODO: use method from the Localizer class instead
        {
            foreach (string filePath in Directory.EnumerateFiles(Refs.dataBank.DefaultLanguagePath))
            {
                string cultureCode = filePath.Substring(filePath.LastIndexOf('\\') + 1).Replace(".xaml", "");
                var newCulture = System.Globalization.CultureInfo.GetCultureInfo(cultureCode);
                Refs.dataBank.AddAvailableLanguage(string.Format("{0} [{1}]", newCulture.DisplayName, newCulture.Name));
            }
        }

        #region Settings
        public static void LoadSettings()
        {
            string configFilePath = Refs.dataBank.DefaultConfigPath;

            if (File.Exists(configFilePath))
            {
                string configFile = File.ReadAllText(configFilePath);
                XElement rootElement = XElement.Parse(configFile);

                foreach (var element in rootElement.Elements())
                {
                    Refs.dataBank.SetSetting(element.Name.LocalName, element.Value);
                }
            }
            else
            {
                Refs.dataBank.RestoreDefaultSettings();
                SaveSettings(); 
            }

            Localizer.Instance.LoadLanguage(Refs.dataBank.GetSetting("languageID"));
        }

        public static void SaveSettings()
        {
            Dictionary<string, string> appSettings = Refs.dataBank.GetAllSettings();

            FileStream fileStream;
            fileStream = new FileStream(Refs.dataBank.DefaultConfigPath, FileMode.Create);

            XElement rootElement = new XElement("Config", appSettings.Select(kv => new XElement(kv.Key, kv.Value)));
            XmlSerializer serializer = new XmlSerializer(rootElement.GetType());
            serializer.Serialize(fileStream, rootElement);

            fileStream.Close();
        }
        #endregion Settings
    }
}
