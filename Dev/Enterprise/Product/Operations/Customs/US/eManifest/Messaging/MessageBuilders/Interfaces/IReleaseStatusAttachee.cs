using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	interface IReleaseStatusAttachee
	{
		void UpdateReleaseStatus(ZString shipmentControlNumber, ZString releaseStatus, ZDateTime releaseStatusDate);
		void UpdateReleaseStatusOnAllLinkedShipments(ZString releaseStatus, ZDateTime releaseStatusDate);
	}
}
