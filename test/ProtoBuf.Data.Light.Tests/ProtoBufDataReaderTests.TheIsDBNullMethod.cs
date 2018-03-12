// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

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
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.IsDBNull(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.IsDBNull(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.IsDBNull(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingValue()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                protoBufDataReader.Read();
                dataReaderMock.Read();

                // Assert
                for (var i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.Equal(dataReaderMock.IsDBNull(i), protoBufDataReader.IsDBNull(i));
                }
            }
        }
    }
}
