using System;
using static JsonModule.Cache;
using static JsonModule.Program;
using JsonModule.Modules;
using System.IO;

namespace JsonModule
{
    public static class Cache
    {
        // Un cache à double niveau, si tu te demandes pourquoi pas à un niveau avec une clé du genre "moduleName + language" c'est parce que ça va rechercher dans n fichiers * n langues, alors qu'avec un double niveau de dictionnaire ça va rechercher dans n fichiers puis une fois le fichier trouvé, dans n langues
        private static Dictionary<string, Dictionary<Language, TranslationModule>> translationsCache = new Dictionary<string, Dictionary<Language, TranslationModule>>();

        private static Dictionary<string, DataModule> datasCache = new Dictionary<string, DataModule>();

        private static Dictionary<string, OperatorModule> operatorCache = new Dictionary<string, OperatorModule>();
        public static DataModule GetDatas(string pModule) => datasCache.TryGetValue(pModule, out DataModule? lModule) ? lModule : CreateData(pModule, GetFileText(pModule));

        public static TranslationModule GetTranslation(string pModule, Language pLanguage) => translationsCache.TryGetValue(pModule, out Dictionary<Language, TranslationModule>? lDict) && lDict.TryGetValue(pLanguage, out TranslationModule? lModule) ? lModule : CreateTranslation(pModule, pLanguage, GetFileText(pModule));

        public static OperatorModule GetOperator(string pModule, string pCondition) => operatorCache.TryGetValue(pModule, out OperatorModule? lOperator) ? lOperator : CreateOperator(pModule, pCondition);

        private static DataModule CreateData(string pModule, string pJsonContent) => datasCache[pModule] = new DataModule(pJsonContent, pModule);

        private static TranslationModule CreateTranslation(string pModule, Language pLanguage, string pJsonContent)
        {
            // Si le module du fichier existe pas encore alors on créé une clé associer au fichier
            if (!translationsCache.ContainsKey(pModule)) translationsCache[pModule] = new Dictionary<Language, TranslationModule>();

            return translationsCache[pModule][pLanguage] = new TranslationModule(pJsonContent, pModule, pLanguage);
        }
        private static OperatorModule CreateOperator(string pModule, string pCondition) => operatorCache[pModule] = new OperatorModule(pCondition);

        private static string GetFileText(string pPath)
        {
            string lPath = Path.Exists(pPath) ? pPath : Path.Combine(PATH_TO_TEXT, pPath.Replace(FILE_SEPARATOR, Path.DirectorySeparatorChar)) + FILE_EXTENSION;
            if (File.Exists(lPath)) return File.ReadAllText(lPath);
            if (testMode) Console.WriteLine($"The JSON file '{pPath}' was not found.");
            return string.Empty;
        }

        // On check si y a des différences entres les langues d'un même fichier
        public static void CheckMissingElements(List<TranslationModule> pTranslationModules)
        {
            // C un peu comme une liste
            HashSet<string> allKeys = new HashSet<string>();

            // Pour tout les modules qu'on demande de checker en paramètres
            foreach (TranslationModule lTranslationModule in pTranslationModules)
            {
                // On ajoute toutes les keys avec UnionWith, UnionWith permet d'éviter les doublons donc on aura une sorte de liste avec tout les éléments de notre json sans doublons
                allKeys.UnionWith(lTranslationModule.GetKeys());
            }

            // Ensuite on reparcourt les modules en paramètres
            foreach (TranslationModule lTranslationModule in pTranslationModules)
            {
                // On récupère à nouveau toutes les keys de notre module
                IEnumerable<string> lKeys = lTranslationModule.GetKeys();

                // Puis pour notre espèce de liste on parcourt toutes les clés enregistrées
                foreach (string lKey in allKeys)
                {
                    // Si il manque une clé
                    if (!lKeys.Contains(lKey))
                    {
                        // Parcourt toutes les langues
                        foreach (Language lLanguage in Enum.GetValues(typeof(Language)))
                        {
                            // Si une des langues contient la clé alors on log le fait que cette langue contient la clé mais pas celle qu'on vient de checker
                            if (GetTranslation(lTranslationModule.module, lLanguage).GetKeys().Contains(lKey))
                            {
                                if (testMode) Console.WriteLine($"{lTranslationModule.module}: \"{lKey}\" is present in {lLanguage} but not in {lTranslationModule.language}");
                            }
                        }
                    }
                }
            }
        }

