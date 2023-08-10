using System;
using static JsonModule.Cache;
using static JsonModule.Program;
using JsonModule.Modules;
using JsonModule.Utils;

namespace JsonModule
{
    public static class Cache
    {
        //A double-level cache, because a single-level dictionary with a key like "moduleName + language" will search n files * n languages, whereas a double-level cache will search n files then n languages.
        private static Dictionary<string, Dictionary<Language, TranslationModule>> translationsCache = new Dictionary<string, Dictionary<Language, TranslationModule>>();

        private static Dictionary<string, DataModule> datasCache = new Dictionary<string, DataModule>();

        private static Dictionary<string, OperatorModule> operatorCache = new Dictionary<string, OperatorModule>();


        public static DataModule GetDatas(string pModule) => datasCache.TryGetValue(pModule, out DataModule? lModule) ? lModule : CreateData(pModule, GetAndReadFileText(pModule));

        public static TranslationModule GetTranslation(string pModule, Language pLanguage) => translationsCache.TryGetValue(pModule, out Dictionary<Language, TranslationModule>? lDict) && lDict.TryGetValue(pLanguage, out TranslationModule? lModule) ? lModule : CreateTranslation(pModule, pLanguage, GetAndReadFileText(pModule));

        public static OperatorModule GetOperator(string pModule) => operatorCache.TryGetValue(pModule, out OperatorModule? lOperator) ? lOperator : CreateOperator(pModule);

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
            string lPath = Path.Exists(pPath) ? pPath : Path.Combine(PATH_TO_TEXT, pPath.Replace(FILE_SEPARATOR, Path.DirectorySeparatorChar)) + FILE_EXTENSION;
            if (File.Exists(lPath)) return File.ReadAllText(lPath);
            if (testMode) Console.WriteLine($"The JSON file '{pPath}' was not found.");
            return string.Empty;
        }

        public static void CheckMissingElements(List<TranslationModule> pTranslationModules)
        {
            // It's a bit like a list
            HashSet<string> allKeys = new HashSet<string>();

            foreach (TranslationModule lTranslationModule in pTranslationModules)
            {
                // We add all the keys with UnionWith.
                // UnionWith avoids duplicates, so we'll have a sort of list with all the keys in our json without duplicates.
                allKeys.UnionWith(lTranslationModule.GetKeys());
            }

            foreach (TranslationModule lTranslationModule in pTranslationModules)
            {
                // We get all the keys of our module
                IEnumerable<string> lKeys = lTranslationModule.GetKeys();

                // Then, in our hashset, we go through all the registered keys
                foreach (string lKey in allKeys)
                {
                    // If a key is missing
                    if (!lKeys.Contains(lKey))
                    {
                        // Browse all languages
                        foreach (Language lLanguage in Enum.GetValues(typeof(Language)))
                        {
                            // If one of the modules has the key, then we log the fact that this language has the key but not the current one.
                            if (GetTranslation(lTranslationModule.module, lLanguage).GetKeys().Contains(lKey))
                            {
                                Console.WriteLine($"{lTranslationModule.module}: \"{lKey}\" is present in {lLanguage} but not in {lTranslationModule.language}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Preload all cache in all json file and check all missing elements
        /// </summary>
        public static void PreloadAllCache()
        {
            if (testMode) Console.WriteLine("Preloading cache...");
            string lJsonContent;
            foreach (string fileToPreload in GetAllFilesInDirectory().ToArray())
            {
                lJsonContent = GetAndReadFileText(fileToPreload);
                PreloadTranslationsCache(fileToPreload, lJsonContent);
                PreloadDatasCache(fileToPreload, lJsonContent);
            }
            if (testMode) Console.WriteLine("Cache preloaded");
        }

        private static void PreloadTranslationsCache(string pFileToPreload, string pJsonContent)
        {
            // For each file, we create a list of TranslationModules, initialize all the modules in all the languages and add them to the list to check for missing elements between them.
            List<TranslationModule> lListTranslationModule = new List<TranslationModule>();
            foreach (Language lLanguage in Enum.GetValues(typeof(Language))) lListTranslationModule.Add(CreateTranslation(GetModuleName(pFileToPreload), lLanguage, pJsonContent));
            if (!testMode) return;

            CheckMissingElements(lListTranslationModule);
        }

        private static void PreloadDatasCache(string pPathFile, string pJsonContent)
        {
            CreateData(GetModuleName(pPathFile), pJsonContent);
        }

        private static string GetModuleName(string pPath)
        {
            string lPath = Path.ChangeExtension(pPath, string.Empty)[..^1];
            return lPath.Split(PATH_TO_TEXT)[1].Replace(Path.DirectorySeparatorChar, FILE_SEPARATOR);
        }

        private static IEnumerable<string> GetAllFilesInDirectory(string pDirectoryPath = PATH_TO_TEXT)
        {
            foreach (string lFile in Directory.GetFiles(pDirectoryPath))
            {
                yield return lFile;
            }

            foreach (string lSubDirectory in Directory.GetDirectories(pDirectoryPath))
            {
                foreach (string lFile in GetAllFilesInDirectory(lSubDirectory))
                {
                    yield return lFile;
                }
            }
        }
    }

    public class Program
    {
        public static bool testMode = false; // Change to true, to get the logs

        public const string FILE_EXTENSION = ".json";

        public const char FILE_SEPARATOR = '.';

        public const string TRANSLATION_ERROR = "TRANSLATION ERROR";

        public const string PATH_TO_TEXT = @"..\..\..\Text\";

        public enum Language
        {
            fr,
            en
        }

        public Program() { }

        public static void Main()
        {
            //Not necessary
            PreloadAllCache();

            TranslationModule lTranslationModuleFR = GetTranslation("jsconfig1", Language.fr);
            TranslationModule lTranslationModuleEN = GetTranslation("jsconfig1", Language.en);

            Console.WriteLine(lTranslationModuleFR.GetAsEnumerable<string>("letters").GetRandomElement());
            Console.WriteLine(lTranslationModuleFR.Format("name", new Dictionary<string, object>() { { "test1", "10" } }));
            Console.WriteLine(lTranslationModuleFR.GetAsEnumerable<Dictionary<string, string>>("t.a").GetRandomElement()?["c"]);
            
            Console.WriteLine(lTranslationModuleEN.Format("name", new Dictionary<string, object>() { { "test2", "YAY" } }));

            lTranslationModuleEN = GetTranslation("test2.jsconfig2", Language.en);
            Console.WriteLine(lTranslationModuleEN.GetAs("desc.lower"));
            Console.WriteLine(lTranslationModuleEN.Format("bonus", new Dictionary<string, object>() { {"number", 1} }));

            Console.WriteLine(StringFormatter.Format("Hi {number>1?number<5?something:my friend:number<-10?people:what ?}", new Dictionary<string, object>() { { "number", -10 } }));
        }
    }
}
