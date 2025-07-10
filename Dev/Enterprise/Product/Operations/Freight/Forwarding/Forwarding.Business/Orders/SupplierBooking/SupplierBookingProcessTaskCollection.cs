using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class SupplierBookingProcessTaskCollection : ProcessTaskCollection
	{
		public SupplierBookingProcessTaskCollection(JobSupplierBooking supplierBooking) : base(supplierBooking)
		{
		}

		public new SupplierBookingProcessTask this[int index]
		{
			get { return (SupplierBookingProcessTask)Elements[index]; }
		}

		public new SupplierBookingProcessTask AddNew()
		{
			return (SupplierBookingProcessTask)base.AddNew();
		}
	}
}
