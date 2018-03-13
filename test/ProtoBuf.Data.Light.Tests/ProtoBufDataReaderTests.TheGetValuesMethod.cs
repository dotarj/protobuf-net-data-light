// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

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
                Assert.Throws<ArgumentNullException>("values", () => this.protoBufDataReader.GetValues(null));
            }

            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                this.protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.GetString(13));
            }

            [Fact]
            public void ShouldReturnCorrespondingValues()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                this.protoBufDataReader.Read();

                var valuesMock = new object[dataReaderMock.FieldCount];
                var values = new object[this.protoBufDataReader.FieldCount];

                dataReaderMock.GetValues(valuesMock);
                this.protoBufDataReader.GetValues(values);

                // Assert
                Assert.Equal(string.Join(string.Empty, valuesMock), string.Join(string.Empty, values));
            }

            [Fact]
            public void ShouldReturnCorrespondingValuesWithSmallerArray()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                this.protoBufDataReader.Read();

                var valuesMock = new object[1];
                var values = new object[1];

                dataReaderMock.GetValues(valuesMock);
                this.protoBufDataReader.GetValues(values);

                // Assert
                Assert.Equal(string.Join(string.Empty, valuesMock), string.Join(string.Empty, values));
            }
        }
    }
}
