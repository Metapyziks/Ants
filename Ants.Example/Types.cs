using System;
using System.Collections.Generic;

namespace Ants.Example
{
    class Parameters
    {
        public TimeSpan LoadTime;
        public TimeSpan TurnTime;
        public int Rows;
        public int Cols;
        public int Turns;
        public int ViewRadius2;
        public int AttackRadius2;
        public int SpawnRadius2;
        public int PlayerSeed;
    }

    enum Cell
    {
        Unknown,
        Land,
        Water
    }

    enum Direction
    {
        None,
        N, E, S, W
    }

    class State
    {
        public readonly Parameters Parameters;

        public readonly Random Random;
        private readonly Cell[,] _cells;

        public readonly List<Ant> AliveAnts = new List<Ant>();
        public readonly List<Ant> DeadAnts = new List<Ant>();
        public readonly List<Hill> Hills = new List<Hill>();
        public readonly List<Food> Food = new List<Food>();

        public State(Parameters parameters)
        {
            Parameters = parameters;

            Random = new Random(parameters.PlayerSeed);
            _cells = new Cell[parameters.Rows, parameters.Cols];
        }

        public void WrapPosition(ref int row, ref int col)
        {
            row = (row % Parameters.Rows + Parameters.Rows) % Parameters.Rows;
            col = (col % Parameters.Cols + Parameters.Cols) % Parameters.Cols;
        }

        public Cell GetCell(int row, int col)
        {
            WrapPosition(ref row, ref col);
            return _cells[row, col];
        }

        public void SetCell(int row, int col, Cell value)
        {
            WrapPosition(ref row, ref col);
            _cells[row, col] = value;
        }

        public int GetDistance2(Entity a, Entity b)
        {
            return GetDistance2(a.Row, a.Col, b.Row, b.Col);
        }

        public int GetDistance2(Entity a, int row, int col)
        {
            return GetDistance2(a.Row, a.Col, row, col);
        }

        public int GetDistance2(int rowA, int colA, int rowB, int colB)
        {
            var rowDiff = Math.Abs(rowA - rowB);
            var colDiff = Math.Abs(colA - colB);

            var wrappedRowDiff = Math.Min(rowDiff, Parameters.Rows - rowDiff);
            var wrappedColDiff = Math.Min(colDiff, Parameters.Cols - colDiff);

            return wrappedRowDiff * wrappedRowDiff + wrappedColDiff * wrappedColDiff;
        }
    }

    class Entity
    {
        public int Row;
        public int Col;
    }

    class OwnedEntity : Entity
    {
        public int Owner;

        public bool IsOwnedByMe => Owner == 0;
        public bool IsEnemy => Owner > 0;
    }

    class Ant : OwnedEntity
    {
        public Direction Order;
    }

    class Hill : OwnedEntity
    {

    }

    class Food : Entity
    {

    }
}