        public static void PreloadAllCache()
        {
            if (testMode) Console.WriteLine("Preloading cache...");
            string lJsonContent;
            foreach (string fileToPreload in GetAllFilesInDirectory().ToArray())
            {
                lJsonContent = GetFileText(fileToPreload);
                PreloadTranslationsCache(fileToPreload, lJsonContent);
                PreloadDatasCache(fileToPreload, lJsonContent);
            }
            if (testMode) Console.WriteLine("Cache preloaded");
        }

        // Utilise la récursivité pour parcourir tout les dossiers et dans chaque dossier tout les fichiers (ou dossiers)
        private static void PreloadTranslationsCache(string pFileToPreload, string pJsonContent)
        {
            // Pour chaque fichier, on créé une liste de TranslationModule, on les initialise dans toutes les langues et on les ajoutes
            List<TranslationModule> lListTranslationModule = new List<TranslationModule>();
            foreach (Language lLanguage in Enum.GetValues(typeof(Language))) lListTranslationModule.Add(CreateTranslation(GetModuleName(pFileToPreload), lLanguage, pJsonContent));

            // Puis on vérifie les différences entre tout les modules du fichier
            CheckMissingElements(lListTranslationModule);
        }

        private static void PreloadDatasCache(string pFileToPreload, string pJsonContent)
        {
            CreateData(GetModuleName(Path.ChangeExtension(pFileToPreload, string.Empty)[..^1]), pJsonContent);
        }

        private static string GetModuleName(string pPath) => pPath.Split(PATH_TO_TEXT)[1].Replace(Path.DirectorySeparatorChar, FILE_SEPARATOR);

        private static IEnumerable<string> GetAllFilesInDirectory(string directoryPath = PATH_TO_TEXT)
        {
            foreach (string file in Directory.GetFiles(directoryPath))
            {
                yield return file;
            }

            foreach (string subDirectory in Directory.GetDirectories(directoryPath))
            {
                foreach (string file in GetAllFilesInDirectory(subDirectory))
                {
                    yield return file;
                }
            }
        }
    }

    public class Program
    {
        public static bool testMode = true; // Changer cette ligne pour avoir les logs

        public const string FILE_EXTENSION = ".json";

        public const char FILE_SEPARATOR = '.';

        public const string TRANSLATION_ERROR = "TRANSLATION ERROR";

        public const string PATH_TO_TEXT = @"..\..\..\text\";

        public enum Language
        {
            fr,
            en
        }

        public Program() { }

        public static void Main()
        {
            Console.WriteLine("fdez?depoz?fdepovfn?".GetNOccurrence('?', 2));
            Console.WriteLine(StringFormatter.Format("salut {number>1?number<5?l'ami:my friend:number<-10?truc:non}", new Dictionary<string, object>() { { "number", -11 } }));
            Console.WriteLine(StringFormatter.Format("test {boolean?yep:nop}truc", new Dictionary<string, object>() { { "boolean", true } }));
            Console.WriteLine(StringFormatter.Format("test {boolean?yep:}truc", new Dictionary<string, object>() { { "boolean", false } }));
            Console.WriteLine(StringFormatter.Format("test {boolean?:nop}truc", new Dictionary<string, object>() { { "boolean", true } }));

            // Not necessary but preload all cache and check missing elements between languages
            PreloadAllCache();

            Console.WriteLine(StringFormatter.Format(GetTranslation("jsconfig1", Language.fr).GetAs("name"), new Dictionary<string, object>() { { "test1", "10" } }));

            Console.WriteLine(GetTranslation("test2.jsconfig2", Language.en).GetAs("desc.lower"));
            Console.WriteLine(GetTranslation("jsconfig1", Language.fr).GetAsEnumerable<string>("letters").GetRandomElement());
            Console.WriteLine(GetTranslation("jsconfig1", Language.en).Format("name", new Dictionary<string, object>() { { "test2", "YAY" } }));
            Console.WriteLine(GetTranslation("jsconfig1", Language.fr).GetAsEnumerable<Dictionary<string, string>>("t.a").GetRandomElement()?["c"]);
        }
    }
}
