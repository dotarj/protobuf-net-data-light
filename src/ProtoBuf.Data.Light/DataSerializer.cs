// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System.Data;
using System.IO;

namespace ProtoBuf.Data.Light
{
    public static class DataSerializer
    {
        /// <summary>
        /// Serializes the <see cref="IDataReader"/> to the given stream.
        /// </summary>
        /// <param name="stream">The stream to which the <see cref="IDataReader"/> is to be serialized.</param>
        /// <param name="reader">The <see cref="IDataReader"/> to serialize.</param>
        /// <exception cref="ArgumentNullException">The stream is null. -or-The reader is null.</exception>
        /// <exception cref="UnsupportedDataTypeException">A data type is not supperted.</exception>
        public static void Serialize(Stream stream, IDataReader reader)
        {
            Throw.IfNull(stream, "stream");
            Throw.IfNull(reader, "reader");

            ProtoBufDataWriter.WriteReader(stream, reader);
        }

        /// <summary>
        /// Deserializes the specified stream into an <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="stream">The stream from which to deserialize the <see cref="IDataReader"/>.</param>
        /// <returns>The <see cref="IDataReader"/>.</returns>
        /// <exception cref="InvalidDataException">The stream has an unexpected format.</exception>
        public static IDataReader Deserialize(Stream stream)
        {
            Throw.IfNull(stream, "stream");

            return new ProtoBufDataReader(stream);
        }
    }
}
