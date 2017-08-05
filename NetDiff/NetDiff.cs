using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDiff
{
    public class NetDiff
    {
        public static IEnumerable<DiffResult<T>> Diff<T>(IEnumerable<T> seq1, IEnumerable<T> seq2, IEqualityComparer<T> compare = null)
        {
            var editGrap = new EditGraph<T>(seq1, seq2, compare);
            var waypoints = editGrap.Snake();

            return MakeResults<T>(waypoints, seq1, seq2);
        }

        private static IEnumerable<DiffResult<T>> MakeResults<T>(IEnumerable<Point> waypoints, IEnumerable<T> seq1, IEnumerable<T> seq2)
        {
            foreach (var pair in waypoints.MakePairsWithNext())
            {
                var status = GetStatus(pair.Item1, pair.Item2);
                T obj1 = default(T);
                T obj2 = default(T);
                switch (status)
                {
                    case DiffStatus.Equal:
                        obj1 = seq1.ElementAt(pair.Item2.X - 1);
                        obj2 = seq2.ElementAt(pair.Item2.Y - 1);
                        break;
                    case DiffStatus.Added:
                        obj2 = seq2.ElementAt(pair.Item2.Y - 1);
                        break;
                    case DiffStatus.Removed:
                        obj1 = seq1.ElementAt(pair.Item2.X - 1);
                        break;
                }

                yield return new DiffResult<T>(obj1, obj2, status);
            }
        }

        private static DiffStatus GetStatus(Point current, Point prev)
        {
            if (current.X != prev.X && current.Y != prev.Y)
                return DiffStatus.Equal;
            else if (current.X != prev.X)
                return DiffStatus.Removed;
            else if (current.Y != prev.Y)
                return DiffStatus.Added;
            else
                throw new Exception();
        }
    }
}
