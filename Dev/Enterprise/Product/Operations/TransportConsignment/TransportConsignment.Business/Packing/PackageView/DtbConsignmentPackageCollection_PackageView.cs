using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentPackageCollection_PackageView : PackageCollection_PackageView
	{
		public DtbConsignmentPackageCollection_PackageView(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DtbConsignmentPackage_PackageView(Factory);
		}

		protected override Package_PackageView GetPackage_PackageView(PkgPackage package, DtbTransport transport)
		{
			return new DtbConsignmentPackage_PackageView(package, (DtbBookingConsignment)transport);
		}
	}
}
