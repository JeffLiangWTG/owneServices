using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBookingLineProcessTask))]
	sealed class JobSupplierBookingLineProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Doesn't have a controller ID", true);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var line = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			return line.WorkflowItems.AddNew();
		}

		#endregion
	}
}
