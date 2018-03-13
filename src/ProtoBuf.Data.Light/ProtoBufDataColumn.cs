// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;

namespace ProtoBuf.Data.Light
{
    internal sealed class ProtoBufDataColumn
    {
        public ProtoBufDataColumn(string name, int ordinal, Type dataType, ProtoBufDataType protoBufDataType)
        {
            this.Name = name;
            this.Ordinal = ordinal;
            this.DataType = dataType;
            this.ProtoBufDataType = protoBufDataType;
        }

        internal string Name { get; }

        internal int Ordinal { get; }

        internal Type DataType { get; }

        internal ProtoBufDataType ProtoBufDataType { get; }
    }
}
