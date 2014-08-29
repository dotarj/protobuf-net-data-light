// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtoBuf.Data.Light.Test
{
    [TestClass]
    public class ProtoBufDataReaderTests
    {
        private IDataReader protoBufDataReader;

        [TestInitialize]
        public void TestInitialize()
        {
            var dataReaderMock = new DataReaderMock(false);
            var memoryStream = new MemoryStream();

            DataSerializer.Serialize(memoryStream, dataReaderMock);

            memoryStream.Position = 0;

            this.protoBufDataReader = DataSerializer.Deserialize(memoryStream);
        }

        [TestClass]
        public class TheDisposeMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
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
                memoryStream.Position = 0;
            }
        }

        [TestClass]
        public class TheNextResultMethod : ProtoBufDataReaderTests
        {
            [TestMethod]
            public void ShouldCloseWhenNooMoreResults()
            {
                // Act
                protoBufDataReader.NextResult();

                // Assert
                Assert.IsTrue(protoBufDataReader.IsClosed);
            }

            [TestMethod]
            public void ShouldReturnFalseWhenNooMoreResults()
            {
                // Act
                var result = protoBufDataReader.NextResult();

                // Assert
                Assert.IsFalse(result);
            }
        }

        [TestClass]
        public class TheGetFieldTypeMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetFieldType(0);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetFieldType(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingFieldType()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.FieldCount, protoBufDataReader.FieldCount);

                for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.AreEqual(dataReaderMock.GetFieldType(i), protoBufDataReader.GetFieldType(i));
                }
            }
        }

        [TestClass]
        public class TheGetNameMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetName(0);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetName(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingName()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.FieldCount, protoBufDataReader.FieldCount);

                for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.AreEqual(dataReaderMock.GetName(i), protoBufDataReader.GetName(i));
                }
            }
        }

        [TestClass]
        public class TheGetDataTypeNameMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetDataTypeName(0);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetDataTypeName(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingDataTypeName()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.FieldCount, protoBufDataReader.FieldCount);

                for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.AreEqual(dataReaderMock.GetDataTypeName(i), protoBufDataReader.GetDataTypeName(i));
                }
            }
        }

        [TestClass]
        public class TheGetOrdinalMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetOrdinal("bool");
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetOrdinal("nonexistent");
            }

            [TestMethod]
            public void ShouldReturnCorrespondingOrdinal()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);
                var schemaTableMock = dataReaderMock.GetSchemaTable();

                // Assert
                for (int i = 0; i < schemaTableMock.Rows.Count; i++)
                {
                    Assert.AreEqual(dataReaderMock.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()), protoBufDataReader.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()));
                }
            }
        }

        [TestClass]
        public class TheGetValueMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetValue(0);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetValue(protoBufDataReader.FieldCount);
            }

            [TestMethod]
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
                        Assert.AreEqual(Convert.ToString(dataReaderMock.GetValue(i)), Convert.ToString(protoBufDataReader.GetValue(i)));
                    }
                }
            }
        }

        [TestClass]
        public class TheGetSchemaTableMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetSchemaTable();
            }

            [TestMethod]
            public void ShouldReturnSchemaTable()
            {
                // Arrange
                var schemaTableMock = new DataReaderMock(false).GetSchemaTable();

                // Act
                var schemaTable = protoBufDataReader.GetSchemaTable();

                // Assert
                Assert.IsNotNull(schemaTable);
                Assert.AreEqual(schemaTableMock.Rows.Count, schemaTable.Rows.Count);

                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    Assert.AreEqual(schemaTableMock.Rows[i]["ColumnName"].ToString(), schemaTable.Rows[i]["ColumnName"].ToString());
                    Assert.AreEqual((int)schemaTableMock.Rows[i]["ColumnOrdinal"], (int)schemaTable.Rows[i]["ColumnOrdinal"]);
                    Assert.AreEqual(schemaTableMock.Rows[i]["DataTypeName"].ToString(), schemaTable.Rows[i]["DataTypeName"].ToString());
                }
            }
        }

        [TestClass]
        public class TheGetBooleanMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetBoolean(0);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetBoolean(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetBoolean(0), protoBufDataReader.GetBoolean(0));
            }
        }

        [TestClass]
        public class TheIsDBNullMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.IsDBNull(0);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.IsDBNull(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                for (var i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.AreEqual(dataReaderMock.IsDBNull(i), protoBufDataReader.IsDBNull(i));
                }
            }
        }

        [TestClass]
        public class TheGetByteMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetByte(1);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetByte(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetByte(1), protoBufDataReader.GetByte(1));
            }
        }

        [TestClass]
        public class TheGetBytesMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetBytes(2, 0, new byte[0], 0, 1);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetBytes(protoBufDataReader.FieldCount, 0, new byte[0], 0, 1);
            }

            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenFieldOffsetIsLessThanZero()
            {
                // Act
                protoBufDataReader.GetBytes(2, -1, new byte[9], 0, 1);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenLengthIsLessThanZero()
            {
                // Act
                protoBufDataReader.GetBytes(2, 0, new byte[9], 0, -1);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ShouldThrowExceptionWhenBufferOffsetIsLessThanZero()
            {
                // Arrange
                protoBufDataReader.Read();

                // Act
                protoBufDataReader.GetBytes(2, 0, new byte[9], -1, 1);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ShouldThrowExceptionWhenBufferOffsetIsGreaterThanBufferSize()
            {
                // Arrange
                protoBufDataReader.Read();

                // Act
                protoBufDataReader.GetBytes(2, 0, new byte[9], 10, 1);
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ShouldThrowExceptionWhenBufferOffsetIsEqualToBufferSize()
            {
                // Arrange
                protoBufDataReader.Read();

                // Act
                protoBufDataReader.GetBytes(2, 0, new byte[9], 9, 1);
            }

            [TestMethod]
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

                Assert.AreEqual(Encoding.UTF8.GetString(bufferMock), Encoding.UTF8.GetString(buffer));
            }
        }

        [TestClass]
        public class TheGetCharMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetChar(3);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetChar(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetChar(3), protoBufDataReader.GetChar(3));
            }
        }

        [TestClass]
        public class TheGetCharsMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetChars(4, 0, new char[0], 0, 1);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetChars(protoBufDataReader.FieldCount, 0, new char[0], 0, 1);
            }

            [TestMethod]
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

                Assert.AreEqual(new string(bufferMock), new string(buffer));
            }
        }

        [TestClass]
        public class TheGetDateTimeMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetDateTime(5);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetDateTime(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetDateTime(5), protoBufDataReader.GetDateTime(5));
            }
        }

        [TestClass]
        public class TheGetDecimalMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetDecimal(6);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetDecimal(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetDecimal(6), protoBufDataReader.GetDecimal(6));
            }
        }

        [TestClass]
        public class TheGetDoubleMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetDouble(7);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetDouble(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetDouble(7), protoBufDataReader.GetDouble(7));
            }
        }

        [TestClass]
        public class TheGetFloatMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetFloat(8);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetFloat(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetFloat(8), protoBufDataReader.GetFloat(8));
            }
        }

        [TestClass]
        public class TheGetGuidMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetGuid(9);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetGuid(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetGuid(9), protoBufDataReader.GetGuid(9));
            }
        }

        [TestClass]
        public class TheGetInt32Method : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetInt32(10);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetInt32(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetInt32(10), protoBufDataReader.GetInt32(10));
            }
        }

        [TestClass]
        public class TheGetInt64Method : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetInt64(11);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetInt64(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetInt64(11), protoBufDataReader.GetInt64(11));
            }
        }

        [TestClass]
        public class TheGetInt16Method : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetInt16(12);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetInt16(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetInt16(12), protoBufDataReader.GetInt16(12));
            }
        }

        [TestClass]
        public class TheGetStringMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetString(13);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetString(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.GetString(13), protoBufDataReader.GetString(13));
            }
        }

        [TestClass]
        public class TheGetValuesMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void ShouldThrowExceptionWhenValuesIsNull()
            {
                // Act
                protoBufDataReader.GetValues(null);
            }

            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetString(13);
            }

            [TestMethod]
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
                Assert.AreEqual(string.Join("", valuesMock), string.Join("", values));
            }

            [TestMethod]
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
                Assert.AreEqual(string.Join("", valuesMock), string.Join("", values));
            }
        }

        [TestClass]
        public class TheRecordsAffectedProperty : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                var recordsAffected = protoBufDataReader.RecordsAffected;
            }

            [TestMethod]
            public void ShouldReturnCorrespondingValues()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Assert
                Assert.AreEqual(dataReaderMock.RecordsAffected, protoBufDataReader.RecordsAffected);
            }
        }
    }
}
