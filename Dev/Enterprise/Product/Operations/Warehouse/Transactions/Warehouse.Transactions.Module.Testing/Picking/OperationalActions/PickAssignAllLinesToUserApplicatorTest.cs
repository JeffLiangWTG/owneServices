using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickAssignAllLinesToUserApplicator))]
	class PickAssignAllLinesToUserApplicatorTest : LinesAssignerActionMethodApplicatorTest<PickAssignAllLinesToUserApplicator, WhsPick>
	{
		protected override PickAssignAllLinesToUserApplicator GetNewApplicator()
		{
			return new PickAssignAllLinesToUserApplicator(Factory);
		}

		protected override Type ExpectedApplicatorValidationType => typeof(AssignAllLinesApplicatorValidation<WhsPick>);

		#region TestOperationalAction

		[TestDate(2012, 06, 04)]
		public void TestOperationalAction()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 5);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var notify = new TestNotificationBuffer();

			// Setup test data.

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[1]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[2]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[3]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[4]);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4");
			var order5 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O5");

			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order4, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order5, data.Part1, 10m);

			var createdPick = Helper.CreatePickByAttachingOrders(order1);
			var cancelledPick = Helper.CreatePickByAttachingOrders(order2);
			var finalizedPick = Helper.CreatePickByAttachingOrders(order3);
			var createdPickWithNoUnAssignedLines = Helper.CreatePickByAttachingOrders(order4);
			var createdPickWithLinesArePicking = Helper.CreatePickByAttachingOrders(order5);

			var orderedInvetoryForCreatedPick = createdPick.OrderedInventories[0];
			var orderedInvetoryForCancelledPick = cancelledPick.OrderedInventories[0];
			var orderedInvetoryForFinalizedPick = finalizedPick.OrderedInventories[0];
			var orderedInventoryForCreatedPickWithNoUnAssignedLines = createdPickWithNoUnAssignedLines.OrderedInventories[0];
			var orderedInventoryForCreatedPickWithLinesArePicking = createdPickWithLinesArePicking.OrderedInventories[0];

			var allocatedEmptyPickedDateForCreatedPick = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick.AvailableInventories[0], true, ZDateTimeOffset.Empty);
			var allocatedNonEmptyPickedDateForCreatedPick = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick.AvailableInventories[1], true, ZDateTimeOffset.Now);
			var nonAllocatedEmptyPickedDateForCreatedPick = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick.AvailableInventories[2], false, ZDateTimeOffset.Empty);
			var nonAllocatedNonEmptyPickedDateForCreatedPick = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick.AvailableInventories[3], false, ZDateTimeOffset.Now);
			var unAssignedLine = Helper.SetAvailableInventory(orderedInventoryForCreatedPickWithNoUnAssignedLines.AvailableInventories[3], true, ZDateTimeOffset.Now);

			var lineIsPicking = Helper.SetAvailableInventory(orderedInventoryForCreatedPickWithLinesArePicking.AvailableInventories[4], true, ZDateTimeOffset.Empty);
			var user = Helper.CreateGlbStaff("XYZ", "XYZ");
			foreach (var line in lineIsPicking.PickLines)
			{
				line.WZ_GS_NKAssignedTo = user.GS_Code;
				line.WZ_IsPicking = true;
			}

			var allocatedEmptyPickedDateForCancelledPick = Helper.SetAvailableInventory(orderedInvetoryForCancelledPick.AvailableInventories[0], true, ZDateTimeOffset.Empty);
			var allocatedEmptyPickedDateForFinalizedPick = Helper.SetAvailableInventory(orderedInvetoryForFinalizedPick.AvailableInventories[0], true, ZDateTimeOffset.Empty);

			createdPick.OrderedInventories.Add(orderedInvetoryForCreatedPick);
			createdPickWithNoUnAssignedLines.OrderedInventories.Add(orderedInventoryForCreatedPickWithNoUnAssignedLines);
			cancelledPick.OrderedInventories.Add(orderedInvetoryForCancelledPick);
			finalizedPick.OrderedInventories.Add(orderedInvetoryForFinalizedPick);

			Factory.Save();

			// ToGenerate pick No's for all picks before changing the pick status.

			createdPick.WP_PickStatus = PickStatus.Codes.Created;
			createdPickWithNoUnAssignedLines.WP_PickStatus = PickStatus.Codes.Created;
			cancelledPick.WP_PickStatus = PickStatus.Codes.Cancelled;
			finalizedPick.WP_PickStatus = PickStatus.Codes.Finalised;
			finalizedPick.WP_FinalizedDateUtc = ZDateTime.UtcNow;
			finalizedPick.GetAllPickLines().FirstOrDefault().WZ_PickedDateTime = ZDateTimeOffset.Now;
			order3.WD_DocketStatus = WhsOrderStatus.Codes.Departed;
			order3.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			Factory.Save();

			// Test operational action.

			var errorMessage = string.Format("WARNING: Pick [HL {0}] - No Lines were assigned because the Pick is Finalized.\n\r" +
											 "WARNING: Pick [HL {1}] - No Lines were assigned because the Pick is Canceled.\n\r" +
											 "INFO: Pick [HL {2}] - All lines have been assigned successfully.\n\r" +
											 "INFO: Pick [HL {3}] - No Lines were assigned because there are no unassigned lines, or Picking has already commenced.\n\r" +
											 "INFO: Pick [HL {4}] - No Lines were assigned because there are no unassigned lines, or Picking has already commenced.\n\r", finalizedPick.WP_PickNo, cancelledPick.WP_PickNo, createdPick.WP_PickNo, createdPickWithNoUnAssignedLines.WP_PickNo, createdPickWithLinesArePicking.WP_PickNo);

			ApplyApplicator(new WhsPick[] { finalizedPick, cancelledPick, createdPick, createdPickWithNoUnAssignedLines, createdPickWithLinesArePicking }, errorMessage);

			AssertEquals(true, allocatedEmptyPickedDateForCreatedPick.PickLines.All(pl => pl.AssignedTo == SelectedUser));
			AssertEquals(true, allocatedNonEmptyPickedDateForCreatedPick.PickLines.All(pl => pl.AssignedTo == null));
			AssertEquals(true, nonAllocatedEmptyPickedDateForCreatedPick.PickLines.All(pl => pl.AssignedTo == null));
			AssertEquals(true, nonAllocatedNonEmptyPickedDateForCreatedPick.PickLines.All(pl => pl.AssignedTo == null));
			AssertEquals(true, allocatedEmptyPickedDateForCancelledPick.PickLines.All(pl => pl.AssignedTo == null));
			AssertEquals(true, allocatedEmptyPickedDateForFinalizedPick.PickLines.All(pl => pl.AssignedTo == null));
		}

		#endregion
	}
}
