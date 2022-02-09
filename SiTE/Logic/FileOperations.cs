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
    public class FileOperations
    {
        public FileOperations()
        {
            CheckConfigDirectories();
        }

        void CheckConfigDirectories()
        {
            if (!Directory.Exists(Refs.dataBank.DefaultNotePath))
                Directory.CreateDirectory(Refs.dataBank.DefaultNotePath);

            if (!Directory.Exists(Refs.dataBank.DefaultConfigPath))
                Directory.CreateDirectory(Refs.dataBank.DefaultConfigPath);
        }

        #region Note file IO

        public bool LoadNote(string noteTitle, TextPointer pointerStart, TextPointer pointerEnd)
        {
            TextRange docContent;
            FileStream fileStream;

            string filePath = Refs.dataBank.DefaultNotePath + noteTitle + ".aes";

            if (File.Exists(filePath))
            {
                string filePathDecrypted = filePath.Replace(".aes", Refs.dataBank.TempFileExtension);
                FileDecrypt(filePath, filePathDecrypted, Refs.dataBank.GetSetting("password"));

                docContent = new TextRange(pointerStart, pointerEnd);
                fileStream = new FileStream(filePathDecrypted, FileMode.OpenOrCreate);

                try
                {
                    docContent.Load(fileStream, DataFormats.XamlPackage);
                    Refs.dataBank.NoteCurrentOpen = noteTitle;
                    Refs.dataBank.NoteLastSaveTime = File.GetLastWriteTime(filePath).ToString();
                }
                catch
                {
                    new ErrorHandler(Application.Current, "ErrorWrongPassword"); // probably caused by incorrect encryption password, inform the user and ask for password in case they know it
                    return false;// add choice to input custom password, get back to new/last note if declined
                }

                fileStream.Close();
                File.Delete(filePathDecrypted);
            }
            else
            { return false; }

            return true;
        }

        public void SaveNote(string noteTitle, TextPointer pointerStart, TextPointer pointerEnd)
        {
            string filePath = Refs.dataBank.DefaultNotePath + noteTitle + Refs.dataBank.TempFileExtension; // TODO remove temp .site files step after changing encryption to direct from document

            TextRange textRange;
            FileStream fileStream;
            textRange = new TextRange(pointerStart, pointerEnd);
            fileStream = new FileStream(filePath, FileMode.Create);
            textRange.Save(fileStream, DataFormats.XamlPackage);
            fileStream.Close();

            if (Refs.dataBank.GetSetting("encryption").ToLower() == "true" && Refs.dataBank.GetSetting("password") != string.Empty)
            {
                FileEncrypt(filePath, Refs.dataBank.GetSetting("password"));
                DeleteNote(noteTitle + Refs.dataBank.TempFileExtension);

                if (Refs.dataBank.NoteCurrentOpen != noteTitle)
                {
                    DeleteNote(Refs.dataBank.NoteCurrentOpen + ".aes");
                    Refs.dataBank.NoteCurrentOpen = noteTitle;
                }
            }

            Refs.dataBank.NoteLastSaveTime = DateTime.Now.ToString();
        }

        public void DeleteNote(string noteFile)
        {
            // TODO after getting rid of temp .site files assume all files have the same extension
            // instead of adding it in multiple classes/places
            string filePath = (noteFile.Contains("\\")) ? noteFile : Refs.dataBank.DefaultNotePath + noteFile;

            if (!File.Exists(filePath))
            {
                new ErrorHandler(Application.Current, "ErrorNoFile");
                return;
            }

            File.Delete(filePath);
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

        private void FileEncrypt(string filePath, string password)
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
            FileStream fileStreamCrypt = new FileStream(filePath.Replace(Refs.dataBank.TempFileExtension,".aes"), FileMode.Create);

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
        private void FileDecrypt(string inputFile, string outputFile, string password)
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

        public void UpdateEncryptionPassword(string oldPassword, string newPassword)
        {
            if (oldPassword == newPassword)
                return;

            string[] filesToUpdate = GetNoteList();

            foreach (string file in filesToUpdate)
            {
                string tempFile = file.Replace(".aes", Refs.dataBank.TempFileExtension);
                FileDecrypt(file, tempFile, oldPassword);
                FileEncrypt(tempFile, newPassword);
                DeleteNote(tempFile);
            }
        }

        #endregion Encryption

        public string[] GetNoteList()
        {
            return Directory.GetFiles(Refs.dataBank.DefaultNotePath);
        }

        public void LoadTranslations()
        {
            foreach (string filePath in Directory.EnumerateFiles(Refs.dataBank.DefaultLanguagePath))
            {
                string cultureCode = filePath.Substring(filePath.LastIndexOf('\\') + 1).Replace(".xaml", "");
                var tempCulture = System.Globalization.CultureInfo.GetCultureInfo(cultureCode);
                Refs.dataBank.AddAvailableLanguage(string.Format("{0} [{1}]", tempCulture.DisplayName, tempCulture.Name));
            }
        }

        #region Settings

        public void LoadSettings()
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

        public void SaveSettings()
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
