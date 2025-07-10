using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[ModuleID(ModuleId.SupplierBookingLine)]
	public class JobSupplierBookingLineCollection : ActiveBusinessObjectCollection<JobSupplierBookingLine>
	{
		public JobSupplierBookingLineCollection(JobSupplierBooking master)
			: base(master.Factory, master, null, JobSupplierBookingLineSchema.JSL_JSB_Booking)
		{
		}

		public JobSupplierBookingLineCollection(OrderLine master)
			: base(master.Factory, master, null, JobSupplierBookingLineSchema.JSL_JO_OrderLine)
		{
		}

		public JobSupplierBookingLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public JobSupplierBookingLineCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
