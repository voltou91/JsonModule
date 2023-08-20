using System.Text.RegularExpressions;

namespace JsonModule.Utils
{
    public static class StringFormatter
    {
        public static string Format(string pString, Dictionary<string, object> pReplacements)
        {
            // Use a regex to detect all {} in a string
            return Regex.Replace(pString, @"\{([^{}]+)\}", (match) =>
            {
                string lReplacement = match.Groups[1].Value;

                // If contains a potential ternary condition.
                if (lReplacement.Contains('?') && lReplacement.Contains(':'))
                {
                    return Cache.GetOperator(lReplacement).Resolve(pReplacements);
                }

                pReplacements.TryGetValue(lReplacement, out object? pValue);
                return pValue?.ToString() ?? Program.TRANSLATION_NOT_FOUND;
            });
        }
    }
}
