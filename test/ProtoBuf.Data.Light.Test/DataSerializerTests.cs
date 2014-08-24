// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace ProtoBuf.Data.Light.Test
{
    [TestClass]
    public class DataSerializerTests
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
        public class TheSerializeMethod : DataSerializerTests
        {
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void ShouldThrowExceptionIfStreamIsNull()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Act
                DataSerializer.Serialize(null, dataReaderMock);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void ShouldThrowExceptionIfDataReaderIsNull()
            {
                // Arrange
                var memoryStream = new MemoryStream();

                // Act
                DataSerializer.Serialize(memoryStream, null);
            }

            [TestMethod]
            public void ShouldDeserializeAllResults()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(true);
                var memoryStream = new MemoryStream();

                DataSerializer.Serialize(memoryStream, dataReaderMock);

                memoryStream.Position = 0;

                this.protoBufDataReader = DataSerializer.Deserialize(memoryStream);

                dataReaderMock = new DataReaderMock(true);

                // Assert
                AssertResult(dataReaderMock);

                Assert.IsTrue(this.protoBufDataReader.NextResult());

                dataReaderMock.NextResult();

                AssertResult(dataReaderMock);

            }

            [TestMethod]
            public void ShouldDeserializeAllRows()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(true);

                var protoBufReadCount = 0;
                var mockReadCount = 0;

                // Act
                while (protoBufDataReader.Read())
                {
                    protoBufReadCount++;
                }

                while (dataReaderMock.Read())
                {
                    mockReadCount++;
                }

                // Assert
                Assert.AreEqual(mockReadCount, protoBufReadCount);
            }

            [TestMethod]
            public void ShouldDeserializeValidProperties()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Assert
                AssertResult(dataReaderMock);
            }

            private void AssertResult(DataReaderMock dataReaderMock)
            {
                while (protoBufDataReader.Read())
                {
                    dataReaderMock.Read();

                    Assert.AreEqual(dataReaderMock.GetBoolean(0), protoBufDataReader.GetBoolean(0));
                    Assert.AreEqual(dataReaderMock.GetByte(1), protoBufDataReader.GetByte(1));
                    //Assert.AreEqual(dataReaderMock.GetBytes(2), dataReader.GetBytes(2));
                    Assert.AreEqual(dataReaderMock.GetChar(3), protoBufDataReader.GetChar(3));
                    //Assert.AreEqual(dataReaderMock.GetChars(4), dataReader.GetChars(4));
                    Assert.AreEqual(dataReaderMock.GetDateTime(5), protoBufDataReader.GetDateTime(5));
                    Assert.AreEqual(dataReaderMock.GetDecimal(6), protoBufDataReader.GetDecimal(6));
                    Assert.AreEqual(dataReaderMock.GetDouble(7), protoBufDataReader.GetDouble(7));
                    Assert.AreEqual(dataReaderMock.GetFloat(8), protoBufDataReader.GetFloat(8));
                    Assert.AreEqual(dataReaderMock.GetGuid(9), protoBufDataReader.GetGuid(9));
                    Assert.AreEqual(dataReaderMock.GetInt32(10), protoBufDataReader.GetInt32(10));
                    Assert.AreEqual(dataReaderMock.GetInt64(11), protoBufDataReader.GetInt64(11));
                    Assert.AreEqual(dataReaderMock.GetInt16(12), protoBufDataReader.GetInt16(12));
                    Assert.AreEqual(dataReaderMock.GetString(13), protoBufDataReader.GetString(13));
                    Assert.AreEqual((TimeSpan)dataReaderMock[14], (TimeSpan)protoBufDataReader[14]);
                }
            }
        }
    }
}
