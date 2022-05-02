using CustomDatabase.Logic;
using CustomDatabase.Logic.Tree;
using SiTE.Logic.Serializers;
using SiTE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SiTE.Logic
{
    class NoteDatabase : IDisposable
    {
        #region Variables
        readonly Stream mainDatabaseFile;
        readonly Stream primaryIndexFile;
        readonly Stream secondaryIndexFile;
        readonly Tree<Guid, uint> primaryIndex;
        readonly Tree<Tuple<string, string>, uint> secondaryIndex;
        readonly RecordStorage noteRecords;
        readonly NoteSerializer noteSerializer = new NoteSerializer();
        #endregion Variables

        #region Constructor
        public NoteDatabase(string pathToDBFile)
        {
            if (pathToDBFile == null)
            { throw new ArgumentNullException("pathToDBFile"); }

            // Open the stream and (create) database files.
            this.mainDatabaseFile = new FileStream(pathToDBFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);
            this.primaryIndexFile = new FileStream(pathToDBFile + ".pidx", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);
            this.secondaryIndexFile = new FileStream(pathToDBFile + ".sidx", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);

            // Create a RecordStorage for main cow data
            this.noteRecords = new RecordStorage(new BlockStorage(this.mainDatabaseFile, 4096, 48));

            // Create the indexes
            this.primaryIndex = new Tree<Guid, uint>(
                new TreeDiskNodeManager<Guid, uint>(
                    new GuidSerializer(),
                    new TreeUIntSerializer(),
                    new RecordStorage(new BlockStorage(this.primaryIndexFile, 4096))
                    ),
                false
            );

            this.secondaryIndex = new Tree<Tuple<string, string>, uint>(
                new TreeDiskNodeManager<Tuple<string, string>, uint>(
                    new StringSerializer(),
                    new TreeUIntSerializer(),
                    new RecordStorage(new BlockStorage(this.secondaryIndexFile, 4096))
                    ),
                true
            );
        }
        #endregion Constructor

        #region Methods (public)
        /// <summary>
        /// Update entry.
        /// </summary>
        public void Update(NoteModel note)
        {
            if (disposed)
            { throw new ObjectDisposedException("NoteDatabase"); }

            var entry = this.primaryIndex.Get(note.ID);

            if (entry == null)
            { return; }

            var temp = new Tuple<string, string>(Refs.dataBank.GetNoteTitle(note.ID), string.Empty);
            this.secondaryIndex.Delete(temp, entry.Item2);

            var serializedNote = this.noteSerializer.Serialize(note);
            this.noteRecords.Update(entry.Item2, serializedNote);
            this.secondaryIndex.Insert(new Tuple<string, string>(note.Title, string.Empty), entry.Item2);
        }

        /// <summary>
        /// Insert new entry into database.
        /// </summary>
        public void Insert(NoteModel note)
        {
            if (disposed)
            { throw new ObjectDisposedException("NoteDatabase"); }

            var recordID = this.noteRecords.Create(this.noteSerializer.Serialize(note));

            this.primaryIndex.Insert(note.ID, recordID);
            // TODO Change to change second string to a note related property that can be used for search (add tags?).
            this.secondaryIndex.Insert(new Tuple<string, string>(note.Title, string.Empty), recordID);
        }

        /// <summary>
        /// Find an entry by ID.
        /// </summary>
        public NoteModel Find(Guid ID)
        {
            if (disposed)
            { throw new ObjectDisposedException("NoteDatabase"); }

            var entry = this.primaryIndex.Get(ID);

            if (entry == null)
            { return null; }

            return this.noteSerializer.Deserialize(this.noteRecords.Find(entry.Item2));
        }

        /// <summary>
        /// Find all entries within parameteres.
        /// </summary>
        public IEnumerable<NoteModel> FindBy(string title, string tags)
        {
            var comparer = Comparer<Tuple<string, string>>.Default;
            var searchKey = new Tuple<string, string>(title, tags);

            foreach (var entry in this.secondaryIndex.LargerThanOrEqualTo(searchKey))
            {
                // Stop upon reaching key larger than provided
                if (comparer.Compare(entry.Item1, searchKey) > 0)
                { break; }

                yield return this.noteSerializer.Deserialize(this.noteRecords.Find(entry.Item2));
            }
        }

        /// <summary>
        /// Get all entries.
        /// </summary>
        public IEnumerable<NoteModel> GetAll()
        {
            var elements = this.primaryIndex.GetAll();

            foreach (var entry in elements)
            {
                if (entry == null)
                    break;

                // Using DeserializeSimple because we DON'T need all the note content when asking for note list
                yield return this.noteSerializer.DeserializeSimple(this.noteRecords.Find(entry.Item2));
            }
        }

        /// <summary>
        /// Delete specified entry from database.
        /// </summary>
        public void Delete(NoteModel note)
        {
            var entry = this.primaryIndex.Get(note.ID);

            if (entry == null)
            { return; }

            var temp = new Tuple<string, string>(note.Title, string.Empty);
            this.secondaryIndex.Delete(temp, entry.Item2);
            this.primaryIndex.Delete(note.ID);
            this.noteRecords.Delete(entry.Item2);
        }
        #endregion Methods (public)

        #region Dispose
        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                this.mainDatabaseFile.Dispose();
                this.primaryIndexFile.Dispose();
                this.secondaryIndexFile.Dispose();
                this.disposed = true;
            }
        }

        ~NoteDatabase()
        {
            Dispose(false);
        }
        #endregion Dispose
    }
}
