// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System.Data;
using System.IO;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class DataSerializerTests
    {
        private IDataReader protoBufDataReader;

        public DataSerializerTests()
        {
            var dataReaderMock = new DataReaderMock(false);
            var memoryStream = new MemoryStream();

            DataSerializer.Serialize(memoryStream, dataReaderMock);

            memoryStream.Position = 0;

            this.protoBufDataReader = DataSerializer.Deserialize(memoryStream);
        }
    }
}
