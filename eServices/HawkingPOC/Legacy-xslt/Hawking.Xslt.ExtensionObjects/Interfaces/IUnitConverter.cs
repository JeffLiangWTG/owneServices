namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IUnitConverter
    {
        string Convert(string value, string fromUnit, string toUnit);
    }
}
