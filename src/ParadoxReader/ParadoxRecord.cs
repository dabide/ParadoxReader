using System;
using System.IO;
using System.Text;

namespace ParadoxReader
{
    public class ParadoxRecord
    {
        internal readonly ParadoxFile.DataBlock Block;
        private readonly int _recIndex;

        private object[] _data;

        internal ParadoxRecord(ParadoxFile.DataBlock block, int recIndex)
        {
            Block = block;
            _recIndex = recIndex;
        }

        public object[] DataValues
        {
            get
            {
                if (_data == null)
                {
                    MemoryStream buff = new MemoryStream(Block.Data) {Position = Block.File.RecordSize * _recIndex};
                    using (BinaryReader r = new BinaryReader(buff, Encoding.Default))
                    {
                        _data = new object[Block.File.FieldCount];
                        for (int colIndex = 0; colIndex < _data.Length; colIndex++)
                        {
                            ParadoxFile.FieldInfo fInfo = Block.File.FieldTypes[colIndex];
                            int dataSize = fInfo.FType == ParadoxFieldTypes.BCD ? 17 : fInfo.FSize;
                            bool empty = true;
                            for (int i = 0; i < dataSize; i++)
                            {
                                if (Block.Data[buff.Position + i] != 0)
                                {
                                    empty = false;
                                    break;
                                }
                            }

                            if (empty)
                            {
                                _data[colIndex] = DBNull.Value;
                                buff.Position += dataSize;
                                continue;
                            }

                            object val;
                            switch (fInfo.FType)
                            {
                                case ParadoxFieldTypes.Alpha:
                                    val = Block.File.GetString(Block.Data, (int) buff.Position, dataSize);
                                    buff.Position += dataSize;
                                    break;
                                case ParadoxFieldTypes.MemoBLOb:
                                    val = Block.File.GetStringFromMemo(Block.Data, (int) buff.Position, dataSize);
                                    buff.Position += dataSize;
                                    break;
                                case ParadoxFieldTypes.Short:
                                    ConvertBytes((int) buff.Position, dataSize);
                                    val = r.ReadInt16();
                                    break;
                                case ParadoxFieldTypes.Long:
                                case ParadoxFieldTypes.AutoInc:
                                    ConvertBytes((int) buff.Position, dataSize);
                                    val = r.ReadInt32();
                                    break;
                                case ParadoxFieldTypes.Currency:
                                    ConvertBytes((int) buff.Position, dataSize);
                                    val = r.ReadDouble();
                                    break;
                                case ParadoxFieldTypes.Number:
                                    ConvertBytesNum((int) buff.Position, dataSize);
                                    double dbl = r.ReadDouble();
                                    val = double.IsNaN(dbl) ? (object) DBNull.Value : dbl;
                                    break;
                                case ParadoxFieldTypes.Date:
                                    ConvertBytes((int) buff.Position, dataSize);
                                    int days = r.ReadInt32();
                                    val = new DateTime(1, 1, 1).AddDays(days - 1);
                                    break;
                                case ParadoxFieldTypes.Timestamp:
                                    ConvertBytes((int) buff.Position, dataSize);
                                    double ms = r.ReadDouble();
                                    val = new DateTime(1, 1, 1).AddMilliseconds(ms).AddDays(-1);
                                    break;
                                case ParadoxFieldTypes.Time:
                                    ConvertBytes((int) buff.Position, dataSize);
                                    val = TimeSpan.FromMilliseconds(r.ReadInt32());
                                    break;
                                case ParadoxFieldTypes.Logical:
                                    // False is stored as 128, and True looks like 129.
                                    val = Block.Data[(int) buff.Position] - 128 > 0;
                                    buff.Position += dataSize;
                                    break;
                                case ParadoxFieldTypes.BLOb:
                                    byte[] blobInfo = new byte[dataSize];
                                    r.Read(blobInfo, 0, dataSize);
                                    val = Block.File.ReadBlob(blobInfo);
                                    break;
                                default:
                                    val = null; // not supported
                                    buff.Position += dataSize;
                                    break;
                            }

                            _data[colIndex] = val;
                        }
                    }
                }

                return _data;
            }
        }

        private void ConvertBytes(int start, int length)
        {
            Block.Data[start] = (byte) (Block.Data[start] ^ 0x80);
            Array.Reverse(Block.Data, start, length);
        }

        private void ConvertBytesNum(int start, int length) /* amk */
        {
            if ((Block.Data[start] & 0x80) != 0)
            {
                Block.Data[start] = (byte) (Block.Data[start] & 0x7F);
            }
            else if (Block.Data[start + 0] == 0 &&
                     Block.Data[start + 1] == 0 &&
                     Block.Data[start + 2] == 0 &&
                     Block.Data[start + 3] == 0 &&
                     Block.Data[start + 4] == 0 &&
                     Block.Data[start + 5] == 0 &&
                     Block.Data[start + 6] == 0 &&
                     Block.Data[start + 7] == 0)
            {
                /* sorry, did not check lenght */
            }

            else
            {
                for (int i = 0; i < 8; i++)
                {
                    Block.Data[start + i] = (byte) ~Block.Data[start + i];
                }
            }

            Array.Reverse(Block.Data, start, length);
        }
    }
}
