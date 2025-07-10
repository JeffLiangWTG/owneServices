using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetInstructionProcessTask))]
	sealed class DtbConsignmentRunSheetInstructionProcessTaskTest : ProcessTaskTest
	{
		#region TestParentControllerIDIsOverridenForNonStandAloneTasks

		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Doesn't have a controller ID", true);
		}

		#endregion

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<DtbConsignmentRunSheetInstruction>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
