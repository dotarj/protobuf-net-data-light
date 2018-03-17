// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using System.Data;
using System.IO;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheDisposeMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldDisposeStream()
            {
                // Arrange
                var dataTable = new DataTable();

                dataTable.Columns.Add("foo", typeof(int));

                var memoryStream = new MemoryStream();

                DataSerializer.Serialize(memoryStream, dataTable.CreateDataReader());

                memoryStream.Position = 0;

                var dataReader = (ProtoBufDataReader)DataSerializer.Deserialize(memoryStream);

                // Act
                dataReader.Dispose();

                // Assert
                Assert.Throws<ObjectDisposedException>(() => memoryStream.Position = 0);
            }
        }
    }
}
