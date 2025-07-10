using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsCycleCountWave))]
	public class WhsCycleCountWaveTest : WhsBusinessObjectTestCase
	{
		#region IWorkflowProvider

		public void TestOnDelete_IWorkflowProvider()
		{
			var wave = Factory.New<WhsCycleCountWave>();
			wave.WorkflowItems.AddNew();
			AssertEquals("Have one Workflow Items", 1, wave.WorkflowItems.Count);

			wave.Delete();
			AssertEquals("Workflow Items all removed", 0, wave.WorkflowItems.Count);
		}

		#endregion
	}
}
