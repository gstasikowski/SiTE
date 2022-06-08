﻿using System;
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
            LoadTranslations();
            LoadSettings();
            //ClearTempDatabaseFiles(); // remove remnant temp files on startup (in case of crash)
            DencryptDatabase();
            PrepareMasterPassword();
        }

        static void CheckAppDirectories()
        {
            if (!Directory.Exists(Refs.dataBank.DefaultNotePath))
            { Directory.CreateDirectory(Refs.dataBank.DefaultNotePath); }
        }

        static bool CheckDatabaseFilesExist(bool encrypted)
        {
            string encryptionExtention = encrypted ? Refs.dataBank.EncryptionExtention : string.Empty;

            return (File.Exists(Refs.dataBank.DefaultDBPath + encryptionExtention)
                && File.Exists(Refs.dataBank.DefaultPIndexPath + encryptionExtention)
                && File.Exists(Refs.dataBank.DefaultSIndexPath + encryptionExtention));
        }

        static void DeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
            { return; }
            
            File.Delete(filePath);
        }

        #region Database IO
        public static void EncryptDatabase()
        {
            if (Refs.dataBank.GetSetting("encryption") == "False")
            { return; }

            if (CheckDatabaseFilesExist(false))
            {
                FileEncrypt(Refs.dataBank.DefaultDBPath, PrepareMasterPassword());
                FileEncrypt(Refs.dataBank.DefaultPIndexPath, PrepareMasterPassword());
                FileEncrypt(Refs.dataBank.DefaultSIndexPath, PrepareMasterPassword());
            }
        }

        private static void DencryptDatabase()
        {
            if (Refs.dataBank.GetSetting("encryption") == "False")
            { return; }

            if (CheckDatabaseFilesExist(true))
            {
                DecryptMasterKeyFile();
                FileDecrypt(Refs.dataBank.DefaultDBPath + Refs.dataBank.EncryptionExtention, Refs.dataBank.DefaultDBPath, PrepareMasterPassword());
                FileDecrypt(Refs.dataBank.DefaultPIndexPath + Refs.dataBank.EncryptionExtention, Refs.dataBank.DefaultPIndexPath, PrepareMasterPassword());
                FileDecrypt(Refs.dataBank.DefaultSIndexPath + Refs.dataBank.EncryptionExtention, Refs.dataBank.DefaultSIndexPath, PrepareMasterPassword());
            }
        }

        public static void ClearTempDatabaseFiles()
        {
            if (Refs.dataBank.GetSetting("encryption") == "False")
            { return; }

            if (File.Exists(Refs.dataBank.DefaultMasterKeyFile + Refs.dataBank.EncryptionExtention))
            { DeleteFile(Refs.dataBank.DefaultMasterKeyFile); }

            DeleteFile(Refs.dataBank.DefaultDBPath);
            DeleteFile(Refs.dataBank.DefaultPIndexPath);
            DeleteFile(Refs.dataBank.DefaultSIndexPath);
        }
        #endregion Database IO
        
        #region Encryption
        private static string PrepareMasterPassword()
        {
            string masterPassword = string.Empty;

            if (File.Exists(Refs.dataBank.DefaultMasterKeyFile))
            { masterPassword = File.ReadAllText(Refs.dataBank.DefaultMasterKeyFile); }
            else
            {
                masterPassword = GenerateMasterPassword();
                
                FileStream fileStream = new FileStream(Refs.dataBank.DefaultMasterKeyFile, FileMode.Create);

                fileStream.Write(Encoding.UTF8.GetBytes(masterPassword));
                fileStream.Close();
            }

            return masterPassword;
        }

        static string GenerateMasterPassword()
        {
            string password = string.Empty;

            List<char> charList = new List<char>();
            Random rng = new Random();

            for (int i = 0; i < 26; i++)
                charList.Add((char)('a' + i));
        
            for (int i = 0; i< 26; i++)
                charList.Add((char)('A' + i));
            
            for (int i = 48; i< 58; i++)
                charList.Add((char) i);

            for (int i = 0;  i < 256; i++)
            { password += charList[rng.Next(0, charList.Count)]; }

            return password;
        }

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
                new ErrorHandler("ErrorNoFileEncrypt", filePath);
                return;
            }

            //generate random salt
            byte[] salt = GenerateRandomSalt();

            //create output file name
            FileStream fileStreamCrypt = new FileStream(filePath + Refs.dataBank.EncryptionExtention, FileMode.Create);

            //convert password string to byte arrray
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            //Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;
            
            //http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
            //"What it does is repeatedly hash the user password along with the salt." High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000, HashAlgorithmName.SHA256);
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
                new ErrorHandler("ErrorDefault", ex.Message);
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
        private static bool FileDecrypt(string inputFile, string outputFile, string password)
        {
            if (!File.Exists(inputFile))
            {
                new ErrorHandler("ErrorNoFileDecrypt", inputFile);
                return false;
            }

            bool isSuccess = true;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000, HashAlgorithmName.SHA256);
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
                isSuccess = false;
                new ErrorHandler("ErrorCryptographicException", ex_CryptographicException.Message);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                new ErrorHandler("ErrorDefault", ex.Message);
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                isSuccess = false;
                new ErrorHandler("ErrorCryptoStream", ex.Message);
            }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }

            return isSuccess;
        }

        private static void DecryptMasterKeyFile()
        {
            if (!File.Exists(Refs.dataBank.DefaultMasterKeyFile + Refs.dataBank.EncryptionExtention))
            { return; }

            bool isPasswordAccepted = false;

            while (!isPasswordAccepted)
            {
                new UserPasswordHandler();
                isPasswordAccepted = FileDecrypt(Refs.dataBank.DefaultMasterKeyFile + Refs.dataBank.EncryptionExtention, Refs.dataBank.DefaultMasterKeyFile, Refs.dataBank.UserPassword);
            }
        }

        /// <summary>
        /// Delete encrypted files when disabling encryption
        /// or reencrypt masterKey file on password change.
        /// </summary>
        public static void UpdateEncryption()
        {
            if (Refs.dataBank.GetSetting("encryption") == "False")
            {
                DeleteFile(Refs.dataBank.DefaultMasterKeyFile);
                DeleteFile(Refs.dataBank.DefaultDBPath + Refs.dataBank.EncryptionExtention);
                DeleteFile(Refs.dataBank.DefaultPIndexPath + Refs.dataBank.EncryptionExtention);
                DeleteFile(Refs.dataBank.DefaultSIndexPath + Refs.dataBank.EncryptionExtention);
            }

            if (Refs.dataBank.UserPassword == string.Empty)
            { DeleteFile(Refs.dataBank.DefaultMasterKeyFile + Refs.dataBank.EncryptionExtention); }
            else
            { FileEncrypt(Refs.dataBank.DefaultMasterKeyFile, Refs.dataBank.UserPassword); }
        }
        #endregion Encryption

        public static void LoadTranslations()
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
            fileStream = new FileStream(Refs.dataBank.DefaultConfigPath, FileMode.Create);

            XElement rootElement = new XElement("Config", appSettings.Select(kv => new XElement(kv.Key, kv.Value)));
            XmlSerializer serializer = new XmlSerializer(rootElement.GetType());
            serializer.Serialize(fileStream, rootElement);

            fileStream.Close();
        }
        #endregion Settings
    }
}
