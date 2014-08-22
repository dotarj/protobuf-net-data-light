// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.IO;

namespace ProtoBuf.Data.Light.Test
{
    [TestClass]
    public class ProtoBufDataReaderTests
    {
        private IDataReader protoBufDataReader;

        [TestInitialize]
        public void TestInitialize()
        {
            var dataReaderMock = new DataReaderMock(false);
            var memoryStream = new MemoryStream();

            DataSerializer.Serialize(memoryStream, dataReaderMock);

            memoryStream.Position = 0;

            this.protoBufDataReader = DataSerializer.Deserialize(memoryStream);
        }

        [TestClass]
        public class TheGetFieldTypeMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetFieldType(0);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetFieldType(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingFieldType()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.FieldCount, protoBufDataReader.FieldCount);

                for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.AreEqual(dataReaderMock.GetFieldType(i), protoBufDataReader.GetFieldType(i));
                }
            }
        }

        [TestClass]
        public class TheGetNameMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetName(0);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetName(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingName()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                dataReaderMock.Read();
                protoBufDataReader.Read();

                // Assert
                Assert.AreEqual(dataReaderMock.FieldCount, protoBufDataReader.FieldCount);

                for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                {
                    Assert.AreEqual(dataReaderMock.GetName(i), protoBufDataReader.GetName(i));
                }
            }
        }

        [TestClass]
        public class TheGetOrdinalMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetOrdinal("bool");
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetOrdinal("nonexistent");
            }

            [TestMethod]
            public void ShouldReturnCorrespondingOrdinal()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);
                var schemaTableMock = dataReaderMock.GetSchemaTable();

                // Assert
                for (int i = 0; i < schemaTableMock.Rows.Count; i++)
                {
                    Assert.AreEqual(dataReaderMock.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()), protoBufDataReader.GetOrdinal(schemaTableMock.Rows[i]["ColumnName"].ToString()));
                }
            }
        }

        [TestClass]
        public class TheGetValueMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetValue(0);
            }

            [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
            public void ShouldThrowExceptionWhenIndexIsOutOfRange()
            {
                // Act
                protoBufDataReader.GetValue(protoBufDataReader.FieldCount);
            }

            [TestMethod]
            public void ShouldReturnCorrespondingOrdinal()
            {
                // Arrange
                var dataReaderMock = new DataReaderMock(false);

                // Assert
                while (protoBufDataReader.Read())
                {
                    dataReaderMock.Read();

                    for (int i = 0; i < protoBufDataReader.FieldCount; i++)
                    {
                        Assert.AreEqual(dataReaderMock.GetValue(i).ToString(), protoBufDataReader.GetValue(i).ToString());
                    }
                }
            }
        }

        [TestClass]
        public class TheGetSchemaTableMethod : ProtoBufDataReaderTests
        {
            [TestMethod, ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionWhenDataReaderIsClosed()
            {
                // Arrange
                protoBufDataReader.Close();

                // Act
                protoBufDataReader.GetSchemaTable();
            }

            [TestMethod]
            public void ShouldReturnSchemaTable()
            {
                // Arrange
                var schemaTableMock = new DataReaderMock(false).GetSchemaTable();

                // Act
                var schemaTable = protoBufDataReader.GetSchemaTable();

                // Assert
                Assert.IsNotNull(schemaTable);
                Assert.AreEqual(schemaTableMock.Rows.Count, schemaTable.Rows.Count);

                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    Assert.AreEqual(schemaTableMock.Rows[i]["ColumnName"].ToString(), schemaTable.Rows[i]["ColumnName"].ToString());
                    Assert.AreEqual((int)schemaTableMock.Rows[i]["ColumnOrdinal"], (int)schemaTable.Rows[i]["ColumnOrdinal"]);
                    Assert.AreEqual(schemaTableMock.Rows[i]["DataTypeName"].ToString(), schemaTable.Rows[i]["DataTypeName"].ToString());
                }
            }
        }
    }
}
