using System;

namespace Ants
{
    public enum CellType
    {
        Unknown = -1,

        Empty = 0,
        Water,
        Food,
        Ant,
        Hill
    }

    public struct Cell
    {
        public const int MaxTeams = 8;

        public static Cell Unknown { get; } = new Cell(CellType.Unknown);
        public static Cell Empty { get; } = new Cell(CellType.Empty);
        public static Cell Water { get; } = new Cell(CellType.Water);
        public static Cell Food { get; } = new Cell(CellType.Food);

        public static Cell Ant(int team)
        {
            return new Cell(CellType.Ant, team);
        }

        public static Cell Hill(int team)
        {
            return new Cell(CellType.Hill, team);
        }

        public readonly CellType Type;
        public readonly int Team;

        public Cell(CellType type, int team = 0)
        {
            if (team < 0 || team > MaxTeams)
            {
                throw new ArgumentOutOfRangeException($"Team index must be between 0 and {MaxTeams}.", nameof(team));
            }

            Type = type;
            Team = team;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case CellType.Empty:
                    return ". ";
                case CellType.Water:
                    return "W ";
                case CellType.Food:
                    return "F ";
                case CellType.Ant:
                    return $"a{Team}";
                case CellType.Hill:
                    return $"H{Team}";
                default:
                    return "? ";
            }
        }
    }
}
