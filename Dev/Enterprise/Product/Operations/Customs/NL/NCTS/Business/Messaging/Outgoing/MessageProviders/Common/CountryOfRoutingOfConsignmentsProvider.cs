using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CountryOfRoutingOfConsignmentsProvider : ICountryOfRoutingOfConsignment
{
	public CountryOfRoutingOfConsignmentsProvider(ZString countryCode, int sequenceNumber)
	{
		Country = countryCode;
		SequenceNumeric = sequenceNumber;
	}

	public int SequenceNumeric { get; }

	public string Country { get; }
}
