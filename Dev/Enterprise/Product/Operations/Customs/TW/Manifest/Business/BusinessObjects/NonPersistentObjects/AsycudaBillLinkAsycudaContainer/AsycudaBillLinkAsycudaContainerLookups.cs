using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaBillLinkAsycudaContainerLookups : ZLookups
	{
		public AsycudaBillLinkAsycudaContainerLookups(AsycudaBillLinkAsycudaContainer parent) : base(parent)
		{
		}

		public new BusinessObjectFactory Factory => factory ?? (factory = ((AsycudaBillLinkAsycudaContainer)Parent).Bill.Factory);

		BusinessObjectFactory factory;

		public RefContainerCollection ContainerTypes => Factory.GetCachedValue("TW.Manifest.ContainersSea", () => new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Sea));

		public CodeDescriptionPairList EmptyFullList => Factory.GetCachedValue<EmptyFullList>();
	}
}
