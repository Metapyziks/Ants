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
        Hill,
        DeadAnt,
        AntOnHill
    }

    public struct Cell
    {
        public const int MaxTeams = 10;

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
            if (team < 0 || team >= MaxTeams)
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
                    return ".";
                case CellType.Water:
                    return "%";
                case CellType.Food:
                    return "*";
                case CellType.Ant:
                    return ((char) ('a' + Team)).ToString();
                case CellType.Hill:
                    return Team.ToString();
                case CellType.AntOnHill:
                    return ((char) ('A' + Team)).ToString();
                case CellType.DeadAnt:
                    return "!";
                default:
                    return "?";
            }
        }
    }
}
