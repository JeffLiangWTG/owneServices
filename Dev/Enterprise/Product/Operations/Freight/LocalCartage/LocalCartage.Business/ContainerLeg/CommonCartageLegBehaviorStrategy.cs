using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageLegBehaviorStrategy
	{
		public CommonCartageLegBehaviorStrategy()
		{
		}

		public virtual void BookedMoveCartageLegLinkCreated(CommonCartageLeg cartageLeg)
		{
		}

		public virtual void BookedMoveCartageLegLinkBroken(CommonCartageLeg cartageLeg)
		{
		}

		public virtual void WorkSheetCartageLegLinkCreated(CommonCartageLeg cartageLeg)
		{
		}

		public virtual void WorkSheetCartageLegLinkBroken(CommonCartageLeg cartageLeg)
		{
		}

		public virtual void PickupAddressIDChanged(CommonCartageLeg cartageLeg, ZGuid originalValue)
		{
		}

		public virtual void WaitPointAddressIDChanged(CommonCartageLeg cartageLeg, ZGuid originalValue)
		{
		}

		public virtual void DeliveryAddressIDChanged(CommonCartageLeg cartageLeg, ZGuid originalValue)
		{
		}

		public DocumentSupporter DocumentSupporter(CommonCartageLeg cartageLeg)
		{
			return CartageLegDocumentSupporter.New(cartageLeg);
		}
	}
}
