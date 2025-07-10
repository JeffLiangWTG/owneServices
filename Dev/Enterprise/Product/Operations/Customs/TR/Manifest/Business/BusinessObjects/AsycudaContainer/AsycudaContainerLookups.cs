using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaContainerLookups : ASYCUDA.Business.AsycudaContainerLookups
	{
		public AsycudaContainerLookups(AsycudaContainer parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList RelationList => Factory.GetCachedValue("TRAsycudaContainerLookups.RelationList", () => new RelationList());
	}
}
