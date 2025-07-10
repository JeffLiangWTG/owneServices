using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public class AsycudaBillSynchroniser : ASYCUDA.Business.AsycudaBillSynchroniser
	{
		public AsycudaBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment shipmentSource) : base(destination, shipmentSource)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_IncotermInfo, GetJS_IncoInfo()));
		}

		ZPropertyInfo GetJS_IncoInfo()
		{
			return new ZPropertyInfoString(Source, nameof(Source.JS_INCO));
		}
	}
}
