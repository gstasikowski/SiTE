using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SiTE.Models
{
	public class NoteModel : INotifyPropertyChanged
	{
		private Guid _id;
		private string _title;
		private string _content;
		private DateTime _created;
		private DateTime _modified;

		public event PropertyChangedEventHandler PropertyChanged;

		public NoteModel()
		{
			ID = System.Guid.Empty;
			Title = string.Empty;
			Content = string.Empty;
		}

		public Guid ID
		{
			get { return _id; }
			set
			{
				_id = value;
				OnPropertyChanged();
			}
		}

		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				OnPropertyChanged();
			}
		}

		public string Content
		{
			get { return _content; }
			set
			{
				_content = value;
				OnPropertyChanged();
			}
		}

		public DateTime Created
		{
			get { return _created; }
			set
			{
				_created = value;
				OnPropertyChanged();
			}
		}

		public DateTime Modified
		{
			get { return _modified; }
			set
			{
				_modified = value;
				OnPropertyChanged();
			}
		}
	 
	   protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
