using Enterprise.Packing.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class PkgPackageOutersCollection : PkgPackageCollection
	{
		public PkgPackageOutersCollection(DtbBookingConsignment consignment)
			: base(consignment.PackageJob)
		{
		}
	}
}
