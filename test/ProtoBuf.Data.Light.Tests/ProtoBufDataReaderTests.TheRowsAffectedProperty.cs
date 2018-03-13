// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

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
                this.protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.RecordsAffected);
            }

            [Fact]
            public void ShouldReturnCorrespondingValues()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Assert
                Assert.Equal(dataReaderMock.RecordsAffected, this.protoBufDataReader.RecordsAffected);
            }
        }
    }
}
