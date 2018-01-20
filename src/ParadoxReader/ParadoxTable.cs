using System;
using System.IO;

namespace ParadoxReader
{
    public class ParadoxTable : ParadoxFile
    {
        private readonly ParadoxBlobFile _blobFile;
        public readonly ParadoxPrimaryKey PrimaryKeyIndex;

        public ParadoxTable(string dbPath, string tableName) : base(Path.Combine(dbPath, tableName + ".db"))
        {
            string[] files = Directory.GetFiles(dbPath, tableName + "*.*");
            foreach (string file in files)
            {
                if (Path.GetFileName(file) == tableName + ".db")
                {
                    continue; // current file
                }

                if (Path.GetFileNameWithoutExtension(file).EndsWith(".PX", StringComparison.InvariantCultureIgnoreCase) ||
                    Path.GetExtension(file).Equals(".PX", StringComparison.InvariantCultureIgnoreCase))
                {
                    PrimaryKeyIndex = new ParadoxPrimaryKey(this, file);
                    break;
                }

                if (Path.GetFileNameWithoutExtension(file).EndsWith(".MB", StringComparison.InvariantCultureIgnoreCase) ||
                    Path.GetExtension(file).Equals(".MB", StringComparison.InvariantCultureIgnoreCase))
                {
                    _blobFile = new ParadoxBlobFile(file);
                }
            }
        }

        internal override byte[] ReadBlob(byte[] blobInfo)
        {
            if (_blobFile == null)
            {
                return base.ReadBlob(blobInfo);
            }

            return _blobFile.ReadBlob(blobInfo);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (PrimaryKeyIndex != null)
            {
                PrimaryKeyIndex.Dispose();
            }

            if (_blobFile != null)
            {
                _blobFile.Dispose();
            }
        }
    }
}
