using System;

namespace JsonModule.Utils
{
    public static class RandomUtils
    {
        // Create a single instance of Random instead of creating a new instance each time you call a function.
        private static Random random = new Random();

        // Extension method.
        // All types implementing IEnumerable<T> will have access to this method.
        public static T? GetRandomElement<T>(this IEnumerable<T> pCollection)
        {
            int lCollectionCount = pCollection.Count();
            if (lCollectionCount > 0)
            {
                return pCollection.ElementAt(random.Next(lCollectionCount));
            }
            return default;
        }

        // Draws a Boolean random as a function of a percentage, e.g. Bool(0.45f) equals 45% chance of getting true
        public static bool Bool(float pPourcent) => random.NextSingle() < pPourcent;

        public static int RandInt(int pMin, int pMax = int.MaxValue) => random.Next(pMin, pMax);

        public static int RandInt(int pMax = int.MaxValue) => random.Next(pMax);
    }
}
