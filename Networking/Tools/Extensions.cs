using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Networking.Tools
{
    public static class Extensions
    {
        public static void Clear([NotNull]this byte[] array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            for (var i = 0; i < array.Length; i++) array[i] = 0;
        }

        public static string StringJoin([NotNull] this IEnumerable<string> stringEnumerable, string seperator = "")
        {
            if (stringEnumerable == null) throw new ArgumentNullException(nameof(stringEnumerable));
            if (seperator == null) throw new ArgumentNullException(nameof(seperator));
            return string.Join(seperator, stringEnumerable);
        }
    }
}