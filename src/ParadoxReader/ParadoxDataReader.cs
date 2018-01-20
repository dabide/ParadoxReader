using System;
using System.Collections.Generic;
using System.Data;

namespace ParadoxReader
{
    public class ParadoxDataReader : IDataReader
    {
        private readonly IEnumerator<ParadoxRecord> _enumerator;

        public ParadoxDataReader(ParadoxFile file, IEnumerable<ParadoxRecord> query)
        {
            File = file;
            _enumerator = query.GetEnumerator();
        }

        public ParadoxFile File { get; }

        public ParadoxRecord CurrentRecord => _enumerator.Current;

        public void Dispose()
        {
        }

        public string GetName(int i)
        {
            return File.FieldNames[i];
        }

        public string GetDataTypeName(int colIndex)
        {
            return "pxf" + File.FieldTypes[colIndex].FType;
        }

        public Type GetFieldType(int colIndex)
        {
            ParadoxFile.FieldInfo fInfo = File.FieldTypes[colIndex];
            switch (fInfo.FType)
            {
                case ParadoxFieldTypes.Alpha:
                case ParadoxFieldTypes.MemoBLOb:
                    return typeof(string);
                case ParadoxFieldTypes.Short:
                    return typeof(short);
                case ParadoxFieldTypes.Long:
                    return typeof(uint);
                case ParadoxFieldTypes.Currency:
                    return typeof(double);
                case ParadoxFieldTypes.Number:
                    return typeof(double);
                case ParadoxFieldTypes.Date:
                    return typeof(DateTime);
                case ParadoxFieldTypes.Timestamp:
                    return typeof(DateTime);
                default:
                    throw new NotSupportedException();
            }
        }

        public object GetValue(int i)
        {
            return CurrentRecord.DataValues[i];
        }

        public int GetValues(object[] values)
        {
            return 0;
        }

        public int GetOrdinal(string name)
        {
            return Array.FindIndex(File.FieldNames,
                                   f => string.Equals(f, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool GetBoolean(int i)
        {
            return (bool) GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte) GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            return (short) GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int) GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long) GetValue(i);
        }

        public float GetFloat(int i)
        {
            return (float) GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double) GetValue(i);
        }

        public string GetString(int i)
        {
            return (string) GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal) GetValue(i);
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime) GetValue(i);
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i) == DBNull.Value;
        }

        public int FieldCount => File.FieldCount;

        public object this[int i] => GetValue(i);

        public object this[string name] => GetValue(GetOrdinal(name));

        public void Close()
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            return _enumerator.MoveNext();
        }

        public int Depth => 0;

        public bool IsClosed => false;

        public int RecordsAffected => 0;
    }
}
