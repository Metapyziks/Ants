using System;

namespace Ants
{
    public struct Vector : IEquatable<Vector>
    {
        public static Vector Zero => new Vector(0, 0);

        public readonly int Rows;
        public readonly int Cols;

        public int LengthSquared => Rows * Rows + Cols * Cols;

        public Vector(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
        }

        public int Dot(Vector other)
        {
            return Rows * other.Rows + Cols * other.Cols;
        }

        public int Cross(Vector other)
        {
            return Rows * other.Cols - Cols * other.Rows;
        }

        public static bool operator ==(Vector a, Vector b)
        {
            return a.Rows == b.Rows && a.Cols == b.Cols;
        }

        public static bool operator !=(Vector a, Vector b)
        {
            return a.Rows != b.Rows || a.Cols != b.Cols;
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.Rows + b.Rows, a.Cols + b.Cols);
        }

        public static Vector operator +(Vector vec, int scalar)
        {
            return new Vector(vec.Rows + scalar, vec.Cols + scalar);
        }

        public static Vector operator +(int scalar, Vector vec)
        {
            return new Vector(vec.Rows + scalar, vec.Cols + scalar);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.Rows - b.Rows, a.Cols - b.Cols);
        }

        public static Vector operator -(Vector vec, int scalar)
        {
            return new Vector(vec.Rows - scalar, vec.Cols - scalar);
        }

        public static Vector operator -(int scalar, Vector vec)
        {
            return new Vector(scalar - vec.Rows, scalar - vec.Cols);
        }

        public static Vector operator *(Vector a, Vector b)
        {
            return new Vector(a.Rows * b.Rows, a.Cols * b.Cols);
        }

        public static Vector operator *(Vector vec, int scalar)
        {
            return new Vector(vec.Rows * scalar, vec.Cols * scalar);
        }

        public static Vector operator *(int scalar, Vector vec)
        {
            return new Vector(vec.Rows * scalar, vec.Cols * scalar);
        }

        public static Vector operator /(Vector a, Vector b)
        {
            return new Vector(a.Rows / b.Rows, a.Cols / b.Cols);
        }

        public static Vector operator /(Vector vec, int scalar)
        {
            return new Vector(vec.Rows / scalar, vec.Cols / scalar);
        }

        public bool Equals(Vector other)
        {
            return Rows == other.Rows && Cols == other.Cols;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Rows * 397) ^ Cols;
            }
        }
    }
}
