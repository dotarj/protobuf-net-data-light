// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

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
