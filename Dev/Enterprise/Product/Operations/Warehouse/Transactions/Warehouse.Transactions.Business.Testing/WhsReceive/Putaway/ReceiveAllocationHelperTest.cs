using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ReceiveAllocationHelperTest : WhsTestCaseWithFactory
	{
		#region TestAllocateLocations_NullArgs

		public void TestAllocateLocations_NullArgs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);

			var notifications = new NotificationBuffer();

			AssertExceptionThrown(
				typeof(ArgumentNullException),
				() => ReceiveAllocationHelper.AllocateLocations(null, data.Whs1, new[] { receiveLine1 }, notifications));

			AssertExceptionThrown(
				typeof(ArgumentNullException),
				() => ReceiveAllocationHelper.AllocateLocations(new[] { receive1 }, data.Whs1, null, notifications));
		}

		#endregion

		#region TestAllocateLocations_DifferentWarehouses

		public void TestAllocateLocations_DifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var otherWarehouse = Helper.CreateWarehouse("WH2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, otherWarehouse, "R1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m);

			var notifications = new NotificationBuffer();

			AssertExceptionThrown(
				typeof(ArgumentException),
				"Warehouse must be same for all dockets.",
				() => ReceiveAllocationHelper.AllocateLocations(new[] { receive1 }, otherWarehouse, new[] { receiveLine1 }, notifications));

			AssertExceptionThrown(
				typeof(ArgumentException),
				"Warehouse must be same for all dockets.",
				() => ReceiveAllocationHelper.AllocateLocations(new[] { receive1, receive2 }, data.Whs1, new[] { receiveLine1, receiveLine2 }, notifications));
		}

		#endregion

		#region TestAllocateLocations_PutawayEngine_ParametersCanBePassedIn

		public void TestAllocateLocations_PutawayEngine_ParametersCanBePassedIn_WithTrueFalse()
		{
			TestAllocateLocations_PutawayEngine_ParametersCanBePassedIn(new[] { ZGuid.NewZGuid(), ZGuid.NewZGuid() }, true, false);
		}

		public void TestAllocateLocations_PutawayEngine_ParametersCanBePassedIn_WithFalseTrue()
		{
			TestAllocateLocations_PutawayEngine_ParametersCanBePassedIn(Enumerable.Empty<ZGuid>(), false, true);
		}

		void TestAllocateLocations_PutawayEngine_ParametersCanBePassedIn(IEnumerable<ZGuid> expectedSkipLocationPKs, bool expectedUseLocationConcurrencyHandling, bool expectedNeedRebuildLocationCache)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(10m, 20m, 30m, 40m, 50m, false, false);

			var selected = new List<WhsReceiveLine>();
			selected.AddRange(new[] { (WhsReceiveLine)data.Line112.InDocketLine, (WhsReceiveLine)data.Line113.InDocketLine, (WhsReceiveLine)data.Line114.InDocketLine });

			var notifications = new NotificationBuffer();

			IEnumerable<ZGuid> skipLocationPKsPassedIn = null;
			var useLocationConcurrencyHandlingPassedIn = !expectedUseLocationConcurrencyHandling;
			var needRebuildLocationCachePassedIn = !expectedNeedRebuildLocationCache;

			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receive => receive.Contains(data.Receive11),
				receiveLinesCondition: receiveLines => receiveLines.SequenceEqual(selected),
				iNotificationsCondition: notification => notification.Equals(notifications),
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, lines, notification, refEquipment, locationPKs, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					skipLocationPKsPassedIn = locationPKs;
					useLocationConcurrencyHandlingPassedIn = useLocationConcurrencyHandling;
					needRebuildLocationCachePassedIn = needRebuildLocationCache;
				});

			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				ReceiveAllocationHelper.AllocateLocations(new[] { data.Receive11 }, data.Whs1, selected, notifications, null, false, expectedSkipLocationPKs, expectedUseLocationConcurrencyHandling, expectedNeedRebuildLocationCache);
				AssertEquals(false, notifications.Events.Length > 0);
				AssertEquals("Should *not* be allocated as stubbed engine did nothing.", false, data.Line112.InDocketLine.WE_WL.IsValid);
				AssertEquals("Should *not* be allocated as stubbed engine did nothing.", false, data.Line113.InDocketLine.WE_WL.IsValid);
				AssertEquals("Should *not* be allocated as stubbed engine did nothing.", false, data.Line114.InDocketLine.WE_WL.IsValid);
				AssertEquals("Should pass skipLocationPKs into PutawayEngineManager Putaway call. ", expectedSkipLocationPKs, skipLocationPKsPassedIn);
				AssertEquals("Should pass useLocationConcurrencyHandling into PutawayEngineManager Putaway call. ", expectedUseLocationConcurrencyHandling, useLocationConcurrencyHandlingPassedIn);
				AssertEquals("Should pass needRebuildLocationCache into PutawayEngineManager Putaway call.", expectedNeedRebuildLocationCache, needRebuildLocationCachePassedIn);
			}

			putawayEngineMock.VerifyAll();
		}

		#endregion

		#region TestAllocateLocations_PutawayEngine_MultipleReceives

		public void TestAllocateLocations_PutawayEngine_MultipleReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 10m);

			var notifications = new NotificationBuffer();

			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				ReceiveAllocationHelper.AllocateLocations(new[] { receive1, receive2, receive3 }, data.Whs1, new[] { receiveLine1, receiveLine2, receiveLine3 }, notifications);
				AssertEquals(false, notifications.Events.Length > 0);
			}

			putawayEngineMock.VerifyAll();
		}

		#endregion

		#region TestAllocateLocations_PutawayEngine_Error

		public void TestAllocateLocations_PutawayEngine_Error() => TestAllocateLocations_PutawayEngine_Error(true);

		public void TestAllocateLocations_PutawayEngine_Error_OverriddenNotifications() => TestAllocateLocations_PutawayEngine_Error(false);

		void TestAllocateLocations_PutawayEngine_Error(bool useReceivesNotificationSubscriber)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(10m, 20m, 30m, 40m, 50m, false, false);

			var selected = new List<WhsReceiveLine>();
			selected.AddRange(new[] { (WhsReceiveLine)data.Line112.InDocketLine, (WhsReceiveLine)data.Line113.InDocketLine, (WhsReceiveLine)data.Line114.InDocketLine });

			var notifications = useReceivesNotificationSubscriber ? (NotificationBuffer)data.Receive11.NotificationSubscriber : new NotificationBuffer();

			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receive => receive.Contains(data.Receive11),
				receiveLinesCondition: receiveLines => receiveLines.SequenceEqual(selected),
				iNotificationsCondition: notification => notification.Equals(notifications),
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, lines, notification, refEquipment, locationPKs, useLocationConcurrencyHandling, needRebuildLocationCache) => notification.AddError("ERROR"));

			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				ReceiveAllocationHelper.AllocateLocations(new[] { data.Receive11 }, data.Whs1, selected, useReceivesNotificationSubscriber ? null : notifications);
				AssertEquals(true, notifications.HasErrors);
				AssertEquals("ERROR", notifications.AsString.Trim());
				AssertEquals("Should *not* be allocated as stubbed engine did nothing.", false, data.Line112.InDocketLine.WE_WL.IsValid);
				AssertEquals("Should *not* be allocated as stubbed engine did nothing.", false, data.Line113.InDocketLine.WE_WL.IsValid);
				AssertEquals("Should *not* be allocated as stubbed engine did nothing.", false, data.Line114.InDocketLine.WE_WL.IsValid);
			}

			putawayEngineMock.VerifyAll();
		}

		#endregion

		#region TestAllocateLocations_PutawayEngine_Equipment

		public void TestAllocateLocations_PutawayEngine_Equipment()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(10m, 20m, 30m, 40m, 50m, false, false);

			var equipment = Helper.CreateEquipment("FRK", 1m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);

			var selected = new List<WhsReceiveLine>();
			selected.AddRange(new[] { (WhsReceiveLine)data.Line112.InDocketLine, (WhsReceiveLine)data.Line113.InDocketLine, (WhsReceiveLine)data.Line114.InDocketLine });

			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receive => receive.Contains(data.Receive11),
				receiveLinesCondition: receiveLines => receiveLines.SequenceEqual(selected),
				iNotificationsCondition: notification => notification.Equals(data.Receive11.NotificationSubscriber),
				refEquipmentCondition: refEquipment => refEquipment == equipment,
				action: (receives, lines, notification, refEquipment, locationPKs, useLocationConcurrencyHandling, needRebuildLocationCache) => { });

			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				ReceiveAllocationHelper.AllocateLocations(new[] { data.Receive11 }, data.Whs1, selected, data.Receive11.NotificationSubscriber, equipment: equipment);
				AssertEquals("Should *not* be allocated as stubbed engine did nothing.", false, data.Line112.InDocketLine.WE_WL.IsValid);
				AssertEquals("Should *not* be allocated as stubbed engine did nothing.", false, data.Line113.InDocketLine.WE_WL.IsValid);
				AssertEquals("Should *not* be allocated as stubbed engine did nothing.", false, data.Line114.InDocketLine.WE_WL.IsValid);
			}

			putawayEngineMock.VerifyAll();
		}

		#endregion

		#region TestAllocateLocationsWithPallets

		public void TestAllocateLocationsWithPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 7, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff");
			var nonEmptyLocation = data.Whs1.FindLocation("A-1");
			var locationWithAllocatedInventory = data.Whs1.FindLocation("A-2");
			var locationWithPickerName = data.Whs1.FindLocation("A-3");
			var locationWithPickerDate = data.Whs1.FindLocation("A-4");
			var locationWithPickerNameAndDate = data.Whs1.FindLocation("A-5");
			var emptyLocation1 = data.Whs1.FindLocation("A-6");
			var emptyLocation2 = data.Whs1.FindLocation("A-7");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, nonEmptyLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationWithAllocatedInventory, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationWithPickerName, "PLT3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationWithPickerDate, "PLT4");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationWithPickerNameAndDate, "PLT5");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order);
			AllocateAvailableInventory(pick, locationWithAllocatedInventory);
			AllocateAvailableInventory(pick, locationWithPickerName, staff);
			// these 2 locations must be considered "Empty" as all stock was picked from them
			AllocateAvailableInventory(pick, locationWithPickerDate, staff, true);
			AllocateAvailableInventory(pick, locationWithPickerNameAndDate, staff, true);
			Factory.Save();

			var receiveAutoAllocateInv = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receiveAutoAllocateInv, data.Part1, 1m, null, "A");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receiveAutoAllocateInv, data.Part1, 1m, null, "B");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receiveAutoAllocateInv, data.Part1, 1m, null, "C");
			Factory.Save();

			receiveAutoAllocateInv.AllocateLocationsWithMock();

			AssertContainsExactElementsInAnyOrder(new[] { "A-4", "A-5", "A-6" },
					new string[] { inventory1.LocationString, inventory2.LocationString, inventory3.LocationString });
		}

		#endregion

		#region TestAllocateLocations_WithCommittedUnits

		public void TestAllocateLocations_WithCommittedUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 7, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff");
			var nonEmptyLocation = data.Whs1.FindLocation("A-1");
			var locationWithAllocatedInventory = data.Whs1.FindLocation("A-2");
			var locationWithPickerName = data.Whs1.FindLocation("A-3");
			var locationWithPickerDate = data.Whs1.FindLocation("A-4");
			var locationWithPickerNameAndDate = data.Whs1.FindLocation("A-5");
			var emptyLocation1 = data.Whs1.FindLocation("A-6");
			var emptyLocation2 = data.Whs1.FindLocation("A-7");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, nonEmptyLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationWithAllocatedInventory);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationWithPickerName);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithPickerDate);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithPickerNameAndDate);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order);
			AllocateAvailableInventory(pick, locationWithAllocatedInventory);
			AllocateAvailableInventory(pick, locationWithPickerName, staff);
			AllocateAvailableInventory(pick, locationWithPickerDate, staff, true, 1m);
			AllocateAvailableInventory(pick, locationWithPickerNameAndDate, staff, true, 1m);
			Factory.Save();

			var receiveAutoAllocateInv = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receiveAutoAllocateInv, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receiveAutoAllocateInv, data.Part1, 1m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receiveAutoAllocateInv, data.Part1, 1m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receiveAutoAllocateInv, data.Part1, 1m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receiveAutoAllocateInv, data.Part1, 1m);
			receiveAutoAllocateInv.AllocateLocationsWithMock();

			AssertContainsExactElementsInAnyOrder(new[] { "A-1", "A-1", "A-1", "A-6", "A-7" },
					new string[] { inventory1.LocationString, inventory2.LocationString, inventory3.LocationString, inventory4.LocationString, inventory5.LocationString });
		}

		void AllocateAvailableInventory(WhsPick pick, WhsLocation locationWithAllocatedInventory, GlbStaff staff = null, bool hasPickerDate = false, decimal? allocatedQty = null)
		{
			var availableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().
					AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == locationWithAllocatedInventory);

			if (allocatedQty.HasValue)
			{
				availableInventory.PickLineQuantity = allocatedQty.Value;
			}
			else
			{
				availableInventory.Allocate = true;
			}

			if (staff != null)
			{
				availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single().AssignedToPK = staff.PK;
			}

			if (hasPickerDate)
			{
				availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single().PickedDate = ZDateTimeOffset.Now;
			}
		}

		#endregion

		#region TestAllocateLocations_PutawayTransfers

		public void TestAllocateLocations_PutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateProductUnit(data.Part1, "PLT", 5);
			var ddlLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = ddlLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			var receivedInventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m).InDocketLine as WhsReceiveLine;
			var puttingAwayInventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "B", 10m).InDocketLine as WhsReceiveLine;
			var putawayInventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "C", 10m).InDocketLine as WhsReceiveLine;
			Factory.Save();

			var transferForPalletB = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPalletB.WD_IsPutawayTransfer = true;
			var transferLineForPalletB = Helper.SetupTransferLineForDockDoorLocation(transferForPalletB, data.Part1, dockDoorLocation, nonDockDoorLocation, "B", 10m);
			transferForPalletB.RunPreSaveValidation();
			transferLineForPalletB.PickedTime = ZDateTimeOffset.Now;

			var transferForPalletC = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPalletC.WD_IsPutawayTransfer = true;
			var transferLineForPalletC = Helper.SetupTransferLineForDockDoorLocation(transferForPalletC, data.Part1, dockDoorLocation, nonDockDoorLocation, "C", 10m);

			transferForPalletC.RunPreSaveValidation();
			transferForPalletC.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, transferForPalletC.IsFinalised);

			receive.AllocateLocationsWithMock(new[] { receivedInventoryLine, puttingAwayInventoryLine, putawayInventoryLine });
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();

			receive.AllocateLocationsWithMock(new[] { puttingAwayInventoryLine, putawayInventoryLine });
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();

			receive.AllocateLocationsWithMock(new[] { putawayInventoryLine });
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();

			receive.AllocateLocationsWithMock(new[] { receivedInventoryLine });
			Assert(!Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
		}

		#endregion

		#region TestAllocateLocations_VirtualWarehouse_DoesNotUsePutawayEngine

		public void TestAllocateLocations_VirtualWarehouse_DoesNotUsePutawayEngine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var mockEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			using (ObjectFactory.Substitute(mockEngine.Object))
			{
				ReceiveAllocationHelper.AllocateLocations(receive, data.Whs1);
			}

			mockEngine
				.Verify(
					m => m.Putaway(
						It.IsAny<IEnumerable<WhsReceive>>(),
						It.IsAny<IEnumerable<WhsReceiveLine>>(),
						It.IsAny<INotifications>(),
						It.IsAny<RefEquipment>(),
						It.IsAny<IEnumerable<ZGuid>>(),
						It.IsAny<bool>(),
						It.IsAny<bool>()), Times.Never);

			AssertEquals("Should not have allocated anything.", ZGuid.Empty, receiveLine.WE_WL);
		}

		#endregion

		#region TestAllocateLocations_VirtualWarehouse_PutsawayToValidLocations

		public void TestAllocateLocations_VirtualWarehouse_PutsawayToValidLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var bondArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var dockDoorType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL));
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WLT_LocationType = dockDoorType.PK;
			location1.WLV_WA_PutawayArea = bondArea.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;

			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_LocationStatus = LocationStatus.Codes.Void;
			location2.WLV_WA_PutawayArea = bondArea.PK;

			var location3 = data.Whs1.FindLocation("A-3");
			location3.WLV_LocationStatus = LocationStatus.Codes.Normal;

			var location4 = data.Whs1.FindLocation("A-4");
			location4.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location4.WLV_WA_PutawayArea = bondArea.PK;

			Factory.Save();

			receive.NotificationManager.Push(Notify);
			ReceiveAllocationHelper.AllocateLocations(receive, data.Whs1);
			AssertEquals("Virtual Warehouse should have allocated the first bonded location with normal status that is not a dock door.", location4.PK, receiveLine.WE_WL);
			AssertEquals("No notifications should be given.", "", Notify.AsString);

			receiveLine.WE_WL = ZGuid.Empty;
			location4.WLV_LocationStatus = LocationStatus.Codes.Damaged;
			Factory.Save();

			ReceiveAllocationHelper.AllocateLocations(receive, data.Whs1);
			Assert("Should have notified that no Location could be found.", Notify.ContainsNotificationType(ReceiveErrorTypes.NoLocationsDefined));
			AssertEquals("Couldn't find valid Location, Location PK should not be set.", ZGuid.Empty, receiveLine.WE_WL);
		}

		#endregion

		#region TestAllocateLocations_VirtualWarehouse_MultipleReceives

		public void TestAllocateLocations_VirtualWarehouse_MultipleReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			receive3.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 10m);

			data.Whs1.WW_IsVirtualWarehouse = true;

			var bondArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var dockDoorType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL));

			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location1.WLV_WA_PutawayArea = bondArea.PK;

			var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WA_PutawayArea = iprArea.PK;
			location2.WLV_WA_PickingArea = iprArea.PK;
			Factory.Save();

			receive1.NotificationManager.Push(Notify);
			ReceiveAllocationHelper.AllocateLocations(receive1, data.Whs1);
			AssertEquals("Virtual Warehouse should have allocated the first bonded location with normal status that is not a dock door.", location1.PK, receiveLine1.WE_WL);
			AssertEquals("No notifications should be given.", "", Notify.AsString);
		}

		#endregion

		#region TestAllocateLocations_VirtualWarehouse_NonCustomsReceipt

		public void TestAllocateLocations_VirtualWarehouse_NonCustomsReceipt()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WA_PutawayArea = iprArea.PK;
			location2.WLV_WA_PickingArea = iprArea.PK;
			Factory.Save();

			ReceiveAllocationHelper.AllocateLocations(receive, data.Whs1);
			AssertEquals("Should have allocated default non bonded location.", data.Whs1.DefaultLocationInNonBondedArea.PK, receiveLine.WE_WL);
		}

		#endregion

		#region TestAllocateLocations_VirtualWarehouse_InwardsProcessing

		public void TestAllocateLocations_VirtualWarehouse_InwardsProcessing()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;

			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WA_PutawayArea = iprArea.PK;
			location2.WLV_WA_PickingArea = iprArea.PK;
			Factory.Save();

			ReceiveAllocationHelper.AllocateLocations(receive, data.Whs1);
			AssertEquals("Should have allocated default inward processing location.", location2.PK, receiveLine.WE_WL);
		}

		#endregion

		#region TestAllocateLocations_VirtualWarehouse_DoesNotOverrideExistingLocation

		public void TestAllocateLocations_VirtualWarehouse_DoesNotOverrideExistingLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var bondArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WA_PutawayArea = bondArea.PK;

			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WA_PutawayArea = bondArea.PK;

			receiveLine.WE_WL = location2.PK;
			Factory.Save();

			ReceiveAllocationHelper.AllocateLocations(receive, data.Whs1);
			AssertEquals("Should not have overriden Location with another one.", location2.PK, receiveLine.WE_WL);
		}

		public void TestAllocateLocations_VirtualWarehouse_OnlyConsidersSuppliedLines_DoesNotOverrideExistingLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var bondArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WA_PutawayArea = bondArea.PK;

			Factory.Save();

			ReceiveAllocationHelper.AllocateLocations(new[] { receive }, data.Whs1, new[] { receiveLine1 }, receive.NotificationSubscriber);
			AssertEquals("Should have set Location for referenced receive line.", location1.PK, receiveLine1.WE_WL);
			AssertEquals("Should not have set Location for other receive lines.", Guid.Empty, receiveLine2.WE_WL);
		}

		#endregion
	}
}
