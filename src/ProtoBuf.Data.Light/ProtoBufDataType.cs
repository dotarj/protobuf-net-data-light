// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

namespace ProtoBuf.Data.Light
{
    internal enum ProtoBufDataType
    {
        Bool = 1,
        Byte = 2,
        ByteArray = 3,
        Char = 4,
        CharArray = 5,
        DateTime = 6,
        Decimal = 7,
        Double = 8,
        Float = 9,
        Guid = 10,
        Int = 11,
        Long = 12,
        Short = 13,
        String = 14,
        TimeSpan = 15
    }
}
