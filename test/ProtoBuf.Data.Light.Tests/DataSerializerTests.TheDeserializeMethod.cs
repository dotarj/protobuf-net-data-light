// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.Data;
using System.IO;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class DataSerializerTests
    {
        public class TheDeserializeMethod : DataSerializerTests
        {
            [Fact]
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

                Assert.True(this.protoBufDataReader.NextResult());

                dataReaderMock.NextResult();

                AssertResult(dataReaderMock);

            }

            [Fact]
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
                Assert.Equal(mockReadCount, protoBufReadCount);
            }

            [Fact]
            public void ShouldDeserializeValidProperties()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Assert
                AssertResult(dataReaderMock);
            }

            [Fact]
            public void ShouldDeserializeAValidDataTableSource()
            {
                // Arrange
                var dataTable = new DataTable();

                // Act
                dataTable.Load(protoBufDataReader);
            }

            private void AssertResult(DataReaderMock dataReaderMock)
            {
                while (protoBufDataReader.Read())
                {
                    dataReaderMock.Read();

                    Assert.Equal(dataReaderMock.GetBoolean(0), protoBufDataReader.GetBoolean(0));
                    Assert.Equal(dataReaderMock.GetByte(1), protoBufDataReader.GetByte(1));
                    //Assert.Equal(dataReaderMock.GetBytes(2), dataReader.GetBytes(2));
                    Assert.Equal(dataReaderMock.GetChar(3), protoBufDataReader.GetChar(3));
                    //Assert.Equal(dataReaderMock.GetChars(4), dataReader.GetChars(4));
                    Assert.Equal(dataReaderMock.GetDateTime(5), protoBufDataReader.GetDateTime(5));
                    Assert.Equal(dataReaderMock.GetDecimal(6), protoBufDataReader.GetDecimal(6));
                    Assert.Equal(dataReaderMock.GetDouble(7), protoBufDataReader.GetDouble(7));
                    Assert.Equal(dataReaderMock.GetFloat(8), protoBufDataReader.GetFloat(8));
                    Assert.Equal(dataReaderMock.GetGuid(9), protoBufDataReader.GetGuid(9));
                    Assert.Equal(dataReaderMock.GetInt32(10), protoBufDataReader.GetInt32(10));
                    Assert.Equal(dataReaderMock.GetInt64(11), protoBufDataReader.GetInt64(11));
                    Assert.Equal(dataReaderMock.GetInt16(12), protoBufDataReader.GetInt16(12));
                    Assert.Equal(dataReaderMock.GetString(13), protoBufDataReader.GetString(13));
                    Assert.Equal((TimeSpan)dataReaderMock[14], (TimeSpan)protoBufDataReader[14]);
                }
            }
        }
    }
}
