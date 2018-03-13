// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheIsDBNullMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                this.protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.IsDBNull(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.IsDBNull(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => this.protoBufDataReader.IsDBNull(this.protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                this.protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                for (var i = 0; i < this.protoBufDataReader.FieldCount; i++)
                {
                    Assert.Equal(dataReaderMock.IsDBNull(i), this.protoBufDataReader.IsDBNull(i));
                }
            }
        }
    }
}
