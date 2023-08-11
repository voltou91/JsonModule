using System;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static JsonModule.Program;
using JsonModule.Utils;

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
            JObject lObject = JsonConvert.DeserializeObject(pJsonContent) as JObject ?? new JObject();
          
            foreach (Language lLanguage in Enum.GetValues(typeof(Language)))
            {
                lObject.Remove(lLanguage.ToString());
            }

            FromObject(lObject);
        }

        protected DataModule(string pModule)
        {
            module = pModule;
        }

        public IEnumerable<string> GetKeys() => dataDict.Keys;

        protected virtual void FromObject(JToken pObject, string currentKey = "")
        {
            if (pObject is JArray lArray)
            {
                dataDict.Add(currentKey, AddToList(lArray));
            }
            else if (pObject is JObject JDictValue)
            {
                foreach (KeyValuePair<string, JToken?> lDictValue in JDictValue)
                {
                    FromObject(lDictValue.Value ?? new JValue(0), string.IsNullOrEmpty(currentKey) ? lDictValue.Key : currentKey + FILE_SEPARATOR + lDictValue.Key);
                }
            }
            else
            {
                dataDict.Add(currentKey, pObject.ToString());
            }
        }

        private List<object> AddToList(JArray pArray)
        {
            List<object> lList = new List<object>();
            foreach (JToken? lToken in pArray) lList.Add(lToken.ToString());
            return lList;
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
                return (T)Convert.ChangeType(Get(pKey), typeof(T));
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

        protected virtual string InvalidTranslation(string pKey)
        {
            if (testMode) Console.WriteLine($"{module}: invalid data {pKey}");
            return TRANSLATION_ERROR;
        }
    }
}
