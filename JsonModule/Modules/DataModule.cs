using static JsonModule.Program;
using JsonModule.Utils;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace JsonModule.Modules
{
    public class DataModule
    {
        protected Dictionary<string, object> dataDict = new Dictionary<string, object>();
        public string module { get; protected set; }

        public DataModule(string pJsonContent, string pModule, Language pLanguage = default)
        {
            module = pModule;
            if (string.IsNullOrEmpty(pJsonContent)) return;

            Init(pJsonContent, pLanguage);
        }

        protected virtual void Init(string pJsonContent, Language pLanguage)
        {
            JSchema lSchema = JSchema.Parse(pJsonContent);

            foreach (Language lLanguage in Enum.GetValues(typeof(Language)))
            {
                lSchema.ExtensionData.Remove(lLanguage.ToString());
            }

            foreach (KeyValuePair<string, JToken> lToken in lSchema.ExtensionData)
            {
                dataDict.Add(lToken.Key, MapObject(lToken.Value));
            }
        }

        protected DataModule(string pModule)
        {
            module = pModule;
        }

        public IEnumerable<string> GetKeys() => dataDict.Keys;

        protected virtual void MapJson(JToken pToken)
        {
            if (pToken is JArray lArray)
            {
                dataDict.Add(lArray.Path, MapObject(lArray));
            }
            else if (pToken is JContainer lObject)
            {
                foreach (JToken item in lObject.Children()) MapJson(item);
            }
            else if (pToken is JValue lValue)
            {
                dataDict.Add(lValue.Path, lValue.Value);
            }
        }

        private object MapObject(JToken pToken)
        {
            if (pToken is JArray lArray)
            {
                object[] lObjectArray = new object[lArray.Count];
                for (int i = 0; i < lArray.Count; i++)
                {
                    lObjectArray.SetValue(MapObject(lArray[i]), i);
                }

                if (!HasSameType(lObjectArray, out Type lType)) return lObjectArray;

                Array lTypedArray = Array.CreateInstance(lType, lObjectArray.Length);
                for (int i = 0; i < lObjectArray.Length; i++)
                {
                    lTypedArray.SetValue(lObjectArray[i], i);
                }
                return lTypedArray;
                
            }
            else if (pToken is JObject lObject)
            {
                Dictionary<string, object> lObjectDict = new Dictionary<string, object>();
                foreach (KeyValuePair<string, JToken?> lItem in lObject)
                {
                    lObjectDict.Add(lItem.Key, MapObject(lItem.Value));
                }
                if (!HasSameType(lObjectDict.Values, out Type lType)) return lObjectDict;

                Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), lType);
                dynamic lDict = Activator.CreateInstance(dictionaryType) ?? new Dictionary<string, object>();
                MethodInfo? lAddMethod = dictionaryType.GetMethod("Add");

                foreach (KeyValuePair<string, object> item in lObjectDict)
                {
                    lAddMethod?.Invoke(lDict, new object[] { item.Key, item.Value });
                }
                return lDict;
            }
            else if (pToken is JValue lValue) return CastJValue(lValue);
            return pToken;
        }

        private bool HasSameType<T>(IEnumerable<T> pEnumerable, out Type pType)
        {
            Type? lElementType = null;
            Type? lType;
            foreach (T lValue in pEnumerable)
            {
                lType = lValue?.GetType();

                lElementType = lElementType ?? lType;
               
                if (lElementType != lType)
                {
                    pType = typeof(object);
                    return false;
                }
            }
            pType = lElementType;
            return true;
        }

        private object CastJValue(JValue pValue)
        {
            switch (pValue.Type)
            {
                case JTokenType.Integer: 
                    return Convert.ToInt32(pValue.Value);
                case JTokenType.Float: 
                    return Convert.ToSingle(pValue.Value);
                case JTokenType.Boolean: 
                    return Convert.ToBoolean(pValue.Value);
                case JTokenType.Date: 
                    return Convert.ToDateTime(pValue.Value);
                default:
                    return pValue.ToString();
            }
        }

        private object Get(string pKey)
        {
            dataDict.TryGetValue(pKey, out object? lObject);
            return lObject ?? InvalidTranslation(pKey);
        }

        public T GetAs<T>(string pKey)
        {
            object lObject = Get(pKey);

            if (lObject.ToString() == TRANSLATION_NOT_FOUND) return default;
            if (lObject is T lTObject) return lTObject;
            
            try
            {
                return (T)Convert.ChangeType(lObject, typeof(T));
            }
            catch (Exception)
            {
                Console.WriteLine($"[Error]: You are trying to convert \"{lObject.GetType()}\" to \"{typeof(T)}\" !");
                return default;
            }
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
            if (testMode) Console.WriteLine($"[{module}]: invalid data {pKey}");
            return TRANSLATION_NOT_FOUND;
        }
    }
}
