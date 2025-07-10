using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsCycleCountWaveProcessTask))]
	public class WhsCycleCountWaveProcessTaskTest : ProcessTaskTest
	{
		#region TestWhsCycleCountWaveProcessTaskCollectionCorrectlyAddsWhsCycleCountWaveProcessTask

		public void TestWhsCycleCountWaveProcessTaskCollectionCorrectlyAddsWhsCycleCountWaveProcessTask()
		{
			var wave = Factory.New<WhsCycleCountWave>();
			var processTask = wave.WorkflowItems.AddNew();
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var reloadedProcessTask = otherFactory.Load<WhsCycleCountWaveProcessTask>(processTask.PK);

			ProcessTask newProcessTask = null;
			AssertNoExceptionThrown(() => newProcessTask = new ProcessTaskCollectionView(reloadedProcessTask.ParentTaskCollection).AddNew());
			AssertEquals(typeof(WhsCycleCountWaveProcessTask), newProcessTask.GetType());
		}

		#endregion

		#region TestParentControllerIDIsOverridenForNonStandAloneTasks

		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Doesn't have a controller ID", true);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var wave = Factory.New<WhsCycleCountWave>();
			return wave.WorkflowItems.AddNew();
		}

		protected virtual WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));

		WhsTestHelperFunctions helper;
	}
}
