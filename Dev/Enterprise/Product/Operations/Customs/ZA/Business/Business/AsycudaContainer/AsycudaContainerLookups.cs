using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaContainerLookups : ManifestBase.AsycudaContainerLookups
	{
		public AsycudaContainerLookups(AsycudaContainer parent) : base(parent)
		{
		}

		public CodeDescriptionPairList EmptyFullList => Factory.GetCachedValue<EmptyFullList>();

		public CodeDescriptionPairList SealTypeList => Factory.GetCachedValue<SealTypeList>();

		public new AsycudaContainer Parent => (AsycudaContainer)base.Parent;
	}
}
