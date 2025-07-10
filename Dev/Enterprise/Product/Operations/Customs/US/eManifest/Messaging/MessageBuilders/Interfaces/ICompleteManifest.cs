using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface ICompleteManifest : ITrip, IEDIFACTMessageAttachee
	{
		//TODO: Printed eManifest sheet is required.

		/// <summary>
		/// A unique identifier issued by the carrier to reference a transmission. (C/50)
		/// It is returned to the carrier in output messages (CUSRES).
		/// </summary>
		ZString TransmissionReferenceNumber { get; }

		ZBool IsFinalized { get; }

		IEnumerable<IShipment> Shipments { get; }
	}
}
