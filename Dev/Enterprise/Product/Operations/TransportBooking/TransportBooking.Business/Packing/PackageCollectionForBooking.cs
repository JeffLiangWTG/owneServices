using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.TransportBookings.Business
{
	public class PackageCollectionForBooking : ActiveBusinessObjectCollection<PkgPackage>
	{
		public PackageCollectionForBooking(DtbBooking booking)
			: base(booking.Factory, new BookingPackageRelationship(booking))
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
