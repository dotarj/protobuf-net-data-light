// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheGetValuesMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenValuesIsNull()
            {
                // Assert
                Assert.Throws<ArgumentNullException>("values", () => protoBufDataReader.GetValues(null));
            }

            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetString(13));
            }

            [Fact]
            public void ShouldReturnCorrespondingValues()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                var valuesMock = new object[dataReaderMock.FieldCount];
                var values = new object[protoBufDataReader.FieldCount];

                dataReaderMock.GetValues(valuesMock);
                protoBufDataReader.GetValues(values);

                // Assert
                Assert.Equal(string.Join("", valuesMock), string.Join("", values));
            }

            [Fact]
            public void ShouldReturnCorrespondingValuesWithSmallerArray()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                var valuesMock = new object[1];
                var values = new object[1];

                dataReaderMock.GetValues(valuesMock);
                protoBufDataReader.GetValues(values);

                // Assert
                Assert.Equal(string.Join("", valuesMock), string.Join("", values));
            }
        }
    }
}
