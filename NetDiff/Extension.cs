using System;
using System.Collections.Generic;

namespace NetDiff
{
    internal static class Extension
    {
        public static IEnumerable<Tuple<TSource, TSource>> MakePairsWithNext<TSource>(
                    this IEnumerable<TSource> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    var previous = enumerator.Current;
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        yield return new Tuple<TSource, TSource>(previous, current);

                        previous = current;
                    }
                }
            }
        }
    }
}
