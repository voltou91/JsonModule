using static JsonModule.Program;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JsonModule.Modules
{
    public class TranslationModule : DataModule
    {
        public Language language { get; private set; }

        public TranslationModule(string pJsonContent, string pModule, Language pLanguage) : base(pJsonContent, pModule, pLanguage) { }

        protected override void Init(string pJsonContent, Language pLanguage)
        {
            language = pLanguage;

            if (JSchema.Parse(pJsonContent).ExtensionData.TryGetValue(language.ToString(), out JToken? lValue))
            {
                MapJson(lValue);
            }
            else if (TEST_MODE)
            {
                Console.WriteLine(PROGRAM_TRANSLATIONS.Format("Errors.NotImplementLanguage", new Dictionary<string, object>() { { "module", module }, { "language", language } }));
            }
        }

        protected override string InvalidTranslation(string pKey)
        {
            if (TEST_MODE) Console.WriteLine(PROGRAM_TRANSLATIONS.Format("Errors.InvalidTranslation", new Dictionary<string, object>() { { "module", module }, {"key", pKey}, { "language", language } }));
            return TRANSLATION_NOT_FOUND;
        }
    }
}
