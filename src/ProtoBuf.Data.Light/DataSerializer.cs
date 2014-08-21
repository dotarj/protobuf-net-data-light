// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System.Data;
using System.IO;

namespace ProtoBuf.Data.Light
{
    public static class DataSerializer
    {
        public static void Serialize(Stream stream, IDataReader reader)
        {
            Throw.IfNull(stream, "stream");
            Throw.IfNull(reader, "reader");

            ProtoBufDataWriter.WriteReader(stream, reader);
        }

        public static IDataReader Deserialize(Stream stream)
        {
            Throw.IfNull(stream, "stream");

            return new ProtoBufDataReader(stream);
        }
    }
}
