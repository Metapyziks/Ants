using System;
using System.Collections.Generic;
using System.Linq;

namespace Ants
{
    public static class Helper
    {
        public static IEnumerable<Tuple<int, int>> GetFactorPairs(int value)
        {
            for (var i = 1; i <= value; ++i)
            {
                var j = value / i;
                if (i * j == value) yield return new Tuple<int, int>(i, j);
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable, Random random)
        {
            var arr = enumerable.ToArray();

            for (var i = 0; i < arr.Length; ++i)
            {
                var j = random.Next(i, arr.Length);
                var temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }

            return arr;
        }
    }
}
