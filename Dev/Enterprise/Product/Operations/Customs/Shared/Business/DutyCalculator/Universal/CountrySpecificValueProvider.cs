namespace Enterprise.Customs.DutyCalculator;

public class CountrySpecificValueProvider : Integration.Customs.ICountrySpecificValueProvider
{
	public IDictionary<string, decimal> GetCountrySpecificValueList(Integration.Customs.ICusEntryLine entryLine)
		=> new Dictionary<string, decimal>();
}
