using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaContainerLookups : ASYCUDA.Business.AsycudaContainerLookups
	{
		public AsycudaContainerLookups(ASYCUDA.Business.AsycudaContainer parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList EmptyFullList => Factory.GetCachedValue<ZaEmptyFullIndicatorList>();

		public CodeDescriptionPairList LandedPurposeList => Factory.GetCachedValue<LandedPurposeList>();
	}
}
