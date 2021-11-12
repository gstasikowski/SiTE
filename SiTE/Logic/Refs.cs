using SiTE.Models;

namespace SiTE.Logic
{
    public static class Refs
    {
        public static ApplicationViewModel viewControl;
        public static DataBank dataBank = new DataBank();
        public static FileOperations fileOperations = new FileOperations();
        public static LocalizationHandler localizationHandler = new LocalizationHandler();
    }
}
