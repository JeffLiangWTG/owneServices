using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
		, Integration.Customs.ASYCUDA.NZManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaContainerBillOrPackageLink Pivot => (AsycudaContainerBillOrPackageLink)base.Pivot;
		public new AsycudaBill Bill => (AsycudaBill)base.Bill;
		public new AsycudaContainer Container => (AsycudaContainer)base.Container;
	}
}
