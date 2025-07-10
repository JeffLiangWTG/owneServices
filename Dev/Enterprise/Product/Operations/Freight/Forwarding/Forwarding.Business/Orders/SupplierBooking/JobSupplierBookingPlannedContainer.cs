using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[DependentBusinessObject(typeof(JobSupplierBooking), "PlannedContainers")]
	public class JobSupplierBookingPlannedContainer : OrderContainer, Integration.Forwarding.IOrderContainer
	{
		public JobSupplierBookingPlannedContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
