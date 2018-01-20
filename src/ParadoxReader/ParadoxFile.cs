using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ParadoxReader
{
    public class ParadoxFile : IDisposable
    {
        private readonly BinaryReader _reader;

        private readonly Stream _stream;
        private int _autoIncVal; //  longint;
        private byte _auxPasswords;
        private byte _changeCount1;
        private byte _changeCount2;
        private int _cryptInfoEndPtr;
        private int _cryptInfoStartPtr; //  pointer;
        private int _encryption1;
        private int[] _fieldNamePtrArray;
        private ushort _fileBlocks;
        private byte _fileVersionId;
        private ushort _firstBlock;
        private int _fldInfoPtr; //  PFldInfoRec;
        private ushort _headerSize;
        private byte _indexFieldNumber;
        private byte _indexUpdateRequired;
        private ushort _lastBlock;
        private ushort _maxBlocks;
        private byte _maxTableSize;
        private byte _modifiedFlags1;
        private byte _modifiedFlags2;
        private ushort _nextBlock;
        private int _primaryIndexWorkspace;
        private short _primaryKeyFields;
        protected byte PxLevelCount;
        protected ushort PxRootBlockId;
        private byte _refIntegrity;
        private byte _sortOrder;
        public string TableName;
        private int _tableNamePtr;
        private int _tableNamePtrPtr; // ^pchar;
        private ushort _unknown12X13;
        private byte[] _unknown2Bx2C; //  array[$002B..$002C] of byte;
        private byte _unknown2F;
        private byte _unknown3C;
        private byte[] _unknown3Ex3F; //  array[$003E..$003F] of byte;
        private byte _unknown48;
        private byte[] _unknown4Dx4E; //array[$004D..$004E] of byte;
        private byte[] _unknown50X54; //array[$0050..$0054] of byte;
        private byte[] _unknown56X57; //array[$0056..$0057] of byte;
        private int _unknownPtr1A;
        private V4Hdr _v4Header;
        private byte _writeProtected;

        public ParadoxFile(string fileName) : this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
        }

        public ParadoxFile(Stream stream)
        {
            _stream = stream;
            _reader = new BinaryReader(stream);
            stream.Position = 0;
            ReadHeader();
        }

        public ushort RecordSize { get; private set; }
        public ParadoxFileType FileType { get; private set; }
        public int RecordCount { get; private set; }
        public short FieldCount { get; private set; }
        internal FieldInfo[] FieldTypes { get; set; } // array[1..255] of TFldInfoRec);
        public string[] FieldNames { get; private set; }

        public virtual void Dispose()
        {
            _stream.Dispose();
        }

        internal virtual byte[] ReadBlob(byte[] blobInfo)
        {
            return null;
        }

        public IEnumerable<ParadoxRecord> Enumerate(Predicate<ParadoxRecord> where = null)
        {
            for (int blockId = 0; blockId < _fileBlocks; blockId++)
            {
                DataBlock block = GetBlock(blockId);
                for (int recId = 0; recId < block.RecordCount; recId++)
                {
                    ParadoxRecord rec = block[recId];
                    if (where == null || where(rec))
                    {
                        yield return rec;
                    }
                }
            }
        }

        private void ReadHeader()
        {
            BinaryReader r = _reader;
            RecordSize = r.ReadUInt16();
            _headerSize = r.ReadUInt16();
            FileType = (ParadoxFileType) r.ReadByte();
            _maxTableSize = r.ReadByte();
            RecordCount = r.ReadInt32();
            _nextBlock = r.ReadUInt16();
            _fileBlocks = r.ReadUInt16();
            _firstBlock = r.ReadUInt16();
            _lastBlock = r.ReadUInt16();
            _unknown12X13 = r.ReadUInt16();
            _modifiedFlags1 = r.ReadByte();
            _indexFieldNumber = r.ReadByte();
            _primaryIndexWorkspace = r.ReadInt32();
            _unknownPtr1A = r.ReadInt32();
            PxRootBlockId = r.ReadUInt16();
            PxLevelCount = r.ReadByte();
            FieldCount = r.ReadInt16();
            _primaryKeyFields = r.ReadInt16();
            _encryption1 = r.ReadInt32();
            _sortOrder = r.ReadByte();
            _modifiedFlags2 = r.ReadByte();
            _unknown2Bx2C = r.ReadBytes(0x002C - 0x002B + 1);
            _changeCount1 = r.ReadByte();
            _changeCount2 = r.ReadByte();
            _unknown2F = r.ReadByte();
            _tableNamePtrPtr = r.ReadInt32(); // ^pchar;
            _fldInfoPtr = r.ReadInt32(); //  PFldInfoRec;
            _writeProtected = r.ReadByte();
            _fileVersionId = r.ReadByte();
            _maxBlocks = r.ReadUInt16();
            _unknown3C = r.ReadByte();
            _auxPasswords = r.ReadByte();
            _unknown3Ex3F = r.ReadBytes(0x003F - 0x003E + 1);
            _cryptInfoStartPtr = r.ReadInt32(); //  pointer;
            _cryptInfoEndPtr = r.ReadInt32();
            _unknown48 = r.ReadByte();
            _autoIncVal = r.ReadInt32(); //  longint;
            _unknown4Dx4E = r.ReadBytes(0x004E - 0x004D + 1);
            _indexUpdateRequired = r.ReadByte();
            _unknown50X54 = r.ReadBytes(0x0054 - 0x0050 + 1);
            _refIntegrity = r.ReadByte();
            _unknown56X57 = r.ReadBytes(0x0057 - 0x0056 + 1);

            if ((FileType == ParadoxFileType.DbFileIndexed ||
                 FileType == ParadoxFileType.DbFileNotIndexed ||
                 FileType == ParadoxFileType.XnnFileInc ||
                 FileType == ParadoxFileType.XnnFileNonInc) &&
                _fileVersionId >= 5)
            {
                _v4Header = new V4Hdr(r);
            }

            List<FieldInfo> buff = new List<FieldInfo>();
            for (int i = 0; i < FieldCount; i++)
            {
                buff.Add(new FieldInfo(r));
            }

            if (FileType == ParadoxFileType.PxFile)
            {
                FieldCount += 3;
                buff.Add(new FieldInfo(ParadoxFieldTypes.Short, sizeof(short)));
                buff.Add(new FieldInfo(ParadoxFieldTypes.Short, sizeof(short)));
                buff.Add(new FieldInfo(ParadoxFieldTypes.Short, sizeof(short)));
            }

            FieldTypes = buff.ToArray();
            _tableNamePtr = r.ReadInt32();
            if (FileType == ParadoxFileType.DbFileIndexed ||
                FileType == ParadoxFileType.DbFileNotIndexed)
            {
                _fieldNamePtrArray = new int[FieldCount];
                for (int i = 0; i < FieldCount; i++)
                {
                    _fieldNamePtrArray[i] = r.ReadInt32();
                }
            }

            byte[] tableNameBuff = r.ReadBytes(_fileVersionId >= 0x0C ? 261 : 79);
            TableName = Encoding.ASCII.GetString(tableNameBuff, 0, Array.FindIndex(tableNameBuff, b => b == 0));
            if (FileType == ParadoxFileType.DbFileIndexed ||
                FileType == ParadoxFileType.DbFileNotIndexed)
            {
                FieldNames = new string[FieldCount];
                for (int i = 0; i < FieldCount; i++)
                {
                    StringBuilder fldNameBuff = new StringBuilder();
                    char ch;
                    while ((ch = r.ReadChar()) != '\x00')
                    {
                        fldNameBuff.Append(ch);
                    }

                    FieldNames[i] = fldNameBuff.ToString();
                }
            }
        }

        internal DataBlock GetBlock(int blockId)
        {
            _stream.Position = blockId * _maxTableSize * 0x0400 + _headerSize;
            return new DataBlock(this, _reader);
        }

        public string GetString(byte[] data, int from, int maxLength)
        {
            int stringLength = Array.FindIndex(data, from, b => b == 0) - from;
            if (stringLength > maxLength)
            {
                stringLength = maxLength;
            }

            return Encoding.Default.GetString(data, from, stringLength);
        }

        public string GetStringFromMemo(byte[] data, int from, int size)
        {
            int memoBufferSize = size - 10;
            byte[] memoDataBuffer = new byte[memoBufferSize];
            byte[] memoMetaData = new byte[10];
            Array.Copy(data, from, memoDataBuffer, 0, memoBufferSize);
            Array.Copy(data, from + memoBufferSize, memoMetaData, 0, 10);

            //var offsetIntoMemoFile = (long)BitConverter.ToInt32(memoMetaData, 0); 
            //offsetIntoMemoFile &= 0xffffff00;
            //var memoModNumber = BitConverter.ToInt16(memoMetaData,8); 
            //var index = memoMetaData[0]; 

            int memoSize = BitConverter.ToInt32(memoMetaData, 4);
            return GetString(memoDataBuffer, 0, memoSize);
        }

        public class V4Hdr
        {
            private short _changeCount4;
            private ushort _dosCodePage;
            private int _encryption2;
            private int _fileUpdateTime; // 4.0 only
            private short _fileVerId2;
            private short _fileVerId3;
            private ushort _hiFieldId;
            private ushort _hiFieldIDinfo;
            private short _sometimesNumFields;
            private byte[] _unknown6Cx6F; //array[$006C..$006F] of byte;
            private byte[] _unknown72X77; //    :  array[$0072..$0077] of byte;

            public V4Hdr(BinaryReader r)
            {
                _fileVerId2 = r.ReadInt16();
                _fileVerId3 = r.ReadInt16();
                _encryption2 = r.ReadInt32();
                _fileUpdateTime = r.ReadInt32(); // 4.0 only
                _hiFieldId = r.ReadUInt16();
                _hiFieldIDinfo = r.ReadUInt16();
                _sometimesNumFields = r.ReadInt16();
                _dosCodePage = r.ReadUInt16();
                _unknown6Cx6F = r.ReadBytes(0x006F - 0x006C + 1); //array[$006C..$006F] of byte;
                _changeCount4 = r.ReadInt16();
                _unknown72X77 = r.ReadBytes(0x0077 - 0x0072 + 1); //    :  array[$0072..$0077] of byte;
            }
        }

        internal class DataBlock
        {
            private ushort _blockNumber;
            public byte[] Data;
            public ParadoxFile File;
            private ushort _nextBlock;
            private readonly ParadoxRecord[] _recCache;

            public DataBlock(ParadoxFile file, BinaryReader r)
            {
                File = file;
                _nextBlock = r.ReadUInt16();
                _blockNumber = r.ReadUInt16();
                short addDataSize = r.ReadInt16();

                RecordCount = addDataSize / file.RecordSize + 1;
                Data = r.ReadBytes(RecordCount * file.RecordSize);
                _recCache = new ParadoxRecord[Data.Length];
            }

            public int RecordCount { get; }

            public ParadoxRecord this[int recIndex] => _recCache[recIndex] ?? (_recCache[recIndex] = new ParadoxRecord(this, recIndex));
        }

        internal class FieldInfo
        {
            public byte FSize;
            public ParadoxFieldTypes FType;

            public FieldInfo(ParadoxFieldTypes fType, byte fSize)
            {
                FType = fType;
                FSize = fSize;
            }

            public FieldInfo(BinaryReader r)
            {
                FType = (ParadoxFieldTypes) r.ReadByte();
                FSize = r.ReadByte();
            }
        }
    }
}
