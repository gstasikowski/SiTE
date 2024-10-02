using System.Windows.Input;
using DynamicData;
using ReactiveUI;
using SiTE.Logic;
using SiTE.Views;

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

			ToggleActiveScreenCommand = ReactiveCommand.Create(ToggleActiveScreen);
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

		/// <summary>
		/// Gets a command that toggles between generator and settings pages
		/// </summary>
		public ICommand ToggleActiveScreenCommand { get; }

		private void ToggleActiveScreen()
		{
			int index = (Pages.IndexOf(CurrentPage) > 0) ? 0 : 1;
			CurrentPage = Pages[index];
		}

		public string AppTitle
		{
			get { return "SiTE"; } // TODO: append * marking for note with unsaved changes
		}
	}
}
