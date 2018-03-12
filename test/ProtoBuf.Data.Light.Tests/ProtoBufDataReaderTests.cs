// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public class ProtoBufDataReaderTests
    {
        private IDataReader protoBufDataReader;
        
        public ProtoBufDataReaderTests()
        {
            var dataReaderMock = new DataReaderMock(false);
            var memoryStream = new MemoryStream();

            DataSerializer.Serialize(memoryStream, dataReaderMock);

            memoryStream.Position = 0;

            this.protoBufDataReader = DataSerializer.Deserialize(memoryStream);
        }

        public class TheDisposeMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsDisposed()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);
                var memoryStream = new MemoryStream();

                DataSerializer.Serialize(memoryStream, dataReaderMock);

                memoryStream.Position = 0;

                this.protoBufDataReader = DataSerializer.Deserialize(memoryStream);

                // Act
                protoBufDataReader.Dispose();

                // Assert
                Assert.Throws<ObjectDisposedException>(() => memoryStream.Position = 0);
            }
        }

        public class TheNextResultMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldNotCloseWhenNoMoreResults()
            {
                // Act
                protoBufDataReader.NextResult();

                // Assert
                Assert.False(protoBufDataReader.IsClosed);
            }

            [Fact]
            public void ShouldReturnFalseWhenNooMoreResults()
            {
                // Act
                var result = protoBufDataReader.NextResult();

                // Assert
                Assert.False(result);
            }
        }

        public class TheGetFieldTypeMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetFieldType(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetFieldType(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingFieldType()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                // Assert
                Assert.Equal(dataReaderMock.FieldCount, protoBufDataReader.FieldCount);

                for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.Equal(dataReaderMock.GetFieldType(i), protoBufDataReader.GetFieldType(i));
                }
            }
        }

        public class TheGetNameMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetName(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetName(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingName()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                // Assert
                Assert.Equal(dataReaderMock.FieldCount, protoBufDataReader.FieldCount);

                for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.Equal(dataReaderMock.GetName(i), protoBufDataReader.GetName(i));
                }
            }
        }

        public class TheGetDataTypeNameMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetDataTypeName(0));
            }

            [Fact]
            public void ShouldReturnCorrespondingDataTypeName()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                // Assert
                Assert.Equal(dataReaderMock.FieldCount, protoBufDataReader.FieldCount);

                for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.Equal(dataReaderMock.GetDataTypeName(i), protoBufDataReader.GetDataTypeName(i));
                }
            }
        }

        public class TheGetOrdinalMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetOrdinal("bool"));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetOrdinal("nonexistent"));
            }

            [Fact]
            public void ShouldReturnCorrespondingOrdinal()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);
                var schemaTableMock = dataReaderMock.GetSchemaTable();

                // Assert
                for (int i = 0; i < schemaTableMock.Rows.Count; i++)
                {
                    Assert.Equal(dataReaderMock.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()), protoBufDataReader.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()));
                }
            }
        }

        public class TheGetValueMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetValue(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetValue(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetValue(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Assert
                while (protoBufDataReader.Read())
                {
                    dataReaderMock.Read();

                    for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                    {
                        Assert.Equal(Convert.ToString(dataReaderMock.GetValue(i)), Convert.ToString(protoBufDataReader.GetValue(i)));
                    }
                }
            }
        }

        public class TheGetSchemaTableMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetSchemaTable());
            }

            [Fact]
            public void ShouldReturnSchemaTable()
            {
                // Arrange
                var schemaTableMock = new DataReaderMock(false).GetSchemaTable();

                // Act
                var schemaTable = protoBufDataReader.GetSchemaTable();

                // Assert
                Assert.NotNull(schemaTable);
                Assert.Equal(schemaTableMock.Rows.Count, schemaTable.Rows.Count);

                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    Assert.Equal(schemaTableMock.Rows[i]["ColumnName"].ToString(), schemaTable.Rows[i]["ColumnName"].ToString());
                    Assert.Equal((int)schemaTableMock.Rows[i]["ColumnOrdinal"], (int)schemaTable.Rows[i]["ColumnOrdinal"]);
                    Assert.Equal(schemaTableMock.Rows[i]["DataTypeName"].ToString(), schemaTable.Rows[i]["DataTypeName"].ToString());
                }
            }
        }

        public class TheGetBooleanMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetBoolean(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetBoolean(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetBoolean(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetBoolean(0), protoBufDataReader.GetBoolean(0));
            }
        }

        public class TheIsDBNullMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.IsDBNull(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.IsDBNull(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.IsDBNull(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                for (var i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.Equal(dataReaderMock.IsDBNull(i), protoBufDataReader.IsDBNull(i));
                }
            }
        }

        public class TheGetByteMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetByte(1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetByte(1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetByte(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetByte(1), protoBufDataReader.GetByte(1));
            }
        }

        public class TheGetBytesMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetBytes(2, 0, new byte[0], 0, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetBytes(2, 0, new byte[9], 0, 9));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetBytes(protoBufDataReader.FieldCount, 0, new byte[0], 0, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenFieldOffsetIsLessThanZero()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetBytes(2, -1, new byte[9], 0, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenLengthIsLessThanZero()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetBytes(2, 0, new byte[9], 0, -1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenBufferOffsetIsLessThanZero()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<ArgumentOutOfRangeException>("bufferOffset", () => protoBufDataReader.GetBytes(2, 0, new byte[9], -1, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenBufferOffsetIsGreaterThanBufferSize()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<ArgumentOutOfRangeException>("bufferOffset", () => protoBufDataReader.GetBytes(2, 0, new byte[9], 10, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenBufferOffsetIsEqualToBufferSize()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<ArgumentOutOfRangeException>("bufferOffset", () => protoBufDataReader.GetBytes(2, 0, new byte[9], 9, 1));
            }

            [Fact]
            public void ShouldReturnByteArrayLenthWhenBufferIsNull()
            {
                // Arrange
                protoBufDataReader.Read();

                // Act
                protoBufDataReader.GetBytes(2, 0, null, 0, 9);
            }

            [Fact]
            public void ShouldThrowExceptionWhenByteArrayLengthAndBufferOffsetIsGreaterThanBufferLength()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetBytes(2, 0, new byte[9], 1, 0));
            }

            [Fact]
            public void ShouldReturnZeroWhenFieldOffsetIsGreaterThanByteArrayLength()
            {
                // Arrange
                protoBufDataReader.Read();

                // Act
                protoBufDataReader.GetBytes(2, 9, new byte[9], 0, 9);
            }

            [Fact]
            public void ShouldAdjustCopyLengthWhenFieldOffsetAndLengthExceedsByteArrayLength()
            {
                // Arrange
                protoBufDataReader.Read();

                // Act
                protoBufDataReader.GetBytes(2, 1, new byte[9], 0, 9);
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                var bufferMock = new byte[9];
                var buffer = new byte[9];
                //9
                
                // Assert
                dataReaderMock.GetBytes(2, 0, bufferMock, 0, 9);
                protoBufDataReader.GetBytes(2, 0, buffer, 0, 9);

                Assert.Equal(Encoding.UTF8.GetString(bufferMock), Encoding.UTF8.GetString(buffer));
            }
        }

        public class TheGetCharMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetChar(3));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetChar(3));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetChar(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetChar(3), protoBufDataReader.GetChar(3));
            }
        }

        public class TheGetCharsMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetChars(4, 0, new char[0], 0, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetChars(4, 0, new char[9], 0, 9));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetChars(protoBufDataReader.FieldCount, 0, new char[0], 0, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenFieldOffsetIsLessThanZero()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetChars(4, -1, new char[9], 0, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenLengthIsLessThanZero()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetChars(4, 0, new char[9], 0, -1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenBufferOffsetIsLessThanZero()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<ArgumentOutOfRangeException>("bufferOffset", () => protoBufDataReader.GetChars(4, 0, new char[9], -1, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenBufferOffsetIsGreaterThanBufferSize()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<ArgumentOutOfRangeException>("bufferOffset", () => protoBufDataReader.GetChars(4, 0, new char[9], 10, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenBufferOffsetIsEqualToBufferSize()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<ArgumentOutOfRangeException>("bufferOffset", () => protoBufDataReader.GetChars(4, 0, new char[9], 9, 1));
            }

            [Fact]
            public void ShouldReturnByteArrayLenthWhenBufferIsNull()
            {
                // Arrange
                protoBufDataReader.Read();

                // Act
                protoBufDataReader.GetChars(4, 0, null, 0, 9);
            }

            [Fact]
            public void ShouldThrowExceptionWhenByteArrayLengthAndBufferOffsetIsGreaterThanBufferLength()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetChars(4, 0, new char[9], 1, 0));
            }

            [Fact]
            public void ShouldReturnZeroWhenFieldOffsetIsGreaterThanByteArrayLength()
            {
                // Arrange
                protoBufDataReader.Read();

                // Act
                protoBufDataReader.GetChars(4, 9, new char[9], 0, 9);
            }

            [Fact]
            public void ShouldAdjustCopyLengthWhenFieldOffsetAndLengthExceedsByteArrayLength()
            {
                // Arrange
                protoBufDataReader.Read();

                // Act
                protoBufDataReader.GetChars(4, 1, new char[9], 0, 9);
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                var bufferMock = new char[9];
                var buffer = new char[9];
                //9

                // Assert
                dataReaderMock.GetChars(4, 0, bufferMock, 0, 9);
                protoBufDataReader.GetChars(4, 0, buffer, 0, 9);

                Assert.Equal(new string(bufferMock), new string(buffer));
            }
        }

        public class TheGetDateTimeMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetDateTime(5));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetDateTime(5));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetDateTime(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetDateTime(5), protoBufDataReader.GetDateTime(5));
            }
        }

        public class TheGetDecimalMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetDecimal(6));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetDecimal(6));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetDecimal(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetDecimal(6), protoBufDataReader.GetDecimal(6));
            }
        }

        public class TheGetDoubleMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetDouble(7));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetDouble(7));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetDouble(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetDouble(7), protoBufDataReader.GetDouble(7));
            }
        }

        public class TheGetFloatMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetFloat(8));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetFloat(8));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetFloat(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetFloat(8), protoBufDataReader.GetFloat(8));
            }
        }

        public class TheGetGuidMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetGuid(9));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetGuid(9));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetGuid(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetGuid(9), protoBufDataReader.GetGuid(9));
            }
        }

        public class TheGetInt32Method : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetInt32(10));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetInt32(10));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetInt32(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetInt32(10), protoBufDataReader.GetInt32(10));
            }
        }

        public class TheGetInt64Method : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetInt64(11));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetInt64(11));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetInt64(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetInt64(11), protoBufDataReader.GetInt64(11));
            }
        }

        public class TheGetInt16Method : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetInt16(12));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetInt16(12));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetInt16(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetInt16(12), protoBufDataReader.GetInt16(12));
            }
        }

        public class TheGetStringMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetString(13));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetString(13));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetString(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetString(13), protoBufDataReader.GetString(13));
            }
        }

        public class TheGetValuesMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenValuesIsNull()
            {
                // Assert
                Assert.Throws<ArgumentNullException>("values", () => protoBufDataReader.GetValues(null));
            }

            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetString(13));
            }

            [Fact]
            public void ShouldReturnCorrespondingValues()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                var valuesMock = new object[dataReaderMock.FieldCount];
                var values = new object[protoBufDataReader.FieldCount];

                dataReaderMock.GetValues(valuesMock);
                protoBufDataReader.GetValues(values);

                // Assert
                Assert.Equal(string.Join("", valuesMock), string.Join("", values));
            }

            [Fact]
            public void ShouldReturnCorrespondingValuesWithSmallerArray()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                var valuesMock = new object[1];
                var values = new object[1];

                dataReaderMock.GetValues(valuesMock);
                protoBufDataReader.GetValues(values);

                // Assert
                Assert.Equal(string.Join("", valuesMock), string.Join("", values));
            }
        }

        public class TheRecordsAffectedProperty : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.RecordsAffected);
            }

            [Fact]
            public void ShouldReturnCorrespondingValues()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Assert
                Assert.Equal(dataReaderMock.RecordsAffected, protoBufDataReader.RecordsAffected);
            }
        }
    }
}
