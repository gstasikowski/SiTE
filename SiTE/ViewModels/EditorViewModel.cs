using System.Collections.ObjectModel;

namespace SiTE.ViewModels
{
	public class EditorViewModel : ViewModelBase
	{
		public ObservableCollection<Models.NoteModel> NoteList
		{
			get { return Logic.Refs.dataBank.NoteList; }
		}
	}
}