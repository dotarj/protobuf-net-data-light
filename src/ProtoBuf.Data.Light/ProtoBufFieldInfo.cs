// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;

namespace ProtoBuf.Data.Light
{
    internal sealed class ProtoBufFieldInfo
    {
        internal string Name;

        internal int Ordinal;

        internal Type DataType;
    }
}
