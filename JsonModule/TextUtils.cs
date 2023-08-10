using System;

namespace JsonModule
{
    public static class TextUtils
    {
        // Recherche la n-ème occurence d'un caractère, comme par exemple le 2e "?" dans la chaîne "fdez?depoz?fdepovfn?", et renvoie son emplacement 
        public static int GetNOccurrence(this string pString, string pStringSearched, int pNb, int PIndex = 0)
        {
            int lIndex = pString.IndexOf(pStringSearched, PIndex);

            if (pNb <= 1) return lIndex;
            return GetNOccurrence(pString, pStringSearched, pNb - 1, lIndex + 1);
        }

        public static int GetNOccurrence(this string pString, char pCharSearched, int pNb, int PIndex = 0)
        {
            int lIndex = pString.IndexOf(pCharSearched, PIndex);

            if (pNb <= 1) return lIndex;
            return GetNOccurrence(pString, pCharSearched, pNb - 1, lIndex + 1);
        }
    }
}
