// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using System.Data;
using Moq;
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
                var dataReader = this.GetDataReader(value: "foo");

                dataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => dataReader.RecordsAffected);
            }

            [Fact]
            public void ShouldReturnCorrespondingValues()
            {
                // Arrange
                var dataReader = Mock.Of<IDataReader>();

                var value = 1;

                Mock.Get(dataReader)
                    .Setup(_ => _.RecordsAffected)
                    .Returns(value);

                // Act
                var result = dataReader.RecordsAffected;

                // Assert
                Assert.Equal(value, result);
            }
        }
    }
}
