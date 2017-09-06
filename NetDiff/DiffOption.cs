using System.Collections.Generic;

namespace NetDiff
{
    public class DiffOption<T>
    {
        public static DiffOption<T> Default
        {
            get
            {
                return new DiffOption<T>
                {
                    EqualityComparer = null,
                    Order = DiffOrder.GreedyDeleteFirst,
                    Limit = 1000,
                };
            }
        }

        public IEqualityComparer<T> EqualityComparer { get; set; }
        public DiffOrder Order { get; set; }
        public int Limit { get; set; }
    }
}
