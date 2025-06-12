namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IStringMapper
    {
        string RemoveNewLines(string input);
        string RemoveNewLines(string input, int lineLen, int outLen);
        string RemoveNewLines(string input, int lineLen);
        string GetValueOrEmpty(string inputString);
        decimal ConvertToDecimal(string stringNumber);
        string PadLeft(string inputString, int length, string paddingChar);
        string PadRight(string inputString, int length, string paddingChar);
        string Replace(string inputString, string oldValue, string newValue);
        string ValueMappingWithReturnValue(string condition, string value);
        string ValueMappingWithReturnValue(string condition, string trueValue, string falseValue);
        string ConvertToCsv(string input);
        string ConvertToCsv(string input, bool simplyReplaceSpecialCharactersWithSpaces);
        string Format(string inputValue, string format);
        string FormatDecimal(string input, string format, bool treatInvalidInputAsZero = false);
    }
}
