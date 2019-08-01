using System;
using System.IO;
using System.Text;

namespace Ants
{
    public class World
    {
        private readonly Cell[,] _cells;

        public string Author { get; set; }
        public int Teams { get; set; }

        public int Width { get; }
        public int Height { get; }

        public Cell this[int x, int y]
        {
            get
            {
                WrapCoords(ref x, ref y);
                return _cells[x, y];
            }
            set
            {
                WrapCoords(ref x, ref y);
                _cells[x, y] = value;
            }
        }

        public World(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                throw new ArgumentOutOfRangeException("Width and height must be > 0.");
            }

            Width = width;
            Height = height;

            _cells = new Cell[width, height];
        }

        public void WrapCoords(ref int x, ref int y)
        {
            x %= Width;
            y %= Height;

            x = x < 0 ? x + Width : x;
            y = y < 0 ? y + Height : y;
        }

        public void Clear()
        {
            Clear(Cell.Empty);
        }

        public void Clear(Cell value)
        {
            SetCells(0, 0, Width, Height, value);
        }

        public void SetCells(int x, int y, int w, int h, Cell value)
        {
            if (w <= 0 || h <= 0) return;

            for (var j = 0; j < h; ++j)
            {
                for (var i = 0; i < w; ++i)
                {
                    this[x + i, y + j] = value;
                }
            }
        }

        public void SetCells(int x, int y, int w, int h, Cell[] values)
        {
            if (w <= 0 || h <= 0) return;

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length < w * h)
            {
                throw new ArgumentException($"Expected at least {w * h} cell values.", nameof(values));
            }

            for (var j = 0; j < h; ++j)
            {
                for (var i = 0; i < w; ++i)
                {
                    this[x + i, y + j] = values[i + j * w];
                }
            }
        }

        public void SetCells(int x, int y, int w, int h, Cell[,] values)
        {
            if (w <= 0 || h <= 0) return;

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.GetLength(0) < w || values.GetLength(1) < h)
            {
                throw new ArgumentException($"Expected at least {w}x{h} cell values.", nameof(values));
            }

            for (var j = 0; j < h; ++j)
            {
                for (var i = 0; i < w; ++i)
                {
                    this[x + i, y + j] = values[i, j];
                }
            }
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

                writer.WriteLine($"width {Width}");
                writer.WriteLine($"height {Height}");

                writer.WriteLine();

                for (var y = 0; y < Height; ++y)
                {
                    writer.Write("row ");

                    for (var x = 0; x < Width; ++x)
                    {
                        var tile = this[x, y];
                        writer.Write(tile.ToString());
                    }

                    writer.WriteLine();
                }
            }
        }
    }
}
