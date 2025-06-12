namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IUNLOCOAccessor
    {
        string GetUNLOCOfromIATA(string IATACode);
        string GetStateFromUNLOCO(string UNLOCO);
        string ConvertUTCToLocalTimeByUNLOCO(string utcTime, string unloco);
    }
}
