using System;
using System.Linq;
using System.Collections.Generic;

namespace SiTE.Logic
{
	public class DatabaseOperations
	{
		private Core _coreApp;

		public DatabaseOperations(Core coreApp)
		{
			_coreApp = coreApp;
		}

		public void GetNoteList()
		{
			using (var database = new NoteDatabase(_coreApp.dataBank.DefaultDBPath))
			{
				List<Models.NoteModel> noteList = new List<Models.NoteModel>(database.GetAll());
				_coreApp.dataBank.NoteList.Clear();

				if (Settings.Instance.SortNotesByDate)
				{
					noteList = noteList.OrderBy(note => note.Modified).ToList();
				}
				else
				{
					noteList = noteList.OrderBy(note => note.Title).ToList();
				}

				if (Settings.Instance.SortNotesDescending)
				{
					noteList.Reverse();
				}

				foreach (var note in noteList)
				{
					_coreApp.dataBank.NoteList.Add(note);
				}
			}
		}

		public Models.NoteModel LoadNote(Guid noteID)
		{
			using (var database = new NoteDatabase(_coreApp.dataBank.DefaultDBPath))
			{ return database.Find(noteID); }
		}

		public void SaveNote(Guid noteID, string noteTitle, string text)
		{
			using (var database = new NoteDatabase(_coreApp.dataBank.DefaultDBPath))
			{
				Guid noteGuid = (noteID == Guid.Empty) ? Guid.NewGuid() : noteID;
				var oldNote = database.Find(noteGuid);

				Models.NoteModel freshNote = new Models.NoteModel
				{
					ID = noteGuid,
					Title = noteTitle,
					Content = text,
					Modified = DateTime.Now
				};

				if (oldNote == null)
				{
					freshNote.Created = DateTime.Now;
					database.Insert(freshNote);
				}
				else
				{
					freshNote.Created = oldNote.Created;
					database.Update(freshNote);
				}
			}

			_coreApp.encryptionOperations.EncryptDatabase();
		}

		public void DeleteNote(Guid noteID)
		{
			using (var database = new NoteDatabase(_coreApp.dataBank.DefaultDBPath))
			{
				var noteToRemove = database.Find(noteID);
				database.Delete(noteToRemove);
			}

			_coreApp.encryptionOperations.EncryptDatabase();
		}
	}
}
