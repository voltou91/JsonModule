using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static JsonModule.Program;

namespace JsonModule.Modules
{
    public class DataModule
    {
        protected Dictionary<string, object> dataDict = new Dictionary<string, object>();
        public string module { get; protected set; }

        public DataModule(string pJsonContent, string pModule, Language pLanguage = 0)
        {
            module = pModule;
            if (string.IsNullOrEmpty(pJsonContent)) return;

            Init(pJsonContent, pLanguage);
        }

        protected virtual void Init(string pJsonContent, Language pLanguage)
        {
            Dictionary<string, object>? lDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(pJsonContent);

            foreach (Language lLanguage in Enum.GetValues(typeof(Language)))
            {
                lDict?.Remove(lLanguage.ToString());
            }

            FromObject(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(lDict, Formatting.Indented)));
        }

        protected DataModule(string pModule)
        {
            module = pModule;
        }

        public IEnumerable<string> GetKeys() => dataDict.Keys;

        protected virtual void FromObject(object? pObject, string currentKey = "")
        {
            // Si c'est un array
            if (pObject is JArray JListValue)
            {
                List<object> lList = new List<object>();
                foreach (JToken lToken in JListValue)
                {
                    if (lToken.Type == JTokenType.Object)
                    {
                        // Si c'est un objet (dictionnaire), on le convertit en Dictionary<string, object>
                        lList.Add(lToken.ToObject<Dictionary<string, object>>());
                    }
                    else
                    {
                        lList.Add(lToken.ToString());
                    }
                }

                dataDict.Add(currentKey, lList);
            }
            // Si c'est un dictionnaire du json
            else if (pObject is JObject JDictValue)
            {
                foreach (KeyValuePair<string, JToken?> lDictValue in JDictValue)
                {
                    string newKey = string.IsNullOrEmpty(currentKey) ? lDictValue.Key : currentKey + FILE_SEPARATOR + lDictValue.Key;
                    FromObject(lDictValue.Value, newKey);
                }
            }
            else
            {
                dataDict.Add(currentKey, pObject?.ToString() ?? string.Empty);
            }
        }

        private object Get(string pKey)
        {
            dataDict.TryGetValue(pKey, out object? lObject);
            return lObject ?? InvalidTranslation(pKey);
        }

        public IEnumerable<T> GetAsEnumerable<T>(string pKey)
        {
            try
            {
                var result = Get(pKey);

                // Si la valeur est une chaîne JSON, on la désérialise en IEnumerable<T>
                if (result is string jsonString)
                {
                    var deserializedValue = JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);
                    return deserializedValue;
                }

                // Si la valeur est déjà de type IEnumerable<T>, on la retourne directement
                if (result is IEnumerable<T> enumerable)
                {
                    return enumerable;
                }

                // Si la valeur est IEnumerable<object>, on essaie de la convertir en IEnumerable<T>
                if (result is IEnumerable<object> objectEnumerable)
                {
                    List<T> lCastedList = new List<T>();
                    foreach (object lItem in objectEnumerable)
                    {
                        if (lItem is T lCastedItem)
                        {
                            lCastedList.Add(lCastedItem);
                        }
                        else if (lItem is IDictionary<string, object> lDictionaryItem)
                        {
                            // Convertir le dictionnaire en T si possible (par exemple, Dictionary<string, string>)
                            lCastedList.Add(JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(lDictionaryItem)));
                        }
                    }
                    return lCastedList;
                }
            }
            catch { }

            return new List<T>(); // Ou une autre action par défaut si le cast échoue
        }

        public T GetAs<T>(string pKey)
        {
            try
            {
                return (T)Get(pKey);
            }
            catch { }
            InvalidTranslation(pKey);
            return default;
        }

        public string GetAs(string pKey)
        {
            return Get(pKey).ToString() ?? string.Empty;
        }

        public string Format(string pKey, Dictionary<string, object> pReplacement)
        {
            return StringFormatter.Format(Get(pKey).ToString() ?? string.Empty, pReplacement);
        }


        //Petit log avant de retourne un message d'erreur
        protected virtual string InvalidTranslation(string pKey)
        {
            if (testMode) Console.WriteLine($"{module}: invalid data {pKey}");
            return TRANSLATION_ERROR;
        }
    }
}
