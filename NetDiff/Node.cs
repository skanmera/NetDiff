namespace NetDiff
{
    internal class Node
    {
        public Point Point { get; }
        public Node Parent { get; set; }

        public Node(Point point)
        {
            Point = point;
        }

        public override string ToString()
        {
            return $"Point:{Point}";
        }
    }
}
