using System;
using System.Collections.Generic;

namespace MogBot.Host.Extensions
{
    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }
    }
}