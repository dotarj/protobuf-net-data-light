// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ProtoBuf.Data.Light.Tests
{
    public class DataReaderMock : IDataReader
    {
        private readonly DataTable schemaTable;
        private readonly List<List<object[]>> values;

        private int currentRow = -1;
        private int currentResult = 0;

        public DataReaderMock(bool multtpleResults)
        {
            this.schemaTable = this.BuildSchemaTable();

            if (multtpleResults)
            {
                this.values = new List<List<object[]>>() { this.GetValues().ToList(), this.GetValues().Reverse().ToList() };
            }
            else
            {
                this.values = new List<List<object[]>>() { this.GetValues().ToList() };
            }
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public int RecordsAffected
        {
            get { return 1; }
        }

        public int FieldCount
        {
            get { return this.values[this.currentResult][0].Length; }
        }

        public object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public object this[int i]
        {
            get
            {
                return this.values[this.currentResult][this.currentRow][i];
            }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            return this.schemaTable;
        }

        public bool NextResult()
        {
            if (this.currentResult < this.values.Count - 1)
            {
                this.currentResult++;

                return true;
            }

            return false;
        }

        public bool Read()
        {
            if (this.currentRow < this.values[this.currentResult].Count - 1)
            {
                this.currentRow++;

                return true;
            }

            return false;
        }

        public void Dispose()
        {
        }

        public bool GetBoolean(int i)
        {
            return (bool)this.values[this.currentResult][this.currentRow][i];
        }

        public byte GetByte(int i)
        {
            return (byte)this.values[this.currentResult][this.currentRow][i];
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            var bytes = (byte[])this.values[this.currentResult][this.currentRow][i];

            length = Math.Min(length, bytes.Length - (int)fieldOffset);

            Array.Copy(bytes, fieldOffset, buffer, bufferoffset, length);

            return length;
        }

        public char GetChar(int i)
        {
            return (char)this.values[this.currentResult][this.currentRow][i];
        }

        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
        {
            var chars = (char[])this.values[this.currentResult][this.currentRow][i];

            length = Math.Min(length, chars.Length - (int)fieldOffset);

            Array.Copy(chars, fieldOffset, buffer, bufferoffset, length);

            return length;
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return this.schemaTable.Rows[i]["DataTypeName"].ToString();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)this.values[this.currentResult][this.currentRow][i];
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)this.values[this.currentResult][this.currentRow][i];
        }

        public double GetDouble(int i)
        {
            return (double)this.values[this.currentResult][this.currentRow][i];
        }

        public Type GetFieldType(int i)
        {
            return (Type)this.schemaTable.Rows[i]["DataType"];
        }

        public float GetFloat(int i)
        {
            return (float)this.values[this.currentResult][this.currentRow][i];
        }

        public Guid GetGuid(int i)
        {
            return (Guid)this.values[this.currentResult][this.currentRow][i];
        }

        public short GetInt16(int i)
        {
            return (short)this.values[this.currentResult][this.currentRow][i];
        }

        public int GetInt32(int i)
        {
            return (int)this.values[this.currentResult][this.currentRow][i];
        }

        public long GetInt64(int i)
        {
            return (long)this.values[this.currentResult][this.currentRow][i];
        }

        public string GetName(int i)
        {
            return this.schemaTable.Rows[i]["ColumnName"].ToString();
        }

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < this.schemaTable.Rows.Count; i++)
            {
                if (this.schemaTable.Rows[i]["ColumnName"].ToString() == name)
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException(name);
        }

        public string GetString(int i)
        {
            return (string)this.values[this.currentResult][this.currentRow][i];
        }

        public object GetValue(int i)
        {
            return this.values[this.currentResult][this.currentRow][i];
        }

        public int GetValues(object[] values)
        {
            var length = values.Length < this.FieldCount ? values.Length : this.FieldCount;

            for (var i = 0; i < length; i++)
            {
                values[i] = this.values[this.currentResult][this.currentRow][i];
            }

            return length;
        }

        public bool IsDBNull(int i)
        {
            return this.values[this.currentResult][this.currentRow][i] == null;
        }

        private DataTable BuildSchemaTable()
        {
            var schemaTable = new DataTable("SchemaTable")
            {
                Locale = CultureInfo.InvariantCulture
            };

            var columnName = new DataColumn("ColumnName", typeof(string));
            var ordinal = new DataColumn("ColumnOrdinal", typeof(int)) { DefaultValue = 0 };
            var dataType = new DataColumn("DataType", typeof(Type));
            var dataTypeName = new DataColumn("DataTypeName", typeof(string));

            schemaTable.Columns.Add(columnName);
            schemaTable.Columns.Add(ordinal);
            schemaTable.Columns.Add(dataType);
            schemaTable.Columns.Add(dataTypeName);

            this.AddSchemaRow(schemaTable, "bool", 0, typeof(bool));
            this.AddSchemaRow(schemaTable, "byte", 1, typeof(byte));
            this.AddSchemaRow(schemaTable, "bytearray", 2, typeof(byte[]));
            this.AddSchemaRow(schemaTable, "char", 3, typeof(char));
            this.AddSchemaRow(schemaTable, "chararray", 4, typeof(char[]));
            this.AddSchemaRow(schemaTable, "DateTime", 5, typeof(DateTime));
            this.AddSchemaRow(schemaTable, "decimal", 6, typeof(decimal));
            this.AddSchemaRow(schemaTable, "double", 7, typeof(double));
            this.AddSchemaRow(schemaTable, "float", 8, typeof(float));
            this.AddSchemaRow(schemaTable, "Guid", 9, typeof(Guid));
            this.AddSchemaRow(schemaTable, "int", 10, typeof(int));
            this.AddSchemaRow(schemaTable, "long", 11, typeof(long));
            this.AddSchemaRow(schemaTable, "short", 12, typeof(short));
            this.AddSchemaRow(schemaTable, "string", 13, typeof(string));
            this.AddSchemaRow(schemaTable, "TimeSpan", 14, typeof(TimeSpan));
            this.AddSchemaRow(schemaTable, "string (null)", 15, typeof(string));

            foreach (DataColumn column in schemaTable.Columns)
            {
                column.ReadOnly = true;
            }

            return schemaTable;
        }

        private void AddSchemaRow(DataTable schemaTable, string columnName, int columnIndex, Type dataType)
        {
            var schemaRow = schemaTable.NewRow();

            schemaRow["ColumnName"] = columnName;
            schemaRow["ColumnOrdinal"] = columnIndex;
            schemaRow["DataType"] = dataType;
            schemaRow["DataTypeName"] = dataType.Name;

            schemaTable.Rows.Add(schemaRow);
        }

        private IEnumerable<object[]> GetValues()
        {
            yield return new object[]
            {
                true,
                byte.MinValue,
                Encoding.UTF8.GetBytes("bytes min"),
                char.MinValue,
                "chars min".ToArray(),
                DateTime.MinValue,
                decimal.MinValue,
                double.MinValue,
                float.MinValue,
                new Guid("00000000-0000-0000-0000-000000000000"),
                int.MinValue,
                long.MinValue,
                short.MinValue,
                "string min",
                TimeSpan.MinValue,
                null
            };

            yield return new object[]
            {
                true,
                byte.MaxValue,
                Encoding.UTF8.GetBytes("bytes max"),
                char.MaxValue,
                "chars max".ToArray(),
                DateTime.MaxValue,
                decimal.MaxValue,
                double.MaxValue,
                float.MaxValue,
                new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                int.MaxValue,
                long.MaxValue,
                short.MaxValue,
                "string max",
                TimeSpan.MaxValue,
                null
            };
        }
    }
}
