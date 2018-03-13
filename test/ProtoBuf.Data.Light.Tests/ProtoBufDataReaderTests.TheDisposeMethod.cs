// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

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
                this.protoBufDataReader.Dispose();

                // Assert
                Assert.Throws<ObjectDisposedException>(() => memoryStream.Position = 0);
            }
        }
    }
}
