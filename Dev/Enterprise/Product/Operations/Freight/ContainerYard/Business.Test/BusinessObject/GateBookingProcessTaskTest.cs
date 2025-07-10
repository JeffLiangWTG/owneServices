using CargoWise.EntityFramework;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateBookingProcessTask))]
	sealed class GateBookingProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Unnecessary test case", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var gateBooking = Factory.New<GateBooking>() as IWorkflowProvider;
			return gateBooking.WorkflowItems.AddNew();
		}
	}
}
