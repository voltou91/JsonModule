using static JsonModule.Cache;
using static JsonModule.Program;
using JsonModule.Modules;
using JsonModule.Utils;

namespace JsonModule
{
    public static class Cache
    {
        // A double-level cache, because a single-level dictionary with a key like "moduleName + language" will search n files * n languages, whereas a double-level cache will search n files then n languages.
        // For example there is 500 files and 3 languages, with a one level dictionary there is 1500 possibilities but with a double-level dictionary there is 503 possibilities.
        private static Dictionary<string, Dictionary<Language, TranslationModule>> translationsCache = new Dictionary<string, Dictionary<Language, TranslationModule>>();

        private static Dictionary<string, DataModule> datasCache = new Dictionary<string, DataModule>();

        private static Dictionary<string, OperatorModule> operatorCache = new Dictionary<string, OperatorModule>();

        #region Get Module
        public static DataModule GetDatas(string pModule) => datasCache.TryGetValue(pModule, out DataModule? lData) ? lData : CreateData(pModule, GetAndReadFileText(pModule));

        public static TranslationModule GetTranslation(string pModule, Language pLanguage) => GetTranslationLanguages(pModule).TryGetValue(pLanguage, out TranslationModule? lTranslation) ? lTranslation : CreateTranslation(pModule, pLanguage, GetAndReadFileText(pModule));

        private static Dictionary<Language, TranslationModule> GetTranslationLanguages(string pModule)
        {
            translationsCache.TryGetValue(pModule, out Dictionary<Language, TranslationModule>? lTranslations);
            return lTranslations ?? new Dictionary<Language, TranslationModule>();
        }

        public static OperatorModule GetOperator(string pModule) => operatorCache.TryGetValue(pModule, out OperatorModule? lOperator) ? lOperator : CreateOperator(pModule);
        #endregion

        #region Create Module
        private static DataModule CreateData(string pModule, string pJsonContent) => datasCache[pModule] = new DataModule(pJsonContent, pModule);

        private static TranslationModule CreateTranslation(string pModule, Language pLanguage, string pJsonContent)
        {
            // If the first cache level doesn't exist
            if (!translationsCache.ContainsKey(pModule)) translationsCache[pModule] = new Dictionary<Language, TranslationModule>();

            return translationsCache[pModule][pLanguage] = new TranslationModule(pJsonContent, pModule, pLanguage);
        }
        private static OperatorModule CreateOperator(string pModule) => operatorCache[pModule] = new OperatorModule(pModule);

        private static string GetAndReadFileText(string pPath)
        {
            string lPath = Path.Exists(pPath) ? pPath : Path.Combine(PATH_TO_TEXT_DIRECTORY, pPath.Replace(FILE_SEPARATOR, Path.DirectorySeparatorChar)) + FILE_EXTENSION;
            if (File.Exists(lPath)) return File.ReadAllText(lPath);
            if (TEST_MODE) Console.WriteLine(PROGRAM_TRANSLATIONS.Format("Errors.JsonFileNotFound", new Dictionary<string, object>() { {"path", pPath} }));
            return string.Empty;
        }
        #endregion

        #region Preload modules
        /// <summary>
        /// Preload all cache in all json file and check all missing elements
        /// </summary>
        public static void PreloadAllCache()
        {
            if (TEST_MODE) Console.WriteLine(PROGRAM_TRANSLATIONS.GetAs("Utils.PreloadingCache"));
            string lJsonContent;
            foreach (string lFileToPreload in GetAllFilesInDirectory())
            {
                lJsonContent = GetAndReadFileText(lFileToPreload);
                PreloadTranslationsCache(GetModuleName(lFileToPreload), lJsonContent);
                CreateData(GetModuleName(lFileToPreload), lJsonContent);
            }
            if (TEST_MODE) Console.WriteLine(PROGRAM_TRANSLATIONS.GetAs("Utils.CachePreloaded"));
        }

        private static void PreloadTranslationsCache(string pModule, string pJsonContent)
        {
            // For each file, we initialize all the modules in all the languages and check for missing elements between them.
            foreach (Language lLanguage in Enum.GetValues(typeof(Language))) CreateTranslation(pModule, lLanguage, pJsonContent);
            if (TEST_MODE) CheckMissingElements(pModule);
        }

