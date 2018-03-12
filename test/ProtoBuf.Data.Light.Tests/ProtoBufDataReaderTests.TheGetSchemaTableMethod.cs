// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
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
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetSchemaTable());
            }

            [Fact]
            public void ShouldReturnSchemaTable()
            {
                // Arrange
                var schemaTableMock = new DataReaderMock(false).GetSchemaTable();

                // Act
                var schemaTable = protoBufDataReader.GetSchemaTable();

                // Assert
                Assert.NotNull(schemaTable);
                Assert.Equal(schemaTableMock.Rows.Count, schemaTable.Rows.Count);

                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    Assert.Equal(schemaTableMock.Rows[i]["ColumnName"].ToString(), schemaTable.Rows[i]["ColumnName"].ToString());
                    Assert.Equal((int)schemaTableMock.Rows[i]["ColumnOrdinal"], (int)schemaTable.Rows[i]["ColumnOrdinal"]);
                    Assert.Equal(schemaTableMock.Rows[i]["DataTypeName"].ToString(), schemaTable.Rows[i]["DataTypeName"].ToString());
                }
            }
        }
    }
}
