using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDiff
{
    public class DiffUtil
    {
        public static IEnumerable<DiffResult<T>> Diff<T>(IEnumerable<T> seq1, IEnumerable<T> seq2)
        {
            return Diff(seq1, seq2, DiffOption<T>.Default);
        }

        public static IEnumerable<DiffResult<T>> Diff<T>(IEnumerable<T> seq1, IEnumerable<T> seq2, DiffOption<T> option)
        {
            if (seq1 == null || seq2 == null || (!seq1.Any() && !seq2.Any()))
                return Enumerable.Empty<DiffResult<T>>();

            var editGrap = new EditGraph<T>(seq1, seq2);
            var waypoints = editGrap.CalculatePath(option);

            return MakeResults<T>(waypoints, seq1, seq2);
        }

        public static IEnumerable<T> CreateSrc<T>(IEnumerable<DiffResult<T>> diffResults)
        {
            return diffResults.Where(r => r.Status != DiffStatus.Inserted).Select(r => r.Obj1);
        }

        public static IEnumerable<T> CreateDst<T>(IEnumerable<DiffResult<T>> diffResults)
        {
            return diffResults.Where(r => r.Status != DiffStatus.Deleted).Select(r => r.Obj2);
        }

        private static IEnumerable<DiffResult<T>> MakeResults<T>(IEnumerable<Point> waypoints, IEnumerable<T> seq1, IEnumerable<T> seq2)
        {
            var array1 = seq1.ToArray();
            var array2 = seq2.ToArray();

            foreach (var pair in waypoints.MakePairsWithNext())
            {
                var status = GetStatus(pair.Item1, pair.Item2);
                T obj1 = default(T);
                T obj2 = default(T);
                switch (status)
                {
                    case DiffStatus.Equal:
                        obj1 = array1[pair.Item2.X - 1];
                        obj2 = array2[pair.Item2.Y - 1];
                        break;
                    case DiffStatus.Inserted:
                        obj2 = array2[pair.Item2.Y - 1];
                        break;
                    case DiffStatus.Deleted:
                        obj1 = array1[pair.Item2.X - 1];
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
                return DiffStatus.Deleted;
            else if (current.Y != prev.Y)
                return DiffStatus.Inserted;
            else
                throw new Exception();
        }
    }
}
