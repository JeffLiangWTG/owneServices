using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(SupplierBookingProcessTask))]
	sealed class SupplierBookingProcessTaskTest : ProcessTaskTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			return booking.WorkflowItems.AddNew();
		}

		#endregion
	}
}
