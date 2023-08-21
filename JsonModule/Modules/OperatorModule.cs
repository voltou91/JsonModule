namespace JsonModule.Modules
{
    public class OperatorModule
    {
        private enum Operators
        {
            BOOL,
            EQUAL,
            NOT_EQUAL,
            SMALLER,
            GREATER,
            SMALLER_OR_EQUAL,
            GREATER_OR_EQUAL
        }

        private string booleanValue = "false";
        private string leftConditionValue = "0";
        private string rightConditionValue = "0";
        private object leftValue;
        private object rightValue;
        private Operators operatorValue;

        public OperatorModule(string pCondition)
        {
            int lIndexMark = pCondition.IndexOf('?');
            string lCondition = pCondition.Substring(0, lIndexMark); // Extract condition (x>y) from string x>y?a:b
            operatorValue = GetOperatorValue(lCondition); 

            if (operatorValue == Operators.BOOL) // If the condition is of the format x?a:b
            {
                booleanValue = lCondition;
            }
            else
            {
                string[] lConditionValues = GetConditionValues(lCondition); // Extract x and y values ​​from condition x>y
                leftConditionValue = lConditionValues[0];
                rightConditionValue = lConditionValues[1];
            }
            
            object[] lValues = GetValues(pCondition.Substring(lIndexMark + 1)); // Extract values ​​a and b from the string x>y?a:b
            leftValue = lValues[0];
            rightValue = lValues[1];
        }

        private object GetModuleOrValue(string pValue)
        {
            // If in our value we detect a ternary condition
            if (pValue.Contains('?') && pValue.Contains(':')) return new OperatorModule(pValue);
            return pValue;
        }

        private object[] GetValues(string pCondition)
        {
            int lSeparator = GetSeparatorEmplacement(pCondition);
            return new object[2] { GetModuleOrValue(pCondition.Substring(0, lSeparator)), GetModuleOrValue(pCondition.Substring(lSeparator+1)) }; ;
        }

        // Function to find out the location of the separator (:) in a ternary condition nesting
        private int GetSeparatorEmplacement(string pString)
        {
            // lMarksCount is 1 because we are in the string a:b and not x>y?a:b so we have already an "?" open
            int lMarksCount = 1;

            // If a "?" is found then the next ":" is ignored
            for (int i = 0; i < pString.Length; i++)
            {
                if (pString[i] == '?') lMarksCount++;
                if (pString[i] == ':') lMarksCount--;
                if (lMarksCount == 0) return i;
            }

            return -1;
        }

        private string[] GetConditionValues(string pCondition)
        {
            switch (operatorValue)
            {
                case Operators.EQUAL:
                    return pCondition.Split("==");
                case Operators.NOT_EQUAL:
                    return pCondition.Split("!=");
                case Operators.SMALLER_OR_EQUAL:
                    return pCondition.Split("<=");
                case Operators.GREATER_OR_EQUAL:
                    return pCondition.Split(">=");
                case Operators.SMALLER:
                    return pCondition.Split("<");
                case Operators.GREATER:
                    return pCondition.Split(">");
                default:
                    return new string[] { };
            }
        }

        private Operators GetOperatorValue(string pCondition)
        {
            if (pCondition.Contains("=="))
            {
                return Operators.EQUAL;
            }
            if (pCondition.Contains("!="))
            {
                return Operators.NOT_EQUAL;
            }
            if (pCondition.Contains("<=")) 
            {
                return Operators.SMALLER_OR_EQUAL;
            }
            if (pCondition.Contains(">="))
            {
                return Operators.GREATER_OR_EQUAL;
            }
            if (pCondition.Contains("<"))
            {
                return Operators.SMALLER;
            }
            if (pCondition.Contains(">"))
            {
                return Operators.GREATER;
            }
            return Operators.BOOL;
        }

        private bool GetConditionResolve(Dictionary<string, object> pDict)
        {
            object GetDictObject(string pObject) => pDict.TryGetValue(pObject, out object? lObject) ? lObject : pObject;

            switch (operatorValue)
            {
                case Operators.BOOL:
                    return Convert.ToBoolean(GetDictObject(booleanValue));
                case Operators.EQUAL:
                    return GetDictObject(leftConditionValue).ToString() == GetDictObject(rightConditionValue).ToString();
                case Operators.NOT_EQUAL:
                    return GetDictObject(leftConditionValue).ToString() != GetDictObject(rightConditionValue).ToString();
                case Operators.SMALLER:
                    return Convert.ToInt32(GetDictObject(leftConditionValue)) < Convert.ToInt32(GetDictObject(rightConditionValue));
                case Operators.GREATER:
                    return Convert.ToInt32(GetDictObject(leftConditionValue)) > Convert.ToInt32(GetDictObject(rightConditionValue));
                case Operators.SMALLER_OR_EQUAL:
                    return Convert.ToInt32(GetDictObject(leftConditionValue)) <= Convert.ToInt32(GetDictObject(rightConditionValue));
                case Operators.GREATER_OR_EQUAL:
                    return Convert.ToInt32(GetDictObject(leftConditionValue)) >= Convert.ToInt32(GetDictObject(rightConditionValue));
                default:
                    return false;
            }
        }

        public string Resolve(Dictionary<string, object> pDict)
        {
            if (GetConditionResolve(pDict))
            {
                if (leftValue is OperatorModule lOperator) return lOperator.Resolve(pDict);
                return leftValue.ToString() ?? string.Empty;
            }
            if (rightValue is OperatorModule rOperator) return rOperator.Resolve(pDict);
            return rightValue.ToString() ?? string.Empty;
        }
    }
}
