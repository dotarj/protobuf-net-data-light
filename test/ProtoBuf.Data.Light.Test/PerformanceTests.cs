using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace ProtoBuf.Data.Light.Test
{
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void Compare()
        {
            var iterations = 100;
            var sqlDataReader = GetData("select top 100 * from Person.Person;");
            var dataTable = new DataTable();

            dataTable.Load(sqlDataReader);

            var sourceDataReaders = new List<IDataReader>();
            
            for(int i = 0; i < iterations; i++)
            {
                sourceDataReaders.Add(dataTable.CreateDataReader());
            }

            // Act
            var protoBufDataLight = Benchmark.RunParallel(i =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    DataSerializer.Serialize(memoryStream, sourceDataReaders[i]);

                    memoryStream.Position = 0;

                    var dataReader = DataSerializer.Deserialize(memoryStream);

                    while (dataReader.Read())
                    {
                    }
                }
            }, iterations).TotalMilliseconds;

            Console.WriteLine(string.Format("ProtoBuf.Data.Light: {0} ms", protoBufDataLight));

            sourceDataReaders.Clear();

            for (int i = 0; i < iterations; i++)
            {
                sourceDataReaders.Add(dataTable.CreateDataReader());
            }

            var protoBufData = Benchmark.RunParallel(i =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    ProtoBuf.Data.DataSerializer.Serialize(memoryStream, sourceDataReaders[i]);

                    memoryStream.Position = 0;

                    var dataReader = ProtoBuf.Data.DataSerializer.Deserialize(memoryStream);

                    while (dataReader.Read())
                    {
                    }
                }
            }, iterations).TotalMilliseconds;

            Console.WriteLine(string.Format("ProtoBuf.Data: {0} ms", protoBufData));

            Console.WriteLine(string.Format("{0} %", (int)(protoBufDataLight / protoBufData * 100)));
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