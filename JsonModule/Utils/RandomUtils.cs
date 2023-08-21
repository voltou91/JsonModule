namespace JsonModule.Utils
{
    public static class RandomUtils
    {
        // Create a single instance of Random instead of creating a new instance each time you call a function.
        private static Random random = new Random();

        /// <summary>
        /// Get a random element from an IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pCollection"></param>
        /// <returns></returns>
        public static T GetRandomElement<T>(this IEnumerable<T> pCollection)
        {
            int lCollectionCount = pCollection.Count();
            return lCollectionCount > 0 ? pCollection.ElementAt(random.Next(lCollectionCount)) : default;
        }

        /// <summary>
        ///  Draws a Boolean random as a function of a percentage, for example Bool(0.45f) equals 45% chance of getting true
        /// </summary>
        /// <param name="pPourcent"></param>
        /// <example>
        /// Bool(0.45f) equals 45% chance of getting true
        /// </example>
        /// <returns></returns>
        public static bool Bool(float pPourcent = 0.5f) => random.NextSingle() < pPourcent;

        public static int RandInt(int pMin, int pMax = int.MaxValue) => random.Next(pMin, pMax);

        public static int RandInt(int pMax = int.MaxValue) => random.Next(pMax);
    }
}
