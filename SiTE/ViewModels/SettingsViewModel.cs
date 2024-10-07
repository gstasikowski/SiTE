using System.Collections.ObjectModel;
using SiTE.Logic;

namespace SiTE.ViewModels
{
	public class SettingsViewModel : ViewModelBase
	{
		public ObservableCollection<string> LanguageList
		{
			get { return Localizer.Instance.LanguageList; }
		}

		public Settings AppSettings
		{
			get { return Settings.Instance; }
		}
	}
}