using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business;

public class ItineraryWrapper : IItinerary
{
	public ItineraryWrapper(ItineraryCountry itineraryCountry, int sequenceNumeric)
	{
		this.itineraryCountry = Argument.NotNull(itineraryCountry, nameof(itineraryCountry));
		SequenceNumeric = sequenceNumeric;
	}

	readonly ItineraryCountry itineraryCountry;

	public int SequenceNumeric { get; }

	public string RoutingCountryCode => itineraryCountry.CY_Code;
}
