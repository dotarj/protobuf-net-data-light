// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

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
                protoBufDataReader.Close();

                // Assert
                Assert.Throws<InvalidOperationException>(() => protoBufDataReader.GetOrdinal("bool"));
            }

            [Fact]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Assert
                Assert.Throws<IndexOutOfRangeException>(() => protoBufDataReader.GetOrdinal("nonexistent"));
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
                    Assert.Equal(dataReaderMock.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()), protoBufDataReader.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()));
                }
            }
        }
    }
}
