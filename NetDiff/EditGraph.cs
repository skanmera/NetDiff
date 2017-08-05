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

    internal class EditGraph<T>
    {
        private List<Node> heads;
        private Point endpoint;
        private Node endNode;
        private int delta;
        private IEqualityComparer<T> compare;
        private HashSet<Point> passedPoints;
        private IEnumerable<T> seq1;
        private IEnumerable<T> seq2;

        public EditGraph(IEnumerable<T> seq1, IEnumerable<T> seq2, IEqualityComparer<T> compare = null)
        {
            this.seq1 = seq1;
            this.seq2 = seq2;
            heads = new List<Node>();
            endpoint = new Point(seq1.Count(), seq2.Count());
            delta = 0;
            this.compare = compare;
            passedPoints = new HashSet<Point>();
        }

        public List<Point> Snake()
        {
            Initialize();

            while (Next()) { }

            var wayponit = new List<Point>();
            var current = endNode;
            while (true)
            {
                wayponit.Add(current.Point);

                if (current.Parent == null)
                    break;

                current = current.Parent;
            }

            wayponit.Reverse();

            return wayponit;
        }

        private void Initialize()
        {
            heads.Clear();
            delta = 0;

            var startHead = new Node(new Point(0, 0));
            while (true)
            {
                Node extendHead;
                if (TryExtendHeadDiagonal(startHead, out extendHead))
                    startHead = extendHead;
                else
                    break;
            }

            heads.Add(startHead);
            UpdateEndNode();
        }

        private bool Next()
        {
            if (IsEnd())
                return false;

            var enabledDiagonalLineNumbers = CalculateEnabledDiagonalLineNumbers(++delta);
            ExtendHeads(enabledDiagonalLineNumbers);
            UpdateEndNode();

            return true;
        }

        private void UpdateEndNode()
        {
            endNode = heads.FirstOrDefault(h => h.Point.Equals(endpoint));
        }

        private bool IsEnd()
        {
            return endNode != null;
        }

        private void ExtendHeads(IEnumerable<int> enabledDiagonalLineNumbers)
        {
            var updated = new List<Node>();
            foreach (var head in heads)
            {
                updated.Add(TryExtendHead(head, enabledDiagonalLineNumbers, Direction.Right));
                updated.Add(TryExtendHead(head, enabledDiagonalLineNumbers, Direction.Bottom));
            }

            heads = updated;
        }

        private Node TryExtendHead(Node head, IEnumerable<int> enabledDiagonalLineNumbers, Direction direction)
        {
            Node extendedHead;
            var isExtended = TryExtendHead(head, enabledDiagonalLineNumbers, direction, out extendedHead);
            if (isExtended)
            {
                Node diagonalHead;
                while (true)
                {
                    if (TryExtendHeadDiagonal(extendedHead, out diagonalHead))
                        extendedHead = diagonalHead;
                    else
                        break;
                }
            }

            return isExtended ? extendedHead : head;
        }

        private bool TryExtendHeadDiagonal(Node head, out Node extendedHead)
        {
            extendedHead = null;
            if (CanMoveDiagonal(head.Point))
            {
                var extendedPoint = new Point(head.Point.X + 1, head.Point.Y + 1);
                extendedHead = new Node(extendedPoint);
                extendedHead.Parent = head;

                return true;
            }

            return false;
        }

        private bool TryExtendHead(Node head, IEnumerable<int> enabledDiagonalLineNumbers, Direction direction, out Node extendedHead)
        {
            extendedHead = null;
            var movePoint = GetMovePoint(head.Point, direction);
            if (CanMove(movePoint))
            {
                if (enabledDiagonalLineNumbers.Any(n => IsOnDiagonalLine(movePoint, n)))
                {
                    extendedHead = new Node(movePoint);
                    extendedHead.Parent = head;

                    return true;
                }
            }

            return false;
        }

        private Point GetMovePoint(Point currentPoint, Direction direction)
        {
            switch (direction)
            {
                case Direction.Right: return new Point(currentPoint.X + 1, currentPoint.Y);
                case Direction.Bottom: return new Point(currentPoint.X, currentPoint.Y + 1);
                case Direction.Diagonal: return new Point(currentPoint.X + 1, currentPoint.Y + 1);
            }

            throw new ArgumentException();
        }

        private bool CanMove(Point movePoint)
        {
            return movePoint.X <= endpoint.X && movePoint.Y <= endpoint.Y && passedPoints.Add(movePoint);
        }

        private bool CanMoveDiagonal(Point currentPoint)
        {
            var diagonal = new Point(currentPoint.X + 1, currentPoint.Y + 1);

            if (diagonal.X > endpoint.X || diagonal.Y > endpoint.Y)
                return false;

            var equals = compare != null
                ? compare.Equals(seq1.ElementAt(currentPoint.X), (seq2.ElementAt(currentPoint.Y)))
                : seq1.ElementAt(currentPoint.X).Equals(seq2.ElementAt(currentPoint.Y));

            if (!equals)
                return false;

            return passedPoints.Add(diagonal);
        }

        private static IEnumerable<int> CalculateEnabledDiagonalLineNumbers(int delta)
        {
            for (int i = delta; i >= -delta; i -= 2)
                yield return i;
        }

        private static bool IsOnDiagonalLine(Point point, int diagonalLineNumber)
        {
            var minX = diagonalLineNumber >= 0 ? diagonalLineNumber : 0;
            var minY = diagonalLineNumber >= 0 ? 0 : diagonalLineNumber;

            return point.X >= minX && point.Y >= minY && Math.Abs(point.X - point.Y) == minX - minY;
        }
    }
}
