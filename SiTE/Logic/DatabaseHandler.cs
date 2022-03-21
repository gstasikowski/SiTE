using CustomDatabase.Logic;
using CustomDatabase.Logic.Tree;
using SiTE.Logic.Serializers;
using SiTE.Models;
using System;
using System.Collections.Generic;
using System.IO;

// TODO add function to get all entries using primary index (for displaying note list)
namespace SiTE.Logic
{
    class DatabaseHandler : IDisposable
    {
        #region Variables
        readonly Stream mainDatabaseFile;
        readonly Stream primaryIndexFile;
        readonly Stream secondaryIndexFile;
        readonly Tree<Guid, uint> primaryIndex;
        readonly Tree<Tuple<string, int>, uint> secondaryIndex;
        readonly RecordStorage noteRecords;
        readonly NoteSerializer noteSerializer = new NoteSerializer();
        #endregion Variables

        #region Constructor
        public DatabaseHandler(string pathToDBFile)
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

            this.secondaryIndex = new Tree<Tuple<string, int>, uint>(
                new TreeDiskNodeManager<Tuple<string, int>, uint>(
                    new StringIntSerializer(),
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
            { throw new ObjectDisposedException("CowDatabase"); }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Insert new entry into database.
        /// </summary>
        public void Insert(NoteModel note)
        {
            if (disposed)
            { throw new ObjectDisposedException("CowDatabase"); }

            var recordID = this.noteRecords.Create(this.noteSerializer.Serialize(note));

            this.primaryIndex.Insert(note.ID, recordID);
            // TODO Change to change "0" to a note related property that can be used for search (add tags?).
            this.secondaryIndex.Insert(new Tuple<string, int>(note.Title, 0), recordID);
        }

        /// <summary>
        /// Find an entry by ID.
        /// </summary>
        public NoteModel Find(Guid ID)
        {
            if (disposed)
            { throw new ObjectDisposedException("CowDatabase"); }

            var entry = this.primaryIndex.Get(ID);

            if (entry == null)
            { return null; }

            return this.noteSerializer.Deserialize(this.noteRecords.Find(entry.Item2));
        }

        /// <summary>
        /// Find all entries within parameteres.
        /// </summary>
        // TODO Replace age with a note related property.
        public IEnumerable<NoteModel> FindBy(string title, int age)
        {
            var comparer = Comparer<Tuple<string, int>>.Default;
            var searchKey = new Tuple<string, int>(title, age);

            foreach (var entry in this.secondaryIndex.LargerThanOrEqualTo(searchKey))
            {
                // Stop upon reaching key larger than provided
                if (comparer.Compare(entry.Item1, searchKey) > 0)
                { break; }

                yield return this.noteSerializer.Deserialize(this.noteRecords.Find(entry.Item2));
            }
        }

        /// <summary>
        /// Delete specified entry from database.
        /// </summary>
        // TODO Implement note deletion.
        public void Delete(NoteModel note)
        {
            throw new NotImplementedException();
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

        ~DatabaseHandler()
        {
            Dispose(false);
        }
        #endregion Dispose
    }
}
