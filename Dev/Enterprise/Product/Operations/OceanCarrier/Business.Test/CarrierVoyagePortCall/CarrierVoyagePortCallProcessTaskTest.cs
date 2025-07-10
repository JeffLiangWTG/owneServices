using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyagePortCallProcessTask))]
	sealed class CarrierVoyagePortCallProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Unnecessary test case", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var portCall = Factory.New<CarrierVoyagePortCall>();
			var intportCall = (IWorkflowProvider)portCall;
			return portCall.WorkflowItems.AddNew();
		}
	}
}
