// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace ProtoBuf.Data.Light
{
    internal static class TypeHelper
    {
        private static readonly Type[] SupportedDataTypes = new[]
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
            typeof(string),
            typeof(TimeSpan)
        };

        private static readonly IDictionary<Type, ProtoBufDataType> ProtoBufDataTypeByType = new Dictionary<Type, ProtoBufDataType>
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

        private static readonly IDictionary<ProtoBufDataType, Type> TypeByProtoBufDataType = new Dictionary<ProtoBufDataType, Type>
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

        private static string supportedDataTypes;

        internal static ProtoBufDataType GetProtoBufDataType(Type type)
        {
            ProtoBufDataType value;

            if (ProtoBufDataTypeByType.TryGetValue(type, out value))
            {
                return value;
            }

            throw new NotSupportedException(string.Format("The data type '{0}' is not supported. The supported data types are: {1}.", type.Name, supportedDataTypes));
        }

        internal static Type GetType(ProtoBufDataType type)
        {
            Type value;

            if (TypeByProtoBufDataType.TryGetValue(type, out value))
            {
                return value;
            }

            throw new InvalidDataException(string.Format("Undefined ProtoBufDataType '{0}'.", (int)type));
        }

        private static string GetSupportedDataTypes()
        {
            if (supportedDataTypes == null)
            {
                var dataTypeNames = new string[TypeHelper.SupportedDataTypes.Length];

                for (var i = 0; i < TypeHelper.SupportedDataTypes.Length; i++)
                {
                    dataTypeNames[i] = TypeHelper.SupportedDataTypes[i].Name;
                }

                supportedDataTypes = string.Join(", ", dataTypeNames);
            }

            return supportedDataTypes;
        }
    }
}
