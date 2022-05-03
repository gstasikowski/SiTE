using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SiTE.Logic
{
    public static class FileOperations
    {
        public static void InitialSetup()
        {
            CheckConfigDirectories();
            LoadTranslations();
            LoadSettings();
            CheckForPlainDatabaseFiles(); // remove remnant temp files on startup (in case of crash)
            CheckForEncryptedDatabaseFiles();
        }

        static void CheckConfigDirectories()
        {
            if (!Directory.Exists(Refs.dataBank.DefaultNotePath))
                Directory.CreateDirectory(Refs.dataBank.DefaultNotePath);

            if (!Directory.Exists(Refs.dataBank.DefaultConfigPath))
                Directory.CreateDirectory(Refs.dataBank.DefaultConfigPath);
        }

        static void DeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
            { return; }
            
            File.Delete(filePath);
        }

        #region Database IO
        private static void EncryptDatabase()
        {
            if (Refs.dataBank.GetSetting("encryption") == "False")
                return;

            if (File.Exists(Refs.dataBank.DefaultDBPath) 
                && File.Exists(Refs.dataBank.DefaultPIndexPath) 
                && File.Exists(Refs.dataBank.DefaultSIndexPath))
            {
                FileEncrypt(Refs.dataBank.DefaultDBPath, Refs.dataBank.GetSetting("password"));
                FileEncrypt(Refs.dataBank.DefaultPIndexPath, Refs.dataBank.GetSetting("password"));
                FileEncrypt(Refs.dataBank.DefaultSIndexPath, Refs.dataBank.GetSetting("password"));
            }
        }

        private static void DencryptDatabase()
        {
            if (Refs.dataBank.GetSetting("encryption") == "False")
                return;

            if (File.Exists(Refs.dataBank.DefaultDBPath + ".aes")
                && File.Exists(Refs.dataBank.DefaultPIndexPath + ".aes")
                && File.Exists(Refs.dataBank.DefaultSIndexPath + ".aes"))
            {
                FileDecrypt(Refs.dataBank.DefaultDBPath + ".aes", Refs.dataBank.DefaultDBPath, Refs.dataBank.GetSetting("password"));
                FileDecrypt(Refs.dataBank.DefaultPIndexPath + ".aes", Refs.dataBank.DefaultPIndexPath, Refs.dataBank.GetSetting("password"));
                FileDecrypt(Refs.dataBank.DefaultSIndexPath + ".aes", Refs.dataBank.DefaultSIndexPath, Refs.dataBank.GetSetting("password"));
            }
            }

        public static void CheckForEncryptedDatabaseFiles()
        {            
            if (File.Exists(Refs.dataBank.DefaultDBPath + ".aes")
                && File.Exists(Refs.dataBank.DefaultPIndexPath + ".aes")
                && File.Exists(Refs.dataBank.DefaultSIndexPath + ".aes"))
            { DencryptDatabase(); }
        }

        public static void CheckForPlainDatabaseFiles()
        {
            if (Refs.dataBank.GetSetting("encryption") == "False")
                return;

            DeleteFile(Refs.dataBank.DefaultDBPath);
            DeleteFile(Refs.dataBank.DefaultPIndexPath);
            DeleteFile(Refs.dataBank.DefaultSIndexPath);
        }
        #endregion Database IO

        #region Note file IO
        public static void GetNoteList()
        {
            using (var db = new NoteDatabase(Refs.dataBank.DefaultDBPath))
            {
                var noteList = db.GetAll();
                Refs.dataBank.NoteList.Clear();

                foreach (var note in noteList)
                { Refs.dataBank.NoteList.Add(note); }
            }
        }

        public static Models.NoteModel LoadNote(Guid noteID)
        {
            using (var db = new NoteDatabase(Refs.dataBank.DefaultDBPath))
            { return db.Find(noteID); }
        }

        public static void SaveNote(string noteID, string noteTitle, TextPointer pointerStart, TextPointer pointerEnd)
        {
            TextRange textRange = new TextRange(pointerStart, pointerEnd);

            using (var db = new NoteDatabase(Refs.dataBank.DefaultDBPath))
            {
                var noteGuid = string.IsNullOrEmpty(noteID) ? Guid.NewGuid() : Guid.Parse(noteID);
                var oldNote = db.Find(noteGuid);

                Models.NoteModel freshNote = new Models.NoteModel
                {
                    ID = noteGuid,
                    Title = noteTitle,
                    Content = textRange.Text,
                    Modified = DateTime.Now
                };

                if (oldNote == null)
                {
                    freshNote.Created = DateTime.Now;
                    db.Insert(freshNote);
                }
                else
                {
                    freshNote.Created = oldNote.Created;
                    db.Update(freshNote);
                }
            }

            EncryptDatabase();
        }

        public static void DeleteNote(Guid noteID)
        {
            using (var db = new NoteDatabase(Refs.dataBank.DefaultDBPath))
            {
                var noteToRemove = db.Find(noteID);
                db.Delete(noteToRemove);
            }

            EncryptDatabase();
        }
        #endregion Note file IO

        #region Encryption
        static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // Fille the buffer with the generated data
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        private static void FileEncrypt(string filePath, string password)
        {
            //http://stackoverflow.com/questions/27645527/aes-encryption-on-large-files

            if (!File.Exists(filePath))
            {
                new ErrorHandler(Application.Current, "ErrorNoFile");
                return;
            }

            //generate random salt
            byte[] salt = GenerateRandomSalt();

            //create output file name
            FileStream fileStreamCrypt = new FileStream(filePath + ".aes", FileMode.Create);

            //convert password string to byte arrray
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            //Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            //http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
            //"What it does is repeatedly hash the user password along with the salt." High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            //Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            AES.Mode = CipherMode.CFB;

            // write salt to the begining of the output file, so in this case can be random every time
            fileStreamCrypt.Write(salt, 0, salt.Length);

            CryptoStream cryptoStream = new CryptoStream(fileStreamCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
            FileStream fileStreamTemp = new FileStream(filePath, FileMode.Open);

            //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fileStreamTemp.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                    cryptoStream.Write(buffer, 0, read);
                }

                // Close up
                fileStreamTemp.Close();
            }
            catch (Exception ex)
            {
                new ErrorHandler(Application.Current, "ErrorWrongPassword", ex.Message);
            }
            finally
            {
                cryptoStream.Close();
                fileStreamCrypt.Close();
            }
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="password"></param>
        private static void FileDecrypt(string inputFile, string outputFile, string password)
        {
            if (!File.Exists(inputFile))
            {
                new ErrorHandler(Application.Current, "ErrorNoFile");
                return;
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Application.DoEvents();
                    fsOut.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                new ErrorHandler(Application.Current, "ErrorCryptographicException", ex_CryptographicException.Message);
            }
            catch (Exception ex)
            {
                new ErrorHandler(Application.Current, "ErrorDefault", ex.Message);
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                new ErrorHandler(Application.Current, "ErrorCryptoStream", ex.Message);
            }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }
        }

        /// <summary>
        /// Delete encrypted files when disabling encryption
        /// or reencrypt database files on password change.
        /// </summary>
        public static void UpdateEncryption()
        {
            if (Refs.dataBank.GetSetting("encryption") == "False")
            {
                DeleteFile(Refs.dataBank.DefaultDBPath + ".aes");
                DeleteFile(Refs.dataBank.DefaultPIndexPath + ".aes");
                DeleteFile(Refs.dataBank.DefaultSIndexPath + ".aes");
            }
            else
            { EncryptDatabase(); }
        }
        #endregion Encryption

        public static void LoadTranslations()
        {
            foreach (string filePath in Directory.EnumerateFiles(Refs.dataBank.DefaultLanguagePath))
            {
                string cultureCode = filePath.Substring(filePath.LastIndexOf('\\') + 1).Replace(".xaml", "");
                var tempCulture = System.Globalization.CultureInfo.GetCultureInfo(cultureCode);
                Refs.dataBank.AddAvailableLanguage(string.Format("{0} [{1}]", tempCulture.DisplayName, tempCulture.Name));
            }
        }

        #region Settings
        public static void LoadSettings()
        {
            string configFilePath = Refs.dataBank.DefaultConfigPath + "Config.xml";

            if (File.Exists(configFilePath))
            {
                string configFile = File.ReadAllText(configFilePath);
                XElement rootElement = XElement.Parse(configFile);

                foreach (var element in rootElement.Elements())
                { Refs.dataBank.SetSetting(element.Name.LocalName, element.Value); }
            }
            else
            {
                Refs.dataBank.RestoreDefaultSettings();
                SaveSettings(); 
            }

            Refs.localizationHandler.SwitchLanguage(Refs.dataBank.GetSetting("languageID"));
        }

        public static void SaveSettings()
        {
            Dictionary<string, string> appSettings = Refs.dataBank.GetAllSettings();

            FileStream fileStream;
            fileStream = new FileStream(Refs.dataBank.DefaultConfigPath + "Config.xml", FileMode.Create);

            XElement rootElement = new XElement("Config", appSettings.Select(kv => new XElement(kv.Key, kv.Value)));
            XmlSerializer serializer = new XmlSerializer(rootElement.GetType());
            serializer.Serialize(fileStream, rootElement);

            fileStream.Close();
        }
        #endregion Settings
    }
}
