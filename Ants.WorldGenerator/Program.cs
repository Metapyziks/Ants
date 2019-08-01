using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace Ants.WorldGenerator
{
    class Program
    {
        class Options
        {
            [Option('s', "seed", Required = false, HelpText = "Seed to use when generating a world.")]
            public int? Seed { get; set; }

            [Option('t', "teams", Required = false, Default = 2, HelpText = "Number of teams this world should support.")]
            public int Teams { get; set; }

            [Option('w', "width", Required = false, HelpText = "Width of the world to generate.")]
            public int? Width
            {
                get => MinWidth == MaxWidth ? (int?) MinWidth : null;
                set
                {
                    if (value.HasValue) MinWidth = MaxWidth = value.Value;
                }
            }

            [Option("min-width", Required = false, Default = 75, HelpText = "Minimum width of the world to generate.")]
            public int MinWidth { get; set; }

            [Option("max-width", Required = false, Default = 100, HelpText = "Maximum width of the world to generate.")]
            public int MaxWidth { get; set; }

            [Option('h', "height", Required = false, HelpText = "Height of the world to generate.")]
            public int? Height
            {
                get => MinHeight == MaxHeight ? (int?) MinHeight : null;
                set
                {
                    if (value.HasValue) MinHeight = MaxHeight = value.Value;
                }
            }

            [Option("min-height", Required = false, Default = 75, HelpText = "Minimum height of the world to generate.")]
            public int MinHeight { get; set; }

            [Option("max-height", Required = false, Default = 100, HelpText = "Maximum height of the world to generate.")]
            public int MaxHeight { get; set; }

            [Option("min-hills", Required = false, Default = 2, HelpText = "Minimum number of anthills per team.")]
            public int MinHills { get; set; }

            [Option("max-hills", Required = false, Default = 2, HelpText = "Maximum number of anthills per team.")]
            public int MaxHills { get; set; }
        }

        class WorldGeneratorException : Exception
        {
            public WorldGeneratorException(string message)
                : base(message) { }
        }

        public static string[] CommandLineArgs { get; private set; }

        static int Main(string[] args)
        {
            CommandLineArgs = args;

            try
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(GenerateWorld);
            }
            catch (WorldGeneratorException e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }

            return 0;
        }

        enum Symmetry
        {
            Tiled
        }

        static void GenerateWorld(Options options)
        {
            var seed = options.Seed ?? new Random().Next(int.MinValue, int.MaxValue);
            var random = new Random(seed);
            var symmetries = (Symmetry[]) Enum.GetValues(typeof(Symmetry));
            var symmetry = symmetries[random.Next(symmetries.Length)];

            World world = null;

            switch (symmetry)
            {
                case Symmetry.Tiled:
                    world = GenerateTiledWorld(options, random);
                    break;
                default:
                    throw new WorldGeneratorException("Unable to select a valid symmetry type.");
            }

            var asmName = typeof(Program).Assembly.GetName();
            var args = options.Seed.HasValue
                ? CommandLineArgs
                : new[] {"--seed", seed.ToString()}.Concat(CommandLineArgs);

            world.Author = $"{asmName.Name}, Version {asmName.Version}, Arguments [ {string.Join(", ", args.Select(x => $"\"{x}\""))} ]";

            using (var stream = Console.OpenStandardOutput())
            {
                world.Write(stream);
            }
        }

        static bool GetTilingSizeRange(int minWorldSize, int maxWorldSize, int tiles, out int minTileSize, out int maxTileSize)
        {
            minTileSize = (minWorldSize + tiles - 1) / tiles;
            maxTileSize = maxWorldSize / tiles;

            return maxTileSize >= minTileSize;
        }

        struct Node
        {
            public readonly double X;
            public readonly double Y;
            public readonly double Radius;

            public Node(double x, double y, double radius)
            {
                X = x;
                Y = y;
                Radius = radius;
            }

            public double Dist2(double x, double y)
            {
                return (X - x) * (X - x) + (Y - y) * (Y - y);
            }

            public double Dist2(Node node)
            {
                return Dist2(node.X, node.Y);
            }
        }

        static void CarveNode(Cell[,] cells, Node node, Vector stagger)
        {
            var w = cells.GetLength(0);
            var h = cells.GetLength(1);

            var minX = (int) Math.Floor(node.X - node.Radius);
            var maxX = (int) Math.Floor(node.X + node.Radius + 1);
            var minY = (int) Math.Floor(node.Y - node.Radius);
            var maxY = (int) Math.Floor(node.Y + node.Radius + 1);

            for (var y = minY; y < maxY; ++y)
            for (var x = minX; x < maxX; ++x)
            {
                var dist2 = node.Dist2(x, y);

                if (dist2 <= node.Radius * node.Radius)
                {
                    var xn = x;
                    var yn = y;

                    if (xn < 0)
                    {
                        xn += w;
                        yn += stagger.Cols;
                    }
                    else if (xn >= w)
                    {
                        xn -= w;
                        yn -= stagger.Cols;
                    }

                    if (yn < 0)
                    {
                        yn += h;
                        xn += stagger.Rows;
                    }
                    else if (yn >= h)
                    {
                        yn -= h;
                        xn -= stagger.Rows;
                    }

                    cells[(xn % w + w) % w, (yn % h + h) % h] = Cell.Land;
                }
            }
        }

        static void CarveNodes(Cell[,] cells, List<Node> nodes, Vector stagger, Random random)
        {
            foreach (var node in nodes)
            {
                if (node.Radius < 2)
                {
                    CarveNode(cells, node, stagger);
                }
                else
                {
                    for (var i = 0; i < node.Radius; ++i)
                    {
                        var omega = random.NextDouble() * Math.PI * 2d;
                        var r = (random.NextDouble() + random.NextDouble()) * node.Radius * 0.5;

                        var x = node.X + (int) Math.Round(Math.Cos(omega) * r);
                        var y = node.Y + (int) Math.Round(Math.Sin(omega) * r);

                        CarveNode(cells, new Node(x, y, (node.Radius + 1) / 2), stagger);
                    }
                }
            }
        }

        static World GenerateTiledWorld(Options options, Random random)
        {
            var factors = Helper.GetFactorPairs(options.Teams)
                .Where(pair => GetTilingSizeRange(options.MinWidth, options.MaxWidth, pair.Item1, out _, out _)
                    && GetTilingSizeRange(options.MinHeight, options.MaxHeight, pair.Item2, out _, out _))
                .ToArray();

            if (factors.Length == 0)
            {
                throw new WorldGeneratorException("Unable to fit a valid tiling inside the given min/max world size.");
            }

            var tiling = factors[random.Next(factors.Length)];

            GetTilingSizeRange(options.MinWidth, options.MaxWidth, tiling.Item1,
                out var minTileWidth, out var maxTileWidth);
            GetTilingSizeRange(options.MinHeight, options.MaxHeight, tiling.Item2,
                out var minTileHeight, out var maxTileHeight);

            var tileSize = new Vector(
                random.Next(minTileWidth, maxTileWidth + 1), 
                random.Next(minTileHeight, maxTileHeight + 1));

            var world = new World(tileSize.Rows * tiling.Item1, tileSize.Cols * tiling.Item2, options.Teams);

            var tile = new Cell[tileSize.Rows, tileSize.Cols];

            for (var r = 0; r < tileSize.Rows; ++r)
            for (var c = 0; c < tileSize.Cols; ++c)
            {
                tile[r, c] = Cell.Water;
            }

            Vector stagger;

            if (random.Next(0, 2) == 0)
            {
                stagger = new Vector(random.Next(0, tileSize.Rows), 0);
            }
            else
            {
                stagger = new Vector(0, random.Next(0, tileSize.Cols));
            }

            var nodes = new List<Node>();

            var minRadius = 2d;
            var maxRadius = Math.Max(Math.Min(tileSize.Rows, tileSize.Cols) / 4d, 2d) + 1d;

            var attempts = random.Next(128, 160);

            const double thinWeight = 1d;

            for (var i = 0; i < attempts; ++i)
            {
                var radius = minRadius + (Math.Pow(random.NextDouble(), thinWeight) * maxRadius - minRadius);
                var r = random.NextDouble() * tileSize.Rows;
                var c = random.NextDouble() * tileSize.Cols;

                var valid = true;

                foreach (var node in nodes)
                {
                    var dist2 = node.Dist2(r, c);
                    var minDist2 = (node.Radius + radius) * (node.Radius + radius);

                    if (dist2 <= minDist2)
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid) continue;

                nodes.Add(new Node(r, c, radius));

                nodes.Add(r < tileSize.Rows / 2d
                    ? new Node(r + tileSize.Rows, c + stagger.Cols, radius)
                    : new Node(r - tileSize.Rows, c - stagger.Cols, radius));

                nodes.Add(c < tileSize.Cols / 2d
                    ? new Node(r + stagger.Rows, c + tileSize.Cols, radius)
                    : new Node(r - stagger.Rows, c - tileSize.Cols, radius));
            }

            var connected = new List<Node> {nodes[0]};
            nodes.RemoveAt(0);

            var connections = new List<Node>();

            while (nodes.Count > 0)
            {
                var bestDist2 = double.MaxValue;
                var bestAIndex = -1;
                var bestBIndex = -1;

                for (var aIndex = connected.Count - 1; aIndex >= 0; --aIndex)
                {
                    var a = connected[aIndex];

                    for (var bIndex = nodes.Count - 1; bIndex >= 0; --bIndex)
                    {
                        var b = nodes[bIndex];
                        var dist2 = a.Dist2(b);

                        if (dist2 >= bestDist2) continue;

                        bestDist2 = dist2;
                        bestAIndex = aIndex;
                        bestBIndex = bIndex;
                    }
                }

                var bestA = connected[bestAIndex];
                var bestB = nodes[bestBIndex];
                nodes.RemoveAt(bestBIndex);
                connected.Add(bestB);

                var dist = Math.Sqrt(bestA.Dist2(bestB));
                var inc = 0.5 / dist;
                for (var t = inc * 0.5; t < 1; t += inc)
                {
                    var x = bestA.X + (bestB.X - bestA.X) * t;
                    var y = bestA.Y + (bestB.Y - bestA.Y) * t;

                    x += random.NextDouble() * 0.5 - 0.25;
                    y += random.NextDouble() * 0.5 - 0.25;

                    connections.Add(new Node((int) Math.Round(x), (int) Math.Round(y), 2));
                }
            }

            nodes.AddRange(connected);
            nodes.AddRange(connections);

            nodes.RemoveAll(node => node.X < 0 || node.X >= tileSize.Rows || node.Y < 0 || node.Y >= tileSize.Cols);

            CarveNodes(tile, nodes, stagger, random);

            var hillsPerTeam = random.Next(options.MinHills, options.MaxHills + 1);

            var hillPositions = Enumerable.Range(0, hillsPerTeam)
                .Select(i => {
                    var hillNode = nodes.OrderByDescending(x => x.Radius).Skip(i).First();

                    int hillX, hillY;

                    while (true)
                    {
                        var hillOffsetAng = random.NextDouble() * Math.PI * 2d;
                        var hillOffsetRad = hillNode.Radius * 0.25f + random.NextDouble() * hillNode.Radius * 0.25;

                        hillX = (int)Math.Round(hillNode.X + Math.Cos(hillOffsetAng) * hillOffsetRad);
                        hillY = (int)Math.Round(hillNode.Y + Math.Cos(hillOffsetAng) * hillOffsetRad);

                        if (hillX < 1 || hillY < 1 || hillX >= tileSize.Rows - 1 || hillY >= tileSize.Cols - 1)
                            continue;

                        var isValid = true;

                        for (var x = hillX - 1; x <= hillX + 1; ++x)
                        {
                            for (var y = hillY - 1; y <= hillY + 1; ++y)
                            {
                                if (tile[x, y] != Cell.Land)
                                {
                                    isValid = false;
                                }
                            }
                        }

                        if (isValid) break;
                    }

                    return new Vector(hillX, hillY);
                })
                .ToArray();

            var hillOrders = Enumerable.Range(0, options.Teams)
                .Shuffle(random)
                .ToArray();

            for (var y = 0; y < tiling.Item2; ++y)
            for (var x = 0; x < tiling.Item1; ++x)
            {
                var offset = world.Position(x * tileSize.Rows + stagger.Rows * y, y * tileSize.Cols + stagger.Cols * x);
                var index = x + y * tiling.Item1;

                world.SetCells(offset, tileSize, tile);

                for (var h = 0; h < hillsPerTeam; ++h)
                {
                    var team = hillOrders[(h + index) % options.Teams];
                    var hill = hillPositions[h];

                    world.CreateHill(world.Translate(offset, hill), team);
                }
            }

            return world;
        }
    }
}
