// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheGetDataTypeNameMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                this.protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.GetDataTypeName(0));
            }

            [Fact]
            public void ShouldReturnCorrespondingDataTypeName()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                this.protoBufDataReader.Read();

                // Assert
                Assert.Equal(dataReaderMock.FieldCount, this.protoBufDataReader.FieldCount);

                for (int i = 0; i < this.protoBufDataReader.FieldCount; i++)
                {
                    Assert.Equal(dataReaderMock.GetDataTypeName(i), this.protoBufDataReader.GetDataTypeName(i));
                }
            }
        }
    }
}
