// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System.Data;
using System.IO;

namespace ProtoBuf.Data.Light.Tests
{
    public partial class ProtoBufDataReaderTests
    {
        private IDataReader protoBufDataReader;

        public ProtoBufDataReaderTests()
        {
            var dataReaderMock = new DataReaderMock(false);
            var memoryStream = new MemoryStream();

            DataSerializer.Serialize(memoryStream, dataReaderMock);

            memoryStream.Position = 0;

            this.protoBufDataReader = DataSerializer.Deserialize(memoryStream);
        }

        private ProtoBufDataReader GetDataReader<TDataType>(TDataType value)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add(typeof(TDataType).Name, typeof(TDataType));

            dataTable.Rows.Add(value);

            return this.ToProtoBufDataReader(dataTable.CreateDataReader());
        }

        private ProtoBufDataReader ToProtoBufDataReader(IDataReader dataReader)
        {
            var memoryStream = new MemoryStream();

            DataSerializer.Serialize(memoryStream, dataReader);

            memoryStream.Position = 0;

            return (ProtoBufDataReader)DataSerializer.Deserialize(memoryStream);
        }
    }
}
