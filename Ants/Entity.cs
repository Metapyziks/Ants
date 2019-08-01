using System;

namespace Ants
{
    public enum EntityType
    {
        None,
        Ant,
        DeadAnt,
        Hill,
        Food
    }

    public struct Entity : IEquatable<Entity>, IComparable<Entity>
    {
        public static Entity None => default(Entity);

        public readonly int Id;
        public readonly EntityType Type;
        public readonly Position Position;
        public readonly int Team;

        public bool IsNone => Type == EntityType.None;
        public bool IsValid => Id > 0;

        public Entity(int id, Position position, EntityType type, int team = 0)
        {
            Id = id;
            Type = type;
            Position = position;
            Team = team;
        }

        public bool Equals(Entity other)
        {
            return Id == other.Id && Type == other.Type && Position.Equals(other.Position) && Team == other.Team;
        }

        public override bool Equals(object obj)
        {
            return obj is Entity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case EntityType.Ant:
                    return $"Ant (Team: {Team})";
                case EntityType.DeadAnt:
                    return $"Dead Ant (Team: {Team})";
                case EntityType.Food:
                    return $"Food";
                case EntityType.Hill:
                    return $"Hill (Team: {Team})";
                case EntityType.None:
                    return $"None";
                default:
                    return "?";
            }
        }

        public int CompareTo(Entity other)
        {
            var positionComparison = Position.CompareTo(other.Position);
            if (positionComparison != 0) return positionComparison;
            var typeComparison = Type.CompareTo(other.Type);
            if (typeComparison != 0) return typeComparison;
            var teamComparison = Team.CompareTo(other.Team);
            if (teamComparison != 0) return teamComparison;
            return Id.CompareTo(other.Id);
        }
    }
}
