using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadListLineProcessTask))]
	sealed class ContainerLoadListLineProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Doesn't have a controller ID", true);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			var containerLoadListLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			containerLoadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			return containerLoadListLine.WorkflowItems.AddNew();
		}

		#endregion
	}
}
