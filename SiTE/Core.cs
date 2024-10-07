using SiTE.Logic;
using SiTE.Models;

namespace SiTE
{
	public class Core
	{
		public DataBank dataBank;
		public FileOperations fileOperations;
		public EncryptionOperations encryptionOperations;
		public DatabaseOperations databaseOperations;

		public static Core Instance { get; set; } = new Core();

		public Core()
		{
			dataBank = new DataBank();
			encryptionOperations = new EncryptionOperations(this);
			databaseOperations = new DatabaseOperations(this);
			fileOperations = new FileOperations(this, dataBank);
			fileOperations.InitialSetup();
		}

		public void InitializeAppComponents()
		{
			Settings.Instance.ApplyUserSettings();
		}
	}
}
