// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using System.Text;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheGetBytesMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                this.protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.GetBytes(2, 0, new byte[0], 0, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.GetBytes(2, 0, new byte[9], 0, 9));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => this.protoBufDataReader.GetBytes(this.protoBufDataReader.FieldCount, 0, new byte[0], 0, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenFieldOffsetIsLessThanZero()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.GetBytes(2, -1, new byte[9], 0, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenLengthIsLessThanZero()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => this.protoBufDataReader.GetBytes(2, 0, new byte[9], 0, -1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenBufferOffsetIsLessThanZero()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<ArgumentOutOfRangeException>("bufferOffset", () => this.protoBufDataReader.GetBytes(2, 0, new byte[9], -1, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenBufferOffsetIsGreaterThanBufferSize()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<ArgumentOutOfRangeException>("bufferOffset", () => this.protoBufDataReader.GetBytes(2, 0, new byte[9], 10, 1));
            }

            [Fact]
            public void ShouldThrowExceptionWhenBufferOffsetIsEqualToBufferSize()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<ArgumentOutOfRangeException>("bufferOffset", () => this.protoBufDataReader.GetBytes(2, 0, new byte[9], 9, 1));
            }

            [Fact]
            public void ShouldReturnByteArrayLenthWhenBufferIsNull()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Act
                this.protoBufDataReader.GetBytes(2, 0, null, 0, 9);
            }

            [Fact]
            public void ShouldThrowExceptionWhenByteArrayLengthAndBufferOffsetIsGreaterThanBufferLength()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => this.protoBufDataReader.GetBytes(2, 0, new byte[9], 1, 0));
            }

            [Fact]
            public void ShouldReturnZeroWhenFieldOffsetIsGreaterThanByteArrayLength()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Act
                this.protoBufDataReader.GetBytes(2, 9, new byte[9], 0, 9);
            }

            [Fact]
            public void ShouldAdjustCopyLengthWhenFieldOffsetAndLengthExceedsByteArrayLength()
            {
                // Arrange
                this.protoBufDataReader.Read();

                // Act
                this.protoBufDataReader.GetBytes(2, 1, new byte[9], 0, 9);
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                this.protoBufDataReader.Read();
                dataReaderMock.Read();

                var bufferMock = new byte[9];
                var buffer = new byte[9];

                // Assert
                dataReaderMock.GetBytes(2, 0, bufferMock, 0, 9);
                this.protoBufDataReader.GetBytes(2, 0, buffer, 0, 9);

                Assert.Equal(Encoding.UTF8.GetString(bufferMock), Encoding.UTF8.GetString(buffer));
            }
        }
    }
}
