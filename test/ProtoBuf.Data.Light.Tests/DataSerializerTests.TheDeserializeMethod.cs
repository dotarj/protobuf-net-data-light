// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

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
                this.AssertResult(dataReaderMock);

                Assert.True(this.protoBufDataReader.NextResult());

                dataReaderMock.NextResult();

                this.AssertResult(dataReaderMock);
            }

            [Fact]
            public void ShouldDeserializeAllRows()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(true);

                var protoBufReadCount = 0;
                var mockReadCount = 0;

                // Act
                while (this.protoBufDataReader.Read())
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
                this.AssertResult(dataReaderMock);
            }

            [Fact]
            public void ShouldDeserializeAValidDataTableSource()
            {
                // Arrange
                var dataTable = new DataTable();

                // Act
                dataTable.Load(this.protoBufDataReader);
            }

            private void AssertResult(DataReaderMock dataReaderMock)
            {
                while (this.protoBufDataReader.Read())
                {
                    dataReaderMock.Read();

                    Assert.Equal(dataReaderMock.GetBoolean(0), this.protoBufDataReader.GetBoolean(0));
                    Assert.Equal(dataReaderMock.GetByte(1), this.protoBufDataReader.GetByte(1));

                    // Assert.Equal(dataReaderMock.GetBytes(2), this.protoBufDataReader.GetBytes(2));
                    Assert.Equal(dataReaderMock.GetChar(3), this.protoBufDataReader.GetChar(3));

                    // Assert.Equal(dataReaderMock.GetChars(4), this.protoBufDataReader.GetChars(4));
                    Assert.Equal(dataReaderMock.GetDateTime(5), this.protoBufDataReader.GetDateTime(5));
                    Assert.Equal(dataReaderMock.GetDecimal(6), this.protoBufDataReader.GetDecimal(6));
                    Assert.Equal(dataReaderMock.GetDouble(7), this.protoBufDataReader.GetDouble(7));
                    Assert.Equal(dataReaderMock.GetFloat(8), this.protoBufDataReader.GetFloat(8));
                    Assert.Equal(dataReaderMock.GetGuid(9), this.protoBufDataReader.GetGuid(9));
                    Assert.Equal(dataReaderMock.GetInt32(10), this.protoBufDataReader.GetInt32(10));
                    Assert.Equal(dataReaderMock.GetInt64(11), this.protoBufDataReader.GetInt64(11));
                    Assert.Equal(dataReaderMock.GetInt16(12), this.protoBufDataReader.GetInt16(12));
                    Assert.Equal(dataReaderMock.GetString(13), this.protoBufDataReader.GetString(13));
                    Assert.Equal((TimeSpan)dataReaderMock[14], (TimeSpan)this.protoBufDataReader[14]);
                }
            }
        }
    }
}
