// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ProtoBuf.Data.Light
{
    [Serializable]
    public sealed class UnsupportedDataTypeException : Exception, ISerializable
    {
        private static readonly string supportedDataTypes;

        static UnsupportedDataTypeException()
        {
            var dataTypeNames = new string[TypeHelper.SupportedDataTypes.Length];

            for (var i = 0; i < TypeHelper.SupportedDataTypes.Length; i++)
            {
                dataTypeNames[i] = TypeHelper.SupportedDataTypes[i].Name;
            }

            supportedDataTypes = string.Join(", ", dataTypeNames);
        }

        internal UnsupportedDataTypeException(string dataTypeName)
            : base(string.Format("The data type '{0}' is not supported. The supported data types are: {1}.", dataTypeName, supportedDataTypes))
        {
            this.DataTypeName = dataTypeName;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private UnsupportedDataTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.DataTypeName = info.GetString("DataTypeName");
        }

        public string DataTypeName { get; private set; }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Throw.IfNull(info, "info");

            info.AddValue("DataTypeName", this.DataTypeName);

            base.GetObjectData(info, context);
        }
    }
}