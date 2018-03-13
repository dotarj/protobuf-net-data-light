// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheGetNameMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                this.protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.GetName(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => this.protoBufDataReader.GetName(this.protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingName()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                this.protoBufDataReader.Read();

                // Assert
                Assert.Equal(dataReaderMock.FieldCount, this.protoBufDataReader.FieldCount);

                for (int i = 0; i < this.protoBufDataReader.FieldCount; i++)
                {
                    Assert.Equal(dataReaderMock.GetName(i), this.protoBufDataReader.GetName(i));
                }
            }
        }
    }
}
