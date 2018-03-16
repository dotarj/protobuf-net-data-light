﻿// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

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

        private IDataReader GetDataReader<TDataType>(TDataType value)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add(typeof(TDataType).Name, typeof(TDataType));

            dataTable.Rows.Add(value);

            var dataReader = dataTable.CreateDataReader();

            var memoryStream = new MemoryStream();

            DataSerializer.Serialize(memoryStream, dataReader);

            memoryStream.Position = 0;

            return DataSerializer.Deserialize(memoryStream);
        }
    }
}
