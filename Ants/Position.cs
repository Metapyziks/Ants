using System;

namespace Ants
{
    public struct Position : IEquatable<Position>, IComparable<Position>
    {
        public static Position Zero => new Position(0, 0);

        public static implicit operator Vector(Position pos)
        {
            return new Vector(pos.Row, pos.Col);
        }

        public static explicit operator Position(Vector vec)
        {
            return new Position(vec.Rows, vec.Cols);
        }

        public readonly int Row;
        public readonly int Col;

        public Position(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public int CompareTo(Position other)
        {
            var rowComparison = Row.CompareTo(other.Row);
            return rowComparison != 0 ? rowComparison : Col.CompareTo(other.Col);
        }

        public bool Equals(Position other)
        {
            return Row == other.Row && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            return obj is Position other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Col;
            }
        }
    }
}
