using System;

namespace SiTE.Logic
{
    public static class DatabaseOperations
    {
        public static void GetNoteList()
        {
            using (var database = new NoteDatabase(Refs.dataBank.DefaultDBPath))
            {
                var noteList = database.GetAll();
                Refs.dataBank.NoteList.Clear();

                foreach (var note in noteList)
                {
                    Refs.dataBank.NoteList.Add(note);
                }
            }
        }

        public static Models.NoteModel LoadNote(Guid noteID)
        {
            using (var database = new NoteDatabase(Refs.dataBank.DefaultDBPath))
            { return database.Find(noteID); }
        }

        public static void SaveNote(Guid noteID, string noteTitle, string text)
        {
            using (var database = new NoteDatabase(Refs.dataBank.DefaultDBPath))
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

            EncryptionOperations.EncryptDatabase();
        }

        public static void DeleteNote(Guid noteID)
        {
            using (var database = new NoteDatabase(Refs.dataBank.DefaultDBPath))
            {
                var noteToRemove = database.Find(noteID);
                database.Delete(noteToRemove);
            }

            EncryptionOperations.EncryptDatabase();
        }
    }
}
