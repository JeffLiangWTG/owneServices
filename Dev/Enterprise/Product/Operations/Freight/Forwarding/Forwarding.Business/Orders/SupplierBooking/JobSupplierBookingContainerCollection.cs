using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[DependentBusinessObject(typeof(JobSupplierBooking), "JobContainers")]
	public class JobSupplierBookingContainerCollection : DependentBusinessObjectCollection<ForwardingContainer, JobSupplierBooking>
	{
		public JobSupplierBookingContainerCollection(JobSupplierBooking master) : base(master)
		{
		}

		protected override bool AllowNewCore => false;

		protected override string FkColumnName => JobContainerSchema.JC_JSB_SupplierBooking.Name;
	}
}
