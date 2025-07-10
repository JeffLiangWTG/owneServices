using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaContainerBillOrPackageLink : ASYCUDA.Business.AsycudaContainerBillOrPackageLink, Integration.Customs.ASYCUDA.SGAccess.IAsycudaContainerBillOrPackageLink
	{
		public AsycudaContainerBillOrPackageLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		public new AsycudaPack Pack => (AsycudaPack)base.Pack;
	}
}
