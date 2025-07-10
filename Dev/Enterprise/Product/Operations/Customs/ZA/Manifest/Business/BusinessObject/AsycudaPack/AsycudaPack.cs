using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
		, Integration.Customs.ASYCUDA.ZAManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool IsPackUQSynchroniserReadonlyCore => false;

		public new AsycudaContainerBillOrPackageLink Pivot => (AsycudaContainerBillOrPackageLink)base.Pivot;
		public new AsycudaBill Bill => (AsycudaBill)base.Bill;
		public new AsycudaContainer Container => (AsycudaContainer)base.Container;
		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;
		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);
		public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;
		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);
	}
}
