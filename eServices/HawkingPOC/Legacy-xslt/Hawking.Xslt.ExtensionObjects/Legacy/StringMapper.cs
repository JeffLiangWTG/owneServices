using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.Xslt.ExtensionObjects.Legacy
{
    public class StringMapper : IStringMapper
    {
        public string RemoveNewLines(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return input.Replace(Environment.NewLine, " ");
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Convert text with line breaks to flat string with padding for specified line length.
        /// </summary>
        public string RemoveNewLines(string input, int lineLen, int outLen)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            string[] inLines = Regex.Split(input, @"\r\n|\n|\r");
            //input.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            List<string> outLines = new List<string>();
            foreach (string line in inLines)
            {
                for (int i = 0; i <= line.Length; i += lineLen)
                {
                    outLines.Add(line.Substring(i, Math.Min(line.Length - i, lineLen)).PadRight(lineLen));
                }
            }

            string result = string.Concat(outLines);

            if (outLen > 0)
            {
                if (result.Length > outLen)
                {
                    return result.Remove(outLen);
                }
                else
                {
                    return result.PadRight(outLen);
                }
            }
            else
            {
                return result;
            }
        }
        public string RemoveNewLines(string input, int lineLen)
        {
            return RemoveNewLines(input, lineLen, 0);
        }
        public string GetValueOrEmpty(string inputString)
        {
            return string.IsNullOrEmpty(inputString) ? string.Empty : inputString;
        }

        public decimal ConvertToDecimal(string stringNumber)
        {
            try
            {
                return Convert.ToDecimal(stringNumber);
            }
            catch
            {
                return 0M;
            }
        }

        public string PadLeft(string inputString, int length, string paddingChar)
        {
            if (string.IsNullOrEmpty(paddingChar))
            {
                paddingChar = " ";
            }

            return inputString.PadLeft(length, Convert.ToChar(paddingChar));
        }

        public string PadRight(string inputString, int length, string paddingChar)
        {
            if (string.IsNullOrEmpty(paddingChar))
            {
                paddingChar = " ";
            }

            return inputString.PadRight(length, Convert.ToChar(paddingChar));
        }

        public string Replace(string inputString, string oldValue, string newValue)
        {
            return inputString.Replace(oldValue, newValue);
        }

        public string ValueMappingWithReturnValue(string condition, string value)
        {
            return ValueMappingWithReturnValue(condition, value, string.Empty);
        }

        public string ValueMappingWithReturnValue(string condition, string trueValue, string falseValue)
        {
            return Convert.ToBoolean(condition) ? trueValue : falseValue;
        }

        public string ConvertToCsv(string input)
        {
            return ConvertToCsv(input, false);
        }

        /// <summary>
        /// Convert input string to csv format.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="simplyReplaceSpecialCharactersWithSpaces"></param>
        /// <returns></returns>
        public string ConvertToCsv(string input, bool simplyReplaceSpecialCharactersWithSpaces)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(input))
            {
                if (simplyReplaceSpecialCharactersWithSpaces)
                {
                    result = input.Replace(Environment.NewLine, " ").Replace(",", " ");
                }
                else
                {
                    bool shouldEncloseWithinDoubleQuote = (input.IndexOf(Environment.NewLine) >= 0) || (input.IndexOf(",") >= 0);

                    if (shouldEncloseWithinDoubleQuote)
                    {
                        result = "\"" + input.Replace("\"", "\"\"") + "\"";
                    }
                    else
                    {
                        result = input;
                    }
                }
            }

            return result;
        }

        public string Format(string inputValue, string format)
        {

            if (!int.TryParse(inputValue, out int value))
            {
                return "";
            }

            return value.ToString(format);
        }

        public string FormatDecimal(string input, string format, bool treatInvalidInputAsZero = false)
        {
            var result = string.Empty;

            if (double.TryParse(input, out double value))
            {
                result = value.ToString(format);
            }
            else
            {
                if (treatInvalidInputAsZero) result = 0D.ToString(format);
            }

            return result;
        }
    }
}
