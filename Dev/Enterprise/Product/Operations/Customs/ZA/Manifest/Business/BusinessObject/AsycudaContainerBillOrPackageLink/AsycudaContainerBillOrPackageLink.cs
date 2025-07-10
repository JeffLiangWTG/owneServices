using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaContainerBillOrPackageLink : ASYCUDA.Business.AsycudaContainerBillOrPackageLink, Integration.Customs.ASYCUDA.ZAManifest.IAsycudaContainerBillOrPackageLink
	{
		public AsycudaContainerBillOrPackageLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		public new AsycudaPack Pack => (AsycudaPack)base.Pack;
	}
}
