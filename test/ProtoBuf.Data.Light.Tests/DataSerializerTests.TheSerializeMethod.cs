// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.IO;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class DataSerializerTests
    {
        public class TheSerializeMethod : DataSerializerTests
        {
            [Fact]
            public void ShouldThrowExceptionIfStreamIsNull()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Assert
                Assert.Throws<ArgumentNullException>("stream", () => DataSerializer.Serialize(null, dataReaderMock));
            }

            [Fact]
            public void ShouldThrowExceptionIfDataReaderIsNull()
            {
                // Arrange
                var memoryStream = new MemoryStream();

                // Assert
                Assert.Throws<ArgumentNullException>("reader", () => DataSerializer.Serialize(memoryStream, null));
            }
        }
    }
}
