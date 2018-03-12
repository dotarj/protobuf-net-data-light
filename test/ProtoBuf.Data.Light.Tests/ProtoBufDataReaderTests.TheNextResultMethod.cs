// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using Xunit;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        public class TheNextResultMethod : ProtoBufDataReaderTests
        {
            [Fact]
            public void ShouldNotCloseWhenNoMoreResults()
            {
                // Act
                protoBufDataReader.NextResult();

                // Assert
                Assert.False(protoBufDataReader.IsClosed);
            }

            [Fact]
            public void ShouldReturnFalseWhenNooMoreResults()
            {
                // Act
                var result = protoBufDataReader.NextResult();

                // Assert
                Assert.False(result);
            }
        }
    }
}
