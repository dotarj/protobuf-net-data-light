// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.IO;
using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheDisposeMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldThrowExceptionWhenDataReaderIsDisposed()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);
                var memoryStream = new MemoryStream();

                DataSerializer.Serialize(memoryStream, dataReaderMock);

                memoryStream.Position = 0;

                this.protoBufDataReader = DataSerializer.Deserialize(memoryStream);

                // Act
                protoBufDataReader.Dispose();

                // Assert
                Assert.Throws<ObjectDisposedException>(() => memoryStream.Position = 0);
            }
        }
    }
}
