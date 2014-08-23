// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ProtoBuf.Data.Light
{
    internal static class ProtoBufDataWriter
    {
        internal static void WriteReader(Stream stream, IDataReader reader)
        {
            var resultIndex = 0;

            using (var protoWriter = new ProtoWriter(stream, null, null))
            {
                do
                {
                    ProtoWriter.WriteFieldHeader(1, WireType.StartGroup, protoWriter);

                    var resultToken = ProtoWriter.StartSubItem(resultIndex, protoWriter);
                    var fieldInfos = GetFieldInfos(reader);

                    WriteFieldInfos(fieldInfos, protoWriter);
                    WriteRows(reader, fieldInfos, protoWriter);

                    ProtoWriter.EndSubItem(resultToken, protoWriter);

                    resultIndex++;
                }
                while (reader.NextResult());
            }
        }

        private static List<ProtoBufFieldInfo> GetFieldInfos(IDataReader dataReader)
        {
            var fieldInfos = new List<ProtoBufFieldInfo>(dataReader.FieldCount);

            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                fieldInfos.Add(new ProtoBufFieldInfo
                {
                    Name = dataReader.GetName(i),
                    Ordinal = i,
                    DataType = dataReader.GetFieldType(i)
                });
            }

            return fieldInfos;
        }

        private static void WriteFieldInfos(List<ProtoBufFieldInfo> fieldInfos, ProtoWriter writer)
        {
            foreach (var fieldInfo in fieldInfos)
            {
                ProtoWriter.WriteFieldHeader(2, WireType.StartGroup, writer);

                var fieldInfoToken = ProtoWriter.StartSubItem(fieldInfo, writer);

                ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
                ProtoWriter.WriteString(fieldInfo.Name, writer);
                ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                ProtoWriter.WriteInt32((int)TypeHelper.GetProtoBufDataType(fieldInfo.DataType), writer);

                ProtoWriter.EndSubItem(fieldInfoToken, writer);
            }
        }

        private static void WriteRows(IDataReader reader, List<ProtoBufFieldInfo> fieldInfos, ProtoWriter writer)
        {
            var rowIndex = 0;

            while (reader.Read())
            {
                var fieldIndex = 1;

                ProtoWriter.WriteFieldHeader(3, WireType.StartGroup, writer);

                var rowToken = ProtoWriter.StartSubItem(rowIndex, writer);

                foreach (var fieldInfo in fieldInfos)
                {
                    ProtoWriter.WriteFieldHeader(4, WireType.StartGroup, writer);

                    var fieldToken = ProtoWriter.StartSubItem(fieldIndex, writer);

                    var isDBNull = reader.IsDBNull(fieldInfo.Ordinal);

                    ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
                    ProtoWriter.WriteBoolean(isDBNull, writer);

                    if (!isDBNull)
                    {
                        var protoBufDataType = TypeHelper.GetProtoBufDataType(fieldInfo.DataType);

                        switch (protoBufDataType)
                        {
                            case ProtoBufDataType.Bool:
                                ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                                ProtoWriter.WriteBoolean(reader.GetBoolean(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.Byte:
                                ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                                ProtoWriter.WriteByte(reader.GetByte(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.ByteArray:
                                ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
                                var value = reader[fieldInfo.Ordinal];
                                ProtoWriter.WriteBytes((byte[])value, 0, ((byte[])value).Length, writer);
                                break;
                            case ProtoBufDataType.Char:
                                ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                                ProtoWriter.WriteInt16((short)reader.GetChar(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.CharArray:
                                ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
                                ProtoWriter.WriteString(new string((char[])reader[fieldInfo.Ordinal]), writer);
                                break;
                            case ProtoBufDataType.DateTime:
                                ProtoWriter.WriteFieldHeader(2, WireType.StartGroup, writer);
                                BclHelpers.WriteDateTime(reader.GetDateTime(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.Decimal:
                                ProtoWriter.WriteFieldHeader(2, WireType.StartGroup, writer);
                                BclHelpers.WriteDecimal(reader.GetDecimal(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.Double:
                                ProtoWriter.WriteFieldHeader(2, WireType.Fixed64, writer);
                                ProtoWriter.WriteDouble(reader.GetDouble(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.Float:
                                ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
                                ProtoWriter.WriteSingle(reader.GetFloat(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.Guid:
                                ProtoWriter.WriteFieldHeader(2, WireType.StartGroup, writer);
                                BclHelpers.WriteGuid(reader.GetGuid(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.Int:
                                ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                                ProtoWriter.WriteInt32(reader.GetInt32(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.Long:
                                ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                                ProtoWriter.WriteInt64(reader.GetInt64(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.Short:
                                ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
                                ProtoWriter.WriteInt16(reader.GetInt16(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.String:
                                ProtoWriter.WriteFieldHeader(2, WireType.String, writer);
                                ProtoWriter.WriteString(reader.GetString(fieldInfo.Ordinal), writer);
                                break;
                            case ProtoBufDataType.TimeSpan:
                                ProtoWriter.WriteFieldHeader(2, WireType.StartGroup, writer);
                                BclHelpers.WriteTimeSpan((TimeSpan)reader[fieldInfo.Ordinal], writer);
                                break;
                        }
                    }

                    ProtoWriter.EndSubItem(fieldToken, writer);

                    fieldIndex++;
                }

                ProtoWriter.EndSubItem(rowToken, writer);

                rowIndex++;
            }
        }
    }
}
