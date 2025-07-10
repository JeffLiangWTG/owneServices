using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.DataTransfer
{
	public abstract class CustomsJobCommand : BaseHVLVRelatedJobCommand
	{
		public CustomsJobCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override bool NeedPreScreening => true;
	}
}
