// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
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
    }
}
