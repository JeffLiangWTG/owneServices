using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CountryProvider : ICountry
{
	public CountryProvider(int sequenceNumber, ZString countryCode)
	{
		SequenceNumber = sequenceNumber.ToString();
		Country = countryCode;
	}

	public string SequenceNumber { get; }

	public string Country { get; }
}
