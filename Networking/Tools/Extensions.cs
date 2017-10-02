using System;

namespace Networking.Tools
{
    public static class Extensions
    {
        public static void Clear(this byte[] array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            for (var i = 0; i < array.Length; i++)
                array[i] = 0;
        }
    }
}