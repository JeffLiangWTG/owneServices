using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaContainerBillOrPackageLink : ManifestBase.AsycudaContainerBillOrPackageLink
		, Integration.Customs.ZA.IAsycudaContainerBillOrPackageLink
	{
		public AsycudaContainerBillOrPackageLink(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		public new AsycudaPack Pack => (AsycudaPack)base.Pack;

		protected override ManifestBase.AsycudaContainerBillOrPackageLinkValidation GetNewValidation() => new AsycudaPackageContainerLinkValidation(this);

		public new AsycudaPackageContainerLinkValidation Validation => (AsycudaPackageContainerLinkValidation)base.Validation;

		public new AsycudaPackageContainerLinkLookups Lookups => (AsycudaPackageContainerLinkLookups)base.Lookups;

		protected override ManifestBase.AsycudaContainerBillOrPackageLinkLookups GetNewLookups() => new AsycudaPackageContainerLinkLookups(this);
	}
}
