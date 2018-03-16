// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheGetDataTypeNameMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                var dataReader = this.GetDataReader(value: "foo");

                dataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => dataReader.GetDataTypeName(0));
            }

            [Fact]
            public void ShouldReturnCorrespondingDataTypeName()
            {
                // Arrange
                var value = "foo";
                var dataReader = this.GetDataReader(value: value);

                // Act
                var result = dataReader.GetDataTypeName(0);

                // Assert
                Assert.Equal(value.GetType().Name, result);
            }
        }
    }
}
