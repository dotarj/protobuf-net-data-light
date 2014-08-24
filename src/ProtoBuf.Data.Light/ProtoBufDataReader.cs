﻿// Copyright (c) Arjen Post. See License.txt in the project root for license information.
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

                return 1;
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
            this.fieldInfos.Clear();

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

                return this.fieldInfos.Count;
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

        public IDataReader GetData(int i)
        {
            this.ThrowIfClosed();
            this.ThrowIfIndexOutOfRange(i);

            throw new NotImplementedException();
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

            return this.fieldInfos[i].DataType.Name;
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

            return this.fieldInfos[i].DataType;
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

            return this.fieldInfos[i].Name;
        }

        public int GetOrdinal(string name)
        {
            this.ThrowIfClosed();

            var fieldInfo = this.GetFieldInfoByName(name);

            if (fieldInfo == null)
            {
                throw new IndexOutOfRangeException(name);
            }

            return fieldInfo.Ordinal;
        }

        private ProtoBufFieldInfo GetFieldInfoByName(string name)
        {
            foreach (var fieldInfo in fieldInfos)
            {
                if (name == fieldInfo.Name)
                {
                    return fieldInfo;
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

            var valuesCount = values.Length < this.fieldInfos.Count ? values.Length : this.fieldInfos.Count;

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
                schemaRow[dataTypeName] = this.fieldInfos[i].DataType.Name;

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
            var ordinal = 0;

            do
            {
                this.ReadFieldInfo(ordinal);

                this.ReadNextFieldHeader();

                ordinal++;
            }
            while (this.currentFieldHeader == 2);
        }

        private void ReadFieldInfo(int ordinal)
        {
            var fieldInfoToken = ProtoReader.StartSubItem(this.protoReader);

            var fieldInfo = new ProtoBufFieldInfo
            {
                Name = this.ReadName(),
                Ordinal = ordinal,
                DataType = TypeHelper.GetType(this.ReadDataType())
            };

            this.fieldInfos.Add(fieldInfo);

            this.protoReader.ReadFieldHeader();

            ProtoReader.EndSubItem(fieldInfoToken, this.protoReader);
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
