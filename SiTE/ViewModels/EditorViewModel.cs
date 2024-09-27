using System.Collections.ObjectModel;

namespace SiTE.ViewModels
{
	public class EditorViewModel : ViewModelBase
	{
		public ObservableCollection<Models.NoteModel> NoteList
		{
			get { return Logic.Refs.dataBank.NoteList; }
		}

		public Models.NoteModel ActiveNote
		{
			get { return Logic.Refs.dataBank.ActiveNote; }
			set { Logic.Refs.dataBank.ActiveNote = value; }
		}

		public void NewNote()
		{
			Logic.Refs.dataBank.NewNote();
		}

		public void OpenNote(System.Guid noteID)
		{
			Logic.Refs.dataBank.OpenNote(noteID);
		}

		public void SaveNote()
		{
			Logic.Refs.dataBank.SaveNote();
		}

		public void DeleteNote()
		{
			Logic.Refs.dataBank.DeleteNote();
		}
	}
}