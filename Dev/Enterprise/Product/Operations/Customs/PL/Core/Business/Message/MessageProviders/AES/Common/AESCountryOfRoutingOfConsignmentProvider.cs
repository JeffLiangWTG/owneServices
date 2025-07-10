using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business;

public class AESCountryOfRoutingOfConsignmentProvider(ItineraryCountry itineraryCountry, int sequenceNumber) : ICountryOfRoutingOfConsignment
{
	readonly ItineraryCountry itineraryCountry = Argument.NotNull(itineraryCountry, nameof(itineraryCountry));

	public int SequenceNumber { get; } = sequenceNumber;

	public string Country => itineraryCountry.CY_Code;
}
