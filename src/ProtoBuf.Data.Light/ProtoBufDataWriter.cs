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
                var dataType = dataReader.GetFieldType(i);

                fieldInfos.Add(new ProtoBufFieldInfo
                {
                    Name = dataReader.GetName(i),
                    Ordinal = i,
                    DataType = dataType,
                    ProtoBufDataType = TypeHelper.GetProtoBufDataType(dataType)
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
                ProtoWriter.WriteInt32((int)fieldInfo.ProtoBufDataType, writer);

                ProtoWriter.EndSubItem(fieldInfoToken, writer);
            }
        }

        private static void WriteRows(IDataReader reader, List<ProtoBufFieldInfo> fieldInfos, ProtoWriter writer)
        {
            var rowIndex = 0;

            while (reader.Read())
            {
                ProtoWriter.WriteFieldHeader(3, WireType.StartGroup, writer);

                var rowToken = ProtoWriter.StartSubItem(rowIndex, writer);

                ProtoWriter.WriteFieldHeader(5, WireType.StartGroup, writer);

                var fieldValuesToken = ProtoWriter.StartSubItem(5, writer);

                for (var i = 0; i < fieldInfos.Count; i++)
                {
                    var fieldIndex = i + 1;

                    if (!reader.IsDBNull(fieldInfos[i].Ordinal))
                    {
                        switch (fieldInfos[i].ProtoBufDataType)
                        {
                            case ProtoBufDataType.Bool:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.Variant, writer);
                                ProtoWriter.WriteBoolean(reader.GetBoolean(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.Byte:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.Variant, writer);
                                ProtoWriter.WriteByte(reader.GetByte(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.ByteArray:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.String, writer);
                                var value = reader[fieldInfos[i].Ordinal];
                                ProtoWriter.WriteBytes((byte[])value, 0, ((byte[])value).Length, writer);
                                break;
                            case ProtoBufDataType.Char:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.Variant, writer);
                                ProtoWriter.WriteInt16((short)reader.GetChar(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.CharArray:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.String, writer);
                                ProtoWriter.WriteString(new string((char[])reader[fieldInfos[i].Ordinal]), writer);
                                break;
                            case ProtoBufDataType.DateTime:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.StartGroup, writer);
                                BclHelpers.WriteDateTime(reader.GetDateTime(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.Decimal:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.StartGroup, writer);
                                BclHelpers.WriteDecimal(reader.GetDecimal(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.Double:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.Fixed64, writer);
                                ProtoWriter.WriteDouble(reader.GetDouble(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.Float:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.Fixed32, writer);
                                ProtoWriter.WriteSingle(reader.GetFloat(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.Guid:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.StartGroup, writer);
                                BclHelpers.WriteGuid(reader.GetGuid(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.Int:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.Variant, writer);
                                ProtoWriter.WriteInt32(reader.GetInt32(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.Long:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.Variant, writer);
                                ProtoWriter.WriteInt64(reader.GetInt64(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.Short:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.Variant, writer);
                                ProtoWriter.WriteInt16(reader.GetInt16(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.String:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.String, writer);
                                ProtoWriter.WriteString(reader.GetString(fieldInfos[i].Ordinal), writer);
                                break;
                            case ProtoBufDataType.TimeSpan:
                                ProtoWriter.WriteFieldHeader(fieldIndex, WireType.StartGroup, writer);
                                BclHelpers.WriteTimeSpan((TimeSpan)reader[fieldInfos[i].Ordinal], writer);
                                break;
                        }
                    }
                }

                ProtoWriter.EndSubItem(fieldValuesToken, writer);

                ProtoWriter.EndSubItem(rowToken, writer);

                rowIndex++;
            }
        }
    }
}
