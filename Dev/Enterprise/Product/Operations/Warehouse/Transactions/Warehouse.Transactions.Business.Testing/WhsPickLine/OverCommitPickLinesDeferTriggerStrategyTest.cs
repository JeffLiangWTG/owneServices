using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class OverCommitPickLinesDeferTriggerStrategyTest : WhsTestCaseWithFactory
	{
		static IReadOnlyCollection<(string Column, IConvertible Value)>
			GetFieldsThatAreNotRelevantForOverCommitTrigger()
		{
			return new (string, IConvertible)[]
			{
				(WhsPickLineSchema.Constants.WZ_GS_NKAssignedTo, "ABC"),
				(WhsPickLineSchema.Constants.WZ_VerifiedEmpty, "X"),
				(WhsPickLineSchema.Constants.WZ_OriginalReservedQty, 10m),
				(WhsPickLineSchema.Constants.WZ_F3_NKAllocatedPackType, "XYZ")
			};
		}

		public void TestRunType_ReturnsInsertOrUpdate()
		{
			var strategy =
				ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IOverCommitPickLinesDeferTriggerStrategy));
			AssertEquals(strategy.RunType, TriggerRunType.InsertOrUpdate);
		}

		public void TestShouldDeferTrigger_ReturnsTrueWhenTriggeringColumnIsUpdated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();

			var strategy =
				ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IOverCommitPickLinesDeferTriggerStrategy));
			AssertEquals("Trigger should not be deferred if there are no changes to the WhsPickLine", false,
				strategy.ShouldDeferTrigger(pickLine));

			foreach (var (column, value) in GetFieldsThatAreNotRelevantForOverCommitTrigger())
			{
				((INeedRow)pickLine).Row[column] = value;
				AssertEquals(
					"Trigger should not be deferred if a column that does not require trigger deferral is updated",
					false, strategy.ShouldDeferTrigger(pickLine));
			}

			pickLine.WZ_Units = 5m;
			AssertEquals("Trigger should be deferred if a column that requires trigger deferral is updated", true,
				strategy.ShouldDeferTrigger(pickLine));
		}
	}
}
