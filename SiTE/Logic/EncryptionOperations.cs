using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SiTE.Logic
{
    public static class EncryptionOperations
    {
        private static string PrepareMasterKeyFile()
        {
            string masterPassword = string.Empty;

            if (File.Exists(Models.DataBank.Instance.DefaultMasterKeyFile))
            { masterPassword = File.ReadAllText(Models.DataBank.Instance.DefaultMasterKeyFile); }
            else
            {
                masterPassword = GenerateMasterPassword();

                FileStream fileStream = new FileStream(Models.DataBank.Instance.DefaultMasterKeyFile, FileMode.Create);

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

            for (int i = 0; i < 26; i++)
                charList.Add((char)('A' + i));

            for (int i = 48; i < 58; i++)
                charList.Add((char)i);

            for (int i = 0; i < 256; i++)
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

        public static void PrepareEncryptedFiles()
        {
            if (DencryptDatabase())
            { PrepareMasterKeyFile(); }
        }

        public static void EncryptDatabase()
        {
            if (Models.DataBank.Instance.GetSetting("encryption") == "False")
            { return; }

            if (FileOperations.CheckDatabaseFilesExist(false))
            {
                FileEncrypt(Models.DataBank.Instance.DefaultDBPath, PrepareMasterKeyFile());
                FileEncrypt(Models.DataBank.Instance.DefaultPIndexPath, PrepareMasterKeyFile());
                FileEncrypt(Models.DataBank.Instance.DefaultSIndexPath, PrepareMasterKeyFile());
            }
        }

        private static bool DencryptDatabase()
        {
            if (Models.DataBank.Instance.GetSetting("encryption") == "False")
            { return true; }

            if (FileOperations.CheckDatabaseFilesExist(true))
            {
                if (DecryptMasterKeyFile())
                {
                    FileDecrypt(Models.DataBank.Instance.DefaultDBPath + Models.DataBank.Instance.EncryptionExtention, Models.DataBank.Instance.DefaultDBPath, PrepareMasterKeyFile());
                    FileDecrypt(Models.DataBank.Instance.DefaultPIndexPath + Models.DataBank.Instance.EncryptionExtention, Models.DataBank.Instance.DefaultPIndexPath, PrepareMasterKeyFile());
                    FileDecrypt(Models.DataBank.Instance.DefaultSIndexPath + Models.DataBank.Instance.EncryptionExtention, Models.DataBank.Instance.DefaultSIndexPath, PrepareMasterKeyFile());
                }
                else
                { return false; }
            }

            return true;
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
            FileStream fileStreamCrypt = new FileStream(filePath + Models.DataBank.Instance.EncryptionExtention, FileMode.Create);

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

        private static bool DecryptMasterKeyFile()
        {
            if (!File.Exists(Models.DataBank.Instance.DefaultMasterKeyFile + Models.DataBank.Instance.EncryptionExtention))
            { return true; }

            bool isPasswordAccepted = false;

            while (!isPasswordAccepted)
            {
                UserPasswordHandler passwordHandler = new UserPasswordHandler();

                if (passwordHandler.canUnlockDatabase)
                { isPasswordAccepted = FileDecrypt(Models.DataBank.Instance.DefaultMasterKeyFile + Models.DataBank.Instance.EncryptionExtention, Models.DataBank.Instance.DefaultMasterKeyFile, Models.DataBank.Instance.UserPassword); }
                else
                { return false; }
            }

            return true;
        }

        /// <summary>
        /// Delete encrypted files when disabling encryption
        /// or reencrypt masterKey file on password change.
        /// </summary>
        public static void UpdateEncryption()
        {
            if (Models.DataBank.Instance.GetSetting("encryption") == "False")
            {
                FileOperations.DeleteFile(Models.DataBank.Instance.DefaultMasterKeyFile);
                FileOperations.DeleteFile(Models.DataBank.Instance.DefaultDBPath + Models.DataBank.Instance.EncryptionExtention);
                FileOperations.DeleteFile(Models.DataBank.Instance.DefaultPIndexPath + Models.DataBank.Instance.EncryptionExtention);
                FileOperations.DeleteFile(Models.DataBank.Instance.DefaultSIndexPath + Models.DataBank.Instance.EncryptionExtention);
            }

            if (Models.DataBank.Instance.UserPassword == string.Empty)
            { FileOperations.DeleteFile(Models.DataBank.Instance.DefaultMasterKeyFile + Models.DataBank.Instance.EncryptionExtention); }
            else
            { FileEncrypt(Models.DataBank.Instance.DefaultMasterKeyFile, Models.DataBank.Instance.UserPassword); }
        }
    }
}
