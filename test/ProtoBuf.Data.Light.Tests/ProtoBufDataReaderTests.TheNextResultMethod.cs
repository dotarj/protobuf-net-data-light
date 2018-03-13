// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

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
                this.protoBufDataReader.NextResult();

                // Assert
                Assert.False(this.protoBufDataReader.IsClosed);
            }

            [Fact]
            public void ShouldReturnFalseWhenNooMoreResults()
            {
                // Act
                var result = this.protoBufDataReader.NextResult();

                // Assert
                Assert.False(result);
            }
        }
    }
}
