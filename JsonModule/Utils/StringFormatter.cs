using JsonModule.Modules;
using System.Text.RegularExpressions;

namespace JsonModule.Utils
{
    public static class StringFormatter
    {
        public static string Format(string pString, Dictionary<string, object> pReplacements)
        {
            // Use a regex to detect all {} in a string and return its contents in the FormatPlaceHolder method.
            return Regex.Replace(pString, @"\{([^{}]+)\}", (match) => FormatPlaceHolder(match, pReplacements));
        }

        private static string FormatPlaceHolder(Match match, Dictionary<string, object> pReplacements)
        {
            string lReplacement = match.Groups[1].Value;

            // If contains a potential ternary condition.
            if (lReplacement.Contains('?') && lReplacement.Contains(':'))
            {
                OperatorModule lOperator = Cache.GetOperator(lReplacement);
                return lOperator.Resolve(pReplacements);
            }
            pReplacements.TryGetValue(lReplacement, out object? pValue);
            return pValue?.ToString() ?? Program.TRANSLATION_NOT_FOUND;
        }
    }
}
