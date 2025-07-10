using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Integration;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.US;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;
using WhsPickLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsPickLine;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickLine))]
	public class WhsPickLineTest : WhsBusinessObjectTestCase
	{
		#region TestDBHitsForCommittedToPickOnInventory

		public void TestDBHitsForCommittedToPickOnInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			for (int i = 0; i < 9; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 25m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1, 25m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part1, 25m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "4", data.Part1, 25m);
			var pick = Helper.CreatePickNew(order1, order2, order3, order4);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var dbHitCountDocket = otherFactory.GetTableHitCount(WhsDocketSchema.Constants.TableName);
			var dbHitCountDocketLine = otherFactory.GetTableHitCount(WhsDocketLineSchema.Constants.TableName);
			var inventoryReloaded = otherFactory.Load<WhsInventoryView>(inventory.PK);

			AssertEquals("Precondition: Committed inventory.", 10m, inventoryReloaded.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("DB hits for WhsDocket.", dbHitCountDocket + 1, otherFactory.GetTableHitCount(WhsDocketSchema.Constants.TableName));
			AssertEquals("DB hits for WhsDocketLine.", dbHitCountDocketLine + 1, otherFactory.GetTableHitCount(WhsDocketLineSchema.Constants.TableName));
		}

		#endregion

		#region Business Object Overrides

		#region TestLightValidatonDisabled

		public void TestLightValidatonDisabled()
		{
			AssertEquals(false, PickLine.LightValidationEnabled);
		}

		#endregion

		#region TestSaving

		#region TestOverCommitCheckOnPickLine

		[UseSnapshotProtection]
		public void TestOverCommitCheckOnPickLine()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var data = new TestDataSimpleEnvironment(factory);
				var helper = new WhsTestHelperFunctions(factory);
				helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
				factory.Save();

				var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var order2 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				var order3 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
				var order4 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O4");
				var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 25m);
				var orderLine2 = helper.CreateWhsOrderLine(order2, data.Part1, 25m);
				var orderLine3 = helper.CreateWhsOrderLine(order3, data.Part1, 25m);
				var orderLine4 = helper.CreateWhsOrderLine(order4, data.Part1, 25m);
				var pick = helper.CreatePickNew(order1, order2, order3, order4);

				BusinessObjectFactory.SavingEventHandler handler = f =>
				{
					f.ServiceContainer.AddAfterOnSavingService(new ServiceThatThrowsException(f));
				};
				factory.Saving += handler;

				foreach (var pickLine in pick.GetAllPickLines())
				{
					pickLine.WZ_Units--; // to cause pick lines to have changes
				}

				// pre-condition
				Helper.AssertZCannotSaveExceptionThrown("Test", factory.Save);
				factory.Saving -= handler;

				foreach (var pickLine in pick.GetAllPickLines())
				{
					pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				}

				AssertNoExceptionThrown(factory.Save); // should not fail when saving this transaction
			}
		}

		// throw exception before committing transaction
		class ServiceThatThrowsException : IAfterOnSavingBOProcessingService
		{
			public ServiceThatThrowsException(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			readonly BusinessObjectFactory Factory;

			void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(
				IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				Factory.ServiceContainer.RemoveAfterOnSavingService<ServiceThatThrowsException>();
				throw new ZCannotSaveException("Test", "Test", ExceptionType.BusinessFailure);
			}
		}

		#endregion

		#region TestReservedQtyRemovedOnSavingWhenPicked

		public void TestReservedQtyRemovedOnSavingWhenPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", true, reservedPickLine.IsReserveLine);

			Helper.CreatePickNew(order);
			reservedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.WZ_OriginalReservedQty);
			AssertEquals("Precondition: Stock is reserved.", true, reservedPickLine.IsReserveLine);
			AssertNoErrors("Should not have validation errors", reservedPickLine.WZ_OriginalReservedQtyInfo);

			Factory.Save();
			AssertEquals("Pick Line should no longer be reserved.", 0m, reservedPickLine.WZ_OriginalReservedQty);
			AssertEquals("Pick Line should no longer be reserved.", false, reservedPickLine.IsReserveLine);
			AssertNoErrors("Should not have validation errors", reservedPickLine.WZ_OriginalReservedQtyInfo);
		}

		#endregion

		#region TestCreateDockDoorTransfers

		public void TestCreateDockDoorTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateGlbStaff("AAA", "AAA");
			Helper.CreateGlbStaff("ZZZ", "ZZZ");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-1");
			var receive2 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, location2, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 3m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_F3_NKPackType = "BOX";
			orderLine2.WE_F3_NKPackType = "KEG";
			orderLine3.WE_F3_NKPackType = "CAS";
			var pick = Helper.CreatePickNew(order);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 4m, location1, location2);
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer Line is Committed.", 4m,
				transferLine.QtyCommittedIncludingMatchingLines);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, -5m, location2);
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("Precondition: Adjustment Line is Committed.", 5m, adjustmentLine.CommittedQuantity);

			Factory.Save();

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			var orderPickLine1 = orderLine1.PickLines.Single();
			var orderPickLine2 = orderLine2.PickLines.Single();
			orderPickLine1.WZ_GS_NKAssignedTo = "AAA";
			orderPickLine2.WZ_GS_NKAssignedTo = "ZZZ";
			var transferLinePickLine = transferLine.PickLines.Single();
			var adjustmentPickLine = adjustmentLine.PickLines.Single();
			transferLine.PickedTime = now;
			adjustment.FinaliseDocketWithoutUserConfirmation();
			orderPickLine1.WZ_PickedDateTime = now.AddDays(-1);
			orderPickLine2.WZ_PickedDateTime = now.AddDays(-2);
			AssertEquals("Precondition: TransferLine is Picked.", true, transferLine.IsPicked);
			AssertIsFinalisedPrecondition(adjustment);
			AssertNoExceptionThrown(() => Factory.Save());

			var query = new ZQuery();
			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, transfer.PK);
			var newTransfers = Factory.Load<WhsTransfer>(query);
			AssertEquals("Should only create one new Transfer.", 1, newTransfers.Length);

			var inTransitTransfer = newTransfers.Single();
			AssertTransfer(inTransitTransfer, pick, data.Org1, data.Whs1);
			AssertEquals("Only two In-Transit Transfer Lines should have been created.", 2,
				inTransitTransfer.Lines.Count);

			var newTransferLine1 =
				inTransitTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK);
			var newTransferLine2 =
				inTransitTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part2.PK);
			AssertTransferLine(newTransferLine1, orderLine1, data.Part1, 2m, "BOX", pick.DockDoorLocation);
			AssertTransferLine(newTransferLine2, orderLine2, data.Part2, 3m, "KEG", pick.DockDoorLocation);
			AssertEquals("In-Transit Transfer Line Original Docket Line for Rating.",
				receive1.Lines[0].WE_WE_OriginalDocketLineForRating,
				newTransferLine1.WE_WE_OriginalDocketLineForRating);
			AssertEquals("In-Transit Transfer Line Original Docket Line for Rating.",
				receive2.Lines[0].WE_WE_OriginalDocketLineForRating,
				newTransferLine2.WE_WE_OriginalDocketLineForRating);
			AssertEquals("In-Transit Transfer Line Transfer From Pallet ID.", "PLT-1",
				newTransferLine1.WE_TransferFromPalletId);
			AssertEquals("In-Transit Transfer Line Transfer From Pallet ID.", "",
				newTransferLine2.WE_TransferFromPalletId);
			AssertEquals("In-Transit Transfer Line Source Location.", location1.PK,
				newTransferLine1.WE_WL_TransferFrom);
			AssertEquals("In-Transit Transfer Line Source Location.", location2.PK,
				newTransferLine2.WE_WL_TransferFrom);
			AssertEquals("In-Transit Transfer Line should have correct Picked Time.", now.AddDays(-1),
				newTransferLine1.PickedTime);
			AssertEquals("In-Transit Transfer Line should have correct Picked Time.", now.AddDays(-2),
				newTransferLine2.PickedTime);
			AssertEquals("In-Transit Transfer Line should have correct Picked By.", "AAA",
				newTransferLine1.GS_NKPickedBy);
			AssertEquals("In-Transit Transfer Line should have correct Picked By.", "ZZZ",
				newTransferLine2.GS_NKPickedBy);
			AssertEquals("In-Transit Transfer Line Putaway By.", "AAA", newTransferLine1.WE_GS_NKPutawayBy);
			AssertEquals("In-Transit Transfer Line Putaway By.", "ZZZ", newTransferLine2.WE_GS_NKPutawayBy);
			AssertEquals("Picker should no longer be assigned.", "", orderPickLine1.WZ_GS_NKAssignedTo);
			AssertEquals("Picker should no longer be assigned.", "", orderPickLine2.WZ_GS_NKAssignedTo);
			AssertEquals("PickLine should have no Picked Time set.", ZDateTimeOffset.Empty, orderPickLine1.WZ_PickedDateTime);
			AssertEquals("PickLine should have no Picked Time set.", ZDateTimeOffset.Empty, orderPickLine2.WZ_PickedDateTime);
			AssertEquals("Units should be unchanged.", 2m, orderPickLine1.WZ_Units);
			AssertEquals("Units should be unchanged.", 3m, orderPickLine2.WZ_Units);
			AssertEquals("PickLine should now be committing Transfer Line.", newTransferLine1.PK,
				orderPickLine1.WZ_WE_InventoryLine);
			AssertEquals("PickLine should now be committing Transfer Line.", newTransferLine2.PK,
				orderPickLine2.WZ_WE_InventoryLine);
			AssertEquals("PickLine should be pointing to the Original Inventory via WZ_WE_OriginalPickedInventoryLine.",
				receive1.Lines[0].PK, orderPickLine1.WZ_WE_OriginalPickedInventoryLine);
			AssertEquals("PickLine should be pointing to the Original Inventory via WZ_WE_OriginalPickedInventoryLine.",
				receive2.Lines[0].PK, orderPickLine2.WZ_WE_OriginalPickedInventoryLine);
		}

		public void TestCreateDockDoorTransfers_InMemory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order Line is not allocated.", 0, orderLine.PickLines.Count);

			Factory.Save();
			AssertEquals("Precondition: No Transfers should be created.", 0, pick.Transfers.Count);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var inTransitTransfer = pick.Transfers.Single();
			AssertTransfer(inTransitTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine = inTransitTransfer.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation);
		}

		public void TestCreateDockDoorTransfers_DifferentDockDoor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WLT_LocationType =
				Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC")).PK;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_WL_DockDoor = location2.PK;
			AssertEquals("Precondition: Order Line is not allocated.", 0, orderLine.PickLines.Count);

			Factory.Save();
			AssertEquals("Precondition: No Transfers should be created.", 0, pick.Transfers.Count);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var inTransitTransfer = pick.Transfers.Single();
			AssertTransfer(inTransitTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine = inTransitTransfer.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine, orderLine, data.Part1, 10m, "UNT", location2);
		}

		public void TestCreateDockDoorTransfers_DoesNotCopyCustomAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Helper.SetDocketLineCustomAttributes(receive.Lines[0], "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1.1m, 2.2m,
				3.3m, 4.4m, 5.5m, new ZDateTime(2017, 1, 1), new ZDateTime(2017, 1, 2), new ZDateTime(2017, 1, 3),
				new ZDateTime(2017, 1, 4), new ZDateTime(2017, 1, 5), true, true, true, true, true, "BLOB");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order Line is not allocated.", 0, orderLine.PickLines.Count);

			Factory.Save();
			AssertEquals("Precondition: No Transfers should be created.", 0, pick.Transfers.Count);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var inTransitTransfer = pick.Transfers.Single();
			AssertTransfer(inTransitTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine = inTransitTransfer.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation);
			Helper.AssertDocketLineCustomAttributes(newTransferLine, "", "", "", "", "", "", 0m, 0m, 0m, 0m, 0m,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false,
				false, false, false, "");
		}

		public void TestCreateDockDoorTransfers_ClearsAvailableInventoryCache()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1),
				data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1),
				data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory =
				(WhsPickAvailableInventory)pick.OrderedInventories[0].AvailableInventories.Single();
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory.PickLines.Count());

			availableInventory.PickLineQuantity = 10m;
			var pickLine = orderLine.PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.PickLines);
			AssertEquals("Should have 1 Inventory Split.", 1,
				availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine },
				availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine },
				availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("In-Transit Transfer should be created.", 1, pick.Transfers.Count);

			var newPickLine = pick.Transfers.Single().Lines.Single().PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { newPickLine },
				availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine },
				availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);
		}

		public void TestCreateDockDoorTransfers_ClearsAvailableInventoryCache_UOM()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1),
				data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1),
				data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory =
				(WhsPickAvailableInventory)pick.OrderedInventories[0].AvailableInventories.Single();
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory.PickLines.Count());

			availableInventory.PickLineQuantity = 10m;
			var pickLine = orderLine.PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.PickLines);
			AssertEquals("Should have 1 Inventory Split.", 1, availableInventory.AvailableInventoriesSplitByUOM.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine },
				availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine },
				availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesOnOrder);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("In-Transit Transfer should be created.", 1, pick.Transfers.Count);

			var newPickLine = pick.Transfers.Single().Lines.Single().PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { newPickLine },
				availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine },
				availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesOnOrder);
		}

		public void TestCreateDockDoorTransfers_PickingInTransitTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order Line is not allocated.", 0, orderLine.PickLines.Count);

			Factory.Save();
			AssertEquals("Precondition: No Transfers should be created.", 0, pick.Transfers.Count);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);
			AssertEquals("Precondition: PickLine does not have Pick Time.", ZDateTimeOffset.Empty,
				pickLine.WZ_PickedDateTime);

			var inTransitTransfer = pick.Transfers.Single();
			var inTransitTransferLine1 = (WhsTransferLine)inTransitTransfer.Lines.Single();
			AssertTransfer(inTransitTransfer, pick, data.Org1, data.Whs1);
			AssertTransferLine(inTransitTransferLine1, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation);

			// HACK: Set interim location (as we wouldn't transfer from DDL to DDL)
			inTransitTransferLine1.WE_WL = data.Whs1.DefaultLocation.PK;

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertTransfer(inTransitTransfer, pick, data.Org1, data.Whs1);

			var inTransitTransferLine2 =
				inTransitTransfer.Lines.Cast<WhsTransferLine>().Single(l => l != inTransitTransferLine1);
			AssertTransferLine(inTransitTransferLine2, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation);
			AssertEquals("PickLine should be committing new Transfer Line.", inTransitTransferLine2.PK,
				pickLine.WZ_WE_InventoryLine);
			AssertEquals("Originally Picked Inventory should be correct.", receive.Lines[0].PK,
				pickLine.WZ_WE_OriginalPickedInventoryLine);

			var newPickLine = inTransitTransferLine2.PickLines.Single();
			AssertEquals("Originally Picked Inventory should not be set on Transfer Line's PickLine.", ZGuid.Empty,
				newPickLine.WZ_WE_OriginalPickedInventoryLine);
		}

		public void TestCreateDockDoorTransfers_TransferFinalised()
		{
			// Ensure we don't attempt to use a finalised Dock Door transfer
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);

			var now = ZDateTimeOffset.Now;
			var orderPickLine1 = orderLine1.PickLines.Single();
			orderPickLine1.WZ_PickedDateTime = now;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var firstTransfer = pick.Transfers.Single();
			AssertTransfer(firstTransfer, pick, data.Org1, data.Whs1);
			firstTransfer
				.FinaliseDocketWithoutUserConfirmation(); // Dont think this should be possible? Theoretically may happen if pick cancellation fails.
			AssertIsFinalisedPrecondition(firstTransfer);

			var orderPickLine2 = orderLine2.PickLines.Single();
			orderPickLine2.WZ_PickedDateTime = now;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create a second Transfer.", 2, pick.Transfers.Count);
			AssertTransfer(pick.Transfers.Single(t => t.PK != firstTransfer.PK), pick, data.Org1, data.Whs1);
		}

		public void TestCreateDockDoorTransfers_MultipleClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var secondClient = Helper.CreateClient("SECOND");
			data.Part1.RelatedOrganisations.AddOwner(secondClient);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			var receive2 =
				Helper.CreateWhsReceiveWithInventory(secondClient, data.Whs1, "R2", data.Part1, 10m, location2, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var order2 = Helper.CreateWhsOrder(secondClient, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			var pick = Helper.CreatePickNew(order1, order2);

			var now = ZDateTimeOffset.Now;
			var orderPickLine1 = orderLine1.PickLines.Single();
			var orderPickLine2 = orderLine2.PickLines.Single();
			orderPickLine1.WZ_PickedDateTime = now;
			orderPickLine2.WZ_PickedDateTime = now;

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create two new Transfers.", 2, pick.Transfers.Count);

			var transfer1 = pick.Transfers.Single(t => t.WD_OH_Client == data.Org1.PK);
			var transfer2 = pick.Transfers.Single(t => t.WD_OH_Client == secondClient.PK);
			AssertTransfer(transfer1, pick, data.Org1, data.Whs1);
			AssertTransfer(transfer2, pick, secondClient, data.Whs1);

			var newTransferLine1 = transfer1.Lines.Cast<WhsTransferLine>().Single();
			var newTransferLine2 = transfer2.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine1, orderLine1, data.Part1, 2m, "UNT", pick.DockDoorLocation);
			AssertTransferLine(newTransferLine2, orderLine2, data.Part1, 3m, "UNT", pick.DockDoorLocation);
		}

		[TestDate(2019, 1, 1)]
		public void TestCreateDockDoorTransfers_PartAttributes()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, new ZDate(year, 1, 2),
				new ZDate(year, 1, 3), "PA1", "PA2", "PA3", "BEK");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year - 1, 12, 31);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "", "ABC");
			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one Transfer.", 1, pick.Transfers.Count);

			var transfer = pick.Transfers.Single();
			AssertTransfer(transfer, pick, data.Org1, data.Whs1);

			var transferLine = transfer.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(transferLine, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation);
			AssertEquals("In-Transit Transfer Line Arrival Date.", new ZDateTimeOffset(year - 1, 12, 31),
				transferLine.WE_AdjustmentArrivalDate);
			AssertEquals("In-Transit Transfer Line Bonded Entry Key.", "BEK", transferLine.WE_BondedEntryKey);
			AssertEquals("In-Transit Transfer Line Expiry Date.", new ZDate(year, 1, 2), transferLine.WE_ExpiryDate);
			AssertEquals("In-Transit Transfer Line Packing Date.", new ZDate(year, 1, 3), transferLine.WE_PackingDate);
			AssertEquals("In-Transit Transfer Line Part Attrib 1.", "PA1", transferLine.WE_PartAttrib1);
			AssertEquals("In-Transit Transfer Line Part Attrib 2.", "PA2", transferLine.WE_PartAttrib2);
			AssertEquals("In-Transit Transfer Line Part Attrib 3.", "PA3", transferLine.WE_PartAttrib3);
		}

		public void TestCreateDockDoorTransfers_PerPackageQtyAndPackageGroupID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location.PK, "BEK-1", "PACK1", 2m);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 15m, location.PK, "BEK-2", "PACK1", 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order Line is fully Allocated.", 10m, orderLine1.PickLineQuantity);
			AssertEquals("Precondition: Order Line is fully Allocated.", 15m, orderLine2.PickLineQuantity);

			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			// split off 2 packs
			pickLine1.Split(4m);
			pickLine2.Split(6m);
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one Transfer.", 1, pick.Transfers.Count);

			var transfer = pick.Transfers.Single();
			AssertTransfer(transfer, pick, data.Org1, data.Whs1);

			var transferLine1 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK);
			var transferLine2 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part2.PK);
			AssertTransferLine(transferLine1, orderLine1, data.Part1, 6m, "UNT", pick.DockDoorLocation);
			AssertTransferLine(transferLine2, orderLine2, data.Part2, 9m, "UNT", pick.DockDoorLocation);
			AssertEquals("In-Transit Transfer Line Per Package Qty.", 2m, transferLine1.WE_PerPackageQty);
			AssertEquals("In-Transit Transfer Line Per Package Qty.", 3m, transferLine2.WE_PerPackageQty);
			AssertEquals("In-Transit Transfer Line Package Group ID.", "PACK1", transferLine1.WE_PackageGroupId);
			AssertEquals("In-Transit Transfer Line Package Group ID.", "PACK1", transferLine2.WE_PackageGroupId);
		}

		public void TestCreateDockDoorTransfers_PreventsSaveIfErrors_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			order.WD_WW_Whs = ZGuid.Invalid; // Will cause transfer created to be incorrect

			var now = ZDateTimeOffset.Now;
			var orderPickLine1 = orderLine1.PickLines.Single();
			orderPickLine1.WZ_PickedDateTime = now.AddDays(-1);
			Helper.AssertZCannotSaveExceptionThrown("Cannot save as Dock Door Transfers are in an invalid state: Error - WI_WW_Whs: Enter a valid Warehouse.\nError - LocationString: This Location does not belong to the Transfer's Warehouse.", Factory.Save);
		}

		public void TestCreateDockDoorTransfers_PreventsSaveIfErrors_Line()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var orderPickLine1 = orderLine1.PickLines.Single();
			orderPickLine1.WZ_GS_NKAssignedTo = "AAA"; // Will create invalid transfer line
			orderPickLine1.WZ_PickedDateTime = now.AddDays(-1);
			Helper.AssertZCannotSaveExceptionThrown("Cannot save as Dock Door Transfers are in an invalid state: Error - GS_NKPickedBy: Enter a valid GS_NKPickedBy.\nError - WE_GS_NKPutawayBy: Enter a valid Putaway By.", Factory.Save);
		}

		public void TestCreateDockDoorTransfers_PreventsSaveIfErrors_LineAddedAfterTransferSaved()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var orderPickLine1 = orderLine1.PickLines.Single();
			orderPickLine1.WZ_PickedDateTime = now.AddDays(-1);
			AssertNoExceptionThrown(Factory.Save);

			var orderPickLine2 = orderLine2.PickLines.Single();
			orderPickLine2.WZ_GS_NKAssignedTo = "AAA"; // Will create invalid transfer line
			orderPickLine2.WZ_PickedDateTime = now.AddDays(-1);
			Helper.AssertZCannotSaveExceptionThrown("Cannot save as Dock Door Transfers are in an invalid state: Error - GS_NKPickedBy: Enter a valid GS_NKPickedBy.\nError - WE_GS_NKPutawayBy: Enter a valid Putaway By.", Factory.Save);
		}

		public void TestCreateDockDoorTransfers_WorkOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("COL");
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var secondBomProduct = Helper.CreateProduct("BOM2", data.Org1);
			var thirdBomProduct = Helper.CreateProduct("BOM3", data.Org1);
			Helper.CreateProductBOM(secondBomProduct, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(thirdBomProduct, data.Part1, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 12m);

			var areaWithNoLocations = Helper.CreateArea(data.Whs1, "FOO");
			var stagingArea1 = Helper.CreateArea(data.Whs1, "STAGING1");
			var stagingArea2 = Helper.CreateArea(data.Whs1, "STAGING2");
			var stagingAreaInOtherWarehouse = Helper.CreateArea(warehouse2, "STAGING");
			var stagingRow1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING1");
			var stagingRow2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING2");
			var stagingRowInOtherWarehouse = Helper.CreateRowAndGenerateLocations(warehouse2, "STAGING");
			var stagingLocation1 = stagingRow1.Locations.Single();
			var stagingLocation2 = stagingRow2.Locations.Single();
			var stagingLocationInOtherWarehouse = stagingRowInOtherWarehouse.Locations.Single();
			stagingLocation1.WLV_WA_PickingArea = stagingArea1.PK;
			stagingLocation2.WLV_WA_PickingArea = stagingArea2.PK;
			stagingLocationInOtherWarehouse.WLV_WA_PickingArea = stagingAreaInOtherWarehouse.PK;

			var paramsWithNoLocations = WhsProduct.GetWhsProduct(secondBomProduct).ParamsByWhsAndClient.AddNew();
			paramsWithNoLocations.W3_OH = data.Org1.PK;
			paramsWithNoLocations.W3_WW = data.Whs1.PK;

			var paramsWithNoArea = WhsProduct.GetWhsProduct(thirdBomProduct).ParamsByWhsAndClient.AddNew();
			paramsWithNoArea.W3_OH = data.Org1.PK;
			paramsWithNoArea.W3_WW = data.Whs1.PK;

			var paramsWithDifferentClient = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsWithDifferentClient.W3_OH = Helper.CreateClient("BAR").PK;
			paramsWithDifferentClient.W3_WW = data.Whs1.PK;
			paramsWithDifferentClient.W3_WL_StagingLocationBOM = stagingLocation1.PK;

			var paramsWithDifferentWarehouse = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsWithDifferentWarehouse.W3_OH = data.Org1.PK;
			paramsWithDifferentWarehouse.W3_WW = warehouse2.PK;
			paramsWithDifferentWarehouse.W3_WL_StagingLocationBOM = stagingLocationInOtherWarehouse.PK;

			var correctParams = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			correctParams.W3_OH = data.Org1.PK;
			correctParams.W3_WW = data.Whs1.PK;
			correctParams.W3_WL_StagingLocationBOM = stagingLocation2.PK;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, secondBomProduct, 2m);
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, thirdBomProduct, 3m);
			var pick = Helper.CreatePickNew(workOrder);

			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			AssertTransfer(newTransfer, pick, data.Org1, data.Whs1);
			AssertEquals("Should only create 3 Transfer Lines.", 3, newTransfer.Lines.Count);

			var newTransferLine1 =
				newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 2m);
			var newTransferLine2 =
				newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 4m);
			var newTransferLine3 =
				newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 6m);

			// Product had a Staging Location set, it should use the one with the same Warehouse and Client.
			AssertTransferLine(newTransferLine1, workOrderLine1.ChildComponentLines.Single(), data.Part1, 2m, "UNT", stagingLocation2);
			// Product had a Staging Area set with no Locations, Destination Location should be Default Location.
			AssertTransferLine(newTransferLine2, workOrderLine2.ChildComponentLines.Single(), data.Part1, 4m, "UNT", data.Whs1.DefaultLocation);
			// Product had no Staging Area, Destination Location should be Default Location.
			AssertTransferLine(newTransferLine3, workOrderLine3.ChildComponentLines.Single(), data.Part1, 6m, "UNT", data.Whs1.DefaultLocation);
		}

		public void TestCreateDockDoorTransfers_WorkOrders_Disassembly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);

			var stagingArea1 = Helper.CreateArea(data.Whs1, "STAGING1");
			var stagingArea2 = Helper.CreateArea(data.Whs1, "STAGING2");
			var stagingRow1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING1");
			var stagingRow2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING2");
			var stagingLocation1 = stagingRow1.Locations.Single();
			var stagingLocation2 = stagingRow2.Locations.Single();
			stagingLocation1.WLV_WA_PickingArea = stagingArea1.PK;
			stagingLocation2.WLV_WA_PickingArea = stagingArea2.PK;

			var paramsOnPart1 = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			paramsOnPart1.W3_OH = data.Org1.PK;
			paramsOnPart1.W3_WW = data.Whs1.PK;
			paramsOnPart1.W3_WL_StagingLocationBOM = stagingLocation1.PK;

			var paramsOnPart2 = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsOnPart2.W3_OH = data.Org1.PK;
			paramsOnPart2.W3_WW = data.Whs1.PK;
			paramsOnPart2.W3_WL_StagingLocationBOM = stagingLocation2.PK;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 2m);

			var pick = Helper.CreatePickNew(workOrder);
			pick.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			AssertTransfer(newTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine = newTransfer.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine, workOrderLine, data.Part2, 2m, "UNT", stagingLocation2);
		}

		public void TestCreateDockDoorTransfers_InTransitTransfersAreNotCreatedWhenPickLinesForOrdersArePickedOnPickFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);

			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition: Pick Line is picked.", true, orderLine.PickLines.Single().IsPicked);

			Factory.Save();
			AssertEquals("Should not have created any new Transfers.", 0, pick.Transfers.Count);
		}

		public void TestCreateDockDoorTransfers_PalletIsFullyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation,
				"PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Now;

			Factory.Save();
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			AssertTransfer(newTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine = newTransfer.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation, "PLT1");
		}

		public void TestCreateDockDoorTransfers_PalletIsFullyPicked_MultipleLocationsAndPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locationA2, "PLT2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);

			Factory.Save();
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			AssertTransfer(newTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine = newTransfer.Lines.Cast<WhsTransferLine>()
				.Single(l => l.WE_WL_TransferFrom == locationA1.PK);
			AssertTransferLine(newTransferLine, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation, "PLT1");

			var newTransferLine2 = newTransfer.Lines.Cast<WhsTransferLine>()
				.Single(l => l.WE_WL_TransferFrom == locationA2.PK);
			AssertTransferLine(newTransferLine2, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation, "PLT2");
		}

		public void TestCreateDockDoorTransfers_PalletIsFullyPicked_MultiplePickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.DefaultLocation,
				"PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m, data.Whs1.DefaultLocation,
				"PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 11m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);

			Factory.Save();
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			AssertTransfer(newTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine1 =
				newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 5m);
			var newTransferLine2 =
				newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 6m);
			AssertTransferLine(newTransferLine1, orderLine, data.Part1, 5m, "UNT", pick.DockDoorLocation, "PLT1");
			AssertTransferLine(newTransferLine2, orderLine, data.Part1, 6m, "UNT", pick.DockDoorLocation, "PLT1");
		}

		public void TestCreateDockDoorTransfers_PalletIsFullyPicked_PickLineIsPickedInAnotherFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation,
				"PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickLineInNewFactory = newFactory.Load<WhsPickLine>(pickLine.PK);
			pickLineInNewFactory.WZ_PickedDateTime = ZDateTimeOffset.Now;
			newFactory.Save();

			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Should create one new Transfer.", 1, pickInNewFactory.Transfers.Count);

			var newTransfer = pickInNewFactory.Transfers.Single();
			AssertTransfer(newTransfer, pickInNewFactory, data.Org1, data.Whs1);

			var newTransferLine = newTransfer.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine, orderLine, data.Part1, 10m, "UNT", pickInNewFactory.DockDoorLocation, "PLT1");
		}

		public void TestCreateDockDoorTransfers_PalletIsFullyPicked_DifferentWarehouseHasSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "Z", 1, 1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation,
				"PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 10m, whs2.DefaultLocation, "PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order);

			var order2 = Helper.CreateWhsOrder(data.Org1, whs2, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Now;

			Factory.Save();

			AssertEquals("Should not create transfer", 0, pick1.Transfers.Count);
			var query = new ZQuery(WhsInventoryViewSchema.WI_WL, data.Whs1.DefaultLocation.PK);
			query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, "PLT1");
			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
			AssertNotNull("Pallet 'PLT1' existed in Whs1", Factory.LoadTop1<WhsInventoryView>(query));

			var newTransferForPick2 = pick2.Transfers.Single();
			AssertTransfer(newTransferForPick2, pick2, data.Org1, whs2);

			var newTransferLine = newTransferForPick2.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine, orderLine2, data.Part1, 10m, "UNT", pick2.DockDoorLocation, "PLT1");
		}

		public void TestCreateDockDoorTransfers_PalletIsFullyPicked_MultiplePicks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation,
				"PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, data.Whs1.DefaultLocation,
				"PLT2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			pick1.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Now;

			Factory.Save();

			AssertEquals("Should create one new Transfer.", 1, pick1.Transfers.Count);
			AssertEquals("Should create one new Transfer.", 1, pick2.Transfers.Count);

			var newTransferForPick1 = pick1.Transfers.Single();
			AssertTransfer(newTransferForPick1, pick1, data.Org1, data.Whs1);
			var newTransferLine1 = newTransferForPick1.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine1, orderLine, data.Part1, 10m, "UNT", pick1.DockDoorLocation, "PLT1");

			var newTransferForPick2 = pick2.Transfers.Single();
			AssertTransfer(newTransferForPick2, pick2, data.Org1, data.Whs1);
			var newTransferLine2 = newTransferForPick2.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine2, orderLine2, data.Part2, 10m, "UNT", pick2.DockDoorLocation, "PLT2");
		}

		public void TestCreateDockDoorTransfers_PalletIsFullyPicked_HasSaveException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.DefaultLocation,
				"PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m, data.Whs1.DefaultLocation,
				"PLT2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 11m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(pl => pl.WZ_Units == 5m);
			var pickLine2 = pick.GetAllPickLines().Single(pl => pl.WZ_Units == 6m);

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			BusinessObjectFactory.SavingEventHandler handler = f =>
			{
				f.ServiceContainer.AddAfterOnSavingService(new ServiceThatThrowsException(f));
			};
			Factory.Saving += handler;

			Helper.AssertZCannotSaveExceptionThrown("Test", Factory.Save);

			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			AssertEquals("Transfer should not be saved into DB when met Exception", false, newTransfer.IsInDatabase);

			var newTransferLine1 = newTransfer.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine1, orderLine, data.Part1, 5m, "UNT", pick.DockDoorLocation, "PLT1");

			Factory.Saving -= handler;

			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());
			AssertEquals("Should not create another new Transfer.", 1, pick.Transfers.Count);
			AssertEquals("Transfer should be saved into DB", true, newTransfer.IsInDatabase);

			newTransferLine1.Reload();
			AssertEquals("PalletID in transfer Line should not be changed", "PLT1", newTransferLine1.WE_PalletID);

			var newTransferLine2 = newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.PK != newTransferLine1.PK);
			AssertTransferLine(newTransferLine2, orderLine, data.Part1, 6m, "UNT", pick.DockDoorLocation, "PLT2");
		}

		public void TestCreateDockDoorTransfers_PalletNotFullyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, data.Whs1.DefaultLocation,
				"PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.DefaultLocation,
				"PLT2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories
				.Cast<WhsPickAvailableInventory>().Single(a => a.QuantityAvailableToPick == 1m);
			var availableInventory2 = pick.OrderedInventories[0].AvailableInventories
				.Cast<WhsPickAvailableInventory>().Single(a => a.QuantityAvailableToPick == 10m);
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory1.PickLines.Count());
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory2.PickLines.Count());

			availableInventory1.PickLineQuantity = 1m;
			availableInventory2.PickLineQuantity = 9m;
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);

			Factory.Save();
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			AssertTransfer(newTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine1 =
				newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 1m);
			var newTransferLine2 =
				newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 9m);
			AssertTransferLine(newTransferLine1, orderLine, data.Part1, 1m, "UNT", pick.DockDoorLocation, "PLT1");
			AssertTransferLine(newTransferLine2, orderLine, data.Part1, 9m, "UNT", pick.DockDoorLocation, "");
		}

		public void TestCreateDockDoorTransfers_PalletNotFullyPicked_MultipleProductsInSamePallet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation,
				"PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, data.Whs1.DefaultLocation,
				"PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			orderLine2.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			AssertTransfer(newTransfer, pick, data.Org1, data.Whs1);
			var newTransferLine = newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK);
			AssertTransferLine(newTransferLine, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation, "");

			var newTransferLine2 = newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part2.PK);
			AssertTransferLine(newTransferLine2, orderLine2, data.Part2, 10m, "UNT", pick.DockDoorLocation, "PLT1");
		}

		public void TestCreateDockDoorTransfers_PalletNotFullyPicked_PalletIDSensitive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, data.Whs1.DefaultLocation,
				"PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m, data.Whs1.DefaultLocation,
				"plt1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().Single(pl => pl.WZ_Units == 4m).WZ_PickedDateTime = ZDateTimeOffset.Now;

			Factory.Save();

			pick.GetAllPickLines().Single(pl => pl.WZ_Units == 6m).WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			AssertTransfer(newTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine = newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 4m);
			AssertTransferLine(newTransferLine, orderLine, data.Part1, 4m, "UNT", pick.DockDoorLocation, "");

			var newTransferLine2 =
				newTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 6m);
			AssertTransferLine(newTransferLine2, orderLine, data.Part1, 6m, "UNT", pick.DockDoorLocation, "plt1");
		}

		public void TestCreateDockDoorTransfers_PalletNotFullyPicked_MultipleProductsInSamePallet_AnotherPickLineIsPickedInDifferentFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation,
				"PLT1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, data.Whs1.DefaultLocation,
				"PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order, order2);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			orderLine2InNewFactory.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			newFactory.Save();

			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Should create one new Transfer.", 1, pickInNewFactory.Transfers.Count);

			var newTransfer = pickInNewFactory.Transfers.Single();
			AssertTransfer(newTransfer, pick, data.Org1, data.Whs1);

			var newTransferLine1 = newTransfer.Lines.Cast<WhsTransferLine>().Single();
			AssertTransferLine(newTransferLine1, orderLine2, data.Part2, 5m, "UNT", pick.DockDoorLocation, "");

			AssertEquals("Should not create another Transfer.", 1, pick.Transfers.Count);
			var newTransfer2 = pick.Transfers.Single();
			AssertEquals("Should load existing Transfer in DB", newTransfer.PK, newTransfer2.PK);
			AssertTransfer(newTransfer2, pick, data.Org1, data.Whs1);

			AssertEquals("Should create a new transfer line", 2, newTransfer2.Lines.Count);
			var newTransferLine2 =
				newTransfer2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 10m);
			AssertTransferLine(newTransferLine2, orderLine, data.Part1, 10m, "UNT", pick.DockDoorLocation, "");
		}

		void AssertTransfer(WhsTransfer transfer, WhsPick pick, OrgHeader client, WhsWarehouse warehouse)
		{
			AssertNotEquals("In Transit Transfer Docket ID.", "", transfer.WD_DocketID);
			AssertEquals("In Transit Transfer Client.", client.PK, transfer.WD_OH_Client);
			AssertEquals("In Transit Transfer Sub Type.", TransferType.Codes.Internal, transfer.WD_DocketSubType);
			AssertEquals("In Transit Transfer Docket Status.", DocketStatus.Codes.Entered, transfer.WD_DocketStatus);
			AssertEquals("In Transit Transfer External Reference.", pick.WP_PickNo + " " + transfer.WD_DocketID,
				transfer.WD_ExternalReference);
			AssertEquals("In Transit Transfer Parent Pick.", pick.PK, transfer.WD_WP_ParentPickForTransfer);
			AssertEquals("In Transit Transfer Warehouse.", warehouse.PK, transfer.WD_WW_Whs);
		}

		void AssertTransferLine(WhsTransferLine transferLine, WhsPickableDocketLine originalOrderLine, OrgSupplierPart part, ZDecimal expectedQty,
			ZString packtype, WhsLocation expectedDestinationLocation, string palletID = "")
		{
			AssertEquals("In-Transit Transfer Line Part.", part.PK, transferLine.WE_OP);
			AssertEquals("In-Transit Transfer Line Inventory Status.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			AssertEquals("In-Transit Transfer Line Original Inventory Status.", InventoryStatus.Codes.InTransit,
				transferLine.WE_OriginalInventoryStatus);
			AssertEquals("In-Transit Transfer Line Status.", DocketLineStatus.Codes.HeldForTransfer,
				transferLine.WE_DocketLineStatus);
			AssertEquals("In-Transit Transfer Line Is Original Inventory.", true, transferLine.WE_IsOriginalInventory);
			AssertEquals("In-Transit Transfer Line Stock on Hand.", expectedQty, transferLine.WE_StockOnHand);
			AssertEquals("In-Transit Transfer Line Transaction Qty.", expectedQty, transferLine.WE_TransactionQuantity);
			AssertEquals("In-Transit Transfer Line Pack Type.", packtype, transferLine.WE_F3_NKPackType);
			AssertEquals("In-Transit Transfer Line Hold Code.", "", transferLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("In-Transit Transfer Line Original Hold Code.", "",
				transferLine.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("In-Transit Transfer Line Destination Location.", expectedDestinationLocation.PK,
				transferLine.WE_WL);
			AssertEquals("In-Transit Transfer Line Pallet ID in Destination Location", palletID,
				transferLine.WE_PalletID);
			AssertEquals("Pick Line for Transfer should store Original Order Line FK.", originalOrderLine.PK, transferLine.PickLines.Single().WZ_WE_OriginalOrderLine);
		}

		#endregion

		#endregion

		#region TestSupportsNotes

		public void TestSupportsNotes()
		{
			AssertEquals("Pick Lines should not support notes.", false, Factory.New<WhsPickLine>().SupportsNotes);
		}

		#endregion

		#region TestSupportsClone

		public void TestSupportsClone()
		{
			AssertEquals("Pick Lines should support cloning.", true, Factory.New<WhsPickLine>().SupportsClone());
		}

		#endregion

		#region TestClone

		public void TestClone()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "XYZ";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = new ZDateTimeOffset(2015, 09, 07);
			pickLine.WZ_VerifiedEmpty = "Y";
			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			pickLine.WZ_F3_NKAllocatedPackType = "CAS";
			pickLine.WZ_P9_Task = ZGuid.BrettsGuid;

			AssertEquals(10m, pickLine.WZ_OriginalReservedQty);
			AssertEquals(10m, pickLine.WZ_Units);
			AssertNotEquals(ZGuid.Empty, pickLine.WZ_WE_InventoryLine);
			AssertNotEquals(ZGuid.Empty, pickLine.WZ_WE_TransactionLine);
			AssertEquals(false, pickLine.IsFinalised);

			var clonedPickLine = (WhsPickLine)pickLine.Clone();

			AssertEquals(10m, clonedPickLine.WZ_Units);
			AssertEquals("WZ_OriginalReservedQty should be excluded from cloning.", 0m,
				clonedPickLine.WZ_OriginalReservedQty);
			AssertEquals(pickLine.WZ_WE_InventoryLine, clonedPickLine.WZ_WE_InventoryLine);
			AssertEquals(pickLine.WZ_WE_TransactionLine, clonedPickLine.WZ_WE_TransactionLine);
			AssertEquals("Y", clonedPickLine.WZ_VerifiedEmpty);
			AssertEquals(data.Whs1.GetWarehouseBranchLocalDateTimeOffset(new ZDateTime(2015, 09, 07)), clonedPickLine.WZ_PickedDateTime);
			AssertEquals("XYZ", clonedPickLine.WZ_GS_NKAssignedTo);
			AssertEquals("CAS", clonedPickLine.WZ_F3_NKAllocatedPackType);
			AssertEquals(false, clonedPickLine.IsFinalised);
			AssertEquals(ZGuid.BrettsGuid, clonedPickLine.WZ_P9_Task);
		}

		public void TestClone_IsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, data.Whs1.DefaultLocation,
				data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();

			var pickLine = transferLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertIsFinalisedPrecondition(pickLine);

			var clone = (WhsPickLine)pickLine.Clone();
			AssertEquals("Clone of Finalised PickLine should be Finalised.", true, clone.IsFinalised);
		}

		public void TestClone_OriginalPickedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var newPickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition.", originalInventory.PK, newPickLine.WZ_WE_OriginalPickedInventoryLine);

			var clone = (WhsPickLine)newPickLine.Clone();
			AssertEquals("Should have cloned OriginalPickedInventoryLine.", originalInventory.PK,
				clone.WZ_WE_OriginalPickedInventoryLine);
		}

		#endregion

		#endregion

		#region Related Entities

		#region TestPick

		public void TestPick()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertNull(pickLine.Pick);

			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			pickLine.WZ_WE_TransactionLine = transferLine.PK;
			AssertNull(pickLine.Pick);

			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			pickLine.WZ_WE_TransactionLine = orderLine.PK;
			AssertNull(pickLine.Pick);

			var pick = Factory.New<WhsPick>();
			order.WD_WP = pick.PK;
			AssertEquals(pick, pickLine.Pick);
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertNull(pickLine.Product);

			var part1 = Factory.New<OrgSupplierPart>();
			var part2 = Factory.New<OrgSupplierPart>();
			var receive = Factory.New<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;

			receiveLine.WE_OP = part1.PK;
			AssertNotNull(pickLine.Product);
			AssertEquals(part1, pickLine.Product.Parent);

			receiveLine.WE_OP = part2.PK;
			AssertNotNull(pickLine.Product);
			AssertEquals(part2, pickLine.Product.Parent);
		}

		#endregion

		#region TestSupplierPart

		public void TestSupplierPart()
		{
			PickLine.Inventory.WI_OP = ZGuid.Empty;
			AssertNull(PickLine.SupplierPart);
			var part = Factory.New<OrgSupplierPart>();
			PickLine.Inventory.WI_OP = part.PK;
			AssertEquals(part, PickLine.SupplierPart);
		}

		#endregion

		#region TestWZ_ReleaseCapturedPartAttrib1_MaxLength

		public void TestWZ_ReleaseCapturedPartAttrib1_MaxLength()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertNoExceptionThrown(() => pickLine.WZ_ReleaseCapturedPartAttrib1 = "".PadLeft(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1.MaxLength, 'A'));
		}

		#endregion

		#region TestWZ_ReleaseCapturedPartAttrib2_MaxLength

		public void TestWZ_ReleaseCapturedPartAttrib2_MaxLength()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertNoExceptionThrown(() => pickLine.WZ_ReleaseCapturedPartAttrib2 = "".PadLeft(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib2.MaxLength, 'A'));
		}

		#endregion

		#region TestWZ_ReleaseCapturedPartAttrib3_MaxLength

		public void TestWZ_ReleaseCapturedPartAttrib3_MaxLength()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertNoExceptionThrown(() => pickLine.WZ_ReleaseCapturedPartAttrib3 = "".PadLeft(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib3.MaxLength, 'A'));
		}

		#endregion

		#region TestDocketLine

		public void TestDocketLine()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertNull(pickLine.DocketLine);

			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			pickLine.WZ_WE_TransactionLine = orderLine.PK;
			AssertEquals(orderLine, pickLine.DocketLine);
			AssertEquals(typeof(WhsOrderLine), pickLine.DocketLine.GetType());

			var workOrder = Factory.New<WhsWorkOrder>();
			var workOrderLine = workOrder.Lines.AddNew();
			pickLine.WZ_WE_TransactionLine = workOrderLine.PK;
			AssertEquals(workOrderLine, pickLine.DocketLine);
			AssertEquals(typeof(WhsWorkOrderLine), pickLine.DocketLine.GetType());
		}

		#endregion

		#region TestInventoryLine

		public void TestInventoryLine()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertNull(pickLine.InventoryLine);

			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			AssertEquals(transferLine, pickLine.InventoryLine);
			AssertEquals(typeof(WhsTransferLine), pickLine.InventoryLine.GetType());

			var receive = Factory.New<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			AssertEquals(receiveLine, pickLine.InventoryLine);
			AssertEquals(typeof(WhsReceiveLine), pickLine.InventoryLine.GetType());
		}

		#endregion

		#endregion

		#region Properties

		// calculated

		#region TestConsigneeNameOrPK

		public void TestConsigneeNameOrPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var reservedPickLine = Helper.CreateReservePickLine(orderLine, inventory, 1m);

			var consignee = Helper.CreateClient();
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;

			orderLine.WE_WD = order.PK;
			AssertEquals(consignee.PK.ToString(), reservedPickLine.ConsigneeNameOrPK);
			AssertEquals(true, reservedPickLine.ConsigneeNameOrPKInfo.ReadOnly);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "OVERRIDEN";
			AssertEquals("OVERRIDEN", reservedPickLine.ConsigneeNameOrPK);
			AssertEquals(true, reservedPickLine.ConsigneeNameOrPKInfo.ReadOnly);

			reservedPickLine.WZ_WE_TransactionLine = ZGuid.Empty;
			AssertEquals("", reservedPickLine.ConsigneeNameOrPK);
			AssertEquals(true, reservedPickLine.ConsigneeNameOrPKInfo.ReadOnly);
		}

		#endregion

		#region TestConsigneeFieldType

		public void TestConsigneeFieldType()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var reservedPickLine = Helper.CreateReservePickLine(orderLine, inventory, 1m);

			var consignee = Helper.CreateClient();
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;

			AssertEquals(nameof(FieldType.Guid), reservedPickLine.ConsigneeFieldType);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "OVERRIDEN";
			AssertEquals(nameof(FieldType.Text), reservedPickLine.ConsigneeFieldType);

			reservedPickLine.WZ_WE_TransactionLine = ZGuid.Empty;
			AssertEquals(nameof(FieldType.Text), reservedPickLine.ConsigneeFieldType);
		}

		#endregion

		#region TestPropertyInfoConcurrencyPolicies

		public void TestPropertyInfoConcurrencyPolicies()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals(ConcurrencyPolicy.Strict, pickLine.WZ_WE_InventoryLineInfo.ConcurrencyPolicy);
			AssertEquals(ConcurrencyPolicy.Strict, pickLine.WZ_WE_OriginalPickedInventoryLineInfo.ConcurrencyPolicy);
			AssertEquals(ConcurrencyPolicy.Strict, pickLine.WZ_UnitsInfo.ConcurrencyPolicy);
			AssertEquals(ConcurrencyPolicy.Strict, pickLine.WZ_IsPickingInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestInventoryLinePKForAvailableInventory

		public void TestInventoryLinePKForAvailableInventory()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = ZGuid.NewZGuid();
			AssertEquals(pickLine.WZ_WE_InventoryLine, pickLine.InventoryLinePKForAvailableInventory);

			pickLine.WZ_WE_OriginalPickedInventoryLine = ZGuid.NewZGuid();
			AssertEquals(pickLine.WZ_WE_OriginalPickedInventoryLine, pickLine.InventoryLinePKForAvailableInventory);
		}

		#endregion

		#region TestInventoryLineForAvailableInventory

		public void TestInventoryLineForAvailableInventory()
		{
			var inventoryLine1 = Factory.New<WhsReceiveLine>();
			var inventoryLine2 = Factory.New<WhsReceiveLine>();

			var pickLine = Factory.New<WhsPickLine>();
			AssertNull(pickLine.InventoryLineForAvailableInventory);

			pickLine.WZ_WE_InventoryLine = inventoryLine1.PK;
			AssertEquals(inventoryLine1, pickLine.InventoryLineForAvailableInventory);

			pickLine.WZ_WE_OriginalPickedInventoryLine = inventoryLine2.PK;
			AssertEquals(inventoryLine2, pickLine.InventoryLineForAvailableInventory);
		}

		#endregion

		#region TestReservedQuantity

		public void TestReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition - Pick Line Quantity is correct.", 10m, reservedPickLine.WZ_Units);
			AssertEquals("Precondition - Reserved Quantity is correct.", 10m, reservedPickLine.ReservedQuantity);
			AssertEquals("Precondition - Original Reserve Quantity is correct.", 10m,
				reservedPickLine.WZ_OriginalReservedQty);

			reservedPickLine.ReservedQuantity = 5m;
			AssertEquals(5m, reservedPickLine.WZ_Units);
			AssertEquals(5m, reservedPickLine.ReservedQuantity);
			AssertEquals("Original Qty should change until such time as the record is in the DB.", 5m,
				reservedPickLine.WZ_OriginalReservedQty);

			Factory.Save();
			reservedPickLine.ReservedQuantity = 8m;
			AssertEquals(8m, reservedPickLine.WZ_Units);
			AssertEquals(8m, reservedPickLine.ReservedQuantity);
			AssertEquals(
				"Original Qty should *not* change as the record is in the DB (And needed to restore if a pick is cancelled).",
				5m, reservedPickLine.WZ_OriginalReservedQty);

			var nonReservedPickLine = Factory.New<WhsPickLine>();
			nonReservedPickLine.WZ_WE_TransactionLine = orderLine.PK;
			AssertEquals("Precondition: PickLine has no units.", 0m, nonReservedPickLine.WZ_Units);
			AssertEquals("Precondition: PickLine has no units.", 0m, nonReservedPickLine.ReservedQuantity);

			nonReservedPickLine.ReservedQuantity = 2m;
			AssertEquals("Non-Reserve pick lines do nothing when setting ReservedQuantity.", 0m,
				nonReservedPickLine.WZ_Units);
			AssertEquals("Non-Reserve pick lines do nothing when setting ReservedQuantity.", 0m,
				nonReservedPickLine.ReservedQuantity);
			nonReservedPickLine.Delete(); // clean-up

			var pick = Factory.New<WhsPick>();
			pick.PickOrders(order);
			AssertEquals(8m, reservedPickLine.ReservedQuantity);
			AssertEquals(8m, reservedPickLine.WZ_Units);
			AssertEquals(5m, reservedPickLine.WZ_OriginalReservedQty);

			reservedPickLine.ReservedQuantity = 10m;
			AssertEquals(8m, reservedPickLine.ReservedQuantity);
			AssertEquals(8m, reservedPickLine.WZ_Units);
			AssertEquals(5m, reservedPickLine.WZ_OriginalReservedQty);
		}

		#endregion

		#region TestIsPickFinalised

		public void TestIsPickFinalised()
		{
			AssertEquals(false, PickLine.IsPickFinalised);
			Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Cancelled;
			AssertEquals(false, PickLine.IsPickFinalised);
			Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Finalised;
			AssertEquals(true, PickLine.IsPickFinalised);
		}

		#endregion

		#region TestIsPickCancelled

		public void TestIsPickCancelled()
		{
			AssertEquals(false, PickLine.IsPickCancelled);
			Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Finalised;
			AssertEquals(false, PickLine.IsPickCancelled);
			Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Cancelled;
			AssertEquals(true, PickLine.IsPickCancelled);
		}

		#endregion

		#region TestIsPickFinalisedOrCancelled

		public void TestIsPickFinalisedOrCancelled()
		{
			AssertEquals(false, PickLine.IsPickFinalisedOrCancelled);
			Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Created;
			AssertEquals(false, PickLine.IsPickFinalisedOrCancelled);
			Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Finalised;
			AssertEquals(true, PickLine.IsPickFinalisedOrCancelled);
			Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Cancelled;
			AssertEquals(true, PickLine.IsPickFinalisedOrCancelled);
		}

		#endregion

		#region TestIsPickByBOMKitPickLine

		public void TestIsPickByBOMKitPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLocation = data.Whs1.FindLocation("A-1-1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, bike, 4m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, inventoryLocation);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 14m);
			var pick = Helper.CreatePickNew(order);

			var normalPickLine = orderLine.PickLines.Single(l => l.WZ_Units == 4m);
			var pickByBOMPickLine = orderLine.PickLines.Single(l => l.WZ_Units == 10m);
			AssertEquals(false, normalPickLine.IsPickByBOMKitPickLine());
			AssertEquals(true, pickByBOMPickLine.IsPickByBOMKitPickLine());

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receive2.Lines[0].PK;
			AssertEquals(false, pickLine.IsPickByBOMKitPickLine());
		}

		public void TestIsPickByBOMKitPickLine_OverPickedLineOnKitReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 8m, location1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 12m, location2);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location2);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickByBOMPickLine = orderLine.PickLines.Single();
			AssertEquals(true, pickByBOMPickLine.IsPickByBOMKitPickLine());

			// mock shorting
			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine1 = wheelOrderLine.PickLines.Single(pl => pl.WZ_Units == 8m);
			wheelPickLine1.Delete();
			pickByBOMPickLine.WZ_Units = 6m;
			var kitReceiveLine = pickByBOMPickLine.InventoryLine;
			kitReceiveLine.WE_TransactionQuantity = 6m;
			kitReceiveLine.WE_StockOnHand = 6m;

			var line2 = inventory1.InDocketLine;
			line2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.ShortPicked;
			line2.HeldCodeChangeQuantity = line2.AvailableToTransferQuantity;
			line2.ChangeInventoryHeldCode(true);

			var links = Factory.Load<WhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine, kitReceiveLine.PK));
			links.ForEach(l => l.Delete());
			var newLink1 = Factory.New<WhsBOMInventoryPivot>();
			newLink1.WIP_ComponentQuantity = 12m;
			newLink1.WIP_WE_ComponentLine = wheelOrderLine.PK;
			newLink1.WIP_WE_InventoryLine = kitReceiveLine.PK;
			var newLink2 = Factory.New<WhsBOMInventoryPivot>();
			newLink2.WIP_ComponentQuantity = 6m;
			newLink2.WIP_WE_ComponentLine = frameOrderLine.PK;
			newLink2.WIP_WE_InventoryLine = kitReceiveLine.PK;

			// Finalise to return over-picked frames.
			wheelOrderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			frameOrderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			AssertEquals(2, kitReceiveLine.Docket.Lines.Count);
			var returnedLine = kitReceiveLine.Docket.Lines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Precondition", inventory3.PK, returnedLine.WE_WE_OriginalDocketLineForRating);
			AssertEquals("Precondition", 4m, returnedLine.WE_TransactionQuantity);
			AssertEquals(true, pickByBOMPickLine.IsPickByBOMKitPickLine());

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = returnedLine.PK;
			AssertEquals(false, pickLine.IsPickByBOMKitPickLine());
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals("", pickLine.ProductCode);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			AssertEquals("P1", pickLine.ProductCode);
		}

		#endregion

		#region TestWZ_Units

		public void TestWZ_Units()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_Units = 10m;
			AssertEquals("Precondition: Setter works.", 10m, pickLine.WZ_Units);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Cannot change quantity on a Picked PickLine (i.e. you cannot change the amount that was Picked).",
				() => pickLine.WZ_Units = 20m);
		}

		public void TestWZ_Units_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_Units = 10m;
			AssertEquals("Precondition: Setter works.", 10m, pickLine.WZ_Units);

			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Cannot change quantity on a Picked PickLine (i.e. you cannot change the amount that was Picked).",
				() => pickLine.WZ_Units = 20m);
		}

		#endregion

		#region TestWZ_UnitsUQ

		public void TestWZ_UnitsUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);

			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals("UNT", pickLine.WZ_UnitsUQ);

			data.Part1.OP_StockKeepingUnit = "KG";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			AssertEquals("KG", pickLine.WZ_UnitsUQ);
		}

		#endregion

		#region TestWZ_Units_ReadOnly

		public void TestWZ_Units_ReadOnly()
		{
			var pickLine = Factory.NewWithValidTestData<WhsPickLine>();
			AssertEquals(true, pickLine.WZ_UnitsInfo.ReadOnly);
		}

		#endregion

		// persistent

		#region TestWZ_OriginalReservedQty

		public void TestWZ_OriginalReservedQty()
		{
			AssertExceptionThrown<NotSupportedException>("Do not call the setter of WZ_OriginalReservedQty directly.",
				() => Factory.New<WhsPickLine>().WZ_OriginalReservedQty = 1m);
		}

		#endregion

		#region TestWZ_OriginalReservedQtyInfo

		public void TestWZ_OriginalReservedQtyInfo()
		{
			AssertEquals(true, Factory.New<WhsPickLine>().WZ_OriginalReservedQtyInfo.ReadOnly);
		}

		#endregion

		#region TestWZ_WE_InventoryLine

		[TestDate(2016, 12, 21)]
		public void TestWZ_WE_InventoryLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			var receiveLine1 = receive1.Lines[0];
			var receiveLine2 = receive2.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: Pickline is not picked.", ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);

			AssertNoExceptionThrown(() => pickLine.WZ_WE_InventoryLine = receiveLine1.PK);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertExceptionThrown(typeof(InvalidOperationException),
				"You cannot change inventory for a picked pickline.",
				() => pickLine.WZ_WE_InventoryLine = receiveLine2.PK);
		}

		#endregion

		#region TestWZ_PickedDateTime

		public void TestWZ_PickedDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, data.Whs1.DefaultLocation,
				data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();

			var pickLine = transferLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory Total Units should be reduced when PickLine picked.", 5m,
				pickLine.InventoryLine.WE_StockOnHand);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Inventory Total Units should restored when PickLine is unpicked.", 20m,
				pickLine.InventoryLine.WE_StockOnHand);

			pickLine.WZ_Units = 16m;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory Total Units should be reduced when PickLine picked.", 4m,
				pickLine.InventoryLine.WE_StockOnHand);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now.AddHours(-1);
			AssertEquals("Inventory Total Units should *not* be reduced again.", 4m,
				pickLine.InventoryLine.WE_StockOnHand);

			transferLine.WE_TransactionQuantity = 16m; // allow transfer line to save
			Factory.Save();
			AssertIsFinalisedPrecondition(pickLine);
			AssertExceptionThrown(typeof(InvalidOperationException),
				"You cannot change the Picked Time of a PickLine that is already Picked in the DB.",
				() => pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty);

			using (((IWhsPickLineInternals)pickLine).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				AssertExceptionThrown(typeof(InvalidOperationException),
					"You cannot change the Picked Time of a PickLine that is already Picked in the DB.",
					() => pickLine.WZ_PickedDateTime = pickLine.WZ_PickedDateTime.AddDays(1));
				AssertNoExceptionThrown(() => pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty);
			}
		}

		public void TestWZ_PickedDateTime_PickingWithoutChangingStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			AssertExceptionThrown<ArgumentException>(() =>
				((IWhsPickLineInternals)pickLine).PickWithoutReducingStock_DoNotUse(ZDateTimeOffset.Empty));
			((IWhsPickLineInternals)pickLine).PickWithoutReducingStock_DoNotUse(ZDateTimeOffset.Now);
			AssertEquals("Inventory Total Units should *not* be reduced when Picking Stock through Interface method.",
				20m, pickLine.InventoryLine.WE_StockOnHand);
		}

		public void TestWZ_PickedDateTime_WhenNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory Total Units should be reduced when PickLine picked.", 5m,
				pickLine.InventoryLine.WE_StockOnHand);

			using (((IWhsPickLineInternals)pickLine).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
				AssertEquals("Inventory Total Units should not be increased when unpicking Semaphore is active.", 5m,
					pickLine.InventoryLine.WE_StockOnHand);
			}
		}

		public void TestWZ_PickedDateTime_WhenOverCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_Units = 30m; // overcommit stock
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Picked Time will be reverted if you attempt to Pick an overcommitted pickline.",
				ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);
			AssertEquals(
				"Picked Time will be set to the Inventory Total Units if you attempt to Pick an overcommitted pickline.",
				20m, pickLine.WZ_Units);
			AssertEquals("Inventory Total Units should be unchanged.", 20m, pickLine.InventoryLine.WE_StockOnHand);
		}

		public void TestWZ_PickedDateTime_ModifyingPicksPercentCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2,
				1); // 2 locations in total, one location picked is 50%.
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locations[1]);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);

			AssertEquals("None of the Locations should be Picked yet", (ZByte)0, pick.WP_PercentageComplete);

			orderLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("One Location should be Picked", (ZByte)50, pick.WP_PercentageComplete);

			orderLine.PickLines[1].WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Two Location should be Picked", (ZByte)100, pick.WP_PercentageComplete);
		}

		[TestDate(2018, 1, 1, 16, 10, 55, 234)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWZ_PickedDateTime_TruncatesMilliseconds()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 14m, locationA1, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			AssertIsFinalisedPrecondition(pickLine);

			// load order from a new factory
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var newPickLine = newFactory.Load<WhsPickLine>(pickLine.PK);
			var dateTime = new ZDateTime(2018, 1, 1, 16, 10, 55, 000);
			AssertEquals("WZ_PickedDateTime is incorrect.", data.Whs1.GetWarehouseBranchDateTimeOffset(dateTime), newPickLine.WZ_PickedDateTime);
		}

		#region TestWZ_PickedDateTime_AffectsLastInventoryModifiedDate

		[TestDate(2018, 12, 12)]
		public void TestWZ_PickedDateTime_AffectsLastInventoryModifiedDate_Picking()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m).Lines[0]
				.Inventory[0];
			Factory.Save();
			AssertEquals("Precondition.", now, inventory.Location.WLV_LastInventoryChangeDate);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition:", ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();
			pickLine.WZ_PickedDateTime = now;
			AssertEquals("Should update when picking with current date not with pick time.", now.AddDays(1),
				inventory.Location.WLV_LastInventoryChangeDate);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Rollback if not saved pick time.", now, inventory.Location.WLV_LastInventoryChangeDate);
		}

		[TestDate(2018, 12, 12)]
		public void TestWZ_PickedDateTime_AffectsLastInventoryModifiedDate_NoChangeInStock()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m).Lines[0]
				.Inventory[0];
			Factory.Save();
			AssertEquals("Precondition.", now, inventory.Location.WLV_LastInventoryChangeDate);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);
			Factory.Save();

			Helper.CreatePickNew(order);
			TestDateAttribute.Date = now.AddDays(1).ToDateTime();
			reservedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Should update LastInventoryChangeDate when pick.", now.AddDays(1),
				inventory.Location.WLV_LastInventoryChangeDate);

			reservedPickLine.ClearOutPickingValuesForReservedLine();
			AssertEquals("Should undo LastInventoryChangeDate when unpick same pick.", now,
				inventory.Location.WLV_LastInventoryChangeDate);
		}

		public void TestWZ_PickedDateTime_AffectsLastInventoryModifiedDate_NoLocation()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, false, false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);

			AssertNull("Precondition.", inventory.Location);
			AssertEquals("Precondition.", ZDateTimeOffset.Empty, reservedPickLine.WZ_PickedDateTime);

			AssertNoExceptionThrown("Should not have exception when location is not specify in inventory.",
				() => reservedPickLine.WZ_PickedDateTime = now);
		}

		#endregion

		#region TestWZ_PickedDateTime_PICStatusOnOrders

		public void TestWZ_PickedDateTime_PICStatusOnOrders_SetsPIC()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order Status is ATP", DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Order WD_DocketStatus now PIC", DocketStatus.Codes.Picking, order.WD_DocketStatus);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Order WD_DocketStatus now ATP", DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);
		}

		public void TestWZ_PickedDateTime_PICStatusOnOrders_UnsettingPickDateTimeDoesNotChangeOriginalPICStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreatePickNew(order);
			order.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertEquals("Precondition: Order Status is PIC", DocketStatus.Codes.Picking, order.WD_DocketStatus);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Order WD_DocketStatus PIC", DocketStatus.Codes.Picking, order.WD_DocketStatus);
			Factory.Save();

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Order WD_DocketStatus still PIC", DocketStatus.Codes.Picking, order.WD_DocketStatus);
		}

		public void TestWZ_PickedDateTime_PICStatusOnOrders_OtherPickedPicklineOnOrderPreventsChangingOrderPICStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order Status is ATP", DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);

			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Order WD_DocketStatus now PIC", DocketStatus.Codes.Picking, order.WD_DocketStatus);

			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Order WD_DocketStatus now PIC", DocketStatus.Codes.Picking, order.WD_DocketStatus);

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Order WD_DocketStatus still PIC", DocketStatus.Codes.Picking, order.WD_DocketStatus);
		}

		#endregion

		#region TestWZ_PickedDateTime_SetAfterCommittedService

		public void TestWZ_PickedDateTime_SetAfterCommittedService()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 101m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			Factory.Save();

			AssertNull("Service should not be added yet.", Factory.ServiceContainer.GetAfterCommittedService<WhsPickLineInPickFaceAfterPickingService>());

			pickLine.WZ_PickedDateTime = new ZDateTimeOffset(2020, 2, 2);
			AssertNotNull("Service should be added.", Factory.ServiceContainer.GetAfterCommittedService<WhsPickLineInPickFaceAfterPickingService>());

			var newFactory = new BusinessObjectFactory();
			var newFactoryPickLine = newFactory.Load<WhsPickLine>(pickLine.PK);
			AssertNull("Service should not be added.", newFactory.ServiceContainer.GetAfterCommittedService<WhsPickLineInPickFaceAfterPickingService>());

			((IBusinessObjectInternals)newFactoryPickLine).Row[WhsPickLineSchema.Constants.WZ_PickedDateTime] = new DateTimeOffset(2020, 2, 3, 0, 0, 0, TimeSpan.Zero);
			AssertEquals("WZ_PickedDateTime is not empty.", false, newFactoryPickLine.WZ_PickedDateTime.IsEmpty);
			AssertNull("Service should not be added.", newFactory.ServiceContainer.GetAfterCommittedService<WhsPickLineInPickFaceAfterPickingService>());

			newFactoryPickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertNull("New value is empty, service should not be added.", newFactory.ServiceContainer.GetAfterCommittedService<WhsPickLineInPickFaceAfterPickingService>());

			newFactoryPickLine.WZ_WE_OriginalPickedInventoryLine = newFactoryPickLine.WZ_WE_InventoryLine;
			newFactoryPickLine.WZ_PickedDateTime = new ZDateTimeOffset(2020, 2, 2);
			AssertNull("Pick line is for dock door movement, service should not be added.", newFactory.ServiceContainer.GetAfterCommittedService<WhsPickLineInPickFaceAfterPickingService>());
			newFactoryPickLine.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;

			newFactoryPickLine.Inventory.Location.LocationType.WLT_LocationClass = "RNO";
			newFactoryPickLine.WZ_PickedDateTime = new ZDateTimeOffset(2020, 2, 2);
			AssertNull("Inventory is not in a pick face, service should not be added.", newFactory.ServiceContainer.GetAfterCommittedService<WhsPickLineInPickFaceAfterPickingService>());
		}

		#endregion

		#endregion

		#region TestWZ_IsPicking

		public void TestWZ_IsPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Getter works.", false, pickLine.WZ_IsPicking);

			pickLine.WZ_IsPicking = true;
			AssertEquals("Setter works.", true, pickLine.WZ_IsPicking);
		}

		[TestDate(2016, 12, 12)]
		public void TestWZ_IsPicking_AfterSetPickedDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_IsPicking = true;
			AssertEquals("Precondition:", true, pickLine.WZ_IsPicking);
			AssertEquals("Precondition:", ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("When PickLine is picked, WZ_IsPicking should be false.", false, pickLine.WZ_IsPicking);

			pickLine.WZ_IsPicking = true;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals(
				"When PickLine is picking and set WZ_PickedDateTime a empty value, WZ_IsPicking should not be false.",
				true, pickLine.WZ_IsPicking);
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly()
		{
			var pickLine = Factory.New<WhsPickLine>();
			var fieldsTester = ObjectFactory.Get<IOperationalActionFieldTester>();

			var propertiesBeyondTheScopeOfThisTest = GetPropertiesBeyondTheScopeOfThisTest();
			foreach (var propertyInfo in pickLine.ZPropertyInfoHash.Cast<ZPropertyInfo>()
						 .Where(info => !propertiesBeyondTheScopeOfThisTest.Contains(info.Name)))
			{
				AssertEquals($"Property {propertyInfo.Name} must be read-only.", true, propertyInfo.ReadOnly);

				var actionFieldAttribute = ActionFieldAttribute.Get(typeof(WhsPickLine).GetProperty(propertyInfo.Name));
				if (actionFieldAttribute != null)
				{
					AssertEquals($"Action field attribute for {propertyInfo.Name} must be read-only.", true,
						actionFieldAttribute.ReadOnly);
				}
				else
				{
					AssertEquals(true, fieldsTester.IsFieldUnsupported(typeof(WhsPickLine), propertyInfo.Name));
				}
			}
		}

		HashSet<ZString> GetPropertiesBeyondTheScopeOfThisTest()
		{
			// These fields are not required to be readonly as there are not shown.
			return new HashSet<ZString>
			{
				WhsPickLineSchema.Constants.WZ_SystemCreateTimeUtc, // Irrelevant, system column value
				WhsPickLineSchema.Constants.WZ_SystemCreateUser, // Irrelevant, system column value
				WhsPickLineSchema.Constants.WZ_SystemLastEditTimeUtc, // Irrelevant, system column value
				WhsPickLineSchema.Constants.WZ_SystemLastEditUser // Irrelevant, system column value
			};
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsPickedFromPutawayLocation

		public void TestIsPickedFromPutawayLocation_WhenPicked()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals(false, pickLine.IsPickedFromPutawayLocation);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals(true, pickLine.IsPickedFromPutawayLocation);
		}

		public void TestIsPickedFromPutawayLocation_WhenOriginalInventoryLineIsSet()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals(false, pickLine.IsPickedFromPutawayLocation);

			pickLine.WZ_WE_OriginalPickedInventoryLine = ZGuid.NewZGuid();
			AssertEquals(true, pickLine.IsPickedFromPutawayLocation);
		}

		#endregion

		#region TestIsFinalised

		public void TestIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, data.Whs1.DefaultLocation,
				data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();

			var pickLine = transferLine.PickLines.Single();
			AssertEquals("IsFinalised is false by default.", false, pickLine.IsFinalised);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("IsFinalised is only true when PickLine is Picked in DB.", false, pickLine.IsFinalised);

			Factory.Save();
			AssertEquals("IsFinalised is true when PickLine is Picked in DB.", true, pickLine.IsFinalised);
		}

		#endregion

		#region TestIsPicked

		public void TestIsPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			AssertEquals("IsPicked is false when there is no Picked Time.", false, pickLine.IsPicked);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("IsPicked is true when there is a Picked Time.", true, pickLine.IsPicked);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("IsPicked is false when there is no Picked Time.", false, pickLine.IsPicked);
		}

		#endregion

		#region TestIsPickedInMemory

		public void TestIsPickedInMemory()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals(false, pickLine.IsPickedInMemory);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals(true, pickLine.IsPickedInMemory);
		}

		public void TestIsPickedInMemory_PickLineInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: Pick Line is in DB.", true, pickLine.IsInDatabase);
			AssertEquals(false, pickLine.IsPickedInMemory);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals(true, pickLine.IsPickedInMemory);
		}

		#endregion

		#region WasPickedInMemoryForStocktake

		public void TestWasPickedInMemoryForStocktake_NotInDb()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals("PickLine is not in db.", true, pickLine.WasPickedInMemoryForStocktake);
		}

		public void TestWasPickedInMemoryForStocktake_InDb_PickDateTimeHasChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: Pick Line is in DB.", true, pickLine.IsInDatabase);
			AssertEquals("No changes to PickdateTime yet.", false, pickLine.WasPickedInMemoryForStocktake);
			AssertEquals("PickLine not picked.", false, pickLine.WasPickedInMemoryForStocktake);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("PickLine picked.", true, pickLine.WasPickedInMemoryForStocktake);
		}

		public void TestWasPickedInMemoryForStocktake_InDb_OriginalPickedInventoryLineHasChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: Pick Line is in DB.", true, pickLine.IsInDatabase);
			AssertEquals("No OriginalPickedInventoryLine yet.", false, pickLine.WasPickedInMemoryForStocktake);
			AssertEquals("PickLine not picked.", false, pickLine.WasPickedInMemoryForStocktake);

			pickLine.WZ_WE_OriginalPickedInventoryLine = ZGuid.BrettsGuid;
			AssertEquals("OriginalPickedInventoryLine added.", true, pickLine.WasPickedInMemoryForStocktake);
		}

		#endregion

		#region TestIsReserveLine

		public void TestIsReserveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			var reservedPickLine = orderLine1.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 5m, reservedPickLine.ReservedQuantity);
			Factory.Save();

			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals(
				"Only picklines with an WZ_OriginalReservedQty > 0 and are in the DB should have IsReserveLine equal to true.",
				false, pickLine.IsReserveLine);

			pickLine.IsReserveLine = true;
			AssertEquals("Setting IsReserveLine to true is allowed for unsaved pick lines.", true,
				pickLine.IsReserveLine);
			AssertExceptionThrown<InvalidOperationException>(
				"Cannot un-reserve a reserve pick line or change a saved pick line.",
				() => pickLine.IsReserveLine = false);
			pickLine.Delete(); // clean-up

			var pick = Helper.CreatePickNew(order2);
			AssertEquals("Precondition: Order is picked.", 5m, orderLine2.PickLineQuantity);

			var pickedPickLine = orderLine2.PickLines[0];
			var otherFactory = new BusinessObjectFactory();
			var reservedPickLineInOtherFactory = otherFactory.Load<WhsPickLine>(reservedPickLine.PK);
			var pickedPickLineInOtherFactory = otherFactory.Load<WhsPickLine>(pickedPickLine.PK);
			AssertEquals(true, reservedPickLineInOtherFactory.IsReserveLine);
			AssertEquals(false, pickedPickLineInOtherFactory.IsReserveLine);
			AssertExceptionThrown<InvalidOperationException>(
				"Cannot un-reserve a reserve pick line or change a saved pick line.",
				() => pickedPickLineInOtherFactory.IsReserveLine = true);
		}

		#endregion

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.SetReleaseCapturedAttributes("PA1", "PA2", "PA3", "");
			pickLine.Delete();
			AssertEquals(true, pickLine.IsDeleted);
		}

		public void TestDelete_WhenPickLineIsPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, data.Whs1.DefaultLocation,
				data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();

			var pickLine1 = transferLine.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var inventory = pickLine1.InventoryLine;
			AssertEquals("Precondition: Picking should have reduced Inventory.", 0m, inventory.WE_StockOnHand);

			pickLine1.Delete();
			AssertEquals("Deleting Picked PickLine should restore Inventory.", 20m, inventory.WE_StockOnHand);

			transferLine.RunPreSaveValidation();

			var pickLine2 = transferLine.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Picking should have reduced Inventory.", 0m,
				pickLine2.InventoryLine.WE_StockOnHand);

			Factory.Save();
			AssertIsFinalisedPrecondition(pickLine2);
			AssertExceptionThrown(typeof(InvalidOperationException),
				"You cannot change the Picked Time of a PickLine that is already Picked in the DB.",
				() => pickLine2.Delete());
		}

		public void TestDelete_WhenPickLineIsPicked_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertExceptionThrown(typeof(InvalidOperationException),
				"You cannot Delete a Pick Line with Originally Picked Inventory set.", () => pickLine.Delete());
		}

		#endregion

		#region TestClearOutPickingValuesForReservedLine

		public void TestClearOutPickingValuesForReservedLine()
		{
			var pickLine1 = Factory.New<WhsPickLine>();
			AssertExceptionThrown<InvalidOperationException>(
				"ClearOutPickingValuesForReservedLine() should only be called on Reserved Pick Lines.",
				() => pickLine1.ClearOutPickingValuesForReservedLine());

			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine2.IsReserveLine = true;
			pickLine2.WZ_Units = 1m;
			pickLine2.WZ_PickedDateTime = new ZDateTimeOffset(2015, 1, 1);
			pickLine2.WZ_GS_NKAssignedTo = "E";
			pickLine2.WZ_VerifiedEmpty = "E";
			pickLine2.WZ_F3_NKAllocatedPackType = "UNT";
			pickLine2.WZ_ReleaseCapturedPartAttrib1 = "attrib1";
			pickLine2.WZ_ReleaseCapturedPartAttrib2 = "attrib2";
			pickLine2.WZ_ReleaseCapturedPartAttrib3 = "attrib3";
			pickLine2.WZ_ReleaseCapturedSerialNumber = "serial";
			pickLine2.ClearOutPickingValuesForReservedLine();
			AssertEquals("ClearOutPickingValuesForReservedLine() should clear out all picking fields.", ZDateTimeOffset.Empty,
				pickLine2.WZ_PickedDateTime);
			AssertEquals("ClearOutPickingValuesForReservedLine() should clear out all picking fields.", "",
				pickLine2.WZ_GS_NKAssignedTo);
			AssertEquals("ClearOutPickingValuesForReservedLine() should clear out all picking fields.", 0m,
				pickLine2.WZ_Units);
			AssertEquals("ClearOutPickingValuesForReservedLine() should clear out all picking fields.", "",
				pickLine2.WZ_VerifiedEmpty);
			AssertEquals("ClearOutPickingValuesForReservedLine() should clear out all picking fields.", "",
				pickLine2.WZ_F3_NKAllocatedPackType);
			AssertEquals("ClearOutPickingValuesForReservedLine() should clear out all picking fields.", "",
				pickLine2.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("ClearOutPickingValuesForReservedLine() should clear out all picking fields.", "",
				pickLine2.WZ_ReleaseCapturedPartAttrib2);
			AssertEquals("ClearOutPickingValuesForReservedLine() should clear out all picking fields.", "",
				pickLine2.WZ_ReleaseCapturedPartAttrib3);
			AssertEquals("ClearOutPickingValuesForReservedLine() should clear out all picking fields.", "",
				pickLine2.WZ_ReleaseCapturedSerialNumber);
		}

		#endregion

		#region TestClearOutPickingValuesForReservedLine_WhenPickLinePicked

		public void TestClearOutPickingValuesForReservedLine_WhenPickLinePicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);
			Factory.Save();

			Helper.CreatePickNew(order);
			reservedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			reservedPickLine.WZ_IsPicking = true;
			AssertEquals("Precondition: Inventory Total Units is reduced.", 0m, inventory.WI_TotalUnits);
			AssertEquals("Precondition: IsPicking is true.", true, reservedPickLine.WZ_IsPicking);

			reservedPickLine.ClearOutPickingValuesForReservedLine();
			AssertEquals("Picked Time should be cleared when clearing Reserved PickLine.", ZDateTimeOffset.Empty,
				reservedPickLine.WZ_PickedDateTime);
			AssertEquals("Units should be cleared when clearing Reserved PickLine.", 0m, reservedPickLine.WZ_Units);
			AssertEquals("Picked Inventory should have its Total Units restored when clearing Reserved PickLine.", 10m,
				inventory.WI_TotalUnits);
			AssertEquals("IsPicking should be false when clearing Reserved PickLine.", false,
				reservedPickLine.WZ_IsPicking);
		}

		#endregion

		#region TestClearOutPickingValuesForReservedLine_WithReleaseCapturedAttribs

		public void TestClearOutPickingValuesForReservedLine_WithReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);
			Factory.Save();

			Helper.CreatePickNew(order);
			reservedPickLine.SetReleaseCapturedAttributes("RED", "", "", "");
			AssertEquals("Pick Line should be fully release captured.", 0m, reservedPickLine.UnreleaseCapturedQty);
			AssertEquals("Pick Line should be fully release captured.", true, reservedPickLine.HasReleaseCapturedAttribs);

			reservedPickLine.ClearOutPickingValuesForReservedLine();
			AssertEquals("Pick Line should no longer be release captured.", false, reservedPickLine.HasReleaseCapturedAttribs);
		}

		#endregion

		#region TestOperationalActionsFieldVisibility

		public void TestOperationalActionsFieldVisibility()
		{
			AssertEquals(true,
				ActionFieldAttribute.Get(typeof(WhsPickLine).GetProperty(WhsPickLineSchema.WZ_Units.Name)).ReadOnly);
			AssertEquals(true,
				ActionFieldAttribute.Get(typeof(WhsPickLine).GetProperty(WhsPickLineSchema.WZ_VerifiedEmpty.Name))
					.ReadOnly);

			// These fields are currently not supported, should become readonly in future if architecture supports them
			var fieldsTester = ObjectFactory.Get<IOperationalActionFieldTester>();
			AssertEquals(true,
				fieldsTester.IsFieldUnsupported(GetExpectedBusinessObjectType(),
					WhsPickLineSchema.WZ_WE_InventoryLine.Name));
			AssertEquals(true,
				fieldsTester.IsFieldUnsupported(GetExpectedBusinessObjectType(),
					WhsPickLineSchema.WZ_WE_TransactionLine.Name));
			AssertEquals(true,
				fieldsTester.IsFieldUnsupported(GetExpectedBusinessObjectType(),
					WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine.Name));
		}

		#endregion

		#region TestPickedDateTime_SetsPickerToCurrentUserIfPicked

		[TestDate(2014, 01, 03)]
		public void TestPickedDateTime_SetsPickerToCurrentUserIfPicked()
		{
			var staff = Helper.CreateGlbStaff("BRS", "BRS");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, sourceLocation, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, destinationLocation);
			AssertEquals("Precondition", "", line.GS_NKPickedBy);

			transfer.RunPreSaveValidation();
			Factory.Save();
			var pickLine = line.PickLines.Single();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now.AddDays(2);
			}

			var now = ZDateTime.Now;
			var nowOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(now);

			AssertEquals("Precondition.", nowOffset.AddDays(2), pickLine.WZ_PickedDateTime);
			AssertEquals("Should have set PickedBy to current user", "BRS", pickLine.WZ_GS_NKAssignedTo);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition.", ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);
			AssertEquals("Should *not* have cleared PickedBy.", "BRS", pickLine.WZ_GS_NKAssignedTo);

			pickLine.WZ_GS_NKAssignedTo = "E";
			AssertEquals("Precondition.", "E", pickLine.WZ_GS_NKAssignedTo);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				pickLine.WZ_PickedDateTime = nowOffset;
			}

			AssertEquals("Precondition.", nowOffset, pickLine.WZ_PickedDateTime);
			AssertEquals("Should *not* have overridden PickedBy.", "E", pickLine.WZ_GS_NKAssignedTo);

			pickLine.WZ_GS_NKAssignedTo = "";
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			}

			AssertEquals("Clearing PickedTime should *not* set PickedBy.", "", pickLine.WZ_GS_NKAssignedTo);
		}

		#endregion

		#region TestSplit

		public void TestSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "XYZ";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = new ZDateTimeOffset(2015, 09, 07);
			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			pickLine.WZ_F3_NKAllocatedPackType = "CAS";
			pickLine.WZ_IsPicking = true;

			AssertEquals("Precondition", 10m, pickLine.WZ_Units);
			AssertEquals("Precondition", receive.Lines[0].PK, pickLine.WZ_WE_InventoryLine);
			AssertEquals("Precondition", order.Lines[0].PK, pickLine.WZ_WE_TransactionLine);
			AssertEquals("Precondition", "XYZ", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Precondition", "CAS", pickLine.WZ_F3_NKAllocatedPackType);
			AssertEquals("Precondition", true, pickLine.WZ_IsPicking);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 1, availableInventory.PickLines.Count());
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());

			var newPickLine = pickLine.Split(7m);
			AssertCollectionContains(newPickLine, newPickLine.DocketLine.PickLines);
			AssertEquals(2, availableInventory.PickLines.Count());
			AssertContainsExactElementsInAnyOrder(availableInventory.PickLines, new[] { pickLine, newPickLine });

			AssertEquals(2, pick.GetAllPickLines().Count());
			AssertContainsExactElementsInAnyOrder(pick.GetAllPickLines(), new[] { pickLine, newPickLine });

			AssertEquals(3m, pickLine.WZ_Units);
			AssertEquals(receive.Lines[0].PK, pickLine.WZ_WE_InventoryLine);
			AssertEquals(order.Lines[0].PK, pickLine.WZ_WE_TransactionLine);
			AssertEquals("XYZ", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("CAS", pickLine.WZ_F3_NKAllocatedPackType);
			AssertEquals(true, pickLine.WZ_IsPicking);

			AssertEquals(7m, newPickLine.WZ_Units);
			AssertEquals(receive.Lines[0].PK, newPickLine.WZ_WE_InventoryLine);
			AssertEquals(order.Lines[0].PK, newPickLine.WZ_WE_TransactionLine);
			AssertEquals("XYZ", newPickLine.WZ_GS_NKAssignedTo);
			AssertEquals("CAS", newPickLine.WZ_F3_NKAllocatedPackType);
			AssertEquals(true, newPickLine.WZ_IsPicking);
		}

		public void TestSplit_RCA_Attribute1()
		{
			TestSplit_RCA_Core(AttributeNumber.One);
		}

		public void TestSplit_RCA_Attribute2()
		{
			TestSplit_RCA_Core(AttributeNumber.Two);
		}

		public void TestSplit_RCA_Attribute3()
		{
			TestSplit_RCA_Core(AttributeNumber.Three);
		}

		void TestSplit_RCA_Core(AttributeNumber attributeNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true, setReleaseCaptured: true);
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "XYZ";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = new ZDateTimeOffset(2015, 09, 07);
			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			pickLine.WZ_F3_NKAllocatedPackType = "CAS";
			pickLine.WZ_IsPicking = true;
			setAttrib(pickLine);

			AssertEquals("Precondition", 10m, pickLine.WZ_Units);
			AssertEquals("Precondition", receive.Lines[0].PK, pickLine.WZ_WE_InventoryLine);
			AssertEquals("Precondition", order.Lines[0].PK, pickLine.WZ_WE_TransactionLine);
			AssertEquals("Precondition", "XYZ", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Precondition", "CAS", pickLine.WZ_F3_NKAllocatedPackType);
			AssertEquals("Precondition", true, pickLine.WZ_IsPicking);
			AssertEquals("Precondition", "RED", getAttrib(pickLine));

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 1, availableInventory.PickLines.Count());
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());

			var newPickLine = pickLine.Split(7m);
			AssertCollectionContains(newPickLine, newPickLine.DocketLine.PickLines);
			AssertEquals(2, availableInventory.PickLines.Count());
			AssertContainsExactElementsInAnyOrder(availableInventory.PickLines, new[] { pickLine, newPickLine });

			AssertEquals(2, pick.GetAllPickLines().Count());
			AssertContainsExactElementsInAnyOrder(pick.GetAllPickLines(), new[] { pickLine, newPickLine });

			AssertEquals(3m, pickLine.WZ_Units);
			AssertEquals(receive.Lines[0].PK, pickLine.WZ_WE_InventoryLine);
			AssertEquals(order.Lines[0].PK, pickLine.WZ_WE_TransactionLine);
			AssertEquals("XYZ", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("CAS", pickLine.WZ_F3_NKAllocatedPackType);
			AssertEquals(true, pickLine.WZ_IsPicking);
			AssertEquals("RED", getAttrib(pickLine));

			AssertEquals(7m, newPickLine.WZ_Units);
			AssertEquals(receive.Lines[0].PK, newPickLine.WZ_WE_InventoryLine);
			AssertEquals(order.Lines[0].PK, newPickLine.WZ_WE_TransactionLine);
			AssertEquals("XYZ", newPickLine.WZ_GS_NKAssignedTo);
			AssertEquals("CAS", newPickLine.WZ_F3_NKAllocatedPackType);
			AssertEquals(true, newPickLine.WZ_IsPicking);
			AssertEquals("RED", getAttrib(newPickLine));

			void setAttrib(WhsPickLine line)
			{
				if (attributeNumber == AttributeNumber.One)
				{
					line.WZ_ReleaseCapturedPartAttrib1 = "RED";
				}
				else if (attributeNumber == AttributeNumber.Two)
				{
					line.WZ_ReleaseCapturedPartAttrib2 = "RED";
				}
				else if (attributeNumber == AttributeNumber.Three)
				{
					line.WZ_ReleaseCapturedPartAttrib3 = "RED";
				}
			}

			string getAttrib(WhsPickLine line)
			{
				var attrib = "";
				if (attributeNumber == AttributeNumber.One)
				{
					attrib = line.WZ_ReleaseCapturedPartAttrib1;
				}
				else if (attributeNumber == AttributeNumber.Two)
				{
					attrib = line.WZ_ReleaseCapturedPartAttrib2;
				}
				else if (attributeNumber == AttributeNumber.Three)
				{
					attrib = line.WZ_ReleaseCapturedPartAttrib3;
				}
				return attrib;
			}
		}

		#endregion

		#region TestSplit_ManagesReleaseLines

		public void TestSplit_ManagesReleaseLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "XYZ";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();

			AssertEquals("Precondition", 10m, pickLine.WZ_Units);
			AssertEquals("Precondition", 1, order.Lines[0].ReleaseLines.Count);

			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals(10m, releaseLine.Quantity);
			AssertEquals(0m, releaseLine.UnreleasedQty);

			var newPickLine = pickLine.Split(7m);
			AssertCollectionContains("Precondition", newPickLine, newPickLine.DocketLine.PickLines);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertContainsExactElementsInAnyOrder("Precondition", pick.GetAllPickLines(),
				new[] { pickLine, newPickLine });

			AssertEquals("Should not have altered Release Lines.", 1, order.Lines[0].ReleaseLines.Count);
			AssertEquals("Should not have altered Release Lines.", 10m, releaseLine.Quantity);
			AssertEquals("Should not have altered Release Lines.", 0m, releaseLine.UnreleasedQty);
		}

		#endregion

		#region TestSplit_WithReleaseCapturedAttribs

		public void TestSplit_WithReleaseCapturedAttribs()
		{
			// Based on Maciej's tests in PickLinePackAssigner
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 236m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition: Order Line is Fully Picked.", 236m, pick.GetAllPickLines().Single().WZ_Units);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Picked Stock is all on the same Inventory.", 236m,
				availableInventory.PickLineQuantity);

			var orderLine = order.Lines[0];
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			AssertEquals("PickLine should be fully release Captured.", 0m, pick.GetAllPickLines().Single().UnreleaseCapturedQty);

			Factory.Save();

			pick.GetAllPickLines().First().Split(200m);
			AssertEquals("Should have split Pick Lines.", 2, pick.GetAllPickLines().Count());
			AssertNotNull("Release Captured Attributes should be split correctly.", orderLine.PickLines.SingleOrDefault(
				pl => pl.WZ_Units == 200 && pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertNotNull("Release Captured Attributes should be split correctly.", orderLine.PickLines.SingleOrDefault(
				pl => pl.WZ_Units == 36 && pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertEquals("Order is still Fully Picked.", 236m, availableInventory.PickLineQuantity);
			AssertEquals("Release Line should be in Sync.", 236m, releaseLine.Quantity);
			AssertEquals("Release Line should be in Sync.", 0m, releaseLine.UnreleasedQty);

			// make sure Data is correct and validates and saves without issue
			releaseLine.Validation.ValidateAll();
			AssertNoErrors(releaseLine);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestSplit_WithReleaseCapturedAttribs_NotFullyReleased

		public void TestSplit_WithReleaseCapturedAttribs_NotFullyReleased()
		{
			// Based on Maciej's tests in PickLinePackAssigner
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 200m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition: Order Line is Fully Picked.", 200m, pick.GetAllPickLines().Single().WZ_Units);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Picked Stock is all on the same Inventory.", 200m,
				availableInventory.PickLineQuantity);

			var orderLine = order.Lines[0];
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			releaseLine.Quantity = 110m;
			AssertEquals("PickLine should not be fully release Captured.", 90m,
				pick.GetAllPickLines().Sum(pl => pl.UnreleaseCapturedQty));

			Factory.Save();

			pick.GetAllPickLines().Single(l => l.WZ_Units == 110m).Split(100m);
			AssertEquals("Should have split Pick Lines.", 3, pick.GetAllPickLines().Count());
			AssertNotNull("Release Captured Attributes should be split correctly amongst new PickLines.",
				orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 100 && pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));

			AssertNotNull("Release Captured Attributes should be split correctly amongst new PickLines.",
				orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 10 && pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));

			AssertEquals("Order is still Fully Picked.", 200m, availableInventory.PickLineQuantity);
			AssertEquals("Release Line should be in Sync.", 110m, releaseLine.Quantity);
			AssertEquals("Release Line should be in Sync.", 90m, releaseLine.UnreleasedQty);
			AssertEquals("PickLines should not be fully release Captured.", 90m,
				pick.GetAllPickLines().Sum(pl => pl.UnreleaseCapturedQty));

			// make sure Data is correct and validates and saves without issue
			releaseLine.RunPreSaveValidation();
			AssertHasError(releaseLine.UnreleasedQtyInfo,
				"Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");

			orderLine.ReleaseLines.AddNew().Quantity = 90m;
			AssertEquals("Should now be fully released.", 0m, releaseLine.UnreleasedQty);
			AssertEquals("PickLines should not be fully release Captured.", 90m,
				pick.GetAllPickLines().Sum(pl => pl.UnreleaseCapturedQty));

			// make sure Data is correct and validates and saves without issue
			releaseLine.RunPreSaveValidation();
			AssertNoErrors(releaseLine);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestSplit_WithMultipleReleaseCapturedAttribs

		public void TestSplit_WithMultipleReleaseCapturedAttribs()
		{
			// Based on Maciej's tests in PickLinePackAssigner
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 200m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition: Order Line is Fully Picked.", 200m, pick.GetAllPickLines().Single().WZ_Units);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Picked Stock is all on the same Inventory.", 200m,
				availableInventory.PickLineQuantity);

			var orderLine = order.Lines[0];
			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.Quantity = 20m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "GREEN";
			releaseLine2.Quantity = 15m;

			var pickLine = pick.GetAllPickLines().Single(pl => !pl.HasReleaseCapturedAttribs);
			AssertEquals("PickLines should not be fully release Captured.", 165m, pickLine.UnreleaseCapturedQty);

			Factory.Save();

			pickLine.Split(160m);
			AssertEquals("Should have 4 Pick Lines, 1 original, 2 from RCA splitting, 1 from calling Split()", 4, pick.GetAllPickLines().Count());
			AssertNotNull("Release Captured Attributes should be split correctly amongst new PickLines.",
				orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 15m && pl.WZ_ReleaseCapturedPartAttrib1 == "GREEN") == 1);
			AssertNotNull("Release Captured Attributes should be split correctly amongst new PickLines.",
				orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 20m && pl.WZ_ReleaseCapturedPartAttrib1 == "RED") == 1);

			AssertEquals("Order is still Fully Picked.", 200m, availableInventory.PickLineQuantity);
			AssertEquals("Release Line should be in Sync.", 20m, releaseLine1.Quantity);
			AssertEquals("Release Line should be in Sync.", 15m, releaseLine2.Quantity);
			AssertEquals("Release Line should be in Sync.", 165m, releaseLine1.UnreleasedQty);
			AssertEquals("Release Line should be in Sync.", 165m, releaseLine2.UnreleasedQty);

			AssertEquals("PickLines should not be fully release Captured.", 165m,
				pick.GetAllPickLines().Sum(pl => pl.UnreleaseCapturedQty));

			releaseLine1.RunPreSaveValidation();
			releaseLine2.RunPreSaveValidation();
			AssertHasError(releaseLine1.UnreleasedQtyInfo,
				"Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");
			AssertHasError(releaseLine2.UnreleasedQtyInfo,
				"Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");

			orderLine.ReleaseLines.AddNew().Quantity = 165m;
			AssertEquals("Should now be fully released.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should now be fully released.", 0m, releaseLine2.UnreleasedQty);
			AssertEquals("PickLines should not be fully release Captured.", 165m,
				pick.GetAllPickLines().Sum(pl => pl.UnreleaseCapturedQty));

			// make sure Data is correct and validates and saves without issue
			releaseLine1.RunPreSaveValidation();
			releaseLine2.RunPreSaveValidation();
			AssertNoErrors(releaseLine1);
			AssertNoErrors(releaseLine2);

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestSplit_ThrowsExceptionsForExceptionalCases

		public void TestSplit_ThrowsExceptionsForExceptionalCases()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLinePart1 = pick.GetAllPickLines().Single(pl => pl.SupplierPart.PK == data.Part1.PK);

			var part1AvailInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.Single(oi => oi.SupplierPartPK == data.Part1.PK).AvailableInventories[0];
			var part2AvailInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.Single(oi => oi.SupplierPartPK == data.Part2.PK).AvailableInventories[0];

			AssertExceptionThrown<InvalidOperationException>(() => pickLinePart1.Split(25m));
			AssertExceptionThrown<InvalidOperationException>(() => pickLinePart1.Split(10m));
		}

		#endregion

		#region TestSplit_SetsOriginalPickedInventory

		public void TestSplit_SetsOriginalPickedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var newPickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition.", originalInventory.PK, newPickLine.WZ_WE_OriginalPickedInventoryLine);

			var splitLine = newPickLine.Split(10m);
			AssertEquals("Should have cloned OriginalPickedInventoryLine.", originalInventory.PK,
				splitLine.WZ_WE_OriginalPickedInventoryLine);
		}

		#endregion

		#region TestSuspendReMerge

		public void TestSuspendReMerge()
		{
			var inventoryLine = Factory.New<WhsReceiveLine>();
			var order = Factory.New<WhsOrder>();
			var orderLine1 = order.Lines.AddNew();

			var pickLine1 = Factory.New<WhsPickLine>();
			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine1.WZ_Units = 4m;
			pickLine2.WZ_Units = 3m;
			pickLine1.WZ_F3_NKAllocatedPackType = "PLT";
			pickLine2.WZ_F3_NKAllocatedPackType = "PLT";
			pickLine1.WZ_GS_NKAssignedTo = "E";
			pickLine2.WZ_GS_NKAssignedTo = "E";
			pickLine1.WZ_WE_InventoryLine = inventoryLine.PK;
			pickLine2.WZ_WE_InventoryLine = inventoryLine.PK;

			pickLine1.WZ_WE_TransactionLine = orderLine1.PK;
			pickLine2.WZ_WE_TransactionLine = orderLine1.PK;
			orderLine1.PickLines.Add(pickLine2);

			IPackableItem item = pickLine1;

			using (pickLine1.SuspendReMerge())
			{
				using (pickLine1.SuspendReMerge())
				{
					item.ReMerge();
					AssertEquals("Nothing should have merged, no OrderLine on PickLine 1.", 3m, pickLine2.WZ_Units);
					AssertEquals("Nothing should have merged, PickLine 3 has Zero Units.", 4m, pickLine1.WZ_Units);
				}

				item.ReMerge();
				AssertEquals("Nothing should have merged, no OrderLine on PickLine 1.", 3m, pickLine2.WZ_Units);
				AssertEquals("Nothing should have merged, PickLine 3 has Zero Units.", 4m, pickLine1.WZ_Units);
			}

			item.ReMerge();
			AssertEquals("PickLine 1 should have merged.", 7m, pickLine2.WZ_Units);
			AssertEquals("PickLine 1 should have merged.", true, pickLine1.IsDeleted);
		}

		#endregion

		#region TestTransferQtyAcrossPickLines

		public void TestTransferQtyAcrossPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, data.Whs1.DefaultLocation,
				data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();

			var pickLine = transferLine.PickLines.Single();
			var newPickLine1 = transferLine.PickLines.AddNew();
			newPickLine1.WZ_WE_InventoryLine = pickLine.WZ_WE_InventoryLine;
			AssertExceptionThrown(typeof(ArgumentException),
				"Cannot transfer more than the PickLine has allocated.\r\nParameter name: qtyToMove",
				() => WhsPickLine.TransferQtyAcrossPickLines(newPickLine1, pickLine, 21m));
			AssertExceptionThrown(typeof(ArgumentException),
				"Cannot transfer negative Units.\r\nParameter name: qtyToMove",
				() => WhsPickLine.TransferQtyAcrossPickLines(newPickLine1, pickLine, -1m));
			AssertExceptionThrown(typeof(ArgumentException), "Cannot transfer zero Units.\r\nParameter name: qtyToMove",
				() => WhsPickLine.TransferQtyAcrossPickLines(newPickLine1, pickLine, 0m));

			WhsPickLine.TransferQtyAcrossPickLines(newPickLine1, pickLine, 5m);
			AssertEquals("Should have transferred 5 units to new PickLine.", 5m, newPickLine1.WZ_Units);
			AssertEquals("Should have transferred 5 units from original PickLine.", 15m, pickLine.WZ_Units);

			newPickLine1.Delete();
			transferLine.WE_TransactionQuantity = 15m; // to allow transferline to save
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			transferLine.WE_TransactionQuantity = 16m;
			var newPickLine2 = transferLine.PickLines.AddNew();
			newPickLine2.WZ_Units = 1m;
			newPickLine2.WZ_WE_TransactionLine = transferLine.PK;
			newPickLine2.WZ_WE_InventoryLine = pickLine.WZ_WE_InventoryLine;
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Can only transfer between PickLines that are either both finalised or both unfinalized.",
				() => WhsPickLine.TransferQtyAcrossPickLines(newPickLine2, pickLine, 5m));

			newPickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			WhsPickLine.TransferQtyAcrossPickLines(newPickLine2, pickLine, 5m);
			AssertEquals("Should have transferred 5 units to new PickLine.", 6m, newPickLine2.WZ_Units);
			AssertEquals("Should have transferred 5 units from original PickLine.", 10m, pickLine.WZ_Units);
		}

		#endregion

		#region TestTransferQtyAcrossPickLines_ValidatesInventoryFKs

		public void TestTransferQtyAcrossPickLines_ValidatesInventoryFKs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine1.PickLines.Single();
			pickLine.WZ_Units = 10m;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var newPickLine = orderLine1.PickLines.Single();
			AssertEquals("Precondition.", originalInventory.PK, newPickLine.WZ_WE_OriginalPickedInventoryLine);

			var otherReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m);
			var otherInventory = otherReceive.Lines[0];

			// Correct inventory, no OriginalPickedInventory
			var dodgyPickLine1 = Factory.New<WhsPickLine>();
			dodgyPickLine1.WZ_Units = 1m;
			dodgyPickLine1.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			dodgyPickLine1.WZ_WE_TransactionLine = orderLine1.PK;
			dodgyPickLine1.WZ_WE_InventoryLine = newPickLine.WZ_WE_InventoryLine;

			// Correct Inventory, incorrect OriginalPickedInventory
			var dodgyPickLine2 = Factory.New<WhsPickLine>();
			dodgyPickLine2.WZ_Units = 1m;
			dodgyPickLine2.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			dodgyPickLine2.WZ_WE_TransactionLine = orderLine1.PK;
			dodgyPickLine2.WZ_WE_InventoryLine = newPickLine.WZ_WE_InventoryLine;
			dodgyPickLine2.WZ_WE_OriginalPickedInventoryLine = otherInventory.PK;

			// Incorrect inventory, correct OriginalPickedInventory
			var dodgyPickLine3 = Factory.New<WhsPickLine>();
			dodgyPickLine3.WZ_Units = 1m;
			dodgyPickLine3.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			dodgyPickLine3.WZ_WE_TransactionLine = orderLine1.PK;
			dodgyPickLine3.WZ_WE_InventoryLine = otherInventory.PK;
			dodgyPickLine3.WZ_WE_OriginalPickedInventoryLine = newPickLine.WZ_WE_OriginalPickedInventoryLine;

			// Correct inventory, correct OriginalPickedInventory
			var matchingPickLine = Factory.New<WhsPickLine>();
			matchingPickLine.WZ_Units = 1m;
			matchingPickLine.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			matchingPickLine.WZ_WE_TransactionLine = orderLine1.PK;
			matchingPickLine.WZ_WE_InventoryLine = newPickLine.WZ_WE_InventoryLine;
			matchingPickLine.WZ_WE_OriginalPickedInventoryLine = newPickLine.WZ_WE_OriginalPickedInventoryLine;

			AssertExceptionThrown(typeof(ArgumentException),
				"Cannot transfer to pick lines for other OriginalPickedInventory.",
				() => WhsPickLine.TransferQtyAcrossPickLines(dodgyPickLine1, newPickLine, 1m));

			AssertExceptionThrown(typeof(ArgumentException),
				"Cannot transfer to pick lines for other OriginalPickedInventory.",
				() => WhsPickLine.TransferQtyAcrossPickLines(dodgyPickLine2, newPickLine, 1m));

			AssertExceptionThrown(typeof(ArgumentException), "Cannot transfer to pick lines for other Inventory.",
				() => WhsPickLine.TransferQtyAcrossPickLines(dodgyPickLine3, newPickLine, 1m));

			AssertEquals("Precondition.", 10m, newPickLine.WZ_Units);
			WhsPickLine.TransferQtyAcrossPickLines(newPickLine, matchingPickLine, 1m);
			AssertEquals("Should have transferred 5 units to new PickLine.", 0m, matchingPickLine.WZ_Units);
			AssertEquals("Should have transferred 5 units from original PickLine.", 11m, newPickLine.WZ_Units);
		}

		#endregion

		#region TestTransferQtyAcrossPickLines_ChecksOriginalOrderLineFK

		public void TestTransferQtyAcrossPickLines_ChecksOriginalOrderLineFK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, data.Whs1.DefaultLocation,
				data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();

			var pickLine = transferLine.PickLines.Single();
			var newPickLine1 = transferLine.PickLines.AddNew();
			newPickLine1.WZ_WE_InventoryLine = pickLine.WZ_WE_InventoryLine;
			pickLine.WZ_WE_OriginalOrderLine = ZGuid.NewZGuid();
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Attempted to split pick line when already picked on Outbound Transfer.",
				() => WhsPickLine.TransferQtyAcrossPickLines(newPickLine1, pickLine, 5m));

			pickLine.WZ_WE_OriginalOrderLine = ZGuid.Empty;
			newPickLine1.WZ_WE_OriginalOrderLine = ZGuid.NewZGuid();
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Attempted to split pick line when already picked on Outbound Transfer.",
				() => WhsPickLine.TransferQtyAcrossPickLines(newPickLine1, pickLine, 5m));
		}

		#endregion

		#region TestOnSave_SetLastAllocatedOrChangedDate

		[TestDate(2018, 12, 12)]
		public void TestOnSave_SetLastAllocatedOrChangedDate()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			AssertEquals("Precondition: location1 should have a null WLV_LastAllocatedOrChangedDateUtc.",
				ZDateTime.Empty, location1.WLV_LastAllocatedOrChangedDateUtc);
			AssertEquals("Precondition: location2 should have a null WLV_LastAllocatedOrChangedDateUtc.",
				ZDateTime.Empty, location2.WLV_LastAllocatedOrChangedDateUtc);

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, location1, location2);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			var pickLine = transferLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = now.AddDays(1);
			TestDateAttribute.Date = now.AddDays(2).ToDateTime();
			Factory.Save();

			AssertEquals("LastAllocatedOrChangedDate should have updated.", now.AddDays(2).Date,
				location1.WLV_LastAllocatedOrChangedDateUtc);
		}

		[TestDate(2018, 12, 12)]
		public void TestOnSave_SetLastAllocatedOrChangedDate_WithDockDoorTransfer()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			AssertEquals("Precondition: location1 should have a null WLV_LastAllocatedOrChangedDateUtc.",
				ZDateTime.Empty, location1.WLV_LastAllocatedOrChangedDateUtc);
			AssertEquals("Precondition: location2 should have a null WLV_LastAllocatedOrChangedDateUtc.",
				ZDateTime.Empty, location2.WLV_LastAllocatedOrChangedDateUtc);

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, location1, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = now.AddDays(1);
			TestDateAttribute.Date = now.AddDays(2).ToDateTime();
			Factory.Save();

			AssertEquals("LastAllocatedOrChangedDate should have updated.", now.AddDays(2).Date,
				location1.WLV_LastAllocatedOrChangedDateUtc);
		}

		[TestDate(2018, 12, 12)]
		public void TestOnSave_SetLastAllocatedOrChangedDate_DoesNotGetSetWhenCloningPickedPickLines()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			AssertEquals("Precondition: location1 should have a null WLV_LastAllocatedOrChangedDateUtc.",
				ZDateTime.Empty, location1.WLV_LastAllocatedOrChangedDateUtc);
			AssertEquals("Precondition: location2 should have a null WLV_LastAllocatedOrChangedDateUtc.",
				ZDateTime.Empty, location2.WLV_LastAllocatedOrChangedDateUtc);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, location1, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = now;
			Factory.Save();

			location1.WLV_LastAllocatedOrChangedDateUtc = now.ToUtcDateTime().AddDays(-2);
			Factory.Save();

			var outboundTransferLinePickLine = pickLine.InventoryLine.PickLines.Single();
			var clonedPickLine = outboundTransferLinePickLine.Clone();

			using (((IWhsPickLineInternals)outboundTransferLinePickLine).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				outboundTransferLinePickLine.Delete();
			}

			Factory.Save();
			AssertEquals("LastAllocatedOrChangedDate should not have updated.", now.ToUtcDateTime().AddDays(-2), location1.WLV_LastAllocatedOrChangedDateUtc);
		}

		#endregion

		#region TestReserveInventory_Unsafe

		public void TestReserveInventory_Unsafe()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var reserveLine = WhsPickLine.ReserveInventory_Unsafe(Factory, inventory.PK, orderLine.PK, 10m);
			AssertNotNull("Reserve Line", reserveLine);
			AssertEquals("IsReserveLine is true", true, reserveLine.IsReserveLine);
			AssertEquals("Inventory Line PK is correct", inventory.PK, reserveLine.WZ_WE_InventoryLine);
			AssertEquals("Transaction Line PK is correct", orderLine.PK, reserveLine.WZ_WE_TransactionLine);
			AssertEquals("WZ_Units is correct", 10m, reserveLine.WZ_Units);
			AssertEquals("WZ_OriginalReservedQty is correct", 10m, reserveLine.WZ_OriginalReservedQty);
		}

		public void TestReserveInventory_Unsafe_InvalidInventoryPK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			AssertExceptionThrown<ArgumentException>("Should throw argument exception",
				() => WhsPickLine.ReserveInventory_Unsafe(Factory, ZGuid.Invalid, orderLine.PK, 10m));
		}

		public void TestReserveInventory_Unsafe_MissingOrderLinePK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Factory.Save();

			AssertExceptionThrown<ArgumentException>("Should throw argument exception",
				() => WhsPickLine.ReserveInventory_Unsafe(Factory, inventory.PK, ZGuid.Missing, 10m));
		}

		public void TestReserveInventory_Unsafe_NullFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertExceptionThrown<ArgumentNullException>("Should throw argument null exception",
				() => WhsPickLine.ReserveInventory_Unsafe(null, inventory.PK, orderLine.PK, 10m));
		}

		public void TestReserveInventory_Unsafe_UnitsToReserveZero()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertExceptionThrown<ArgumentException>("Should throw argument exception",
				() => WhsPickLine.ReserveInventory_Unsafe(Factory, inventory.PK, orderLine.PK, 0m));
		}

		public void TestReserveInventory_Unsafe_UnitsToReserveLessThanZero()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertExceptionThrown<ArgumentException>("Should throw argument exception",
				() => WhsPickLine.ReserveInventory_Unsafe(Factory, inventory.PK, orderLine.PK, -150m));
		}

		#endregion

		// interfaces

		#region TestICartonisableItemMembers

		public void TestICartonisableItemMembers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine1.PickLines.Single();
			AssertNotNull("Precondition", pickLine.SupplierPart);

			var pickLineAsICartonisableItem = (ICartonisableItem)pickLine;
			AssertEquals("PK", pickLine.PK, pickLineAsICartonisableItem.PK);
			AssertEquals("Quantity", 5m, pickLineAsICartonisableItem.Quantity);
			AssertEquals("Location", originalInventory.WE_WL, pickLineAsICartonisableItem.LocationPK);
			AssertEquals("ItemDefinition", WhsProduct.GetWhsProduct(pickLine.SupplierPart),
				pickLineAsICartonisableItem.ItemDefinition);
		}

		public void TestICartonisableItemMembers_PickByBOMKitLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine1.PickLines.Single();
			AssertNotNull("Precondition", pickLine.SupplierPart);

			var pickLineAsICartonisableItem = (ICartonisableItem)pickLine;
			AssertEquals("PK", pickLine.PK, pickLineAsICartonisableItem.PK);
			AssertEquals("Quantity", 5m, pickLineAsICartonisableItem.Quantity);
			AssertEquals("Location", ZGuid.Empty, pickLineAsICartonisableItem.LocationPK);
			AssertEquals("ItemDefinition", WhsProduct.GetWhsProduct(pickLine.SupplierPart), pickLineAsICartonisableItem.ItemDefinition);
		}

		#endregion

		#region IEmptyLocationAfterPickFinalisation Members

		public void TestIsLocationEmptyAfterFinalisingPick()
		{
			var pickLine = Factory.New<WhsPickLine>();
			var line = ((IEmptyLocationAfterPickFinalisation)pickLine);

			line.IsLocationEmptyAfterFinalisingPick = true;
			AssertEquals(true, line.IsLocationEmptyAfterFinalisingPick);

			line.IsLocationEmptyAfterFinalisingPick = false;
			AssertEquals(false, line.IsLocationEmptyAfterFinalisingPick);
		}

		public void TestIEmptyLocationAfterPickFinalisation_Inventory()
		{
			var inventoryLine1 = Factory.New<WhsReceiveLine>();
			var inventoryLine2 = Factory.New<WhsReceiveLine>();

			var pickLine = Factory.New<WhsPickLine>();
			IEmptyLocationAfterPickFinalisation line = pickLine;
			AssertNull(line.Inventory);

			pickLine.WZ_WE_InventoryLine = inventoryLine1.PK;
			AssertEquals(inventoryLine1, line.Inventory);

			pickLine.WZ_WE_OriginalPickedInventoryLine = inventoryLine2.PK;
			AssertEquals(inventoryLine2, line.Inventory);
		}

		public void TestIEmptyLocationAfterPickFinalisation_AllocatedQty_IsPicked_NotInDb()
		{
			var pickLine = Factory.New<WhsPickLine>();
			var line = (IEmptyLocationAfterPickFinalisation)pickLine;
			pickLine.WZ_Units = 5m;

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("AllocatedQty is correct", 5m, line.AllocatedQty);
		}

		public void TestIEmptyLocationAfterPickFinalisation_AllocatedQty_IsPicked_InDb()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location = data.Whs1.FindLocation("A-1-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: PickLines", 1, pickLines.Count());
			var pickline = pickLines.First();
			pickline.WZ_GS_NKAssignedTo = "E";
			pickline.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			AssertEquals("AllocatedQty is correct", 0m, ((IEmptyLocationAfterPickFinalisation)pickline).AllocatedQty);
		}

		#endregion

		#region IPackableItemMembers

		#region TestIPackableItem_Key

		[TestDate(2019, 1, 1)]
		public void TestIPackableItem_Key()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, setReleaseCaptured: true,
				useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, setReleaseCaptured: true,
				useSerialNumber: false);

			var dayOne = ZDate.Today.AddDays(1);
			var dayTwo = ZDate.Today.AddDays(2);
			var dayThree = ZDate.Today.AddDays(3);
			var dayFour = ZDate.Today.AddDays(4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, dayOne, dayTwo, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, dayOne, dayTwo, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, dayThree, dayFour, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, dayOne, dayTwo, "", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			IPackableItem orderLine1PickLine1 =
				orderLine1.PickLines.First(p => p.InventoryLine.WE_ExpiryDate == dayOne);
			IPackableItem orderLine1PickLine2 =
				orderLine1.PickLines.Last(p => p.InventoryLine.WE_ExpiryDate == dayOne);
			IPackableItem orderLine1PickLine3 =
				orderLine1.PickLines.Single(p => p.InventoryLine.WE_ExpiryDate == dayThree);
			IPackableItem orderLine2PickLine = orderLine2.PickLines.Single();

			// pickLine 1 and 2 are equal
			AssertEquals("PickLine 1 and 2 have the same Inventory, should be equal.", orderLine1PickLine2.Key,
				orderLine1PickLine1.Key);
			AssertEquals("PickLine 1 and 2 have the same Inventory, should be equal.", true,
				orderLine1PickLine1.Key.Equals(orderLine1PickLine2.Key));
			AssertEquals("PickLine 1 and 2 have the same Inventory, should be equal.", true,
				orderLine1PickLine1.Key == orderLine1PickLine2.Key);
			AssertEquals("PickLine 1 and 2 have the same Inventory, should be equal.",
				orderLine1PickLine2.Key.GetHashCode(), orderLine1PickLine1.Key.GetHashCode());

			// pickLine 3 and 1 are not equal
			AssertNotEquals("PickLine 1 and 3 have different Expiry and Packing Dates, should not be equal.",
				orderLine1PickLine3.Key, orderLine1PickLine1.Key);
			AssertEquals("PickLine 1 and 3 have different Expiry and Packing Dates, should not be equal.", false,
				orderLine1PickLine1.Key.Equals(orderLine1PickLine3.Key));
			AssertEquals("PickLine 1 and 3 have different Expiry and Packing Dates, should not be equal.", false,
				orderLine1PickLine1.Key == orderLine1PickLine3.Key);

			// pickLine 1 and pickLine on orderLine 2 are not equal even though they have the same attributes
			AssertNotEquals("PickLine 1 and 4 have different Products, should not be equal.", orderLine2PickLine.Key,
				orderLine1PickLine1.Key);
			AssertEquals("PickLine 1 and 4 have different Products, should not be equal.", false,
				orderLine1PickLine1.Key.Equals(orderLine2PickLine.Key));
			AssertEquals("PickLine 1 and 4 have different Products, should not be equal.", false,
				orderLine1PickLine1.Key == orderLine2PickLine.Key);
		}

		#endregion

		#region TestIPackableItem_Quantity

		public void TestIPackableItem_Quantity()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 10m;
			IPackableItem item = pickLine;
			AssertEquals(nameof(item.Quantity), 10m, item.Quantity);

			AssertEquals("10 should be available no matter Release Captured or not.", 10m, item.Quantity);

			pickLine.WZ_ReleaseCapturedPartAttrib1 = "RED";
			AssertEquals("10 should be available no matter Release Captured or not.", 10m, item.Quantity);
		}

		#endregion

		#region TestIPackableItem_ReMerge

		public void TestIPackableItem_ReMerge()
		{
			var inventoryLine = Factory.New<WhsReceiveLine>();
			var order = Factory.New<WhsOrder>();
			var orderLine1 = order.Lines.AddNew();

			var pickLine1 = Factory.New<WhsPickLine>();
			var pickLine2 = Factory.New<WhsPickLine>();
			var pickLine3 = Factory.New<WhsPickLine>();
			pickLine1.WZ_Units = 4m;
			pickLine2.WZ_Units = 3m;
			pickLine1.WZ_F3_NKAllocatedPackType = "PLT";
			pickLine2.WZ_F3_NKAllocatedPackType = "PLT";
			pickLine3.WZ_F3_NKAllocatedPackType = "PLT";
			pickLine1.WZ_GS_NKAssignedTo = "E";
			pickLine2.WZ_GS_NKAssignedTo = "E";
			pickLine3.WZ_GS_NKAssignedTo = "E";
			pickLine1.WZ_WE_InventoryLine = inventoryLine.PK;
			pickLine2.WZ_WE_InventoryLine = inventoryLine.PK;
			pickLine3.WZ_WE_InventoryLine = inventoryLine.PK;

			pickLine2.WZ_WE_TransactionLine = orderLine1.PK;
			pickLine3.WZ_WE_TransactionLine = orderLine1.PK;
			orderLine1.PickLines.Add(pickLine2);
			orderLine1.PickLines.Add(pickLine3);

			IPackableItem item1 = pickLine1;
			IPackableItem item2 = pickLine3;
			item1.ReMerge();
			AssertEquals("Nothing should have merged, no OrderLine on PickLine 1.", 3m, pickLine2.WZ_Units);
			AssertEquals("Nothing should have merged, PickLine 3 has Zero Units.", 0m, pickLine3.WZ_Units);

			var orderLine2 = order.Lines.AddNew();
			pickLine1.WZ_WE_TransactionLine = orderLine2.PK;
			item1.ReMerge();
			AssertEquals("Nothing should have merged, wrong OrderLine on PickLine 1.", 3m, pickLine2.WZ_Units);

			pickLine1.WZ_WE_TransactionLine = orderLine1.PK;
			pickLine1.WZ_F3_NKAllocatedPackType = "BOX";
			item1.ReMerge();
			AssertEquals("Nothing should have merged, wrong Pack Type on PickLine 1.", 3m, pickLine2.WZ_Units);
			pickLine1.WZ_F3_NKAllocatedPackType = "PLT"; // clean up

			pickLine1.WZ_GS_NKAssignedTo = "XXX";
			item1.ReMerge();
			AssertEquals("Nothing should have merged, wrong Staff on PickLine 1.", 3m, pickLine2.WZ_Units);
			pickLine1.WZ_GS_NKAssignedTo = "E"; // clean up

			pickLine1.WZ_WE_InventoryLine = ZGuid.Empty;
			item1.ReMerge();
			AssertEquals("Nothing should have merged, wrong Inventory on PickLine 1.", 3m, pickLine2.WZ_Units);
			pickLine1.WZ_WE_InventoryLine = inventoryLine.PK; // clean up

			((IBusinessObjectInternals)pickLine2).Row[WhsPickLineSchema.Constants.WZ_PickedDateTime] = DateTimeOffset.Now;
			item1.ReMerge();
			AssertEquals("Nothing should have merged, PickLine 2 has a Time set.", 3m, pickLine2.WZ_Units);
			((IBusinessObjectInternals)pickLine2).Row[WhsPickLineSchema.Constants.WZ_PickedDateTime] = DBNull.Value; // clean up

			var divot = Factory.New<PkgPackageItemDivot>();
			divot.KI_ParentID = pickLine2.PK;
			item1.ReMerge();
			AssertEquals("Nothing should have merged, PickLine 2 is Packed.", 3m, pickLine2.WZ_Units);
			divot.Delete(); // clean up

			item1.ReMerge();
			AssertEquals("PickLine 1 should have merged.", 7m, pickLine2.WZ_Units);
			AssertEquals("PickLine 1 should have merged.", true, pickLine1.IsDeleted);

			pickLine3.WZ_Units = 6m;
			pickLine3.WZ_ReleaseCapturedPartAttrib1 = "RED";
			item2.ReMerge();
			AssertEquals("PickLine 3 should have not been merged as it has RCAs.", 7m, pickLine2.WZ_Units);
			AssertEquals("PickLine 3 should have not been merged as it has RCAs.", 6m, pickLine3.WZ_Units);
			AssertEquals("PickLine 3 should have not been deleted as it has RCAs.", false, pickLine3.IsDeleted);

			pickLine2.WZ_ReleaseCapturedPartAttrib1 = "RED";
			item2.ReMerge();
			AssertEquals("PickLine 1 should have merged with PickLine 2.", 13m, pickLine2.WZ_Units);
			AssertEquals("PickLine 3 should have been deleted.", true, pickLine3.IsDeleted);
		}

		#endregion

		#region TestIPackableItem_ReMerge_ChecksOriginalPickedInventory

		public void TestIPackableItem_ReMerge_ChecksOriginalPickedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var otherReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			var originalInventory = receive.Lines[0];
			var otherInventory = otherReceive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_Units = 10m;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var newPickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition.", originalInventory.PK, newPickLine.WZ_WE_OriginalPickedInventoryLine);

			// Correct inventory, no OriginalPickedInventory
			var dodgyPickLine1 = Factory.New<WhsPickLine>();
			dodgyPickLine1.WZ_Units = 5m;
			dodgyPickLine1.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			dodgyPickLine1.WZ_WE_TransactionLine = newPickLine.WZ_WE_TransactionLine;
			dodgyPickLine1.WZ_WE_InventoryLine = newPickLine.WZ_WE_InventoryLine;

			// Correct Inventory, incorrect OriginalPickedInventory
			var dodgyPickLine2 = Factory.New<WhsPickLine>();
			dodgyPickLine2.WZ_Units = 5m;
			dodgyPickLine2.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			dodgyPickLine2.WZ_WE_TransactionLine = newPickLine.WZ_WE_TransactionLine;
			dodgyPickLine2.WZ_WE_InventoryLine = newPickLine.WZ_WE_InventoryLine;
			dodgyPickLine2.WZ_WE_OriginalPickedInventoryLine = otherInventory.PK;

			// Incorrect inventory, correct OriginalPickedInventory
			var dodgyPickLine3 = Factory.New<WhsPickLine>();
			dodgyPickLine3.WZ_Units = 5m;
			dodgyPickLine3.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			dodgyPickLine3.WZ_WE_TransactionLine = newPickLine.WZ_WE_TransactionLine;
			dodgyPickLine3.WZ_WE_InventoryLine = otherInventory.PK;
			dodgyPickLine3.WZ_WE_OriginalPickedInventoryLine = newPickLine.WZ_WE_OriginalPickedInventoryLine;

			// Correct inventory, correct OriginalPickedInventory
			var matchingPickLine = Factory.New<WhsPickLine>();
			matchingPickLine.WZ_Units = 5m;
			matchingPickLine.WZ_GS_NKAssignedTo = newPickLine.WZ_GS_NKAssignedTo;
			matchingPickLine.WZ_WE_TransactionLine = newPickLine.WZ_WE_TransactionLine;
			matchingPickLine.WZ_WE_InventoryLine = newPickLine.WZ_WE_InventoryLine;
			matchingPickLine.WZ_WE_OriginalPickedInventoryLine = newPickLine.WZ_WE_OriginalPickedInventoryLine;

			AssertEquals("Precondition.", 5, orderLine.PickLines.Count);
			AssertEquals("Precondition", 10m, newPickLine.WZ_Units);
			AssertEquals("Precondition", 5m, matchingPickLine.WZ_Units);

			IPackableItem item1 = newPickLine;
			IPackableItem item2 = dodgyPickLine1;
			IPackableItem item3 = dodgyPickLine2;
			IPackableItem item4 = dodgyPickLine3;
			item1.ReMerge();
			item2.ReMerge();
			item3.ReMerge();
			item4.ReMerge();
			AssertEquals("Should have merged.", true, newPickLine.IsDeleted);
			AssertEquals("Should have merged the pickline with correct inventory and OriginalPickedInventory.", 15m,
				matchingPickLine.WZ_Units);
			AssertEquals("Should *not* have merged other picklines.", false, dodgyPickLine1.IsDeleted);
			AssertEquals("Should *not* have merged other picklines.", false, dodgyPickLine2.IsDeleted);
			AssertEquals("Should *not* have merged other picklines.", false, dodgyPickLine3.IsDeleted);
			AssertEquals("Should *not* have merged other picklines.", 5m, dodgyPickLine1.WZ_Units);
			AssertEquals("Should *not* have merged other picklines.", 5m, dodgyPickLine2.WZ_Units);
			AssertEquals("Should *not* have merged other picklines.", 5m, dodgyPickLine3.WZ_Units);
		}

		#endregion

		#region TestIPackableItem_ReMerge_AttemptToMergeFinalisedPicklines

		public void TestIPackableItem_ReMerge_AttemptToMergeFinalisedPicklines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine.PickLines.Single();
			pickLine1.WZ_Units = 10m;
			pickLine1.WZ_GS_NKAssignedTo = "AAA";
			pickLine1.WZ_F3_NKAllocatedPackType = "CAS";

			var pickLine2 = pickLine1.Split(3m);
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine3 = pickLine1.Split(5m);
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Now;

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition", 2m, pickLine1.WZ_Units);
			AssertEquals("Precondition", 3m, pickLine2.WZ_Units);
			AssertEquals("Precondition", 5m, pickLine3.WZ_Units);
			AssertEquals("Precondition", 3, orderLine.PickLines.Count);
			AssertEquals("Precondition", true,
				orderLine.PickLines.All(line =>
					line.WZ_WE_OriginalPickedInventoryLine == pickLine1.WZ_WE_OriginalPickedInventoryLine));
			AssertEquals("Precondition", true,
				orderLine.PickLines.All(line => line.WZ_F3_NKAllocatedPackType.EqualsIgnoringCase("CAS")));
			AssertEquals("Precondition", true,
				orderLine.PickLines.All(line => line.WZ_GS_NKAssignedTo.EqualsIgnoringCase("AAA")));
			AssertEquals("Precondition", true, orderLine.PickLines.All(line => line.IsUnpacked(Factory)));
			AssertEquals("Precondition", true, orderLine.PickLines.All(line => line.WZ_Units > 0m));
			AssertEquals("Precondition: Pickline1 is not finalised.", false, pickLine1.IsFinalised);
			AssertIsFinalisedPrecondition(pickLine2);
			AssertIsFinalisedPrecondition(pickLine3);

			IPackableItem item1 = pickLine1;
			IPackableItem item2 = pickLine2;
			IPackableItem item3 = pickLine3;

			AssertNoExceptionThrown("There should be no exception.", () => item1.ReMerge());
			AssertNoExceptionThrown("There should be no exception.", () => item2.ReMerge());
			AssertNoExceptionThrown("There should be no exception.", () => item3.ReMerge());
			AssertEquals("Should not have merged with other picklines.", false, pickLine1.IsDeleted);
			AssertEquals("Should not have merged with other picklines.", false, pickLine2.IsDeleted);
			AssertEquals("Should not have merged with other picklines.", false, pickLine3.IsDeleted);
			AssertEquals("Should not have merged with other picklines.", 2m, pickLine1.WZ_Units);
			AssertEquals("Should not have merged with other picklines.", 3m, pickLine2.WZ_Units);
			AssertEquals("Should not have merged with other picklines.", 5m, pickLine3.WZ_Units);
		}

		#endregion

		#region TestOutboundDockDoorTransferCreator_MoreThanOneInTransitTransfer

		public void TestOutboundDockDoorTransferCreator_MoreThanOneInTransitTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			Factory.Save();
			AssertEquals("Precondition: No In-Transit Transfer.", 0, pick.Transfers.Count);

			var pickLine2 = pickLine.Split(3m);
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: In-Transit Transfer created.", 1, pick.Transfers.Count);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_WP_ParentPickForTransfer = pick.PK;
			Factory.Save();

			var pickLine3 = pickLine.Split(3m);
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown("Should be able to save we we have more than one In-Transit transfer.",
				Factory.Save);
		}

		#endregion

		#region TestOutboundDockDoorTransferCreator_UpdatesRelatedTaskField

		public void TestOutboundDockDoorTransferCreator_UpdatesRelatedTaskField()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			Factory.Save();

			var orderPickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: No In-Transit Transfer.", 0, pick.Transfers.Count);
			AssertEquals("Precondition: PickLine has task FK.", task.PK, orderPickLine.WZ_P9_Task);

			var orderPickLine2 = orderPickLine.Split(3m);
			AssertEquals("Precondition: No In-Transit Transfer.", 0, pick.Transfers.Count);
			AssertEquals("Precondition: PickLine has task FK.", task.PK, orderPickLine2.WZ_P9_Task);
			Factory.Save();

			orderPickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("In-Transit Transfer created.", 1, pick.Transfers.Count);
			AssertEquals("Unpicked PickLine has task FK.", task.PK, orderPickLine.WZ_P9_Task);
			AssertEquals("Picked PickLine has no task FK.", ZGuid.Empty, orderPickLine2.WZ_P9_Task);
			var transferLinePickLine = pick.Transfers[0].Lines[0].PickLines.Single();
			AssertEquals("Transfer Line PickLine has task FK.", task.PK, transferLinePickLine.WZ_P9_Task);
		}

		#endregion

		#region TestOutboundDockDoorTransferCreator_CanDirectlyPutawayComponent

		public void TestOutboundDockDoorTransferCreator_CanDirectlyPutawayComponent()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 2m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 1m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var componentOrderLine = orderLine.ChildComponentLines.Single();
			var componentPickLine = componentOrderLine.PickLines.Single();
			componentPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			AssertEquals("The Component Line hasn't been picked, so transfer line will still be created.", 1, transfer.Lines.Count);
			var transferLine = transfer.Lines.Single();
			AssertEquals("In-Transit Transfer Line was created.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var thePickLine = componentOrderLine.PickLines.Single();
			AssertEquals("Picked from putaway location.", true, thePickLine.WZ_WE_OriginalPickedInventoryLine.IsValid);
			transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			AssertEquals("No more Transfer Lines created.", 1, transfer.Lines.Count);
		}

		#endregion

		#region TestIPackableItem_Split

		public void TestIPackableItem_Split()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.Single(o => o.SupplierPart == data.Part1).AvailableInventories[0];
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			AssertContainsExactElementsInAnyOrder("Precondition: Available Inventory contains PickLine.",
				new[] { pickLine1 }, availableInventory.PickLines);

			IPackableItem item1 = pickLine1;
			pickLine1.WZ_Units = 4m;

			AssertExceptionThrown(typeof(ArgumentException), "Cannot Split more than is Available on the PickLine.",
				() => item1.Split(7m));
			pickLine1.WZ_Units = 10m; // clean up

			var releaseLine = orderLine1.ReleaseLines[0];
			IPackableItemParent packableItemParent = releaseLine;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, packableItemParent.PackableItems);

			var newPickLine = (WhsPickLine)item1.Split(7m);
			AssertEquals("Original PickLine should have Quantity Reduced.", 3m, pickLine1.WZ_Units);
			AssertEquals("New PickLine should have Split Quantity.", 7m, newPickLine.WZ_Units);
			AssertEquals("New PickLine should be a Clone.", pickLine1.WZ_WE_InventoryLine,
				newPickLine.WZ_WE_InventoryLine);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1, newPickLine }, availableInventory.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1, newPickLine }, packableItemParent.PackableItems);
		}

		#endregion

		#region TestOutboundDockDoorTransferCreator_CreateSerialNumber

		public void TestOutboundDockDoorTransferCreator_CreateSerialNumber()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R01");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = "SN0";
				var pivots = new List<WhsSerialNumberPivot>(10);
				for (var i = 0; i < 10; i++)
				{
					var pivot = receiveLine.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{i}";
					pivots.Add(pivot);
				}

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_PickedDateTime = DateTime.Now;
				Factory.Save();

				var transfer = pick.Transfers.Single();
				var transferLine = transfer.Lines.Single();
				var transferPickLine = transferLine.PickLines.Single();

				var receivePivots = LoadPivots(receiveLine.PK);
				AssertEquals(10, receivePivots.Length);
				foreach (var pivot in receivePivots)
				{
					AssertEquals(pivot, receiveLine.SerialNumbers.Single(s => s.WSV_WSN_SerialNumber.Equals(pivot.WSV_WSN_SerialNumber)));
					AssertEquals("It should pubulate WZ_PickingLine.", pivot.WSV_WZ_PickingLine, transferPickLine.PK);
				}

				var transferPivots = LoadPivots(transferLine.PK);
				AssertEquals(10, transferPivots.Length);
				foreach (var newPivot in transferPivots)
				{
					AssertNotEquals("Should create new one.", newPivot.PK, receiveLine.SerialNumbers.Single(rp => rp.WSV_WSN_SerialNumber.Equals(newPivot.WSV_WSN_SerialNumber)).PK);
				}
			}

			WhsSerialNumberPivot[] LoadPivots(ZGuid parentID)
			{
				var query = new ZQuery(WhsSerialNumberPivotSchema.WSV_ParentID, parentID);
				return Factory.Load<WhsSerialNumberPivot>(query);
			}
		}

		#endregion

		#endregion

		#region IReducibleItem members

		public void TestIReducibleItem_CanBeReduced()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			AssertEquals("Precondition", ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);
			AssertEquals("Should not be able to reduce when is picked.", false,
				((IReducibleItem)pickLine).CanBeReduced);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals(true, ((IReducibleItem)pickLine).CanBeReduced);

			var package = order.PackageJob.Packages.AddNew();
			PackPickLine(package, pickLine);
			AssertEquals("Precondition", false, pickLine.IsUnpacked(Factory));
			AssertEquals("Should not be able to reduce when is packed.", false,
				((IReducibleItem)pickLine).CanBeReduced);
		}

		static void PackPickLine(PkgPackage package, WhsPickLine pickLine)
		{
			var divot = package.PackedItemDivots.AddNew();
			divot.KI_ParentTableCode = WhsPickLineSchema.Constants.Prefix;
			divot.KI_ParentID = pickLine.PK;
			divot.KI_PackedQty = pickLine.WZ_Units;
		}

		public void TestIReducibleItem_CanBeReduced_InTransit()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals(false, ((IReducibleItem)pickLine).CanBeReduced);

			pickLine.WZ_WE_OriginalPickedInventoryLine = ZGuid.NewZGuid();
			AssertEquals(true, ((IReducibleItem)pickLine).CanBeReduced);
		}

		public void TestIReducibleItem_AttemptToReduceMoreStockThanPickedResultsInError()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 4;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var recorder = new Mock<IPickedStockAdjuster>();
			AssertExceptionThrown(typeof(ArgumentException),
				"quantityToLose must not be greater than WZ_Units.",
				() => ((IReducibleItem)pickLine).ReduceStock(5m, recorder.Object));
		}

		public void TestIReducibleItem_AttemptToReduceSameStockAsPicked_NoError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var receiveLine = receive.Lines[0];
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 4;
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var recorder = new Mock<IPickedStockAdjuster>();
			AssertNoExceptionThrown("Should be able to reduce Stock by 4.",
				() => ((IReducibleItem)pickLine).ReduceStock(4m, recorder.Object));
		}

		public void TestIReducibleItem_AttemptToReduceMoreStockThanPickedResultsInError_SomeReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var receiveLine = receive.Lines[0];
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 4;
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var recorder = new Mock<IPickedStockAdjuster>();
			AssertNoExceptionThrown("Should be able to reduce Stock by 4.",
				() => ((IReducibleItem)pickLine).ReduceStock(4m, recorder.Object));

			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine2.WZ_Units = 4;
			pickLine2.WZ_WE_InventoryLine = receiveLine.PK;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;

			AssertNoExceptionThrown(
				"Should be able to reduce Stock by 1, as this is unreleased quantity (no release captured attributes).",
				() => ((IReducibleItem)pickLine2).ReduceStock(1m, recorder.Object));
		}

		public void TestIReducibleItem_AttemptToReportNonPositiveStockLostResultsInError()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 4;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var recorder = new Mock<IPickedStockAdjuster>();
			AssertExceptionThrown(typeof(ArgumentException), "quantityToLose must be positive.",
				() => ((IReducibleItem)pickLine).ReduceStock(0m, recorder.Object));
			AssertExceptionThrown(typeof(ArgumentException), "quantityToLose must be positive.",
				() => ((IReducibleItem)pickLine).ReduceStock(-1m, recorder.Object));
		}

		public void TestIReducibleItem_RecorderMustBePassedToReduceStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines[0];

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 4;
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertExceptionThrown<ArgumentNullException>(() => ((IReducibleItem)pickLine).ReduceStock(1m, null));

			var recorder = new Mock<IPickedStockAdjuster>();
			AssertNoExceptionThrown(() => ((IReducibleItem)pickLine).ReduceStock(1m, recorder.Object));
		}

		public void TestIReducibleItem_ItemMustBeAbleToBeReduced()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 4;
			var recorder = new Mock<IPickedStockAdjuster>();
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Should not try to Reduce Stock when the PickLine is not Picked or does not have Unpacked item.",
				() => ((IReducibleItem)pickLine).ReduceStock(4m, recorder.Object));
		}

		public void TestIReducibleItem_ReducingStockReducesWZ_UnitsAndIncreasesTotalUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines[0];

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 4;
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Stock is reduced.", 6m, receiveLine.WE_StockOnHand);

			var recorder = new Mock<IPickedStockAdjuster>();

			var result = ((IReducibleItem)pickLine).ReduceStock(1m, recorder.Object);
			AssertEquals("ReduceStock() should succeed.", true, result.IsSuccess);
			AssertEquals(3m, pickLine.WZ_Units);
			AssertEquals("Stock should be increased by amount Lost.", 7m, receiveLine.WE_StockOnHand);

			var inventory = receiveLine.Inventory[0];
			recorder.Verify(r => r.AdjustOutInventory(inventory, inventory.PK, 1m));
		}

		public void TestIReducibleItem_ReducingStock_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is in-transit with stock on hand.", 10m,
				transferLine.WE_StockOnHand);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 10m;
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = transferLine.PickLines.Single().WZ_WE_InventoryLine;

			var recorder = new Mock<IPickedStockAdjuster>();
			var result = ((IReducibleItem)pickLine).ReduceStock(1m, recorder.Object);
			AssertEquals("ReduceStock() should succeed.", true, result.IsSuccess);
			AssertEquals(9m, pickLine.WZ_Units);
			AssertEquals("Should have split 1 Unit off Transfer Line.", 9m, transferLine.WE_StockOnHand);
			AssertEquals("Should have split 1 Unit off Transfer Line.", 9m, transferLine.WE_TransactionQuantity);
			AssertEquals("Original Transfer Line should still be In-Transit.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);

			var transferLinePickLine = transferLine.PickLines.Single();
			AssertEquals("Should have reduced Picked Amount by 1 unit.", 9m, transferLinePickLine.WZ_Units);
			AssertEquals("Should still be picked.", true, transferLinePickLine.IsPicked);

			var newTransferLine = transfer.Lines.Single(l => l != transferLine);
			AssertEquals("Split transfer Line should have the Quantity reduced.", 1m,
				newTransferLine.WE_TransactionQuantity);
			AssertEquals("Split transfer Line should have the Quantity reduced.", 1m, newTransferLine.WE_StockOnHand);
			AssertEquals("Split transfer Line should be finalised.", true, newTransferLine.IsFinalised);

			var newTransferLinePickLine = newTransferLine.PickLines.Single();
			AssertEquals("New Transfer Line Should have picked the Quantity reduced.", 1m,
				newTransferLinePickLine.WZ_Units);
			AssertEquals("New TransferLine should be picked.", true, newTransferLinePickLine.IsPicked);
			AssertEquals("New TransferLine should pick the same Inventory as the Original Transfer Line.",
				transferLinePickLine.WZ_WE_InventoryLine, newTransferLinePickLine.WZ_WE_InventoryLine);
			var inventory = newTransferLine.Inventory[0];
			recorder.Verify(r => r.AdjustOutInventory(inventory, pickLine.WZ_WE_OriginalPickedInventoryLine, 1m));
		}

		public void TestIReducibleItem_ReducingStock_InTransit_MatchingLine()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 5m,
				location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 5m,
				location1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transferLine.PickedTime = ZDateTimeOffset.Now;

			var matchingLine = transferLine.MatchingLines.Single();
			AssertEquals("Precondition: Transfer Line is in-transit with stock on hand.", 5m,
				transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Matching Line is in-transit with stock on hand.", 5m,
				matchingLine.WE_StockOnHand);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 5m;
			pickLine.WZ_WE_InventoryLine = matchingLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = matchingLine.PickLines.Single().WZ_WE_InventoryLine;

			var recorder = new Mock<IPickedStockAdjuster>();
			var result = ((IReducibleItem)pickLine).ReduceStock(1m, recorder.Object);
			AssertEquals("ReduceStock() should succeed.", true, result.IsSuccess);
			AssertEquals(4m, pickLine.WZ_Units);
			AssertEquals("Stock should not change on Main Transfer Line.", 5m, transferLine.WE_StockOnHand);
			AssertEquals("Should have split 1 Unit off Transfer Line Group.", 9m,
				transferLine.QtyToMoveIncludingMatchingLines);
			AssertEquals("Original Transfer Lines should still be In-Transit.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have split 1 Unit off Matching Line.", 4m, matchingLine.WE_StockOnHand);
			AssertEquals("Should have split 1 Unit off Matching Line.", 4m, matchingLine.WE_TransactionQuantity);
			AssertEquals("Original Transfer Lines should still be In-Transit.", InventoryStatus.Codes.InTransit,
				matchingLine.WE_CurrentInventoryStatus);

			var matchingLinePickLine = matchingLine.PickLines.Single();
			AssertEquals("Should have reduced Picked Amount by 1 unit.", 4m, matchingLinePickLine.WZ_Units);
			AssertEquals("Should still be picked.", true, matchingLinePickLine.IsPicked);

			var newTransferLine = transfer.Lines.Single(l => l != transferLine);
			AssertEquals("Split transfer Line should have the Quantity reduced.", 1m,
				newTransferLine.WE_TransactionQuantity);
			AssertEquals("Split transfer Line should have the Quantity reduced.", 1m, newTransferLine.WE_StockOnHand);
			AssertEquals("Split transfer Line should be finalised.", true, newTransferLine.IsFinalised);

			var newTransferLinePickLine = newTransferLine.PickLines.Single();
			AssertEquals("New Transfer Line Should have picked the Quantity reduced.", 1m,
				newTransferLinePickLine.WZ_Units);
			AssertEquals("New TransferLine should be picked.", true, newTransferLinePickLine.IsPicked);
			AssertEquals("New TransferLine should pick the same Inventory as the Original Transfer Line.",
				matchingLinePickLine.WZ_WE_InventoryLine, newTransferLinePickLine.WZ_WE_InventoryLine);
			var inventory = newTransferLine.Inventory[0];
			recorder.Verify(r => r.AdjustOutInventory(inventory, pickLine.WZ_WE_OriginalPickedInventoryLine, 1m));
		}

		public void TestIReducibleItem_ReducingStock_InTransitAndFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is in-transit with stock on hand.", 10m,
				transferLine.WE_StockOnHand);

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 10m;
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = transferLine.PickLines.Single().WZ_WE_InventoryLine;

			var recorder = new Mock<IPickedStockAdjuster>();
			var result = ((IReducibleItem)pickLine).ReduceStock(1m, recorder.Object);
			AssertEquals("ReduceStock() should succeed.", true, result.IsSuccess);
			AssertEquals(9m, pickLine.WZ_Units);
			AssertEquals("Should not have changed Stock on Hand.", 10m, transferLine.WE_StockOnHand);
			AssertEquals("Should not have changed Transaction Qty.", 10m, transferLine.WE_TransactionQuantity);
			AssertEquals("Original Transfer Line should still be Finalised.", true, transferLine.IsFinalised);

			var transferLinePickLine = transferLine.PickLines.Single();
			AssertEquals("Original PickLine should be untouched.", 10m, transferLinePickLine.WZ_Units);
			AssertEquals("Original PickLine should be untouched.", true, transferLinePickLine.IsPicked);

			var inventory = transferLine.Inventory[0];
			recorder.Verify(r => r.AdjustOutInventory(inventory, pickLine.WZ_WE_OriginalPickedInventoryLine, 1m));
		}

		public void TestIReducibleItem_ReducingStockPartially_InTransit_TransferLineFinalisationFails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is in-transit with stock on hand.", 10m,
				transferLine.WE_StockOnHand);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 10m;
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = transferLine.PickLines.Single().WZ_WE_InventoryLine;

			var originalPickLine = transferLine.PickLines.Single();
			EventHandler handler = null;
			handler = (sender, e) =>
			{
				originalPickLine.WZ_UnitsInfo.ValueChanged -= handler;

				// hack finalisation to fail.
				var newTransferLine = transfer.Lines.Single(l => l != transferLine);
				newTransferLine.WE_CurrentInventoryStatusInfo.ValueChanged +=
					(x, y) => newTransferLine.AddRowError("Test");
			};

			originalPickLine.WZ_UnitsInfo.ValueChanged += handler;

			var recorder = new Mock<IPickedStockAdjuster>();
			var result = ((IReducibleItem)pickLine).ReduceStock(1m, recorder.Object);
			AssertEquals("Should have failed to Reduce Stock.", false, result.IsSuccess);
			AssertEquals("Should have correct Error Message.", "Error - Docket Line: Test", result.ErrorMessage);
			recorder.Verify(
				r => r.AdjustOutInventory(It.IsAny<WhsInventoryView>(), It.IsAny<ZGuid>(), It.IsAny<ZDecimal>()),
				Times.Never);
		}

		public void TestIReducibleItem_ReducingStockPartially_InTransit_NoDestinationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location, location);
			transferLine.LocationString = ""; // clear out destination location
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is in-transit with stock on hand.", 10m,
				transferLine.WE_StockOnHand);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 10m;
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = transferLine.PickLines.Single().WZ_WE_InventoryLine;

			var recorder = new Mock<IPickedStockAdjuster>();
			var result = ((IReducibleItem)pickLine).ReduceStock(1m, recorder.Object);
			AssertEquals("ReduceStock() should succeed.", true, result.IsSuccess);
			AssertEquals(9m, pickLine.WZ_Units);

			var newTransferLine = transfer.Lines.Single(l => l != transferLine);
			AssertEquals("Split transfer Line should have defaulted a Location.", location.PK, newTransferLine.WE_WL);
			AssertEquals("Split transfer Line should be finalised.", true, newTransferLine.IsFinalised);

			var inventory = newTransferLine.Inventory[0];
			recorder.Verify(r => r.AdjustOutInventory(inventory, pickLine.WZ_WE_OriginalPickedInventoryLine, 1m));
		}

		public void TestIReducibleItem_ReduceAllStock_InTransit_MatchingLine()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 5m,
				location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 5m,
				location1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transferLine.PickedTime = ZDateTimeOffset.Now;

			var matchingLine = transferLine.MatchingLines.Single();
			AssertEquals("Precondition: Matching Line is in-transit with stock on hand.", 5m,
				matchingLine.WE_StockOnHand);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 5m;
			pickLine.WZ_WE_InventoryLine = matchingLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = matchingLine.PickLines.Single().WZ_WE_InventoryLine;
			var originalPickedInventory = pickLine.WZ_WE_OriginalPickedInventoryLine;

			var recorder = new Mock<IPickedStockAdjuster>();
			var result = ((IReducibleItem)pickLine).ReduceStock(5m, recorder.Object);
			AssertEquals("ReduceStock() should succeed.", true, result.IsSuccess);
			Assert("As WZ_Units reduced to 0 pickLine must be deleted.", pickLine.IsDeleted);
			AssertEquals("Since whole Transfer Quantity is being reduced, we do not split the Quantity.", 5m,
				matchingLine.WE_StockOnHand);
			AssertEquals("Since whole Transfer Quantity is being reduced, we do not split the Quantity.", 5m,
				matchingLine.WE_TransactionQuantity);
			AssertEquals("Since whole Transfer Quantity is being reduced, we Finalise the Transfer Line.", true,
				matchingLine.IsFinalised);
			AssertEquals(
				"Since whole Transfer Quantity is being reduced, we make the Transfer Line a Top level Transfer Line.",
				ZGuid.Empty, matchingLine.WE_WE_MatchingLine);

			var matchingLinePickLine = matchingLine.PickLines.Single();
			AssertEquals("Should not have changed the Pick Line.", 5m, matchingLinePickLine.WZ_Units);
			AssertEquals("Should still be picked.", true, matchingLinePickLine.IsPicked);

			var inventory = matchingLine.Inventory[0];
			recorder.Verify(r => r.AdjustOutInventory(inventory, originalPickedInventory, 5m));
		}

		public void TestIReducibleItem_ReduceAllStock_InTransit_NoDestinationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location, location);
			transferLine.LocationString = ""; // clear out destination location
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is in-transit with stock on hand.", 10m,
				transferLine.WE_StockOnHand);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 10m;
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = transferLine.PickLines.Single().WZ_WE_InventoryLine;

			var recorder = new Mock<IPickedStockAdjuster>();
			var result = ((IReducibleItem)pickLine).ReduceStock(10m, recorder.Object);
			AssertEquals("ReduceStock() should succeed.", true, result.IsSuccess);
			Assert("As WZ_Units reduced to 0 pickLine must be deleted.", pickLine.IsDeleted);
			AssertEquals("Transfer Line should have defaulted a Location.", location.PK, transferLine.WE_WL);
			AssertEquals("Transfer Line should be finalised.", true, transferLine.IsFinalised);

			var inventory = transferLine.Inventory[0];
			recorder.Verify(r => r.AdjustOutInventory(inventory, receiveLine.PK, 10m));
		}

		public void TestIReducibleItem_ReduceAllStock_InTransit_TransferLineFinalisationFails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is in-transit with stock on hand.", 10m,
				transferLine.WE_StockOnHand);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 10m;
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = transferLine.PickLines.Single().WZ_WE_InventoryLine;

			// hack finalisation to fail.
			transferLine.WE_CurrentInventoryStatusInfo.ValueChanged += (sender, e) => transferLine.AddRowError("Test");

			var recorder = new Mock<IPickedStockAdjuster>();
			var result = ((IReducibleItem)pickLine).ReduceStock(10m, recorder.Object);
			AssertEquals("Should have failed to Reduce Stock.", false, result.IsSuccess);
			AssertEquals("Should have correct Error Message.", "Error - Docket Line: Test\n" +
															   "Error - Docket Line: Error occurred during finalization. Close the form without saving and try again.",
				result.ErrorMessage);
			recorder.Verify(
				r => r.AdjustOutInventory(It.IsAny<WhsInventoryView>(), It.IsAny<ZGuid>(), It.IsAny<ZDecimal>()),
				Times.Never);
		}

		public void TestIReducibleItem_PickLineDeletedWhenAllStockReducedToZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines[0];
			Factory.Save();

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 4;
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var recorder = new Mock<IPickedStockAdjuster>();

			var result = ((IReducibleItem)pickLine).ReduceStock(4m, recorder.Object);
			AssertEquals("ReduceStock() should succeed.", true, result.IsSuccess);
			Assert("As WZ_Units reduced to 0 pickLine must be deleted.", pickLine.IsDeleted);
		}

		public void TestIReducibleItem_PickLineDeletedWhenAllStockReducedToZero_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is in-transit with stock on hand.", 10m,
				transferLine.WE_StockOnHand);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 10m;
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = transferLine.PickLines.Single().WZ_WE_InventoryLine;

			var recorder = new Mock<IPickedStockAdjuster>();
			var result = ((IReducibleItem)pickLine).ReduceStock(10m, recorder.Object);
			AssertEquals("ReduceStock() should succeed.", true, result.IsSuccess);
			Assert("As WZ_Units reduced to 0 pickLine must be deleted.", pickLine.IsDeleted);
			AssertEquals("Since whole Transfer Quantity is being reduced, we do not split the Quantity.", 10m,
				transferLine.WE_StockOnHand);
			AssertEquals("Since whole Transfer Quantity is being reduced, we do not split the Quantity.", 10m,
				transferLine.WE_TransactionQuantity);
			AssertEquals("Since whole Transfer Quantity is being reduced, we Finalise the Transfer Line.", true,
				transferLine.IsFinalised);
			AssertContainsExactElementsInAnyOrder("Should not add any new Transfer Lines.", new[] { transferLine },
				transfer.Lines);

			var transferLinePickLine = transferLine.PickLines.Single();
			AssertEquals("Should not have changed the Pick Line.", 10m, transferLinePickLine.WZ_Units);
			AssertEquals("Should still be picked.", true, transferLinePickLine.IsPicked);

			var inventory = transferLine.Inventory[0];
			recorder.Verify(r => r.AdjustOutInventory(inventory, receiveLine.PK, 10m));
		}

		#endregion

		#region TestISyncWithDB_SafeDeleteForBizOAlreadyDeletedInDatabase

		public void TestISyncWithDB_SafeDeleteForBizOAlreadyDeletedInDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 15m, data.Whs1.DefaultLocation,
				data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();

			var pickLine = transferLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertIsFinalisedPrecondition(pickLine);

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickLineInOtherFactory = otherFactory.Load<WhsPickLine>(pickLine.PK);
			var clonedPickLine =
				pickLineInOtherFactory.Clone(); // the transfer line must always have the correct pick line sum.

			using (((IWhsPickLineInternals)pickLineInOtherFactory)
				   .TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				pickLineInOtherFactory.Delete();
				AssertEquals("PickLine was deleted in other Factory.", true, pickLineInOtherFactory.IsDeleted);
			}

			otherFactory.Save();

			AssertNoExceptionThrown(((ISyncWithDB)pickLine).SafeDeleteForBizOAlreadyDeletedInDatabase_DoNotUse);
			AssertEquals("PickLine was deleted.", true, pickLine.IsDeleted);
		}

		#endregion

		#region TestISyncWithDB_SafeDeleteForBizOAlreadyDeletedInDatabase_InTransit

		public void TestISyncWithDB_SafeDeleteForBizOAlreadyDeletedInDatabase_InTransit()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 15m);
			helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: PickLine is finalised.", true, pickLine.IsPickedFromPutawayLocation);

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickLineInOtherFactory = otherFactory.Load<WhsPickLine>(pickLine.PK);
			pickLineInOtherFactory.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
			pickLineInOtherFactory.Delete();
			AssertEquals("PickLine was deleted in other Factory.", true, pickLineInOtherFactory.IsDeleted);

			otherFactory.Save();
			AssertNoExceptionThrown(((ISyncWithDB)pickLine).SafeDeleteForBizOAlreadyDeletedInDatabase_DoNotUse);
			AssertEquals("PickLine was deleted.", true, pickLine.IsDeleted);
		}

		#endregion

		//

		#region Validation

		public void TestIsValidationEnabled()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals("WZ_Units should have validation enabled if the pick line is not a reserved line.", true,
				pickLine.IsValidationEnabled(pickLine.WZ_UnitsInfo));
			AssertEquals(
				"ReservedQuantity should *not* have validation enabled if the pick line is not a reserved line.", false,
				pickLine.IsValidationEnabled(pickLine.ReservedQuantityInfo));

			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			pickLine.WZ_WE_TransactionLine = orderLine.PK;
			pickLine.IsReserveLine = true;
			AssertEquals("WZ_Units should *not* have validation enabled if the pick line is a reserved line.", false,
				pickLine.IsValidationEnabled(pickLine.WZ_UnitsInfo));
			AssertEquals("ReservedQuantity should have validation enabled if the pick line is a reserved line.", true,
				pickLine.IsValidationEnabled(pickLine.ReservedQuantityInfo));

			order.WD_WP = Factory.New<WhsPick>().PK;
			AssertEquals(
				"WZ_Units should have validation enabled if the pick line is a reserved line but it is picked.", true,
				pickLine.IsValidationEnabled(pickLine.WZ_UnitsInfo));
			AssertEquals(
				"ReservedQuantity should *not* have validation enabled if the pick line is a reserved line but it is picked.",
				false, pickLine.IsValidationEnabled(pickLine.ReservedQuantityInfo));
		}

		public void TestValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals(typeof(WhsPickLineValidation), pickLine.Validation.GetType());

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			pickLine.WZ_WE_TransactionLine = orderLine.PK;
			AssertEquals(typeof(WhsPickLineValidation), pickLine.Validation.GetType());

			pickLine.IsReserveLine = true;
			AssertEquals(typeof(WhsPickLineValidationUS), pickLine.Validation.GetType());

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals(typeof(WhsPickLineValidation), pickLine.Validation.GetType());
		}

		#endregion

		#region TestPickLineDeletedByDataRefresh

		public void TestPickLineDeletedByDataRefresh()
		{
			if (Globals.IsWeb)
			{
				// DataRefreshManager is not Enabled when Globals.IsWeb = true
				Assert(true);
			}
			else
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				order.WD_RequiredDate = ZDateTimeOffset.Now;
				order.ConsigneePK = data.Org1.PK;

				var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 10m);

				Helper.CreatePickNew(order);
				var pickLine = orderLine.PickLines.Single();
				Assert("Precondition", orderLine.ReleaseLines.Count > 0);
				Factory.Save();

				var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
				var orderLineInNewFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
				Assert("Precondition", orderLineInNewFactory.ReleaseLines.Count > 0);

				pickLine.Delete();
				AssertNoExceptionThrown(() => Factory.Save());

				AssertEquals(false, orderLineInNewFactory.ReleaseLines.Count > 0);
			}
		}

		public void TestPickLineDeletedByDataRefresh_PickLineToRefreshDeleted()
		{
			if (Globals.IsWeb)
			{
				// DataRefreshManager is not Enabled when Globals.IsWeb = true
				Assert(true);
			}
			else
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				order.WD_RequiredDate = ZDateTimeOffset.Now;
				order.ConsigneePK = data.Org1.PK;

				var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 10m);

				Helper.CreatePickNew(order);
				Factory.Save();

				var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
				var pickLine = orderLine.PickLines.Single();
				var pickLineInNewFactory = newFactory.Load<WhsPickLine>(pickLine.PK);
				pickLineInNewFactory.BeforeDeleteByDataRefresh += (s, e) =>
				{
					pickLineInNewFactory.Delete();
				};

				pickLine.Delete();
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		#endregion

		#region TestReleaseCapturedAttribs

		public void TestHasReleaseCapturedAttribs_Attrib1()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 10m;
			releaseLine.PartAttribute1 = "attrib1";

			AssertEquals("attrib1", pickLine.WZ_ReleaseCapturedPartAttrib1);
			Assert("WZ_ReleaseCapturedPartAttrib1 is not null, HasCapturedAttribs should be true.", pickLine.HasReleaseCapturedAttribs);
		}

		public void TestHasReleaseCapturedAttribs_Attrib2()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 10m;
			releaseLine.PartAttribute2 = "attrib2";

			AssertEquals("attrib2", pickLine.WZ_ReleaseCapturedPartAttrib2);
			Assert("WZ_ReleaseCapturedPartAttrib2 is not null, HasCapturedAttribs should be true.", pickLine.HasReleaseCapturedAttribs);
		}

		public void TestHasReleaseCapturedAttribs_Attrib3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 10m;
			releaseLine.PartAttribute3 = "attrib3";

			AssertEquals("attrib3", pickLine.WZ_ReleaseCapturedPartAttrib3);
			Assert("WZ_ReleaseCapturedPartAttrib3 is not null, HasCapturedAttribs should be true.", pickLine.HasReleaseCapturedAttribs);
		}

		public void TestHasReleaseCapturedAttribs_Serial()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 1m;
			releaseLine.SerialNumber = "SerialNumber";

			AssertEquals("SerialNumber", pickLine.WZ_ReleaseCapturedSerialNumber);
			Assert("WZ_ReleaseCapturedSerialNumber is not null, HasCapturedAttribs should be true.", pickLine.HasReleaseCapturedAttribs);
		}

		public void TestPickLineHasRCAs_AllEmpty()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_ReleaseCapturedPartAttrib1 = string.Empty;
			pickLine.WZ_ReleaseCapturedPartAttrib2 = string.Empty;
			pickLine.WZ_ReleaseCapturedPartAttrib3 = string.Empty;
			pickLine.WZ_ReleaseCapturedSerialNumber = string.Empty;
			AssertEquals("Attributes are all empty", false, pickLine.HasReleaseCapturedAttribs);
		}

		public void TestReleaseCapturedQty_Attrib1()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();

			AssertEquals("ReleaseCapturedPartAttrib is empty, ReleaseCapturedQty should be 0m.", 0m, pickLine.ReleaseCapturedQty);

			pickLine.WZ_ReleaseCapturedPartAttrib1 = "attrib1";

			AssertEquals("attrib1", pickLine.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("WZ_ReleaseCapturedPartAttrib1 is not null, ReleaseCapturedQty should be 10m.", 10m, pickLine.ReleaseCapturedQty);
		}

		public void TestReleaseCapturedQty_Attrib2()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();

			AssertEquals("ReleaseCapturedPartAttrib is empty, ReleaseCapturedQty should be 0m.", 0m, pickLine.ReleaseCapturedQty);

			pickLine.WZ_ReleaseCapturedPartAttrib2 = "attrib2";

			AssertEquals("attrib2", pickLine.WZ_ReleaseCapturedPartAttrib2);
			AssertEquals("WZ_ReleaseCapturedPartAttrib2 is not null, ReleaseCapturedQty should be 10m.", 10m, pickLine.ReleaseCapturedQty);
		}

		public void TestReleaseCapturedQty_Attrib3()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();

			AssertEquals("ReleaseCapturedPartAttrib is empty, ReleaseCapturedQty should be 0m.", 0m, pickLine.ReleaseCapturedQty);

			pickLine.WZ_ReleaseCapturedPartAttrib3 = "attrib3";

			AssertEquals("attrib3", pickLine.WZ_ReleaseCapturedPartAttrib3);
			AssertEquals("WZ_ReleaseCapturedPartAttrib3 is not null, ReleaseCapturedQty should be 10m.", 10m, pickLine.ReleaseCapturedQty);
		}

		public void TestReleaseCapturedQty_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();

			AssertEquals("ReleaseCapturedPartAttrib is empty, ReleaseCapturedQty should be 0m.", 0m, pickLine.ReleaseCapturedQty);

			pickLine.WZ_ReleaseCapturedSerialNumber = "serial";

			AssertEquals("serial", pickLine.WZ_ReleaseCapturedSerialNumber);
			AssertEquals("WZ_ReleaseCapturedSerialNumber is not null, ReleaseCapturedQty should be 10m.", 10m, pickLine.ReleaseCapturedQty);
		}

		public void TestUnreleaseCapturedQty_Attrib1()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();

			AssertEquals("ReleaseCapturedPartAttrib is empty, UnreleaseCapturedQty should be 10m.", 10m, pickLine.UnreleaseCapturedQty);

			pickLine.WZ_ReleaseCapturedPartAttrib1 = "attrib1";

			AssertEquals("attrib1", pickLine.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("WZ_ReleaseCapturedPartAttrib1 is not null, UnreleaseCapturedQty should be 0m.", 0m, pickLine.UnreleaseCapturedQty);
		}

		public void TestUnreleaseCapturedQty_Attrib2()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();

			AssertEquals("ReleaseCapturedPartAttrib is empty, UnreleaseCapturedQty should be 10m.", 10m, pickLine.UnreleaseCapturedQty);

			pickLine.WZ_ReleaseCapturedPartAttrib2 = "attrib2";

			AssertEquals("attrib2", pickLine.WZ_ReleaseCapturedPartAttrib2);
			AssertEquals("WZ_ReleaseCapturedPartAttrib2 is not null, UnreleaseCapturedQty should be 0m.", 0m, pickLine.UnreleaseCapturedQty);
		}

		public void TestUnreleaseCapturedQty_Attrib3()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();

			AssertEquals("ReleaseCapturedPartAttrib is empty, UnreleaseCapturedQty should be 10m.", 10m, pickLine.UnreleaseCapturedQty);

			pickLine.WZ_ReleaseCapturedPartAttrib3 = "attrib3";

			AssertEquals("attrib3", pickLine.WZ_ReleaseCapturedPartAttrib3);
			AssertEquals("WZ_ReleaseCapturedPartAttrib3 is not null, UnreleaseCapturedQty should be 0m.", 0m, pickLine.UnreleaseCapturedQty);
		}

		public void TestUnreleaseCapturedQty_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();

			AssertEquals("ReleaseCapturedPartAttrib is empty, UnreleaseCapturedQty should be 10m.", 10m, pickLine.UnreleaseCapturedQty);

			pickLine.WZ_ReleaseCapturedSerialNumber = "serialNumber";

			AssertEquals("serialNumber", pickLine.WZ_ReleaseCapturedSerialNumber);
			AssertEquals("WZ_ReleaseCapturedSerialNumber is not null, UnreleaseCapturedQty should be 0m.", 0m, pickLine.UnreleaseCapturedQty);
		}

		public void TestSetReleaseCapturedAttributes()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals(false, pickLine.HasReleaseCapturedAttribs);

			pickLine.SetReleaseCapturedAttributes("1", "2", "3", "s");
			AssertEquals(true, pickLine.HasReleaseCapturedAttribs);
			AssertEquals("1", pickLine.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("2", pickLine.WZ_ReleaseCapturedPartAttrib2);
			AssertEquals("3", pickLine.WZ_ReleaseCapturedPartAttrib3);
			AssertEquals("s", pickLine.WZ_ReleaseCapturedSerialNumber);
		}

		public void TestClearReleaseCapturedAttributes()
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.SetReleaseCapturedAttributes("1", "2", "3", "s");

			AssertEquals(true, pickLine.HasReleaseCapturedAttribs);

			pickLine.ClearReleaseCapturedAttributes();

			AssertEquals(false, pickLine.HasReleaseCapturedAttribs);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(factory);
			var helper = new WhsTestHelperFunctions(factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			helper.CreatePickNew(order);

			return orderLine.PickLines.Single();
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			return orderLine.ReserveStockIfAbleTo(inventory);
		}

		#region PickLine

		protected WhsPickLine PickLine
		{
			get
			{
				if (pickLine == null)
				{
					pickLine = (WhsPickLine)GetNewBusinessObject();
					pickLine.WZ_WE_InventoryLine = Inventory.WI_WE_InDocketLine;

					var order = Factory.New<WhsOrder>();
					order.WD_WP = Pick.PK;

					var orderLine = order.Lines.AddNew();
					pickLine.WZ_WE_TransactionLine = orderLine.PK;
				}

				return pickLine;
			}
		}

		WhsPickLine pickLine;

		#endregion

		#region Inventory

		protected WhsInventoryView Inventory => inventory ?? (inventory = Factory.NewWithValidTestData<WhsInventoryView>());
		WhsInventoryView inventory;

		#endregion

		#region Pick

		protected WhsPick Pick
		{
			get { return pick ?? (pick = Factory.New<WhsPick>()); }
		}

		WhsPick pick;

		#endregion

		#endregion
	}

	[TestedType(typeof(WhsPickLine))]
	class PreventOverCommitOfStockViaPickLineTest : DeferrableTriggerTestCase<WhsPickLine>
	{
		// cancelled orders

		#region TestTrigger_IgnoresCancelledDockets

		public void TestTrigger_IgnoresCancelledDockets()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			// order 7 units for order1 and then Cancel Order
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m,
				pickOption: WhsPickOption.Codes.Manual);
			var order1Line = order1.Lines[0];
			order1.CancelReactivateDocket();
			AssertEquals("Precondition: Order is Cancelled.", true, order1.IsCancelled);

			var dodgyPickLine =
				Helper.CreateWhsPickLine(order1Line, inventory,
					7m); // create dodgy pickline attached to cancelled Order.
			((IBusinessObjectInternals)dodgyPickLine).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 7m;
			Factory.Save();

			// create another order for 4 units, technically 10 units available, since the 7 'committed' units above are attached to a cancelled order.
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m);
			AssertNoExceptionThrown("Pick should not have any error.", () => Helper.CreatePickNew(order2));
			AssertEquals("Pick Save should have had no issues saving.", true, order2.IsInDatabase);
		}

		#endregion

		// picks

		#region TestTrigger_ConsidersOtherPicks

		public void TestTrigger_ConsidersOtherPicks()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// pick 7 units for order1
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m,
				WhsPickOption.Codes.Manual);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.AutoAllocateItemsWithMock();

			// create and pick order for 7 units in another factory, but *before* order 1 takes stock
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var helperNF = new WhsTestHelperFunctions(newFactory);
			var order2 = helperNF.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 7m,
				WhsPickOption.Codes.Manual);
			var pick2 = helperNF.CreatePickNew(order2);
			pick2.AutoAllocateItemsWithMock();

			// save the first order/pick
			Factory.Save();

			// save the second order/pick (trigger should prevent save)
			AssertTriggerPreventsSave(newFactory);
		}

		#endregion

		#region TestTrigger_ConsidersOtherPicks_DirectSQL

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTrigger_ConsidersOtherPicks_DirectSQL()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection1);
				var data = new TestDataSimpleEnvironment(factory);

				// create 10 inventory
				var helper = new WhsTestHelperFunctions(factory);
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				factory.Save();

				// pick 7 units for order1
				var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m,
					WhsPickOption.Codes.Manual);
				var pick1 = helper.CreatePickNew(order1);
				pick1.AutoAllocateItemsWithMock();

				// create and pick order for 7 units in another factory, but *before* order 1 takes stock
				var newFactory = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
				var helperNF = new WhsTestHelperFunctions(newFactory);
				var order2 = helperNF.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 7m,
					WhsPickOption.Codes.Manual);
				var pick2 = newFactory.New<WhsPick>();
				pick2.Orders.Add(order2);
				newFactory.Save();

				// save the first order/pick
				factory.Save();
				connection2.BeginTransaction();
				AssertEquals("Precondition: Connection is in Transaction.", true, connection2.IsInTransaction);

				var sqlInventoryLine = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine
					.ShallowLoadFromDB(TestConnection, w => w.PK == receive.Inventory[0].WI_WE_InDocketLine)[0];
				var sqlTransactionLine = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine
					.ShallowLoadFromDB(TestConnection, w => w.PK == order2.Lines[0].PK)[0];

				NUnit.Framework.Assert.That(() =>
					{
						new WhsPickLineDO(sqlInventoryLine, sqlTransactionLine, 7).Insert(connection2);
					}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), $"TriggerLikelyConcurrencyError: {WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID}", true), "Trigger should prevent Save.");

				AssertEquals("Transaction should be rolledback.", false, connection2.IsInTransaction);
			}
		}

		#endregion

		#region TestTrigger_ConsidersOtherPicks_IgnoresFinalisedPickLines

		public void TestTrigger_ConsidersOtherPicks_IgnoresFinalisedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// pick 7 units for order1 and finalise
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m);
			var pick1 = Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order1);
			Factory.Save(); // save after pick finalise
			AssertEquals("Precondition - Stock should be reduced to 3 units.", 3m,
				receive.Inventory[0].InDocketLine.WE_StockOnHand);

			// pick 3 units
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertNoExceptionThrown("Finalised picklines should not count towards the trigger's sum of pickline units.",
				() => Factory.Save());
		}

		#endregion

		#region TestTrigger_ConsidersExpectedQuantity

		#region TestTrigger_ConsidersExpectedQuantity_IgnoresFinalisedReceiveLine

		public void TestTrigger_ConsidersExpectedQuantity_IgnoresFinalisedReceiveLine()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);
			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 10m, allocateLocations: true,
				finalise: true);
			var receiveLine = receive.Lines[0];
			receiveLine.WE_ClientOrderedUnits = 10m;
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(client, whs, "A1");
			Helper.CreateWhsAdjustmentLine(adjustment, product, -5m, inventory.Location);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment);
			AssertEquals("Precondition - receiveLine.WE_TransactionQuantity", 10m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition - receiveLine.WE_ClientOrderedUnits", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition - receiveLine.WE_StockOnHand", 5m, receiveLine.WE_StockOnHand);
			AssertNoExceptionThrown("Can save without issue.", () => Factory.Save());

			var order = Helper.CreateWhsOrder(client, whs, "Order1");
			var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
			var pickLine = Helper.CreateReservePickLine(orderLine, inventory, 10m);
			AssertEquals("Receive line is finalised.", true, receiveLine.IsFinalised);
			AssertEquals("Pick line quantity > inventory stock on hand.", true,
				pickLine.WZ_Units > receiveLine.WE_StockOnHand);
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#region TestTrigger_ConsidersExpectedQuantity_SingleOrder

		public void TestTrigger_ConsidersExpectedQuantity_SinglePickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m, finalise: false);
			var receiveLine = receive.Lines[0];
			receiveLine.WE_TransactionQuantity = 5m;
			var inventory = receive.Inventory[0];
			AssertEquals("receiveLine.WE_TransactionQuantity", 5m, receiveLine.WE_TransactionQuantity);
			AssertEquals("receiveLine.WE_ClientOrderedUnits", 15m, receiveLine.WE_ClientOrderedUnits);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pickLine = Helper.CreateReservePickLine(orderLine, inventory, 5m);
			AssertEquals("Receive line is not finalised.", false, receiveLine.IsFinalised);
			AssertNoExceptionThrown("Can reserve 5 units for order1 and save without issue.", () => Factory.Save());

			pickLine.ReservedQuantity = 10m;
			AssertNoExceptionThrown("Can reserve 10 units for order1 if 15 units are expected.", () => Factory.Save());

			pickLine.ReservedQuantity = 20m;
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#region TestTrigger_ConsidersExpectedQuantity_MultipleOrders

		public void TestTrigger_ConsidersExpectedQuantity_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, finalise: false);
			var receiveLine = receive.Lines[0];
			receiveLine.WE_TransactionQuantity = 20m;
			var inventory = receive.Inventory[0];
			AssertEquals("receiveLine.WE_TransactionQuantity", 20m, receiveLine.WE_TransactionQuantity);
			AssertEquals("receiveLine.WE_StockOnHand", 20m, receiveLine.WE_StockOnHand);
			AssertEquals("receiveLine.WE_ClientOrderedUnits", 30m, receiveLine.WE_ClientOrderedUnits);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 25m);
			var pickLine1 = Helper.CreateReservePickLine(orderLine1, inventory, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			Helper.CreateReservePickLine(orderLine2, inventory, 5m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order3");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part1, 5m);
			Helper.CreateReservePickLine(orderLine3, inventory, 5m);

			AssertEquals("Receive line is not finalised.", false, receiveLine.IsFinalised);
			AssertEquals("Client ordered units is more than the stock on hand.", true,
				receiveLine.WE_ClientOrderedUnits > receiveLine.WE_StockOnHand);
			AssertNoExceptionThrown("Can reserve a total of 15 units from receive and save without issue.",
				() => Factory.Save());

			pickLine1.ReservedQuantity = 15m;
			AssertNoExceptionThrown("Can reserve a total of 25 units from receive if 30 units are expected.",
				() => Factory.Save());

			pickLine1.ReservedQuantity = 25m;
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#region TestTrigger_ConsidersExpectedQuantity_MultipleReceiveLines

		public void TestTrigger_ConsidersExpectedQuantity_MultipleReceiveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			receive1.Lines[0].WE_TransactionQuantity = 5m;
			var receive2 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, finalise: false);
			var receive3 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 5m, finalise: false);

			CombineAssertions(delegate
			{
				AssertEquals("receiveLine1.Lines[0].WE_TransactionQuantity", 5m,
					receive1.Lines[0].WE_TransactionQuantity);
				AssertEquals("receiveLine1.Lines[0]..WE_StockOnHand", 5m, receive1.Lines[0].WE_StockOnHand);
				AssertEquals("receiveLine1.Lines[0].WE_ClientOrderedUnits", 10m,
					receive1.Lines[0].WE_ClientOrderedUnits);
				AssertEquals("receiveLine2.Lines[0].WE_TransactionQuantity", 5m,
					receive2.Lines[0].WE_TransactionQuantity);
				AssertEquals("receiveLine2.Lines[0]..WE_StockOnHand", 5m, receive2.Lines[0].WE_StockOnHand);
				AssertEquals("receiveLine2.Lines[0].WE_ClientOrderedUnits", 5m,
					receive2.Lines[0].WE_ClientOrderedUnits);
				AssertEquals("receiveLine3.Lines[0].WE_TransactionQuantity", 5m,
					receive3.Lines[0].WE_TransactionQuantity);
				AssertEquals("receiveLine3.Lines[0]..WE_StockOnHand", 5m, receive3.Lines[0].WE_StockOnHand);
				AssertEquals("receiveLine3.Lines[0].WE_ClientOrderedUnits", 5m,
					receive3.Lines[0].WE_ClientOrderedUnits);
			});

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 25m);
			var pickLine1 = Helper.CreateReservePickLine(orderLine1, receive1.Inventory[0], 5m);
			Helper.CreateReservePickLine(orderLine1, receive2.Inventory[0], 5m);
			Helper.CreateReservePickLine(orderLine1, receive3.Inventory[0], 5m);
			AssertNoExceptionThrown("Can reserve 15 units between receive inventory and save without issue.",
				() => Factory.Save());

			pickLine1.ReservedQuantity = 10m;
			AssertNoExceptionThrown("Can reserve 20 units from receives if a total of 20 units are expected.",
				() => Factory.Save());

			pickLine1.ReservedQuantity = 15m;
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#region TestTrigger_ConsidersExpectedQuantity_TransactionQuantityGreaterThanExpectedQuantity

		public void TestTrigger_ConsidersExpectedQuantity_TransactionQuantityGreaterThanExpectedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, finalise: false);
			var receiveLine = receive.Lines[0];
			receiveLine.WE_TransactionQuantity = 15m;
			var inventory = receive.Inventory[0];
			AssertEquals("receiveLine.WE_TransactionQuantity", 15m, receiveLine.WE_TransactionQuantity);
			AssertEquals("receiveLine.WE_StockOnHand", 15m, receiveLine.WE_StockOnHand);
			AssertEquals("receiveLine.WE_ClientOrderedUnits", 5m, receiveLine.WE_ClientOrderedUnits);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pickLine = Helper.CreateReservePickLine(orderLine, inventory, 5m);
			AssertNoExceptionThrown("Can reserve 5 units for order1 and save without issue.", () => Factory.Save());

			pickLine.ReservedQuantity = 10m;
			AssertNoExceptionThrown("Can reserve 10 units for order1 as there's 15 stock on hand.",
				() => Factory.Save());

			pickLine.ReservedQuantity = 20m;
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#endregion

		#region TestTrigger_ConsidersOtherPicks_WhenPickedButNotFinalised

		public void TestTrigger_ConsidersOtherPicks_WhenPickedButNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// pick 7 units for order1 and pick
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m);
			var pick1 = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order1);
			order1.Lines[0].PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save(); // save after picking
			AssertEquals("Precondition - Stock should be reduced to 3 units.", 3m,
				receive.Inventory[0].InDocketLine.WE_StockOnHand);

			// pick 3 units
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertNoExceptionThrown("Picked picklines should not count towards the trigger's sum of pickline units.",
				() => Factory.Save());
		}

		#endregion

		#region TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_PickedDateTime

		public void TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_PickedDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.Lines[0].WE_ClientOrderedUnits = 0m;
			Factory.Save();

			// pick 6 units and save to DB
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 6m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);

			var pickLine = order.Lines[0].PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			// testing a specific case with Picked Time in the trigger
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
				AssertEquals("Precondition: Stock is reduced.", 4m, pickLine.InventoryLine.WE_StockOnHand);
			}

			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalized",
					   WhsPickLineSchema.Constants.TableName))
			{
				// change below will create imbalance in 2 places, suspend one check procedure to test other fails.
				WhsTestHelperFunctions.SuspendDatabaseProcForTest(
					$"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)",
					"0");

				using (((IWhsPickLineInternals)pickLine).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
				{
					pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
				}

				AssertTriggerPreventsSave(Factory);
			}
		}

		#endregion

		#region TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_PickedDateTime_InTransitLine

		public void TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_PickedDateTime_InTransitLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.Lines[0].WE_ClientOrderedUnits = 0m;
			Factory.Save();

			// pick 6 units and save to DB
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 6m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);

			var pickLine = order.Lines[0].PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: Stock is reduced.", 4m,
				pickLine.InventoryLineForAvailableInventory.WE_StockOnHand);

			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalized", WhsPickLineSchema.Constants.TableName))
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsPickLine_PreventChangingOriginalPickedInventory", WhsPickLineSchema.Constants.TableName)) // can't change WZ_WE_OriginalOrderLine once it's set
			{
				// change below will create imbalance in 2 places, suspend one check procedure to test other fails.
				WhsTestHelperFunctions.SuspendDatabaseProcForTest(
					$"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)",
					"0");

				var pickedPickLine = pickLine.InventoryLine.PickLines.Single();
				using (((IWhsPickLineInternals)pickedPickLine).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
				{
					pickedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
					pickedPickLine.WZ_WE_OriginalOrderLine = ZGuid.Empty; // to test trigger we need to set original order line to null
				}

				AssertTriggerPreventsSave(Factory);
			}
		}

		#endregion

		#region TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_PickedDateTime_DirectSQL

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_PickedDateTime_DirectSQL()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalized",
					   WhsPickLineSchema.Constants.TableName, connection))
			{
				var factory = new BusinessObjectFactory(connection);
				var data = new TestDataSimpleEnvironment(factory);

				// create 10 inventory
				var helper = new WhsTestHelperFunctions(factory);
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				receive.Lines[0].WE_ClientOrderedUnits = 0m;
				factory.Save();

				// pick 6 units and save to DB
				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 6m);
				helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);

				var pickLine = order.Lines[0].PickLines.Single();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				// testing a specific case with Picked Time in the trigger
				using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
				{
					factory.Save();
					AssertEquals("Precondition: Stock is reduced.", 4m, pickLine.InventoryLine.WE_StockOnHand);
				}

				// changes below will trigger multiple triggers, suspending balance checker to test other trigger.
				WhsTestHelperFunctions.SuspendDatabaseProcForTest(
					$"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)",
					"0", connection);

				NUnit.Framework.Assert.That(() =>
					{
						ExecuteSqlInTransaction(connection,
							WhsPickLineDO
								.UpdateWhere(pickLine.PK.ToGuid())
								.Set(l => l.WZ_PickedDateTime, null).AsSQL());
					}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), $"TriggerLikelyConcurrencyError: {WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID}", true), "Trigger should prevent Save.");
			}
		}

		#endregion

		#region TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_Units

		public void TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_Units()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// pick 1 unit for order1 and save to DB
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick1 = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order1);
			Factory.Save();

			// pick 7 units for order2 and save to DB
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 7m);
			var pick2 = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order2);
			Factory.Save();

			// increase order1's units from 1 to 4 (only 3 available: 1 committed to this order/pick, 2 uncommitted in inventory)
			order1.Lines[0].WE_TransactionQuantity = 4m;
			order1.Lines[0].PickLines[0].WZ_Units = 4;
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#region TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_Units_DirectSQL

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_Units_DirectSQL()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var data = new TestDataSimpleEnvironment(factory);

				// create 10 inventory
				var helper = new WhsTestHelperFunctions(factory);
				helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				factory.Save();

				// pick 1 unit for order1 and save to DB
				var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
				var pick1 = helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order1);
				factory.Save();

				// pick 7 units for order2 and save to DB
				var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 7m);
				var pick2 = helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order2);
				factory.Save();

				// increase order1's units from 1 to 4 (only 3 available: 1 committed to this order/pick, 2 uncommitted in inventory)
				NUnit.Framework.Assert.That(() =>
					{
						var pickLine = order1.Lines[0].PickLines[0];
						ExecuteSqlInTransaction(connection, $@"
UPDATE
	dbo.WhsDocketLine
SET
	WE_TransactionQuantity = 4,
	WE_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	WE_SystemLastEditUser = '~BP'
WHERE
	WE_PK = '{order1.Lines[0].PK}'

UPDATE
	dbo.WhsPickLine
SET
	WZ_Units = 4,
	WZ_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	WZ_SystemLastEditUser = '~BP'
WHERE
	WZ_PK = '{pickLine.PK}'
");
					}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), $"TriggerLikelyConcurrencyError: {WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID}", true), "Trigger should prevent Save.");
			}
		}

		#endregion

		#region TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_WE_InventoryLine

		[ExpectNoExceptions]
		public void TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_WE_InventoryLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// pick 5 units for the order and save to DB
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);

			// change to a different inventory record (which only has 1 unit available, but we are trying to take 5)
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m);
			Factory.Save();

			var pickLine = order.Lines[0].PickLines[0];
			pickLine.WZ_WE_InventoryLine = receive2.Inventory[0].WI_WE_InDocketLine;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID}", true), "Should have thrown an exception.");
		}

		#endregion

		#region TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_WE_InventoryLine_DirectSQL

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTrigger_ConsidersOtherPicks_OnColumnUpdate_WZ_WE_InventoryLine_DirectSQL()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var data = new TestDataSimpleEnvironment(factory);

				// create 10 inventory
				var helper = new WhsTestHelperFunctions(factory);
				helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				factory.Save();

				// pick 5 units for the order and save to DB
				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
				var pick = helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
				factory.Save();

				var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m);
				factory.Save();

				// change to a different inventory record (which only has 1 unit available, but we are trying to take 5)
				NUnit.Framework.Assert.That(() =>
					{
						var pickLine = order.Lines[0].PickLines[0];
						ExecuteSqlInTransaction(connection,
							WhsPickLineDO
								.UpdateWhere(pickLine.PK.ToGuid())
								.Set(l => l.WZ_WE_InventoryLine,
									receive2.Inventory[0].WI_WE_InDocketLine.ToSqlParameter())
								.AsSQL());
					}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), $"TriggerLikelyConcurrencyError: {WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID}", true), "Trigger should prevent Save.");
			}
		}

		#endregion

		// reserved stock

		#region TestTrigger_ConsidersReservedStock

		public void TestTrigger_ConsidersReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.Lines[0].WE_ClientOrderedUnits = 0m;
			var inventory = receive.Inventory[0];
			Factory.Save();

			// pick 7 units for order1 and finalise
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m);
			var pick1 = Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order1);
			Factory.Save(); // save after pick finalise
			AssertEquals("Precondition - Stock should be reduced to 3 units.", 3m, inventory.WI_TotalUnits);

			// reserve 3 units
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m);
			var order2Line = order2.Lines[0];
			var reservedPickLine = order2Line.ReserveStockIfAbleTo(inventory, 3m);
			AssertEquals("Precondition - 3 units should be reserved.", 3m, reservedPickLine.ReservedQuantity);
			reservedPickLine.ReservedQuantity = 4m; // over-reserve stock (only 3 available)
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		// adjustments

		#region TestTrigger_ConsidersAdjustments

		public void TestTrigger_ConsidersAdjustments()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -6m, inventory.Location);
			adjustmentLine.RunPreSaveValidation(); // commit inventory.
			AssertEquals("Precondition: Stock is committed.", 6m, adjustmentLine.CommittedQuantity);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var helperInNewFactory = new WhsTestHelperFunctions(newFactory);
			var orderInNewFactory = helperInNewFactory.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1",
				data.Part1, 5m, WhsPickOption.Codes.Manual);
			var pickInNewFactory = helperInNewFactory.CreatePickNew(orderInNewFactory);
			pickInNewFactory.AutoAllocateItemsWithMock();

			// save adjustment
			Factory.Save();

			// save order and pick -- should fail because pick is for 5 units, but only 4/10 are available (6/10 are committed to the Adjustment)
			AssertTriggerPreventsSave(newFactory);
		}

		#endregion

		#region TestTrigger_ConsidersAdjusments_AllowsPickingFromFinalisedAdjustments

		public void TestTrigger_ConsidersAdjusments_AllowsPickingFromFinalisedAdjustments()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, -5m, inventory.Location);
			adjustmentLine1.RunPreSaveValidation(); // commit inventory.
			AssertEquals("Precondition: Stock is committed.", 5m, adjustmentLine1.CommittedQuantity);

			// finalise 5 units (reduces first inventory's total units to 5, finalised transfer picklines should be ignored in pickline sum)
			adjustment1.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment1);

			var adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A2");
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, -5m, inventory.Location);
			adjustmentLine2.RunPreSaveValidation(); // commit inventory.
			AssertEquals("Precondition: Stock is committed.", 5m, adjustmentLine2.CommittedQuantity);
			AssertNoExceptionThrown("Trigger should ignore Finalised Adjustments (picklines).", () => Factory.Save());
		}

		#endregion

		// transfers

		#region TestTrigger_ConsidersTransferLines

		public void TestTrigger_ConsidersTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.Lines[0].WE_ClientOrderedUnits = 0m;
			var inventory = receive.Inventory[0];
			Factory.Save();

			// create transfer for 7 units
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, inventory.LocationString,
				inventory.LocationString);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.LocationString,
				inventory.LocationString);
			transfer.RunPreSaveValidation(); // this commits transfer changes

			// finalise 2 of 7 units
			transferLine1.FinaliseDocketLine();

			// create and pick an order in another factory for 4 units
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var helperNF = new WhsTestHelperFunctions(newFactory);
			var orderNF = helperNF.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m,
				WhsPickOption.Codes.Manual);
			var pickNF = helperNF.CreatePickNew(orderNF);
			pickNF.AutoAllocateItemsWithMock();

			// save transfer
			Factory.Save();

			// save order and pick -- should fail because pick is for 4 units, but only 3/10 are available (2/10 were finalised to another location/inventory, 5/10 are committed)
			AssertTriggerPreventsSave(newFactory);
		}

		#endregion

		#region TestTrigger_ConsidersTransferLines_AllowsPickingFromFinalisedTransferLines

		public void TestTrigger_ConsidersTransferLines_AllowsPickingFromFinalisedTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventoryLocation = receive.Inventory[0].LocationString;
			Factory.Save();

			// create transfer for all 10 units
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventoryLocation, inventoryLocation);
			var transferLine2 =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventoryLocation, inventoryLocation);
			transfer.RunPreSaveValidation(); // this commits transfer changes

			// finalise 5 units (reduces first inventory's total units to 5, finalised transfer picklines should be ignored in pickline sum)
			transferLine1.FinaliseDocketLine();
			AssertNoExceptionThrown("Trigger should ignore Finalised Transfer Lines (picklines).",
				() => Factory.Save());
		}

		#endregion

		#region TestTrigger_ConsidersTransferLines_WhenPickedButNotFinalised

		public void TestTrigger_ConsidersTransferLines_WhenPickedButNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create 10 inventory
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventoryLocation = receive.Inventory[0].LocationString;

			// create transfer for all 10 units
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventoryLocation, inventoryLocation);
			var transferLine2 =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventoryLocation, inventoryLocation);
			transfer.RunPreSaveValidation(); // this commits transfer changes

			// Pick 5 units (reduces first inventory's total units to 5, Picked transfer picklines should be ignored in pickline sum)
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown("Trigger should ignore Finalised Transfer Lines (picklines).",
				() => Factory.Save());
		}

		#endregion

		// putaway transfers

		#region TestTrigger_PreventsOverCommittingStock_PutawayTransfers

		public void TestTrigger_PreventsOverCommittingStock_PutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 2m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "A", 2m);
			transferLine.RunPreSaveValidation(); // Commit Stock
			transferLine.PickLines.Single().WZ_Units = 3m; // Try to overcommit stock

			// we have 2 triggers that fail for same bad data setup, check if one of them fails, as we can not be sure which will be run first.
			// the duplicity will be removed in the following WI's.
			var exception =
				AssertExceptionThrown<ZConcurrencyCheckFailureException>("Should not let you overcommit Putaway Transfers.", Factory.Save);
			var correctTriggerFailed =
				(exception.Message.Contains(WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID) ||
				 exception.Message.Contains(WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID));
			Assert("Should not let you overcommit Putaway Transfers.", correctTriggerFailed);
		}

		#endregion

		#region TestTrigger_PreventsOverCommittingStock_ConcurrentPutawayTransfers

		[ExpectNoExceptions]
		public void TestTrigger_PreventsOverCommittingStock_ConcurrentPutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, dockDoorLocation, "A");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "A", 1m);
			transferLine.RunPreSaveValidation(); // Commit Stock
			AssertEquals("Precondition: Stock is committed.", 1m, transferLine.QtyCommittedIncludingMatchingLines);

			var newFactory = new BusinessObjectFactory();
			var helperInNF = new WhsTestHelperFunctions(newFactory);
			var transferInNewFactory = helperInNF.CreateWhsTransfer(data.Org1.PK, data.Whs1.PK);
			transferInNewFactory.WD_IsPutawayTransfer = true;
			var part1InNewFactory = newFactory.Load<OrgSupplierPart>(data.Part1.PK);
			var dockDoorLocationInNewFactory = newFactory.Load<WhsLocation>(dockDoorLocation.PK);
			var nonDockDoorLocationInNewFactory = newFactory.Load<WhsLocation>(nonDockDoorLocation.PK);
			var transferLineInNewFactory = helperInNF.SetupTransferLineForDockDoorLocation(transferInNewFactory,
				part1InNewFactory, dockDoorLocationInNewFactory, nonDockDoorLocationInNewFactory, "A", 1m);
			transferLineInNewFactory.RunPreSaveValidation(); // Commit Stock
			AssertEquals("Precondition: Stock is committed.", 1m,
				transferLineInNewFactory.QtyCommittedIncludingMatchingLines);
			newFactory.Save();

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID}"), "Should not let you overcommit Putaway Transfers.");
		}

		#endregion

		// cross dock orders

		#region TestTrigger_ConsidersCrossDockOrders

		public void TestTrigger_ConsidersCrossDockOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			orderLine1.ReserveStockIfAbleTo(inventory);
			orderLine2.ReserveStockIfAbleTo(inventory);
			AssertNoExceptionThrown("Can reserve 7 units for order1 and save without issue.", () => Factory.Save());

			var order2 = helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			var orderLine3 = helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			Helper.CreateReservePickLine(orderLine3, inventory, 4m);
			AssertTriggerPreventsSave(Factory);
		}

		#endregion

		#region Implementation

		void AssertTriggerPreventsSave(BusinessObjectFactory factory)
		{
			var exceptionThrown = false;
			try
			{
				factory.Save();
			}
			catch (ZConcurrencyCheckFailureException e)
			{
				AssertEquals("Correct trigger exception message should display",
					$"TriggerLikelyConcurrencyError: {WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID}", e.Message);
				exceptionThrown = true;
			}

			AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}
	}

	class PreventOverCommitOfStockViaPickLineConcurrencyTest : TestCase
	{
		#region TestTriggerLocksInventory

		[UseSnapshotProtection]
		public void TestTriggerLocksInventory()
		{
			using (var conn1 = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn1);
				var data = new TestDataSimpleEnvironment(factory);
				var helper = new WhsTestHelperFunctions(factory);
				helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
				var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 6m);
				var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 6m);
				var order3 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part2, 10m);
				factory.Save(); // now we have inventory and order1 in database.

				helper.CreatePickNew(order1);
				helper.CreatePickNew(order2);
				helper.CreatePickNew(order3);

				var pickLine1 = order1.Lines[0].PickLines.Single();
				var pickLine2 = order2.Lines[0].PickLines.Single();
				var pickLine3 =
					order3.Lines[0].PickLines.Single(); // this pickline points to second inventory (for Part2)
				pickLine1.WZ_Units = 4m;
				pickLine2.WZ_Units = 4m;
				pickLine3.WZ_Units = 9m;
				factory.Save();
				AssertEquals("Precondition", 4m,
					WhsPickLineDO.ShallowLoadFromDB(conn1, pickLine1.PK.ToGuid()).WZ_Units);
				AssertEquals("Precondition", 4m,
					WhsPickLineDO.ShallowLoadFromDB(conn1, pickLine2.PK.ToGuid()).WZ_Units);
				AssertEquals("Precondition", 9m,
					WhsPickLineDO.ShallowLoadFromDB(conn1, pickLine3.PK.ToGuid()).WZ_Units);

				conn1.ExecuteNonQuery(
					"ALTER INDEX FK_RX__WZ_WE_InventoryLine_WZ_WE_TransactionLine_WZ_IsPicking ON WhsPickLine SET (ALLOW_PAGE_LOCKS = OFF)");
				conn1.ExecuteNonQuery(
					"ALTER INDEX NR_RX__WZ_PickedDateTime ON WhsPickLine SET (ALLOW_PAGE_LOCKS = OFF)");

				using (var conn2 = Db.NewExtraConnectionToMainDb())
				{
					conn2.BeginTransaction();
					WhsPickLineDO.UpdateWhere(pickLine1.PK.ToGuid()).Set(l => l.WZ_Units, 6).Post(conn2);
					conn2.CommitTransaction();

					var taskUpdateWhsPickline2 = new Task(() =>
					{
						using (var connT = Db.NewExtraConnectionToMainDb())
						using (var commandT = connT.Command(WhsPickLineDO.UpdateWhere(pickLine2.PK.ToGuid())
								   .Set(l => l.WZ_Units, 6).AsSQL()))
						{
							AssertExceptionThrown("Over-pick attempt.", typeof(SqlException),
								() => commandT.ExecuteNonQuery());
						}
					});

					taskUpdateWhsPickline2.Start();
					using (var conn3 = Db.NewExtraConnectionToMainDb())
					using (conn3.BeginTransactionWithManager())
					using (var command3 = conn3.Command(WhsPickLineDO.UpdateWhere(pickLine3.PK.ToGuid())
							   .Set(l => l.WZ_Units, 10).AsSQL()))
					{
						command3.CommandTimeout = 3;
						AssertNoExceptionThrown(
							"Only row lock should be placed on inventory, so other transactions can proceed.", () =>
							{
								command3.ExecuteNonQuery();
								conn3.CommitTransaction();
							});
					}

					taskUpdateWhsPickline2.Wait(1000);

					taskUpdateWhsPickline2.Wait();
				}

				conn1.ExecuteNonQuery(
					"ALTER INDEX FK_RX__WZ_WE_InventoryLine_WZ_WE_TransactionLine_WZ_IsPicking ON WhsPickLine SET (ALLOW_PAGE_LOCKS = ON)");
				conn1.ExecuteNonQuery(
					"ALTER INDEX NR_RX__WZ_PickedDateTime ON WhsPickLine SET (ALLOW_PAGE_LOCKS = ON)");

				AssertEquals("Committed change from 4 to 6", 6m,
					conn1.ExecuteScalar($"SELECT WZ_Units FROM dbo.WhsPickLine WHERE WZ_PK = '{pickLine1.PK}'"));
				AssertEquals("Rejected change from 4 to 6 to prevent over-pick", 4m,
					conn1.ExecuteScalar($"SELECT WZ_Units FROM dbo.WhsPickLine WHERE WZ_PK = '{pickLine2.PK}'"));
				AssertEquals("Committed change from 9 to 10", 10m,
					conn1.ExecuteScalar($"SELECT WZ_Units FROM dbo.WhsPickLine WHERE WZ_PK = '{pickLine3.PK}'"));
			}
		}

		#endregion
	}

	class PreventPickingReservedLinesOnUnAllocatedJobsTest : WhsTestCaseWithFactory
	{
		#region TestTrigger_WhenPickingReservedPickLinesNotAllocatedToAPick

		[ExpectNoExceptions]
		public void TestTrigger_WhenPickingReservedPickLinesNotAllocatedToAPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			Factory.Save();

			reservedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsPickLine.PreventPickingReservedLinesOnUnAllocatedJobsTriggerID), "Picking a Reserved Pick Line when there is no Pick on the Order should fail.");
		}

		#endregion

		#region TestTrigger_WhenPickingReservedPickLinesAllocatedToAPick

		public void TestTrigger_WhenPickingReservedPickLinesAllocatedToAPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			reservedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(Factory.Save); // Picking reserved Pick Line is allowed when allocated to a Pick.
		}

		public void TestTrigger_WhenPickingReservedPickLinesAllocatedToAPick_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItemsWithMock();
			Helper.PickAndMakeInTransitTransfer(reservedPickLine, ZDateTimeOffset.Now);

			// Avoid creating second dock door transfer.
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				reservedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertNoExceptionThrown(Factory
					.Save); // Picking reserved Pick Line is allowed when allocated to a Pick.
			}
		}

		#endregion

		#region TestTrigger_WhenPickingReservedPickLinesAfterCancelPick

		[ExpectNoExceptions]
		public void TestTrigger_WhenPickingReservedPickLinesAfterCancelPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.CancelPick();
			reservedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsPickLine.PreventPickingReservedLinesOnUnAllocatedJobsTriggerID), "Picking a Reserved Pick Line when the Pick was Cancelled should fail.");
		}

		#endregion
	}

	class PreventReassigningPickLineTest : WhsTestCaseWithFactory
	{
		public void TestTrigger_WhenPickLineIsNotPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("JGU", "JGU");

			// create 10 inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// pick 10 units
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.PickLineQuantity);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;

			AssertEquals("Precondition: PickLine not Picking", false, pickLine.WZ_IsPicking);

			AssertNoExceptionThrown("Should not cause error", () => Factory.Save());
		}

		[ExpectNoExceptions]
		public void TestTrigger_WhenPickLineIsPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("XYZ", "XYZ");
			var user2 = Helper.CreateGlbStaff("JGU", "JGU");

			// create 10 inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// pick 10 units
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.PickLineQuantity);
			Factory.Save();

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			AssertNoExceptionThrown("Should not cause error", () => Factory.Save());

			pickLine.WZ_IsPicking = true;
			AssertNoExceptionThrown("Should not cause error", () => Factory.Save());

			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			AssertNoExceptionThrown("Assign to current user, should not cause error", () => Factory.Save());

			pickLine.WZ_GS_NKAssignedTo = user2.GS_Code;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsPickLine.PreventReassigningPickLineTriggerID), "When PickLine is Picking update WZ_GS_NKAssignedTo should fail.");
		}

		#region TestWP_CriticalChangesVersionID_AddAndDeletePickLine

		public void TestWP_CriticalChangesVersionID_AddAndDeletePickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.Lines[0].WE_ClientOrderedUnits = 0m;
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m,
				WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(finaliseOrders: false, finalisePick: false, pickableDockets: order);
			Factory.Save();
			var pickVersion = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;

			var newPickLine = order.Lines[0].PickLines.AddNew();
			newPickLine.WZ_Units = 4m;
			newPickLine.WZ_WE_InventoryLine = inventory.PK;
			Factory.Save();
			AssertNotEquals("Add new PickLine should update version!", pickVersion,
				NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);
			var pickVersionAfterAdd = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;

			order.Lines[0].PickLines[0].Delete();
			Factory.Save();
			AssertNotEquals("Delete PickLine should update version!", pickVersionAfterAdd,
				NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);
		}

		#endregion

		#region TestWP_CriticalChangesVersionID_UpdatePickLine

		public void TestWP_CriticalChangesVersionID_UpdatePickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.Lines[0].WE_ClientOrderedUnits = 0m;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m,
				WhsPickOption.Codes.Auto);
			var pick = Helper.CreatePickNew(finaliseOrders: false, finalisePick: false, pickableDockets: order);
			Factory.Save();
			var pickVersion = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;

			var newPickLine = order.Lines[0].PickLines[0];
			newPickLine.WZ_Units = newPickLine.WZ_Units - 1m;
			Factory.Save();

			var newPickVersion = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
			AssertNotEquals("Update PickLine should update version!", pickVersion, newPickVersion);

			newPickLine.Factory.Save();
			AssertEquals("Update verstion if only has changes!", newPickVersion,
				NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);
		}

		#endregion

		#region TestWP_CriticalChangesVersionID_ChangeInMemoryAndUnrelated

		public void TestWP_CriticalChangesVersionID_ChangeInMemoryAndUnrelated()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.Lines[0].WE_ClientOrderedUnits = 0m;
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m,
				WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(finaliseOrders: false, finalisePick: false, pickableDockets: order);
			Factory.Save();
			var pickVersion = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;

			var newPickLine = order.Lines[0].PickLines.AddNew();
			newPickLine.WZ_Units = 4m;
			newPickLine.WZ_WE_InventoryLine = inventory.PK;
			newPickLine.Delete();
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			AssertEquals("Not change if change only happend in memory or not critical change!", pickVersion,
				NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);
		}

		#endregion
	}

	class PreventPickingReservedLinesOnUnAllocatedJobsConcurrencyTest : TestCase
	{
		#region TestTriggerLocksDocketLine

		[UseSnapshotProtection]
		public void TestTriggerLocksDocketLine()
		{
			using (var conn1 = Db.NewExtraConnectionToMainDb())
			using (var conn2 = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn1) { RefreshEnabled = false };
				var data = new TestDataSimpleEnvironment(factory);
				var helper = new WhsTestHelperFunctions(factory);
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				var inventory = receive.Inventory[0];
				factory.Save();

				var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 10m);
				var reservedPickLine = orderLine1.ReserveStockIfAbleTo(inventory);

				var order2 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				var orderLine2 = helper.CreateWhsOrderLine(order2, data.Part1, 10m);
				factory.Save();
				helper.CreatePickNew(order1);

				// change below will create imbalance in 2 places, suspend one check procedure to test other fails.
				WhsTestHelperFunctions.SuspendDatabaseProcForTest(
					$"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)",
					"0");

				conn1.BeginTransaction();
				conn2.BeginTransaction();
				// Set up command one to change the Order Line of the Pick Line to an unpicked Order.
				using (var command1 = conn2.Command(WhsPickLineDO.UpdateWhere(reservedPickLine.PK.ToGuid())
						   .Set(l => l.WZ_WE_TransactionLine, orderLine2.PK.ToSqlParameter())
						   .AsSQL()))
				// Set up command two to Pick the PickLine
				using (var command2 = conn1.Command(WhsPickLineDO.UpdateWhere(reservedPickLine.PK.ToGuid())
						   .Set(l => l.WZ_GS_NKAssignedTo, "E")
						   .Set(l => l.WZ_PickedDateTime, ZDateTime.Now.ToDateTime())
						   .AsSQL()))
				{
					bool command2IsInTransaction = false;
					bool pickLineWasPreventedFromBeingPicked = false;

					var task = new Task(() =>
					{
						command2IsInTransaction = true;
						NUnit.Framework.Assert.That(delegate
						{
							command2.ExecuteNonQuery();
						}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsPickLine.PreventPickingReservedLinesOnUnAllocatedJobsTriggerID, true), "Picking Reserved Pick Line should be prevented as the Order for this PickLine is no longer Picked.");

						pickLineWasPreventedFromBeingPicked = true;
					});

					command1.ExecuteNonQuery(); // Pick Line is now on an unpicked Order in Transaction 1.

					// we start the task and make the main thread sleep to allow the task to execute, it will be blocked by the SQL Lock.
					task.Start();
					Thread.Sleep(5000);
					AssertEquals("SQL Command to Pick the PickLine the should have executed.", true,
						command2IsInTransaction);
					AssertEquals("SQL Command to Pick the PickLine should be blocked by the SQL Lock.", false,
						pickLineWasPreventedFromBeingPicked);

					conn2.CommitTransaction(); // Commit the Transaction and release the Lock.
					task.Wait(); // Allow the Task to finish Execution of the Pick SQL Command.
					AssertEquals(
						"SQL Command to Pick the PickLine should have finished and been prevented by the Trigger.",
						true, pickLineWasPreventedFromBeingPicked);
				}
			}
		}

		#endregion
	}

	class PreventUnPickedPickLinesOnFinalisedJobsTest : WhsTestCaseWithFactory
	{
		#region TestProcedureOnlyRunOnceForSavingMultiplePicklines

		public void TestProcedureOnlyRunOnceForSavingMultiplePicklines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			for (int i = 0; i < 3; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part1, 10m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "4", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1, order2, order3, order4);
			Factory.Save();

			AssertEquals("Precondition", 4, pick.GetAllPickLines().Count());
			using (MockDeferrableTriggerCheckProcedure())
			{
				AssertNoExceptionThrown("The procedure should only run once for saving multiple picklines.", () =>
				{
					pick.FinaliseAllOrders();
					pick.FinalisePick();
					Factory.Save();
				});
			}
		}

		public static IDisposable MockDeferrableTriggerCheckProcedure()
		{
			var currentCheckProcedure = Db.Connection.ExecuteScalar($@"
SELECT
    definition
FROM
    sys.sql_modules
WHERE
    objectproperty(OBJECT_ID, 'IsProcedure') = 1
	and OBJECT_NAME(OBJECT_ID) = 'WhsCheckFinalisedJobsWithUnPickedPickLines'").ToString();

			var mockedCheckProcedure = $@"
ALTER PROC WhsCheckFinalisedJobsWithUnPickedPickLines (@PKsToCheck dbo.TVP_uniqueidentifier READONLY) AS
BEGIN
	DECLARE @result int = (select COUNT(*) from @PKsToCheck)

	IF (@result = 1)
	BEGIN
		RAISERROR('WhsCheckFinalisedJobsWithUnPickedPickLines should only be called once with multiple picklines.', 16, 1)
	END
	RETURN 1
END";

			return new DisposableAction(
				() => Db.Connection.ExecuteNonQuery(mockedCheckProcedure),
				() => Db.Connection.ExecuteNonQuery(currentCheckProcedure.Replace("CREATE PROC", "ALTER PROC")));
		}

		#endregion
	}

	#region Triggers_WhsPickLineTest

	class Triggers_WhsPickLineTest : WhsTestCaseWithFactory
	{
		#region TestTG_WhsPickLine_LinkedToCorrectTransactionLine

		public void TestTG_WhsPickLine_LinkedToCorrectTransactionLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, finalise: false);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			// create incorrect pick line that pick stock during receive
			Helper.CreateWhsPickLine(receiveLine, receiveLine.Inventory[0], 5m);
			var exceptionThrown = false;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException e)
			{
				AssertEquals(WhsPickLine.PreventPickLineFromLinkingToIncorrectTransactionLine,
					e.InnerException.InnerException.Message);
				exceptionThrown = true;
			}

			AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
		}

		#endregion

		#region TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_Transfers

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_Transfers_Insert()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_TransfersCore((transferLine, inventory) =>
			{
				var newPickLine = Helper.CreateWhsPickLine(transferLine, inventory, 5m);
				newPickLine.WZ_GS_NKAssignedTo = "E";
				newPickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			});
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_Transfers_Update()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_TransfersCore((transferLine, inventory) =>
			{
				var pickLine = transferLine.PickLines.Single();
				pickLine.HasChanges = true;
				((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_Units] = 7m;
			});
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_Transfers_Delete()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_TransfersCore((transferLine, inventory) =>
			{
				var pickLine = transferLine.PickLines.Single();
				((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_Units] = 0m;
				pickLine.Delete();
			});
		}

		void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_TransfersCore(Action<WhsTransferLine, WhsInventoryView> setDataForTriggerToFail)
		{
			// It should consistently target the trigger being tested, regardless of the order of execution.
			DisableBalanceCheckTriggers();

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA2);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			var skipRunningSPAferFactorySave = new Mock<IDeferredTriggerRunner>();
			skipRunningSPAferFactorySave.Setup(x => x.RunDeferredTriggers(It.IsAny<IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>>>(), It.IsAny<IDbConnected>()))
					 .Callback<IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>>, IDbConnected>((triggers, factory) => { });

			using (ObjectFactory.Substitute(skipRunningSPAferFactorySave.Object))
			{
				Factory.Save();

				// dodgy add/modify/delete of pick line to ensure trigger fails
				setDataForTriggerToFail(transferLine, receive.Inventory[0]);
				NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID, true), "When trying to over-pick transfer line the trigger should fail.");
			}

			void DisableBalanceCheckTriggers()
			{
				((IDbConnected)Factory).Connection.ExecuteNonQuery(@"
DISABLE TRIGGER TG_WhsPickLine_StockOnHandIsBalanced ON WhsPickLine;
DISABLE TRIGGER TG_WhsDocketLine_StockOnHandIsBalanced ON WhsDocketLine;
DISABLE TRIGGER TG_WhsDocketLine_StockOnHandIsBalanced_Insert ON WhsDocketLine;
DISABLE TRIGGER TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect ON WhsDocketLine;
DISABLE TRIGGER TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert ON WhsDocketLine;
");
			}
		}

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_SemephoreStillResetOnDeletedPickLine_OnDbRollback()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			locationA1.WLV_LocationStatus = "VOI";
			var pickLine = orderLine.PickLines.Single();
			pickLine.Delete();

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempted to Void out a Location that has Existing or Pending Stock.", true), "Precondition: When trying to void a location with stock trigger should fire and rollback db.");
		}

		#endregion

		#region TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_Orders

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_Orders_Insert()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_TransfersCore((orderLine, inventory) =>
			{
				var newPickLine = Helper.CreateWhsPickLine(orderLine, inventory, 5m);
				newPickLine.WZ_GS_NKAssignedTo = "E";
				newPickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			});
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_Orders_Update()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_TransfersCore((orderLine, inventory) =>
			{
				var pickLine = orderLine.PickLines.Single();
				pickLine.HasChanges = true;
				((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_Units] = 17m;
			});
		}

		#endregion

		#region TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_WorkOrders

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_WorkOrders_Insert()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_WorkOrdersCore((pickline) =>
			{
				Helper.CreateWhsPickLine(pickline.DocketLine, pickline.Inventory, 5m);
			});
		}

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_WorkOrders_Update()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_WorkOrdersCore((pickline) =>
			{
				pickline.WZ_Units = 10m;
			});
		}

		void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_WorkOrdersCore(
			Action<WhsPickLine> setDataForTriggerToFail)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 2m);
			Factory.Save();

			var pick = Helper.CreatePickNew(workOrder);
			// dodgy add/modify/delete of pick line to ensure trigger fails
			var pickline = pick.GetAllPickLines().Single();
			setDataForTriggerToFail(pickline);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID}", true), "When trying to over-pick work order the trigger should fail");
		}

		#endregion

		#region TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_AdjustmentOuts

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_AdjustmentOuts_Insert()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_AdjustmentOutsCore((adjustmentLine, inventory) =>
			{
				var newPickLine = Helper.CreateWhsPickLine(adjustmentLine, inventory, 5m);
				newPickLine.WZ_GS_NKAssignedTo = "E";
				newPickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			});
		}

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_AdjustmentOuts_Update()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_AdjustmentOutsCore((adjustmentLine, inventory) =>
			{
				var pickLine = adjustmentLine.PickLines.Single();
				pickLine.WZ_Units = 7m;
			});
		}

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_AdjustmentOuts_Delete()
		{
			TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_AdjustmentOutsCore((adjustmentLine, inventory) =>
			{
				var pickLine = adjustmentLine.PickLines.Single();
				pickLine.Delete();
			});
		}

		void TestTG_WhsPickLine_TransactionAndPickedQtyIsCorrect_AdjustmentOutsCore(
			Action<WhsAdjustmentLine, WhsInventoryView> setDataForTriggerToFail)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, locationA1);
			adjustment.RunPreSaveValidation();
			Factory.Save();

			// doggy add/modify/delete of pick line to ensure trigger fails
			setDataForTriggerToFail(adjustmentLine, receive.Inventory[0]);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID}", true), "When trying to over-pick adjustment out line the trigger should fail.");
		}

		#endregion

		#region TestTG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalizedTest

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalizedTest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.Location, inventory.Location);
			var pickLine = Helper.CreateWhsPickLine(transferLine, inventory, 10m);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			pickLine.WZ_GS_NKAssignedTo = "X";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsPickLine.PreventCriticalFieldChangeWhenPickLineIsFinalizedTriggerID, true), "The Trigger was expected to throw an Exception");
		}

		public void TestTG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalizedTest_FinalizeWithChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var inventory = receive.Inventory[0];
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "T1", data.Part1, 50m,
				WhsPickOption.Codes.Manual);
			var orderLine = order.Lines.Single();
			var pick = Helper.CreatePickNew(order);
			var pickLine = Helper.CreateWhsPickLine(orderLine, inventory, 10m);
			Factory.Save();

			pickLine.WZ_GS_NKAssignedTo = "~BP";
			pickLine.WZ_Units = 5m;
			pickLine.WZ_VerifiedEmpty = "N";
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertNoExceptionThrown(
				"No Exception must be thrown when finalizing a pick line with changes to critical fields.",
				Factory.Save);
		}

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalizedTest_CancelPickWithReservePickLines()
		{
			var pickDate = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var inventory = receive.Inventory[0];
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "T1", data.Part1, 50m);
			var orderLine = order.Lines.Single();
			var pickLine = Helper.CreateWhsPickLine(orderLine, inventory, 10m);
			pickLine.IsReserveLine = true;
			pickLine.ReservedQuantity = 8m;
			Factory.Save();

			// Current Business Rule: We cannot pick Reserved PickLines that are not allocated to a Pick. So we assign a pick.
			var pick = Helper.CreatePickNew(order);
			pickLine.WZ_PickedDateTime = pickDate;
			pickLine.WZ_GS_NKAssignedTo = "R";
			pickLine.WZ_F3_NKAllocatedPackType = "PLT";
			pickLine.ReservedQuantity = 25m;

			// this test is testing some strange case, so we will simply prevent the creation of the in-transit transfers to simulate the original test
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				AssertNoExceptionThrown(
					"No Exception must be thrown when picking a reserved pick line with changes to critical fields.",
					Factory.Save);
			}

			// Try to cancel pick while modifying critical fields.
			pick.CancelPick();
			pickLine.WZ_GS_NKAssignedTo = "L";
			pickLine.WZ_F3_NKAllocatedPackType = "BOX";

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsPickLine.PreventCriticalFieldChangeWhenPickLineIsFinalizedTriggerID, true), "The Trigger was expected to throw an Exception");
			AssertEquals(
				"Pick cancelling for picked lines is not allowed. Adjust this trigger in case the business rule has changed.",
				false, pick.IsCancelled);
			AssertEquals("Picklines must not be deleted when trying to cancel pick.", false, pickLine.IsDeleted);
			AssertEquals("Pickline picked date must remain the same after trying to cancel pick.", pickDate,
				pickLine.WZ_PickedDateTime);
		}

		#endregion

		#region TestTG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouse

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs2, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = Helper.CreateWhsPickLine(order.Lines.Single(), receive.Inventory[0], 10m);

			AssertNotEquals("Precondition: Warehouse of inventory must be different to Warehouse in order.",
				receive.Inventory[0].Location.Warehouse, order.Warehouse);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsPickLine.PreventTransactionsPickingFromDifferentWarehouseTriggerID, true), "Expected trigger to prevent this operation.");
		}

		#endregion

		#region TestTG_WhsPickLine_StockOnHandIsBalanced

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_StockOnHandIsBalanced_InsertUpdate()
		{
			TestTG_WhsPickLine_StockOnHandIsBalancedCore(
				"Ensure that SOH balance keeping trigger was fired on Insert / Update.", (pick) =>
				{
					var pickLine = pick.GetAllPickLines().Single();
					pickLine.WZ_GS_NKAssignedTo = "~BP";
					// hack to modify PickLine without changing SOH to make trigger fail.
					((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_PickedDateTime] =
						DateTimeOffset.UtcNow;
				});
		}

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_StockOnHandIsBalanced_Delete()
		{
			TestTG_WhsPickLine_StockOnHandIsBalancedCore("Ensure that SOH balance keeping trigger was fired on Delete.",
				(pick) =>
				{
					pick.FinaliseAllOrders();
					pick.FinalisePick();
					Factory.Save();
					AssertIsFinalisedPrecondition(pick);

					var pickLine = pick.GetAllPickLines().Single();
					var inventoryLine = pickLine.InventoryLine;
					pickLine.HasChanges = true;
					// hack to modify PickLine without changing SOH to make trigger fail.
					((IBusinessObjectInternals)pickLine).Row.Delete();
					AssertEquals("Precondition - stock should still be picked.", 0m, inventoryLine.WE_StockOnHand);
				});
		}

		void TestTG_WhsPickLine_StockOnHandIsBalancedCore(string errorMessage,
			Action<WhsPick> modifyPickLineToFailTrigger)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			modifyPickLineToFailTrigger(pick);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WhsPickLine.PreventAttemptToPutStockOnHandOutOfBalance}", true), errorMessage);
		}

		#endregion
	}

	#endregion

	[UseSnapshotProtection]
	class PreventPickLineAllocatingUnallocateableInventory : TestCase
	{
		#region TestTrigger_AllocatedInventoryIsNotAllocateable

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_InventoryIsHeld()
		{
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory);
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				factory.Save();

				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var newFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var receiveLineInNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine.PK);
					receiveLineInNewFactory.HeldCodeChangeQuantity = 10m;
					receiveLineInNewFactory.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
					receiveLineInNewFactory.ChangeInventoryHeldCode(true);
					AssertEquals("Precondition", InventoryStatus.Codes.Held,
						receiveLineInNewFactory.WE_CurrentInventoryStatus);
					newFactory.Save();
				}

				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				var orderLine = order.Lines[0];
				helper.CreatePickNew(order);
				AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.PickLineQuantity);

				var exceptionThrown = false;
				try
				{
					factory.Save();
				}
				catch (ZSaveConcurrencyException e)
				{
					AssertEquals(WhsPickLine.PreventAttemptToAlllocateUnallocateableInventory,
						e.GetInnermostException().Message);
					exceptionThrown = true;
				}

				AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
			}
		}

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_InventoryIsHeld_VirtualWarehouse()
		{
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory);
				data.Whs1.WW_IsVirtualWarehouse = true;
				factory.Save();

				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				factory.Save();

				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var newFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var receiveLineInNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine.PK);
					receiveLineInNewFactory.HeldCodeChangeQuantity = 10m;
					receiveLineInNewFactory.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
					receiveLineInNewFactory.ChangeInventoryHeldCode(true);
					AssertEquals("Precondition", InventoryStatus.Codes.Held,
						receiveLineInNewFactory.WE_CurrentInventoryStatus);
					newFactory.Save();
				}

				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				var orderLine = order.Lines[0];
				helper.CreatePickNew(order);
				AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.PickLineQuantity);

				AssertNoExceptionThrown(
					"Unallocateable stock can be allocated to pick lines in a virtual warehouse(Customs related).",
					factory.Save);
			}
		}

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_InventoryIsAvailable()
			=> TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_InventoryIsAvailable_Core(isVirtualWarehouse: false);

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_InventoryIsAvailable_VirtualWarehouse()
			=> TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_InventoryIsAvailable_Core(isVirtualWarehouse: true);

		void TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_InventoryIsAvailable_Core(bool isVirtualWarehouse)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory, 1, 1);
				data.Whs1.WW_IsVirtualWarehouse = isVirtualWarehouse;
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				factory.Save();

				var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				var pick = helper.CreatePickNew(order);
				var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
				availableInventory.Allocate = false;
				AssertEquals("Precondition: Stock is not Picked.", 0m, orderLine.PickLineQuantity);
				factory.Save();

				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var newFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var receiveLineInNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine.PK);
					receiveLineInNewFactory.HeldCodeChangeQuantity = 10m;
					receiveLineInNewFactory.HeldCodeToChangeTo = "";
					receiveLineInNewFactory.ChangeInventoryHeldCode(true);
					AssertEquals("Precondition", InventoryStatus.Codes.Available, receiveLineInNewFactory.WE_CurrentInventoryStatus);
					newFactory.Save();
				}

				availableInventory.Allocate = true;
				AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.PickLineQuantity);

				if (isVirtualWarehouse)
				{
					AssertNoExceptionThrown("Unallocateable stock can be allocated to pick lines in a virtual warehouse(Customs related).", factory.Save);
				}
				else
				{
					var exceptionThrown = false;
					try
					{
						factory.Save();
					}
					catch (ZSaveConcurrencyException e)
					{
						AssertEquals(WhsPickLine.PreventAttemptToAlllocateUnallocateableInventory, e.GetInnermostException().Message);
						exceptionThrown = true;
					}

					AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
				}
			}
		}

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_IsDifferentHoldCode()
			=> TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_IsDifferentHoldCode_Core(isVirtualWarehouse: false);

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_IsDifferentHoldCode_VirtualWarehouse()
			=> TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_IsDifferentHoldCode_Core(isVirtualWarehouse: true);

		void TestTrigger_AllocatedInventoryIsNotAllocateable_HeldInventoryOrder_IsDifferentHoldCode_Core(bool isVirtualWarehouse)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory, 1, 1);
				data.Whs1.WW_IsVirtualWarehouse = isVirtualWarehouse;
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				factory.Save();

				var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
				var pick = helper.CreatePickNew(order);
				var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
				availableInventory.Allocate = false;
				AssertEquals("Precondition: Stock is not Picked.", 0m, orderLine.PickLineQuantity);
				factory.Save();

				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var newFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var receiveLineInNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine.PK);
					receiveLineInNewFactory.HeldCodeChangeQuantity = 10m;
					receiveLineInNewFactory.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
					receiveLineInNewFactory.ChangeInventoryHeldCode(true);
					AssertEquals("Precondition", InventoryStatus.Codes.Held, receiveLineInNewFactory.WE_CurrentInventoryStatus);
					AssertEquals("Precondition", InventoryHoldCodes.Codes.Damaged, receiveLineInNewFactory.WE_WHC_NKCurrentInventoryHeldCode);
					newFactory.Save();
				}

				availableInventory.Allocate = true;
				AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.PickLineQuantity);

				if (isVirtualWarehouse)
				{
					AssertNoExceptionThrown("Unallocateable stock can be allocated to pick lines in a virtual warehouse(Customs related).", factory.Save);
				}
				else
				{
					var exceptionThrown = false;
					try
					{
						factory.Save();
					}
					catch (ZSaveConcurrencyException e)
					{
						AssertEquals(WhsPickLine.PreventAttemptToAlllocateUnallocateableInventory, e.GetInnermostException().Message);
						exceptionThrown = true;
					}

					AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
				}
			}
		}

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_InventoryIsHeld_VirtualWarehouse_WorkOrder()
		{
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory);
				data.Whs1.WW_IsVirtualWarehouse = true;
				helper.CreateProductBOM(data.Part2, data.Part1, 1m, "UNT");
				factory.Save();

				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				factory.Save();

				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var newFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var receiveLineInNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine.PK);
					receiveLineInNewFactory.HeldCodeChangeQuantity = 10m;
					receiveLineInNewFactory.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
					receiveLineInNewFactory.ChangeInventoryHeldCode(true);
					AssertEquals("Precondition", InventoryStatus.Codes.Held,
						receiveLineInNewFactory.WE_CurrentInventoryStatus);
					newFactory.Save();
				}

				var order = helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "O1", data.Part2, 10m);
				var orderLine = order.Lines[0];
				helper.CreatePickNew(order);
				AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.ChildComponentLines.Single().PickLineQuantity);

				var exceptionThrown = false;
				try
				{
					factory.Save();
				}
				catch (ZSaveException e)
				{
					AssertEquals(WhsPickLine.PreventAttemptToAlllocateUnallocateableInventory,
						e.InnerException.InnerException.Message);
					exceptionThrown = true;
				}

				AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
			}
		}

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_InventoryIsHeld_VirtualWarehouse_DynamicWorkOrder()
		{
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory);
				data.Whs1.WW_IsVirtualWarehouse = true;
				var inwardProcessingArea = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
				var inwardProcessingLocation = helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
				inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
				inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;
				factory.Save();

				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				receive.WD_IsInwardsProcessingJob = true;
				receive.WD_DocketSubType = ReceiveType.Codes.Customs;
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m, inwardProcessingLocation.PK, "ENT1", "", 10m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				factory.Save();

				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var newFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var receiveLineInNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine.PK);
					receiveLineInNewFactory.HeldCodeChangeQuantity = 10m;
					receiveLineInNewFactory.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
					receiveLineInNewFactory.ChangeInventoryHeldCode(true);
					AssertEquals("Precondition", InventoryStatus.Codes.Held,
						receiveLineInNewFactory.WE_CurrentInventoryStatus);
					newFactory.Save();
				}

				var dynamicWorkOrder = factory.New<WhsDynamicWorkOrder>();
				dynamicWorkOrder.WD_OH_Client = data.Org1.PK;
				dynamicWorkOrder.WD_WW_Whs = data.Whs1.PK;
				dynamicWorkOrder.WD_ExternalReference = "O1";
				dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

				var orderLine = dynamicWorkOrder.Lines.AddNew();
				orderLine.WE_OP = data.Part2.PK;
				orderLine.WE_TransactionQuantity = 10m;
				orderLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

				var childLine = dynamicWorkOrder.Lines.AddNew();
				childLine.WE_OP = data.Part1.PK;
				childLine.WE_TransactionQuantity = 10m;
				childLine.WE_WE_ParentDocketLine = orderLine.PK;
				helper.CreatePickNew(dynamicWorkOrder);

				AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.ChildComponentLines.Single().PickLineQuantity);

				var exceptionThrown = false;
				try
				{
					factory.Save();
				}
				catch (ZSaveException e)
				{
					AssertEquals(WhsPickLine.PreventAttemptToAlllocateUnallocateableInventory,
						e.InnerException.InnerException.Message);
					exceptionThrown = true;
				}

				AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
			}
		}

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_InventoryIsHeld_ReservedPickLine()
		{
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory);
				factory.Save();

				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				factory.Save();

				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var newFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var receiveLineInNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine.PK);
					receiveLineInNewFactory.HeldCodeChangeQuantity = 10m;
					receiveLineInNewFactory.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
					receiveLineInNewFactory.ChangeInventoryHeldCode(true);
					AssertEquals("Precondition", InventoryStatus.Codes.Held,
						receiveLineInNewFactory.WE_CurrentInventoryStatus);
					newFactory.Save();
				}

				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				var orderLine = order.Lines[0];
				helper.CreateReservePickLine(orderLine, receiveLine.Inventory[0], 10m);
				AssertEquals("Precondition", receiveLine.WE_StockOnHand,
					receiveLine.ReservedPickLines.Sum(l => l.WZ_Units));

				AssertNoExceptionThrown("Unallocateable stock can be reserved.", factory.Save);
			}
		}

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_InventoryIsReadyToPack()
		{
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory, 2, 1);
				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				factory.Save();

				var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
				transferLine.FinaliseDocketLine();
				transfer.FinaliseDocketWithoutUserConfirmation();
				factory.Save();

				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var newFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var transferLineInNewFactory = newFactory.Load<WhsTransferLine>(transferLine.PK);
					transferLineInNewFactory.WE_CurrentInventoryStatus = InventoryStatus.Codes.ReadyToPack;
					AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack,
						transferLineInNewFactory.WE_CurrentInventoryStatus);
					newFactory.Save();
				}

				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				var orderLine = order.Lines[0];
				helper.CreatePickNew(order);
				AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.PickLineQuantity);
				AssertNoExceptionThrown("Trigger should have NOT prevented save.", () => factory.Save());
			}
		}

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_InventoryIsPendingFromPickByBOM()
		{
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory, 2, 1);
				var bike = helper.CreateProduct(data.Org1, "BIKE");
				bike.OP_IsComponentPickedOnSalesOrder = true;
				var wheel = helper.CreateProduct(data.Org1, "WHEEL");
				helper.CreateProductBOM(bike, wheel, 2m, "UNT");

				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				helper.CreateWhsReceiveInventoryLine(receive, wheel, 70m, data.Whs1.FindLocation("A-1"));
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

				var pick = factory.New<WhsPick>();
				pick.WP_WW_Whs = data.Whs1.PK;
				pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
				pick.WP_PickOption = WhsPickOption.Codes.Manual;
				factory.Save();

				var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
				var orderLine = helper.CreateWhsOrderLine(order, bike, 2m);

				pick.AddOrders(new[] { order });
				pick.AutoAllocateItemsWithMock();

				AssertEquals("Precondition: Stock is Picked.", 2m, orderLine.PickLineQuantity);
				AssertNoExceptionThrown("Trigger should have NOT prevented save.", () => factory.Save());
			}
		}

		public void TestTrigger_AllocatedInventoryIsNotAllocateable_ThrowsWhenIsNormalPending()
		{
			using (var testCaseDbConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(testCaseDbConnection);
				var helper = new WhsTestHelperFunctions(factory);

				var data = new TestDataSimpleEnvironment(factory);
				var receive = factory.New<WhsReceive>();
				receive.WD_WW_Whs = data.Whs1.PK;
				receive.WD_OH_Client = data.Org1.PK;

				var receiveLine = receive.Lines.AddNew();

				receiveLine.WE_OP = data.Part1.PK;
				receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
				receiveLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Pending;
				receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Pending;
				receiveLine.WE_TransactionQuantity = 10m;
				receiveLine.WE_StockOnHand = 10m;
				receiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Empty;

				factory.Save();

				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				var orderLine = order.Lines[0];

				var pick = factory.New<WhsPick>();
				pick.WP_WW_Whs = data.Whs1.PK;
				pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
				pick.AddOrders(new[] { order });
				factory.Save();

				var pickLine = factory.New<WhsPickLine>();
				pickLine.WZ_F3_NKAllocatedPackType = receiveLine.WE_F3_NKPackType;
				pickLine.WZ_Units = 10m;
				pickLine.WZ_WE_InventoryLine = receiveLine.PK;
				pickLine.WZ_WE_TransactionLine = orderLine.PK;
				AssertEquals("Precondition: Stock is Picked.", 10m, orderLine.PickLineQuantity);

				var exceptionThrown = false;
				try
				{
					factory.Save();
				}
				catch (ZSaveException e)
				{
					AssertEquals(WhsPickLine.PreventAttemptToAlllocateUnallocateableInventory,
						e.InnerException.InnerException.Message);
					exceptionThrown = true;
				}

				AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
			}
		}

		#endregion
	}
}
