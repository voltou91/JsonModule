using static JsonModule.Program;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JsonModule.Modules
{
    public class TranslationModule : DataModule
    {
        public Language language { get; private set; }

        public TranslationModule(string jsonContent, string pModule, Language pLanguage) : base(jsonContent, pModule, pLanguage) { }

        protected override void Init(string pJsonContent, Language pLanguage)
        {
            language = pLanguage;

            JSchema lSchema = JSchema.Parse(pJsonContent);

            // Si le dictionnaire a cette langue
            if (lSchema.ExtensionData.TryGetValue(language.ToString(), out JToken? lValue))
            {
                //On desérialise la langue en tant que Dictionnaire (important car les prochains dictionnaire seront du type "JObject") et on peut commencer la récursivité du FromObject
                MapJson(lValue);
            }
            else
            {
                if (testMode) Console.WriteLine($"[{module}] not implement the language {language}");
            }
        }

        //Petit log avant de retourne un message d'erreur
        protected override string InvalidTranslation(string pKey)
        {
            if (testMode) Console.WriteLine($"[{module}]: invalid translation {pKey} in {language}");
            return base.InvalidTranslation(pKey);
        }
    }
}
