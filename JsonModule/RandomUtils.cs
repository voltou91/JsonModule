using System;

namespace JsonModule
{
    public static class RandomUtils
    {
        // Comme ça on créé qu'une seule instance de Random, plutôt que de faire un new Random à chaque fois qu'on a besoin
        private static Random random = new Random();

        // Méthode d'extension, j'ai étendu à l'interface IEnumerable<T> la méthode "GetRandom", tout les types implémentant "IEnumerable<T>" auront cette méthode
        public static T? GetRandomElement<T>(this IEnumerable<T> pCollection)
        {
            int lCollectionCount = pCollection.Count();
            if (lCollectionCount > 0)
            {
                return pCollection.ElementAt(random.Next(lCollectionCount));
            }
            return default;
        }

        // Tire un booléen random en fonction d'un pourcentage, exemple Bool(0.45f) équivaut à 45% de chance d'avoir true
        public static bool Bool(float pPourcent) => random.NextSingle() < pPourcent;

        public static int RandInt(int pMin, int pMax = int.MaxValue) => random.Next(pMin, pMax);

        public static int RandInt(int pMax = int.MaxValue) => random.Next(pMax);
    }
}
