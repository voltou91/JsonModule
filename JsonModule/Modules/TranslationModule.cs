using System;
using Newtonsoft.Json;
using System;
using static JsonModule.Program;

namespace JsonModule.Modules
{
    public class TranslationModule : DataModule
    {
        public Language language { get; private set; }

        public TranslationModule(string jsonContent, string pModule, Language pLanguage) : base(jsonContent, pModule, pLanguage) { }

        protected override void Init(string pJsonContent, Language pLanguage)
        {
            language = pLanguage;

            // On récupère toutes les langues en tant que dictionnaire
            Dictionary<string, object>? lDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(pJsonContent);

            // Si le dictionnaire a cette langue
            if (lDict != null && lDict.TryGetValue(language.ToString(), out object? lValue))
            {
                //On desérialise la langue en tant que Dictionnaire (important car les prochains dictionnaire seront du type "JObject") et on peut commencer la récursivité du FromObject
                FromObject(JsonConvert.DeserializeObject(lValue.ToString() ?? string.Empty));
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
