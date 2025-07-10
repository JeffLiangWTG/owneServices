using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DummyWithDtbBookingWithIServiceDirection : DummyWithDtbBooking
	{
		public DummyWithDtbBookingWithIServiceDirection(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new DummyWithDtbBookingJobInvoicingSupporterWithIServiceDirection()); }
		}
	}
}
