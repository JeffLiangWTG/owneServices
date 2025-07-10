using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobSupplierBookingLineProcessTaskCollection(JobSupplierBookingLine parent)
		: ProcessTaskCollection(parent)
	{
		public new JobSupplierBookingLineProcessTask this[int index]
		{
			get { return (JobSupplierBookingLineProcessTask)Elements[index]; }
		}

		public new JobSupplierBookingLineProcessTask AddNew()
		{
			return (JobSupplierBookingLineProcessTask)base.AddNew();
		}
	}
}
