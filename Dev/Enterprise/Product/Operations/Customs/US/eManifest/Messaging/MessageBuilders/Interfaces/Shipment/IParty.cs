using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface IParty : Customs.Business.MessageBuilders.eManifest.IParty
	{
		/// <summary>
		/// Unique Party Identifier. (C/12)
		/// ACE ID – 10 char,
		/// DUNS # - 11 char
		/// IRS # - 12 char
		/// FAST ID – 7 char
		/// Filer Code - 3 char
		/// </summary>
		ZString PartyId { get; }

		/// <summary>
		/// ACE ID/FAST/filer code/FIRMS/SSN or EIN/DUNS/SCAC/Customs assigned number
		/// </summary>
		ZString PartyIdType { get; }

		/// <summary>
		/// ABI routing code for Broker identified as a Shipment-Party if a Broker Download is required. (C/17)
		/// The Shipment information would be sent to the Broker via ABI with the routing code specified. Filer Code + Port Code + Office Code.
		/// Condition: For Shipment-Party = 'Brokers' if Broker Download is required
		/// </summary>
		ZString ABIRoutingCode { get; }
	}
}
