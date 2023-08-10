using System;

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
            string lCondition = pCondition.Substring(0, lIndexMark); //Extrait la condition (x>y) de la chaîne x>y?a:b
            operatorValue = GetOperatorValue(lCondition); 

            if (operatorValue == Operators.BOOL) // Si la condition est du format x?a:b
            {
                booleanValue = lCondition;
            }
            else
            {
                string[] lConditionValues = GetConditionValues(lCondition);
                leftConditionValue = lConditionValues[0];
                rightConditionValue = lConditionValues[1];
            }
            
            object[] lValues = GetValues(pCondition.Substring(lIndexMark + 1)); //Extrait les valeurs (a:b) de la chaîne x>y?a:b
            leftValue = lValues[0];
            rightValue = lValues[1];
        }

        private object GetModuleOrValue(string pValue)
        {
            //Si dans notre valeur on détecte une condition ternaire
            if (pValue.Contains('?') && pValue.Contains(':'))
            {
                return new OperatorModule(pValue);
            }
            return pValue;
        }

        private object[] GetValues(string pCondition)
        {
            int lSeparator = GetSeparatorEmplacement(pCondition);
            return new object[2] { GetModuleOrValue(pCondition.Substring(0, lSeparator)), GetModuleOrValue(pCondition.Substring(lSeparator+1)) }; ;
        }

        // Fonction pour connaître quel est l'emplacement du séparateur (:) dans une imbrication de condition ternaire
        private int GetSeparatorEmplacement(string pString)
        {
            // Etant donné qu'on est dans les values (a:b), et non dans la condition, on a déjà un "?" d'ouvert. Donc lMarksCount vaut 1.
            int lMarksCount = 1;

            // On parcours tout les char de nos values, si un nouveau "?" est trouvé alors c'est une condition ternaire imbriqué donc le prochain ":" on s'en fou
            for (int i = 0; i < pString.Length; i++)
            {
                if (pString[i] == '?') lMarksCount++;
                if (pString[i] == ':') lMarksCount--;
                if (lMarksCount == 0) return i;
            }
            //Une fois le lMarksCount à 0, c'est qu'on a trouvé notre emplacement, alors on le return.
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
            if (operatorValue == Operators.BOOL)
            {
                return Convert.ToBoolean(pDict.TryGetValue(booleanValue, out object? pBool) ? pBool : booleanValue);
            }

            int lLeft = Convert.ToInt32(pDict.TryGetValue(leftConditionValue, out object? pValue) ? pValue : leftConditionValue);
            int lRight = Convert.ToInt32(pDict.TryGetValue(rightConditionValue, out pValue) ? pValue : rightConditionValue);
            switch (operatorValue)
            {
                case Operators.BOOL:
                    
                case Operators.EQUAL:
                    return lLeft == lRight;
                case Operators.NOT_EQUAL:
                    return lLeft != lRight;
                case Operators.SMALLER:
                    return lLeft < lRight;
                case Operators.GREATER:
                    return lLeft > lRight;
                case Operators.SMALLER_OR_EQUAL:
                    return lLeft <= lRight;
                case Operators.GREATER_OR_EQUAL:
                    return lLeft >= lRight;
                default:
                    return false;
            }
        }

        // Fonction qui utilise la récursivité pour parcourir ses enfants "OperatorModule" si il y en a.
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
