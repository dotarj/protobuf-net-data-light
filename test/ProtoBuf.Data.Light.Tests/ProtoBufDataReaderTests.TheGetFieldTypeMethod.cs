// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheGetFieldTypeMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetFieldType(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                protoBufDataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetFieldType(protoBufDataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnCorrespondingFieldType()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                // Assert
                Assert.Equal(dataReaderMock.FieldCount, protoBufDataReader.FieldCount);

                for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.Equal(dataReaderMock.GetFieldType(i), protoBufDataReader.GetFieldType(i));
                }
            }
        }
    }
}
