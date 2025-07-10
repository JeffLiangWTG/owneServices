using CargoWise.Types;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
