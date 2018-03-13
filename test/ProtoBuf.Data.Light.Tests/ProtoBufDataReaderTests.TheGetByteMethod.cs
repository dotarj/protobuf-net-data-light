// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheGetByteMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                this.protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.GetByte(1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.GetByte(1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => this.protoBufDataReader.GetByte(this.protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                this.protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                Assert.Equal(dataReaderMock.GetByte(1), this.protoBufDataReader.GetByte(1));
            }
        }
    }
}
