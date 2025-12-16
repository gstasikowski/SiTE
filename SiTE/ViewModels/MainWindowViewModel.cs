using DynamicData;
using ReactiveUI;

namespace SiTE.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		// A read.only array of possible pages
		private ViewModelBase[] Pages;
		// The default is the first page
		private ViewModelBase _currentPage;

		public MainWindowViewModel()
		{
			InitializePages();
			_currentPage = Pages[0];
		}

		private void InitializePages()
		{
			Pages = new ViewModelBase[]
			{
				new EditorViewModel(),
				new SettingsViewModel()
			};
		}

		/// <summary>
		/// Gets the current page. The property is read-only
		/// </summary>
		public ViewModelBase CurrentPage
		{
			get { return _currentPage; }
			private set { this.RaiseAndSetIfChanged(ref _currentPage, value); }
		}

		public void ToggleActiveScreen()
		{
			int index = (Pages.IndexOf(CurrentPage) > 0) ? 0 : 1;
			CurrentPage = Pages[index];
		}

		public string AppTitle
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } // TODO: append * marking for note with unsaved changes
		}

		public string AppVersion
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
		}
	}
}
