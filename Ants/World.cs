using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ants
{
    public class World
    {
        private readonly Cell[,] _cells;

        public string Author { get; set; }
        public int Teams { get; }

        public int Rows { get; }
        public int Cols { get; }

        public Vector Size => new Vector(Rows, Cols);

        private int _nextEntityId = 1;
        private readonly SortedSet<Entity> _entities = new SortedSet<Entity>();

        public Cell this[Position pos]
        {
            get
            {
                pos = WrapPosition(pos);
                return _cells[pos.Row, pos.Col];
            }
            set
            {
                pos = WrapPosition(pos);
                _cells[pos.Row, pos.Col] = value;
            }
        }

        public World(int rows, int cols, int teams)
        {
            if (rows < 1 || cols < 1)
            {
                throw new ArgumentOutOfRangeException("Rows and columns must be > 0.");
            }

            Rows = rows;
            Cols = cols;
            Teams = teams;

            _cells = new Cell[rows, cols];
        }

        public Position Position(int row, int col)
        {
            return WrapPosition(new Position(row, col));
        }

        public Position Position(Vector vector)
        {
            return Position(vector.Rows, vector.Cols);
        }

        public Vector Subtract(Position a, Position b)
        {
            var offset = new Vector(Rows / 2, Cols / 2);
            return Position((Vector) a - b + offset) - offset;
        }

        public Position Translate(Position position, Vector vector)
        {
            return Position(position + vector);
        }

        private Entity CreateEntity(Position position, EntityType type, int team = 0)
        {
            var entity = new Entity(_nextEntityId++, WrapPosition(position), type, team);
            _entities.Add(entity);
            return entity;
        }

        public Entity CreateAnt(Position position, int team)
        {
            return CreateEntity(position, EntityType.Ant, team);
        }

        public Entity CreateHill(Position position, int team)
        {
            return CreateEntity(position, EntityType.Hill, team);
        }

        public bool TranslateEntity(Entity entity, Vector vector)
        {
            return SetEntityPosition(entity, Translate(entity.Position, vector));
        }

        public bool SetEntityPosition(Entity entity, Position position)
        {
            position = WrapPosition(position);
            return _entities.Remove(entity) && _entities.Add(new Entity(entity.Id, position, entity.Type, entity.Team));
        }

        public int GetEntities(Position position, List<Entity> result)
        {
            var count = 0;

            foreach (var entity in _entities)
            {
                var positionComparison = position.CompareTo(entity.Position);
                if (positionComparison > 0) continue;
                if (positionComparison < 0) break;
                result.Add(entity);
                ++count;
            }

            return count;
        }

        public Position WrapPosition(Position pos)
        {
            var row = pos.Row % Rows;
            var col = pos.Col % Cols;

            return new Position(
                row < 0 ? row + Rows : row,
                col < 0 ? col + Cols : col);
        }

        public void Clear()
        {
            Clear(Cell.Land);
        }

        public void Clear(Cell value)
        {
            SetCells(Ants.Position.Zero, Size, value);
        }

        public void SetCells(Position start, Vector size, Cell value)
        {
            if (size.Rows <= 0 || size.Cols <= 0) return;

            for (var r = 0; r < size.Rows; ++r)
            {
                for (var c = 0; c < size.Cols; ++c)
                {
                    this[Translate(start, new Vector(r, c))] = value;
                }
            }
        }

        public void SetCells(Position start, Vector size, Cell[,] values)
        {
            if (size.Rows <= 0 || size.Cols <= 0) return;

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.GetLength(0) < size.Rows || values.GetLength(1) < size.Cols)
            {
                throw new ArgumentException($"Expected at least {size.Rows}x{size.Cols} cell values.", nameof(values));
            }

            for (var r = 0; r < size.Rows; ++r)
            {
                for (var c = 0; c < size.Cols; ++c)
                {
                    this[Position(start.Row + r, start.Col + c)] = values[r, c];
                }
            }
        }

        [ThreadStatic]
        private static List<Entity> _sTempEntities;

        private char GetCellChar(Position position)
        {
            var cell = this[position];

            switch (cell)
            {
                case Cell.Land:
                    break;
                case Cell.Water:
                    return '%';
                case Cell.Unknown:
                    return '?';
            }

            if (_sTempEntities == null) _sTempEntities = new List<Entity>();
            else _sTempEntities.Clear();

            GetEntities(position, _sTempEntities);

            var hill = _sTempEntities.FirstOrDefault(x => x.Type == EntityType.Hill);
            var ant = _sTempEntities.FirstOrDefault(x => x.Type == EntityType.Ant);

            if (ant.IsValid)
            {
                return hill.IsValid && hill.Team == ant.Team
                    ? (char)('A' + ant.Team)
                    : (char)('a' + ant.Team);
            }

            if (hill.IsValid)
            {
                return (char)('0' + hill.Team);
            }

            if (_sTempEntities.Any(x => x.Type == EntityType.Food))
            {
                return '*';
            }

            if (_sTempEntities.Any(x => x.Type == EntityType.DeadAnt))
            {
                return '!';
            }

            return '.';
        }

        public void Write(Stream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.ASCII, 128,  true))
            {
                if (!string.IsNullOrEmpty(Author))
                {
                    writer.WriteLine($"author {Author}");
                }

                if (Teams != 0)
                {
                    writer.WriteLine($"teams {Teams}");
                }

                writer.WriteLine($"rows {Rows}");
                writer.WriteLine($"cols {Cols}");

                writer.WriteLine();

                for (var r = 0; r < Rows; ++r)
                {
                    writer.Write("m ");

                    for (var c = 0; c < Cols; ++c)
                    {
                        writer.Write(GetCellChar(Position(r, c)));
                    }

                    writer.WriteLine();
                }
            }
        }
    }
}
