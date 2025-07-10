using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaContainerBillOrPackageLink : ASYCUDA.Business.AsycudaContainerBillOrPackageLink, Integration.Customs.ASYCUDA.ACEManifest.IAsycudaContainerBillOrPackageLink
	{
		public AsycudaContainerBillOrPackageLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		public new AsycudaPack Pack => (AsycudaPack)base.Pack;
	}
}
