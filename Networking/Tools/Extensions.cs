using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Text;

namespace Networking.Tools
{
    public static class Extensions
    {
        public static void Clear([NotNull]this byte[] array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            for (var i = 0; i < array.Length; i++) array[i] = 0;
        }

        [NotNull]
        public static string StringJoin([NotNull] this IEnumerable<string> stringEnumerable, string seperator = "")
        {
            if (stringEnumerable == null) throw new ArgumentNullException(nameof(stringEnumerable));
            if (seperator == null) throw new ArgumentNullException(nameof(seperator));
            return string.Join(seperator, stringEnumerable);
        }

        [NotNull]
        public static string GetShallowExceptionMessages([NotNull]this AggregateException aggregateException)
        {
            if (aggregateException == null) throw new ArgumentNullException(nameof(aggregateException));
            var messageBuilder = new StringBuilder();
            for (var i = 0; i < aggregateException.InnerExceptions.Count; i++)
            {
                messageBuilder.Append(aggregateException.InnerExceptions[i].Message);
                if (i + 1 == aggregateException.InnerExceptions.Count) continue;
                messageBuilder.AppendLine();
            }
            return messageBuilder.ToString();
        }

        public static TValue Get<TKey, TValue>([NotNull]this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            var itemExists = dictionary.TryGetValue(key, out TValue tValue);
            return itemExists ? tValue : default;
        }
    }
}