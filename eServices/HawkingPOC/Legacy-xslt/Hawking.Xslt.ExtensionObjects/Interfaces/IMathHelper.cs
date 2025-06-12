namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IMathHelper
    {
        string RoundAwayFromZero(string inputValue);
        string RoundAwayFromZero(string inputValue, string decimalPlaces);
        string RoundToEven(string inputValue);
        string RoundToEven(string inputValue, string decimalPlaces);
    }
}
