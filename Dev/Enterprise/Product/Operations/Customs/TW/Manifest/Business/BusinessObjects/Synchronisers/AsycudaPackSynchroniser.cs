using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaPackSynchroniser : ASYCUDA.Business.AsycudaPackSynchroniser
	{
		public AsycudaPackSynchroniser(ASYCUDA.Business.AsycudaPack destination, PackLine source) : base(destination, source)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(((AsycudaBill)Destination.Bill).GoodsDescriptionInfo, Source.JL_DescriptionInfo));
		}
	}
}
