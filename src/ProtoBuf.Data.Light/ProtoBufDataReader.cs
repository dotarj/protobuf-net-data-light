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
    /// <summary>
    /// Provides a way of reading a forward-only stream of a serialized <see cref="IDataReader"/>.
    /// </summary>
    public class ProtoBufDataReader : IDataReader
    {
        private readonly List<ProtoBufDataColumn> columns = new List<ProtoBufDataColumn>();
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

        /// <summary>
        /// Closes the <see cref="ProotoBufDataReader"/> object.
        /// </summary>
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

                return 0;
            }
        }

        /// <summary>
        /// Returns a <see cref="DataTable"/> that describes the column metadata of the
        /// <see cref="ProtoBufDataReader"/>.
        /// </summary>
        /// <returns>A <see cref="DataTable"/> that describes the column metadata.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        public DataTable GetSchemaTable()
        {
            this.ThrowIfClosed();

            if (this.schemaTable == null)
            {
                this.schemaTable = this.BuildSchemaTable();
            }

            return this.schemaTable;
        }

        /// <summary>
        /// Retrieves a Boolean value that indicates whether the specified <see cref="ProtoBufDataReader"/> 
        /// instance has been closed.
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// Advances the data reader to the next result.
        /// </summary>
        /// <returns>true if there are more result sets; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        public bool NextResult()
        {
            this.ThrowIfClosed();

            this.ReadRemainingRows();

            this.buffers = null;
            this.schemaTable = null;
            this.columns.Clear();

            this.ReadNextFieldHeader();

            if (this.currentFieldHeader == 0)
            {
                this.IsClosed = true;

                return false;
            }

            this.reachedEndOfCurrentResult = false;

            this.ReadNextResult();

            return true;
        }

        /// <summary>
        /// Advances the <see cref="ProtoBufDataReader"/> to the next record.
        /// </summary>
        /// <returns>true if there are more rows; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
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

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ProtoBufDataReader"/>
        /// class.
        /// </summary>
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

                return this.columns.Count;
            }
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public bool GetBoolean(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Bool;
        }

        /// <summary>
        /// Gets the value of the specified column as a byte.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public byte GetByte(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Byte;
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer
        /// an array starting at the given buffer offset.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <param name="fieldOffset">The index within the field from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
        /// <param name="bufferoffset">The index within the buffer where the write operation is to start.</param>
        /// <param name="length">The maximum length to copy into the buffer.</param>
        /// <returns>The actual number of bytes read.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            var bytes = this.buffers[i].ByteArray;

            length = Math.Min(length, bytes.Length - (int)fieldOffset);

            Array.Copy(bytes, fieldOffset, buffer, bufferoffset, length);

            return length;
        }

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The character value of the specified column.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public char GetChar(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Char;
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer
        /// as an array starting at the given buffer offset.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <param name="fieldOffset">The index within the field from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
        /// <param name="bufferoffset">The index within the buffer where the write operation is to start.</param>
        /// <param name="length">The maximum length to copy into the buffer.</param>
        /// <returns>The actual number of characters read.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            var chars = this.buffers[i].CharArray;

            length = Math.Min(length, chars.Length - (int)fieldOffset);

            Array.Copy(chars, fieldOffset, buffer, bufferoffset, length);

            return length;
        }

        IDataReader IDataRecord.GetData(int i)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a string representing the data type of the specified column.
        /// </summary>
        /// <param name="i">The zero-based ordinal position of the column to find.</param>
        /// <returns>The string representing the data type of the specified column.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        public string GetDataTypeName(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.columns[i].DataType.Name;
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public DateTime GetDateTime(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].DateTime;
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The fixed-position numeric value of the specified field.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public decimal GetDecimal(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Decimal;
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The double-precision floating point number of the specified field.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
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

            return this.columns[i].DataType;
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The single-precision floating point number of the specified field.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public float GetFloat(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Float;
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The GUID value of the specified field.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public Guid GetGuid(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Guid;
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The 16-bit signed integer value of the specified field.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public short GetInt16(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Short;
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The 32-bit signed integer value of the specified field.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public int GetInt32(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Int;
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The 64-bit signed integer value of the specified field.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public long GetInt64(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Long;
        }

        /// <summary>
        /// Gets the name of the specified column.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The name of the specified column.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        public string GetName(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.columns[i].Name;
        }

        public int GetOrdinal(string name)
        {
            this.ThrowIfClosed();

            var column = this.GetColumnByName(name);

            if (column == null)
            {
                throw new IndexOutOfRangeException(name);
            }

            return column.Ordinal;
        }

        private ProtoBufDataColumn GetColumnByName(string name)
        {
            foreach (var column in columns)
            {
                if (name == column.Name)
                {
                    return column;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The string value of the specified field.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        /// <exception cref="InvalidCastException">The specified cast is not valid.</exception>
        public string GetString(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].String;
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The <see cref="Object"/> which will contain the field value upon return.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        public object GetValue(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].Value;
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current row.
        /// </summary>
        /// <param name="values">An array of <see cref="Object"/> into which to copy the attribute columns.</param>
        /// <returns>The number of instances of <see cref="Object"/> in the array.</returns>
        /// <exception cref="ArgumentNullException">values is null.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        public int GetValues(object[] values)
        {
            Throw.IfNull(values, "values");

            this.ThrowIfClosed();

            var valuesCount = values.Length < this.columns.Count ? values.Length : this.columns.Count;

            for (int i = 0; i < valuesCount; i++)
            {
                values[i] = this.buffers[i].Value;
            }

            return valuesCount;
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>true if the specified field is set to null; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        public bool IsDBNull(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            return this.buffers[i].IsNull;
        }

        /// <summary>
        /// Gets the value of the specified column in its native format given the column
        /// name.
        /// </summary>
        /// <param name="name">The column name.</param>
        /// <returns>The value of the specified column in its native format.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="IndexOutOfRangeException">No column with the specified name was found.</exception>
        public object this[string name]
        {
            get
            {
                return this.GetValue(this.GetOrdinal(name));
            }
        }

        /// <summary>
        /// Gets the value of the specified column in its native format given the column
        /// ordinal.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column in its native format.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ProtoBufDataReader"/> is closed.</exception>
        /// <exception cref="OutOfRangeException">The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
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
            if (i < 0 || i >= this.columns.Count)
            {
                throw new IndexOutOfRangeException();
            }
        }

        private DataTable BuildSchemaTable()
        {
            var schemaTable = new DataTable("SchemaTable")
            {
                Locale = CultureInfo.InvariantCulture,
                MinimumCapacity = this.columns.Count
            };

            var columnName = new DataColumn("ColumnName", typeof(string));
            var columnOrdinal = new DataColumn("ColumnOrdinal", typeof(int)) { DefaultValue = 0 };
            var dataTypeName = new DataColumn("DataTypeName", typeof(string));

            schemaTable.Columns.Add(columnName);
            schemaTable.Columns.Add(columnOrdinal);
            schemaTable.Columns.Add(dataTypeName);

            for (int i = 0; i < this.columns.Count; i++)
            {
                var schemaRow = schemaTable.NewRow();

                schemaRow[columnName] = this.columns[i].Name;
                schemaRow[columnOrdinal] = this.columns[i].Ordinal;
                schemaRow[dataTypeName] = this.columns[i].DataType.Name;

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
                this.ReadColumns();
            }
        }

        private void ReadColumns()
        {
            var ordinal = 0;

            do
            {
                this.ReadColumn(ordinal);

                ordinal++;
            }
            while (this.currentFieldHeader == 2);
        }

        private void ReadColumn(int ordinal)
        {
            var columnToken = ProtoReader.StartSubItem(this.protoReader);

            var name = this.ReadName();
            var protoBufDataType = this.ReadDataType();

            var column = new ProtoBufDataColumn
            {
                Name = name,
                Ordinal = ordinal,
                DataType = TypeHelper.GetType(protoBufDataType),
                ProtoBufDataType = protoBufDataType
            };

            this.columns.Add(column);

            this.protoReader.ReadFieldHeader();

            ProtoReader.EndSubItem(columnToken, this.protoReader);

            this.ReadNextFieldHeader();
        }

        private string ReadName()
        {
            var field = this.protoReader.ReadFieldHeader();

            return this.protoReader.ReadString();
        }

        private ProtoBufDataType ReadDataType()
        {
            var field = this.protoReader.ReadFieldHeader();

            return (ProtoBufDataType)this.protoReader.ReadInt32();
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
                this.buffers = new ProtoBufDataBuffer[this.columns.Count];

                ProtoBufDataBuffer.Initialize(this.buffers);
            }
            else
            {
                ProtoBufDataBuffer.Clear(this.buffers);
            }

            var rowToken = ProtoReader.StartSubItem(this.protoReader);

            this.protoReader.ReadFieldHeader();
            
            this.ReadFieldValues();

            ProtoReader.EndSubItem(rowToken, this.protoReader);
        }

        private void ReadFieldValues()
        {
            var fieldValuesToken = ProtoReader.StartSubItem(this.protoReader);

            int fieldIndex;

            while ((fieldIndex = this.protoReader.ReadFieldHeader()) != 0)
            {
                switch (this.columns[fieldIndex - 1].ProtoBufDataType)
                {
                    case ProtoBufDataType.Bool:
                        this.buffers[fieldIndex - 1].Bool = this.protoReader.ReadBoolean();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.Byte:
                        this.buffers[fieldIndex - 1].Byte = this.protoReader.ReadByte();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.ByteArray:
                        this.buffers[fieldIndex - 1].ByteArray = ProtoReader.AppendBytes(null, this.protoReader);
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.Char:
                        this.buffers[fieldIndex - 1].Char = (char)this.protoReader.ReadInt16();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.CharArray:
                        this.buffers[fieldIndex - 1].CharArray = this.protoReader.ReadString().ToCharArray();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.DateTime:
                        this.buffers[fieldIndex - 1].DateTime = BclHelpers.ReadDateTime(this.protoReader);
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.Decimal:
                        this.buffers[fieldIndex - 1].Decimal = BclHelpers.ReadDecimal(this.protoReader);
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.Double:
                        this.buffers[fieldIndex - 1].Double = this.protoReader.ReadDouble();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.Float:
                        this.buffers[fieldIndex - 1].Float = this.protoReader.ReadSingle();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.Guid:
                        this.buffers[fieldIndex - 1].Guid = BclHelpers.ReadGuid(this.protoReader);
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.Int:
                        this.buffers[fieldIndex - 1].Int = this.protoReader.ReadInt32();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.Long:
                        this.buffers[fieldIndex - 1].Long = this.protoReader.ReadInt64();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.Short:
                        this.buffers[fieldIndex - 1].Short = this.protoReader.ReadInt16();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.String:
                        this.buffers[fieldIndex - 1].String = this.protoReader.ReadString();
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                    case ProtoBufDataType.TimeSpan:
                        this.buffers[fieldIndex - 1].TimeSpan = BclHelpers.ReadTimeSpan(this.protoReader);
                        this.buffers[fieldIndex - 1].IsNull = false;
                        break;
                }
            }

            ProtoReader.EndSubItem(fieldValuesToken, this.protoReader);

            this.protoReader.ReadFieldHeader();
        }

        private void ReadRemainingRows()
        {
            while (this.Read())
            {
            }
        }
    }
}
