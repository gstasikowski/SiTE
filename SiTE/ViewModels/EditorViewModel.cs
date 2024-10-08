using Avalonia.Controls.ApplicationLifetimes;
using System.Collections.ObjectModel;

namespace SiTE.ViewModels
{
	public class EditorViewModel : ViewModelBase
	{
		public ObservableCollection<Models.NoteModel> NoteList
		{
			get { return DB.NoteList; }
		}

		public Models.NoteModel ActiveNote
		{
			get { return DB.ActiveNote; }
			set { DB.ActiveNote = value; }
		}

		public bool IsNoteModified
		{
			get { return DB.IsNoteModified; }
			set { DB.IsNoteModified = value; }
		}

		public void NewNote()
		{
			DB.NewNote();
		}

		public void OpenNote(System.Guid noteID)
		{
			DB.OpenNote(noteID);
		}

		public void SaveNote()
		{
			DB.SaveNote();
		}

		public void DeleteNote()
		{
			DB.DeleteNote();
		}

		public void SwitchToSettingsView()
		{
			((ViewModels.MainWindowViewModel)((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.DataContext).ToggleActiveScreen();
		}

		public void OpenAppRepo()
		{
			new Logic.NoteLinkCommand().Execute(SiTE.Core.Instance.dataBank.projectUrl);
		}

		public void DisplayAppInfo()
		{
			string message = string.Format("{0}\n\nVersion: {1}", "SiTE"/*App.ResourceAssembly.GetName().Name*/, "2.0"/*Avalonia.Application.ResourceAssembly.GetName().Version*/);

			Views.ErrorMessageView messageWindow = new Views.ErrorMessageView(message, string.Empty);
			messageWindow.Title = (string)Logic.Localizer.Instance["MenuAbout"];
			messageWindow.ShowDialog(((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow);
		}

		private Models.DataBank DB
		{
			get { return SiTE.Core.Instance.dataBank; }
		}
	}
}