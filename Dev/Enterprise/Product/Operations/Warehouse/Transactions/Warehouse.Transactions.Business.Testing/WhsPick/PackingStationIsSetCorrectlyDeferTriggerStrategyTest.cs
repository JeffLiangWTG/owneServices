using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PackingStationIsSetCorrectlyDeferTriggerStrategyTest : WhsTestCaseWithFactory
	{
		public void TestRunType_ReturnsInsertOrUpdate()
		{
			var strategy =
				ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IPackingStationIsSetCorrectlyDeferTriggerStrategy));
			AssertEquals(strategy.RunType, TriggerRunType.InsertOrUpdate);
		}

		public void TestShouldDeferTrigger_ReturnsTrueWhenTriggeringColumnIsUpdated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var strategy =
				ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IPackingStationIsSetCorrectlyDeferTriggerStrategy));
			AssertEquals("Trigger should not be deferred if there are no changes to the WhsPickLine", false,
				strategy.ShouldDeferTrigger(pick));

			foreach (var (column, value) in GetFieldsThatAreNotRelevantForOverCommitTrigger())
			{
				((INeedRow)pick).Row[column] = value;
				AssertEquals(
					"Trigger should not be deferred if a column that does not require trigger deferral is updated",
					false, strategy.ShouldDeferTrigger(pick));
			}

			pick.WP_WL_PackingStation = data.Whs1.DefaultLocation.PK;
			AssertEquals("Trigger should be deferred if a column that requires trigger deferral is updated", true,
				strategy.ShouldDeferTrigger(pick));
		}

		static IReadOnlyCollection<(string Column, IConvertible Value)>
			GetFieldsThatAreNotRelevantForOverCommitTrigger()
		{
			return new (string, IConvertible)[]
			{
				(WhsPickSchema.Constants.WP_PickPalletsByLabel, true),
				(WhsPickSchema.Constants.WP_PercentageComplete, 10),
				(WhsPickSchema.Constants.WP_PickOption, "MAN"),
				(WhsPickSchema.Constants.WP_PickNo, "P058")
			};
		}

		public void TestShouldDeferTrigger_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingArea = Helper.CreateArea(data.Whs1, "PACK", AreaTypes.Codes.FreeStore, isPutawayArea: false);

			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CC", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			packingLocation.WLV_WA_PickingArea = packingArea.PK;
			pick.WP_WL_PackingStation = packingLocation.PK;

			pick.Orders.AddRange(new[] { order });
			pick.AutoAllocateItemsWithMock();
			AssertNoExceptionThrown("Should save without error.", () => Factory.Save());
		}
	}
}
