// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ProtoBuf.Data.Light.Test
{
    public class DataReaderMock : IDataReader
    {
        private readonly DataTable schemaTable;
        private readonly object[][] values;

        private int currentRow = -1;

        public DataReaderMock()
        {
            schemaTable = this.BuildSchemaTable();
            values = this.GetValues().ToArray();
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
            this.AddSchemaRow(schemaTable, "DateTime", 2, typeof(DateTime));
            this.AddSchemaRow(schemaTable, "double", 3, typeof(double));
            this.AddSchemaRow(schemaTable, "float", 4, typeof(float));
            this.AddSchemaRow(schemaTable, "Guid", 5, typeof(Guid));
            this.AddSchemaRow(schemaTable, "int", 6, typeof(int));
            this.AddSchemaRow(schemaTable, "long", 7, typeof(long));
            this.AddSchemaRow(schemaTable, "short", 8, typeof(short));
            this.AddSchemaRow(schemaTable, "string", 9, typeof(string));
            this.AddSchemaRow(schemaTable, "char", 10, typeof(char));
            this.AddSchemaRow(schemaTable, "decimal", 11, typeof(decimal));
            this.AddSchemaRow(schemaTable, "bytearray", 12, typeof(byte[]));
            this.AddSchemaRow(schemaTable, "chararray", 13, typeof(char[]));
            this.AddSchemaRow(schemaTable, "TimeSpan", 14, typeof(TimeSpan));

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
                DateTime.MinValue,
                double.MinValue,
                float.MinValue,
                new Guid("00000000-0000-0000-0000-000000000000"),
                int.MinValue,
                long.MinValue,
                short.MinValue,
                "string min",
                char.MinValue,
                decimal.MinValue,
                Encoding.UTF8.GetBytes("bytes min"),
                "chars min".ToArray(),
                TimeSpan.MinValue
            };

            yield return new object[]
            {
                true,
                byte.MaxValue,
                DateTime.MaxValue,
                double.MaxValue,
                float.MaxValue,
                new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                int.MaxValue,
                long.MaxValue,
                short.MaxValue,
                "string max",
                char.MaxValue,
                decimal.MaxValue,
                Encoding.UTF8.GetBytes("bytes max"),
                "chars max".ToArray(),
                TimeSpan.MaxValue
            };
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            return this.schemaTable;
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            if (currentRow < values.Length - 1)
            {
                currentRow++;

                return true;
            }

            return false;
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
        }

        public int FieldCount
        {
            get { return this.values[0].Length; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)this.values[currentRow][i];
        }

        public byte GetByte(int i)
        {
            return (byte)this.values[currentRow][i];
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)this.values[currentRow][i];
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
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
            return (DateTime)this.values[currentRow][i];
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)this.values[currentRow][i];
        }

        public double GetDouble(int i)
        {
            return (double)this.values[currentRow][i];
        }

        public Type GetFieldType(int i)
        {
            return (Type)this.schemaTable.Rows[i]["DataType"];
        }

        public float GetFloat(int i)
        {
            return (float)this.values[currentRow][i];
        }

        public Guid GetGuid(int i)
        {
            return (Guid)this.values[currentRow][i];
        }

        public short GetInt16(int i)
        {
            return (short)this.values[currentRow][i];
        }

        public int GetInt32(int i)
        {
            return (int)this.values[currentRow][i];
        }

        public long GetInt64(int i)
        {
            return (long)this.values[currentRow][i];
        }

        public string GetName(int i)
        {
            return this.schemaTable.Rows[i]["ColumnName"].ToString();
        }

        public int GetOrdinal(string name)
        {
            for(var i = 0; i < this.schemaTable.Rows.Count; i++)
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
            return (string)this.values[currentRow][i];
        }

        public object GetValue(int i)
        {
            return this.values[currentRow][i];
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return this.values[currentRow][i] == null;
        }

        public object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public object this[int i]
        {
            get
            {
                return this.values[currentRow][i];
            }
        }
    }
}
