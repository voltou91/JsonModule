using System;
using JsonModule.Modules;
using System.Text.RegularExpressions;

namespace JsonModule
{
    public static class StringFormatter
    {
        public static string Format(string pString, Dictionary<string, object> pReplacements)
        {
            return Regex.Replace(pString, @"\{([^{}]+)\}", (match) => FormatPlaceHolder(match, pString, pReplacements));
        }

        private static string FormatPlaceHolder(Match match, string pInitialString, Dictionary<string, object> pReplacements)
        {
            string lReplacement = match.Groups[1].Value;
            if (lReplacement.Contains('?') && lReplacement.Contains(':'))
            {
                OperatorModule lOperator = Cache.GetOperator(pInitialString, lReplacement);
                return lOperator.Resolve(pReplacements);
            }
            pReplacements.TryGetValue(lReplacement, out object? pValue);
            return pValue?.ToString() ?? Program.TRANSLATION_ERROR;
        }
    }
}
