// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace ProtoBuf.Data.Light
{
    internal sealed class ProtoBufDataBuffer
    {
        private ValueTypeBuffer valueTypeBuffer;
        private BufferType bufferType;
        private object referenceTypeBuffer;

        internal ProtoBufDataBuffer()
        {
            this.IsNull = true;
        }

        private enum BufferType
        {
            Empty = 0,
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

        internal bool IsNull { get; set; }

        internal bool Bool
        {
            get
            {
                if (this.bufferType == BufferType.Bool)
                {
                    return this.valueTypeBuffer.Bool;
                }

                return (bool)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Bool;

                this.valueTypeBuffer.Bool = value;
            }
        }

        internal byte Byte
        {
            get
            {
                if (this.bufferType == BufferType.Byte)
                {
                    return this.valueTypeBuffer.Byte;
                }

                return (byte)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Byte;

                this.valueTypeBuffer.Byte = value;
            }
        }

        internal byte[] ByteArray
        {
            get
            {
                if (this.bufferType == BufferType.ByteArray)
                {
                    return (byte[])this.referenceTypeBuffer;
                }

                return (byte[])this.Value;
            }

            set
            {
                this.bufferType = BufferType.ByteArray;

                this.referenceTypeBuffer = value;
            }
        }

        internal char Char
        {
            get
            {
                if (this.bufferType == BufferType.Char)
                {
                    return this.valueTypeBuffer.Char;
                }

                return (char)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Char;

                this.valueTypeBuffer.Char = value;
            }
        }

        internal char[] CharArray
        {
            get
            {
                if (this.bufferType == BufferType.CharArray)
                {
                    return (char[])this.referenceTypeBuffer;
                }

                return (char[])this.Value;
            }

            set
            {
                this.bufferType = BufferType.CharArray;

                this.referenceTypeBuffer = value;
            }
        }

        internal DateTime DateTime
        {
            get
            {
                if (this.bufferType == BufferType.DateTime)
                {
                    return this.valueTypeBuffer.DateTime;
                }

                return (DateTime)this.Value;
            }

            set
            {
                this.bufferType = BufferType.DateTime;

                this.valueTypeBuffer.DateTime = value;
            }
        }

        internal decimal Decimal
        {
            get
            {
                if (this.bufferType == BufferType.Decimal)
                {
                    return this.valueTypeBuffer.Decimal;
                }

                return (decimal)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Decimal;

                this.valueTypeBuffer.Decimal = value;
            }
        }

        internal double Double
        {
            get
            {
                if (this.bufferType == BufferType.Double)
                {
                    return this.valueTypeBuffer.Double;
                }

                return (double)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Double;

                this.valueTypeBuffer.Double = value;
            }
        }

        internal float Float
        {
            get
            {
                if (this.bufferType == BufferType.Float)
                {
                    return this.valueTypeBuffer.Float;
                }

                return (float)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Float;

                this.valueTypeBuffer.Float = value;
            }
        }

        internal Guid Guid
        {
            get
            {
                if (this.bufferType == BufferType.Guid)
                {
                    return this.valueTypeBuffer.Guid;
                }

                return (Guid)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Guid;

                this.valueTypeBuffer.Guid = value;
            }
        }

        internal int Int
        {
            get
            {
                if (this.bufferType == BufferType.Int)
                {
                    return this.valueTypeBuffer.Int;
                }

                return (int)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Int;

                this.valueTypeBuffer.Int = value;
            }
        }

        internal long Long
        {
            get
            {
                if (this.bufferType == BufferType.Long)
                {
                    return this.valueTypeBuffer.Long;
                }

                return (long)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Long;

                this.valueTypeBuffer.Long = value;
            }
        }

        internal short Short
        {
            get
            {
                if (this.bufferType == BufferType.Short)
                {
                    return this.valueTypeBuffer.Short;
                }

                return (short)this.Value;
            }

            set
            {
                this.bufferType = BufferType.Short;

                this.valueTypeBuffer.Short = value;
            }
        }

        internal string String
        {
            get
            {
                if (this.bufferType == BufferType.String)
                {
                    return (string)this.referenceTypeBuffer;
                }

                return (string)this.Value;
            }

            set
            {
                this.bufferType = BufferType.String;

                this.referenceTypeBuffer = value;
            }
        }

        internal TimeSpan TimeSpan
        {
            get
            {
                if (this.bufferType == BufferType.TimeSpan)
                {
                    return this.valueTypeBuffer.TimeSpan;
                }

                return (TimeSpan)this.Value;
            }

            set
            {
                this.bufferType = BufferType.TimeSpan;

                this.valueTypeBuffer.TimeSpan = value;
            }
        }

        internal object Value
        {
            get
            {
                if (this.IsNull)
                {
                    return DBNull.Value;
                }

                switch (this.bufferType)
                {
                    case BufferType.Empty: return DBNull.Value;
                    case BufferType.Bool: return this.Bool;
                    case BufferType.Byte: return this.Byte;
                    case BufferType.ByteArray: return this.ByteArray;
                    case BufferType.Char: return this.Char;
                    case BufferType.CharArray: return this.CharArray;
                    case BufferType.DateTime: return this.DateTime;
                    case BufferType.Decimal: return this.Decimal;
                    case BufferType.Double: return this.Double;
                    case BufferType.Float: return this.Float;
                    case BufferType.Guid: return this.Guid;
                    case BufferType.Int: return this.Int;
                    case BufferType.Long: return this.Long;
                    case BufferType.Short: return this.Short;
                    case BufferType.String: return this.String;
                    case BufferType.TimeSpan: return this.TimeSpan;
                }

                return null;
            }
        }

        internal static void Clear(ProtoBufDataBuffer[] buffers)
        {
            for (var i = 0; i < buffers.Length; i++)
            {
                buffers[i].Clear();
            }
        }

        internal static void Initialize(ProtoBufDataBuffer[] buffers)
        {
            for (var i = 0; i < buffers.Length; i++)
            {
                buffers[i] = new ProtoBufDataBuffer();
            }
        }

        private void Clear()
        {
            this.referenceTypeBuffer = null;
            this.IsNull = true;
            this.bufferType = BufferType.Empty;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct ValueTypeBuffer
        {
            [FieldOffset(0)]
            internal bool Bool;
            [FieldOffset(0)]
            internal byte Byte;
            [FieldOffset(0)]
            internal char Char;
            [FieldOffset(0)]
            internal DateTime DateTime;
            [FieldOffset(0)]
            internal decimal Decimal;
            [FieldOffset(0)]
            internal double Double;
            [FieldOffset(0)]
            internal float Float;
            [FieldOffset(0)]
            internal Guid Guid;
            [FieldOffset(0)]
            internal int Int;
            [FieldOffset(0)]
            internal long Long;
            [FieldOffset(0)]
            internal short Short;
            [FieldOffset(0)]
            internal TimeSpan TimeSpan;
        }
    }
}
