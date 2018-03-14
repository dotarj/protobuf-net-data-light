// Copyright (c) Arjen Post. See LICENSE and NOTICE in the project root for license information.

using System;

namespace ProtoBuf.Data.Light
{
    internal static class Throw
    {
        public static void IfNull<T>(T value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
