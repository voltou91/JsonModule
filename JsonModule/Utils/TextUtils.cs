using System;

namespace JsonModule.Utils
{
    public static class TextUtils
    {
        /// <summary>
        /// Searches for the n-th occurrence of a string and returns its location
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pStringSearched"></param>
        /// <param name="pNb"></param>
        /// <param name="PIndex"></param>
        /// <returns></returns>
        public static int GetNOccurrence(this string pString, string pStringSearched, int pNb, int PIndex = 0)
        {
            int lIndex = pString.IndexOf(pStringSearched, PIndex);

            if (pNb <= 1) return lIndex;
            return pString.GetNOccurrence(pStringSearched, pNb - 1, lIndex + 1);
        }

        /// <summary>
        /// Searches for the n-th occurrence of a character and returns its location
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pCharSearched"></param>
        /// <param name="pNb"></param>
        /// <param name="PIndex"></param>
        /// <returns></returns>
        public static int GetNOccurrence(this string pString, char pCharSearched, int pNb, int PIndex = 0)
        {
            int lIndex = pString.IndexOf(pCharSearched, PIndex);

            if (pNb <= 1) return lIndex;
            return pString.GetNOccurrence(pCharSearched, pNb - 1, lIndex + 1);
        }
    }
}
