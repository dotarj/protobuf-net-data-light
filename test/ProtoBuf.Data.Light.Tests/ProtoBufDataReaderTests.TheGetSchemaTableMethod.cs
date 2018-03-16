// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using System.Data;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheGetSchemaTableMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                var dataReader = this.GetDataReader(value: "foo");

                dataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => dataReader.GetSchemaTable());
            }

            [Fact]
            public void ShouldSetTableName()
            {
                // Arrange
                var dataReader = this.GetDataReader(value: "foo");

                // Act
                var schemaTable = dataReader.GetSchemaTable();

                // Assert
                Assert.Equal("SchemaTable", schemaTable.TableName);
            }

            [Fact]
            public void ShouldSetColumnName()
            {
                // Arrange
                var dataTable = new DataTable();

                dataTable.Columns.Add("foo", typeof(int));
                dataTable.Columns.Add("bar", typeof(string));

                var dataReader = this.ToProtoBufDataReader(dataTable.CreateDataReader());

                // Act
                var schemaTable = dataReader.GetSchemaTable();

                // Assert
                Assert.Equal(dataTable.Columns[1].ColumnName, schemaTable.Rows[1]["ColumnName"]);
            }

            [Fact]
            public void ShouldSetColumnOrdinal()
            {
                // Arrange
                var dataTable = new DataTable();

                dataTable.Columns.Add("foo", typeof(int));
                dataTable.Columns.Add("bar", typeof(string));

                var dataReader = this.ToProtoBufDataReader(dataTable.CreateDataReader());

                // Act
                var schemaTable = dataReader.GetSchemaTable();

                // Assert
                Assert.Equal(1, schemaTable.Rows[1]["ColumnOrdinal"]);
            }

            [Fact]
            public void ShouldSetDefaultColumnSize()
            {
                // Arrange
                var dataTable = new DataTable();

                dataTable.Columns.Add("foo", typeof(int));
                dataTable.Columns.Add("bar", typeof(string));

                var dataReader = this.ToProtoBufDataReader(dataTable.CreateDataReader());

                // Act
                var schemaTable = dataReader.GetSchemaTable();

                // Assert
                Assert.Equal(-1, schemaTable.Rows[1]["ColumnSize"]);
            }

            [Fact]
            public void ShouldSetDataType()
            {
                // Arrange
                var dataTable = new DataTable();

                dataTable.Columns.Add("foo", typeof(int));
                dataTable.Columns.Add("bar", typeof(string));

                var dataReader = this.ToProtoBufDataReader(dataTable.CreateDataReader());

                // Act
                var schemaTable = dataReader.GetSchemaTable();

                // Assert
                Assert.Equal(dataTable.Columns[1].DataType, schemaTable.Rows[1]["DataType"]);
            }

            [Fact]
            public void ShouldSetDataTypeName()
            {
                // Arrange
                var dataTable = new DataTable();

                dataTable.Columns.Add("foo", typeof(int));
                dataTable.Columns.Add("bar", typeof(string));

                var dataReader = this.ToProtoBufDataReader(dataTable.CreateDataReader());

                // Act
                var schemaTable = dataReader.GetSchemaTable();

                // Assert
                Assert.Equal(dataTable.Columns[1].DataType.Name, schemaTable.Rows[1]["DataTypeName"]);
            }
        }
    }
}
