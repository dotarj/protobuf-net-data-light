// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System.Data;
using System.IO;

namespace ProtoBuf.Data.Light
{
    /// <summary>
    /// Serializes and deserializes an <see cref="IDataReader"/> in binary format.
    /// </summary>
    public static class DataSerializer
    {
        /// <summary>
        /// Serializes the <see cref="IDataReader"/> to the given stream.
        /// </summary>
        /// <param name="stream">The stream to which the <see cref="IDataReader"/> is to be serialized.</param>
        /// <param name="reader">The <see cref="IDataReader"/> to serialize.</param>
        /// <exception cref="System.ArgumentNullException">The stream is null. -or-The reader is null.</exception>
        /// <exception cref="System.NotSupportedException">A data type is not supperted.</exception>
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
