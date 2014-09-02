// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.Collections.Generic;
using System.IO;

namespace ProtoBuf.Data.Light
{
    internal static class TypeHelper
    {
        internal static readonly Type[] SupportedDataTypes = new []
        {
            typeof(bool),
            typeof(byte),
            typeof(byte[]),
            typeof(char),
            typeof(char[]),
            typeof(DateTime),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(Guid),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(String),
            typeof(TimeSpan)
        };

        private static readonly IDictionary<Type, ProtoBufDataType> ProtoBufTypeByType = new Dictionary<Type, ProtoBufDataType>
        {
            { typeof(bool), ProtoBufDataType.Bool },
            { typeof(byte), ProtoBufDataType.Byte },
            { typeof(DateTime), ProtoBufDataType.DateTime },
            { typeof(double), ProtoBufDataType.Double },
            { typeof(float), ProtoBufDataType.Float },
            { typeof(Guid), ProtoBufDataType.Guid },
            { typeof(int), ProtoBufDataType.Int },
            { typeof(long), ProtoBufDataType.Long },
            { typeof(short), ProtoBufDataType.Short },
            { typeof(string), ProtoBufDataType.String },
            { typeof(char), ProtoBufDataType.Char },
            { typeof(decimal), ProtoBufDataType.Decimal },
            { typeof(byte[]), ProtoBufDataType.ByteArray },
            { typeof(char[]), ProtoBufDataType.CharArray },
            { typeof(TimeSpan), ProtoBufDataType.TimeSpan },
        };

        private static readonly IDictionary<ProtoBufDataType, Type> TypeByProtoBufType = new Dictionary<ProtoBufDataType, Type>
        {
            { ProtoBufDataType.Bool, typeof(bool) },
            { ProtoBufDataType.Byte, typeof(byte) },
            { ProtoBufDataType.DateTime, typeof(DateTime) },
            { ProtoBufDataType.Double, typeof(double) },
            { ProtoBufDataType.Float, typeof(float) },
            { ProtoBufDataType.Guid, typeof(Guid) },
            { ProtoBufDataType.Int, typeof(int) },
            { ProtoBufDataType.Long, typeof(long) },
            { ProtoBufDataType.Short, typeof(short) },
            { ProtoBufDataType.String, typeof(string) },
            { ProtoBufDataType.Char, typeof(char) },
            { ProtoBufDataType.Decimal, typeof(decimal) },
            { ProtoBufDataType.ByteArray, typeof(byte[]) },
            { ProtoBufDataType.CharArray, typeof(char[]) },
            { ProtoBufDataType.TimeSpan, typeof(TimeSpan) },
        };

        internal static ProtoBufDataType GetProtoBufDataType(Type type)
        {
            ProtoBufDataType value;

            if (ProtoBufTypeByType.TryGetValue(type, out value))
            {
                return value;
            }

            throw new UnsupportedDataTypeException(type.ToString());
        }

        internal static Type GetType(ProtoBufDataType type)
        {
            Type value;

            if (TypeByProtoBufType.TryGetValue(type, out value))
            {
                return value;
            }

            throw new InvalidDataException(string.Format("Undefined ProtoBufDataType '{0}'.", (int)type));
        }
    }
}
