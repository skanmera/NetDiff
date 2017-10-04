using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDiff
{
    internal enum Direction
    {
        Right,
        Bottom,
        Diagonal,
    }

    internal struct Point : IEquatable<Point>
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;

            return Equals((Point)obj);
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();

            return hash;
        }

        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override string ToString()
        {
            return $"X:{X} Y:{Y}";
        }
    }

    internal class Node
    {
        public Point Point { get; set; }
        public Node Parent { get; set; }
        public int ScoreX { get; set; }
        public int ScoreY { get; set; }
        public int StraightCount { get; set; }

        public Node(Point point)
        {
            Point = point;
        }

        public override string ToString()
        {
            return $"X:{Point.X} Y:{Point.Y} ScoreX:{ScoreX} ScoreY:{ScoreY} Straight:{StraightCount}";
        }
    }

    internal class EditGraph<T>
    {
        private T[] seq1;
        private T[] seq2;
        private DiffOption<T> option;
        private List<Node> heads;
        private Point endpoint;
        private int editDistance;
        private int[] editDistances;
        private int offset;
        private bool isEnd;

        public EditGraph(
            IEnumerable<T> seq1, IEnumerable<T> seq2)
        {
            this.seq1 = seq1.ToArray();
            this.seq2 = seq2.ToArray();
            endpoint = new Point(this.seq1.Length, this.seq2.Length);
            offset = this.seq2.Length;
        }

        public List<Point> CalculatePath(DiffOption<T> option)
        {
            this.option = option;

            BeginCalculatePath();

            editDistance = 1;

            while (Next()) { editDistance++; }

            return EndCalculatePath();
        }

        private void Initialize()
        {
            editDistances = new int[seq1.Length + seq2.Length + 1];
            heads = new List<Node>();
        }

        private void BeginCalculatePath()
        {
            Initialize();

            heads.Add(new Node(new Point(0, 0)));

            Snake();
        }

        private List<Point> EndCalculatePath()
        {
            var wayponit = new List<Point>();

            var current = heads.Where(h => h.Point.Equals(endpoint)).FirstOrDefault();
            while (current != null)
            {
                wayponit.Add(current.Point);

                current = current.Parent;
            }

            wayponit.Reverse();

            return wayponit;
        }

        private bool Next()
        {
            if (isEnd)
                return false;

            UpdateHeads();

            return true;
        }

        private void UpdateHeads()
        {
            if (option.Limit > 0 && heads.Count > option.Limit)
            {
                var selectedNode = SelectNode(heads);
                heads.Clear();
                heads.Add(selectedNode);
            }

            var updated = new List<Node>();

            foreach (var head in heads)
            {
                Node rightHead;
                if (TryCreateHead(head, Direction.Right, out rightHead))
                {
                    updated.Add(rightHead);
                }

                Node bottomHead;
                if (TryCreateHead(head, Direction.Bottom, out bottomHead))
                {
                    updated.Add(bottomHead);
                }
            }

            heads.Clear();

            var samePointHeadGroups = updated.GroupBy(n => n.Point);
            foreach (var samePointHeads in samePointHeadGroups)
            {
                var selectedNode = SelectNode(samePointHeads);
                heads.Add(selectedNode);
            }

            Snake();
        }

        private Node SelectNode(IEnumerable<Node> nodes)
        {
            switch (option.Order)
            {
                case DiffOrder.GreedyDeleteFirst:
                    return nodes.FindMin(n => n.ScoreY);
                case DiffOrder.GreedyInsertFirst:
                    return nodes.FindMin(n => n.ScoreX);
                case DiffOrder.LazyDeleteFirst:
                    return nodes.FindMins(n => n.StraightCount).FindMin(n => n.ScoreY);
                case DiffOrder.LazyInsertFirst:
                    return nodes.FindMins(n => n.StraightCount).FindMin(n => n.ScoreX);
            }

            return nodes.FirstOrDefault();
        }

        public void UpdateScore(Node node, Point prev)
        {
            node.ScoreX += node.Point.X;
            node.ScoreY += node.Point.Y;

            if (node.Parent != null && node.Parent.Parent != null)
            {
                if (node.Point.X == node.Parent.Point.X && node.Point.X == node.Parent.Parent.Point.X ||
                    node.Point.Y == node.Parent.Point.Y && node.Point.Y == node.Parent.Parent.Point.Y)
                {
                    node.StraightCount++;
                }
            }
        }

        private void Snake()
        {
            var tmp = new List<Node>();
            foreach (var h in heads)
            {
                tmp.Add(h);
                var newHead = Snake(h);

                if (newHead != null)
                    tmp.Add(newHead);
            }

            heads = tmp;
        }

        private Node Snake(Node head)
        {
            Node newHead = null;
            while (true)
            {
                Node tmp;
                if (TryCreateHead(newHead ?? head, Direction.Diagonal, out tmp))
                    newHead = tmp;
                else
                    break;
            }

            return newHead;
        }

        private bool TryCreateHead(Node head, Direction direction, out Node newHead)
        {
            newHead = null;
            var newPoint = GetPoint(head.Point, direction);

            if (!CanCreateHead(head.Point, direction, newPoint))
                return false;

            newHead = new Node(newPoint);
            newHead.Parent = head;
            newHead.ScoreX = head.ScoreX;
            newHead.ScoreY = head.ScoreY;
            newHead.StraightCount = head.StraightCount;
            UpdateScore(newHead, head.Point);

            isEnd |= newHead.Point.Equals(endpoint);

            return true;
        }

        private bool CanCreateHead(Point currentPoint, Direction direction, Point nextPoint)
        {
            if (!InRange(nextPoint))
                return false;

            if (direction == Direction.Diagonal)
            {
                var equal = option.EqualityComparer != null
                    ? option.EqualityComparer.Equals(seq1[nextPoint.X - 1], (seq2[nextPoint.Y - 1]))
                    : seq1[nextPoint.X - 1].Equals(seq2[nextPoint.Y - 1]);

                if (!equal)
                    return false;
            }

            return UpdateFarthestPoint(nextPoint);
        }

        private Point GetPoint(Point currentPoint, Direction direction)
        {
            switch (direction)
            {
                case Direction.Right:
                    return new Point(currentPoint.X + 1, currentPoint.Y);
                case Direction.Bottom:
                    return new Point(currentPoint.X, currentPoint.Y + 1);
                case Direction.Diagonal:
                    return new Point(currentPoint.X + 1, currentPoint.Y + 1);
            }

            throw new ArgumentException();
        }

        private bool InRange(Point point)
        {
            return point.X >= 0 && point.Y >= 0 && point.X <= endpoint.X && point.Y <= endpoint.Y;
        }

        private bool UpdateFarthestPoint(Point point)
        {
            var k = point.X - point.Y;

            var d = editDistances[k + offset];

            if (editDistance < d)
                return false;

            editDistances[k + offset] = editDistance;

            return true;
        }
    }
}

