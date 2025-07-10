using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface ICrew : Customs.Business.MessageBuilders.eManifest.ICrew
	{
		/// <summary>
		/// ACE ID/Proximity Card ID (C/10/50)
		/// </summary>
		ZString IdType { get; }

		/// <summary>
		/// US Address of Crew. (M)
		/// </summary>
		IAddress USAddress { get; }

		/// <summary>
		/// Shows permission to transport hazardous materials. (C/4)
		/// Condition: if Shipment is Hazmat.
		/// </summary>
		ZString HazmatEndorsement { get; }
	}
}
