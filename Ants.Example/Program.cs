using System;
using System.Linq;

namespace Ants.Example
{
    class Program
    {
        static void Main()
        {
            var parameters = new Parameters();
            State state = null;

            while (TryReadLine(out var command, out var args))
            {
                switch (command)
                {
                    case "turn":
                        var turnNo = int.Parse(args[0]);

                        if (turnNo == 0)
                        {
                            UpdateParameters(parameters);
                            state = new State(parameters);
                            Bot.DoSetup(state);
                            Console.WriteLine("go");
                        }
                        else
                        {
                            UpdateState(state);
                            Bot.DoTurn(state, turnNo);

                            foreach (var ant in state.AliveAnts.Where(x => x.Owner == 0))
                            {
                                if (ant.Order == Direction.None) continue;
                                Console.WriteLine($"o {ant.Row} {ant.Col} {ant.Order}");
                            }

                            Console.WriteLine("go");
                        }
                        break;
                    case "end":
                        var scores = ReadScores(out int noPlayers);
                        Bot.DoEnd(noPlayers, scores);
                        return;
                }
            }
        }

        static bool TryReadLine(out string command, out string[] args)
        {
            command = null;
            args = null;

            while (true)
            {
                var str = Console.ReadLine();

                if (str == null)
                {
                    return false;
                }

                var split = str.Split(' ');

                if (split.Length == 0)
                {
                    continue;
                }

                command = split[0];
                args = split.Skip(1).ToArray();

                return true;
            }
        }

        static void UpdateParameters(Parameters parameters)
        {
            while (TryReadLine(out var command, out var args))
            {
                switch (command)
                {
                    case "loadtime":
                        parameters.LoadTime = TimeSpan.FromMilliseconds(double.Parse(args[0]));
                        break;
                    case "turntime":
                        parameters.TurnTime = TimeSpan.FromMilliseconds(double.Parse(args[0]));
                        break;
                    case "rows":
                        parameters.Rows = int.Parse(args[0]);
                        break;
                    case "cols":
                        parameters.Cols = int.Parse(args[0]);
                        break;
                    case "turns":
                        parameters.Turns = int.Parse(args[0]);
                        break;
                    case "viewradius2":
                        parameters.ViewRadius2 = int.Parse(args[0]);
                        break;
                    case "attackradius2":
                        parameters.AttackRadius2 = int.Parse(args[0]);
                        break;
                    case "spawnradius2":
                        parameters.SpawnRadius2 = int.Parse(args[0]);
                        break;
                    case "player_seed":
                        parameters.PlayerSeed = int.Parse(args[0]);
                        break;
                    case "ready":
                        return;
                }
            }
        }

        static void UpdateState(State state)
        {
            state.AliveAnts.Clear();
            state.DeadAnts.Clear();
            state.Food.Clear();
            state.Hills.Clear();

            while (TryReadLine(out var command, out var args))
            {
                if (command == "go") break;

                var row = int.Parse(args[0]);
                var col = int.Parse(args[1]);

                var team = args.Length > 2 ? int.Parse(args[2]) : 0;

                switch (command)
                {
                    case "w":
                        state.SetCell(row, col, Cell.Water);
                        break;
                    case "f":
                        state.Food.Add(new Food {Row = row, Col = col});
                        break;
                    case "h":
                        state.Hills.Add(new Hill {Row = row, Col = col, Owner = team});
                        break;
                    case "a":
                        state.AliveAnts.Add(new Ant {Row = row, Col = col, Owner = team});
                        break;
                    case "d":
                        state.DeadAnts.Add(new Ant {Row = row, Col = col, Owner = team});
                        break;
                }
            }

            foreach (var ant in state.AliveAnts)
            {
                if (!ant.IsOwnedByMe) continue;

                RevealAroundAnt(state, ant);
            }
        }

        static void RevealAroundAnt(State state, Ant ant)
        {
            var viewRadius = (int) Math.Ceiling(Math.Sqrt(state.Parameters.ViewRadius2));
            var minRow = ant.Row - viewRadius;
            var maxRow = ant.Row + viewRadius;
            var minCol = ant.Col - viewRadius;
            var maxCol = ant.Col + viewRadius;

            for (var row = minRow; row <= maxRow; ++row)
            {
                for (var col = minCol; col <= maxCol; ++col)
                {
                    if (state.GetCell(row, col) == Cell.Unknown)
                    {
                        state.SetCell(row, col, Cell.Land);
                    }
                }
            }
        }

        static int[] ReadScores(out int noPlayers)
        {
            noPlayers = 0;

            while (TryReadLine(out var command, out var args))
            {
                switch (command)
                {
                    case "players":
                        noPlayers = int.Parse(args[0]);
                        break;
                    case "score":
                        return args
                            .Select(int.Parse)
                            .ToArray();
                }
            }

            return null;
        }
    }
}
