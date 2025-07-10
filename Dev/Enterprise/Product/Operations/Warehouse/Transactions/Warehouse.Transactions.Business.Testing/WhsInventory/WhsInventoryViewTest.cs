using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsInventoryView))]
	public class WhsInventoryViewTest : WhsBusinessObjectTestCase
	{
		#region TestSetValidationPartAttribWarningMessageBlowsUpIfAttributeNumberOutOfRange

		public void TestSetValidationPartAttribWarningMessageBlowsUpIfAttributeNumberOutOfRange()
		{
			var inventory = Factory.New<WhsInventoryView>();
			AssertExceptionThrown(typeof(ArgumentException), "Valid range for attributes is 1-3.", () => inventory.SetValidationPartAttribWarningMessage(0, "Message1"));
			AssertExceptionThrown(typeof(ArgumentException), "Valid range for attributes is 1-3.", () => inventory.SetValidationPartAttribWarningMessage(4, "Message1"));
		}

		#endregion

		#region TestILocationConsumerMembers

		public void TestILocationConsumerMembers()
		{
			var whs = Helper.CreateWarehouse("WH1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);
			Factory.Save();

			var inventory = Factory.New<WhsInventoryView>();
			var locationPK = row.Locations[0].PK;
			inventory.WI_WL = locationPK;
			var locationConsumer = (ILocationConsumer)inventory;

			AssertEquals(locationConsumer.LocationTypeForMessages, "Inventory");
			AssertEquals(locationConsumer.LocationPK, locationPK);

			locationConsumer.LocationTitle = "Test";
			AssertEquals(locationConsumer.LocationTitle, "Test");
		}

		#endregion

		#region Customs Stuff

		public void TestClassification()
		{
			AssertEquals("CustomsTariffLookup", Inventory.CustomsTariffLookupInfo.Name);
			AssertEquals("CustomsTariffItem", Inventory.CustomsTariffItemInfo.Name);
			AssertEquals("CustomsTariffDesc", Inventory.CustomsTariffDescInfo.Name);

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			Inventory.WI_OP = part.PK;

			BusinessObject classification = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassification>();
			classification[CusClassificationSchema.CC_ClassificationType.Name] = "IMP";
			classification[CusClassificationSchema.CC_LookupCode.Name] = "LOOKUP";
			classification[CusClassificationSchema.CC_TariffNum.Name] = "9901.22.23";
			classification[CusClassificationSchema.CC_Description.Name] = "DESCRIPTION";

			BusinessObject pivot = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassPartPivot>();
			pivot[CusClassPartPivotSchema.CI_CC.Name] = classification.PK;
			pivot[CusClassPartPivotSchema.CI_OP.Name] = part.PK;

			AssertEquals("LOOKUP", Inventory.CustomsTariffLookup);
			AssertEquals("9901.22.23", Inventory.CustomsTariffItem);
			AssertEquals("DESCRIPTION", Inventory.CustomsTariffDesc);
		}

		[ExpectNoExceptions]
		public void TestTariffLookupWhenNullClassification()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			Inventory.WI_OP = part.PK;

			BusinessObject pivot = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassPartPivot>();
			pivot[CusClassPartPivotSchema.CI_CC.Name] = new ZGuid();
			pivot[CusClassPartPivotSchema.CI_OP.Name] = part.PK;

			AssertEquals("No exceptions when referencing CustomsClassification within TariffLookup", ZString.Empty, Inventory.CustomsTariffLookup);
		}

		public void TestCustomsData()
		{
			var receive = GetNewReceive();
			var inventory = receive.Lines.AddNew().Inventory[0];
			AssertEquals(false, inventory.IsRegisteredEditableChildObject(inventory.CustomsData));
		}

		#endregion

		#region Business Object Overrides

		#region TestBusinessObjectsWithRelatedEvents

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var inventory1 = (WhsInventoryView)GetNewBusinessObject();
			AssertContainsExactElementsInAnyOrder(new[] { inventory1.InDocketLine }, inventory1.BusinessObjectsWithRelatedEvents);
		}

		#endregion

		public virtual void TestSetDefaultValues()
		{
			AssertEquals("WI_InventoryStatus should be set to Pending", CodeLists.InventoryStatus.Codes.Pending, Inventory.OriginalInventoryStatus);

			var newInventory = Factory.New<WhsInventoryView>(); // Make without backing WhsDocketLine
			AssertEquals("Should set row defaults to match old behaviour.", CodeLists.InventoryStatus.Codes.Pending, ((IBusinessObjectInternals)newInventory).Row[WhsInventoryViewSchema.Constants.WI_InventoryStatus]);
			AssertEquals("Should set row defaults to match old behaviour.", true, ((IBusinessObjectInternals)newInventory).Row[WhsInventoryViewSchema.Constants.WI_IsOriginalReceiptLine]);
			AssertEquals("Should match old behaviour.", 0m, ((IBusinessObjectInternals)newInventory).Row[WhsInventoryViewSchema.Constants.WI_TotalUnits]);
			AssertEquals("Should match old behaviour.", 0m, ((IBusinessObjectInternals)newInventory).Row[WhsInventoryViewSchema.Constants.WI_InDocketLineUnits]);

			AssertNoExceptionThrown(() => newInventory.WI_TotalUnits = 11.1m);
			AssertNoExceptionThrown(() => newInventory.WI_InDocketLineUnits = 11.1m);
		}

		public virtual void TestLightValidatonDisabled()
		{
			AssertEquals(false, Inventory.LightValidationEnabled);
		}

		public virtual void TestRunLoadValidation()
		{
			OrgHeader org = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			OrgSupplierPart part = Helper.CreateProduct(org, "P1");
			WhsReceive docket = Helper.CreateWhsReceive(org, whs);
			var whsInventoryLine = Helper.CreateWhsReceiveInventoryLine(docket, part, 10);
			Factory.Save();

			AssertEquals("Only one line must exist", 1, docket.Inventory.Count);
			ZGuid whsInventoryLinePK = docket.Inventory[0].PK;

			docket = null;
			BusinessObjectFactory newFactory1 = new BusinessObjectFactory();
			whsInventoryLine = newFactory1.Load<WhsInventoryView>(whsInventoryLinePK);
			AssertEquals("Precondition", part.OP_StockKeepingUnit, whsInventoryLine.WI_F3_NKPackType);
			AssertEquals(whsInventoryLine.WI_InDocketLineUnits, whsInventoryLine.InDocketLine.WE_PackQuantity);

			whsInventoryLine.WI_F3_NKPackType = "CTN";
			whsInventoryLine.InDocketLine.WE_PackQuantity = 10m;
			AssertEquals(120m, whsInventoryLine.WI_InDocketLineUnits);
			newFactory1.Save();

			docket = null;
			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			whsInventoryLine = newFactory2.Load<WhsInventoryView>(whsInventoryLinePK);
			AssertEquals("CTN", whsInventoryLine.WI_F3_NKPackType);
			AssertEquals(120m, whsInventoryLine.WI_InDocketLineUnits);
			AssertEquals(10m, whsInventoryLine.InDocketLine.WE_PackQuantity);
		}

		public void TestHumanReadableShortcutName()
		{
			Inventory.WI_OH_Client = ZGuid.Empty;
			Inventory.WI_OP_PartNum = ZString.Empty;
			Inventory.WI_ArrivalDate = ZDateTimeOffset.Empty;

			AssertEquals("Inventory", Inventory.HumanReadableShortcutName);

			var org = Helper.CreateClient();
			Inventory.WI_OH_Client = org.PK;
			AssertEquals(org.OH_Code, Inventory.HumanReadableShortcutName);

			Inventory.WI_OP_PartNum = "blah";
			AssertEquals(org.OH_Code + " - blah", Inventory.HumanReadableShortcutName);

			Inventory.WI_ArrivalDate = new ZDateTimeOffset(1979, 12, 12);
			AssertEquals(org.OH_Code + " - blah - 12-Dec-79", Inventory.HumanReadableShortcutName);
		}

		public void TestGetLogsParentTableName()
		{
			var inventoryView = (WhsInventoryView)GetNewBusinessObject();
			AssertEquals("LogsParentTableName should be overridden.", WhsDocketLineSchema.Constants.TableName, ((IStmALogParent)inventoryView).LogsParentTableName);
		}

		#region Fetch Strategy

		public void TestGetFetchStrategyIsCorrectType()
		{
			AssertEquals(typeof(WhsInventoryViewFetchStrategy), Inventory.FetchStrategy.GetType());
		}

		#endregion

		#region TestSupportNotes

		public void TestSupportNotes()
		{
			var line = Factory.New<WhsInventoryView>();
			AssertEquals("Notes for inventory are now on DocketLine.", false, line.SupportsNotes);
		}

		#endregion

		#endregion

		#region Delete

		public void TestInventoryIsDeletedWhenDeletingReceiveLineInAnotherFactory()
		{
			if (!WhsEnvironment.IsWebTracker)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				var inventory = receive.Inventory[0];
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
				var receiveLineInNewFactory = receiveInNewFactory.Lines[0];
				var inventoryModule = newFactory.Load<WhsModuleInventory>(inventory.PK);

				Assert(!inventoryModule.IsDeleted);
				receiveLine.Delete();
				Factory.Save();
				Assert("Inventory line should be marked as deleted in other factory", inventoryModule.IsDeleted);
			}
			else
			{
				Assert("DataRefreshManager is disabled in the web environment", true);
			}
		}

		#region TestDelete

		public void TestDelete()
		{
			var docket = Factory.New<WhsReceive>();
			var docketLine1 = docket.Lines.AddNew();
			var docketLine2 = docket.Lines.AddNew();
			var inventory1 = docketLine1.Inventory[0];
			var inventory2 = docketLine2.Inventory[0];

			var orderLine1 = Factory.New<WhsOrder>().Lines.AddNew();
			var orderLine2 = Factory.New<WhsOrder>().Lines.AddNew();
			var pickLine11 = Helper.CreateReservePickLine(orderLine1, inventory1, 1m);
			var pickLine12 = Helper.CreateReservePickLine(orderLine1, inventory1, 1m);
			var pickLine22 = Helper.CreateReservePickLine(orderLine2, inventory2, 1m);

			inventory1.Delete();

			AssertEquals(true, docketLine1.IsDeleted);
			AssertEquals(false, docketLine2.IsDeleted);
			AssertEquals(true, pickLine11.IsDeleted);
			AssertEquals(true, pickLine12.IsDeleted);
			AssertEquals(false, pickLine22.IsDeleted);
			AssertEquals(false, orderLine1.IsDeleted);
			AssertEquals(false, orderLine2.IsDeleted);

			inventory2.Delete(); // ensure no stack overflow

			AssertEquals(true, docketLine1.IsDeleted);
			AssertEquals(true, docketLine2.IsDeleted);
			AssertEquals(true, pickLine22.IsDeleted);
		}

		#endregion

		#region TestDeleteAndThenLoad_CreatedAndSavedInSameFactory_DeleteAfterSave

		public void TestDeleteAndThenLoad_CreatedAndSavedInSameFactory_DeleteAfterSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			inventory.Delete(); // Defect case: would previously cause a row state of Detached which blew up when Loading using a DbOnlyQuery
			AssertNoExceptionThrown(() => Factory.Load<WhsInventoryView>(new ZDBOnlyQuery(typeof(WhsInventoryView)).AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, 15m)));
		}

		#endregion

		#region TestDelete_DoesNotDeleteDocketLinesOnFinalisedOrCancelledDockets

		public void TestDelete_DoesNotDeleteDocketLinesOnFinalisedOrCancelledDockets()
		{
			var receive = Factory.New<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();
			var inventory = receiveLine.Inventory[0];
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			inventory.Delete();
			AssertEquals(false, receiveLine.IsDeleted);

			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			inventory.Delete();
			AssertEquals(false, receiveLine.IsDeleted);
		}

		#endregion

		#region TestDelete_DoesDeleteUnattachedDocketLines

		public void TestDelete_DoesDeleteUnattachedDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			Factory.Save();

			var docketLine = inventory1.InDocketLine;
			inventory1.Delete();
			AssertEquals(true, docketLine.IsDeleted);
		}

		#endregion

		#region TestDelete_UpdatesDocketsTotalWeightAndVolume_WithDocketLines

		public void TestDelete_UpdatesDocketsTotalWeightAndVolume_WithDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, Constants.Weight.Kilograms, 0.1m, Constants.Volume.CubicMetres);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			AssertEquals("Precondition", 70m, receive.WD_TotalWeight);
			AssertEquals("Precondition", 3.5m, receive.WD_TotalCubic);

			receive.WD_TotalWeight = 100m;
			receive.WD_TotalCubic = 5m;
			AssertEquals("Precondition", 100m, receive.WD_TotalWeight);
			AssertEquals("Precondition", 5m, receive.WD_TotalCubic);

			inventory1.Delete();
			AssertEquals(80m, receive.WD_TotalWeight);
			AssertEquals(4m, receive.WD_TotalCubic);

			inventory2.Delete();
			AssertEquals(40m, receive.WD_TotalWeight);
			AssertEquals(2m, receive.WD_TotalCubic);

			inventory3.Delete();
			AssertEquals(30m, receive.WD_TotalWeight);
			AssertEquals(1.5m, receive.WD_TotalCubic);
		}

		#endregion

		#endregion

		#region TestFinaliseReceiveInactiveProducts

		public void TestFinaliseReceiveInactiveProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			var inventory = receive.Inventory[0];
			var line = receive.Lines[0];
			Helper.CreateAsnLine(receive, data.Part1, 10m);
			Factory.Save();

			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receive.FinaliseDocket();
				AssertNoError(inventory.WI_OPInfo, "This Product is inactive - it may not be used.");
			}

			data.Part1.OP_IsActive = false;
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receive.FinaliseDocket();
				AssertHasError(inventory.WI_OPInfo, "This Product is inactive - it may not be used.");
			}

			line.WE_TransactionQuantity = 0m;
			Factory.Save();
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receive.FinaliseDocket();
				AssertNoError(inventory.WI_OPInfo, "This Product is inactive - it may not be used.");
			}
		}

		#endregion

		#region Related Entities

		#region TestAllPickLines

		public void TestAllPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			AssertEquals("No Pick Lines attached to Inventory.", 0, inventory.AllPickLines.Count());

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, inventory.AllPickLines);

			var inDocketLinePK = inventory.WI_WE_InDocketLine;
			inventory.WI_WE_InDocketLine = ZGuid.Empty;
			AssertEquals("Inventory has no DocketLine FK, AllPickLines should be empty.", 0, inventory.AllPickLines.Count());

			inventory.WI_WE_InDocketLine = inDocketLinePK; // cleanup
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, inventory.AllPickLines);

			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertContainsExactElementsInAnyOrder("AllPickLines should include Picked PickLines.", orderLine.PickLines, inventory.AllPickLines);
		}

		#endregion

		#region TestAllPickLines_FetchOnlyFromLocalCache

		public void TestAllPickLines_FetchOnlyFromLocalCache()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			rowFactory.ResetDatabaseLoadCount();
			rowFactory.ClearQueryCache();
			AssertEquals("No Pick Lines attached to Inventory.", 0, inventory.AllPickLines.Count());
			AssertEquals("Should *not* have gone to the database.", 0, rowFactory.DatabaseLoadCount);
		}

		public void TestAllPickLines_FetchOnlyFromLocalCache_WithPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = inventory.PK;

			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			rowFactory.ResetDatabaseLoadCount();
			rowFactory.ClearQueryCache();
			AssertContainsExactElementsInAnyOrder("No Pick Lines attached to Inventory.", new[] { pickLine }, inventory.AllPickLines);
			AssertEquals("Should *not* have gone to the database.", 0, rowFactory.DatabaseLoadCount);
		}

		#endregion

		#region TestAllPickLines_InNewFactory

		public void TestAllPickLines_InNewFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			AssertEquals("No Pick Lines attached to Inventory.", 0, inventory.AllPickLines.Count());

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, inventory.AllPickLines);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventoryInNewFactory = newFactory.Load<WhsInventoryView>(inventory.PK);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines.Select(pl => pl.PK), inventoryInNewFactory.AllPickLines.Select(pl => pl.PK));
		}

		public void TestAllPickLines_InNewFactory_NoInDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			AssertEquals("No Pick Lines attached to Inventory.", 0, inventory.AllPickLines.Count());

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, inventory.AllPickLines);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventoryInNewFactory = newFactory.Load<WhsInventoryView>(inventory.PK);
			inventoryInNewFactory.WI_WE_InDocketLine = ZGuid.NewZGuid();
			AssertEquals(0, inventoryInNewFactory.AllPickLines.Select(pl => pl.PK).Count());
		}

		#endregion

		#region TestCommittedPickLines

		public void TestCommittedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			AssertEquals("No Pick Lines attached to Inventory.", 0, inventory.CommittedPickLines.Count());

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, inventory.CommittedPickLines);

			var inDocketLinePK = inventory.WI_WE_InDocketLine;
			inventory.WI_WE_InDocketLine = ZGuid.Empty;
			AssertEquals("Inventory has no DocketLine FK, AllPickLines should be empty.", 0, inventory.CommittedPickLines.Count());

			inventory.WI_WE_InDocketLine = inDocketLinePK; // cleanup
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, inventory.CommittedPickLines);

			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("CommittedPickLines should not include Picked PickLines.", 0, inventory.CommittedPickLines.Count());
		}

		#endregion

		#region TestCommittedToPickOrAdjustmentQuantity_TransactionLineDeletedFromAnotherFactory

		public void TestCommittedToPickOrAdjustmentQuantity_TransactionLineDeletedFromAnotherFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, inventory.CommittedPickLines);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventoryInNewFactory = newFactory.Load<WhsInventoryView>(inventory.PK);

			// inventory.CommittedPickLines call meant to cache the pick lines on the inventory
			// so that inventory.CommittedPickLines still returns a pickline even after the order line gets deleted
			AssertEquals("Inventory has committed pick lines.", 1, inventoryInNewFactory.CommittedPickLines.Count());

			orderLine.Delete();
			Factory.Save();
			AssertNoExceptionThrown("No exception is thrown while calculating the committed quantity.", () => { var committedQty = inventoryInNewFactory.CommittedQuantityIncludingUnfinalisedReceipt; });
		}

		#endregion

		#region TestSupplierPart

		public void TestSupplierPart()
		{
			OrgSupplierPart prod = Factory.New<OrgSupplierPart>();
			Inventory.WI_OP = prod.PK;
			AssertEquals(prod.PK, Inventory.SupplierPart.PK);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			AssertNull(Inventory.Warehouse);

			WhsWarehouse whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			WhsWarehouse whs2 = Helper.CreateWarehouse("WHS2", "B", 2, 1);

			WhsReceive receive = Factory.New<WhsReceive>();
			receive.WD_WW_Whs = whs1.PK;
			Inventory.WI_WD = receive.PK;
			AssertEquals(whs1, Inventory.Warehouse);

			Inventory.WI_WL = whs1.Rows[0].Locations[0].PK;
			AssertEquals(whs1, Inventory.Warehouse);

			Inventory.WI_WL = whs2.Rows[0].Locations[0].PK;
			AssertEquals(whs2, Inventory.Warehouse);
		}

		#endregion

		#region TestInDocketLine

		#region TestInDocketLine

		public void TestInDocketLine()
		{
			var receiveLine = GetNewReceiveLine();
			var transferLine = GetNewTransferLine();
			var adjustmentLine = GetNewAdjustmentLine();

			var inventory = receiveLine.Inventory[0];
			AssertEquals(receiveLine.PK, inventory.InDocketLine.PK);

			inventory.WI_InDocketLineType = DocketType.Codes.Transfer;
			inventory.WI_WE_InDocketLine = transferLine.PK;
			AssertEquals(transferLine.PK, inventory.InDocketLine.PK);

			inventory.WI_InDocketLineType = DocketType.Codes.Adjustment;
			inventory.WI_WE_InDocketLine = adjustmentLine.PK;
			AssertEquals(adjustmentLine.PK, inventory.InDocketLine.PK);
		}

		#endregion

		#region TestInDocketLine_ThrowsExceptionIfAttachedToAnOrderLine

		[ExpectException(typeof(InvalidOperationException))]
		public void TestInDocketLine_ThrowsExceptionIfAttachedToAnOrderLine()
		{
			var inventory = GetNewReceiveLine().Inventory[0];
			inventory.WI_InDocketLineType = DocketType.Codes.Order;
			_ = inventory.InDocketLine;
		}

		#endregion

		#region TestInDocketLine_ThrowsExceptionIfAttachedToAWorkOrderLine

		[ExpectException(typeof(InvalidOperationException))]
		public void TestInDocketLine_ThrowsExceptionIfAttachedToAWorkOrderLine()
		{
			var inventory = GetNewReceiveLine().Inventory[0];
			inventory.WI_InDocketLineType = DocketType.Codes.WorkOrder;
			_ = inventory.InDocketLine;
		}

		#endregion

		#region TestInDocketLine_ThrowsExceptionIfAttachedToAnInvalidDocketType

		[ExpectException(typeof(InvalidOperationException))]
		public void TestInDocketLine_ThrowsExceptionIfAttachedToAnInvalidDocketType()
		{
			var inventory = GetNewReceiveLine().Inventory[0];
			inventory.WI_InDocketLineType = "XXX";
			_ = inventory.InDocketLine;
		}

		#endregion

		#region TestInDocketLine_ThrowsExceptionIfAttachedToAnEmptyDocketType

		[ExpectException(typeof(InvalidOperationException))]
		public void TestInDocketLine_ThrowsExceptionIfAttachedToAnEmptyDocketType()
		{
			var inventory = GetNewReceiveLine().Inventory[0];
			inventory.WI_InDocketLineType = "";
			_ = inventory.InDocketLine;
		}

		#endregion

		#endregion

		#region TestDocket

		public void TestDocket()
		{
			var receive = GetNewReceive();
			var line = receive.Lines.AddNew();
			var inventory = line.Inventory[0];
			AssertEquals(line.Docket, inventory.Docket);

			var docket2 = GetNewReceive();
			inventory.WI_WD = docket2.PK;
			AssertEquals("WI_WD has preference over InDocketLine.Docket", docket2, inventory.Docket);
		}

		#endregion

		#region TestDocketOriginal

		public void TestDocketOriginal()
		{
			var receive1 = GetNewReceive();
			var receiveLine1 = receive1.Lines.AddNew();
			var inventory = receive1.Inventory[0];
			AssertEquals("If not setup of docketLine should be taken from inventory.", receive1.PK, inventory.DocketOriginal.PK);

			var receive2 = GetNewReceive();
			var line2 = receive2.Lines.AddNew();
			inventory.WI_WE_OriginalInDocketLineForRating = line2.PK;
			AssertEquals("OrigInDocketLine.Docket has overall preference", receive2.PK, inventory.DocketOriginal.PK);
		}

		#endregion

		#region TestReservedPickLines

		public void TestReservedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			var inventory = receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.Location, inventory.Location);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 5m, transferLine.QtyCommittedIncludingMatchingLines);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 6m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);

			var reservedPickLine1 = orderLine1.ReserveStockIfAbleTo(inventory);
			var reservedPickLine2 = orderLine2.ReserveStockIfAbleTo(inventory);

			AssertContainsExactElementsInAnyOrder(new[] { reservedPickLine1, reservedPickLine2 }, inventory.ReservedPickLines);
			AssertEquals(true, inventory.IsRegisteredEditableChildObject(inventory.ReservedPickLines));
			AssertEquals(10m, inventory.WI_CrossDockQuantity);

			Helper.CreatePickNew(order1);
			AssertContainsExactElementsInAnyOrder(new[] { reservedPickLine2 }, inventory.ReservedPickLines);
			AssertEquals(4m, inventory.WI_CrossDockQuantity);
		}

		#region TestReservedPickLines_NotCachedIfDocketLineIsEmpty

		public void TestReservedPickLines_NotCachedIfDocketLineIsEmpty()
		{
			var inventory = Factory.New<WhsInventoryView>();
			AssertEquals("Precondition", ZGuid.Empty, inventory.WI_WE_InDocketLine);

			var reservedPickLines1 = inventory.ReservedPickLines;
			AssertNotNull(reservedPickLines1);
			AssertEquals(0, reservedPickLines1.Count);

			var reservedPickLines2 = inventory.ReservedPickLines;
			AssertNotNull(reservedPickLines2);
			AssertEquals(0, reservedPickLines2.Count);

			AssertNotEquals("When WI_WE_InDocketLine is empty, reserved picklines should not be cached", reservedPickLines1, reservedPickLines2);
		}

		#endregion

		#endregion

		#region TestPackageDetails

		public void TestPackageDetails()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.FindLocation("A"), "");
			var inventory = receive.Inventory[0];
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var orderPackage1 = packageJob1.Packages.AddNew("PLT", "o11");
			var orderPackage2 = packageJob1.Packages.AddNew("PLT", "o12");

			Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines.AddNew();
			pickLine1.WZ_Units = 5m;
			pickLine1.WZ_WE_InventoryLine = receive.Lines[0].PK;
			var pickLine2 = orderLine.PickLines.AddNew();
			pickLine2.WZ_Units = 5m;
			pickLine2.WZ_WE_InventoryLine = receive.Lines[0].PK;

			var divot1 = orderPackage1.PackedItemDivots.AddNew();
			divot1.KI_ParentTableCode = WhsPickLineSchema.Constants.Prefix;
			divot1.KI_ParentID = pickLine1.PK;
			divot1.KI_PackedQty = pickLine1.WZ_Units;
			divot1.KI_ParentTableCode = pickLine1.TablePrefix;

			var divot2 = orderPackage2.PackedItemDivots.AddNew();
			divot2.KI_ParentID = pickLine2.PK;
			divot2.KI_PackedQty = pickLine2.WZ_Units;
			divot2.KI_ParentTableCode = pickLine2.TablePrefix;
			Factory.Save();

			AssertEquals(2, inventory.PackageDetails.Count);
			AssertEquals("o11", inventory.PackageDetails[0].PackageID);
			AssertEquals("O1", inventory.PackageDetails[0].OrderNumber);
			AssertEquals(ZBool.False, inventory.PackageDetails[0].IsTote);

			AssertEquals("o12", inventory.PackageDetails[1].PackageID);
			AssertEquals("O1", inventory.PackageDetails[1].OrderNumber);
			AssertEquals(ZBool.False, inventory.PackageDetails[1].IsTote);
		}

		public void TestPackageDetailsWithToteTrue()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			receive1.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var order1Package = packageJob1.Packages.AddNew("PLT");
			order1Package.KP_PackageID = "o11";
			order1Package.SetIsTote(true);

			Helper.CreatePickNew(order1);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var divot1 = Factory.New<PkgPackageItemDivot>();
			divot1.KI_ParentID = pickLine1.PK;
			divot1.KI_KP_Package = order1Package.PK;
			divot1.KI_PackedQty = 5m;
			divot1.KI_ParentTableCode = pickLine1.TablePrefix;
			Factory.Save();

			AssertEquals(1, pickLine1.Inventory.PackageDetails.Count);
			AssertEquals("o11", pickLine1.Inventory.PackageDetails[0].PackageID);
			AssertEquals("O1", pickLine1.Inventory.PackageDetails[0].OrderNumber);
			AssertEquals(ZBool.True, pickLine1.Inventory.PackageDetails[0].IsTote);
		}

		public void TestPackageDetailsWithReleaseCapturedAttribs()
		{
			var today = ZDate.Today;
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");

			Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, false);
			Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, false);

			Helper.SetProductAttributeUse(client, product, AttributeNumber.One, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Two, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Three, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.ExpiryDate, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.PackingDate, true, true);

			var receive = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m);
			var receiveline = receive.Lines[0];
			receiveline.WE_ExpiryDate = today.AddDays(+10);
			receiveline.WE_PackingDate = today.AddDays(-5);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, product, 10m);
			var orderline = order.Lines[0];
			orderline.WE_ExpiryDate = today.AddDays(+10);
			orderline.WE_PackingDate = today.AddDays(-5);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var orderPackage1 = packageJob1.Packages.AddNew("PLT", "o11");
			var orderPackage2 = packageJob1.Packages.AddNew("PLT", "o12");

			Helper.CreatePickNew(order);
			var pickLine1 = orderline.PickLines[0];
			pickLine1.WZ_Units = 5m;
			pickLine1.WZ_WE_InventoryLine = receive.Lines[0].PK;
			var pickLine2 = orderline.PickLines.AddNew();
			pickLine2.WZ_Units = 5m;
			pickLine2.WZ_WE_InventoryLine = receive.Lines[0].PK;
			pickLine1.WZ_ReleaseCapturedPartAttrib1 = "PA1";
			pickLine1.WZ_ReleaseCapturedPartAttrib2 = "PA2";
			pickLine1.WZ_ReleaseCapturedPartAttrib3 = "PA3";
			pickLine2.WZ_ReleaseCapturedPartAttrib1 = "PA1";
			pickLine2.WZ_ReleaseCapturedPartAttrib2 = "PA2";
			pickLine2.WZ_ReleaseCapturedPartAttrib3 = "PA3";
			Factory.Save();

			var divot1 = orderPackage1.PackedItemDivots.AddNew();
			divot1.KI_ParentID = pickLine1.PK;
			divot1.KI_PackedQty = pickLine1.WZ_Units;
			divot1.KI_ParentTableCode = pickLine1.TablePrefix;

			var divot2 = orderPackage2.PackedItemDivots.AddNew();
			divot2.KI_ParentID = pickLine2.PK;
			divot2.KI_PackedQty = pickLine2.WZ_Units;
			divot2.KI_ParentTableCode = pickLine2.TablePrefix;
			Factory.Save();

			AssertEquals(2, pickLine1.Inventory.PackageDetails.Count);
			AssertEquals("o11", pickLine1.Inventory.PackageDetails[0].PackageID);
			AssertEquals("TEST", pickLine1.Inventory.PackageDetails[0].OrderNumber);
			AssertEquals(ZBool.False, pickLine1.Inventory.PackageDetails[0].IsTote);

			AssertEquals("o12", pickLine1.Inventory.PackageDetails[1].PackageID);
			AssertEquals("TEST", pickLine1.Inventory.PackageDetails[1].OrderNumber);
			AssertEquals(ZBool.False, pickLine1.Inventory.PackageDetails[1].IsTote);
		}

		public void TestPackageDetails_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1001m);
			receive.FinaliseDocket();
			Factory.Save();

			for (int count = 0; count < 10; count++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + count.ToString(), Notify);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
				var orderPackage = packageJob.Packages.AddNew("PLT", "package" + count.ToString());

				Helper.CreatePickNew(order);
				var pickLine = orderLine.PickLines[0];
				pickLine.WZ_Units = 5m;
				pickLine.WZ_WE_InventoryLine = receive.Lines[0].PK;

				var divot = orderPackage.PackedItemDivots.AddNew();
				divot.KI_ParentTableCode = WhsPickLineSchema.Constants.Prefix;
				divot.KI_ParentID = pickLine.PK;
				divot.KI_PackedQty = pickLine.WZ_Units;
				divot.KI_ParentTableCode = pickLine.TablePrefix;
			}
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			{
				var inventoryInOtherFactory = otherFactory.Load<WhsInventoryView>(receive.Inventory.Single().PK);
				var packageDetails = inventoryInOtherFactory.PackageDetails;
				AssertEquals(10, packageDetails.Count);
			}
		}

		#endregion

		#region TestSupplier

		public void TestSupplier()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var supplier = Helper.CreateClient("Supplier123");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Supplier should come from the Receive.", supplier.PK, inventory.Supplier.PK);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.Location, data.Whs1.FindLocation("A-2"));
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			AssertEquals("Original Inventory should have been reduced to 0.", 0m, inventory.WI_TotalUnits);

			var newInventory = (WhsInventoryView)transferLine.Inventory.Single();
			AssertNotEquals("Precondition", inventory.PK, newInventory.PK);
			AssertEquals("Inventory not created on Receives should still get the Supplier from the Original Receive.", supplier.PK, newInventory.Supplier.PK);
		}

		#endregion

		#endregion

		#region Cloning

		#region TestSupportsCloneCore

		public void TestSupportsCloneCore()
		{
			AssertEquals(true, Inventory.SupportsClone());
		}

		#endregion

		#region TestCloneInternal

		#region TestCloneInternal

		public void TestCloneInternal()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDateTime.Today;

			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(docket, data.Part1, 10m);
			inventory.WI_CustomAttrib_1 = "CA1";
			inventory.WI_LineNo = 5;
			inventory.WI_SubLineNo = 4;
			inventory.WI_ReceiveCrossDockOrderNo = "ORDER1";

			var clone = (WhsInventoryView)inventory.Clone();
			AssertNotNull(clone);
			AssertNotEquals("CloneInternalNeedsOverride " + inventory.GetType().FullName, ErrorReporter.LastKeyReported);
			AssertEquals("Wrong Custom Attribute 1", "CA1", clone.WI_CustomAttrib_1);
			AssertEquals("Wrong Line No", (ZShort)5, clone.WI_LineNo);
			AssertEquals("Wrong Sub Line No", (ZShort)5, clone.WI_SubLineNo);
			AssertEquals("Wrong Order No", "ORDER1", clone.WI_ReceiveCrossDockOrderNo);
		}

		#endregion

		#region TestCloneInternal_UpdatingTotals

		public void TestCloneInternal_UpdatingTotals()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 5m, "KG", 0.05m, "M3");

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertEquals("Precondition - Total Weight should be set correctly.", 50m, receive.WD_TotalWeight);
			AssertEquals("Precondition - Total Volume should be set correctly.", 0.5m, receive.WD_TotalCubic);
			AssertEquals("Precondition - Total Line Units should be set correctly.", 10m, receive.WD_TotalUnitsFromLines);

			var clone = (WhsInventoryView)inventory.Clone();
			AssertEquals("Total Weight should be updated after clonning.", 100m, receive.WD_TotalWeight);
			AssertEquals("Total Volume should be updated after clonning.", 1m, receive.WD_TotalCubic);
			AssertEquals("Total Line Units should be updated after clonning.", 20m, receive.WD_TotalUnitsFromLines);
		}

		#endregion

		#region TestCloneInternal_ClonesCustomsDataAndUSBondedColumns

		public void TestCloneInternal_ClonesCustomsDataAndUSBondedColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "ABC", 2m);
			Factory.Save();

			var clone = (WhsInventoryView)inventory.Clone();
			AssertEquals("123", clone.CustomsData.WB_EntryKey);
			AssertEquals((ZShort)1, clone.CustomsData.WB_EntryLineNo);
			AssertNotEquals(inventory.CustomsData.PK, clone.CustomsData.PK);
			AssertEquals("ABC", clone.PackageGroupId);
			AssertEquals(2m, clone.PerPackageQty);
		}

		#endregion

		#endregion

		#endregion

		#region Validation

		public void TestValidation()
		{
			AssertEquals(GetExpectedValidationType(), Inventory.Validation.GetType());
		}

		public void TestValidationUS()
		{
			US.Testing.WhsTestHelperFunctionsUS helper = new US.Testing.WhsTestHelperFunctionsUS(Factory);
			WhsWarehouse whs = helper.CreateWarehouse("WHS");
			WhsRow row = helper.CreateRow(whs, "ROW");
			WhsLocation location = row.Locations.AddNew();
			Inventory.WI_WL = location.PK;
			AssertEquals(GetExpectedUSValidationType(), Inventory.Validation.GetType());
		}

		#endregion

		#region Properties

		#region TestIsExpired

		[TestDate(2022, 02, 09)]
		public void TestIsExpired()
		{
			var today = ZDate.Today;

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			inventory.WI_ExpiryDate = ZDate.Empty;
			AssertEquals("Expire Date Empty", false, inventory.IsExpired);

			inventory.WI_ExpiryDate = today;
			AssertEquals("Expire Today", true, inventory.IsExpired);

			inventory.WI_ExpiryDate = today.AddDays(-1);
			AssertEquals("Expire Yesterday", true, inventory.IsExpired);

			inventory.WI_ExpiryDate = today.AddDays(1);
			AssertEquals("Expire Tomorrow", false, inventory.IsExpired);
		}

		#endregion

		#region TestIsDamaged

		public void TestIsDamaged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			AssertEquals("Precondition.", false, inventory.IsDamaged);

			inventory.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			inventory.WI_HeldCode = InventoryHoldCodes.Codes.Held;
			AssertEquals(false, inventory.IsDamaged);

			inventory.WI_HeldCode = "AAA";
			AssertEquals(false, inventory.IsDamaged);

			inventory.WI_HeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertEquals(true, inventory.IsDamaged);

			inventory.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Arrived; // Can have a HoldCode + non available inventory status
			AssertEquals(true, inventory.IsDamaged);

			inventory.InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available; // This is not valid, should short circuit to avoid loading DocketLine + HoldCode
			AssertEquals(false, inventory.IsDamaged);
		}

		#endregion

		#region TestSettingWI_InDocketLineUnitsSetsWE_PackQuantity

		public void TestSettingWI_InDocketLineUnitsSetsWE_PackQuantity()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var part = Helper.CreateProduct(org, "P1");
			var docket = Helper.CreateWhsReceive(org, whs);
			var inventory = Helper.CreateWhsReceiveInventoryLine(docket, part, 10);

			inventory.WI_InDocketLineUnits = 5m;
			AssertEquals(5m, inventory.InDocketLine.WE_PackQuantity);

			inventory.WI_InDocketLineUnits = 10m;
			AssertEquals(10m, inventory.InDocketLine.WE_PackQuantity);
		}

		#endregion

		#region TestWI_WW_Whs

		public void TestWI_WW_Whs()
		{
			var inventory = Factory.New<WhsInventoryView>();
			AssertEquals(ZGuid.Empty, inventory.WI_WW_Whs);
			var receive = Factory.New<WhsReceive>();
			var whs = Factory.New<WhsWarehouse>();
			receive.WD_WW_Whs = whs.PK;

			inventory.WI_WD = receive.PK;
			AssertEquals(receive.WD_WW_Whs, inventory.WI_WW_Whs);
		}

		public void TestWI_WW_WhsInfo()
		{
			var inventory = Factory.New<WhsInventoryView>();
			AssertEquals(WhsInventoryView.Schema.WI_WW_Whs, inventory.WI_WW_WhsInfo.Name);
		}

		#endregion

		#region CountryCode

		public void TestCountryCode()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS");
			WhsRow row = Helper.CreateRow(whs, "ROW");
			WhsLocation location = row.Locations.AddNew();
			Inventory.WI_WL = location.PK;

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Constants.CountryCodes.Australia).RL_Code;
			AssertEquals(Constants.CountryCodes.Australia, Inventory.CountryCode);

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Constants.CountryCodes.UnitedStates).RL_Code;
			AssertEquals(Constants.CountryCodes.UnitedStates, Inventory.CountryCode);

			Inventory.WI_WL = ZGuid.Empty;
			AssertEquals(ZString.Empty, Inventory.CountryCode);
		}

		#endregion

		#region Flags

		#region TestHasStockIncludingNotYetFinalised

		public void TestHasStockIncludingNotYetFinalised_WithNoDocket()
		{
			var inventory = Factory.New<WhsInventoryView>();
			inventory.WI_TotalUnits = 1m;
			inventory.WI_InDocketLineUnits = 1;
			AssertEquals(false, inventory.HasStockIncludingNotYetFinalised);
		}

		public void TestHasStockIncludingNotYetFinalised_WithUnFinalisedDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_InDocketLineUnits = 1m;
			Factory.Save();
			AssertEquals("Precondition - ensure Receive is not Finalised", false, receive.IsFinalised);
			AssertEquals(true, inventory.HasStockIncludingNotYetFinalised);

			inventory.WI_InDocketLineUnits = 0m;
			AssertEquals(false, inventory.HasStockIncludingNotYetFinalised);
		}

		public void TestHasStockIncludingNotYetFinalised_WithFinalisedDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1-1"));
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals(true, inventory.HasStockIncludingNotYetFinalised);

			inventory.WI_TotalUnits = 0m;
			AssertEquals(false, inventory.HasStockIncludingNotYetFinalised);
		}

		public void TestHasStockIncludingNotYetFinalised_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "", locationB, "", picker);
			Factory.Save();

			var inventory = transferLine.Inventory[0];
			AssertEquals("Precondition: In-Transit.", InventoryStatus.Codes.InTransit, inventory.WI_InventoryStatus);
			AssertEquals("Should have (unfinalised) stock.", true, inventory.HasStockIncludingNotYetFinalised);

			inventory.WI_TotalUnits = 0m;
			AssertEquals("Should *not* have stock.", false, inventory.HasStockIncludingNotYetFinalised);
		}

		public void TestHasStockIncludingNotYetFinalised_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.FindLocation("A"), "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order1);

			var pickLine = order1.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var inventory = transferLine.Inventory[0];
			AssertEquals("Precondition: Staged.", InventoryStatus.Codes.Staged, inventory.WI_InventoryStatus);
			AssertEquals("Should have (finalised) stock.", true, inventory.HasStockIncludingNotYetFinalised);

			inventory.WI_TotalUnits = 0m;
			AssertEquals("Should *not* have stock.", false, inventory.HasStockIncludingNotYetFinalised);
		}

		#endregion

		#region TestIsAvailable

		public void TestIsAvailable()
		{
			var inventory = Factory.New<WhsInventoryView>();
			inventory.WI_InventoryStatus = InventoryStatus.Codes.Available;
			AssertEquals(true, inventory.IsAvailable);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Held;
			AssertEquals(false, inventory.IsAvailable);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Putaway;
			AssertEquals(false, inventory.IsAvailable);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.InTransit;
			AssertEquals(false, inventory.IsAvailable);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.PuttingAway;
			AssertEquals(false, inventory.IsAvailable);
		}

		#endregion

		#region TestIsHeld

		public void TestIsHeld()
		{
			var inventory = Factory.New<WhsInventoryView>();
			inventory.WI_InventoryStatus = InventoryStatus.Codes.Held;
			AssertEquals(true, inventory.IsHeld);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Available;
			AssertEquals(false, inventory.IsHeld);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Putaway;
			AssertEquals(false, inventory.IsHeld);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.InTransit;
			AssertEquals(false, inventory.IsHeld);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.PuttingAway;
			AssertEquals(false, inventory.IsHeld);
		}

		#endregion

		#region TestIsPickByBOMKitInventory

		public void TestIsPickByBOMKitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var cacheKey = "WhsReceiveLine|IsCreatedFromPickByBOMCore|" + receive.PK;
			Factory.Save();

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Pending;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(false, inventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Putaway;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(false, inventory.IsPickByBOMKitInventory);

			var pick = Factory.New<WhsPick>();
			receive.WD_WP_ParentPickForReceive = pick.PK;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(true, inventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.InTransit;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(false, inventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.PuttingAway;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(false, inventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Held;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(false, inventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.ReadyToPack;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(false, inventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Received;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(false, inventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Staged;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(false, inventory.IsPickByBOMKitInventory);
		}

		#endregion

		#region TestIsInTransit

		public void TestIsInTransit()
		{
			var inventory = Factory.New<WhsInventoryView>();
			inventory.WI_InventoryStatus = InventoryStatus.Codes.Available;
			AssertEquals(false, inventory.IsInTransit);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Held;
			AssertEquals(false, inventory.IsInTransit);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Putaway;
			AssertEquals(false, inventory.IsInTransit);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.InTransit;
			AssertEquals(true, inventory.IsInTransit);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.PuttingAway;
			AssertEquals(true, inventory.IsInTransit);
		}

		#endregion

		#region TestIsDocketLineFinalised

		public void TestIsDocketLineFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();
			AssertEquals("IsDocketLineFinalised should be false if the docket is not finalised.", false, inventory.IsDocketLineFinalised);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("IsDocketLineFinalised should be true if the docket is finalised.", true, inventory.IsDocketLineFinalised);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			AssertEquals("IsDocketLineFinalised should be true if Inventory is on a finalised Transfer Line.", true, transferLine1.Inventory[0].IsDocketLineFinalised);
		}

		#endregion

		#region TestIsInventoryEditForm

		public void TestIsInventoryEditForm()
		{
			var receive = GetNewReceive();
			var inventory = receive.Lines.AddNew().Inventory[0];
			AssertEquals(false, inventory.IsInventoryEditForm);

			inventory.IsInventoryEditForm = true;
			AssertEquals(true, inventory.IsInventoryEditForm);

			inventory.IsInventoryEditForm = false;
			AssertEquals(false, inventory.IsInventoryEditForm);
		}

		#endregion

		#region TestIsAllocatedToPickFace

		public void TestIsAllocatedToPickFace()
		{
			OrgHeader client = Helper.CreateClient("Client");
			WhsWarehouse whs = Helper.CreateWarehouse("Warehouse");
			WhsRow row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			OrgSupplierPart part = Helper.CreateProduct(client, "P1");
			Inventory.WI_OP = part.PK;
			Inventory.WI_WL = row.Locations[0].PK;
			WhsPickFace pickFace = Helper.CreateProductPickFace(inventory.Product, client, row.Locations[4]);
			Inventory.Product.PickFaces.Add(pickFace);

			AssertEquals("This location should not be a PickFace", false, Inventory.IsAllocatedToPickFace());
			Inventory.WI_WL = row.Locations[4].PK;
			AssertEquals("This location should be a PickFace", true, Inventory.IsAllocatedToPickFace());
		}

		#endregion

		#region TestHasEDocsOrNotesAttached

		public void TestHasEDocsOrNotesAttached()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var inventory = receiveLine.Inventory[0];
			AssertEquals(false, inventory.HasEDocsOrNotesAttached);

			receiveLine.Notes.AddNew();
			AssertEquals(true, inventory.HasEDocsOrNotesAttached);

			receiveLine.Notes.RemoveAndDeleteAll();
			AssertEquals(false, inventory.HasEDocsOrNotesAttached);
		}

		#endregion

		#region TestHasEDocsOrNotesAttachedInfo

		public void TestHasEDocsOrNotesAttachedInfo()
		{
			AssertEquals("HasEDocsOrNotesAttached", Inventory.HasEDocsOrNotesAttachedInfo.Name);
		}

		#endregion

		#region TestIsTemporaryProduct

		public void TestIsTemporaryProduct()
		{
			TestDataSimpleEnvironment data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			AssertEquals(false, inventory.IsTemporaryProduct);

			inventory.WI_OP = ZGuid.Invalid;
			AssertEquals(false, inventory.IsTemporaryProduct);

			inventory.WI_OP_PartNum = "NewProduct";
			AssertEquals(true, inventory.IsTemporaryProduct);
		}

		#endregion

		#endregion

		#region Calculated Quantities

		#region TestWI_CrossDockQuantity

		public void TestWI_CrossDockQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			Helper.CreateReservePickLine(orderLine1, data.Line111, 5m);
			Helper.CreateReservePickLine(orderLine2, data.Line111, 2m);

			AssertEquals(7m, data.Line111.WI_CrossDockQuantity);
		}

		#endregion

		#region TestWI_AvailableToTransferQuantity

		public void TestWI_AvailableToTransferQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			// not finalised job
			var receive_UnFinalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory_Arrived = Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 10m, null, "");
			var inventory_Putaway = Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 10m, locationA1, "");
			var inventory_Damaged = Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 10m, locationA1, "", InventoryStatus.Codes.Putaway, InventoryHoldCodes.Codes.Damaged);
			var inventory_Held = Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 10m, locationA1, "", InventoryStatus.Codes.Putaway, InventoryHoldCodes.Codes.Held);

			AssertEquals("Inventory should not be available if job is not finalised.", 0m, inventory_Arrived.WI_AvailableToTransferQuantity);
			AssertEquals("Inventory should not be available if job is not finalised.", 0m, inventory_Putaway.WI_AvailableToTransferQuantity);
			AssertEquals("Inventory should not be available if job is not finalised.", 0m, inventory_Damaged.WI_AvailableToTransferQuantity);
			AssertEquals("Inventory should not be available if job is not finalised.", 0m, inventory_Held.WI_AvailableToTransferQuantity);

			// finalised job
			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory_AVL = Helper.CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 10m, locationA1, "", inventoryStatus: InventoryStatus.Codes.Putaway);
			var inventory_DMG = Helper.CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 10m, locationA1, "", inventoryStatus: InventoryStatus.Codes.Held, heldCode: InventoryHoldCodes.Codes.Damaged);
			var inventory_HLD = Helper.CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 10m, locationA1, "", inventoryStatus: InventoryStatus.Codes.Held);
			receive_Finalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_Finalised);
			AssertEquals("Inventory should be available if job is finalised and stock is not reserved or committed.", 10m, inventory_AVL.WI_AvailableToTransferQuantity);
			AssertEquals("Inventory should be available if job is finalised and stock is not reserved or committed.", 10m, inventory_DMG.WI_AvailableToTransferQuantity);
			AssertEquals("Inventory should be available if job is finalised and stock is not reserved or committed.", 10m, inventory_HLD.WI_AvailableToTransferQuantity);

			// finalised job with committed and reserved quantities
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 10m, locationA1, "");
			var inventory = receive.Inventory[0];

			// commit to transfer
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, 1m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to committ stock
			AssertEquals("Available Qty should not include qty committed to a transfer.", 9m, inventory.WI_AvailableToTransferQuantity);
			Factory.Save();

			// reserve
			var order_Reserve = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine_Reserve = Helper.CreateWhsOrderLine(order_Reserve, data.Part2, 2m);
			Helper.CreateReservePickLine(orderLine_Reserve, inventory, 2m);
			AssertEquals("Available Qty should not include reserved qty.", 7m, inventory.WI_AvailableToTransferQuantity);

			// commit to order
			var order_Commit = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", Notify);
			var orderLine_Commit = Helper.CreateWhsOrderLine(order_Commit, data.Part2, 4m);
			var pick = Helper.CreatePickNew(order_Commit);
			AssertEquals("Available Qty should not include qty committed to a order.", 3m, inventory.WI_AvailableToTransferQuantity);
		}

		#endregion

		#region TestWI_AvailableForCrossDockQuantity

		public void TestWI_AvailableForCrossDockQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 7m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 7m, data.Line111.CommittedQuantityIncludingUnfinalisedReceipt);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2", Notify);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			Helper.CreateReservePickLine(orderLine2, data.Line111, 5m);
			AssertEquals("Precondition", 5m, data.Line111.WI_CrossDockQuantity);

			var locationDifferentFromInventory = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations.First(l => l.PK != data.Line111.WI_WL);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, data.Line111.Location, locationDifferentFromInventory);
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition", 8m, data.Line111.WI_CommittedToTransferQuantity);
			AssertEquals("100m - 7m - 5m - 8m", 80m, data.Line111.WI_AvailableForCrossDockQuantity);
		}

		#endregion

		#region TestWI_AvailableForCrossDockQuantity_ReceivedInDockDoor

		public void TestWI_AvailableForCrossDockQuantity_ReceivedInDockDoor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save(); // Create Default Dock Door

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PLT-123");
			inventory.InDocketLine.WE_WL = data.Whs1.WW_DefaultOutboundDockDoor;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);
			AssertEquals("There should be nothing available to Reserve.", 0m, inventory.WI_AvailableForCrossDockQuantity);
		}

		#endregion

		#region TestWI_AvailableForCrossDockQuantity_ConsidersExpectedQuantity

		public void TestWI_AvailableForCrossDockQuantity_UnfinalisedReceive_ExpectedQuantityGreaterThanTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m);
			receiveLine.WE_TransactionQuantity = 10m;
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			AssertEquals("Precondition", 10m, inventory.WI_TotalUnits);
			AssertEquals("Precondition", 15m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("WI_AvailableForCrossDockQuantity is WI_ExpectedQuantity.", 15m, inventory.WI_AvailableForCrossDockQuantity);
		}

		public void TestWI_AvailableForCrossDockQuantity_UnfinalisedReceive_TransactionQuantityGreaterThanExpectedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine.WE_TransactionQuantity = 10m;
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			AssertEquals("Precondition", 10m, inventory.WI_TotalUnits);
			AssertEquals("Precondition", 5m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("WI_AvailableForCrossDockQuantity is WI_TotalUnits.", 10m, inventory.WI_AvailableForCrossDockQuantity);
		}

		public void TestWI_AvailableForCrossDockQuantity_FinalisedReceive_ExpectedQuantityGreaterThanTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m);
			receiveLine.WE_TransactionQuantity = 10m;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition", 10m, inventory.WI_TotalUnits);
			AssertEquals("Precondition", 15m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("WI_AvailableForCrossDockQuantity is WI_TotalUnits.", 10m, inventory.WI_AvailableForCrossDockQuantity);
		}

		#endregion

		#region TestCommittedQuantityIncludingUnfinalisedReceipt

		#region TestCommittedQuantityIncludingUnfinalisedReceipt

		public void TestCommittedQuantityIncludingUnfinalisedReceipt()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;
			IWhsInventoryInternals inventoryInternals = inventory;
			AssertEquals(0m, inventoryInternals.CommittedToTransactionQuantity);
			AssertEquals(0m, inventory.WI_CommittedToTransferQuantity);
			AssertEquals(100m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);

			data.Receive11.FinaliseDocket();
			AssertEquals(true, data.Receive11.IsFinalised);
			AssertEquals(0m, inventoryInternals.CommittedToTransactionQuantity);
			AssertEquals(0m, inventory.WI_CommittedToTransferQuantity);
			AssertEquals(0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 60m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals(60m, inventoryInternals.CommittedToTransactionQuantity);
			AssertEquals(0m, inventory.WI_CommittedToTransferQuantity);
			AssertEquals(60m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);

			order.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals(0m, inventoryInternals.CommittedToTransactionQuantity);
			AssertEquals(0m, inventory.WI_CommittedToTransferQuantity);
			AssertEquals(0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);

			var locationDifferentFromInventories = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations.First(l => l.PK != inventory.WI_WL);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.Location, locationDifferentFromInventories);
			AssertEquals(0m, inventoryInternals.CommittedToTransactionQuantity);
			AssertEquals(0m, inventory.WI_CommittedToTransferQuantity);
			AssertEquals(0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);

			transfer.RunPreSaveValidation();
			AssertEquals(10m, inventoryInternals.CommittedToTransactionQuantity);
			AssertEquals(10m, inventory.WI_CommittedToTransferQuantity);
			AssertEquals(10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			AssertEquals(0m, inventoryInternals.CommittedToTransactionQuantity);
			AssertEquals(0m, inventory.WI_CommittedToTransferQuantity);
			AssertEquals(0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1");
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -15, inventory.Location);
			adjustment.RunPreSaveValidation();
			AssertEquals(15m, inventoryInternals.CommittedToTransactionQuantity);
			AssertEquals(0m, inventory.WI_CommittedToTransferQuantity);
			AssertEquals(15m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);

			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			AssertEquals(0m, inventoryInternals.CommittedToTransactionQuantity);
			AssertEquals(0m, inventory.WI_CommittedToTransferQuantity);
			AssertEquals(0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
		}

		#endregion

		#region TestCommittedQuantityIncludingUnfinalisedReceipt_WithInvalidReservedPickLines

		public void TestCommittedQuantityIncludingUnfinalisedReceipt_WithInvalidReservedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.IsReserveLine = true;
			pickLine.WZ_Units = 10m;
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			pickLine.WZ_WE_TransactionLine = orderLine.PK;
			AssertEquals("The Reserved Pick Line is incorrect the Order Line should not show it as Reserved.", 0m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Even though the Reserved Pick Line is incorrect the Inventory should still show 10 Units Committed.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Even though the Reserved Pick Line is technically a Reservation, it can't be shown as such because the Pick Line is incorrect.", 0m, inventory.WI_CrossDockQuantity);
			AssertEquals("Even though the Reserved Pick Line is incorrect the Inventory should still show 10 Units Committed.", 10m, inventory.InternalsProxy.CommittedToTransactionQuantity);
		}

		#endregion

		#region TestCommittedQuantityIncludingUnfinalisedReceipt_WhenPickedButNotFinalised

		public void TestCommittedQuantityIncludingUnfinalisedReceipt_WhenPickedButNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("10 Units Committed should be committed to the Order.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("10 Units Committed should be committed to the Order.", 10m, inventory.InternalsProxy.CommittedToTransactionQuantity);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory is no longer committed to the Order as it is Picked.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory is no longer committed to the Order as it is Picked.", 0m, inventory.InternalsProxy.CommittedToTransactionQuantity);
		}

		#endregion

		#region TestPerformanceCommittedQuantityIncludingUnfinalisedReceipt

		public void TestPerformanceCommittedQuantityIncludingUnfinalisedReceipt_AdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.DefaultLocation, "", false);

			for (int count = 0; count < 10; count++)
			{
				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A" + count.ToString());
				var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, data.Whs1.DefaultLocation);
				adjustmentLine.RunPreSaveValidation(); // to commit stock;
				AssertEquals("Precondition: Stock is committed.", 1m, adjustmentLine.CommittedQuantity);
			}
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventoryInOtherFactory = otherFactory.Load<WhsInventoryView>(receive.Inventory.Single().PK);
			AssertEquals(10m, inventoryInOtherFactory.CommittedQuantityIncludingUnfinalisedReceipt);

			var expectedDBHits = new Dictionary<string, int>();
			expectedDBHits.Add(WhsPickLineSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			AssertDbHits(expectedDBHits, otherFactory, true);
		}

		#endregion

		#region TestPerformanceCommittedQuantityIncludingUnfinalisedReceipt_PickLine

		public void TestPerformanceCommittedQuantityIncludingUnfinalisedReceipt_PickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1001m, data.Whs1.DefaultLocation, "", false);

			for (int count = 0; count < 10; count++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, count.ToString(), data.Part1, 1m);
				Factory.Save();

				var pick = Helper.CreatePickNew(order);
				pick.WP_PickNo = count.ToString();
				AssertEquals("Precondition: Stock is committed.", 1m + count,
					pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>().Single().QuantityCommitted);
			}
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventoryInOtherFactory = otherFactory.Load<WhsInventoryView>(receive.Inventory.Single().PK);
			AssertEquals(10m, inventoryInOtherFactory.CommittedQuantityIncludingUnfinalisedReceipt);

			var expectedDBHits = new Dictionary<string, int>();
			expectedDBHits.Add(WhsDocketSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsPickLineSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			AssertDbHits(expectedDBHits, otherFactory, true);
		}

		#endregion

		#endregion

		#region TestCommittedToTransactionQuantity

		public void TestCommittedToTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "", false, false);
			IWhsInventoryInternals inventory = receive.Inventory[0];
			AssertEquals(0m, inventory.CommittedToTransactionQuantity);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals(10m, inventory.CommittedToTransactionQuantity);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2");
			transfer.RunPreSaveValidation();
			AssertEquals(30m, inventory.CommittedToTransactionQuantity);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1");
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -25m, data.Whs1.FindLocation("A-1"));
			adjustment.RunPreSaveValidation();
			AssertEquals(55m, inventory.CommittedToTransactionQuantity);
		}

		#endregion

		#region TestWI_CommittedToTransferQuantity

		public void TestWI_CommittedToTransferQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit stock.
			AssertEquals("All stock still should be in the original inventory.", 100m, receive.Inventory[0].WI_TotalUnits);
			AssertEquals("Both transfer lines should commit stock", 25m, receive.Inventory[0].WI_CommittedToTransferQuantity);

			transferLine2.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);
			AssertEquals("Finalised transfer line should have reduced original inventory record.", 85m, receive.Inventory[0].WI_TotalUnits);
			AssertEquals("Only not finalised transfer line should commit stock", 10m, receive.Inventory[0].WI_CommittedToTransferQuantity);
		}

		#endregion

		#region TestWI_CommittedToTransferQuantity_PutawayTransfers

		public void TestWI_CommittedToTransferQuantity_PutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save(); // Create Default Dock Door

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryReceiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PLT-123");
			inventoryReceiveLine.InDocketLine.WE_WL = data.Whs1.WW_DefaultOutboundDockDoor;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT-123", 10m);
			transfer.RunPreSaveValidation(); // to commit stock.

			AssertEquals("All stock should still be in the original inventory.", 10m, inventoryReceiveLine.WI_TotalUnits);
			AssertEquals("All stock should be committed.", 10m, inventoryReceiveLine.WI_CommittedToTransferQuantity);
			AssertEquals("Receives with picklines to Putaway Transfers are fully committed so available Quantity for Cross-Dock Quantity should be 0.", 0m, inventoryReceiveLine.WI_AvailableForCrossDockQuantity);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory stock should be reduced as real time picked transfers reduce stock.", 0m, inventoryReceiveLine.WI_TotalUnits);
			AssertEquals("Transfer Line is picked, committed stock should be 0.", 0m, inventoryReceiveLine.WI_CommittedToTransferQuantity);
			AssertEquals("Receives with picked picklines to Putaway Transfers are fully committed so available Quantity for Cross-Dock Quantity should be 0.", 0m, inventoryReceiveLine.WI_AvailableForCrossDockQuantity);

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			AssertEquals("Receiveline inventory should still have no units.", 0m, inventoryReceiveLine.WI_TotalUnits);

			var inventoryTransferLine = Factory.Load<WhsInventoryView>(transferLine.PK);
			AssertEquals("All stock should now on Putaway Transfer which has its own inventoryview line.", 10m, inventoryTransferLine.WI_TotalUnits);
			AssertEquals("Putaway Transfer Line is finalised, there should be no stock Committed.", 0m, inventoryReceiveLine.WI_CommittedToTransferQuantity);
			AssertEquals("All stock should now on Putaway Transfer no inventory to crossdock.", 0m, inventoryReceiveLine.WI_AvailableForCrossDockQuantity);
		}

		#endregion

		#region TestWI_CommittedToTransferQuantity_WhenPickedButNotFinalised

		public void TestWI_CommittedToTransferQuantity_WhenPickedButNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.Location, inventory.Location);
			transferLine.RunPreSaveValidation();
			AssertEquals("10 Units Committed should be committed to the Transfer Line.", 10m, inventory.WI_CommittedToTransferQuantity);

			var pickLine = transferLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory is no longer committed to the Transfer Line as it is Picked.", 0m, inventory.WI_CommittedToTransferQuantity);
		}

		#endregion

		#region TestExpectedReceiptQuantity

		#region TestExpectedReceiptQuantity

		public void TestExpectedReceiptQuantity()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var inventory = receiveLine.Inventory[0];
			AssertEquals(0m, Inventory.WI_ExpectedReceiptQuantity);

			inventory.WI_ExpectedReceiptQuantity = 10m;
			AssertEquals(10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals(10m, inventory.WI_ExpectedReceiptQuantity);

			receiveLine.WE_ClientOrderedUnits = 15m;
			AssertEquals(15m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals(15m, receiveLine.WE_ClientOrderedUnits);
		}

		#endregion

		#region TestWI_ExpectedReceiptQuantity_SetValueToDocketLine

		public void TestWI_ExpectedReceiptQuantity_SetValueToDocketLine()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var inventory = receiveLine.Inventory[0];
			AssertEquals("Precondition", 0m, inventory.WI_ExpectedReceiptQuantity);

			TestSetsValueToDocketLine(inventory, WhsDocketLineSchema.WE_ClientOrderedUnits.Name, WhsInventoryView.Schema.WI_ExpectedReceiptQuantity, (ZDecimal)2m, (ZDecimal)5m);
		}

		#endregion

		#endregion

		public void TestWI_CrossDockQuantityInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_CrossDockQuantity, Inventory.WI_CrossDockQuantityInfo.Name);
		}

		public void TestWI_AvailableForCrossDockQuantityInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_AvailableForCrossDockQuantity, Inventory.WI_AvailableForCrossDockQuantityInfo.Name);
		}

		public void TestWI_AvailableToPickQuantityInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_AvailableToPickQuantity, Inventory.WI_AvailableToPickQuantityInfo.Name);
		}

		public void TestExpectedReceiptQuantityInfo()
		{
			AssertEquals("WI_ExpectedReceiptQuantity", Inventory.WI_ExpectedReceiptQuantityInfo.Name);
		}

		#endregion

		#region  TestOriginalInventoryHeldCode

		public void TestOriginalInventoryHeldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.OriginalInventoryHeldCode = "HEL";
			AssertEquals("HEL", inventory.OriginalInventoryHeldCode);
			Factory.Save();
			AssertEquals("HEL", inventory.OriginalInventoryHeldCode);
			AssertEquals("HEL", inventory.WI_HeldCode);
			AssertEquals("HEL", inventory.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("HEL", inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode);

			inventory.OriginalInventoryHeldCode = "AAA";
			AssertEquals("AAA", inventory.OriginalInventoryHeldCode);
			AssertEquals("AAA", inventory.WI_HeldCode);
			AssertEquals("AAA", inventory.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("AAA", inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode);
		}

		#endregion

		#region  TestOriginalInventoryHeldCodeReadOnly

		public void TestOriginalInventoryHeldCodeReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			AssertEquals(false, inventory.OriginalInventoryHeldCodeInfo.ReadOnly);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals(true, inventory.OriginalInventoryHeldCodeInfo.ReadOnly);
		}

		#endregion

		#region TestWI_HeldCode

		public void TestWI_HeldCode()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var inventory = receiveLine.Inventory[0];
			AssertEquals("Precondition", ZString.Empty, inventory.WI_HeldCode);

			inventory.WI_HeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertEquals(InventoryHoldCodes.Codes.Damaged, receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			TestSetsValueToDocketLine(inventory, WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode.Name, WhsInventoryViewSchema.WI_HeldCode.Name, (ZString)InventoryHoldCodes.Codes.Held, (ZString)InventoryHoldCodes.Codes.Damaged);
		}

		#endregion

		#region TestWI_HeldCodeReadOnly

		public void TestWI_HeldCodeReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			TestReadOnly(inventory.WI_HeldCodeInfo, true, true, true, true);
		}

		#endregion

		#region Location

		#region TestWI_WL

		public void TestWI_WL()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Factory.Save();

			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;

			Inventory.WI_WL = locations[0].PK;
			AssertEquals(locations[0].PK, Inventory.WI_WL);
			AssertEquals(whs.PK, Inventory.LocationWhsGuid);

			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_WL.Name, WhsInventoryViewSchema.WI_WL.Name, locations[1].PK, locations[0].PK);
		}

		#region TestWI_WL_SetsInventoryStatus

		public void TestWI_WL_SetsInventoryStatus_DockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			AssertEquals("Precondition: location is a dockdoor location.", true, dockDoorLocation.IsDockDoorLocation);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventoryLine.InDocketLine;
			AssertNotEquals("Precondition: Inventory status is not Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertNotEquals("Precondition: Docket status is not Putaway.", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);

			inventoryLine.WI_WL = dockDoorLocation.PK;
			AssertEquals("Inventory status is updated to Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertNotEquals("Docket status is not updated to Putaway.", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);
			AssertNoExceptionThrown("Save should be successful.", Factory.Save);
		}

		public void TestWI_WL_SetsInventoryStatus_NotDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			AssertEquals("Precondition: location is not a dockdoor location", false, nonDockDoorLocation.IsDockDoorLocation);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventoryLine.InDocketLine;
			receiveLine.WE_PalletID = "ABCD123";
			AssertNotEquals("Precondition: Inventory status is not Putaway.", InventoryStatus.Codes.Putaway, receiveLine.WE_OriginalInventoryStatus);
			AssertNotEquals("Precondition: Docket status is not Putaway.", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);

			inventoryLine.WI_WL = nonDockDoorLocation.PK;
			AssertEquals("Inventory status is updated to Putaway.", InventoryStatus.Codes.Putaway, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("Docket status is updated to Putaway.", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);
			AssertNoExceptionThrown("Save should be successful.", Factory.Save);
		}

		public void TestWI_WL_SetsInventoryStatus_CrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_WL_CrossDock = dockDoorLocation.PK;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventoryLine);
			AssertEquals("Precondition: Stock is cross-docked.", 10m, reservedPickLine.ReservedQuantity);
			AssertEquals("Precondition: location is a dockdoor location", true, dockDoorLocation.IsDockDoorLocation);
			Factory.Save();

			var receiveLine = inventoryLine.InDocketLine;
			receiveLine.WE_PalletID = "ABCD123";
			AssertNotEquals("Precondition: Inventory status is not Putaway.", InventoryStatus.Codes.Putaway, receiveLine.WE_OriginalInventoryStatus);
			AssertNotEquals("Precondition: Docket status is not Putaway.", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);

			inventoryLine.WI_WL = dockDoorLocation.PK;
			AssertEquals("Inventory status is updated to Putaway.", InventoryStatus.Codes.Putaway, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("Docket status is updated to Putaway.", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);
			AssertNoExceptionThrown("Save should be successful.", Factory.Save);
		}

		#endregion

		#endregion

		#region TestWI_WL_ReadOnly

		public void TestWI_WL_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			AssertEquals(false, inventory.WI_WLInfo.ReadOnly);

			receive.WD_WW_Whs = ZGuid.Empty;
			AssertEquals(true, inventory.WI_WLInfo.ReadOnly);
			receive.WD_WW_Whs = data.Whs1.PK; // cleanup

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals(true, inventory.WI_WLInfo.ReadOnly);
		}

		#endregion

		#region TestWI_WL_WhenSettingToEmptyClearsOutLocationString

		public void TestWI_WL_WhenSettingToEmptyClearsOutLocationString()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventoryLine.WI_WL = data.Whs1.DefaultLocation.PK;
			AssertEquals("Precondition:", "A", inventoryLine.LocationString);

			receive.ClearLocations();
			AssertEquals(ZGuid.Empty, inventoryLine.WI_WL);

			receive.RunPreSaveValidation();
			AssertNoErrors(receive);

			receive.AllocateLocationsWithMock();
			AssertEquals("Precondition:", "A", inventoryLine.LocationString);
			Factory.Save(); // Creates receive Lines

			inventoryLine.WI_WL = ZGuid.Empty;
			AssertEquals("", inventoryLine.LocationString);
			AssertEquals("", inventoryLine.InDocketLine.LocationString);

			receive.RunPreSaveValidation();
			AssertNoErrors(receive);
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertNull(inventory.Location);

			inventory.LocationString = "A-1";
			AssertEquals(data.Whs1.FindLocation("A-1"), inventory.Location);
		}

		public void TestLocation_FixedWidthLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, warehouse, "1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertNull(inventory.Location);

			var location = warehouse.FindLocation("Z040302");
			inventory.LocationString = "Z040302";
			AssertEquals(location, inventory.Location);

			inventory.LocationString = "";
			AssertEquals(null, inventory.Location);

			inventory.LocationString = "Z-04-03-02";
			AssertEquals(location, inventory.Location);
		}

		#endregion

		#region TestCurrentLocationProperties

		public void TestCurrentLocationProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			nonDockDoorLocation.PickingArea.WA_AreaType = "ABC";
			nonDockDoorLocation.PickingArea.WA_Name = "NonDDLAreaName";
			nonDockDoorLocation.WLV_LocationStatus = LocationStatus.Codes.Damaged;
			nonDockDoorLocation.WLV_PickMethod = "ABC";
			dockDoorLocation.PickingArea.WA_AreaType = "XYZ";
			dockDoorLocation.WLV_LocationStatus = LocationStatus.Codes.Held;
			dockDoorLocation.PickingArea.WA_Name = "DDLAreaName";
			dockDoorLocation.WLV_PickMethod = "XYZ";

			var arrivalDate = ZDateTimeOffset.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var pendingInventory = CreateInventoryForDifferentStatuses(receive, data.Part1, null, 1m, "A", InventoryStatus.Codes.Pending);
			var receivedInventory = CreateInventoryForDifferentStatuses(receive, data.Part1, dockDoorLocation, 2m, "B", InventoryStatus.Codes.Received, arrivalDate);
			var putawayInventory = CreateInventoryForDifferentStatuses(receive, data.Part2, nonDockDoorLocation, 4m, "D", InventoryStatus.Codes.Putaway, arrivalDate);
			var arrivedInventory = CreateInventoryForDifferentStatuses(receive, data.Part2, null, 5m, "E", InventoryStatus.Codes.Arrived, arrivalDate);
			Factory.Save();

			AssertEquals("Precondition: ArrivalDate - Received", arrivalDate, receivedInventory.WI_ArrivalDate);
			AssertEquals("Precondition: ArrivalDate - Arrived", arrivalDate, arrivedInventory.WI_ArrivalDate);
			nonDockDoorLocation.WLV_MaximumPickCountBeforeAutomatedStocktake = 10;
			dockDoorLocation.WLV_MaximumPickCountBeforeAutomatedStocktake = 8;
			nonDockDoorLocation.WLV_FinalisedPickCount = 5;
			dockDoorLocation.WLV_FinalisedPickCount = 5;
			AssertEquals("", pendingInventory.CurrentLocationString);
			AssertLocationPropertiesWithDockDoor(receivedInventory, dockDoorLocation, LocationStatus.Codes.Held, LocationStatus.Descriptions.Held, 3, "XYZ", dockDoorLocationType.WLT_Code, LocationClasses.Codes.DDL);
			AssertLocationPropertiesWithDockDoor(putawayInventory, nonDockDoorLocation, LocationStatus.Codes.Damaged, LocationStatus.Descriptions.Damaged, 5, "ABC", nonDockDoorLocation.LocationType.WLT_Code, LocationClasses.Codes.NOR);
			AssertEquals("", arrivedInventory.CurrentLocationString);
		}

		void AssertLocationPropertiesWithDockDoor(WhsInventoryView inventoryView, WhsLocation expectedLocation = null,
			string expectedLocationStatusCode = "", string expectedLocationStatusDescription = "",
			int expectedTouchesUntilStocktake = 0, string expectedPickMethod = "", string expectedLocationType = "",
			string expectedLocationClass = "")
		{
			if (expectedLocation != null)
			{
				AssertEquals(expectedLocation.PK, inventoryView.CurrentLocation.PK);
				AssertEquals(expectedLocation?.WLV_LocationString_UserFriendly ?? ZString.Empty, inventoryView.CurrentLocationString);
				AssertEquals(expectedLocation.PickingArea.WA_Name, inventoryView.CurrentLocationPickAreaName);
				AssertEquals(expectedLocation.PickingArea.WA_AreaType, inventoryView.CurrentLocationPickAreaType);
			}
			else
			{
				AssertNull(inventoryView.CurrentLocation);
				AssertEquals("", inventoryView.CurrentLocationString);
				AssertEquals("", inventoryView.CurrentLocationPickAreaName);
				AssertEquals("", inventoryView.CurrentLocationPickAreaType);
			}
			AssertEquals(expectedTouchesUntilStocktake, inventoryView.TouchesUntilStocktake);
			AssertEquals(expectedLocationStatusCode, inventoryView.CurrentLocationStatusCode);
			AssertEquals(expectedLocationStatusDescription, inventoryView.CurrentLocationStatus);
			AssertEquals(expectedPickMethod, inventoryView.CurrentPickMethod);
			AssertEquals(expectedLocationType, inventoryView.CurrentLocationType);
			AssertEquals(expectedLocationClass, inventoryView.CurrentLocationClass);
		}

		WhsInventoryView CreateInventoryForDifferentStatuses(WhsReceive receive, OrgSupplierPart part, WhsLocation location, ZDecimal qty, string palletID, string inventoryStatus, ZDateTimeOffset? arrivalDate = null)
		{
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, qty);
			var receiveLine = inventory.InDocketLine;
			receiveLine.WE_AdjustmentArrivalDate = arrivalDate != null && !arrivalDate.Value.IsEmpty ? arrivalDate.Value : receiveLine.WE_AdjustmentArrivalDate;
			receiveLine.WE_WL = location?.PK ?? ZGuid.Empty;
			receiveLine.WE_PalletID = palletID;
			receiveLine.WE_OriginalInventoryStatus = inventoryStatus;
			AssertEquals("Precondition", inventoryStatus, receiveLine.WE_OriginalInventoryStatus);
			return inventory;
		}

		#endregion

		#region TestGetCurrentLocationQuery

		public void TestGetCurrentLocationQuery_InTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderPickLine = orderLine.PickLines.Single();

			var transferLine = Helper.PickAndMakeInTransitTransfer(orderPickLine, ZDateTimeOffset.Now);
			Factory.Save();

			AssertEquals($"Precondition: TransferLine Original Inventory Status should be {InventoryStatus.Codes.InTransit}", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals($"Precondition: TransferLine Current Inventory Status should be {InventoryStatus.Codes.InTransit}", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var result = Factory.Load<WhsInventoryView>(WhsInventoryView.GetInventoryQueryByLocation());
			AssertEquals("Should only find receive Inventory", inventory.PK, result.Single().PK);
		}

		public void TestGetCurrentLocationQuery_PuttingAwayInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, dockDoorLocation1, "PLT1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "PLT1", 15m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals($"Precondition: TransferLine Original Inventory Status should be {InventoryStatus.Codes.PuttingAway}", InventoryStatus.Codes.PuttingAway, transferLine.WE_OriginalInventoryStatus);
			AssertEquals($"Precondition: TransferLine Current Inventory Status should be {InventoryStatus.Codes.PuttingAway}", InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);

			var result = Factory.Load<WhsInventoryView>(WhsInventoryView.GetInventoryQueryByLocation());
			AssertEquals("Should only find receive Inventory", receiveLine.PK, result.Single().PK);
		}

		public void TestGetCurrentLocationQuery_ReceivedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 15m);
			Factory.Save();

			AssertEquals($"Precondition: Inventory Status should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);

			var result = Factory.Load<WhsInventoryView>(WhsInventoryView.GetInventoryQueryByLocation());
			AssertEquals("Should find out Received Inventory", inventory.PK, result.Single().PK);
		}

		public void TestGetCurrentLocationQuery_WithSubQuery()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			AssertEquals($"Precondtion: Inventory status should be  {InventoryStatus.Codes.Available}", InventoryStatus.Codes.Available, inventory1.WI_InventoryStatus);
			AssertEquals($"Precondtion: Inventory status should be  {InventoryStatus.Codes.Available}", InventoryStatus.Codes.Available, inventory2.WI_InventoryStatus);

			var result = Factory.Load<WhsInventoryView>(WhsInventoryView.GetInventoryQueryByLocation(s =>
			{
				s.AddToFilter(WhsLocationViewSchema.PK, location2.PK);
			}));
			AssertEquals("Should find out Inventory2", inventory2.PK, result.Single().PK);
		}

		#endregion

		#region TestCurrentLocationPutawayPathSequence

		public void TestCurrentLocationPutawayPathSequence()
		{
			WhsLocation location = Factory.New<WhsLocation>();
			location.WLV_PutawayPathSequence = 1;

			Inventory.WI_WL = location.PK;
			AssertEquals("CurrentLocationPutawayPathSequence value is correct when set.", 1, Inventory.CurrentLocationPutawayPathSequence);

			location.WLV_PutawayPathSequence = 5;
			AssertEquals("CurrentLocationPutawayPathSequence value is correct when changed.", 5, Inventory.CurrentLocationPutawayPathSequence);

			Inventory.WI_WL = ZGuid.Empty;
			AssertEquals("CurrentLocationPutawayPathSequence value is correct when NOT set.", int.MaxValue, Inventory.CurrentLocationPutawayPathSequence);
		}

		#endregion

		#region TestLocationString_ReadOnly

		public void TestLocationString_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			var propertyInfo = inventory.LocationStringInfo;
			AssertEquals(false, propertyInfo.ReadOnly);

			inventory.IsInventoryEditForm = true;
			AssertEquals(true, propertyInfo.ReadOnly);
			inventory.IsInventoryEditForm = false; // cleanup

			receive.WD_WW_Whs = ZGuid.Empty;
			AssertEquals(true, propertyInfo.ReadOnly);
			receive.WD_WW_Whs = data.Whs1.PK; // cleanup

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals(true, propertyInfo.ReadOnly);
		}

		#endregion

		#region TestAttributesReadonly

		#region TestWI_PalletID_ReadOnly

		public void TestWI_PalletID_IsReadOnly()
		{
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_PalletIDInfo);
			AssertAttributeReadonly_IfFinalised(i => i.WI_PalletIDInfo, "WI_PalletID");
		}

		#endregion

		#region TestCustomAttributesReadonly

		public void TestCustomAttributesReadonly_IfIsInventoryEditForm()
		{
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomAttrib_1Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomAttrib_2Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomAttrib_3Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomAttrib4Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomAttrib5Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomAttrib6Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDate1Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDate2Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDate3Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDate4Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDate5Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDecimal1Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDecimal2Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDecimal3Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDecimal4Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomDecimal5Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomFlag1Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomFlag2Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomFlag3Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomFlag4Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomFlag5Info);
			AssertAttributeReadonly_IfIsInventoryEditForm(i => i.WI_CustomTextBlob1Info);
		}

		public void TestCustomAttributesReadonly_IfDocketFinalised()
		{
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomAttrib_1Info, "WI_CustomAttrib_1");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomAttrib_2Info, "WI_CustomAttrib_2");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomAttrib_3Info, "WI_CustomAttrib_3");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomAttrib4Info, "WI_CustomAttrib4");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomAttrib5Info, "WI_CustomAttrib5");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomAttrib6Info, "WI_CustomAttrib6");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDate1Info, "WI_CustomDate1");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDate2Info, "WI_CustomDate2");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDate3Info, "WI_CustomDate3");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDate4Info, "WI_CustomDate4");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDate5Info, "WI_CustomDate5");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDecimal1Info, "WI_CustomDecimal1");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDecimal2Info, "WI_CustomDecimal2");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDecimal3Info, "WI_CustomDecimal3");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDecimal4Info, "WI_CustomDecimal4");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomDecimal5Info, "WI_CustomDecimal5");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomFlag1Info, "WI_CustomFlag1");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomFlag2Info, "WI_CustomFlag2");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomFlag3Info, "WI_CustomFlag3");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomFlag4Info, "WI_CustomFlag4");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomFlag5Info, "WI_CustomFlag5");
			AssertAttributeReadonly_IfFinalised(i => i.WI_CustomTextBlob1Info, "WI_CustomTextBlob1");
		}

		#endregion

		void AssertAttributeReadonly_IfIsInventoryEditForm(Func<WhsInventoryView, ZPropertyInfo> getPropertyInfo)
		{
			var data = Factory.GetCachedValue("WhsInventoryViewTest|TestData", () => new TestDataSimpleEnvironment(Factory, 2, 1));
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "", finalise: false);
			receive.Inventory.Load();
			var inventory = receive.Inventory[0];
			var propertyInfo = getPropertyInfo(inventory);
			AssertEquals(false, propertyInfo.ReadOnly);

			inventory.IsInventoryEditForm = true;
			AssertEquals(propertyInfo.Name + " should be Read-Only if we are in the Inventory Edit Form.", true, propertyInfo.ReadOnly);
		}

		void AssertAttributeReadonly_IfFinalised(Func<WhsInventoryView, ZPropertyInfo> getPropertyInfo, ZString name)
		{
			var data = Factory.GetCachedValue("WhsInventoryViewTest|TestData", () => new TestDataSimpleEnvironment(Factory, 2, 1));
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, name, data.Part1, 12m, data.Whs1.FindLocation("A-1"), "", finalise: false);
			receive.Inventory.Load();
			var inventory = receive.Inventory[0];
			var propertyInfo = getPropertyInfo(inventory);
			AssertEquals(false, propertyInfo.ReadOnly);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals(propertyInfo.Name + " should be Read-Only if DocketLine is finalised.", true, propertyInfo.ReadOnly);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 7m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 5m, transferLine1.QtyCommittedIncludingMatchingLines);
			AssertEquals("Precondition: Stock is committed.", 7m, transferLine2.QtyCommittedIncludingMatchingLines);

			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);

			var inventoryFromTransferLine = transferLine1.Inventory[0];
			Factory.Save();
			AssertEquals(propertyInfo.Name + " should be Read Only if Transfer Line is Finalised.", true, inventoryFromTransferLine.WI_PalletIDInfo.ReadOnly);
		}

		#endregion

		#region TestLocationSettersUpdateStatus

		public void TestLocationSettersUpdateStatus()
		{
			var data = new TestDataForInventory(Factory);
			Factory.Save();

			data.CreateSimpleInventoryManyLines(new ZDecimal[2] { 10m, 20m }, false);
			data.Receive11.ClearLocations();

			AssertEquals("Precondition: When all lines have no location, status should be Entered", DocketStatus.Codes.New, data.Receive11.WD_DocketStatus);

			data.Receive11.Inventory[0].WI_WL = data.Whs1.FindLocation("A").PK;
			AssertEquals("Line has a location, status should be Putaway", InventoryStatus.Codes.Putaway, data.Receive11.Inventory[0].WI_InventoryStatus);
			AssertEquals("1 of 2 lines has a location, status should be Putaway", DocketStatus.Codes.Putaway, data.Receive11.WD_DocketStatus);

			data.Receive11.Inventory[1].LocationString = "A-1-1";
			AssertEquals("Line has a location, status should be Putaway", InventoryStatus.Codes.Putaway, data.Receive11.Inventory[1].WI_InventoryStatus);
			AssertEquals("2 of 2 lines have a location, status should be Putaway", DocketStatus.Codes.Putaway, data.Receive11.WD_DocketStatus);

			data.Receive11.Inventory[0].LocationString = "";
			AssertEquals("Line has no location, header has an arrival date, status should be Arrived", InventoryStatus.Codes.Arrived, data.Receive11.Inventory[0].WI_InventoryStatus);
			AssertEquals("1 of 2 lines has a location, status should be Putaway", DocketStatus.Codes.Putaway, data.Receive11.WD_DocketStatus);

			data.Receive11.Inventory[1].WI_WL = ZGuid.Empty;
			AssertEquals("Line has no location, header has an arrival date, status should be Arrived", InventoryStatus.Codes.Arrived, data.Receive11.Inventory[1].WI_InventoryStatus);
			AssertEquals("No lines have a location, status should be Entered", DocketStatus.Codes.New, data.Receive11.WD_DocketStatus);

			data.Receive11.Inventory[1].LocationString = "A-1-1";
			AssertEquals("Line has a location, status should be Putaway", InventoryStatus.Codes.Putaway, data.Receive11.Inventory[1].WI_InventoryStatus);
			AssertEquals("1 of 2 lines has a location, status should be Putaway", DocketStatus.Codes.Putaway, data.Receive11.WD_DocketStatus);

			data.Receive11.WD_ArrivalDate = ZDateTimeOffset.Empty;
			data.Receive11.Inventory[1].LocationString = "";
			AssertEquals("Line has no location, header has no arrival date, status should be Pending", InventoryStatus.Codes.Pending, data.Receive11.Inventory[1].WI_InventoryStatus);
			AssertEquals("No lines have a location, status should be Entered", DocketStatus.Codes.New, data.Receive11.WD_DocketStatus);
		}

		#endregion

		#region TestLocationStatus

		public void TestLocationStatus()
		{
			WhsLocation location = Factory.New<WhsLocation>();
			Inventory.WI_WL = location.PK;
			location.WLV_LocationStatus = Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Damaged;
			AssertEquals(Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Descriptions.Damaged, Inventory.LocationStatus);

			Inventory.WI_WL = ZGuid.Empty;
			AssertEquals("", Inventory.LocationStatus);
		}

		#endregion

		#region TestLocationClass

		public void TestLocationClass()
		{
			WhsLocation location = Factory.New<WhsLocation>();

			Inventory.WI_WL = location.PK;
			location.WLV_WLT_LocationType = Helper.CreateLocationType("XYZ", LocationClasses.Codes.DDL).PK;
			AssertEquals(LocationClasses.Codes.DDL, Inventory.LocationClass);

			Inventory.WI_WL = ZGuid.Empty;
			AssertEquals(ZString.Empty, Inventory.LocationClass);
		}

		#endregion

		#region TestLocationStatusInfo

		public void TestLocationStatusInfo()
		{
			AssertEquals(WhsInventoryView.Schema.LocationStatus, Inventory.LocationStatusInfo.Name);
		}

		#endregion

		#region TestLocationWhsGuid

		public void TestLocationWhsGuid()
		{
			WhsWarehouse whs1 = Helper.CreateWarehouse("1", "A");
			WhsWarehouse whs2 = Helper.CreateWarehouse("2", "A");
			Factory.Save();

			var whsInventoryLine = Factory.New<WhsInventoryView>();

			whsInventoryLine.LocationWhsGuid = whs1.PK;
			AssertEquals(whs1.PK, whsInventoryLine.LocationWhsGuid);

			whsInventoryLine.LocationWhsGuid = whs2.PK;
			AssertEquals(whs2.PK, whsInventoryLine.LocationWhsGuid);

			whsInventoryLine.WI_WL = whs1.FindLocation("A").PK;
			AssertEquals(whs1.PK, whsInventoryLine.LocationWhsGuid);
		}

		#endregion

		#region TestLocationWhsGuidInfo

		public void TestLocationWhsGuidInfo()
		{
			var whsInventoryLine = Factory.New<WhsInventoryView>();
			AssertEquals("LocationWhsGuid", whsInventoryLine.LocationWhsGuidInfo.Name);
		}

		#endregion

		#region TestOnClearLocationsStatusIsReverted

		public void TestOnClearLocationsStatusIsReverted()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 10, 20, 40 }, false);
			AssertEquals(DocketStatus.Codes.Putaway, data.Receive11.WD_DocketStatus);

			data.Receive11.Inventory[0].LocationString = ZString.Empty;
			AssertEquals(DocketStatus.Codes.Putaway, data.Receive11.WD_DocketStatus);

			data.Receive11.Inventory[1].LocationString = ZString.Empty;
			data.Receive11.Inventory[2].LocationString = ZString.Empty;
			AssertEquals(DocketStatus.Codes.New, data.Receive11.WD_DocketStatus);

			data.Receive11.AllocateLocationsWithMock();
			AssertEquals(DocketStatus.Codes.Putaway, data.Receive11.WD_DocketStatus);

			data.Receive11.ClearLocations();
			AssertEquals(DocketStatus.Codes.New, data.Receive11.WD_DocketStatus);

			data.Receive11.Inventory[0].LocationString = "A";
			data.Receive11.WD_DocketStatus = DocketStatus.Codes.Finalised;
			data.Receive11.Inventory[0].LocationString = ZString.Empty;
			AssertEquals(DocketStatus.Codes.Finalised, data.Receive11.WD_DocketStatus);
		}

		#endregion

		#endregion

		#region BondInfo

		[TestDate(2005, 1, 20)]
		public void TestBondedInfoProperties()
		{
			var data = new TestDataForBondedEntriesWithBondIDs(Factory);
			Factory.Save(); // needed for DBOnly queries below

			Inventory = data.FindInventory("E11AA1-5")[0];
			AssertEquals("PickableUnits", 150m, Inventory.WI_AvailableToPickQuantity);
		}

		public void TestBondedInfoPropertiesNotCached()
		{
			TestDataForBondedEntriesWithBondIDs data = new TestDataForBondedEntriesWithBondIDs(Factory);
			Factory.Save(); // needed for DBOnly queries below

			Inventory = data.FindInventory("E11AA1-5")[0];
			AssertEquals("PickableUnits.", 150m, Inventory.WI_AvailableToPickQuantity);

			Inventory.WI_TotalUnits = 75m;
			AssertEquals("PickableUnits.", 75m, Inventory.WI_AvailableToPickQuantity);

			WhsOrder order = Helper.CreateWhsOrder(Inventory.Client, Inventory.Warehouse, "1", Notify);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, Inventory.SupplierPart, 60m);
			Helper.CreatePickByAttachingOrders(order);
			Inventory.WI_TotalUnits = 75m; // require to refresh BondInfo.

			AssertEquals("PickableUnits", 15m, Inventory.WI_AvailableToPickQuantity);

			Inventory.WI_BondedEntryKey = "E31AA1-1";
			AssertEquals("PickableUnits", 15m, Inventory.WI_AvailableToPickQuantity);
		}

		#endregion

		#region WI_LineNo

		public void TestWI_LineNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = receive.Lines.AddNew().Inventory[0];
			var inventory2 = receive.Lines.AddNew().Inventory[0];
			var inventory3 = receive.Lines.AddNew().Inventory[0];

			inventory1.WI_LineNo = 1;
			inventory2.WI_LineNo = 2;
			inventory3.WI_LineNo = 3;
			AssertEquals(new ZShort(1), inventory1.WI_LineNo);
			AssertEquals(new ZShort(2), inventory2.WI_LineNo);
			AssertEquals(new ZShort(3), inventory3.WI_LineNo);

			receive.RunPreSaveValidation();
			AssertEquals(new ZShort(1), inventory1.WI_LineNo);
			AssertEquals(new ZShort(2), inventory2.WI_LineNo);
			AssertEquals(new ZShort(3), inventory3.WI_LineNo);
		}

		public void TestWI_LineNoInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_LineNo, Inventory.WI_LineNoInfo.Name);
		}

		protected virtual bool ExpectedAlwaysToBeReadOnly()
		{
			return false;
		}

		#endregion

		#region WI_SubLineNo

		public void TestWI_SubLineNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = receive.Lines.AddNew().Inventory[0];
			var inventory2 = receive.Lines.AddNew().Inventory[0];
			var inventory3 = receive.Lines.AddNew().Inventory[0];

			inventory1.WI_SubLineNo = 1;
			inventory2.WI_SubLineNo = 2;
			inventory3.WI_SubLineNo = 3;
			AssertEquals(new ZShort(1), inventory1.WI_SubLineNo);
			AssertEquals(new ZShort(2), inventory2.WI_SubLineNo);
			AssertEquals(new ZShort(3), inventory3.WI_SubLineNo);

			receive.RunPreSaveValidation();
			AssertEquals(new ZShort(1), inventory1.WI_SubLineNo);
			AssertEquals(new ZShort(2), inventory2.WI_SubLineNo);
			AssertEquals(new ZShort(3), inventory3.WI_SubLineNo);
		}

		public void TestWI_SubLineNoInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_SubLineNo, Inventory.WI_SubLineNoInfo.Name);
		}

		#endregion

		#region WI_OP

		public void TestWI_OP()
		{
			var client = Helper.CreateClient();
			var part1 = Helper.CreateProduct(client, "P1");
			var part2 = Helper.CreateProduct(client, "P2");
			var whs = Helper.CreateWarehouse("A");
			var receive = Helper.CreateWhsReceive(client, whs);
			var inventory = receive.Lines.AddNew().Inventory[0];

			inventory.WI_OP = part1.PK;
			AssertEquals(part1.PK, inventory.WI_OP);
			Factory.Save();

			// test SetLineDataBasedOnProduct()

			part2.OP_StockKeepingUnit = "KG";
			inventory.WI_F3_NKPackType = "";
			inventory.WI_OP = part2.PK;
			AssertEquals(part2.OP_StockKeepingUnit, inventory.WI_F3_NKPackType);

			inventory.WI_F3_NKPackType = "#!@"; // junk
			inventory.WI_OP = part1.PK;
			AssertEquals("#!@", inventory.WI_F3_NKPackType);

			SetLineAttributes(inventory);
			inventory.WI_OP = part2.PK;
			AssertLineAttributes(inventory, ZDate.Empty, ZDate.Empty, "", "", "", "");

			client.MiscServ.OM_IMPartAttrib1Type = Enterprise.MasterFiles.Business.PartAttributeTypeList.Codes.BatchNumber;
			client.MiscServ.OM_IMPartAttrib2Type = Enterprise.MasterFiles.Business.PartAttributeTypeList.Codes.BatchNumber;
			client.MiscServ.OM_IMPartAttrib3Type = Enterprise.MasterFiles.Business.PartAttributeTypeList.Codes.BatchNumber;
			client.MiscServ.OM_IMUseSerialNumber = true;
			client.MiscServ.OM_IMUseExpiryDate = true;
			client.MiscServ.OM_IMUsePackingDate = true;

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetProductAttributeUse(client, part1, attribNo, true);
			}

			SetLineAttributes(inventory);
			inventory.WI_OP = part1.PK;
			AssertLineAttributes(inventory, ZDate.Today, ZDate.Today, "PA1", "PA2", "PA3", "SN1");

			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			inventory.WI_OP = part2.PK;
			AssertEquals(true, inventory.WI_OPInfo.ReadOnly);
		}

		public void TestWI_OP_RecalculatesDocketLineUnitsFromPackQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, finalise: false);
			var inventory = receive.Inventory[0];
			inventory.WI_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertEquals("Precondition.", 1m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals("Precondition.", 5m, inventory.WI_InDocketLineUnits);

			inventory.WI_OP = data.Part2.PK;
			AssertEquals("Should have retained PackQuantity.", 1m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals("Should have recalculated InDocketLineUnits.", 10m, inventory.WI_InDocketLineUnits);
		}

		public void TestWI_OP_SetValueToDocketLine()
		{
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_OP.Name, WhsInventoryViewSchema.WI_OP.Name, ZGuid.NewZGuid(), ZGuid.NewZGuid());
		}

		public void TestWI_OPInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			AssertEquals("WI_OP", inventory.WI_OPInfo.Name);

			TestStandardReadOnly(inventory.WI_OPInfo);
		}

		#region TestPackTypeDefaultedFromProduct

		public void TestPackTypeDefaultedFromProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			var productParams = product.ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_F3_NKReceivedPackType = "BAG";

			Factory.Save();

			line.WI_OP = data.Part2.PK;
			AssertEquals("UNT", line.WI_F3_NKPackType);

			_ = line.Product;
			line.WI_OP = data.Part1.PK;
			AssertEquals("Pack Type should be defaulted from W3_F3_NKReceivedPackType.", "BAG", line.WI_F3_NKPackType);
		}

		#endregion

		#endregion

		#region WI_OP_PartNum

		public void TestWI_OP_PartNum()
		{
			TestDataSimpleEnvironment data = new TestDataSimpleEnvironment(Factory);
			AssertEquals("", Inventory.WI_OP_PartNum);

			Inventory.WI_OP = data.Part1.PK;
			AssertEquals("WI_OP_PartNum should be set from WI_OP", "P1", Inventory.WI_OP_PartNum);

			Inventory.WI_OP_PartNum = "NEWPRODUCT";
			AssertEquals("WI_OP_PartNum still should come from SupplierPart since it is not null", "P1", Inventory.WI_OP_PartNum);

			Inventory.WI_OP = ZGuid.Empty;
			AssertEquals("WI_OP_PartNum should has previously set value since SupplierPart is null.", "NEWPRODUCT", Inventory.WI_OP_PartNum);

			Inventory.WI_OP = ZGuid.Invalid;
			Inventory.WI_OP_PartNum = "NEWPRODUCT-2";
			AssertEquals("WI_OP_PartNum should not be reset", "NEWPRODUCT-2", Inventory.WI_OP_PartNum);
		}

		public void TestWI_OP_PartNum_SetValueToDocketLine()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var inventory = docketLine.Inventory[0];
			inventory.WI_OP = ZGuid.Invalid;

			TestSetsValueToDocketLine(inventory, WhsDocketLine.Schema.ProductCode, WhsInventoryView.Schema.WI_OP_PartNum, (ZString)"TEST 1", (ZString)"TEST 2");
		}

		public void TestWI_OP_PartNumInfo()
		{
			AssertEquals("WI_OP_PartNum", Inventory.WI_OP_PartNumInfo.Name);
			AssertEquals(OrgSupplierPartSchema.OP_PartNum.MaxLength, Inventory.WI_OP_PartNumInfo.MaxLength);
		}

		#endregion

		#region WI_OP_Desc

		public void TestWI_OP_Desc()
		{
			AssertEquals(ZString.Empty, Inventory.WI_OP_Desc);
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_Desc = "PRODDESCRIPTION";
			Inventory.WI_OP = part.PK;
			AssertEquals("PRODDESCRIPTION", Inventory.WI_OP_Desc);
		}

		public void TestWI_OP_Desc_SetValueToDocketLine()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var inventory = docketLine.Inventory[0];
			inventory.WI_OP = ZGuid.Invalid;

			TestSetsValueToDocketLine(inventory, WhsDocketLine.Schema.ProductDesc, WhsInventoryView.Schema.WI_OP_Desc, (ZString)"TEST 1", (ZString)"TEST 2");
		}

		public void TestWI_OP_PartDescInfo()
		{
			AssertEquals("WI_OP_Desc", Inventory.WI_OP_DescInfo.Name);
			TestTempProductReadOnly(Inventory.WI_OP_DescInfo);
		}

		#endregion

		#region WI_InDocketLine

		public void TestWI_WE_InDocketLine()
		{
			WhsReceiveLine line = GetNewReceiveLine();
			Inventory.WI_InDocketLineType = CodeLists.DocketType.Codes.Receive;
			Inventory.WI_WE_InDocketLine = line.PK;
			AssertEquals(line.PK, Inventory.WI_WE_InDocketLine);
		}

		#endregion

		#region TestWI_WE_OriginalInDocketLineForRating

		public void TestWI_WE_OriginalInDocketLineForRating()
		{
			var randomDocketLinePK = ZGuid.NewZGuid();
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventory.InDocketLine;
			AssertEquals(receiveLine.WE_WE_OriginalDocketLineForRating, inventory.WI_WE_OriginalInDocketLineForRating);

			// when modifying Original Docket line for rating on inventory, docket line's original docket line for rating should also change.
			inventory.WI_WE_OriginalInDocketLineForRating = randomDocketLinePK;
			AssertEquals(randomDocketLinePK, inventory.WI_WE_OriginalInDocketLineForRating);
			AssertEquals(randomDocketLinePK, receiveLine.WE_WE_OriginalDocketLineForRating);
		}

		public void TestWI_WE_OriginalInDocketLineForRating_SetValueToDocketLine()
		{
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_WE_OriginalDocketLineForRating.Name, WhsInventoryViewSchema.WI_WE_OriginalInDocketLineForRating.Name, ZGuid.NewZGuid(), ZGuid.NewZGuid());
		}

		#endregion

		#region TestWI_PackQuantity_UpdateWI_InDocketLineUnitsForTemporaryProducts

		public void TestWI_PackQuantity_UpdateWI_InDocketLineUnitsForTemporaryProducts()
		{
			AssertEquals(0m, Inventory.InDocketLine.WE_PackQuantity);
			AssertEquals(0m, Inventory.WI_InDocketLineUnits);

			Inventory.InDocketLine.WE_PackQuantity = 5m;
			AssertEquals(5m, Inventory.InDocketLine.WE_PackQuantity);
			AssertEquals(0m, Inventory.WI_InDocketLineUnits);

			Inventory.WI_OP = ZGuid.Invalid; // Temp Product
			Inventory.WI_OP_PartNum = "TEMP";
			Inventory.InDocketLine.WE_PackQuantity = 10m;
			AssertEquals(10m, Inventory.InDocketLine.WE_PackQuantity);
			AssertEquals(10m, Inventory.WI_InDocketLineUnits);
		}

		#endregion

		#region TestWI_F3_NKPackType

		#region TestWI_F3_NKPackType

		public void TestWI_F3_NKPackType()
		{
			OrgHeader org = Helper.CreateClient();
			OrgSupplierPart part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m
			Helper.CreateProductUnit(part, "BOX", 2m);

			Inventory.WI_OP = part.PK;
			Inventory.WI_InDocketLineUnits = 10m;
			AssertEquals("Precondition", part.OP_StockKeepingUnit, Inventory.WI_F3_NKPackType);
			AssertEquals("Precondition", 10m, Inventory.InDocketLine.WE_PackQuantity);

			Inventory.WI_F3_NKPackType = "CTN";
			AssertEquals("CTN", Inventory.WI_F3_NKPackType);
			AssertEquals(120m, Inventory.WI_InDocketLineUnits);

			Inventory.WI_F3_NKPackType = "BOX";
			AssertEquals("BOX", Inventory.WI_F3_NKPackType);
			AssertEquals(20m, Inventory.WI_InDocketLineUnits);
		}

		#endregion

		#region TestWI_F3_NKPackType_UpdateWI_UnitsUQForTemporaryProducts

		public void TestWI_F3_NKPackType_UpdateWI_UnitsUQForTemporaryProducts()
		{
			AssertEquals("", Inventory.WI_F3_NKPackType);
			AssertEquals("UNT", Inventory.WI_UnitsUQ);

			Inventory.WI_F3_NKPackType = "BOX";
			AssertEquals("BOX", Inventory.WI_F3_NKPackType);
			AssertEquals("UNT", Inventory.WI_UnitsUQ);

			Inventory.WI_OP = ZGuid.Invalid; // Temp Product
			Inventory.WI_OP_PartNum = "TEMP";
			Inventory.WI_F3_NKPackType = "PLT";
			AssertEquals("PLT", Inventory.WI_F3_NKPackType);
			AssertEquals("PLT", Inventory.WI_UnitsUQ);
		}

		#endregion

		#region TestWI_F3_NKPackType_SetValueToDocketLine

		public void TestWI_F3_NKPackType_SetValueToDocketLine()
		{
			AssertEquals("", Inventory.WI_F3_NKPackType);
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_F3_NKPackType.Name, WhsInventoryViewSchema.WI_F3_NKPackType.Name, (ZString)"BOX", (ZString)"PLT");
		}

		#endregion

		#region TestWI_F3_NKPackTypeInfo

		public void TestWI_F3_NKPackTypeInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_F3_NKPackType, Inventory.WI_F3_NKPackTypeInfo.Name);
		}

		#endregion

		#endregion

		#region TestWI_UnitsUQ

		#region TestWI_UnitsUQ

		public void TestWI_UnitsUQ()
		{
			AssertEquals("UNT", Inventory.WI_UnitsUQ);

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "XXX";
			Inventory.WI_OP = part.PK;
			AssertEquals("XXX", Inventory.WI_UnitsUQ);
		}

		#endregion

		#region TestWI_UnitsUQ_UpdateWI_F3_NKPackTypeForTemporaryProducts

		public void TestWI_UnitsUQ_UpdateWI_F3_NKPackTypeForTemporaryProducts()
		{
			AssertEquals("UNT", Inventory.WI_UnitsUQ);
			AssertEquals("", Inventory.WI_F3_NKPackType);

			Inventory.WI_UnitsUQ = "BOX";
			AssertEquals("BOX", Inventory.WI_UnitsUQ);
			AssertEquals("", Inventory.WI_F3_NKPackType);

			Inventory.WI_OP = ZGuid.Invalid; // Temp Product
			Inventory.WI_OP_PartNum = "TEMP";
			Inventory.WI_UnitsUQ = "PLT";
			AssertEquals("PLT", Inventory.WI_UnitsUQ);
			AssertEquals("PLT", Inventory.WI_F3_NKPackType);
		}

		#endregion

		#region TestWI_UnitsUQ_SetValueToDocketLine

		public void TestWI_UnitsUQ_SetValueToDocketLine()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var inventory = docketLine.Inventory[0];
			inventory.WI_OP = ZGuid.Invalid;

			TestSetsValueToDocketLine(inventory, WhsDocketLine.Schema.ProductUQ, WhsInventoryView.Schema.WI_UnitsUQ, (ZString)"BAG", (ZString)"BOX");
		}

		#endregion

		#region TestWI_UnitsUQInfo

		public void TestWI_UnitsUQInfo()
		{
			AssertEquals("WI_UnitsUQ", Inventory.WI_UnitsUQInfo.Name);
			TestTempProductReadOnly(Inventory.WI_UnitsUQInfo);
		}

		#endregion

		#endregion

		#region TestWI_TotalUnits

		public void TestWI_TotalUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventory.InDocketLine;
			AssertEquals(10m, inventory.WI_TotalUnits);
			AssertEquals(10m, receiveLine.WE_StockOnHand);

			// when modifying WI_TotalUnits on inventory, docket line's WE_StockOnHand should also change.
			inventory.WI_TotalUnits = 20m;
			AssertEquals(20m, inventory.WI_TotalUnits);
			AssertEquals(20m, receiveLine.WE_StockOnHand);
		}

		#endregion

		#region WI_SplitQuantity

		public void TestWI_SplitQuantity()
		{
			Inventory.WI_InDocketLineUnits = 10m;

			Inventory.WI_SplitQuantity = 10m;
			AssertEquals(10m, Inventory.WI_SplitQuantity);

			Inventory.WI_SplitQuantity = -10m;
			AssertEquals(-10m, Inventory.WI_SplitQuantity);

			Inventory.WI_SplitQuantity = 10m;
			AssertEquals(10m, Inventory.WI_SplitQuantity);
		}

		public void TestWI_SplitQuantityInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_SplitQuantity, Inventory.WI_SplitQuantityInfo.Name);
		}

		#endregion

		#region TestWI_ArrivalDate

		public void TestWI_ArrivalDate()
		{
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_AdjustmentArrivalDate.Name, WhsInventoryViewSchema.WI_ArrivalDate.Name, ZDateTimeOffset.Today.AddDays(5), ZDateTimeOffset.Today.AddDays(10));
		}

		public void TestWI_ArrivalDate_NotUpdateOriginalInventoryStatus_WhenReceiveIsFinalising()
		{
			var arrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_WW_Whs = data.Whs1.PK;

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);

			Factory.Save();

			AssertEquals("Precondition", 3, receive.Inventory.Count);
			foreach (WhsInventoryView inv in receive.Inventory)
			{
				AssertEquals(ZDateTimeOffset.Empty, inv.WI_ArrivalDate);
				AssertEquals(ZDateTimeOffset.Empty, inv.InDocketLine.WE_AdjustmentArrivalDate);
				AssertEquals(InventoryStatus.Codes.Pending, inv.WI_InventoryStatus);
			}

			receive.WD_ArrivalDate = arrivalDate;
			foreach (WhsInventoryView inv in receive.Inventory)
			{
				AssertEquals("WD_ArrivalDate setter should set inventory arrival date", arrivalDate, inv.WI_ArrivalDate);
				AssertEquals("WD_ArrivalDate setter should set docket line arrival date", arrivalDate, inv.InDocketLine.WE_AdjustmentArrivalDate);
				AssertEquals("WD_ArrivalDate setter should set inventory status", InventoryStatus.Codes.Arrived, inv.WI_InventoryStatus);
			}

			inventory1.WI_InventoryStatus = InventoryStatus.Codes.Held;
			inventory1.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			inventory2.WI_InventoryStatus = InventoryStatus.Codes.Held;
			inventory2.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;

			// receive is Finalising, change WI_ArrivalDate should not update original inventory status
			AssertEquals(false, receive.IsFinalising);
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				AssertEquals(true, receive.IsFinalising);

				inventory1.WI_ArrivalDate = ZDateTimeOffset.Empty;
				inventory2.WI_ArrivalDate = ZDateTimeOffset.Empty;
				inventory3.WI_ArrivalDate = ZDateTimeOffset.Empty;
				AssertEquals(ZDateTimeOffset.Empty, inventory1.InDocketLine.WE_AdjustmentArrivalDate);
				AssertEquals(ZDateTimeOffset.Empty, inventory2.InDocketLine.WE_AdjustmentArrivalDate);
				AssertEquals(ZDateTimeOffset.Empty, inventory3.InDocketLine.WE_AdjustmentArrivalDate);

				AssertEquals("Should not change status if finalised", InventoryStatus.Codes.Held, inventory1.WI_InventoryStatus);
				AssertEquals("Should not change HeldCode if finalised", InventoryHoldCodes.Codes.Damaged, inventory1.WI_HeldCode);
				AssertEquals("Should not change status if finalised", InventoryStatus.Codes.Held, inventory2.WI_InventoryStatus);
				AssertEquals("Should not change HeldCode if finalised", InventoryHoldCodes.Codes.Held, inventory2.WI_HeldCode);
				AssertEquals("Should not change status if finalised", InventoryStatus.Codes.Arrived, inventory3.WI_InventoryStatus);
			}
		}

		public void TestWI_ArrivalDate_NotUpdateOriginalInventoryStatus_WhenReceiveIsFinalised()
		{
			var arrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_WW_Whs = data.Whs1.PK;

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);

			Factory.Save();

			AssertEquals("Precondition", 3, receive.Inventory.Count);
			foreach (WhsInventoryView inv in receive.Inventory)
			{
				AssertEquals(ZDateTimeOffset.Empty, inv.WI_ArrivalDate);
				AssertEquals(ZDateTimeOffset.Empty, inv.InDocketLine.WE_AdjustmentArrivalDate);
				AssertEquals(InventoryStatus.Codes.Pending, inv.WI_InventoryStatus);
			}

			receive.WD_ArrivalDate = arrivalDate;
			foreach (WhsInventoryView inv in receive.Inventory)
			{
				AssertEquals("WD_ArrivalDate setter should set inventory arrival date", arrivalDate, inv.WI_ArrivalDate);
				AssertEquals("WD_ArrivalDate setter should set docket line arrival date", arrivalDate, inv.InDocketLine.WE_AdjustmentArrivalDate);
				AssertEquals("WD_ArrivalDate setter should set inventory status", InventoryStatus.Codes.Arrived, inv.WI_InventoryStatus);
			}

			inventory1.WI_InventoryStatus = InventoryStatus.Codes.Held;
			inventory1.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			inventory2.WI_InventoryStatus = InventoryStatus.Codes.Held;
			inventory2.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;

			// receive Finalised, change WI_ArrivalDate should not update original inventory status
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Empty;
			inventory2.WI_ArrivalDate = ZDateTimeOffset.Empty;
			inventory3.WI_ArrivalDate = ZDateTimeOffset.Empty;
			AssertEquals(ZDateTimeOffset.Empty, inventory1.InDocketLine.WE_AdjustmentArrivalDate);
			AssertEquals(ZDateTimeOffset.Empty, inventory2.InDocketLine.WE_AdjustmentArrivalDate);
			AssertEquals(ZDateTimeOffset.Empty, inventory3.InDocketLine.WE_AdjustmentArrivalDate);

			AssertEquals("Should not change status if finalised", InventoryStatus.Codes.Held, inventory1.WI_InventoryStatus);
			AssertEquals("Should not change HeldCode if finalised", InventoryHoldCodes.Codes.Damaged, inventory1.WI_HeldCode);
			AssertEquals("Should not change status if finalised", InventoryStatus.Codes.Held, inventory2.WI_InventoryStatus);
			AssertEquals("Should not change HeldCode if finalised", InventoryHoldCodes.Codes.Held, inventory2.WI_HeldCode);
			AssertEquals("Should not change status if finalised", InventoryStatus.Codes.Arrived, inventory3.WI_InventoryStatus);
		}

		#endregion

		#region TestWI_ArrivalDateOrETA

		public void TestWI_ArrivalDateOrETA()
		{
			var arrivalDate = ZDateTimeOffset.Now.AddDays(1);
			var etaDate = arrivalDate.AddDays(1);

			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			data.Line111.WI_ArrivalDate = arrivalDate;
			data.Line111.Docket.WD_ETA = etaDate;

			data.Line111.WI_InventoryStatus = "";
			AssertEquals(arrivalDate, data.Line111.WI_ArrivalDateOrETA);

			data.Line111.WI_InventoryStatus = InventoryStatus.Codes.Pending;
			AssertEquals(etaDate, data.Line111.WI_ArrivalDateOrETA);

			data.Line111.WI_InventoryStatus = InventoryStatus.Codes.Arrived;
			AssertEquals(arrivalDate, data.Line111.WI_ArrivalDateOrETA);
		}

		#endregion

		#region WI_InventoryStatus

		public void TestStatuses()
		{
			AssertNotNull(Inventory.Statuses);
		}

		public void TestStatusDesc()
		{
			Inventory.WI_InventoryStatus = InventoryStatus.Codes.Available;
			AssertEquals(InventoryStatus.Descriptions.Available, Inventory.StatusDesc);
		}

		public void TestStatusDescInfo()
		{
			AssertEquals("StatusDesc", Inventory.StatusDescInfo.Name);
		}

		#endregion

		#region TestWI_InDocketLineUnits

		#region TestWI_InDocketLineUnits

		public void TestWI_InDocketLineUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part = Helper.CreateProduct(data.Org1, "PR1"); // auto creates a PartUnit CTN 12m
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 10);

			inventory.WI_OP = part.PK;
			inventory.WI_InDocketLineUnits = 10m;
			AssertEquals("Precondition", part.OP_StockKeepingUnit, inventory.WI_F3_NKPackType);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals("Precondition", 10m, inventory.WI_ExpectedReceiptQuantity);

			inventory.WI_InDocketLineUnits = 25m;
			AssertEquals(25m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals("WI_InDocketLineUnits Should never update ExpectedReceiptQuantity", 10m, inventory.WI_ExpectedReceiptQuantity);

			inventory.WI_F3_NKPackType = "CTN";
			AssertEquals(25m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals("WI_InDocketLineUnits updated from pack type update.", 300m, inventory.WI_InDocketLineUnits);
			AssertEquals("ExpectedReceiptQuantity updated from pack type update.", 300m, inventory.WI_ExpectedReceiptQuantity);

			Factory.Save();
			inventory.WI_InDocketLineUnits = 12m;
			AssertEquals(1m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals(12m, inventory.WI_InDocketLineUnits);
			AssertEquals("WI_InDocketLineUnits Should never update ExpectedReceiptQuantity", 300m, inventory.WI_ExpectedReceiptQuantity);
		}

		#endregion

		#region TestWI_InDocketLineUnits_WithRefreshTotalUnitsFromLinesSemaphore

		public void TestWI_InDocketLineUnits_WithRefreshTotalUnitsFromLinesSemaphore()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var part = Helper.CreateProduct(org, "P1");
			var receive = Helper.CreateWhsReceive(org, whs);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 10);

			var refreshCount = 0;
			receive.WD_TotalUnitsFromLinesInfo.ValueChanged += (sender, e) => refreshCount++;

			using (new SemaphoreManager(receive.RefreshTotalUnitsFromLinesSemaphore))
			{
				inventory.WI_InDocketLineUnits = 5m;
				AssertEquals("With Semaphore, total units from lines of reciever don't refresh", 0, refreshCount);
			}

			inventory.WI_InDocketLineUnits = 5m;
			AssertEquals("Without Semaphore, total units from lines of reciever will refresh", 1, refreshCount);
		}

		#endregion

		#region TestWI_InDocketLineUnits_WithEmptyPackType

		public void TestWI_InDocketLineUnits_WithEmptyPackType()
		{
			OrgHeader org = Helper.CreateClient();
			OrgSupplierPart part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m

			Inventory.WI_OP = part.PK;
			part.OP_StockKeepingUnit = "";
			Inventory.WI_F3_NKPackType = "";
			Inventory.WI_InDocketLineUnits = 10m;
			AssertEquals(10m, Inventory.InDocketLine.WE_PackQuantity);
		}

		#endregion

		#region TestWI_InDocketLineUnits_WithLargeUnitConversion

		public void TestWI_InDocketLineUnits_WithLargeUnitConversion()
		{
			// Refer to WI00080290
			// The important part is that we don't default WE_TransactionQuantity in this case
			// I have retained the behaviour we use on WhsDocketLine (current ALP-GP1 behaviour) which is different to DRD's original fix
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "UNT", "BOX", 10000);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			inventory.WI_F3_NKPackType = "BOX";
			inventory.WI_InDocketLineUnits = 5;
			AssertEquals(0m, inventory.InDocketLine.WE_PackQuantity);

			inventory.WI_InDocketLineUnits = 600;
			AssertEquals(0.06m, inventory.InDocketLine.WE_PackQuantity);

			inventory.WI_InDocketLineUnits = 0;
			AssertEquals(0m, inventory.InDocketLine.WE_PackQuantity);
		}

		#endregion

		#region TestWI_InDocketLineUnits_UpdatesTotalLineUnits

		public void TestWI_InDocketLineUnits_UpdatesTotalLineUnits()
		{
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");

			var receive = GetNewReceive();
			var inventory = receive.Lines.AddNew().Inventory[0];
			AssertEquals("Precondition:", 0m, receive.WD_TotalUnitsFromLines);

			inventory.WI_OP = part.PK;
			inventory.WI_InDocketLineUnits = 10m;
			AssertEquals("Total Weight is incorrect", 10m, receive.WD_TotalUnitsFromLines);
		}

		#endregion

		#region TestWI_InDocketLineUnits_UpdatesTotalWeightAndVolume

		public void TestWI_InDocketLineUnits_UpdatesTotalWeightAndVolume()
		{
			OrgHeader org = Helper.CreateClient();
			OrgSupplierPart part = Helper.CreateProduct(org, "P1");

			WhsReceive receive = GetNewReceive();
			var inventory = receive.Inventory.AddNew();

			receive.WD_TotalCubicUnit = Constants.Volume.Litre;
			receive.WD_TotalWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Precondition:", 0m, receive.WD_TotalWeight);
			AssertEquals("Precondition:", 0m, receive.WD_TotalCubic);

			inventory.WI_OP = part.PK;
			inventory.WI_InDocketLineUnits = 10m;

			AssertEquals("Total Weight is incorrect", 44.09m, receive.WD_TotalWeight);
			AssertEquals("Total Volume is incorrect", 200m, receive.WD_TotalCubic);
		}

		#endregion

		#region TestWI_InDocketLineUnits_UpdateWE_PackQuantityForTemporaryProducts

		public void TestWI_InDocketLineUnits_UpdateWE_PackQuantityForTemporaryProducts()
		{
			AssertEquals(0m, Inventory.WI_InDocketLineUnits);
			AssertEquals(0m, Inventory.InDocketLine.WE_PackQuantity);

			Inventory.WI_InDocketLineUnits = 5m;
			AssertEquals(5m, Inventory.WI_InDocketLineUnits);
			AssertEquals(0m, Inventory.InDocketLine.WE_PackQuantity);

			Inventory.WI_OP = ZGuid.Invalid; // Temp Product
			Inventory.WI_OP_PartNum = "TEMP";
			Inventory.WI_InDocketLineUnits = 10m;
			AssertEquals(10m, Inventory.WI_InDocketLineUnits);
			AssertEquals(10m, Inventory.InDocketLine.WE_PackQuantity);
		}

		#endregion

		#region TestWI_InDocketLineUnits_SetValueToDocketLine

		public void TestWI_InDocketLineUnits_SetValueToDocketLine()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			TestSetsValueToDocketLine(receiveLine.Inventory[0], WhsDocketLineSchema.WE_TransactionQuantity.Name, WhsInventoryViewSchema.WI_InDocketLineUnits.Name, (ZDecimal)2m, (ZDecimal)5m);

			var adjustmentLine = Factory.New<WhsAdjustmentLine>();
			var inventory = adjustmentLine.Inventory.AddNew();
			inventory.WI_InDocketLineType = DocketType.Codes.Adjustment;
			inventory.WI_WE_InDocketLine = adjustmentLine.PK;
			AssertEquals("Precondition", 0m, adjustmentLine.WE_TransactionQuantity);
			AssertEquals("Precondition", 0m, inventory.WI_InDocketLineUnits);

			adjustmentLine.WE_TransactionQuantity = 5m;
			AssertEquals("For adjustments In Docket Line Units should stay at 0.", 0m, inventory.WI_InDocketLineUnits);

			inventory.WI_InDocketLineUnits = 10m;
			AssertEquals("Inventory should not populate adjustment line qty.", 5m, adjustmentLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestWI_InDocketLineUnits_SetWI_TotalUnits

		public void TestWI_InDocketLineUnits_SetWI_TotalUnits()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var inventory = receiveLine.Inventory[0];
			AssertEquals("Precondition", 0m, inventory.WI_TotalUnits);

			inventory.WI_InDocketLineUnits = 5m;
			AssertEquals("Setting WI_InDocketLineUnits should propagate to WI_TotalUnits.", 5m, inventory.WI_TotalUnits);
		}

		#endregion

		#endregion

		#region TestUsedAttributesCount

		public void TestUsedAttributesCount()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_PartAttrib1 = "";
			inventory.WI_PartAttrib2 = "";
			inventory.WI_PartAttrib3 = "";
			inventory.WI_PackingDate = ZDate.Empty;
			inventory.WI_ExpiryDate = ZDate.Empty;
			inventory.WI_PalletID = "";
			AssertEquals(0, inventory.UsedAttributesAndPalletIdCount);

			inventory.WI_PartAttrib1 = "1";
			AssertEquals(1, inventory.UsedAttributesAndPalletIdCount);

			inventory.WI_PartAttrib2 = "2";
			AssertEquals(2, inventory.UsedAttributesAndPalletIdCount);

			inventory.WI_PartAttrib3 = "3";
			AssertEquals(3, inventory.UsedAttributesAndPalletIdCount);

			inventory.WI_PackingDate = ZDate.Today;
			AssertEquals(4, inventory.UsedAttributesAndPalletIdCount);

			inventory.WI_ExpiryDate = ZDate.Today;
			AssertEquals(5, inventory.UsedAttributesAndPalletIdCount);

			inventory.WI_PalletID = "ABC";
			AssertEquals(6, inventory.UsedAttributesAndPalletIdCount);

			inventory.WI_SerialNumber = "SN1";
			AssertEquals(7, inventory.UsedAttributesAndPalletIdCount);
		}

		#endregion

		#region CommodityCode

		public void TestCommodityCode()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			AssertEquals(ZString.Empty, inventory.CommodityCode);

			var part = Helper.CreateProduct(Helper.CreateClient("xxx"), "P1");
			inventory.WI_OP = part.PK;
			AssertEquals(ZString.Empty, inventory.CommodityCode);

			part.OP_RH_NKCommodityCode = "11";
			AssertEquals(part.OP_RH_NKCommodityCode, inventory.CommodityCode);
		}

		public void TestCommodityCode_SetValueToDocketLine()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var inventory = docketLine.Inventory[0];
			inventory.WI_OP = ZGuid.Invalid;

			TestSetsValueToDocketLine(inventory, WhsDocketLine.Schema.CommodityCode, WhsInventoryView.Schema.CommodityCode, (ZString)"AFAT", (ZString)"BEDD");
		}

		public void TestCommodityCodeInfo()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			AssertEquals("CommodityCode", inventory.CommodityCodeInfo.Name);
			TestTempProductReadOnly(inventory.CommodityCodeInfo);
		}

		#endregion

		#region WI_PalletID

		public void TestWI_PalletID()
		{
			Inventory.ValidationPalletIDWarningMessage = "ToDelete";
			Inventory.WI_PalletID = "12345A";
			AssertEquals("12345A", Inventory.WI_PalletID);
			AssertEquals(ZString.Empty, Inventory.ValidationPalletIDWarningMessage);

			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_PalletID.Name, WhsInventoryViewSchema.WI_PalletID.Name, (ZString)"TEST 1", (ZString)"TEST 2");
		}

		#endregion

		#region Areas

		#region TestLocationArea

		public void TestLocationArea()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("1", "A");
			WhsLocation loc = whs.DefaultLocation;
			Inventory.WI_WL = loc.PK;
			AssertEquals(loc.PickingArea, Inventory.LocationPickArea);
		}

		#endregion

		#region TestLocationPickAreaName

		public void TestLocationPickAreaName_Translatable()
		{
			var whs = Helper.CreateWarehouse("W1", "A");
			var location = whs.DefaultLocation;
			location.PickingArea.WA_Name = "TestArea";
			Inventory.WI_WL = location.PK;

			string resKey = location.PickingArea.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(location.PickingArea, "TestArea").ResourceKey;
			AssertEquals("TestArea", location.PickingArea.WA_NameMultilingual);
			AssertEquals("LocationPickAreaName in English.", "TestArea", Inventory.LocationPickAreaName);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试区"));
				AssertEquals("LocationPickAreaName in Chinese", "测试区", Inventory.LocationPickAreaName);
			}
		}

		#endregion

		#region TestLocationPickAreaNameAndType

		public void TestLocationPickAreaNameAndType()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var loc = whs.DefaultLocation;
			loc.PickingArea.WA_Name = "A1";
			loc.PickingArea.WA_AreaType = "AT1";
			Factory.Save();
			Inventory.WI_WL = loc.PK;
			AssertEquals("A1", Inventory.LocationPickAreaName);
			AssertEquals("AT1", Inventory.LocationPickAreaType);
		}

		#endregion

		#region TestLocationPickAreaNameAndTypeInfos

		public void TestLocationPickAreaNameAndTypeInfos()
		{
			AssertEquals("LocationPickAreaName", Inventory.LocationPickAreaNameInfo.Name);
			AssertEquals("LocationPickAreaType", Inventory.LocationPickAreaTypeInfo.Name);
		}

		#endregion

		#region TestLocationPutawayAreaType

		public void TestLocationPutawayAreaType()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var loc = whs.DefaultLocation;
			loc.PutawayArea.WA_Name = "A1";
			loc.PutawayArea.WA_AreaType = "AT1";
			Factory.Save();

			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_WL = loc.PK;
			AssertEquals("AT1", inventory.LocationPutawayAreaType);
		}

		#endregion

		#endregion

		#region Details

		public void TestDetails()
		{
			AssertEquals("Details", Inventory.Details);
		}

		#endregion

		#region WI_TotalValue

		public void TestWI_TotalValue()
		{
			AssertEquals(0m, Inventory.WI_TotalValue);

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_LastCost = 5m;
			Inventory.WI_OP = part.PK;
			Inventory.WI_TotalUnits = 10m;
			AssertEquals(50m, Inventory.WI_TotalValue);
		}

		public void TestWI_TotalValueInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_TotalValue, Inventory.WI_TotalValueInfo.Name);
		}

		#endregion

		#region WI_Currency

		public void TestWI_Currency()
		{
			AssertEquals("", Inventory.WI_Currency);

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_RX_NKLastWeightedCostCurr = "AAA";
			Inventory.WI_OP = part.PK;
			AssertEquals("AAA", Inventory.WI_Currency);
		}

		public void TestWI_CurrencyInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_Currency, Inventory.WI_CurrencyInfo.Name);
		}

		#endregion

		#region WI_LastCost

		public void TestWI_LastCost()
		{
			AssertEquals(0m, Inventory.WI_LastCost);

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_LastCost = 5m;
			Inventory.WI_OP = part.PK;
			AssertEquals(5m, Inventory.WI_LastCost);
		}

		public void TestWI_LastCostInfo()
		{
			AssertEquals(WhsInventoryView.Schema.WI_LastCost, Inventory.WI_LastCostInfo.Name);
		}

		#endregion

		#region TestWI_ReceiveCrossDockOrderNo

		public void TestWI_ReceiveCrossDockOrderNo()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(true, false, ZDateTimeOffset.Today, false);
			var inventory = data.Line111;

			inventory.WI_ReceiveCrossDockOrderNo = "1";
			AssertEquals("1", inventory.WI_ReceiveCrossDockOrderNo);

			inventory.WI_ReceiveCrossDockOrderNo = "2";
			AssertEquals("2", inventory.WI_ReceiveCrossDockOrderNo);

			Factory.Save(); // test caching bringing it in from docketline blablabla

			// InDocketLine is now created and set
			AssertEquals("2", inventory.WI_ReceiveCrossDockOrderNo);
			AssertEquals("2", inventory.InDocketLine.WE_ReceiveCrossDockOrderNo);

			inventory.WI_ReceiveCrossDockOrderNo = "1";
			AssertEquals("1", inventory.WI_ReceiveCrossDockOrderNo);
			AssertEquals("1", inventory.InDocketLine.WE_ReceiveCrossDockOrderNo);
		}

		#endregion

		#region TestWI_WD

		public void TestWI_WD()
		{
			var client = Helper.CreateClient();
			var receive = Factory.New<WhsReceive>();
			receive.WD_OH_Client = client.PK;
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;

			var inventory = Factory.New<WhsInventoryView>();
			AssertEquals("Precondition", 0, receive.Inventory.Count);
			AssertEquals("Precondition", ZGuid.Empty, inventory.WI_OH_Client);
			AssertEquals("Precondition", "", inventory.WI_InDocketLineType);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, inventory.WI_ArrivalDate);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory.OriginalInventoryStatus);

			inventory.WI_WD = receive.PK;
			AssertEquals("When inventories link to docket established it should automatically be added to the docket.", inventory, receive.Inventory.Single());
			AssertEquals("When inventories link to docket established it should automatically populate client on Inventory.", client.PK, inventory.WI_OH_Client);
			AssertEquals("When inventories link to docket established it should automatically populate InDocketLineType on Inventory.", DocketType.Codes.Receive, inventory.WI_InDocketLineType);
			AssertEquals("When inventories link to docket established it should automatically populate Arrival Date on Inventory.", receive.WD_ArrivalDate, inventory.WI_ArrivalDate);
			AssertEquals("When inventories link to docket established it should automatically set correct inventory status.", InventoryStatus.Codes.Arrived, inventory.OriginalInventoryStatus);

			var receive2 = Factory.New<WhsReceive>();
			inventory.WI_WD = receive2.PK;
			AssertEquals("When inventories link to docket established it should automatically removed from old docket.", 0, receive.Inventory.Count);
			AssertEquals("When inventories link to docket established it should automatically be added to the docket.", inventory, receive2.Inventory.Single());
		}

		public void TestWI_WD_SetsValueToDocketLine()
		{
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_WD.Name, WhsInventoryViewSchema.WI_WD.Name, ZGuid.NewZGuid(), ZGuid.NewZGuid());
		}

		#endregion

		#region TestWI_WD_Proxy

		public void TestWI_WD_Proxy()
		{
			var inventory = Factory.New<WhsInventoryView>();
			AssertEquals(ZGuid.Empty, inventory.WI_WD_Proxy);

			var receiveLine = Factory.New<WhsReceiveLine>();
			inventory.WI_InDocketLineType = DocketType.Codes.Receive;
			inventory.WI_WE_InDocketLine = receiveLine.PK;
			AssertEquals(ZGuid.Empty, inventory.WI_WD_Proxy);

			var receive1 = Factory.New<WhsReceive>();
			receiveLine.WE_WD = receive1.PK;
			AssertEquals(receive1.PK, inventory.WI_WD_Proxy);

			var receive2 = Factory.New<WhsReceive>();
			inventory.WI_WD = receive2.PK;
			AssertEquals(receive2.PK, inventory.WI_WD_Proxy);
		}

		#endregion

		#region Part Attributes

		#region TestWI_ExpiryDate

		public void TestWI_ExpiryDate()
		{
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_ExpiryDate.Name, WhsInventoryViewSchema.WI_ExpiryDate.Name, ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(10));
		}

		#endregion

		#region TestWI_PackingDate

		public void TestWI_PackingDate()
		{
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_PackingDate.Name, WhsInventoryViewSchema.WI_PackingDate.Name, ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(10));
		}

		#endregion

		#region TestWI_PartAttrib1

		[TestDate(2013, 2, 26)]
		public void TestWI_PartAttrib1_SetExpiryDateIfJulianBatchNumberIsUsed()
		{
			TestWI_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(WhsInventoryViewSchema.WI_PartAttrib1, AttributeNumber.One);
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_PartAttrib1.Name, WhsInventoryViewSchema.WI_PartAttrib1.Name, (ZString)"TEST 1", (ZString)"TEST 2");
		}

		#endregion

		#region TestWI_PartAttrib2

		[TestDate(2013, 2, 26)]
		public void TestWI_PartAttrib2_SetExpiryDateIfJulianBatchNumberIsUsed()
		{
			TestWI_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(WhsInventoryViewSchema.WI_PartAttrib2, AttributeNumber.Two);
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_PartAttrib2.Name, WhsInventoryViewSchema.WI_PartAttrib2.Name, (ZString)"TEST 1", (ZString)"TEST 2");
		}

		#endregion

		#region TestWI_PartAttrib3

		[TestDate(2013, 2, 26)]
		public void TestWI_PartAttrib3_SetExpiryDateIfJulianBatchNumberIsUsed()
		{
			TestWI_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(WhsInventoryViewSchema.WI_PartAttrib3, AttributeNumber.Three);
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_PartAttrib3.Name, WhsInventoryViewSchema.WI_PartAttrib3.Name, (ZString)"TEST 1", (ZString)"TEST 2");
		}

		#endregion

		#region TestWI_SerialNumber

		public void TestWI_SerialNumber()
		{
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_SerialNumber.Name, WhsInventoryViewSchema.WI_SerialNumber.Name, (ZString)"TEST 1", (ZString)"TEST 2");
		}

		#endregion

		#region TestIsSyncingDocketLineInventoryView

		public void TestIsSyncingDocketLineInventoryView()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			var inventory = receive.Inventory[0];

			var reciveline = receive.Lines[0];
			reciveline.WE_PartAttrib1Info.ValueChanged += (sender, e) => { reciveline.WE_PartAttrib2 = "Att2 "; };

			Assert("Precondition", inventory.WI_PartAttrib1.IsEmpty);
			Assert("Precondition", inventory.WI_PartAttrib2.IsEmpty);
			Assert("Precondition", reciveline.WE_PartAttrib1.IsEmpty);
			Assert("Precondition", reciveline.WE_PartAttrib2.IsEmpty);

			inventory.WI_PartAttrib1 = "Att1 ";
			AssertEquals("Precondition", "Att1", reciveline.WE_PartAttrib1);
			AssertEquals("Precondition", "Att1", inventory.WI_PartAttrib1);
			AssertEquals("Precondition", "Att2", reciveline.WE_PartAttrib2);
			AssertEquals("Precondition", "Att2", inventory.WI_PartAttrib2);
		}

		#endregion

		#region TestWI_PartAttrib_SetExpiryDateIfJulianBatchNumberIsUsedCore

		void TestWI_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(SchemaStringColumn partAttributeColumn, AttributeNumber attributeNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 10;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			var inventory = receive.Inventory[0];

			// Normal Attribute
			AssertEquals("Precondition", ZDateTime.Empty, inventory.WI_ExpiryDate);

			inventory[partAttributeColumn] = "ABC1005";
			AssertEquals("Expiry date should not be modified when normal attribute is set.", ZDateTime.Empty, inventory.WI_ExpiryDate);

			// Julian Batch Number Attribute
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber); // should turn on Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true); // should turn on Expiry Date
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;

			inventory[partAttributeColumn] = "";
			AssertEquals("Expiry date should be empty when Julian Batch Number is not set.", ZDateTime.Empty, inventory.WI_ExpiryDate);
			AssertEquals("Packing date should be empty when Julian Batch Number is not set.", ZDateTime.Empty, inventory.WI_PackingDate);

			inventory[partAttributeColumn] = "ABC1005";
			AssertEquals("Expiry date should be set when Julian Batch Number is set.", new ZDateTime(2010 + 1, 1, 1).AddDays((005 - 1) + 10), inventory.WI_ExpiryDate);
			AssertEquals("Packing date should be set when Julian Batch Number is set.", new ZDateTime(2010 + 1, 1, 1).AddDays((005 - 1)), inventory.WI_PackingDate);

			inventory[partAttributeColumn] = "ABC4050";
			AssertEquals("Expiry date should be set when Julian Batch Number is set.", new ZDateTime(2000 + 4, 1, 1).AddDays((050 - 1) + 10), inventory.WI_ExpiryDate);
			AssertEquals("Packing date should be set when Julian Batch Number is set.", new ZDateTime(2000 + 4, 1, 1).AddDays((050 - 1)), inventory.WI_PackingDate);

			inventory[partAttributeColumn] = "1005ABC";
			AssertEquals("Expiry date should be empty when Julian Batch Number is incorrect.", ZDateTime.Empty, inventory.WI_ExpiryDate);
			AssertEquals("Packing date should be empty when Julian Batch Number is incorrect.", ZDateTime.Empty, inventory.WI_PackingDate);
		}

		#endregion

		#region Readonly

		#region TestWI_PartAttrib1Info

		public void TestWI_PartAttrib1Info()
		{
			PartAttributesTestingCore(AttributeNumber.One);
		}

		#endregion

		#region TestWI_PartAttrib2Info

		public void TestWI_PartAttrib2Info()
		{
			PartAttributesTestingCore(AttributeNumber.Two);
		}

		#endregion

		#region TestWI_PartAttrib3Info

		public void TestWI_PartAttrib3Info()
		{
			PartAttributesTestingCore(AttributeNumber.Three);
		}

		#endregion

		#region TestWI_SerialNumberInfo

		public void TestWI_SerialNumberInfo()
		{
			PartAttributesTestingCore(AttributeNumber.Serial);
		}

		#endregion

		#region TestWI_ExpectedReceiptQuantity

		public void TestWI_ExpectedReceiptQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			TestStandardReadOnly(inventory.WI_ExpectedReceiptQuantityInfo);
		}

		public void TestWI_ExpectedReceiptQuantity_SetsInDocketLineUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part = Helper.CreateProduct(data.Org1, "PR1"); // auto creates a PartUnit CTN 12m
			Helper.CreateProductUnit(part, "BOX", 2m);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 10);

			AssertEquals("Precondition", 10m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("Precondition", 10m, inventory.WI_InDocketLineUnits);

			inventory.WI_ExpectedReceiptQuantity = 20m;
			AssertEquals("WI_ExpectedReceiptQuantity is assigned.", 20m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("WI_InDocketLineUnits is assigned.", 20m, inventory.WI_InDocketLineUnits);
		}

		public void TestWI_ExpectedReceiptQuantity_FinalisedDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var inventory = receiveLine.Inventory[0];
			AssertEquals("Precondition: WI_ExpectedReceiptQuantity is assigned.", 10m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("Precondition: WI_InDocketLineUnits is assigned.", 10m, inventory.WI_InDocketLineUnits);
			receiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receiveLine);
			inventory.WI_ExpectedReceiptQuantity = 15m;
			AssertEquals("WI_ExpectedReceiptQuantity is updated.", 15m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("WI_InDocketLineUnits is not updated.", 10m, inventory.WI_InDocketLineUnits);
		}

		public void TestWI_ExpectedReceiptQuantity_AfterAsnLinesCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Precondition: WE_ClientOrderedUnits is assigned.", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: WE_TransactionQuantity is assigned.", 10m, receiveLine.WE_TransactionQuantity);
			Factory.Save();

			receive.PopulateASNLines();
			Factory.Save();

			AssertEquals("Precondition: receive has ASN lines.", true, receive.AsnLines.Count > 0);
			receiveLine.WE_ClientOrderedUnits = 15m;
			AssertEquals("WE_ClientOrderedUnits is updated.", 15m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("WE_TransactionQuantity is not updated.", 10m, receiveLine.WE_TransactionQuantity);
		}

		public void TestWI_ExpectedReceiptQuantity_SettingPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part = Helper.CreateProduct(data.Org1, "PR1"); // auto creates a PartUnit CTN 12m
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 10);

			inventory.WI_OP = part.PK;
			AssertEquals("Precondition", part.OP_StockKeepingUnit, inventory.WI_F3_NKPackType);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals("Precondition", 10m, inventory.WI_ExpectedReceiptQuantity);

			inventory.WI_ExpectedReceiptQuantity = 25m;
			AssertEquals(25m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals(25m, inventory.WI_InDocketLineUnits);

			inventory.WI_F3_NKPackType = "CTN";
			AssertEquals(25m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals(300m, inventory.WI_InDocketLineUnits);
			AssertEquals(300m, inventory.WI_ExpectedReceiptQuantity);
		}

		public void TestWI_ExpectedReceiptQuantity_SettingPackTypeAfterAsnLinesCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part = Helper.CreateProduct(data.Org1, "PR1"); // auto creates a PartUnit CTN 12m
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 10);

			inventory.WI_OP = part.PK;
			AssertEquals("Precondition", part.OP_StockKeepingUnit, inventory.WI_F3_NKPackType);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals("Precondition", 10m, inventory.WI_ExpectedReceiptQuantity);

			inventory.WI_ExpectedReceiptQuantity = 25m;
			AssertEquals(25m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals(25m, inventory.WI_InDocketLineUnits);
			Factory.Save();

			receive.PopulateASNLines();
			Factory.Save();

			inventory.WI_F3_NKPackType = "CTN";
			AssertEquals(25m, inventory.InDocketLine.WE_PackQuantity);
			AssertEquals(300m, inventory.WI_InDocketLineUnits);
			AssertEquals(25m, inventory.WI_ExpectedReceiptQuantity);
		}

		#endregion

		#region TestWI_ExpiryDateInfo

		#region TestWI_ExpiryDateInfo

		public void TestWI_ExpiryDateInfo()
		{
			PartAttributesTestingCore(AttributeNumber.ExpiryDate);
		}

		#endregion

		#region TestWI_ExpiryDateInfo_WithJulianBatchNumberAttribute

		public void TestWI_ExpiryDateInfo_WithJulianBatchNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			var inventory = receive.Inventory[0];

			TestDateInfoWithJulianBatchNumberAttributeCore(data.Org1, data.Part1, inventory.WI_ExpiryDateInfo, AttributeNumber.One);
			TestDateInfoWithJulianBatchNumberAttributeCore(data.Org1, data.Part1, inventory.WI_ExpiryDateInfo, AttributeNumber.Two);
			TestDateInfoWithJulianBatchNumberAttributeCore(data.Org1, data.Part1, inventory.WI_ExpiryDateInfo, AttributeNumber.Three);
		}

		public void TestWI_PackingDateInfo_WithJulianBatchNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			var inventory = receive.Inventory[0];

			TestDateInfoWithJulianBatchNumberAttributeCore(data.Org1, data.Part1, inventory.WI_PackingDateInfo, AttributeNumber.One);
			TestDateInfoWithJulianBatchNumberAttributeCore(data.Org1, data.Part1, inventory.WI_PackingDateInfo, AttributeNumber.Two);
			TestDateInfoWithJulianBatchNumberAttributeCore(data.Org1, data.Part1, inventory.WI_PackingDateInfo, AttributeNumber.Three);
		}

		void TestDateInfoWithJulianBatchNumberAttributeCore(OrgHeader client, OrgSupplierPart part, ZPropertyInfo propertyInfo, AttributeNumber attributeNumber)
		{
			Helper.SetClientAttributeType(client, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(client, part, attributeNumber, true);
			AssertEquals(false, propertyInfo.ReadOnly);

			Helper.SetClientAttributeType(client, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(client, part, attributeNumber, true);
			AssertEquals(true, propertyInfo.ReadOnly);
			Helper.SetProductAttributeUse(client, part, attributeNumber, false); // clean up
		}

		#endregion

		#endregion

		#region TestWI_PackingDateInfo

		public void TestWI_PackingDateInfo()
		{
			PartAttributesTestingCore(AttributeNumber.PackingDate);
		}

		#endregion

		#endregion

		#endregion

		#region TestWI_BondedEntryKey

		public void TestWI_BondedEntryKey()
		{
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_BondedEntryKey.Name, WhsInventoryViewSchema.WI_BondedEntryKey.Name, (ZString)"TEST 1", (ZString)"TEST 2");
		}

		#endregion

		#region TestWI_AllocationKey

		public void TestWI_AllocationKey()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			TestSetsValueToDocketLine(inventory, WhsDocketLineSchema.WE_AllocationKey.Name, WhsInventoryViewSchema.WI_AllocationKey.Name, (ZString)"TEST 1", (ZString)"TEST 2");
			AssertEquals(true, inventory.WI_AllocationKeyInfo.ReadOnly);
		}

		#endregion

		#region TestWI_IsOriginalReceiptLine

		public void TestWI_IsOriginalReceiptLine()
		{
			TestSetsValueToDocketLine(Inventory, WhsDocketLineSchema.WE_IsOriginalInventory.Name, WhsInventoryViewSchema.WI_IsOriginalReceiptLine.Name, ZBool.False, ZBool.True);
		}

		#endregion

		#region TestPutawayReceives_InventoryStatus

		public void TestPutawayReceives_InventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receiveWithArrivalDate = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Today);
			var receiveWithoutArrivalDate = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", ZDateTimeOffset.Empty);
			var receiveLineWithArrivalDate = receiveWithArrivalDate.Lines.AddNew();
			var receiveLineWithoutArrivalDate = receiveWithoutArrivalDate.Lines.AddNew();
			var inventoryWithArrivalDate = receiveLineWithArrivalDate.Inventory[0];
			var inventoryWithoutArrivalDate = receiveLineWithoutArrivalDate.Inventory[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Arrived, inventoryWithArrivalDate.OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventoryWithoutArrivalDate.OriginalInventoryStatus);

			receiveLineWithArrivalDate.WE_WL = dockDoorLocation.PK;
			receiveLineWithoutArrivalDate.WE_WL = dockDoorLocation.PK;
			AssertEquals("Since there is a dock door location, status should be received.", InventoryStatus.Codes.Received, inventoryWithArrivalDate.OriginalInventoryStatus);
			AssertEquals("Since there is a dock door location, status should be received.", InventoryStatus.Codes.Received, inventoryWithoutArrivalDate.OriginalInventoryStatus);

			receiveLineWithArrivalDate.WE_WL = nonDockDoorLocation.PK;
			receiveLineWithoutArrivalDate.WE_WL = nonDockDoorLocation.PK;
			AssertEquals(InventoryStatus.Codes.Putaway, inventoryWithArrivalDate.OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Putaway, inventoryWithoutArrivalDate.OriginalInventoryStatus);

			// Only happens when a putaway transfer is cancelled
			receiveLineWithArrivalDate.WE_WL = ZGuid.Empty;
			receiveLineWithoutArrivalDate.WE_WL = ZGuid.Empty;
			AssertEquals(InventoryStatus.Codes.Arrived, inventoryWithArrivalDate.OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Pending, inventoryWithoutArrivalDate.OriginalInventoryStatus);
		}

		#endregion

		// Calculated Properties

		#region TestReceiptReference

		public void TestReceiptReference()
		{
			AssertEquals("Precondition: ReceiptReference should be empty", ZString.Empty, Inventory.ReceiptReference);
			var receive = Factory.NewWithValidTestData<WhsReceive>();

			receive.WD_ExternalReference = "TEST";
			Inventory.WI_WD = receive.PK;

			AssertEquals("ReceiptReference has valid value", "TEST", Inventory.ReceiptReference);
		}

		#endregion

		#region Custom Attributes

		public void TestWI_CustomAttrib1()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomAttrib_1 = "CA1";
			AssertEquals("CA1", inventory.WI_CustomAttrib_1);
			inventory.WI_CustomAttrib_1 = "CA2";
			AssertEquals("CA2", inventory.WI_CustomAttrib_1);
		}

		public void TestWI_CustomAttrib2()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomAttrib_2 = "CA1";
			AssertEquals("CA1", inventory.WI_CustomAttrib_2);
			inventory.WI_CustomAttrib_2 = "CA2";
			AssertEquals("CA2", inventory.WI_CustomAttrib_2);
		}

		public void TestWI_CustomAttrib3()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomAttrib_3 = "CA1";
			AssertEquals("CA1", inventory.WI_CustomAttrib_3);
			inventory.WI_CustomAttrib_3 = "CA2";
			AssertEquals("CA2", inventory.WI_CustomAttrib_3);
		}

		public void TestWI_CustomAttrib4()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomAttrib4 = "CA1";
			AssertEquals("CA1", inventory.WI_CustomAttrib4);
			inventory.WI_CustomAttrib4 = "CA2";
			AssertEquals("CA2", inventory.WI_CustomAttrib4);
		}

		public void TestWI_CustomAttrib5()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomAttrib5 = "CA1";
			AssertEquals("CA1", inventory.WI_CustomAttrib5);
			inventory.WI_CustomAttrib5 = "CA2";
			AssertEquals("CA2", inventory.WI_CustomAttrib5);
		}

		public void TestWI_CustomAttrib6()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomAttrib6 = "CA1";
			AssertEquals("CA1", inventory.WI_CustomAttrib6);
			inventory.WI_CustomAttrib6 = "CA2";
			AssertEquals("CA2", inventory.WI_CustomAttrib6);
		}

		public void TestWI_CustomDecimal1()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomDecimal1 = 10.1m;
			AssertEquals(10.1m, inventory.WI_CustomDecimal1);
			inventory.WI_CustomDecimal1 = 20.2m;
			AssertEquals(20.2m, inventory.WI_CustomDecimal1);
		}

		public void TestWI_CustomDecimal2()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomDecimal2 = 10.1m;
			AssertEquals(10.1m, inventory.WI_CustomDecimal2);
			inventory.WI_CustomDecimal2 = 20.2m;
			AssertEquals(20.2m, inventory.WI_CustomDecimal2);
		}

		public void TestWI_CustomDecimal3()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomDecimal3 = 10.1m;
			AssertEquals(10.1m, inventory.WI_CustomDecimal3);
			inventory.WI_CustomDecimal3 = 20.2m;
			AssertEquals(20.2m, inventory.WI_CustomDecimal3);
		}

		public void TestWI_CustomDecimal4()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomDecimal4 = 10.1m;
			AssertEquals(10.1m, inventory.WI_CustomDecimal4);
			inventory.WI_CustomDecimal4 = 20.2m;
			AssertEquals(20.2m, inventory.WI_CustomDecimal4);
		}

		public void TestWI_CustomDecimal5()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomDecimal5 = 10.1m;
			AssertEquals(10.1m, inventory.WI_CustomDecimal5);
			inventory.WI_CustomDecimal5 = 20.2m;
			AssertEquals(20.2m, inventory.WI_CustomDecimal5);
		}

		public void TestWI_CustomDate1()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			ZDateTime today = ZDateTime.Today;
			inventory.WI_CustomDate1 = today;
			AssertEquals(today, inventory.WI_CustomDate1);
			inventory.WI_CustomDate1 = today.AddDays(1);
			AssertEquals(today.AddDays(1), inventory.WI_CustomDate1);
		}

		public void TestWI_CustomDate2()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			ZDateTime today = ZDateTime.Today;
			inventory.WI_CustomDate2 = today;
			AssertEquals(today, inventory.WI_CustomDate2);
			inventory.WI_CustomDate2 = today.AddDays(1);
			AssertEquals(today.AddDays(1), inventory.WI_CustomDate2);
		}

		public void TestWI_CustomDate3()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			ZDateTime today = ZDateTime.Today;
			inventory.WI_CustomDate3 = today;
			AssertEquals(today, inventory.WI_CustomDate3);
			inventory.WI_CustomDate3 = today.AddDays(1);
			AssertEquals(today.AddDays(1), inventory.WI_CustomDate3);
		}

		public void TestWI_CustomDate4()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			ZDateTime today = ZDateTime.Today;
			inventory.WI_CustomDate4 = today;
			AssertEquals(today, inventory.WI_CustomDate4);
			inventory.WI_CustomDate4 = today.AddDays(1);
			AssertEquals(today.AddDays(1), inventory.WI_CustomDate4);
		}

		public void TestWI_CustomDate5()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			ZDateTime today = ZDateTime.Today;
			inventory.WI_CustomDate5 = today;
			AssertEquals(today, inventory.WI_CustomDate5);
			inventory.WI_CustomDate5 = today.AddDays(1);
			AssertEquals(today.AddDays(1), inventory.WI_CustomDate5);
		}

		public void TestWI_CustomFlag1()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomFlag1 = true;
			AssertEquals(true, inventory.WI_CustomFlag1);
			inventory.WI_CustomFlag1 = false;
			AssertEquals(false, inventory.WI_CustomFlag1);
		}

		public void TestWI_CustomFlag2()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomFlag2 = true;
			AssertEquals(true, inventory.WI_CustomFlag2);
			inventory.WI_CustomFlag2 = false;
			AssertEquals(false, inventory.WI_CustomFlag2);
		}

		public void TestWI_CustomFlag3()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomFlag3 = true;
			AssertEquals(true, inventory.WI_CustomFlag3);
			inventory.WI_CustomFlag3 = false;
			AssertEquals(false, inventory.WI_CustomFlag3);
		}

		public void TestWI_CustomFlag4()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomFlag4 = true;
			AssertEquals(true, inventory.WI_CustomFlag4);
			inventory.WI_CustomFlag4 = false;
			AssertEquals(false, inventory.WI_CustomFlag4);
		}

		public void TestWI_CustomFlag5()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomFlag5 = true;
			AssertEquals(true, inventory.WI_CustomFlag5);
			inventory.WI_CustomFlag5 = false;
			AssertEquals(false, inventory.WI_CustomFlag5);
		}

		public void TestWI_CustomTextBlob1()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var inventory = data.Line111;

			inventory.WI_CustomTextBlob1 = "CA1";
			AssertEquals("CA1", inventory.WI_CustomTextBlob1);
			inventory.WI_CustomTextBlob1 = "CA2";
			AssertEquals("CA2", inventory.WI_CustomTextBlob1);
		}

		#endregion

		#region TestPackageGroupId

		public void TestPackageGroupId()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = "CUS";
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertEquals("Precondition", "", inventory.PackageGroupId);

			inventory.InDocketLine.WE_PackageGroupId = "123";
			AssertEquals("123", inventory.PackageGroupId);
		}

		#endregion

		#region TestPackageGroupIdInfo

		public void TestPackageGroupIdInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertNotNull(inventory.PackageGroupIdInfo);
		}

		#endregion

		#region TestPerPackageQty

		public void TestPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = "CUS";
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertEquals("Precondition", 0m, inventory.PerPackageQty);

			inventory.PerPackageQty = 2m;
			AssertEquals(2m, inventory.PerPackageQty);
			Factory.Save();
			AssertNotNull(inventory.InDocketLine);
			AssertEquals(2m, inventory.InDocketLine.WE_PerPackageQty);

			inventory.PerPackageQty = 4m;
			AssertEquals(4m, inventory.InDocketLine.WE_PerPackageQty);

			var inventoryInOtherFactory = new BusinessObjectFactory().Load<WhsInventoryView>(inventory.PK);
			inventoryInOtherFactory.InDocketLine.WE_PerPackageQty = 5m;
			AssertEquals(5m, inventoryInOtherFactory.PerPackageQty);
		}

		#endregion

		#region TestPerPackageQtyInfo

		public void TestPerPackageQtyInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			TestStandardReadOnly(inventory.PerPackageQtyInfo);
		}

		#endregion

		#region TestTouchesUntilStocktake

		public void TestTouchesUntilStocktake()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var part2 = Helper.CreateProduct(data.Org1, "P1");
			var location = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, location, "");
			AssertEquals("Precondition", 0, receive1.Inventory[0].TouchesUntilStocktake);

			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 10;
			AssertEquals("TouchesUntilStocktake Should equal 10.", 10, receive1.Inventory[0].TouchesUntilStocktake);

			location.WLV_FinalisedPickCount = 5;
			AssertEquals("TouchesUntilStocktake Should equal 5.", 5, receive1.Inventory[0].TouchesUntilStocktake);
		}

		#endregion

		#region TestOriginalInventoryStatus

		public void TestOriginalInventoryStatus()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var inventory = receiveLine.Inventory[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory.OriginalInventoryStatus);

			TestSetsValueToDocketLine(inventory, WhsDocketLineSchema.WE_OriginalInventoryStatus.Name, WhsInventoryView.Schema.OriginalInventoryStatus, (ZString)InventoryStatus.Codes.Held, (ZString)InventoryStatus.Codes.Putaway);
		}

		#endregion

		#region TestWI_InventoryStatus

		public void TestWI_InventoryStatus()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var inventory = receiveLine.Inventory[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory.WI_InventoryStatus);

			TestSetsValueToDocketLine(inventory, WhsDocketLineSchema.WE_CurrentInventoryStatus.Name, WhsInventoryViewSchema.WI_InventoryStatus.Name, (ZString)InventoryStatus.Codes.Held, (ZString)InventoryStatus.Codes.Putaway);
		}

		#endregion

		#region TestSetsValueToDocketLine

		void TestSetsValueToDocketLine(WhsInventoryView inventory, ZString docketLineColumn, ZString inventorySchema, IZType value1, IZType value2)
		{
			var docketLine = inventory.InDocketLine;
			if (docketLine == null)
			{
				docketLine = Factory.New<WhsReceiveLine>();
				inventory.WI_WE_InDocketLine = docketLine.PK;
			}
			inventory[inventorySchema] = value1;
			AssertEquals("Setting property on inventory view should populate docket line as well.", value1, docketLine[docketLineColumn]);

			docketLine[docketLineColumn] = value2;
			AssertEquals("Setting property on docket line should populate inventory as well.", value2, inventory[inventorySchema]);

			TestSetValueToInventoryViewWithSpace(docketLine, inventory, docketLineColumn, inventorySchema, value1);
		}

		static void TestSetValueToInventoryViewWithSpace(WhsDocketLine docketLine, WhsInventoryView inventory, ZString docketLineColumn, ZString inventoryViewColumn, IZType value1)
		{
			// they have local variable to store data
			// in setter of base class trim the value
			var exceptZStringColumns = new[] { WhsDocketLine.Schema.CommodityCode, WhsDocketLine.Schema.ProductDesc, WhsDocketLine.Schema.ProductUQ, WhsDocketLine.Schema.ProductCode };

			// check for trailing space
			if (value1 is ZString && !exceptZStringColumns.Any(c => c == docketLineColumn))
			{
				var value = value1.ToString();
				ZString valueWithSpace = value.Remove(value.Length - 1) + " "; // in case if properti has max length like inventory status
				docketLine[docketLineColumn] = valueWithSpace;
				AssertEquals("It will remove the space in setter.", valueWithSpace.TrimEndSpaceTab(), docketLine[docketLineColumn]);
				AssertEquals("Setting property on docket line should populate inventory as well.", docketLine[docketLineColumn], inventory[inventoryViewColumn]);

				ZString valueWithTab = valueWithSpace = value.Remove(value.Length - 1) + "\t";
				docketLine[docketLineColumn] = valueWithSpace;
				AssertEquals("It will remove the tab in setter.", valueWithTab.TrimEndSpaceTab(), docketLine[docketLineColumn]);
				AssertEquals("Setting property on docket line should populate inventory as well.", docketLine[docketLineColumn], inventory[inventoryViewColumn]);
			}
		}

		#endregion

		#region Implementation

		void PartAttributesTestingCore(AttributeNumber attribNumber)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			var attribInfo = GetInventoryAttributeInfo(data.Line111, attribNumber);
			var value = GetValueToSetForAttributes(attribNumber);

			Helper.SetClientAttributeType(data.Org1, attribNumber, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attribNumber, true);
			attribInfo.Value = value;
			TestStandardReadOnly(attribInfo);

			attribInfo.Value = value; // needed to refresh Readonly property
			AssertEquals(attribInfo.Description, false, attribInfo.ReadOnly);

			attribInfo.ClearValue();
			AssertEquals(attribInfo.Description, false, attribInfo.ReadOnly);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, attribNumber, false);
			attribInfo.Value = value;
			AssertEquals(attribInfo.Description, false, attribInfo.ReadOnly);

			attribInfo.ClearValue();
			AssertEquals(attribInfo.Description, true, attribInfo.ReadOnly);

			// For Temp Products
			data.Line111.WI_OP = ZGuid.Invalid;
			data.Line111.WI_OP_PartNum = "NewProduct";
			AssertEquals(attribInfo.Description, false, attribInfo.ReadOnly);

			data.Line111.WI_OH_Client = ZGuid.Empty;
			attribInfo.Value = value; // needed to refresh ReadOnly porperty
			AssertEquals(attribInfo.Description, true, attribInfo.ReadOnly);
		}

		ZPropertyInfo GetInventoryAttributeInfo(WhsInventoryView line, AttributeNumber attribNumber)
		{
			switch (attribNumber)
			{
				case AttributeNumber.One:
					return line.WI_PartAttrib1Info;
				case AttributeNumber.Two:
					return line.WI_PartAttrib2Info;
				case AttributeNumber.Three:
					return line.WI_PartAttrib3Info;
				case AttributeNumber.Serial:
					return line.WI_SerialNumberInfo;
				case AttributeNumber.ExpiryDate:
					return line.WI_ExpiryDateInfo;
				default:
					return line.WI_PackingDateInfo;
			}
		}

		IZType GetValueToSetForAttributes(AttributeNumber attribNumber)
		{
			switch (attribNumber)
			{
				case AttributeNumber.One:
				case AttributeNumber.Two:
				case AttributeNumber.Three:
					return (ZString)("PA" + attribNumber.ToString());
				case AttributeNumber.Serial:
					return (ZString)"SN1";
				default:
					return ZDate.Today;
			}
		}

		#endregion

		#endregion

		#region TestOperationalActionsFieldVisibility

		public void TestOperationalActionsFieldVisibility()
		{
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_ArrivalDate.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_BondedEntryKey.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_AllocationKey.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_ExpiryDate.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_F3_NKPackType.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_InDocketLineType.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_InDocketLineUnits.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_InventoryStatus.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_IsOriginalReceiptLine.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_OH_Client.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_PalletID.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_PartAttrib1.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_PartAttrib2.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_PartAttrib3.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsInventoryView).GetProperty(WhsInventoryViewSchema.WI_SerialNumber.Name)).ReadOnly);
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsInventoryView).GetProperty(WhsInventoryView.Schema.CurrentHeldCode)));
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsInventoryView).GetProperty(WhsInventoryView.Schema.OriginalHeldCode)));
		}

		#endregion

		#region Find Attributes

		public void TestUseChosenInventoryRow()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			data.Line111.WI_PartAttrib1 = "PA1";
			data.Line111.WI_CustomAttrib_2 = "CA2";
			data.Line111.WI_WL = locations[0].PK;

			Inventory.UseChosenInventoryRow(data.Line111);

			AssertEquals("Product: ", data.Line111.WI_OP, Inventory.WI_OP);
			AssertEquals("Part Attribute 1: ", "PA1", Inventory.WI_PartAttrib1);
			AssertEquals("Custom Attribute 2: ", "CA2", Inventory.WI_CustomAttrib_2);
			AssertEquals("Location: ", locations[0].PK, Inventory.WI_WL);

			data.Line111.WI_WL = locations[1].PK;
			Inventory.UseChosenInventoryRow(data.Line111);
			AssertEquals("Location shouldn't have changed as it is not Empty", locations[0].PK, Inventory.WI_WL);
		}

		public void TestUseChosenInventoryRowErrorNotifications_FinalisedStatus()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			data.Receive11.RunPreSaveValidation();

			((NotificationBuffer)data.Receive11.NotificationManager.Peek).Clear();
			data.Receive11.WD_FinalisedDate = ZDateTimeOffset.Now;
			data.Receive11.WD_DocketStatus = DocketStatus.Codes.Finalised;
			data.Line111.UseChosenInventoryRow(Factory.New<WhsInventoryView>());
			AssertEquals(true, ((NotificationBuffer)data.Receive11.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		public void TestUseChosenInventoryRowErrorNotifications_CancelledStatus()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			data.Receive11.RunPreSaveValidation();
			data.Receive11.WD_DocketStatus = DocketStatus.Codes.Entered;

			((NotificationBuffer)data.Receive11.NotificationManager.Peek).Clear();
			data.Receive11.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			data.Line111.UseChosenInventoryRow(Factory.New<WhsInventoryView>());
			AssertEquals(true, ((NotificationBuffer)data.Receive11.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		public void TestUseChosenInventoryRowErrorNotifications_EnteredStatus()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			data.Receive11.RunPreSaveValidation();

			((NotificationBuffer)data.Receive11.NotificationManager.Peek).Clear();
			data.Receive11.WD_DocketStatus = DocketStatus.Codes.Entered;
			data.Line111.UseChosenInventoryRow(Factory.New<WhsInventoryView>());
			AssertEquals(false, ((NotificationBuffer)data.Receive11.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestUseChosenInventoryRowHandlesNullInventory()
		{
			Inventory.UseChosenInventoryRow(null);
		}

		#endregion

		#region ReadOnly Control

		#region TestReadOnly_WhenParentReceiveCreatedFromWorkOrder

		public void TestReadOnly_WhenParentReceiveCreatedFromWorkOrder()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();

			AssertEquals("Precondition - " + inv.WI_PartAttrib1Info.Name, true, inv.WI_PartAttrib1Info.ReadOnly);
			AssertEquals("Precondition - " + inv.WI_PartAttrib2Info.Name, true, inv.WI_PartAttrib2Info.ReadOnly);
			AssertEquals("Precondition - " + inv.WI_PartAttrib3Info.Name, true, inv.WI_PartAttrib3Info.ReadOnly);
			AssertEquals("Precondition - " + inv.WI_ExpiryDateInfo.Name, true, inv.WI_ExpiryDateInfo.ReadOnly);
			AssertEquals("Precondition - " + inv.WI_PackingDateInfo.Name, true, inv.WI_PackingDateInfo.ReadOnly);

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(data.Org1, attribNo, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, attribNo, true);
			}

			receive.WD_WD_ParentDocket = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1).PK;

			AssertEquals(inv.WI_PartAttrib1Info.Name, false, inv.WI_PartAttrib1Info.ReadOnly);
			AssertEquals(inv.WI_PartAttrib2Info.Name, false, inv.WI_PartAttrib2Info.ReadOnly);
			AssertEquals(inv.WI_PartAttrib3Info.Name, false, inv.WI_PartAttrib3Info.ReadOnly);
			AssertEquals(inv.WI_ExpiryDateInfo.Name, false, inv.WI_ExpiryDateInfo.ReadOnly);
			AssertEquals(inv.WI_PackingDateInfo.Name, false, inv.WI_PackingDateInfo.ReadOnly);
		}

		#endregion

		#region TestStandardReadOnly

		protected void TestStandardReadOnly(ZPropertyInfo info)
		{
			TestReadOnly(info, false, false, true, true);
		}

		#endregion

		#region TestTempProductReadOnly

		protected void TestTempProductReadOnly(ZPropertyInfo info)
		{
			var inventory = (WhsInventoryView)info.BizObj;

			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var part = Helper.CreateProduct(data.Org1, "ReadOnlyTestProduct");

			// Save Old Values to restore Test data to its previous state
			ZGuid oldDocketPK = inventory.WI_WD;
			ZGuid oldProductPK = inventory.WI_OP;
			ZString oldProductCode = inventory.WI_OP_PartNum;

			inventory.WI_WD = receive.PK;
			inventory.WI_OP = ZGuid.Empty;
			receive.Inventory.Add(inventory);
			receive.RunPreSaveValidation();
			TestReadOnly(info, true, true, true, true);

			inventory.WI_OP = part.PK;
			TestReadOnly(info, true, true, true, true);

			inventory.WI_OP = ZGuid.Invalid;
			inventory.WI_OP_PartNum = "NewProduct";
			TestStandardReadOnly(info);

			// Restore Test data to its previous state
			inventory.WI_WD = oldDocketPK;
			inventory.WI_OP = oldProductPK;
			inventory.WI_OP_PartNum = oldProductCode;
		}

		#endregion

		#region TestReadOnly

		protected void TestReadOnly(ZPropertyInfo info, bool whenNew, bool whenEntered, bool whenFinalised, bool whenCancelled)
		{
			var inventory = (WhsInventoryView)info.BizObj;
			var receive = (WhsReceive)inventory.Docket;
			if (inventory.WI_WE_InDocketLine.IsEmpty)
			{
				receive.RunPreSaveValidation();
			}

			var status = receive.WD_DocketStatus;

			receive.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals(info.Name + " should " + (whenNew ? "" : "not ") + "be readonly if docket is New", whenNew, inventory.ZPropertyInfoHash[info.Name].ReadOnly);
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(info.Name + " should " + (whenEntered ? "" : "not ") + "be readonly if docket is Entered", whenEntered, inventory.ZPropertyInfoHash[info.Name].ReadOnly);
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(info.Name + " should " + (whenCancelled ? "" : "not ") + "be readonly if docket is Cancelled", whenCancelled, inventory.ZPropertyInfoHash[info.Name].ReadOnly);
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(info.Name + " should " + (whenFinalised ? "" : "not ") + "be readonly if docket is Finalised", whenFinalised, inventory.ZPropertyInfoHash[info.Name].ReadOnly);
			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;

			receive.WD_DocketStatus = status;
		}

		#endregion

		#region TestReadOnlyOnAllProperties

		public void TestReadOnlyOnAllProperties()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "A");
			var receive = Helper.CreateWhsReceive(org, whs, Notify);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, part, 10);
			receive.AllocateLocationsWithMock();

			AssertEquals(inv.LocationStringInfo.Name, false, inv.LocationStringInfo.ReadOnly);
			AssertEquals(inv.WI_PalletIDInfo.Name, false, inv.WI_PalletIDInfo.ReadOnly);
			AssertEquals(inv.WI_ArrivalDateInfo.Name, false, inv.WI_ArrivalDateInfo.ReadOnly);
			AssertEquals(inv.WI_InDocketLineUnitsInfo.Name, false, inv.WI_InDocketLineUnitsInfo.ReadOnly);
			AssertEquals(inv.WI_ExpectedReceiptQuantityInfo.Name, false, inv.WI_ExpectedReceiptQuantityInfo.ReadOnly);
			AssertEquals(inv.WI_InventoryStatusInfo.Name, false, inv.WI_InventoryStatusInfo.ReadOnly);
			AssertEquals(inv.WI_OH_ClientInfo.Name, false, inv.WI_OH_ClientInfo.ReadOnly);
			AssertEquals(inv.WI_TotalUnitsInfo.Name, true, inv.WI_TotalUnitsInfo.ReadOnly);
			AssertEquals(inv.WI_F3_NKPackTypeInfo.Name, false, inv.WI_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals(inv.WI_BondedEntryKeyInfo.Name, false, inv.WI_BondedEntryKeyInfo.ReadOnly);
			AssertEquals(inv.WI_PartAttrib1Info.Name, true, inv.WI_PartAttrib1Info.ReadOnly);
			AssertEquals(inv.WI_PartAttrib2Info.Name, true, inv.WI_PartAttrib2Info.ReadOnly);
			AssertEquals(inv.WI_PartAttrib3Info.Name, true, inv.WI_PartAttrib3Info.ReadOnly);
			AssertEquals(inv.WI_ExpiryDateInfo.Name, true, inv.WI_ExpiryDateInfo.ReadOnly);
			AssertEquals(inv.WI_PackingDateInfo.Name, true, inv.WI_PackingDateInfo.ReadOnly);
			AssertEquals(inv.WI_PalletIDInfo.Name, false, inv.WI_PalletIDInfo.ReadOnly);
			AssertEquals(inv.WI_LineNoInfo.Name, false, inv.WI_LineNoInfo.ReadOnly);
			AssertEquals(inv.WI_SubLineNoInfo.Name, false, inv.WI_SubLineNoInfo.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib_1Info.Name, false, inv.WI_CustomAttrib_1Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib_2Info.Name, false, inv.WI_CustomAttrib_2Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib_3Info.Name, false, inv.WI_CustomAttrib_3Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib4Info.Name, false, inv.WI_CustomAttrib4Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib5Info.Name, false, inv.WI_CustomAttrib5Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib6Info.Name, false, inv.WI_CustomAttrib6Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal1Info.Name, false, inv.WI_CustomDecimal1Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal2Info.Name, false, inv.WI_CustomDecimal2Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal3Info.Name, false, inv.WI_CustomDecimal3Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal4Info.Name, false, inv.WI_CustomDecimal4Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal5Info.Name, false, inv.WI_CustomDecimal5Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate1Info.Name, false, inv.WI_CustomDate1Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate2Info.Name, false, inv.WI_CustomDate2Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate3Info.Name, false, inv.WI_CustomDate3Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate4Info.Name, false, inv.WI_CustomDate4Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate5Info.Name, false, inv.WI_CustomDate5Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag1Info.Name, false, inv.WI_CustomFlag1Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag2Info.Name, false, inv.WI_CustomFlag2Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag3Info.Name, false, inv.WI_CustomFlag3Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag4Info.Name, false, inv.WI_CustomFlag4Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag5Info.Name, false, inv.WI_CustomFlag5Info.ReadOnly);
			AssertEquals(inv.WI_CustomTextBlob1Info.Name, false, inv.WI_CustomTextBlob1Info.ReadOnly);
			AssertEquals(inv.WI_TotalValueInfo.Name, true, inv.WI_TotalValueInfo.ReadOnly);
			AssertEquals(inv.WI_CurrencyInfo.Name, true, inv.WI_CurrencyInfo.ReadOnly);

			inv.IsInventoryEditForm = true;

			AssertEquals(inv.WI_ArrivalDateInfo.Name, true, inv.WI_ArrivalDateInfo.ReadOnly);
			AssertEquals(inv.WI_InDocketLineUnitsInfo.Name, true, inv.WI_InDocketLineUnitsInfo.ReadOnly);
			AssertEquals(inv.WI_ExpectedReceiptQuantityInfo.Name, true, inv.WI_ExpectedReceiptQuantityInfo.ReadOnly);
			AssertEquals(inv.WI_InventoryStatusInfo.Name, true, inv.WI_InventoryStatusInfo.ReadOnly);
			AssertEquals(inv.WI_OH_ClientInfo.Name, true, inv.WI_OH_ClientInfo.ReadOnly);
			AssertEquals(inv.WI_TotalUnitsInfo.Name, true, inv.WI_TotalUnitsInfo.ReadOnly);
			AssertEquals(inv.LocationStringInfo.Name, true, inv.LocationStringInfo.ReadOnly);
			AssertEquals(inv.WI_F3_NKPackTypeInfo.Name, true, inv.WI_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals(inv.WI_PalletIDInfo.Name, true, inv.WI_PalletIDInfo.ReadOnly);
			AssertEquals(inv.WI_BondedEntryKeyInfo.Name, true, inv.WI_BondedEntryKeyInfo.ReadOnly);
			AssertEquals(inv.WI_PartAttrib1Info.Name, true, inv.WI_PartAttrib1Info.ReadOnly);
			AssertEquals(inv.WI_PartAttrib2Info.Name, true, inv.WI_PartAttrib2Info.ReadOnly);
			AssertEquals(inv.WI_PartAttrib3Info.Name, true, inv.WI_PartAttrib3Info.ReadOnly);
			AssertEquals(inv.WI_ExpiryDateInfo.Name, true, inv.WI_ExpiryDateInfo.ReadOnly);
			AssertEquals(inv.WI_PackingDateInfo.Name, true, inv.WI_PackingDateInfo.ReadOnly);
			AssertEquals(inv.WI_PalletIDInfo.Name, true, inv.WI_PalletIDInfo.ReadOnly);
			AssertEquals(inv.WI_LineNoInfo.Name, true, inv.WI_LineNoInfo.ReadOnly);
			AssertEquals(inv.WI_SubLineNoInfo.Name, true, inv.WI_SubLineNoInfo.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib_1Info.Name, true, inv.WI_CustomAttrib_1Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib_2Info.Name, true, inv.WI_CustomAttrib_2Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib_3Info.Name, true, inv.WI_CustomAttrib_3Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib4Info.Name, true, inv.WI_CustomAttrib4Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib5Info.Name, true, inv.WI_CustomAttrib5Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib6Info.Name, true, inv.WI_CustomAttrib6Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal1Info.Name, true, inv.WI_CustomDecimal1Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal2Info.Name, true, inv.WI_CustomDecimal2Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal3Info.Name, true, inv.WI_CustomDecimal3Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal4Info.Name, true, inv.WI_CustomDecimal4Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal5Info.Name, true, inv.WI_CustomDecimal5Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate1Info.Name, true, inv.WI_CustomDate1Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate2Info.Name, true, inv.WI_CustomDate2Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate3Info.Name, true, inv.WI_CustomDate3Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate4Info.Name, true, inv.WI_CustomDate4Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate5Info.Name, true, inv.WI_CustomDate5Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag1Info.Name, true, inv.WI_CustomFlag1Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag2Info.Name, true, inv.WI_CustomFlag2Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag3Info.Name, true, inv.WI_CustomFlag3Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag4Info.Name, true, inv.WI_CustomFlag4Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag5Info.Name, true, inv.WI_CustomFlag5Info.ReadOnly);
			AssertEquals(inv.WI_CustomTextBlob1Info.Name, true, inv.WI_CustomTextBlob1Info.ReadOnly);
			AssertEquals(inv.WI_TotalValueInfo.Name, true, inv.WI_TotalValueInfo.ReadOnly);
			AssertEquals(inv.WI_CurrencyInfo.Name, true, inv.WI_CurrencyInfo.ReadOnly);

			inv.IsInventoryEditForm = false;
			AssertEquals(inv.LocationStringInfo.Name, false, inv.LocationStringInfo.ReadOnly);

			receive.FinaliseDocket();

			AssertEquals(inv.WI_ArrivalDateInfo.Name, true, inv.WI_ArrivalDateInfo.ReadOnly);
			AssertEquals(inv.WI_InDocketLineUnitsInfo.Name, true, inv.WI_InDocketLineUnitsInfo.ReadOnly);
			AssertEquals(inv.WI_ExpectedReceiptQuantityInfo.Name, true, inv.WI_ExpectedReceiptQuantityInfo.ReadOnly);
			AssertEquals(inv.WI_InventoryStatusInfo.Name, true, inv.WI_InventoryStatusInfo.ReadOnly);
			AssertEquals(inv.WI_OH_ClientInfo.Name, true, inv.WI_OH_ClientInfo.ReadOnly);
			AssertEquals(inv.WI_TotalUnitsInfo.Name, true, inv.WI_TotalUnitsInfo.ReadOnly);
			AssertEquals(inv.LocationStringInfo.Name, true, inv.LocationStringInfo.ReadOnly);
			AssertEquals(inv.WI_F3_NKPackTypeInfo.Name, true, inv.WI_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals(inv.WI_PalletIDInfo.Name, true, inv.WI_PalletIDInfo.ReadOnly);
			AssertEquals(inv.WI_BondedEntryKeyInfo.Name, true, inv.WI_BondedEntryKeyInfo.ReadOnly);
			AssertEquals(inv.WI_PartAttrib1Info.Name, true, inv.WI_PartAttrib1Info.ReadOnly);
			AssertEquals(inv.WI_PartAttrib2Info.Name, true, inv.WI_PartAttrib2Info.ReadOnly);
			AssertEquals(inv.WI_PartAttrib3Info.Name, true, inv.WI_PartAttrib3Info.ReadOnly);
			AssertEquals(inv.WI_ExpiryDateInfo.Name, true, inv.WI_ExpiryDateInfo.ReadOnly);
			AssertEquals(inv.WI_PackingDateInfo.Name, true, inv.WI_PackingDateInfo.ReadOnly);
			AssertEquals(inv.WI_PalletIDInfo.Name, true, inv.WI_PalletIDInfo.ReadOnly);
			AssertEquals(inv.WI_LineNoInfo.Name, true, inv.WI_LineNoInfo.ReadOnly);
			AssertEquals(inv.WI_SubLineNoInfo.Name, true, inv.WI_SubLineNoInfo.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib_1Info.Name, true, inv.WI_CustomAttrib_1Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib_2Info.Name, true, inv.WI_CustomAttrib_2Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib_3Info.Name, true, inv.WI_CustomAttrib_3Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib4Info.Name, true, inv.WI_CustomAttrib4Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib5Info.Name, true, inv.WI_CustomAttrib5Info.ReadOnly);
			AssertEquals(inv.WI_CustomAttrib6Info.Name, true, inv.WI_CustomAttrib6Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal1Info.Name, true, inv.WI_CustomDecimal1Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal2Info.Name, true, inv.WI_CustomDecimal2Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal3Info.Name, true, inv.WI_CustomDecimal3Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal4Info.Name, true, inv.WI_CustomDecimal4Info.ReadOnly);
			AssertEquals(inv.WI_CustomDecimal5Info.Name, true, inv.WI_CustomDecimal5Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate1Info.Name, true, inv.WI_CustomDate1Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate2Info.Name, true, inv.WI_CustomDate2Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate3Info.Name, true, inv.WI_CustomDate3Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate4Info.Name, true, inv.WI_CustomDate4Info.ReadOnly);
			AssertEquals(inv.WI_CustomDate5Info.Name, true, inv.WI_CustomDate5Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag1Info.Name, true, inv.WI_CustomFlag1Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag2Info.Name, true, inv.WI_CustomFlag2Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag3Info.Name, true, inv.WI_CustomFlag3Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag4Info.Name, true, inv.WI_CustomFlag4Info.ReadOnly);
			AssertEquals(inv.WI_CustomFlag5Info.Name, true, inv.WI_CustomFlag5Info.ReadOnly);
			AssertEquals(inv.WI_CustomTextBlob1Info.Name, true, inv.WI_CustomTextBlob1Info.ReadOnly);
			AssertEquals(inv.WI_TotalValueInfo.Name, true, inv.WI_TotalValueInfo.ReadOnly);
			AssertEquals(inv.WI_CurrencyInfo.Name, true, inv.WI_CurrencyInfo.ReadOnly);

			inv.IsInventoryEditForm = true;
			AssertEquals(inv.WI_InventoryStatusInfo.Name, true, inv.WI_InventoryStatusInfo.ReadOnly);
		}

		#endregion

		#region TestReadOnly_AttributeReadOnly

		public void TestReadOnly_AttributeReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 5m, transferLine1.QtyCommittedIncludingMatchingLines);
			AssertEquals("Precondition: Stock is committed.", 5m, transferLine2.QtyCommittedIncludingMatchingLines);

			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);

			var inventory = transferLine1.Inventory[0];
			Factory.Save();
			AssertEquals("Pallet ID should be Read Only if Transfer Line is Finalised.", true, inventory.WI_PalletIDInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region Split Methods

		public void TestSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var testLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);

			bool isValidationSuspendedDuringSplit = false;
			testLine.WI_InDocketLineUnitsInfo.ValueChanged += (sender, e) => isValidationSuspendedDuringSplit = testLine.IsValidationSuspended;

			var resultLine = testLine.Split(2m);
			AssertEquals(2m, resultLine.WI_InDocketLineUnits);
			AssertEquals(3m, testLine.WI_InDocketLineUnits);
			AssertEquals("Validation should be suspended when splitting Inventory.", true, isValidationSuspendedDuringSplit);
		}

		public void TestSplit_WithArgumentException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var testLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			AssertExceptionThrown(typeof(ArgumentException), "Should not be splitting Inventory by an amount greater than or equal to its Quantity.", () => testLine.Split(5m));
			AssertExceptionThrown(typeof(ArgumentException), "Should not be splitting Inventory by an amount greater than or equal to its Quantity.", () => testLine.Split(6m));
		}

		#endregion

		#region TestGetDocketLineZDBOnlySubQuery

		public void TestGetDocketLineZDBOnlySubQuery()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			query.AddSubQuery(WhsInventoryView.GetDocketLineZDBOnlySubQuery(new[] { pick.GetAllPickLines().Single().WZ_WE_TransactionLine }), JoinCondition.And);

			AssertEquals(order.PK, Factory.Load<WhsOrder>(query).Single().PK);
		}

		public void TestGetDocketLineZDBOnlySubQuery_AllowTableValuedParameters()
		{
			AssertEquals("AllowTableValuedParameters always should be true", true, WhsInventoryView.GetDocketLineZDBOnlySubQuery(Array.Empty<ZGuid>()).AllowTableValuedParameters);
			AssertEquals("AllowTableValuedParameters always should be true", true, WhsInventoryView.GetDocketLineZDBOnlySubQuery(new[] { ZGuid.NewZGuid() }).AllowTableValuedParameters);
		}

		#endregion

		// interfaces

		#region ICodeDescription - FindBox

		public void TestICodeDescriptionCode()
		{
			ICodeDescription codeDescPair = Inventory;
			AssertEquals("Code incorrect", "!FINDBOX!" + Inventory.PK.ToString(), codeDescPair.Code);
		}

		public void TestICodeDescriptionDescription()
		{
			ICodeDescription codeDescPair = Inventory;
			AssertEquals("Desc incorrect", "UNUSED", codeDescPair.Description);
		}

		#endregion

		#region ILineAttributes Members

		public void TestILineAttributesExpiryDate()
		{
			var date = ZDate.Today.AddDays(10);
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_ExpiryDate = date;
			AssertEquals(date, ((ILineAttributes)inventory).ExpiryDate);
		}

		public void TestILineAttributesPackingDate()
		{
			var date = ZDate.Today.AddDays(-10);
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_PackingDate = date;
			AssertEquals(date, ((ILineAttributes)inventory).PackingDate);
		}

		public void TestILineAttributesBondedEntryKey()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_BondedEntryKey = "BEK-1";
			AssertEquals("BEK-1", ((ILineAttributes)inventory).BondedEntryKey);
		}

		public void TestILineAttributesPartAttrib1()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_PartAttrib1 = "PA1";
			AssertEquals("PA1", ((ILineAttributes)inventory).PartAttrib1);
		}

		public void TestILineAttributesPartAttrib2()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_PartAttrib2 = "PA2";
			AssertEquals("PA2", ((ILineAttributes)inventory).PartAttrib2);
		}

		public void TestILineAttributesPartAttrib3()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_PartAttrib3 = "PA3";
			AssertEquals("PA3", ((ILineAttributes)inventory).PartAttrib3);
		}

		public void TestILineAttributesSerialNumber()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_SerialNumber = "SLN";
			AssertEquals("SLN", ((ILineAttributes)inventory).SerialNumber);
		}

		public void TestILineAttributesAllocationKey()
		{
			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_AllocationKey = "ALO";
			AssertEquals("ALO", ((ILineAttributes)inventory).AllocationKey);
		}

		public void TestILineAttributesSetAttributes()
		{
			var expiry = ZDate.Today.AddDays(1);
			var packing = ZDate.Today.AddDays(-1);

			var inventory = (WhsInventoryView)GetNewBusinessObject();
			inventory.WI_ExpiryDate = expiry;
			inventory.WI_PackingDate = packing;
			inventory.WI_BondedEntryKey = "BEK-1";
			inventory.WI_PartAttrib1 = "PA1";
			inventory.WI_PartAttrib2 = "PA2";
			inventory.WI_PartAttrib3 = "PA3";
			inventory.WI_SerialNumber = "SLN";
			inventory.WI_AllocationKey = "ALO";

			var inventory1 = (WhsInventoryView)GetNewBusinessObject();
			inventory1.SetAttributes(inventory);
			AssertEquals(true, AttributeComparer.Compare(inventory1, inventory));
		}

		#endregion

		#region ILineCustomAttributes Members

		public void TestICustomLineAttributesCustomAttrib1()
		{
			Inventory.WI_CustomAttrib_1 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)Inventory).CustomAttrib1);
		}

		public void TestICustomLineAttributesCustomAttrib2()
		{
			Inventory.WI_CustomAttrib_2 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)Inventory).CustomAttrib2);
		}

		public void TestICustomLineAttributesCustomAttrib3()
		{
			Inventory.WI_CustomAttrib_3 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)Inventory).CustomAttrib3);
		}

		public void TestICustomLineAttributesCustomAttrib4()
		{
			Inventory.WI_CustomAttrib4 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)Inventory).CustomAttrib4);
		}

		public void TestICustomLineAttributesCustomAttrib5()
		{
			Inventory.WI_CustomAttrib5 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)Inventory).CustomAttrib5);
		}

		public void TestICustomLineAttributesCustomAttrib6()
		{
			Inventory.WI_CustomAttrib6 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)Inventory).CustomAttrib6);
		}

		public void TestICustomLineAttributesCustomDecimal1()
		{
			Inventory.WI_CustomDecimal1 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)Inventory).CustomDecimal1);
		}

		public void TestICustomLineAttributesCustomDecimal2()
		{
			Inventory.WI_CustomDecimal2 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)Inventory).CustomDecimal2);
		}

		public void TestICustomLineAttributesCustomDecimal3()
		{
			Inventory.WI_CustomDecimal3 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)Inventory).CustomDecimal3);
		}

		public void TestICustomLineAttributesCustomDecimal4()
		{
			Inventory.WI_CustomDecimal4 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)Inventory).CustomDecimal4);
		}

		public void TestICustomLineAttributesCustomDecimal5()
		{
			Inventory.WI_CustomDecimal5 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)Inventory).CustomDecimal5);
		}

		public void TestILineCustomAttributesCustomDate1()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			Inventory.WI_CustomDate1 = date;
			AssertEquals(date, ((ILineCustomAttributes)Inventory).CustomDate1);
		}

		public void TestILineCustomAttributesCustomDate2()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			Inventory.WI_CustomDate2 = date;
			AssertEquals(date, ((ILineCustomAttributes)Inventory).CustomDate2);
		}

		public void TestILineCustomAttributesCustomDate3()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			Inventory.WI_CustomDate3 = date;
			AssertEquals(date, ((ILineCustomAttributes)Inventory).CustomDate3);
		}

		public void TestILineCustomAttributesCustomDate4()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			Inventory.WI_CustomDate4 = date;
			AssertEquals(date, ((ILineCustomAttributes)Inventory).CustomDate4);
		}

		public void TestILineCustomAttributesCustomDate5()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			Inventory.WI_CustomDate5 = date;
			AssertEquals(date, ((ILineCustomAttributes)Inventory).CustomDate5);
		}

		public void TestILineCustomAttributesCustomFlag1()
		{
			Inventory.WI_CustomFlag1 = true;
			AssertEquals(true, ((ILineCustomAttributes)Inventory).CustomFlag1);
		}

		public void TestILineCustomAttributesCustomFlag2()
		{
			Inventory.WI_CustomFlag2 = true;
			AssertEquals(true, ((ILineCustomAttributes)Inventory).CustomFlag2);
		}

		public void TestILineCustomAttributesCustomFlag3()
		{
			Inventory.WI_CustomFlag3 = true;
			AssertEquals(true, ((ILineCustomAttributes)Inventory).CustomFlag3);
		}

		public void TestILineCustomAttributesCustomFlag4()
		{
			Inventory.WI_CustomFlag4 = true;
			AssertEquals(true, ((ILineCustomAttributes)Inventory).CustomFlag4);
		}

		public void TestILineCustomAttributesCustomFlag5()
		{
			Inventory.WI_CustomFlag5 = true;
			AssertEquals(true, ((ILineCustomAttributes)Inventory).CustomFlag5);
		}

		public void TestILineCustomAttributesCustomTextBlob1()
		{
			Inventory.WI_CustomTextBlob1 = "TEXTBLOB";
			AssertEquals("TEXTBLOB", ((ILineCustomAttributes)Inventory).CustomTextBlob1);
		}

		public void TestILineCustomAttributesSetCustomAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var date1 = ZDateTime.Today.AddDays(1);
			var date2 = ZDateTime.Today.AddDays(2);
			var date3 = ZDateTime.Today.AddDays(3);
			var date4 = ZDateTime.Today.AddDays(4);
			var date5 = ZDateTime.Today.AddDays(5);

			inventory1.WI_CustomAttrib_1 = "CA1";
			inventory1.WI_CustomAttrib_2 = "CA2";
			inventory1.WI_CustomAttrib_3 = "CA3";
			inventory1.WI_CustomAttrib4 = "CA4";
			inventory1.WI_CustomAttrib5 = "CA5";
			inventory1.WI_CustomAttrib6 = "CA6";

			inventory1.WI_CustomDecimal1 = 1.1m;
			inventory1.WI_CustomDecimal2 = 2.2m;
			inventory1.WI_CustomDecimal3 = 3.3m;
			inventory1.WI_CustomDecimal4 = 4.4m;
			inventory1.WI_CustomDecimal5 = 5.5m;

			inventory1.WI_CustomDate1 = date1;
			inventory1.WI_CustomDate2 = date2;
			inventory1.WI_CustomDate3 = date3;
			inventory1.WI_CustomDate4 = date4;
			inventory1.WI_CustomDate5 = date5;

			inventory1.WI_CustomFlag1 = true;
			inventory1.WI_CustomFlag2 = true;
			inventory1.WI_CustomFlag3 = true;
			inventory1.WI_CustomFlag4 = true;
			inventory1.WI_CustomFlag5 = true;

			inventory1.WI_CustomTextBlob1 = "TEXTBLOB1";

			inventory2.SetCustomAttributes(inventory1);
			AssertEquals(true, new CustomAttributeComparer().Compare(inventory1, inventory2));
		}

		#endregion

		#region IPartAttributeValidationConsumer Members

		public void TestIsInventoryAdjustedOutOnSiblings()
		{
			AssertEquals(false, Inventory.IsInventoryAdjustedOutOnSiblings(Inventory));
		}

		public void TestIsRegisteredForUniqueSerialNumberChecking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory.WI_SerialNumber = "PA1";
			Factory.Save();

			AssertEquals("Precondition", 1m, inventory.WI_InDocketLineUnits);
			AssertEquals("Precondition", 1m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("Precondition", false, receive.StartedReceiving);
			AssertEquals("Precondition", true, inventory.IsRegisteredForUniqueSerialNumberChecking);

			inventory.WI_InDocketLineUnits = 0m;
			AssertEquals("IsRegisteredForUniqueSerialNumberChecking is true, expected quantity still has value.", true, inventory.IsRegisteredForUniqueSerialNumberChecking);

			inventory.WI_ExpectedReceiptQuantity = 0m;
			AssertEquals("IsRegisteredForUniqueSerialNumberChecking is false after transaction and expected quantity are cleared.", false, inventory.IsRegisteredForUniqueSerialNumberChecking);

			inventory.WI_InDocketLineUnits = 1m;
			inventory.WI_ExpectedReceiptQuantity = 1m;
			Factory.Save();
			receive.PopulateASNLines();

			AssertEquals("Precondition", true, receive.StartedReceiving);
			AssertEquals("Precondition", true, inventory.IsRegisteredForUniqueSerialNumberChecking);

			inventory.WI_InDocketLineUnits = 0m;
			AssertEquals("IsRegisteredForUniqueSerialNumberChecking is false after transaction quantity is cleared and receive has started receiving.", false, inventory.IsRegisteredForUniqueSerialNumberChecking);
		}

		public void TestIsValidForUniqueSerialNumberChecking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine.WE_SerialNumber = "PA1";
			var inventory = receiveLine.Inventory[0];

			AssertEquals("Should be validated", true, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, false);
			AssertEquals("Should not be validated - Serial # not used on product", false, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			AssertEquals("Should be validated - Confirmation", true, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));

			inventory.WI_OP = ZGuid.Empty;
			AssertEquals("Should not be validated - Product is null", false, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));
			inventory.WI_OP = data.Part1.PK;
			AssertEquals("Should be validated - Confirmation", true, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));

			var save1PK = inventory.WI_WD;
			var save2PK = inventory.WI_WE_InDocketLine;
			inventory.WI_WD = ZGuid.Empty;
			inventory.WI_WE_InDocketLine = ZGuid.Empty;
			AssertEquals("Should not be validated - Docket is null", false, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));
			inventory.WI_WE_InDocketLine = save2PK;
			inventory.WI_WD = save1PK;
			AssertEquals("Should be validated - Confirmation", true, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));

			inventory.WI_OH_Client = ZGuid.Empty;
			AssertEquals("Should not be validated - Client is null", false, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));
			inventory.WI_OH_Client = data.Org1.PK;
			AssertEquals("Should be validated - Confirmation", true, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));

			inventory.WI_SerialNumber = "";
			AssertEquals("Should not be validated - Value is empty", false, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));
			inventory.WI_SerialNumber = "PA1";
			AssertEquals("Should be validated - Confirmation", true, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals(false, inventory.IsValidForUniqueSerialNumberChecking(inventory.WI_SerialNumber));
		}

		public void TestIsSerialNumberUsedOnThis()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine.WE_SerialNumber = "SER";
			var inventory = receiveLine.Inventory[0];
			AssertEquals("IsSerialNumberUsedOnThis true serial number on", true, inventory.IsSerialNumberUsedOnThis("SER"));

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, false);
			AssertEquals("IsSerialNumberUsedOnThis false serial number off", false, inventory.IsSerialNumberUsedOnThis("SER"));
		}

		public void TestIsSerialNumberUsedOnSiblings()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "1PA1", "1PA2", "1PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "2PA1", "2PA2", "2PA3", "");
			inventory.WI_SerialNumber = "SN1";
			inventory1.WI_SerialNumber = "1SN1";
			inventory2.WI_SerialNumber = "2SN1";
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("PA1"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("PA2"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("PA3"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("SN1"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("1PA1"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("1PA2"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("1PA3"));
			AssertEquals(true, inventory.IsSerialNumberUsedOnSiblings("1SN1"));
			AssertEquals("Part Attributes cannot be serials.", false, inventory.IsSerialNumberUsedOnSiblings("2PA1"));
			AssertEquals("Part Attributes cannot be serials.", false, inventory.IsSerialNumberUsedOnSiblings("2PA2"));
			AssertEquals("Part Attributes cannot be serials.", false, inventory.IsSerialNumberUsedOnSiblings("2PA3"));
			AssertEquals("No match because product is different", false, inventory.IsSerialNumberUsedOnSiblings("2SN1"));

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI");

			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("PA1"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("PA2"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("PA3"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("SN1"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("1PA1"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("1PA2"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("1PA3"));
			AssertEquals(true, inventory.IsSerialNumberUsedOnSiblings("1SN1"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("2PA1"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("2PA2"));
			AssertEquals(false, inventory.IsSerialNumberUsedOnSiblings("2PA3"));
			AssertEquals(true, inventory.IsSerialNumberUsedOnSiblings("2SN1"));
		}

		#endregion

		#region Methods for Setting Attribute values

		protected void SetLineAttributes(WhsInventoryView line)
		{
			SetLineAttributes(line, ZDate.Today, ZDate.Today, "PA1", "PA2", "PA3", "SN1");
		}

		protected void SetLineAttributes(WhsInventoryView line, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serial)
		{
			line.WI_ExpiryDate = expiryDate;
			line.WI_PackingDate = packingDate;
			line.WI_PartAttrib1 = partAttrib1;
			line.WI_PartAttrib2 = partAttrib2;
			line.WI_PartAttrib3 = partAttrib3;
			line.WI_SerialNumber = serial;
		}

		#endregion

		#region Methods for Asserting Line Properties

		protected void AssertLineAttributes(WhsInventoryView line, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serial)
		{
			AssertEquals("Expiry Date incorrect", expiryDate, line.WI_ExpiryDate);
			AssertEquals("Packing Date incorrect", packingDate, line.WI_PackingDate);
			AssertEquals("Part Attrib1 incorrect", partAttrib1, line.WI_PartAttrib1);
			AssertEquals("Part Attrib2 incorrect", partAttrib2, line.WI_PartAttrib2);
			AssertEquals("Part Attrib3 incorrect", partAttrib3, line.WI_PartAttrib3);
			AssertEquals("Serial incorrect", serial, line.WI_SerialNumber);
		}

		#endregion

		#region IDocManagerSupport Members

		public void TestDocManagerInfo()
		{
			DocManagerInfo info = Inventory.DocManagerInfo;
			AssertEquals(Inventory, info.BusinessEntity);
			AssertEquals("WIN", info.DocManagerCode);
		}

		#endregion

		#region IParentDocManagerSupport Members

		public void TestIParentDocManagerSupport()
		{
			var parentDocSupport = (IParentDocManagerSupport)Inventory;
			AssertEquals("ParentGuid", Inventory.InDocketLine.PK, parentDocSupport.ParentGuid);
			AssertEquals("ParentTableName", WhsDocketLineSchema.Constants.TableName, parentDocSupport.ParentTableName);
		}

		#endregion

		#region IDocumentSupportable Members

		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(WhsInventoryDocumentSupporter), Inventory.DocumentSupporter.GetType());
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		public void TestICustomLabelsConfigOrgProviderConfigOrg()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var inventory = (WhsInventoryView)GetNewBusinessObject();

			inventory.Docket.WD_OH_Client = org1.PK;
			inventory.WI_OH_Client = org2.PK;
			AssertEquals("Should proxy custom label interface from the docket line.", org1, ((ICustomLabelsConfigOrgProvider)inventory).ConfigOrg);
		}

		public void TestICustomLabelsConfigOrgProviderConfigOrgChanged()
		{
			var configOrgChangedCalled = false;
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var inventory = (WhsInventoryView)GetNewBusinessObject();

			EventHandler action = (s, e) => configOrgChangedCalled = true;
			((ICustomLabelsConfigOrgProvider)inventory).ConfigOrgChanged += action;

			inventory.WI_OH_Client = org1.PK;
			AssertEquals("Should *not* fire org changed as we proxy custom label interface from the docket line.", false, configOrgChangedCalled);

			inventory.Docket.WD_OH_Client = org1.PK;
			AssertEquals("Should fire org changed as we proxy custom label interface from the docket line.", true, configOrgChangedCalled);

			configOrgChangedCalled = false;
			((ICustomLabelsConfigOrgProvider)inventory).ConfigOrgChanged -= action;
			inventory.Docket.WD_OH_Client = org2.PK;
			AssertEquals("Should have unhooked event handler.", false, configOrgChangedCalled);
		}

		#endregion

		#region ICustomLabelsProvider Members

		public void TestCustomLabelsProvider()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			Inventory.WI_OH_Client = org.PK;

			var provider = new WhsInventoryView.CustomLabelsProvider(Inventory);
			AssertEquals(Inventory, provider.ConfigOrgProvider);

			CustomLabelInfoList list = provider.GetCustomFields(org, Factory);
			AssertEquals("the client", list.ConfigOrgLocatedAt);

			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomAttrib1);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomAttrib2);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomAttrib3);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomAttrib4);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomAttrib5);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomAttrib6);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDate1);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDate2);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDate3);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDate4);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDate5);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDecimal1);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDecimal2);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDecimal3);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDecimal4);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomDecimal5);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomFlag1);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomFlag2);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomFlag3);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomFlag4);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomFlag5);
			AssertCustomLabelListContains(list, WhsInventoryView.Schema.WI_CustomTextBlob1);
		}

		protected void AssertCustomLabelListContains(CustomLabelInfoList list, string propertyName)
		{
			foreach (CustomLabelInfo info in list)
			{
				if (info.PropertyName == propertyName)
				{
					return;
				}
			}
			Fail("Could not find property: " + propertyName + " in CustomLabelInfoList");
		}

		#endregion

		#region IOrgSupplierPartCollectionDefaultsForNewChild Members

		public void TestIOrgSupplierPartCollectionDefaultsForNewChild_SetupSupplierPart()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			WhsArea area = Helper.CreateArea(data.Whs1, "AREA1", Environment.CodeLists.AreaTypes.Codes.FreeStore);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			// Do Not Setup Attribute 3
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			// Preconditions
			AssertEquals("P1", data.Part1.OP_PartNum);
			AssertEquals("", data.Part1.OP_RH_NKCommodityCode);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UseSerialNumber);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePackingDate);

			// When Called but data is empty
			data.Line111.SetupSupplierPart(data.Part1);
			AssertEquals("P1", data.Part1.OP_PartNum);
			AssertEquals("", data.Part1.OP_RH_NKCommodityCode);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UseSerialNumber);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePackingDate);
			AssertEquals(0, (WhsProduct.GetWhsProduct(data.Part1)).ParamsByWhsAndClient.Count);

			// Setup data
			data.Part1.OP_PartNum = ""; // clear to see if it will be setup.
			data.Line111.WI_OP = ZGuid.Invalid;
			data.Line111.WI_OP_PartNum = "NEWP1";
			data.Line111.CommodityCode = "CODE";
			data.Line111.WI_PartAttrib1 = "PA1";
			data.Line111.WI_PartAttrib2 = "PA2";
			data.Line111.WI_PartAttrib3 = "PA3";
			data.Line111.WI_SerialNumber = "SN1";
			data.Line111.WI_ExpiryDate = ZDate.Today;
			data.Line111.WI_PackingDate = ZDate.Today;

			// When Called but data is set
			data.Line111.SetupSupplierPart(data.Part1);
			AssertEquals("NEWP1", data.Part1.OP_PartNum);
			AssertEquals("CODE", data.Part1.OP_RH_NKCommodityCode);
			AssertEquals(true, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(true, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib3); // not setup on Client;
			AssertEquals(true, data.Part1.RelatedOrganisations[0].OU_UseSerialNumber);
			AssertEquals(true, data.Part1.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(true, data.Part1.RelatedOrganisations[0].OU_UsePackingDate);
		}

		#endregion

		#region TestPalletLabelAutoPrinter

		public void TestPrintPalletIdLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var printer = Helper.CreatePrintQueue("PRINTER");
			Factory.Save();

			inventory.PrintPalletIdLabel(printer.PK.ToGuid(), 1);
			var printJobQuery = new ZQuery();
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_SQ, printer.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, inventory.Docket.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_JobType, "PRN");
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_Copies, (short)1);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_DocumentName, "Pallet Labels" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling);
			AssertNotNull("Should have created the correct Print Job.", Factory.LoadTop1<IStmPrintJob>(printJobQuery));
		}

		public void TestPalletLabelPrintFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();

			var printFailedEventHit = 0;
			var message = "";
			inventory.PalletLabelPrintFailed += (sender, e) =>
			{
				printFailedEventHit++;
				message = e.Message;
			};

			inventory.PrintPalletIdLabel(new Guid(), 1);
			AssertNull("No Print Job created.", Factory.LoadTop1<IStmPrintJob>(new ZQuery()));
			AssertEquals(1, printFailedEventHit);
			AssertEquals("Error message returned.", "Invalid Printer provided.", message);
		}

		#endregion

		#region TestCanCommitToTransactionLine

		public void TestCanCommitToTransactionLine_ReceiveLineInventory_TransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.Location, data.Whs1.FindLocation("A-2"));

			AssertEquals("Inventory can commit to transfer line.", true, inventory.CanCommitToTransactionLine(transferLine));
		}

		public void TestCanCommitToTransactionLine_ReceiveLineInventory_AdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"));
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = (WhsAdjustmentLine)adjustment.CreateDocketLineFromInventory(inventory);
			adjustmentLine.WE_TransactionQuantity = -4;
			adjustmentLine.WE_ReasonCode = "CLI";

			AssertEquals("Inventory can commit to an adjustment line.", true, inventory.CanCommitToTransactionLine(adjustmentLine));
		}

		public void TestCanCommitToTransactionLine_TransferLineInventory_TransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.Location, data.Whs1.FindLocation("A-2"));
			transferLine.SetDocketLineFromInventory(inventory.InDocketLine, ExcludeFromCopy.None);
			transferLine.RunPreSaveValidation();
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer2Line = Helper.CreateWhsTransferLine(transfer2, data.Part1, 5m, transferLine.Location, data.Whs1.FindLocation("A-1"));

			var transferLineInventory = transferLine.Inventory[0];
			AssertEquals("Inventory can commit to transfer line.", true, transferLineInventory.CanCommitToTransactionLine(transfer2Line));
		}

		public void TestCanCommitToTransactionLine_TransferLineInventory_AdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.Location, data.Whs1.FindLocation("A-2"));
			transferLine.SetDocketLineFromInventory(inventory.InDocketLine, ExcludeFromCopy.None);
			transferLine.RunPreSaveValidation();
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);

			var transferLineInventory = transferLine.Inventory[0];
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = (WhsAdjustmentLine)adjustment.CreateDocketLineFromInventory(transferLineInventory);
			adjustmentLine.WE_TransactionQuantity = -4;
			adjustmentLine.WE_ReasonCode = "CLI";

			AssertEquals("Inventory can commit to transfer line.", true, transferLineInventory.CanCommitToTransactionLine(adjustmentLine));
		}

		public void TestCanCommitToTransactionLine_AdjustmentLineInventory_TransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, inventory.Location);
			adjustmentLine.RunPreSaveValidation();
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, adjustmentLine.Location, data.Whs1.FindLocation("A-2"));

			var adjustmentLineInventory = adjustmentLine.Inventory[0];
			AssertEquals("Inventory can commit to transfer line.", true, adjustmentLineInventory.CanCommitToTransactionLine(transferLine));
		}

		public void TestCanCommitToTransactionLine_AdjustmentLineInventory_AdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, inventory.Location);
			adjustmentLine.RunPreSaveValidation();
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);

			var adjusmtmentLineInventory = adjustmentLine.Inventory[0];
			var adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustment2Line = (WhsAdjustmentLine)adjustment2.CreateDocketLineFromInventory(adjusmtmentLineInventory);
			adjustment2Line.WE_TransactionQuantity = -4;
			adjustment2Line.WE_ReasonCode = "CLI";

			AssertEquals("Inventory can commit to transfer line.", true, adjusmtmentLineInventory.CanCommitToTransactionLine(adjustment2Line));
		}

		public void TestCanCommitToTransactionLine_VasOrderTransferInInventory_TransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);

			var transferInInventory = intoServiceAreaTransfer.Lines.Single();
			var destinationLocation = data.Whs1.FindLocation("A-2");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, transferInInventory.Location, destinationLocation);

			AssertEquals("VAS Order Transfer In Inventory can only commit to a VAS Order Transfer Out transaction line.", false, transferInInventory.Inventory[0].CanCommitToTransactionLine(transferLine));
		}

		public void TestCanCommitToTransactionLine_VasOrderTransferInInventory_AdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);

			var transferInInventory = intoServiceAreaTransfer.Lines.Single().Inventory[0];

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = (WhsAdjustmentLine)adjustment.CreateDocketLineFromInventory(transferInInventory);
			adjustmentLine.WE_TransactionQuantity = -4;
			adjustmentLine.WE_ReasonCode = "CLI";

			AssertEquals("VAS Order Transfer In Inventory can only commit to a VAS Order Transfer Out transaction line.", false,
				transferInInventory.CanCommitToTransactionLine(adjustmentLine));
		}

		public void TestCanCommitToTransactionLine_VasOrderTransferInInventory_VASOrderTransferOut()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}
			AssertNotNull("Precondition: Return transfer successfully created.", returnTransfer);

			var transferInInventory = intoServiceAreaTransfer.Lines.Single().Inventory[0];
			var transferLineOnReturnTransfer = (WhsTransferLine)returnTransfer.Lines.Single();

			AssertEquals("VAS Order Transfer In Inventory can only commit to a VAS Order Transfer Out transaction line.", true,
				transferInInventory.CanCommitToTransactionLine(transferLineOnReturnTransfer));
		}

		public void TestCanCommitToTransactionLine_FinalisedVasOrderTransferInInventory_TransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			var transferInLine = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			transferInLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("VAS order is completed", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			vasOrder.FinaliseVASOrder(Notify, false);
			AssertEquals("VAS order is finalised.", true, vasOrder.IsFinalised);
			Factory.Save();

			var transferInInventory = intoServiceAreaTransfer.Lines.Single();
			var destinationLocation = data.Whs1.FindLocation("A-2");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, transferInInventory.Location, destinationLocation);

			AssertEquals("Finalised VAS Order Transfer In Inventory can commit to a transfer line.", true,
				transferInInventory.Inventory[0].CanCommitToTransactionLine(transferLine));
		}

		public void TestCanCommitToTransactionLine_FinalisedVasOrderTransferInInventory_AdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			var transferInLine = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			transferInLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("VAS order is completed", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			vasOrder.FinaliseVASOrder(Notify, false);
			AssertEquals("VAS order is finalised.", true, vasOrder.IsFinalised);
			Factory.Save();

			var transferInInventory = intoServiceAreaTransfer.Lines.Single().Inventory[0];

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = (WhsAdjustmentLine)adjustment.CreateDocketLineFromInventory(transferInInventory);
			adjustmentLine.WE_TransactionQuantity = -4;
			adjustmentLine.WE_ReasonCode = "CLI";

			AssertEquals("Finalised VAS Order Transfer In Inventory can commit to an adjustment line.", true,
				transferInInventory.CanCommitToTransactionLine(adjustmentLine));
		}

		#endregion

		//

		#region Implementation

		WhsInventoryView inventory;

		protected WhsInventoryView Inventory
		{
			get { return inventory ?? (inventory = (WhsInventoryView)GetNewBusinessObject()); }
			set { inventory = value; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewReceiveLine().Inventory[0];
		}

		public override void TestCallsBaseSetDefaultValues()
		{
			// When inventory created from receive line (like in all tests here) SetDefaultValues will not be actually called
			Assert(true);
		}

		protected virtual Type GetExpectedValidationType()
		{
			return typeof(WhsInventoryViewValidation);
		}

		protected virtual Type GetExpectedUSValidationType()
		{
			return typeof(US.WhsInventoryValidation);
		}

		protected virtual WhsReceive GetNewReceive()
		{
			return Factory.New<WhsReceive>();
		}

		protected virtual WhsReceiveLine GetNewReceiveLine()
		{
			return GetNewReceive().Lines.AddNew();
		}

		protected virtual WhsTransferLine GetNewTransferLine()
		{
			return Factory.New<WhsTransferLine>();
		}

		protected virtual WhsAdjustmentLine GetNewAdjustmentLine()
		{
			return Factory.New<WhsAdjustmentLine>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, false, false);
			return receive.Inventory[0];
		}

		#endregion
	}
}
