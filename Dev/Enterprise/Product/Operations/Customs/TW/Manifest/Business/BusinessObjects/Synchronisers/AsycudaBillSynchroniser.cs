using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaBillSynchroniser : ASYCUDA.Business.AsycudaBillSynchroniser
	{
		public AsycudaBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment shipmentSource) : base(destination, shipmentSource)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_RL_NKPortOfLoadingInfo, Source.JS_RL_NKOriginInfo));
		}

		protected override ASYCUDA.Business.AsycudaPackCollectionSynchroniser GetAsycudaPackCollectionSynchroniser() => new AsycudaPackCollectionSynchroniser(Source, Destination);
	}
}
