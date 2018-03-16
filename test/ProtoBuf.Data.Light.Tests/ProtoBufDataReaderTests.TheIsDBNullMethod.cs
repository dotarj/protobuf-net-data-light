// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

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
                var dataReader = this.GetDataReader(value: "foo");

                dataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => dataReader.IsDBNull(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Arrange
                var dataReader = this.GetDataReader(value: "foo");

                // Assert
                Assert.Throws<InvalidOperationException>(() => dataReader.IsDBNull(0));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Arrange
                var dataReader = this.GetDataReader(value: "foo");

                dataReader.Read();

                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => dataReader.IsDBNull(dataReader.FieldCount));
            }

            [Fact]
            public void ShouldReturnTrueIfNull()
            {
                // Arrange
                var dataReader = this.GetDataReader(value: (string)null);

                dataReader.Read();

                // Assert
                Assert.True(dataReader.IsDBNull(0));
            }

            [Fact]
            public void ShouldReturnFalseIfNotNull()
            {
                // Arrange
                var dataReader = this.GetDataReader(value: "foo");

                dataReader.Read();

                // Assert
                Assert.False(dataReader.IsDBNull(0));
            }
        }
    }
}
