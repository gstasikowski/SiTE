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

		private Models.DataBank DB
		{
			get { return SiTE.Core.Instance.dataBank; }
		}
	}
}