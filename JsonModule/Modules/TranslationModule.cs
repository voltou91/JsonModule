using System;
using Newtonsoft.Json;
using static JsonModule.Program;
using Newtonsoft.Json.Linq;

namespace JsonModule.Modules
{
    public class TranslationModule : DataModule
    {
        public Language language { get; private set; }

        public TranslationModule(string jsonContent, string pModule, Language pLanguage) : base(jsonContent, pModule, pLanguage) { }

        protected override void Init(string pJsonContent, Language pLanguage)
        {
            language = pLanguage;

            JObject lDict = JsonConvert.DeserializeObject(pJsonContent) as JObject ?? new JObject();

            // Si le dictionnaire a cette langue
            if (lDict.TryGetValue(language.ToString(), out JToken? lValue))
            {
                //On desérialise la langue en tant que Dictionnaire (important car les prochains dictionnaire seront du type "JObject") et on peut commencer la récursivité du FromObject
                FromObject(lValue);
            }
            else
            {
                if (testMode) Console.WriteLine($"{module} not implement the language {language}");
            }
        }

        //Petit log avant de retourne un message d'erreur
        protected override string InvalidTranslation(string pKey)
        {
            if (testMode) Console.WriteLine($"{module}: invalid translation {pKey} in {language}");
            return base.InvalidTranslation(pKey);
        }
    }
}
