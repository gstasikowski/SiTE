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

        public void LoadDocument(string filepath, TextPointer pointerStart, TextPointer pointerEnd)
        {
            TextRange docContent;
            FileStream fStream;

            string fullFilePath = Refs.dataBank.DefaultNotePath + filepath + ".aes";

            if (File.Exists(fullFilePath))
            {
                string filepathDecrypted = fullFilePath.Replace(".aes", ".site");
                FileDecrypt(fullFilePath, filepathDecrypted, Refs.dataBank.GetSetting("password"));
                
                docContent = new TextRange(pointerStart, pointerEnd);
                fStream = new FileStream(filepathDecrypted, FileMode.OpenOrCreate);
                try
                {  docContent.Load(fStream, DataFormats.XamlPackage); }
                catch
                {                    
                    new ErrorHandler(Application.Current, "ErrorWrongPassword"); // probably caused by incorrect encryption password, inform the user and ask for password in case they know it
                }
                fStream.Close();

                File.Delete(filepathDecrypted);
                Refs.dataBank.CurrentOpenNote = filepath;
                Refs.dataBank.LastSaveNote = File.GetLastWriteTime(fullFilePath).ToString();
            }
        }

        public void SaveDocument(string filepath, TextPointer pointerStart, TextPointer pointerEnd)
        {
            string fullFilePath = Refs.dataBank.DefaultNotePath + filepath + ".site"; // TODO remove temp .site files step after changing encryption to direct from document

            TextRange range;
            FileStream fStream;
            range = new TextRange(pointerStart, pointerEnd);
            fStream = new FileStream(fullFilePath, FileMode.Create);
            range.Save(fStream, DataFormats.XamlPackage);
            fStream.Close();

            if (Refs.dataBank.GetSetting("encryption") == "False")
                return;
            
            FileEncrypt(fullFilePath, Refs.dataBank.GetSetting("password"));
            DeleteDocument(filepath + ".site");

            if (Refs.dataBank.CurrentOpenNote != string.Empty && Refs.dataBank.CurrentOpenNote != filepath)
            {
                DeleteDocument(Refs.dataBank.CurrentOpenNote + ".aes");
                Refs.dataBank.CurrentOpenNote = filepath;
            }

            Refs.dataBank.LastSaveNote = DateTime.Now.ToString();
        }

        public void DeleteDocument(string filepath)
        {
            // TODO after getting rid of temp .site files assume all files have the same extension
            // instead of adding it in multiple classes/places
            File.Delete(Refs.dataBank.DefaultNotePath + filepath);
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

        private void FileEncrypt(string filepath, string password) // TODO modify encryption to save straight from text area instead of temp file
        {
            //http://stackoverflow.com/questions/27645527/aes-encryption-on-large-files

            //generate random salt
            byte[] salt = GenerateRandomSalt();

            //create output file name
            FileStream fsCrypt = new FileStream(filepath.Replace(".site",".aes"), FileMode.Create);

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
            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(filepath, FileMode.Open);

            //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                    cs.Write(buffer, 0, read);
                }

                // Close up
                fsIn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
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
                Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error by closing CryptoStream: " + ex.Message);
            }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }
        }

        public string[] GetNoteList()
        {
            return Directory.GetFiles(Refs.dataBank.DefaultNotePath);
        }

        public void LoadTranslations()
        {
            foreach (string fileName in Directory.EnumerateFiles(Refs.dataBank.DefaultLanguagePath))
            {
                string cultureCode = fileName.Substring(fileName.LastIndexOf('\\') + 1).Replace(".xaml", "");
                var tempCulture = System.Globalization.CultureInfo.GetCultureInfo(cultureCode);
                Refs.dataBank.AddAvailableLanguage(string.Format("{0} [{1}]", tempCulture.DisplayName, tempCulture.Name));
            }
        }

        public void LoadAppSettings()
        {
            string configFilePath = Refs.dataBank.DefaultConfigPath + "Config.xml";

            if (File.Exists(configFilePath))
            {
                string configFile = File.ReadAllText(configFilePath);
                XElement rootElement = XElement.Parse(configFile);
                foreach (var el in rootElement.Elements())
                {
                    Refs.dataBank.SetSetting(el.Name.LocalName, el.Value);
                }
            }
            else
            { SaveSettings(); }

            Refs.localizationHandler.SwitchLanguage(Refs.dataBank.GetSetting("languageID"));
        }

        public void SaveSettings()
        {
            Dictionary<string, string> settingsObject = Refs.dataBank.GetAllSettings();

            FileStream fStream;
            fStream = new FileStream(Refs.dataBank.DefaultConfigPath + "Config.xml", FileMode.Create);

            XElement rootElement = new XElement("Config", settingsObject.Select(kv => new XElement(kv.Key, kv.Value)));
            XmlSerializer serializer = new XmlSerializer(rootElement.GetType());
            serializer.Serialize(fStream, rootElement);

            fStream.Close();
        }
    }
}
