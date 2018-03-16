// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using System.Data;
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
                // Arrange
                var dataReader = this.GetDataReader(value: "foo");

                dataReader.Close();

                // Assert
                Assert.Throws<ArgumentNullException>("values", () => dataReader.GetValues(null));
            }

            [Fact]
            public void ShouldThrowExceptionWhenNoData()
            {
                // Arrange
                var dataReader = this.GetDataReader(value: "foo");

                // Assert
                Assert.Throws<InvalidOperationException>(() => dataReader.GetValues(new object[1]));
            }

            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                var dataReader = this.GetDataReader(value: "foo");

                dataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => dataReader.GetValues(new object[1]));
            }

            [Fact]
            public void ShouldReturnCorrespondingValues()
            {
                // Arrange
                var value = "foo";
                var dataReader = this.GetDataReader(value: value);

                dataReader.Read();

                var result = new object[1];

                // Act
                dataReader.GetValues(result);

                // Assert
                Assert.Equal(new[] { value }, result);
            }

            [Fact]
            public void ShouldReturnCorrespondingValuesWithSmallerArray()
            {
                // Arrange
                var dataTable = new DataTable();

                dataTable.Columns.Add("foo", typeof(int));
                dataTable.Columns.Add("bar", typeof(int));

                dataTable.Rows.Add(1, 2);

                var dataReader = this.ToProtoBufDataReader(dataTable.CreateDataReader());

                dataReader.Read();

                var result = new object[1];

                // Act
                dataReader.GetValues(result);

                // Assert
                Assert.Equal(new[] { dataTable.Rows[0][0] }, result);
            }
        }
    }
}
