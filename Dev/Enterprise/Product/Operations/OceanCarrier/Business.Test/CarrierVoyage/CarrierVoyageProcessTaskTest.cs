using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyageProcessTask))]
	sealed class CarrierVoyageProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Unnecessary test case", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var voyage = Factory.New<CarrierVoyage>();
			var intVoyage = (IWorkflowProvider)voyage;
			return intVoyage.WorkflowItems.AddNew();
		}
	}
}
