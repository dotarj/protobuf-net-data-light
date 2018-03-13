// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheGetOrdinalMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                this.protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => this.protoBufDataReader.GetOrdinal("bool"));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => this.protoBufDataReader.GetOrdinal("nonexistent"));
            }

            [Fact]
            public void ShouldReturnCorrespondingOrdinal()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);
                var schemaTableMock = dataReaderMock.GetSchemaTable();

                // Assert
                for (int i = 0; i < schemaTableMock.Rows.Count; i++)
                {
                    Assert.Equal(dataReaderMock.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()), this.protoBufDataReader.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()));
                }
            }
        }
    }
}
