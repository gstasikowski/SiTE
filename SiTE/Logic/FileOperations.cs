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
            if (!Directory.Exists(Models.DataBank.Instance.DefaultNotePath))
            {
                Directory.CreateDirectory(Models.DataBank.Instance.DefaultNotePath);
            }
        }

        public static bool CheckDatabaseFilesExist(bool encrypted)
        {
            string encryptionExtention = encrypted ? Models.DataBank.Instance.EncryptionExtention : string.Empty;

            return (File.Exists(Models.DataBank.Instance.DefaultDBPath + encryptionExtention)
                && File.Exists(Models.DataBank.Instance.DefaultPIndexPath + encryptionExtention)
                && File.Exists(Models.DataBank.Instance.DefaultSIndexPath + encryptionExtention));
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
            if (Models.DataBank.Instance.GetSetting("encryption") == "False")
            {
                return;
            }

            if (File.Exists(Models.DataBank.Instance.DefaultMasterKeyFile + Models.DataBank.Instance.EncryptionExtention))
            {
                DeleteFile(Models.DataBank.Instance.DefaultMasterKeyFile);
            }

            DeleteFile(Models.DataBank.Instance.DefaultDBPath);
            DeleteFile(Models.DataBank.Instance.DefaultPIndexPath);
            DeleteFile(Models.DataBank.Instance.DefaultSIndexPath);
        }
        
        public static void LoadTranslations() // TODO: use method from the Localizer class instead
        {
            foreach (string filePath in Directory.EnumerateFiles(Models.DataBank.Instance.DefaultLanguagePath))
            {
                string cultureCode = filePath.Substring(filePath.LastIndexOf('\\') + 1).Replace(".xaml", "");
                var newCulture = System.Globalization.CultureInfo.GetCultureInfo(cultureCode);
                Models.DataBank.Instance.AddAvailableLanguage(string.Format("{0} [{1}]", newCulture.DisplayName, newCulture.Name));
            }
        }

        #region Settings
        public static void LoadSettings()
        {
            string configFilePath = Models.DataBank.Instance.DefaultConfigPath;

            if (File.Exists(configFilePath))
            {
                string configFile = File.ReadAllText(configFilePath);
                XElement rootElement = XElement.Parse(configFile);

                foreach (var element in rootElement.Elements())
                {
                    Models.DataBank.Instance.SetSetting(element.Name.LocalName, element.Value);
                }
            }
            else
            {
                Models.DataBank.Instance.RestoreDefaultSettings();
                SaveSettings(); 
            }

            Localizer.Instance.LoadLanguage(Models.DataBank.Instance.GetSetting("languageID"));
        }

        public static void SaveSettings()
        {
            Dictionary<string, string> appSettings = Models.DataBank.Instance.GetAllSettings();

            FileStream fileStream;
            fileStream = new FileStream(Models.DataBank.Instance.DefaultConfigPath, FileMode.Create);

            XElement rootElement = new XElement("Config", appSettings.Select(kv => new XElement(kv.Key, kv.Value)));
            XmlSerializer serializer = new XmlSerializer(rootElement.GetType());
            serializer.Serialize(fileStream, rootElement);

            fileStream.Close();
        }
        #endregion Settings
    }
}
