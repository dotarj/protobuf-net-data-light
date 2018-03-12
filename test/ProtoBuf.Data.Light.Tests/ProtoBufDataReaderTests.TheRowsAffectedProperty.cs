// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheRecordsAffectedProperty : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.RecordsAffected);
            }

            [Fact]
            public void ShouldReturnCorrespondingValues()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Assert
                Assert.Equal(dataReaderMock.RecordsAffected, protoBufDataReader.RecordsAffected);
            }
        }
    }
}
