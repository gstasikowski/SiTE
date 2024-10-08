using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SiTE.Logic
{
	public class EncryptionOperations
	{
		private Core _coreApp;

		public EncryptionOperations(Core coreApp)
		{
			_coreApp = coreApp;
		}

		private string PrepareMasterKeyFile()
		{
			string masterPassword = string.Empty;

			if (File.Exists(_coreApp.dataBank.DefaultMasterKeyFile))
			{
				masterPassword = File.ReadAllText(_coreApp.dataBank.DefaultMasterKeyFile);
			}
			else
			{
				masterPassword = GenerateMasterPassword();

				FileStream fileStream = new FileStream(_coreApp.dataBank.DefaultMasterKeyFile, FileMode.Create);

				fileStream.Write(Encoding.UTF8.GetBytes(masterPassword));
				fileStream.Close();
			}

			return masterPassword;
		}

		private string GenerateMasterPassword()
		{
			string password = string.Empty;

			List<char> charList = new List<char>();
			Random rng = new Random();

			for (int i = 0; i < 26; i++)
			{
				charList.Add((char)('a' + i));
			}

			for (int i = 0; i < 26; i++)
			{
				charList.Add((char)('A' + i));
			}

			for (int i = 48; i < 58; i++)
			{
				charList.Add((char)i);
			}

			for (int i = 0; i < 256; i++)
			{
				password += charList[rng.Next(0, charList.Count)];
			}

			return password;
		}

		private byte[] GenerateRandomSalt()
		{
			byte[] data = new byte[32];

			using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
			{
				for (int i = 0; i < 10; i++)
				{
					// Fille the buffer with the generated data
					rng.GetBytes(data);
				}
			}

			return data;
		}

		public void PrepareEncryptedFiles()
		{
			if (DecryptDatabase())
			{
				PrepareMasterKeyFile();
			}
		}

		public void EncryptDatabase()
		{
			if (_coreApp.dataBank.GetSetting("EncryptDatabase") == "False")
			{
				return;
			}

			if (_coreApp.fileOperations.CheckDatabaseFilesExist(false))
			{
				FileEncrypt(_coreApp.dataBank.DefaultDBPath, PrepareMasterKeyFile());
				FileEncrypt(_coreApp.dataBank.DefaultPIndexPath, PrepareMasterKeyFile());
				FileEncrypt(_coreApp.dataBank.DefaultSIndexPath, PrepareMasterKeyFile());
			}
		}

		private bool DecryptDatabase()
		{
			if (_coreApp.dataBank.GetSetting("EncryptDatabase") == "False")
			{
				return true;
			}

			if (_coreApp.fileOperations.CheckDatabaseFilesExist(true))
			{
				if (DecryptMasterKeyFile())
				{
					FileDecrypt(
						_coreApp.dataBank.DefaultDBPath + _coreApp.dataBank.EncryptionExtention,
						_coreApp.dataBank.DefaultDBPath,
						PrepareMasterKeyFile()
					);
					FileDecrypt(
						_coreApp.dataBank.DefaultPIndexPath + _coreApp.dataBank.EncryptionExtention,
						_coreApp.dataBank.DefaultPIndexPath,
						PrepareMasterKeyFile()
					);
					FileDecrypt(
						_coreApp.dataBank.DefaultSIndexPath + _coreApp.dataBank.EncryptionExtention,
						_coreApp.dataBank.DefaultSIndexPath,
						PrepareMasterKeyFile()
					);
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		private void FileEncrypt(string filePath, string password)
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
			FileStream fileStreamCrypt = new FileStream(filePath + _coreApp.dataBank.EncryptionExtention, FileMode.Create);

			//convert password string to byte arrray
			byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

			//Set Rijndael symmetric encryption algorithm
			using (Aes myAes = Aes.Create())
			{
				myAes.KeySize = 256;
				myAes.BlockSize = 128;
				myAes.Padding = PaddingMode.PKCS7;

				//http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
				//"What it does is repeatedly hash the user password along with the salt." High iteration counts.
				var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000, HashAlgorithmName.SHA256);
				myAes.Key = key.GetBytes(myAes.KeySize / 8);
				myAes.IV = key.GetBytes(myAes.BlockSize / 8);

				//Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
				myAes.Mode = CipherMode.CFB;

				// write salt to the begining of the output file, so in this case can be random every time
				fileStreamCrypt.Write(salt, 0, salt.Length);

				CryptoStream cryptoStream = new CryptoStream(fileStreamCrypt, myAes.CreateEncryptor(), CryptoStreamMode.Write);
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
		}

		/// <summary>
		/// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
		/// </summary>
		/// <param name="inputFile"></param>
		/// <param name="outputFile"></param>
		/// <param name="password"></param>
		private bool FileDecrypt(string inputFile, string outputFile, string password)
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

			using (Aes myAes = Aes.Create())
			{
				myAes.KeySize = 256;
				myAes.BlockSize = 128;
				var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000, HashAlgorithmName.SHA256);
				myAes.Key = key.GetBytes(myAes.KeySize / 8);
				myAes.IV = key.GetBytes(myAes.BlockSize / 8);
				myAes.Padding = PaddingMode.PKCS7;
				myAes.Mode = CipherMode.CFB;

				CryptoStream cs = new CryptoStream(fsCrypt, myAes.CreateDecryptor(), CryptoStreamMode.Read);

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
			}

			return isSuccess;
		}

		private bool DecryptMasterKeyFile()
		{
			if (!File.Exists(_coreApp.dataBank.DefaultMasterKeyFile + _coreApp.dataBank.EncryptionExtention))
			{
				return true;
			}

			bool isPasswordAccepted = false;

			while (!isPasswordAccepted)
			{
				UserPasswordHandler passwordHandler = new UserPasswordHandler();

				if (passwordHandler.canUnlockDatabase)
				{
					isPasswordAccepted = FileDecrypt(
						_coreApp.dataBank.DefaultMasterKeyFile + _coreApp.dataBank.EncryptionExtention,
						_coreApp.dataBank.DefaultMasterKeyFile,
						_coreApp.dataBank.UserPassword
					);
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Delete encrypted files when disabling encryption
		/// or reencrypt masterKey file on password change.
		/// </summary>
		public void UpdateEncryption()
		{
			if (_coreApp.dataBank.GetSetting("encryptDatabase") == "False")
			{
				Core.Instance.fileOperations.DeleteFile(_coreApp.dataBank.DefaultMasterKeyFile);
				Core.Instance.fileOperations.DeleteFile(_coreApp.dataBank.DefaultDBPath + _coreApp.dataBank.EncryptionExtention);
				Core.Instance.fileOperations.DeleteFile(_coreApp.dataBank.DefaultPIndexPath + _coreApp.dataBank.EncryptionExtention);
				Core.Instance.fileOperations.DeleteFile(_coreApp.dataBank.DefaultSIndexPath + _coreApp.dataBank.EncryptionExtention);
			}

			if (_coreApp.dataBank.UserPassword == string.Empty)
			{
				Core.Instance.fileOperations.DeleteFile(_coreApp.dataBank.DefaultMasterKeyFile + _coreApp.dataBank.EncryptionExtention);
			}
			else
			{
				FileEncrypt(_coreApp.dataBank.DefaultMasterKeyFile, _coreApp.dataBank.UserPassword);
			}
		}
	}
}
