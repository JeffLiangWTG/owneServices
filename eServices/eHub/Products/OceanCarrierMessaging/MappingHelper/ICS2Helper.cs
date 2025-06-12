using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class ICS2Helper
  {
    public const string NorthernIreland = "NORTHERN IRELAND";
    const char countryAndStateSeperator = '-';
    List<string> consigneeCountriesAndStates = new List<string>();

    public void ClearConsigneeCountriesAndStates()
    {
      consigneeCountriesAndStates.Clear();
    }

    public void AddConsigneeCountryAndState(string country, string state)
    {
      var value = string.Concat(country, countryAndStateSeperator, state);
      if (!consigneeCountriesAndStates.Contains(value))
      {
        consigneeCountriesAndStates.Add(value);
      }
    }

    public List<string> GetConsigneeCountryAndState()
    {
      return consigneeCountriesAndStates;
    }

    public bool IsConsigneeCountryAndStateInEUNorwaySwitzerlandNorthernIreland()
    {
      var countriesInEUNorwaySwitzerland = new List<string> { "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE", "NO", "CH" };
      var statesInNorthernIreland = new List<string> { "ABC", "AND", "ANN", "ANT", "ARD", "ARM", "BFS", "BLA", "BLY", "BNB", "CCG", "CGV", "CKF", "CKT", "CLR", "CSR", "DGN", "DOW", "DRS", "DRY", "FER", "FMO", "LBC", "LMV", "LRN", "LSB", "MEA", "MFT", "MUL", "MYL", "NDN", "NIR", "NMD", "NTA", "NYM", "OMH", "STB" };

      var isInEUNorwaySwitzerlandNorthernIreland = false;

      foreach (var countryAndState in consigneeCountriesAndStates)
      {
        var country = countryAndState.Split(countryAndStateSeperator)[0];
        var state = countryAndState.Split(countryAndStateSeperator)[countryAndState.Split(countryAndStateSeperator).Length - 1];

        if (countriesInEUNorwaySwitzerland.Contains(country))
        {
          isInEUNorwaySwitzerlandNorthernIreland = true;
        }
        if (country == "GB" && statesInNorthernIreland.Contains(state))
        {
          isInEUNorwaySwitzerlandNorthernIreland = true;
        }
      }

      return isInEUNorwaySwitzerlandNorthernIreland;
    }

    public virtual bool IsICS2Port(string portcode)
    {
      var portCountryCode = new StringHelper().SubstringSafe(portcode, 0, 2);

      return IsNorthernIreland(portcode) || CountryIsInEuropeUnion(portCountryCode) ||
          portCountryCode == Constants.CountryCodes.Switzerland ||
          portCountryCode == Constants.CountryCodes.Norway;
    }

    public bool CountryIsInEuropeUnion(string countryCode)
    {
      return Constants.CountriesInEuropeanUnion.Contains(countryCode.ToUpper());
    }

    public bool IsNorthernIreland(string portcode)
    {
      var administratorRegion = new OCMHelper().GetAdministrativeRegion(portcode);

      return !string.IsNullOrEmpty(administratorRegion) && administratorRegion.ToUpper() == NorthernIreland;
    }

    public string GetPOBox(string address)
    {
      if (string.IsNullOrEmpty(address))
      {
        return string.Empty;
      }

      var pattern = @"(P\.?\s*O\.?\s*Box\s*\d+)|(PO\s*Box\s*\d+)|(Post\s*Office\s*Box\s*\d+)|(POB\s*\d+)|" +
                    @"(POBOX\s*\d+)|(P\.?\s*O\.?\s*Box\s*\d+)|(P\.?\s*O\s*Box\s*\d+)";
      var regex = new Regex(pattern, RegexOptions.IgnoreCase);
      var match = regex.Match(address);

      return match.Success ? match.Value : string.Empty;
    }
  }
}
