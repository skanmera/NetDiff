namespace NetDiff
{
    public enum DiffStatus
    {
        Equal,
        Added,
        Removed,
    }

    public static class DiffStatusExtension
    {
        public static char GetStatusChar(this DiffStatus self)
        {
            switch (self)
            {
                case DiffStatus.Equal: return '=';
                case DiffStatus.Added: return '+';
                case DiffStatus.Removed: return '-';
            }

            throw new System.Exception();
        }
    }
}
