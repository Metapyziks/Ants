using System.Linq;

namespace Ants.Example
{
    static class Bot
    {
        public static void DoSetup(State state)
        {

        }

        public static void DoTurn(State state, int turnNo)
        {
            foreach (var ant in state.AliveAnts)
            {
                if (!ant.IsOwnedByMe) continue;

                // Random walk
                var dir = (Direction) state.Random.Next(1, 5);
                ant.Order = dir;
            }
        }

        public static void DoEnd(int noPlayers, int[] scores)
        {

        }
    }
}
