// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace ProtoBuf.Data.Light.Test
{
    [TestClass]
    public class DataSerializerTests
    {
        private IDataReader protoBufDataReader;

        [TestInitialize]
        public void TestInitialize()
        {
            var dataReaderMock = new DataReaderMock();
            var memoryStream = new MemoryStream();

            DataSerializer.Serialize(memoryStream, dataReaderMock);
            
            memoryStream.Position = 0;

            this.protoBufDataReader = DataSerializer.Deserialize(memoryStream);
        }

        [TestClass]
        public class TheSerializeMethod : DataSerializerTests
        {
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void ShouldThrowExceptionIfStreamIsNull()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock();

                // Act
                DataSerializer.Serialize(null, dataReaderMock);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void ShouldThrowExceptionIfDataReaderIsNull()
            {
                // Arrange
                var memoryStream = new MemoryStream();

                // Act
                DataSerializer.Serialize(memoryStream, null);
            }

            [TestMethod]
            public void ShouldDeserializeTwoRows()
            {
                // Arrange
                var readCount = 0;

                // Act
                while (protoBufDataReader.Read())
                {
                    readCount++;
                }

                // Assert
                Assert.AreEqual(2, readCount);
            }

            [TestMethod]
            public void ShouldDeserializeValidProperties()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock();

                // Assert
                dataReaderMock = new DataReaderMock();

                while (protoBufDataReader.Read())
                {
                    dataReaderMock.Read();

                    Assert.AreEqual(dataReaderMock.GetBoolean(0), protoBufDataReader.GetBoolean(0));
                    Assert.AreEqual(dataReaderMock.GetByte(1), protoBufDataReader.GetByte(1));
                    Assert.AreEqual(dataReaderMock.GetDateTime(2), protoBufDataReader.GetDateTime(2));
                    Assert.AreEqual(dataReaderMock.GetDouble(3), protoBufDataReader.GetDouble(3));
                    Assert.AreEqual(dataReaderMock.GetFloat(4), protoBufDataReader.GetFloat(4));
                    Assert.AreEqual(dataReaderMock.GetGuid(5), protoBufDataReader.GetGuid(5));
                    Assert.AreEqual(dataReaderMock.GetInt32(6), protoBufDataReader.GetInt32(6));
                    Assert.AreEqual(dataReaderMock.GetInt64(7), protoBufDataReader.GetInt64(7));
                    Assert.AreEqual(dataReaderMock.GetInt16(8), protoBufDataReader.GetInt16(8));
                    Assert.AreEqual(dataReaderMock.GetString(9), protoBufDataReader.GetString(9));
                    Assert.AreEqual(dataReaderMock.GetChar(10), protoBufDataReader.GetChar(10));
                    Assert.AreEqual(dataReaderMock.GetDecimal(11), protoBufDataReader.GetDecimal(11));
                    //Assert.AreEqual(dataReaderMock.GetChars(12), dataReader.GetChars(12));
                    //Assert.AreEqual(dataReaderMock.GetBytes(13), dataReader.GetBytes(13));
                    Assert.AreEqual((TimeSpan)dataReaderMock[14], (TimeSpan)protoBufDataReader[14]);
                }
            }

            [TestMethod]
            public void Performance()
            {
                var iterations = 100000;
                var sqlDataReader = GetData("select top 100 * from Person.Person;");
                var dataTable = new DataTable();

                dataTable.Load(sqlDataReader);
                
                var originalDataReader = dataTable.CreateDataReader();

                // Act
                Console.WriteLine(string.Format("ProtoBuf.Data.Light: {0} ms", Benchmark.RunParallel(() =>
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        DataSerializer.Serialize(memoryStream, originalDataReader);

                        memoryStream.Position = 0;

                        var dataReader = DataSerializer.Deserialize(memoryStream);

                        while (dataReader.Read())
                        {
                        }
                    }
                }, iterations).TotalMilliseconds));

                Console.WriteLine(string.Format("ProtoBuf.Data:       {0} ms", Benchmark.RunParallel(() =>
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        ProtoBuf.Data.DataSerializer.Serialize(memoryStream, originalDataReader);

                        memoryStream.Position = 0;

                        var dataReader = ProtoBuf.Data.DataSerializer.Deserialize(memoryStream);

                        while (dataReader.Read())
                        {
                        }
                    }
                }, iterations).TotalMilliseconds));
            }
        }

        private SqlDataReader GetData(string commandText)
        {
            var connectionString = new SqlConnectionStringBuilder
            {
                DataSource = @".\SQLEXPRESS",
                InitialCatalog = "AdventureWorks2012",
                IntegratedSecurity = true
            };

            var sqlConnection = new SqlConnection(connectionString.ConnectionString);

            using (var sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = commandText;

                sqlConnection.Open();

                return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }
    }
}
