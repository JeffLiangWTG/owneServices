using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaContainerLookups : ASYCUDA.Business.AsycudaContainerLookups
	{
		public AsycudaContainerLookups(AsycudaContainer parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList EmptyFullList => Factory.GetCachedValue<EmptyFullList>();

		public override RefContainerCollection ContainerTypes => Factory.GetCachedValue("TW.Manifest.ContainersSea", () => new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Sea));
	}
}
