// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProtoBuf.Data.Light
{
    public class ProtoBufDataReader : IDataReader
    {
        private readonly Dictionary<string, ProtoBufFieldInfo> fieldInfoByName = new Dictionary<string, ProtoBufFieldInfo>();
        private readonly List<ProtoBufFieldInfo> fieldInfos = new List<ProtoBufFieldInfo>();
        private readonly ProtoReader protoReader;
        private readonly Stream stream;

        private ProtoBufDataBuffer[] buffers;
        private int currentFieldHeader;
        private SubItemToken currentResultToken;
        private bool disposed;
        private bool reachedEndOfCurrentResult;
        private DataTable schemaTable;

        internal ProtoBufDataReader(Stream stream)
        {
            Throw.IfNull(stream, "stream");

            this.stream = stream;
            this.protoReader = new ProtoReader(this.stream, null, null);

            this.ReadNextFieldHeader();

            if (this.currentFieldHeader != 1)
            {
                throw new InvalidDataException(string.Format("Field header 2 expected, actual '{0}'.", this.currentFieldHeader));
            }

            this.ReadNextResult();
        }

        ~ProtoBufDataReader()
        {
            this.Dispose(false);
        }

        public void Close()
        {
            this.stream.Close();

            this.IsClosed = true;
        }

        public int Depth
        {
            get
            {
                this.ThrowIfClosed();

                return 1;
            }
        }

        public DataTable GetSchemaTable()
        {
            this.ThrowIfClosed();

            if (this.schemaTable == null)
            {
                this.schemaTable = this.BuildSchemaTable();
            }

            return this.schemaTable;
        }

        public bool IsClosed { get; private set; }

        public bool NextResult()
        {
            // TODO: Implement...

            return false;
        }

        public bool Read()
        {
            this.ThrowIfClosed();

            if (this.reachedEndOfCurrentResult)
            {
                return false;
            }

            if (this.currentFieldHeader == 0)
            {
                ProtoReader.EndSubItem(this.currentResultToken, this.protoReader);

                this.reachedEndOfCurrentResult = true;

                return false;
            }

            this.ReadRow();
            this.ReadNextFieldHeader();

            return true;
        }

        public int RecordsAffected
        {
            get
            {
                this.ThrowIfClosed();

                return 0;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        public int FieldCount
        {
            get
            {
                this.ThrowIfClosed();

                return this.fieldInfos.Count;
            }
        }

        public bool GetBoolean(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Bool;
        }

        public byte GetByte(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Byte;
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            var buffers = this.buffers[i].ByteArray;

            length = Math.Min(length, buffers.Length - (int)fieldOffset);

            Array.Copy(buffers, fieldOffset, buffer, bufferoffset, length);

            return length;
        }

        public char GetChar(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Char;
        }

        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            var buffers = this.buffers[i].CharArray;

            length = Math.Min(length, buffers.Length - (int)fieldOffset);

            Array.Copy(buffers, fieldOffset, buffer, bufferoffset, length);

            return length;
        }

        public IDataReader GetData(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.fieldInfos[i].DataTypeName;
        }

        public DateTime GetDateTime(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].DateTime;
        }

        public decimal GetDecimal(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Decimal;
        }

        public double GetDouble(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Double;
        }

        public Type GetFieldType(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.fieldInfos[i].DataType;
        }

        public float GetFloat(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Float;
        }

        public Guid GetGuid(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Guid;
        }

        public short GetInt16(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Short;
        }

        public int GetInt32(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Int;
        }

        public long GetInt64(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Long;
        }

        public string GetName(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.fieldInfos[i].Name;
        }

        public int GetOrdinal(string name)
        {
            this.ThrowIfClosed();

            ProtoBufFieldInfo fieldInfo;

            if (!this.fieldInfoByName.TryGetValue(name, out fieldInfo))
            {
                throw new IndexOutOfRangeException(name);
            }

            return fieldInfo.Ordinal;
        }

        public string GetString(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].String;
        }

        public object GetValue(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Value;
        }

        public int GetValues(object[] values)
        {
            Throw.IfNull(values, "values");

            this.ThrowIfClosed();

            var valuesCount = values.Length < this.fieldInfos.Count ? values.Length : this.fieldInfos.Count;

            for (int i = 0; i < valuesCount; i++)
            {
                values[i] = this.buffers[i].Value;
            }

            return valuesCount;
        }

        public bool IsDBNull(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].IsNull;
        }

        public object this[string name]
        {
            get
            {
                return this.GetValue(this.GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return this.GetValue(i);
            }
        }

        private void ThrowIfClosed([CallerMemberName]string memberName = "")
        {
            if (this.IsClosed)
            {
                throw new InvalidOperationException(string.Format("Invalid attempt to call {0} when reader is closed.", memberName));
            }
        }

        private void ThrowIfIndexOutOfRange(int i)
        {
            if (i < 0 || i >= this.fieldInfos.Count)
            {
                throw new IndexOutOfRangeException();
            }
        }

        private DataTable BuildSchemaTable()
        {
            var schemaTable = new DataTable("SchemaTable")
            {
                Locale = CultureInfo.InvariantCulture,
                MinimumCapacity = this.fieldInfos.Count
            };

            var columnName = new DataColumn("ColumnName", typeof(string));
            var columnOrdinal = new DataColumn("ColumnOrdinal", typeof(int)) { DefaultValue = 0 };
            var dataTypeName = new DataColumn("DataTypeName", typeof(string));

            schemaTable.Columns.Add(columnName);
            schemaTable.Columns.Add(columnOrdinal);
            schemaTable.Columns.Add(dataTypeName);

            for (int i = 0; i < this.fieldInfos.Count; i++)
            {
                var schemaRow = schemaTable.NewRow();

                schemaRow[columnName] = this.fieldInfos[i].Name;
                schemaRow[columnOrdinal] = this.fieldInfos[i].Ordinal;
                schemaRow[dataTypeName] = this.fieldInfos[i].DataTypeName;

                schemaTable.Rows.Add(schemaRow);
            }

            foreach (DataColumn column in schemaTable.Columns)
            {
                column.ReadOnly = true;
            }

            return schemaTable;
        }

        private void ReadNextResult()
        {
            this.currentResultToken = ProtoReader.StartSubItem(this.protoReader);

            this.ReadNextFieldHeader();

            if (this.currentFieldHeader == 0)
            {
                this.reachedEndOfCurrentResult = true;

                ProtoReader.EndSubItem(this.currentResultToken, this.protoReader);
            }
            else if (this.currentFieldHeader != 2)
            {
                throw new InvalidDataException(string.Format("Field header 2 expected, actual '{0}'.", this.currentFieldHeader));
            }
            else
            {
                this.ReadFieldInfos();
            }
        }

        private void ReadFieldInfos()
        {
            do
            {
                this.ReadFieldInfo();

                this.ReadNextFieldHeader();
            }
            while (this.currentFieldHeader == 2);
        }

        private void ReadFieldInfo()
        {
            var fieldInfoToken = ProtoReader.StartSubItem(this.protoReader);

            var fieldInfo = new ProtoBufFieldInfo
            {
                Name = this.ReadName(),
                Ordinal = this.ReadOrdinal(),
                DataType = TypeHelper.GetType(this.ReadDataType()),
                DataTypeName = this.ReadDataTypeName()
            };

            this.fieldInfos.Add(fieldInfo);

            if (!this.fieldInfoByName.ContainsKey(fieldInfo.Name))
            {
                this.fieldInfoByName.Add(fieldInfo.Name, fieldInfo);
            }

            this.protoReader.ReadFieldHeader();

            ProtoReader.EndSubItem(fieldInfoToken, this.protoReader);
        }

        private string ReadName()
        {
            var field = this.protoReader.ReadFieldHeader();

            return this.protoReader.ReadString();
        }

        private int ReadOrdinal()
        {
            var field = this.protoReader.ReadFieldHeader();

            return this.protoReader.ReadInt32();
        }

        private ProtoBufDataType ReadDataType()
        {
            var field = this.protoReader.ReadFieldHeader();

            return (ProtoBufDataType)this.protoReader.ReadInt32();
        }

        private string ReadDataTypeName()
        {
            var field = this.protoReader.ReadFieldHeader();

            return this.protoReader.ReadString();
        }

        private void ReadNextFieldHeader()
        {
            this.currentFieldHeader = this.protoReader.ReadFieldHeader();
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.protoReader != null)
                    {
                        this.protoReader.Dispose();
                    }

                    if (this.stream != null)
                    {
                        this.stream.Dispose();
                    }
                }

                this.disposed = true;
            }
        }

        private void ReadRow()
        {
            if (this.buffers == null)
            {
                this.buffers = new ProtoBufDataBuffer[this.fieldInfos.Count];

                ProtoBufDataBuffer.Initialize(this.buffers);
            }
            else
            {
                ProtoBufDataBuffer.Clear(this.buffers);
            }

            var rowToken = ProtoReader.StartSubItem(this.protoReader);
            var i = 0;

            while (this.protoReader.ReadFieldHeader() != 0)
            {
                this.ReadField(i);

                i++;
            }

            ProtoReader.EndSubItem(rowToken, this.protoReader);
        }

        private void ReadField(int i)
        {
            var fieldToken = ProtoReader.StartSubItem(this.protoReader);

            this.buffers[i].IsNull = this.ReadFieldIsNull();

            if (!this.buffers[i].IsNull)
            {
                this.ReadFieldValue(i);
            }

            this.protoReader.ReadFieldHeader();

            ProtoReader.EndSubItem(fieldToken, this.protoReader);
        }

        private void ReadFieldValue(int i)
        {
            var field = this.protoReader.ReadFieldHeader();
            var protoBufDataType = TypeHelper.GetProtoBufDataType(this.fieldInfos[i].DataType);

            switch (protoBufDataType)
            {
                case ProtoBufDataType.Bool:
                    this.buffers[i].Bool = this.protoReader.ReadBoolean();
                    break;
                case ProtoBufDataType.Byte:
                    this.buffers[i].Byte = this.protoReader.ReadByte();
                    break;
                case ProtoBufDataType.ByteArray:
                    this.buffers[i].ByteArray = ProtoReader.AppendBytes(null, this.protoReader);
                    break;
                case ProtoBufDataType.Char:
                    this.buffers[i].Char = (char)this.protoReader.ReadInt16();
                    break;
                case ProtoBufDataType.CharArray:
                    this.buffers[i].CharArray = this.protoReader.ReadString().ToCharArray();
                    break;
                case ProtoBufDataType.DateTime:
                    this.buffers[i].DateTime = BclHelpers.ReadDateTime(this.protoReader);
                    break;
                case ProtoBufDataType.Decimal:
                    this.buffers[i].Decimal = BclHelpers.ReadDecimal(this.protoReader);
                    break;
                case ProtoBufDataType.Double:
                    this.buffers[i].Double = this.protoReader.ReadDouble();
                    break;
                case ProtoBufDataType.Float:
                    this.buffers[i].Float = this.protoReader.ReadSingle();
                    break;
                case ProtoBufDataType.Guid:
                    this.buffers[i].Guid = BclHelpers.ReadGuid(this.protoReader);
                    break;
                case ProtoBufDataType.Int:
                    this.buffers[i].Int = this.protoReader.ReadInt32();
                    break;
                case ProtoBufDataType.Long:
                    this.buffers[i].Long = this.protoReader.ReadInt64();
                    break;
                case ProtoBufDataType.Short:
                    this.buffers[i].Short = this.protoReader.ReadInt16();
                    break;
                case ProtoBufDataType.String:
                    this.buffers[i].String = this.protoReader.ReadString();
                    break;
                case ProtoBufDataType.TimeSpan:
                    this.buffers[i].TimeSpan = BclHelpers.ReadTimeSpan(this.protoReader);
                    break;
            }
        }

        private bool ReadFieldIsNull()
        {
            var field = this.protoReader.ReadFieldHeader();

            return this.protoReader.ReadBoolean();
        }

        private void ReadRemainingRows()
        {
            while (this.Read())
            {
            }
        }
    }
}
