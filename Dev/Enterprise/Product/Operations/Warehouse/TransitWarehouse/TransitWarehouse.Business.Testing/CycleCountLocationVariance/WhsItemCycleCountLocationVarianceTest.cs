using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemCycleCountLocationVariance))]
	class WhsItemCycleCountLocationVarianceTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var warehouse = Helper.CreateWarehouse("TTT", "A", 2, 1);
			factory.Save();

			var now = ZDateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(warehouse.DefaultLocation, CycleCountLocationStatuses.Codes.Completed, now, now.AddMinutes(3), now.AddMinutes(3), "TTT");

			return Helper.CreateCycleCountLocationVariance(cycleCount, packageNotInWhsID: "P1");
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}

		WhsTransitTestHelper helper;

		#endregion
	}

	#region Triggers_WhsCycleCountLocationVarianceTest class

	public class Triggers_WhsCycleCountLocationVarianceTest : WhsTransitTestCaseWithFactory
	{
		#region TestTG_WhsItemCycleCountVariance_CannotBeModifiedOrDeleted

		[ExpectNoExceptions]
		public void TestTG_WhsCycleCountVariance_CannotBeModifiedOrDeleted_Delete()
		{
			var warehouse = Helper.CreateWarehouse("TTT", "A", 2, 1);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(warehouse.DefaultLocation, CycleCountLocationStatuses.Codes.Completed, now, now.AddMinutes(3), now.AddMinutes(3), "TTT");
			var variance = Helper.CreateCycleCountLocationVariance(cycleCount, packageNotInWhsID: "P1");
			Factory.Save();

			variance.Delete(); // variance records cannot be deleted once created
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsItemCycleCountLocationVariance.PreventDeleteTriggerError, true), "Trigger should prevent delete variance records.");
		}

		#endregion
	}

	#endregion
}
