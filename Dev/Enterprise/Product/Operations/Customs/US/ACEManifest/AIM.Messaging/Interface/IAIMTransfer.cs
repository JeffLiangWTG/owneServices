using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMTransfer
	{
		ZString DestinationAirport { get; }
		ZString DomesticInternationalIdentifier { get; }
		ZString BondedCarrierIDOrOnwardCarrier { get; }
		ZString BondedPremisesIdentifierOrInbondControlNumber { get; }
	}
}
