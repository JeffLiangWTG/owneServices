using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	public interface ICMMMessagingData
	{
		CMMMessageType Type { get; }
		string LloydsNumber { get; }
		string VoyageNumber { get; }
		string TransportMode { get; }

		/// <summary>
		/// NAD MS - Message Issuer / Sender
		/// </summary>
		ICMMOrganisationData MessageSender { get; }

		IEnumerable<ICMMEquipmentData> Equipment { get; }
	}
}