        /// <summary>
        /// Check for missing module key translations
        /// </summary>
        /// <param name="pModule"></param>
        public static void CheckMissingElements(string pModule)
        {
            // It's like a list but with specialized features
            HashSet<string> lAllTranslationKeys = new HashSet<string>();

            TranslationModule[] lTranslationModules = GetTranslationLanguages(pModule).Values.ToArray();

            // UnionWith avoids duplicates, so we'll get a list with all the keys in our json without duplicates.
            foreach (TranslationModule lTranslation in lTranslationModules) lAllTranslationKeys.UnionWith(lTranslation.GetKeys());

            foreach (TranslationModule lTranslationModule in lTranslationModules)
            {
                // Then we use Except fonction which return an enumerator of differents elements between two enumerator, here our lTranslationModule and our HashSet.
                foreach (string lKey in lAllTranslationKeys.Except(lTranslationModule.GetKeys()))
                {
                    // We are checking in each languages who contains this lKey and then we log it
                    foreach (TranslationModule lModule in lTranslationModules)
                    {
                        // This is not useful to check whether it contains the key in our lTranslationModule and lModule, because it's the same instance.
                        if (lTranslationModule.language == lModule.language) continue;

                        if (lModule.GetKeys().Contains(lKey))
                        {
                            Console.WriteLine(PROGRAM_TRANSLATIONS.Format("Errors.KeyNotPresentInSpecificLanguage", new Dictionary<string, object>() { { "module", lTranslationModule.module }, { "key", lKey }, { "language", lModule.language }, { "actualLanguage", lTranslationModule.language } }));
                        }
                    }
                }
            }
        }


        private static IEnumerable<string> GetAllFilesInDirectory(string pDirectoryPath = PATH_TO_TEXT_DIRECTORY)
        {
            foreach (string lFile in Directory.GetFiles(pDirectoryPath)) yield return lFile;

            foreach (string lSubDirectory in Directory.GetDirectories(pDirectoryPath))
            {
                foreach (string lFile in GetAllFilesInDirectory(lSubDirectory))
                {
                    yield return lFile;
                }
            }
        }
        
        private static string GetModuleName(string pPath)
        {
            string lPath = Path.ChangeExtension(pPath, string.Empty)[..^1];
            return lPath.Split(PATH_TO_TEXT_DIRECTORY)[1].Replace(Path.DirectorySeparatorChar, FILE_SEPARATOR);
        }
        #endregion
    }

    public class Program
    {
        // Change to true/false to enable/disable the logs
        public static bool TEST_MODE = true; 

        // Change language to change logs's dialect
        public static TranslationModule PROGRAM_TRANSLATIONS = GetTranslation("ProgramTranslations", Language.en);

        public const string FILE_EXTENSION = ".json";

        public const char FILE_SEPARATOR = '.';

        public const string TRANSLATION_NOT_FOUND = "TRANSLATION NOT FOUND";

        public const string PATH_TO_TEXT_DIRECTORY = @"..\..\..\Text\";

        // You can add/remove languages here
        public enum Language
        {
            fr,
            en
        }

        public Program() { }

        public static void Main()
        {
            // Not necessary but preload cache and check missing translations
            PreloadAllCache();

            TranslationModule lTranslationModuleFR = GetTranslation("Example1", Language.fr);
            TranslationModule lTranslationModuleEN = GetTranslation("Example1", Language.en);

            Console.WriteLine(lTranslationModuleFR.GetAs("name"));
            Console.WriteLine(lTranslationModuleEN.GetAs("desc.short"));

            DataModule lDataModule = GetDatas("Example1");
            Console.WriteLine(lDataModule.GetAs<string[]>("attacksId").GetRandomElement());

            // Here an example of using StringFormatter.Format to use condition in string
            Console.WriteLine(StringFormatter.Format("{number>0?number<5?Hi:Hello:number<-10?Bye:GoodBye}", new Dictionary<string, object>() { { "number", -10 } }));

            // Json file in directory needs/ a "." separator (Determined by FILE_SEPARATOR)
            lTranslationModuleEN = GetTranslation("ExampleDirectory.Example2", Language.en);

            // You can use StringFormatter.Format directly from a Data/Translations Module
            Console.WriteLine(lTranslationModuleEN.Format("randomSentence", new Dictionary<string, object>() { { "number", 1 } }));
        }
    }
}
