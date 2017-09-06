namespace NetDiff
{
    public enum DiffStatus
    {
        Equal,
        Inserted,
        Deleted,
    }

    public static class DiffStatusExtension
    {
        public static char GetStatusChar(this DiffStatus self)
        {
            switch (self)
            {
                case DiffStatus.Equal: return '=';
                case DiffStatus.Inserted: return '+';
                case DiffStatus.Deleted: return '-';
            }

            throw new System.Exception();
        }
    }
}
