using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaBillSynchroniser : ASYCUDA.Business.AsycudaBillSynchroniser
	{
		public AsycudaBillSynchroniser(AsycudaBill destination, ForwardingShipment shipmentSource)
			: base(destination, shipmentSource)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			var consol = Source.Consols[0];
			if (consol != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.ABL_OA_ContainerAgentInfo, consol.JK_OA_ReceivingForwarderAddress_ReadOnlyInfo));
			}
		}
	}
}
