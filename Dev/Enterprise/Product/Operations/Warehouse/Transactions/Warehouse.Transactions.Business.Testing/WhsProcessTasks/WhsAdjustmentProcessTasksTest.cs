using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentProcessTasks))]
	public class WhsAdjustmentProcessTasksTest : WhsDocketProcessTasksTest
	{
		#region TestTypeDecider

		public void TestTypeDecider()
		{
			WhsAdjustment adjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			ProcessTask task = adjustment.WorkflowItems.AddNew();
			AssertEquals(adjustment, task.Parent);
			Factory.Save();

			AssertEquals("type decided correctly", typeof(WhsAdjustmentProcessTasks), new BusinessObjectFactory().Load<ProcessTask>(task.PK).GetType());
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsAdjustment(Client, Warehouse);
		}

		#endregion
	}
}
