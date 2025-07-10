using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsPickableDocketLineTest<TDocketLine, TDocket> : WhsDocketLineTestCase<TDocketLine, TDocket>
		where TDocketLine : WhsPickableDocketLine
		where TDocket : WhsPickableDocket
	{
		#region Related Entities

		#region TestInventory

		protected override void TestInventoryCore()
		{
			Assert("Inventory not used by Pickable Dockets.", true);
		}

		#endregion

		#region TestPickLines

		public void TestPickLines()
		{
			TestPickLinesCore();
		}

		protected virtual void TestPickLinesCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// receive parts to build a bike
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			for (var i = 0; i < 2; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeWheel, 1m); // make sure these are on sep inv lines
			}

			Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeEngine, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.Polish, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// order some bikes
			var docket = GetPickableDocket_ForPickLinesTest(data);
			Helper.CreatePickNew(docket);
			AssertEquals("Precondition - Pick failed.", true, docket.IsAttachedToPickButNotFinalised);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var docketInOtherFactory = otherFactory.Load<WhsPickableDocket>(docket.PK);
			var line1 = FindPickableDocketLine(docketInOtherFactory, data.BOM.BikeEngine);
			var line2 = FindPickableDocketLine(docketInOtherFactory, data.BOM.BikeWheel);
			var initialDBHits = otherFactory.DatabaseLoadCount;
			AssertEquals("Should be 1 PickLine (1 Engine).", 1, line1.PickLines.Count);
			AssertEquals("Should be child editable.", true, line1.IsRegisteredEditableChildObject(line1.PickLines));
			AssertEquals("Should be 2 PickLines (1 Wheel per PickLine).", 2, line2.PickLines.Count);
			AssertEquals("Should be child editable.", true, line2.IsRegisteredEditableChildObject(line2.PickLines));
			AssertEquals("Should have used only 1 DB hit to load all PickLines", 1, otherFactory.DatabaseLoadCount - initialDBHits);
		}

		protected abstract WhsPickableDocket GetPickableDocket_ForPickLinesTest(TestDataForBOM data);

		protected WhsPickableDocketLine FindPickableDocketLine(WhsPickableDocket order, OrgSupplierPart part)
		{
			foreach (WhsPickableDocketLine line in order.AllLines)
			{
				if (line.WE_OP == part.PK)
				{
					return line;
				}
			}
			throw new ArgumentException("No DocketLine on Order was found for product with part number " + part.OP_PartNum + ".");
		}

		#endregion

		#region TestPickLinesWithNonZeroUnits

		public void TestPickLinesWithNonZeroUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);

			var pick = Helper.CreatePickNew(order);
			AssertEquals(1, orderLine.PickLines.Count);
			AssertEquals(1, orderLine.PickLinesWithNonZeroUnits.Count);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			AssertEquals(1, orderLine.PickLines.Count);
			AssertEquals(0, orderLine.PickLinesWithNonZeroUnits.Count);
		}

		#endregion

		#region TestUseChosenInventoryRow

		protected override void TestUseChosenInventoryRowCore()
		{
			// do not call base
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			SetInventoryDataForUseChosenInventoryRowMethod(data.Line111);
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			docketLine.CustomsData.WB_EntryLineNo = 1;
			docketLine.CustomsData.WB_EntryKey = "DEF";
			docketLine.WE_PerPackageQty = 3m;
			docketLine.WE_PackageGroupId = "321";
			docketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			docketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			docketLine.UseChosenInventoryRowFindAttributes(data.Line111);
			AssertUseChosenInventoryRowHasSetProperties(data.Line111, docketLine);
		}

		protected override void AssertUseChosenInventoryRowHasSetProperties(WhsInventoryView expected, WhsDocketLine actual)
		{
			base.AssertUseChosenInventoryRowHasSetProperties(expected, actual);
			AssertEquals("Pallet ID", string.Empty, actual.WE_PalletID);
		}

		#endregion

		#region TestPickLinesWithNonZeroUnits_DbHits

		public void TestPickLinesWithNonZeroUnits_DbHits()
		{
			const int NumberOfInventoriesToCreate = 10;

			var data = new TestDataSimpleEnvironment(Factory);

			for (int i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R" + i, data.Part1, 1m);
			}

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			for (int i = 0; i < NumberOfInventoriesToCreate / 2; i++)
			{
				Helper.CreateWhsPickableDocketLine(order, data.Part1, 2m);
			}

			Helper.CreatePickNew(order);

			var otherFactory1 = new BusinessObjectFactory();
			var orderInOtherFactory1 = otherFactory1.Load<WhsOrder>(order.PK);
			var poke1 = orderInOtherFactory1.Pick.OrderedInventories;
			var poke2 = orderInOtherFactory1.LinesToPickForBinding.Cast<WhsPickableDocketLine>().SelectMany(l => l.PickLines).Count();
			var poke3 = orderInOtherFactory1.LinesToPickForBinding.Cast<WhsPickableDocketLine>().SelectMany(l => l.PickLinesWithNonZeroUnits).Count();
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgMiscServSchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsDocketSchema.Constants.TableName, 2);
			expectedDbHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsPickSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsPickLineSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, otherFactory1);

			var otherFactory2 = new BusinessObjectFactory();
			var orderInOtherFactory2 = otherFactory2.Load<WhsOrder>(order.PK);
			var poke4 = orderInOtherFactory2.Pick.OrderedInventories;
			var poke5 = orderInOtherFactory2.LinesToPickForBinding.Cast<WhsPickableDocketLine>().SelectMany(l => l.PickLines).Count();
			var poke6 = orderInOtherFactory2.LinesToPickForBinding.Cast<WhsPickableDocketLine>().SelectMany(l => l.PickLinesWithNonZeroUnits).Count();
			expectedDbHits.Remove(WhsWarehouseSchema.Constants.TableName);
			AssertDbHits(expectedDbHits, otherFactory2);
		}

		#endregion

		#region TestReleaseLines

		public void TestReleaseLines()
		{
			var line = TestReleaseLines_SetUpWhsPickableDocket(new TestDataSimpleEnvironment(Factory));
			AssertEquals("Release Lines Collection hasn't been built yet.", false, line.IsReleaseLineCollectionBuilt);

			Helper.CreatePickNew(line.PickableDocket);
			AssertEquals("Release Lines Collection is of correct type.", typeof(WhsReleaseLineCollection), line.ReleaseLines.GetType());
			AssertEquals("Release Lines Collection is registered child editable.", true, line.IsRegisteredEditableChildObject(line.ReleaseLines));
			AssertEquals("Release Lines Collection has been poked.", true, line.IsReleaseLineCollectionBuilt);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var orderLineInFactory2 = factory2.Load<TDocketLine>(line.PK);

			AssertEquals("Release line collection hasnt been built yet.", false, orderLineInFactory2.IsReleaseLineCollectionBuilt);

			_ = orderLineInFactory2.ReleaseLines;
			AssertEquals("Release line collection has been poked.", true, orderLineInFactory2.IsReleaseLineCollectionBuilt);

			orderLineInFactory2.ClearReleaseLines();
			AssertEquals("Release line collection was invalidated.", false, orderLineInFactory2.IsReleaseLineCollectionBuilt);
		}

		protected virtual WhsPickableDocketLine TestReleaseLines_SetUpWhsPickableDocket(TestDataSimpleEnvironment data)
		{
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Now;
			pickableDocket.ConsigneePK = data.Org1.PK;

			var orderLine = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 20m);
			if (orderLine is WhsWorkOrderLine && !(orderLine.ChildComponentLines.Count > 0))
			{
				var componentLine = pickableDocket.Lines.AddNew();
				componentLine.WE_OP = data.Part2.PK;
				componentLine.WE_TransactionQuantity = 20m;
				componentLine.WE_WE_ParentDocketLine = orderLine.PK;
			}

			return orderLine;
		}

		#endregion

		#region TestReservedPickLines

		public void TestReservedPickLines_MultipleReservations()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20);

			var link1 = Helper.CreateReservePickLine(orderLine1, data.Line111, 5m);
			var link2 = Helper.CreateReservePickLine(orderLine2, data.Line213, 7m);
			var link3 = Helper.CreateReservePickLine(orderLine2, data.Line214, 2m);
			AssertContainsExactElementsInAnyOrder(new[] { link1 }, orderLine1.ReservedPickLines);
			AssertContainsExactElementsInAnyOrder(new[] { link2, link3 }, orderLine2.ReservedPickLines);
			AssertEquals(true, orderLine1.IsRegisteredEditableChildObject(orderLine1.ReservedPickLines));
			AssertEquals(true, orderLine2.IsRegisteredEditableChildObject(orderLine2.ReservedPickLines));
		}

		#endregion

		#region TestStagingLocationBOM

		public void TestStagingLocationBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.Part1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			AssertNull(docketLine.StagingLocationBOM);

			var productParams = docketLine.Product.ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WL_StagingLocationBOM = Factory.New<WhsLocation>().PK;
			productParams.W3_WL_InwardsProcessingStagingLocationBOM = Factory.New<WhsLocation>().PK;

			AssertNotNull(docketLine.StagingLocationBOM);
			AssertEquals(
				docket.WD_IsInwardsProcessingJob
					? productParams.InwardProcessingStagingLocationBOM
					: productParams.StagingLocationBOM,
				docketLine.StagingLocationBOM);
		}

		public void TestStagingLocationBOM_InwardsProcessing()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket();
			docket.WD_IsInwardsProcessingJob = true;
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.Part1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			AssertNull(docketLine.StagingLocationBOM);

			var productParams = docketLine.Product.ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WL_InwardsProcessingStagingLocationBOM = Factory.New<WhsLocation>().PK;

			AssertNotNull(docketLine.StagingLocationBOM);
			AssertEquals(productParams.InwardProcessingStagingLocationBOM, docketLine.StagingLocationBOM);
		}

		#endregion

		#region TestPickableDocket

		public void TestPickableDocket()
		{
			AssertEquals(Docket.PK, DocketLine.PickableDocket.PK);
		}

		#endregion

		#region TestBOMComponentLinks

		public override bool TestBOMComponentLinks_ExpectedLinkResult => false;

		#endregion

		#endregion

		#region TestStandardReadOnly

		protected override void TestStandardReadOnly(ZPropertyInfo info)
		{
			base.TestStandardReadOnly(info);

			var line = (WhsDocketLine)info.BizObj;
			line.Docket.WD_WP = Factory.New<WhsPick>().PK;
			AssertEquals(info.Name + " should be ReadOnly if the Docket is Picking.", true, line.ZPropertyInfoHash[info.Name].ReadOnly);
		}

		#endregion

		#region Shortfalls

		#region TestDeferSettingHasProductUnitsOrAttribsChanged

		public void TestDeferSettingHasProductUnitsOrAttribsChanged()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket, needWarehouse: false);
			AssertEquals("Precondition: ", false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

			using (docketLine.DeferSettingHasProductUnitsOrAttribsChanged())
			{
				docketLine.WE_TransactionQuantity = 10m;
				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			}

			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
		}

		public void TestDeferSettingHasProductUnitsOrAttribsChanged_WhenDocketIsSuspended()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket, needWarehouse: false);
			AssertEquals("Precondition: ", false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

			using (docketLine.PickableDocket.ShortfallManager.DeferMarkingLinesAsShortfallPropertiesChanged())
			{
				using (docketLine.DeferSettingHasProductUnitsOrAttribsChanged())
				{
					docketLine.WE_TransactionQuantity = 10m;
					AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
				}

				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			}

			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
		}

		#endregion

		#region TestHasProductUnitsOrAttribsChanged

		public void TestHasProductUnitsOrAttribsChanged()
		{
			var year = ZDateTime.Now.Year;

			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket, needWarehouse: false);
			AssertEquals("Precondition: ", false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

			docketLine.WE_TransactionQuantity = 10m;
			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			docketLine.Shortfall.HasProductUnitsOrAttribsChanged = false; // cleanup

			docketLine.WE_PartAttrib1 = "PA1";
			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			docketLine.Shortfall.HasProductUnitsOrAttribsChanged = false; // cleanup

			docketLine.WE_PartAttrib2 = "PA2";
			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			docketLine.Shortfall.HasProductUnitsOrAttribsChanged = false; // cleanup

			docketLine.WE_PartAttrib3 = "PA3";
			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			docketLine.Shortfall.HasProductUnitsOrAttribsChanged = false; // cleanup

			docketLine.WE_SerialNumber = "SN1";
			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			docketLine.Shortfall.HasProductUnitsOrAttribsChanged = false; // cleanup

			docketLine.WE_ExpiryDate = new ZDate(year, 1, 1);
			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			docketLine.Shortfall.HasProductUnitsOrAttribsChanged = false; // cleanup

			docketLine.WE_PackingDate = new ZDate(year, 1, 1);
			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			docketLine.Shortfall.HasProductUnitsOrAttribsChanged = false; // cleanup

			docketLine.WE_OP = ZGuid.NewZGuid();
			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			docketLine.Shortfall.HasProductUnitsOrAttribsChanged = false; // cleanup

			docketLine.WE_BondedEntryKey = "NKEY";
			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			docketLine.Shortfall.HasProductUnitsOrAttribsChanged = false; // cleanup

			docketLine.WE_LineNo = 2;
			AssertEquals("LineNo is not part of the Shortfall calculation, should be ignored.", false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
		}

		public void TestHasProductUnitsOrAttribsChanged_IsNotCheckedIfShortfallManagerIsSuspended()
		{
			var year = ZDateTime.Now.Year;

			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket, needWarehouse: false);
			AssertEquals("Precondition: ", false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

			using (docketLine.PickableDocket.ShortfallManager.DeferMarkingLinesAsShortfallPropertiesChanged())
			{
				docketLine.WE_TransactionQuantity = 10m;
				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

				docketLine.WE_PartAttrib1 = "PA1";
				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

				docketLine.WE_PartAttrib2 = "PA2";
				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

				docketLine.WE_PartAttrib3 = "PA3";
				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

				docketLine.WE_SerialNumber = "SN1";
				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

				docketLine.WE_ExpiryDate = new ZDate(year, 1, 1);
				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

				docketLine.WE_PackingDate = new ZDate(year, 1, 1);
				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);

				docketLine.WE_BondedEntryKey = "NKEY";
				AssertEquals(false, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
			}

			AssertEquals(SupportsHasProductUnitsOrAttribsChanged, docketLine.Shortfall.HasProductUnitsOrAttribsChanged);
		}

		protected virtual bool SupportsHasProductUnitsOrAttribsChanged => true;

		#endregion

		#region TestGetShortfallExistsStatus

		#region TestGetShortfallExistsStatus_BeforePick

		public void TestGetShortfallExistsStatus_BeforePick()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.Part1.PK;

			docketLine.WE_TransactionQuantity = 120;
			AssertEquals(UsesShortfall, docketLine.GetShortfallExistsStatus());

			docketLine.WE_TransactionQuantity = 100;
			AssertEquals(false, docketLine.GetShortfallExistsStatus());
		}

		protected virtual bool UsesShortfall => true;

		#endregion

		#region TestGetShortfallExistsStatus_AfterPick

		public void TestGetShortfallExistsStatus_AfterPick()
		{
			TestGetShortfallExistsStatus_AfterPickCore();
		}

		protected abstract void TestGetShortfallExistsStatus_AfterPickCore();

		#endregion

		#region TestGetShortfallExistsStatus_WithoutUpdatingCache

		public void TestGetShortfallExistsStatus_WithoutUpdatingCache()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "rcv", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_TransactionQuantity = 130; // only 100 units in inventory
			Factory.Save();

			AssertEquals(UsesShortfall, docketLine.GetShortfallExistsStatus_WithoutUpdatingCache());

			if (ShortfallQuantityIsCached)
			{
				// adjust out the inventory (cannot simply set WE_TransactionQuantity as it will refresh the shortfall cache).
				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "adj", Notify);
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 100m, line1.Location);
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 100m, line2.Location);
				adjustment.FinaliseDocket();
				Factory.Save();

				AssertIsFinalisedPrecondition(adjustment);
				AssertEquals("Shortfall Qty Cache should not have been updated.", UsesShortfall, docketLine.GetShortfallExistsStatus_WithoutUpdatingCache());
			}
		}

		#endregion

		#endregion

		#region TestWE_ShortfallQuantityCached

		#region TestWE_ShortfallQuantityCached_BeforePick

		public void TestWE_ShortfallQuantityCached_BeforePick()
		{
			TestWE_ShortfallQuantityCached_BeforePickCore();
		}

		protected abstract void TestWE_ShortfallQuantityCached_BeforePickCore();

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsed

		[TestDate(2013, 03, 25)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsed_BeforeSave_WarehouseConsignee()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsedCore(
				isBeforeSave: true,
				minShelfWarehoueConsignee: true,
				minShelfConsignee: false,
				minShelfProduct: false);
		}

		[TestDate(2013, 03, 25)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsed_BeforeSave_Consignee()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsedCore(
				isBeforeSave: true,
				minShelfWarehoueConsignee: false,
				minShelfConsignee: true,
				minShelfProduct: false);
		}

		[TestDate(2013, 03, 25)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsed_BeforeSave_Product()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsedCore(
				isBeforeSave: true,
				minShelfWarehoueConsignee: false,
				minShelfConsignee: false,
				minShelfProduct: true);
		}

		[TestDate(2013, 03, 25)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsed_AfterSave_WarehouseConsignee()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsedCore(
				isBeforeSave: false,
				minShelfWarehoueConsignee: true,
				minShelfConsignee: false,
				minShelfProduct: false);
		}

		[TestDate(2013, 03, 25)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsed_AfterSave_Consignee()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsedCore(
				isBeforeSave: false,
				minShelfWarehoueConsignee: false,
				minShelfConsignee: true,
				minShelfProduct: false);
		}

		[TestDate(2013, 03, 25)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsed_AfterSave_Product()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsedCore(
				isBeforeSave: false,
				minShelfWarehoueConsignee: false,
				minShelfConsignee: false,
				minShelfProduct: true);
		}

		void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_JulianNumberPartAttributeUsedCore(
			bool isBeforeSave,
			bool minShelfWarehoueConsignee,
			bool minShelfConsignee,
			bool minShelfProduct)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part1ClientRelation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			var part2ClientRelation = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>().Single();

			var consignee = Helper.CreateClient("CONSIGNEE");
			var part1ConsigneeRelation = Helper.CreateProductClientRelationShip(consignee, data.Part1, "WCN");
			var part2ConsigneeRelation = Helper.CreateProductClientRelationShip(consignee, data.Part2, "WCN");

			ZShort minShelfLife = 15;
			if (minShelfWarehoueConsignee)
			{
				part1ConsigneeRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
				part2ConsigneeRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
			}

			if (minShelfConsignee)
			{
				consignee.MiscServ.OM_MinimumShelfLifeAccepted = minShelfLife;
			}

			if (minShelfProduct)
			{
				part1ClientRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
				part2ClientRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
			}

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // will enable Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true); // will enable Expiry Date
			part2ClientRelation.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1).W3_MaximumShelfLife = 30;
			Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1).W3_MaximumShelfLife = 30;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Today.AddDays(10), ZDate.Empty, "ABCD", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today.AddDays(20), ZDate.Empty, "ABCD", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, ZDate.Empty, ZDate.Empty, "", "3060ABCD", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, ZDate.Empty, ZDate.Empty, "", "3070ABCD", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// Query for each separate line will be run
			if (isBeforeSave)
			{
				Factory.Save();
			}

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.ConsigneePK = consignee.PK;
			var orderLine_NormalAttributes = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine_JulianBatchNumber = Helper.CreateWhsOrderLine(order, data.Part2, 40m);

			// 1 Query for all lines in 1 DB hit will be run.
			if (!isBeforeSave)
			{
				Factory.Save();
			}

			AssertEquals("Julian Batch Number is not used, stock not satisfying minimum shelf life should be available to pick.", 10m, orderLine_NormalAttributes.WE_ShortfallQuantityCached);
			AssertEquals("Julian Batch Number is used, only stock that satisfy minimum shelf life should be available to pick.", 20m, orderLine_JulianBatchNumber.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsed

		[TestDate(2019, 04, 15)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsed_BeforeSave_WarehouseConsignee()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsedCore(
				isBeforeSave: true,
				minShelfWarehoueConsignee: true,
				minShelfConsignee: false,
				minShelfProduct: false);
		}

		[TestDate(2019, 04, 15)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsed_BeforeSave_Consignee()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsedCore(
				isBeforeSave: true,
				minShelfWarehoueConsignee: false,
				minShelfConsignee: true,
				minShelfProduct: false);
		}

		[TestDate(2019, 04, 15)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsed_BeforeSave_Product()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsedCore(
				isBeforeSave: true,
				minShelfWarehoueConsignee: false,
				minShelfConsignee: false,
				minShelfProduct: true);
		}

		[TestDate(2019, 04, 15)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsed_AfterSave_WarehouseConsignee()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsedCore(
				isBeforeSave: false,
				minShelfWarehoueConsignee: true,
				minShelfConsignee: false,
				minShelfProduct: false);
		}

		[TestDate(2019, 04, 15)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsed_AfterSave_Consignee()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsedCore(
				isBeforeSave: false,
				minShelfWarehoueConsignee: false,
				minShelfConsignee: true,
				minShelfProduct: false);
		}

		[TestDate(2019, 04, 15)]
		public void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsed_AfterSave_Product()
		{
			TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsedCore(
				isBeforeSave: false,
				minShelfWarehoueConsignee: false,
				minShelfConsignee: false,
				minShelfProduct: true);
		}

		void TestWE_ShortfallQuantityCached_BeforePick_TakesIntoAccountMinimumShelfLifeOfConsignee_ExpiryDatePartAttributeUsedCore(
			bool isBeforeSave,
			bool minShelfWarehoueConsignee,
			bool minShelfConsignee,
			bool minShelfProduct)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part1ClientRelation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			var part2ClientRelation = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>().Single();

			var consignee = Helper.CreateClient("CONSIGNEE");
			var part1ConsigneeRelation = Helper.CreateProductClientRelationShip(consignee, data.Part1, "WCN");
			var part2ConsigneeRelation = Helper.CreateProductClientRelationShip(consignee, data.Part2, "WCN");

			ZShort minShelfLife = 15;
			if (minShelfWarehoueConsignee)
			{
				part1ConsigneeRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
				part2ConsigneeRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
			}

			if (minShelfConsignee)
			{
				consignee.MiscServ.OM_MinimumShelfLifeAccepted = minShelfLife;
			}

			if (minShelfProduct)
			{
				part1ClientRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
				part2ClientRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
			}

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, ZDate.Today.AddDays(5), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, ZDate.Today.AddDays(10), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// Query for each separate line will be run
			if (isBeforeSave)
			{
				Factory.Save();
			}

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.ConsigneePK = consignee.PK;
			var orderLine_ProdWithExpDate = Helper.CreateWhsOrderLine(order, data.Part1, 40m);
			var orderLine_ProdWithNoExpDate = Helper.CreateWhsOrderLine(order, data.Part2, 20m);

			// 1 Query for all lines in 1 DB hit will be run.
			if (!isBeforeSave)
			{
				Factory.Save();
			}

			AssertEquals("Expiry date is not used, all stocks are available to pick.", 5m, orderLine_ProdWithNoExpDate.WE_ShortfallQuantityCached);
			AssertEquals("Expiry date is used, only stock that satisfy minimum shelf life should be available to pick.", 40m, orderLine_ProdWithExpDate.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_ExpiryDatePartAttributeUsed

		[TestDate(2019, 08, 12)]
		public void TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_ExpiryDatePartAttributeUsed_BeforeSave()
		{
			TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_ExpiryDatePartAttributeUsed_Core(true);
		}

		[TestDate(2019, 08, 12)]
		public void TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_ExpiryDatePartAttributeUsed_AfterSave()
		{
			TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_ExpiryDatePartAttributeUsed_Core(false);
		}

		void TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_ExpiryDatePartAttributeUsed_Core(bool isBeforeSave)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today.AddDays(10), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Today.AddDays(1), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, ZDate.Today, ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Today.AddDays(-5), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 13m, ZDate.Today.AddDays(-10), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// Query for each separate line will be run
			if (isBeforeSave)
			{
				Factory.Save();
			}

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine_ProdWithExpDate = Helper.CreateWhsOrderLine(order, data.Part1, 43m);
			var orderLine_ProdWithNoExpDate = Helper.CreateWhsOrderLine(order, data.Part2, 20m);

			// 1 Query for all lines in 1 DB hit will be run.
			if (!isBeforeSave)
			{
				Factory.Save();
			}

			AssertEquals("Expiry date is not used, all stocks are available to pick.", 5m, orderLine_ProdWithNoExpDate.WE_ShortfallQuantityCached);
			AssertEquals("Expiry date is used, only stock that are not expired should be available to pick.", 30m, orderLine_ProdWithExpDate.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_JulianNumberPartAttributeUsed

		[TestDate(2019, 08, 12)]
		public void TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_JulianNumberPartAttributeUsed_BeforeSave()
		{
			TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_JulianNumberPartAttributeUsed_Core(true);
		}

		[TestDate(2019, 08, 12)]
		public void TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_JulianNumberPartAttributeUsed_AfterSave()
		{
			TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_JulianNumberPartAttributeUsed_Core(false);
		}

		void TestWE_ShortfallQuantityCached_BeforePick_NoMinimumShelf_JulianNumberPartAttributeUsed_Core(bool isBeforeSave)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // will enable Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true); // will enable Expiry Date
			data.Part2.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1).W3_MaximumShelfLife = 5;
			Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1).W3_MaximumShelfLife = 5;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Today, ZDate.Empty, "ABCD", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today.AddDays(2), ZDate.Empty, "ABCD", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, ZDate.Empty, ZDate.Empty, "", "9201ABCD", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, ZDate.Empty, ZDate.Empty, "", "9238ABCD", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// Query for each separate line will be run
			if (isBeforeSave)
			{
				Factory.Save();
			}

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine_NormalAttributes = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine_JulianBatchNumber = Helper.CreateWhsOrderLine(order, data.Part2, 40m);

			// 1 Query for all lines in 1 DB hit will be run.
			if (!isBeforeSave)
			{
				Factory.Save();
			}

			AssertEquals("Julian Batch Number is not used, only unexpired stock should be available to pick.", 10m, orderLine_NormalAttributes.WE_ShortfallQuantityCached);
			AssertEquals("Julian Batch Number is used, only unexpired stock should be available to pick.", 20m, orderLine_JulianBatchNumber.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_AfterPick

		public void TestWE_ShortfallQuantityCached_AfterPick()
		{
			TestWE_ShortfallQuantityCached_AfterPickCore();
		}

		protected abstract void TestWE_ShortfallQuantityCached_AfterPickCore();

		#endregion

		#region TestWE_ShortfallQuantityCached_ReadOnly

		public void TestWE_ShortfallQuantityCached_ReadOnly()
		{
			TestReadOnly(d => d.WE_ShortfallQuantityCachedInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_ExtendedLinePriceInfo

		public void TestWE_ExtendedLinePriceInfo_ReadOnly()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_WhsIsRecalculateOrderPricing = false;
			var docket = GetNewWhsDocket(client, null);
			var docketLine = GetNewBusinessObject(docket);

			client.MiscServ.OM_WhsIsRecalculateOrderPricing = true;
			TestReadOnly(docketLine.WE_ExtendedLinePriceInfo, docket.IsRecalculateOrderPricing, docket.IsRecalculateOrderPricing, true, true);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_HumanReadableName

		public void TestWE_ShortfallQuantityCached_HumanReadableName()
		{
			AssertEquals("Shortfall Qty", DocketLine.WE_ShortfallQuantityCachedInfo.HumanReadableName);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccess

		public void TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccess()
		{
			TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccessCore();
		}

		protected virtual void TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccessCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);

			// commit some units
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var newDocket = GetNewWhsDocket();
			newDocket.WD_OH_Client = data.Org1.PK;
			newDocket.WD_WW_Whs = data.Whs1.PK;

			var line1 = newDocket.Lines.AddNew();
			var line2 = newDocket.Lines.AddNew();

			line1.WE_OP = data.Part1.PK;
			line2.WE_OP = data.Part2.PK;

			line1.WE_TransactionQuantity = 100; // only 90 available (10 are committed)
			line2.WE_TransactionQuantity = 120; // only 100 available

			Factory.Save();

			AssertNoWarnings("Precondition", line1.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings("Precondition", line2.WE_ShortfallQuantityCachedInfo);

			var dbLoadCount = Factory.DatabaseLoadCount;
			AssertEquals(10m, line1.WE_ShortfallQuantityCached);
			AssertEquals(20m, line2.WE_ShortfallQuantityCached);
			AssertHasWarning(line1.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 90 unit(s) currently available");
			AssertHasWarning(line2.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 100 unit(s) currently available");
			AssertEquals("The DB should have been hit only once, thereafter the ShortfallQty on each line should be cached.", dbLoadCount + 1, Factory.DatabaseLoadCount);
		}

		public void TestTotalPickLineQuantityAndOrderFromComponents()
		{
			TestTotalPickLineQuantityAndOrderFromComponentsCore();
		}

		protected virtual void TestTotalPickLineQuantityAndOrderFromComponentsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			mainProduct.OP_StockKeepingUnit = Constants.PkgUnit.Piece;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			Helper.CreatePickNew(orderLine.PickableDocket);
			AssertEquals("TotalPickLineQuantityFromComponents should be 5", 5m, orderLine.TotalPickLineQuantityFromComponents);
			AssertEquals("TotalQuantityOrderedFromComponents should be 5", 5m, orderLine.TotalQuantityOrderedFromComponents);
		}

		public void TestIsBOMProductPickedOnSalesOrder()
		{
			TestIsBOMProductPickedOnSalesOrderCore();
		}

		protected virtual void TestIsBOMProductPickedOnSalesOrderCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			Helper.CreatePickNew(orderLine.PickableDocket);
			AssertEquals("1 Child Lines should be created", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("IsComponentPickedOnBOMOrder should be true", true, orderLine.IsBOMProductPickedOnSalesOrder);
			AssertEquals("IsComponentPickedOnBOMOrder should be false for child line", false, orderLine.ChildComponentLines.ElementAt(0).IsBOMProductPickedOnSalesOrder);
		}

		public void TestWE_ShortfallQuantityCached_UpdateAllLinesOnFirstAccessWithBOM()
		{
			TestWE_ShortfallQuantityCached_UpdateAllLinesOnFirstAccessWithBOMCore();
		}

		protected virtual void TestWE_ShortfallQuantityCached_UpdateAllLinesOnFirstAccessWithBOMCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			AssertNoWarnings("Should not have any error.", orderLine.WE_ShortfallQuantityCachedInfo);

			// when we access this property, we will actually calculate and set it. And trigger the validation for it.
			AssertEquals(4m, orderLine.WE_ShortfallQuantityCached);
			AssertHasWarning(orderLine.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 11 unit(s) currently available");
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccess_OnlyCalledWhenLineInDB

		public void TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccess_OnlyCalledWhenLineInDB()
		{
			TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccess_OnlyCalledWhenLineInDBCore();
		}

		protected virtual void TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccess_OnlyCalledWhenLineInDBCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			// commit some units
			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			WhsPickableDocket newDocket = GetNewWhsDocket();
			newDocket.WD_OH_Client = data.Org1.PK;
			newDocket.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();

			Helper.CreatePickNew(order);

			WhsPickableDocketLine unsavedLine = newDocket.Lines.AddNew();
			unsavedLine.WE_OP = data.Part1.PK;
			unsavedLine.WE_TransactionQuantity = 100; // only 50 available (50 are committed)

			AssertNoWarnings("Precondition", unsavedLine.WE_ShortfallQuantityCachedInfo);

			AssertEquals(50m, unsavedLine.WE_ShortfallQuantityCached);
			AssertHasWarning(unsavedLine.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 50 unit(s) currently available");
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_OneDbHitQuery

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys); // Create 700 product in stock

			var order = Helper.CreateWhsOrder(data.Org2, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 701m);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);

			AssertEquals("Should have no shortfall", 0m, otherFactory.Load<WhsOrderLine>(orderLine1.PK).WE_ShortfallQuantityCached);
			AssertEquals("Should have 2 unit shortfall (699 available)", 2m, otherFactory.Load<WhsOrderLine>(orderLine2.PK).WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_OneDbHitQuery_TakeIntoAccountReservedQuantities

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_TakesIntoAccountReservedQuantities()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(30m, 25m, 10m, 0m, 0m, true);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m); // reserved no shortfall
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m); // part reserved and part allocated normally no shortfall
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 30m); // part reserved and part allocated with shortfall
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 40m); // part reserved to multiple inventories and part allocated with shortfall

			Helper.CreateReservePickLine(orderLine1, data.Line111, 10m);
			Helper.CreateReservePickLine(orderLine2, data.Line111, 10m);
			Helper.CreateReservePickLine(orderLine3, data.Line112, 15m);
			Helper.CreateReservePickLine(orderLine4, data.Line112, 10m);
			Helper.CreateReservePickLine(orderLine4, data.Line113, 10m);

			Factory.Save();

			AssertEquals("10 Ordered, 10 Reserved + 10 Available, 0 Shortfall.", 0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("20 Ordered, 10 Reserved + 10 Available, 0 Shortfall.", 0m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals("30 Ordered, 15 Reserved + 0 Available, 15 Shortfall.", 15m, orderLine3.WE_ShortfallQuantityCached);
			AssertEquals("40 Ordered, 10 Reserved + 10 Reserved + 0 Available, 20 Shortfall.", 20m, orderLine4.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_OneDbHitQuery_TakesIntoAccountAdjustments

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_TakesIntoAccountAdjustments()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "RED", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "BLUE", "", "", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -4m, inventory1.LocationString, "RED", "", "", "", ZDate.Empty, ZDate.Empty);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -7m, inventory2.LocationString, "BLUE", "", "", "", ZDate.Empty, ZDate.Empty);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, -1m, inventory1.Location);
			adjustment.RunPreSaveValidation(); // commit stock.
			AssertEquals("Precondition: Stock is committed.", 12m, adjustment.Lines.Cast<WhsAdjustmentLine>().Sum(l => l.CommittedQuantity));

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 6m, ZDate.Empty, ZDate.Empty, "RED", "", "", "", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "BLUE", "", "", "", "");
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 8m);
			AssertEquals("Ordered full available amount, should have no shortfall.", 0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("Ordered 1 more than available amount, should have a shortfall.", 1m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals("Ordered less than available amount, should have no shortfall.", 0m, orderLine3.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_IsNeverNegative

		public void TestWE_ShortfallQuantityCached_IsNeverNegative()
		{
			DocketLine.SetShortfallForTest(-3);
			AssertEquals("Should never show a negative Shortfall (which can happen when a user alters the Order after picking, prior to deallocation of 'overpicked' stock).", 0m, DocketLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_IsNotCalculatedUntilRequiredPropertiesSet

		public void TestWE_ShortfallQuantityCached_IsNotCalculatedUntilRequiredPropertiesSet()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = order.Lines.AddNew();
			orderLine.WE_TransactionQuantity = 125m;
			var poke1 = orderLine.WE_ShortfallQuantityCached;
			AssertNull("Should only calculate shortfall when docket and product are set.", orderLine.GetShortfallCacheValueForTest());

			orderLine.WE_WD = order.PK;
			var poke2 = orderLine.WE_ShortfallQuantityCached;
			AssertNull("Should only calculate shortfall when docket and product are set.", orderLine.GetShortfallCacheValueForTest());

			orderLine.WE_OP = data.Part1.PK;
			var poke3 = orderLine.WE_ShortfallQuantityCached;
			AssertEquals("All required properties are set, should calculate shortfall.", 25m, orderLine.GetShortfallCacheValueForTest());
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_TakesIntoAccountStockCommittedByTransfers

		public void TestWE_ShortfallQuantityCached_TakesIntoAccountStockCommittedByTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit units

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 100m);

			Factory.Save();

			AssertEquals("Transfer should have committed some stock, so now we should have shortfall.", 25m, orderLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_LinesWithSameProduct

		public void TestWE_ShortfallQuantityCached_LinesWithSameProduct()
		{
			if (UsesShortfall)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateProductBOM(data.Part1, data.Part2);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO1", data.Part1, 20m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO2", data.Part2, 20m);
				Factory.Save();

				var order = GetNewWhsDocket(data.Org1, data.Whs1);
				var orderLine1 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 20m);
				var orderLine2 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 20m);
				var orderLine3 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 20m);

				AssertEquals("20 Ordered, 20 Available, 0 Shortfall.", 0m, orderLine1.WE_ShortfallQuantityCached);
				AssertEquals("20 Ordered, 0 Available, 20 Shortfall.", 20m, orderLine2.WE_ShortfallQuantityCached);
				AssertEquals("20 Ordered, 0 Available, 20 Shortfall.", 20m, orderLine3.WE_ShortfallQuantityCached);
			}
			else
			{
				// We don't calculate shortfall
				Assert(true);
			}
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_OneDbHitQuery

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_PartAttrib1()
		{
			TestWE_ShortfallQuantityCached_OneDbHitQuery_Core(AttributeNumber.One, (docketline) => docketline.WE_PartAttrib1 = "AT1");
		}

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_PartAttrib2()
		{
			TestWE_ShortfallQuantityCached_OneDbHitQuery_Core(AttributeNumber.Two, (docketline) => docketline.WE_PartAttrib2 = "AT2");
		}

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_PartAttrib3()
		{
			TestWE_ShortfallQuantityCached_OneDbHitQuery_Core(AttributeNumber.Three, (docketline) => docketline.WE_PartAttrib3 = "AT3");
		}

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_ExpiryDate()
		{
			// should not be expired, anytime in future
			TestWE_ShortfallQuantityCached_OneDbHitQuery_Core(AttributeNumber.ExpiryDate, (docketline) => docketline.WE_ExpiryDate = ZDate.Today.AddDays(+10));
		}

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_PackingDate()
		{
			TestWE_ShortfallQuantityCached_OneDbHitQuery_Core(AttributeNumber.PackingDate, (docketline) => docketline.WE_PackingDate = ZDate.Today);
		}

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_Core(AttributeNumber attributeNumber, Action<WhsDocketLine> setAttribute)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 2m, true, false);
			setAttribute(receive.Lines[0]);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2); // line with no attribute can take any available inventory with attribute
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5);
			setAttribute(orderLine2);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 4);
			setAttribute(orderLine3);

			Helper.CreateReservePickLine(orderLine2, receive.Inventory[0], 1m);
			Factory.Save();

			AssertEquals("2 Ordered, 0 Reserved + 1 Available, 1 Shortfall.", 1m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("5 Ordered, 1 Reserved + 1 Available, 3 Shortfall.", 3m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals("4 Ordered, 0 Reserved + 0 Available, 4 Shortfall.", 4m, orderLine3.WE_ShortfallQuantityCached);
		}

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_BondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, false);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO1", data.Part1, 1m, "B123-3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO2", data.Part1, 1m, "B123-3").Lines[0].WE_PartAttrib1 = "AT1";
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1, "B123-3", "DummyOutward-3", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 2, "B123-3", "DummyOutward-3", "");
			orderLine2.WE_PartAttrib1 = "AT1";
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 3, "B123-4", "DummyOutward-4", "");
			orderLine3.WE_PartAttrib1 = "AT1";

			Helper.CreateReservePickLine(orderLine1, receive.Inventory[0], 1m);
			Factory.Save();

			AssertEquals("1 Ordered, 1 Reserved + 0 Available, 0 Shortfall.", 0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("2 Ordered, 0 Reserved + 1 Available, 1 Shortfall.", 1m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals("3 Ordered, 0 Reserved + 0 Available, 3 Shortfall.", 3m, orderLine3.WE_ShortfallQuantityCached);
		}

		public void TestWE_ShortfallQuantityCached_OneDbHitQuery_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine1.WE_SerialNumber = "SN1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine2.WE_SerialNumber = "SN2";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine3.WE_SerialNumber = "SN3";
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine4.WE_SerialNumber = "SN4";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "SN1";
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine3.WE_SerialNumber = "SN1";
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine4.WE_SerialNumber = "SN6";
			var orderLine5 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine5.WE_SerialNumber = "SN3";
			var orderLine6 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine6.WE_SerialNumber = "SN1";

			Helper.CreateReservePickLine(orderLine2, receiveLine1.Inventory[0], 1m);
			Factory.Save();

			AssertEquals("1 Ordered, 0 Reserved + 3 Available, 0 Shortfall.", 0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("1 Ordered, 1 Reserved + 0 Available, 0 Shortfall.", 0m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals("1 Ordered, 0 Reserved + 0 Available, 1 Shortfall.", 1m, orderLine3.WE_ShortfallQuantityCached);
			AssertEquals("1 Ordered, 0 Reserved + 1 Available, 0 Shortfall.", 0m, orderLine5.WE_ShortfallQuantityCached);
			AssertEquals("1 Ordered, 0 Reserved + 0 Available, 1 Shortfall.", 1m, orderLine6.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_ListChangedIsSuspendedOnDocketLineWhenLoadingShortfallInOneHit

		public void TestWE_ShortfallQuantityCached_ListChangedIsSuspendedOnDocketLineWhenLoadingShortfallInOneHit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var listChangedHitCount = 0;
			((IBindingList)orderLine1).ListChanged += (sender, e) => listChangedHitCount++;
			((IBindingList)orderLine2).ListChanged += (sender, e) => listChangedHitCount++;
			((IBindingList)orderLine3).ListChanged += (sender, e) => listChangedHitCount++;

			_ = orderLine1.WE_ShortfallQuantityCached;
			AssertEquals(0, listChangedHitCount);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_DoesNotRunTotalUnitsValidationIfOrderIsRunningPreSaveValidation

		public void TestWE_ShortfallQuantityCached_DoesNotRunTotalUnitsValidationIfOrderIsRunningPreSaveValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			Helper.CreatePickNew(order);

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			var validationHitCount = 0;
			orderInOtherFactory.WD_TotalUnitsInfo.AdditionalValidation += () => validationHitCount++;
			using (SetIsInPreSaveValidationHack(orderInOtherFactory))
			{
				orderInOtherFactory.Lines.Cast<WhsPickableDocketLine>().ForEach(l => _ = l.WE_ShortfallQuantityCached);
			}

			AssertEquals(0, validationHitCount);

			IDisposable SetIsInPreSaveValidationHack(BusinessObject parent)
			{
				var fieldInfo = typeof(BusinessObject).GetField("preSaveValidationDepthCount", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
				if ((int)fieldInfo.GetValue(parent) == 0)
				{
					fieldInfo.SetValue(parent, 1);
				}

				return new DisposableAction(() => fieldInfo.SetValue(parent, 0));
			}
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_DoesNotRunTotalUnitsValidationWhenSuspended

		public void TestWE_ShortfallQuantityCached_DoesNotRunTotalUnitsValidationWhenSuspended()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			Helper.CreatePickNew(order);

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			var validationHitCount = 0;
			orderInOtherFactory.WD_TotalUnitsInfo.AdditionalValidation += () => validationHitCount++;

			foreach (var line in orderInOtherFactory.Lines.Cast<WhsPickableDocketLine>())
			{
				using (line.SuspendTotalUnitsValidationOnSettingShortfallQuantity())
				{
					_ = line.WE_ShortfallQuantityCached;
				}
			}

			AssertEquals(0, validationHitCount);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_DeleteLine

		public void TestWE_ShortfallQuantityCached_DeleteLine()
		{
			if (UsesShortfall)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateProductBOM(data.Part1, data.Part2);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO1", data.Part1, 20m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO2", data.Part2, 20m);
				Factory.Save();

				var order = GetNewWhsDocket(data.Org1, data.Whs1);
				var orderLine1 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 10m);
				var orderLine2 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 20m);
				var orderLine3 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 30m);
				var orderLine4 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 40m);

				Factory.Save();

				AssertEquals("Precondition.", 0m, orderLine1.WE_ShortfallQuantityCached);
				AssertEquals("Precondition.", 10m, orderLine2.WE_ShortfallQuantityCached);
				AssertEquals("Precondition.", 30m, orderLine3.WE_ShortfallQuantityCached);
				AssertEquals("Precondition.", 40m, orderLine4.WE_ShortfallQuantityCached);

				orderLine2.Delete();

				order.Lines.Cast<WhsPickableDocketLine>().ForEach(l => l.ClearWE_ShortfallQuantityCached());
				AssertEquals("Should be the same.", 0m, orderLine1.WE_ShortfallQuantityCached);
				AssertEquals("Should update shortfalls.", 20m, orderLine3.WE_ShortfallQuantityCached);
				AssertEquals("Should update shortfalls.", 40m, orderLine4.WE_ShortfallQuantityCached);

				orderLine3.Delete();
				Factory.Save();

				order.Lines.Cast<WhsPickableDocketLine>().ForEach(l => l.ClearWE_ShortfallQuantityCached());
				AssertEquals("Should be the same.", 0m, orderLine1.WE_ShortfallQuantityCached);
				AssertEquals("Should update shortfalls.", 30m, orderLine4.WE_ShortfallQuantityCached);
			}
			else
			{
				// We don't calculate shortfall
				Assert(true);
			}
		}

		#endregion

		#endregion

		#region TestSuspendShortfallCalculation

		public void TestSuspendShortfallCalculation()
		{
			TestSuspendShortfallCalculationCore();
		}

		protected abstract void TestSuspendShortfallCalculationCore();

		#endregion

		#region ShortfallQuantityIsCached

		protected virtual bool ShortfallQuantityIsCached => true;

		#endregion

		#endregion

		#region TestPickLineQuantity_ShouldIncludeKitsBuiltFromComponents

		public void TestPickLineQuantityIncludeKitsBuiltFromComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			AssertEquals("TotalQuantityOrderedFromComponents should be 6.", 6m, orderLine.TotalQuantityOrderedFromComponents);
			AssertEquals("TotalPickLineQuantityFromComponents should be 6.", 6m, orderLine.TotalPickLineQuantityFromComponents);
			AssertEquals("PickLineQuantity should includ Kits Built From Components.", 11m, orderLine.PickLineQuantity);
		}

		#endregion

		#region Properties

		#region TestWE_OP_DefaultsPriceFields

		public void TestWE_OP_DefaultsPriceFields()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_UnitPrice = 10m;
			data.Part1.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "AUD";
			data.Part2.RelatedOrganisations[0].OU_UnitPrice = 15m;
			data.Part2.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "USD";
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Precondition: default price field values.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountPercent);

			orderLine.WE_OP = data.Part1.PK;

			if (DefaultPriceFieldsEnabled)
			{
				AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Product unit price info populated on product.", "AUD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountPercent);

				orderLine.WE_OP = data.Part2.PK;
				AssertEquals("Product unit price info populated on product.", 15m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Product unit price info populated on product.", 15m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Product unit price info populated on product.", "USD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountPercent);
			}
			else
			{
				AssertPriceFieldsNotDefaulted(orderLine);
			}
		}

		static void AssertPriceFieldsNotDefaulted(WhsPickableDocketLine orderLine)
		{
			AssertEquals("Price field values not defaulted.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Price field values not defaulted.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Price field values not defaulted.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Price field values not defaulted.", 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Price field values not defaulted.", 0m, orderLine.WE_UnitDiscountPercent);
		}

		protected virtual bool DefaultPriceFieldsEnabled => true;

		public void TestWE_OP_DefaultsPriceFields_ProductWithCurrencyInfoOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "AUD";
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Precondition: default price field values.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountPercent);

			orderLine.WE_OP = data.Part1.PK;

			AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Product unit price info populated on product.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountPercent);
		}

		public void TestWE_OP_DefaultsPriceFields_SamePreviousProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_UnitPrice = 10m;
			data.Part1.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "AUD";
			data.Part2.RelatedOrganisations[0].OU_UnitPrice = 15m;
			data.Part2.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "USD";
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Precondition: default price field values.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountPercent);

			orderLine.WE_OP = data.Part1.PK;

			if (DefaultPriceFieldsEnabled)
			{
				AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Product unit price info populated on product.", "AUD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountPercent);

				orderLine.WE_RecommendedUnitPrice = 5m;
				orderLine.WE_UnitPriceAfterDiscount = 4m;
				orderLine.WE_RX_NKUnitPriceCurrency = "USD";
				orderLine.WE_UnitDiscountAmount = 1m;
				orderLine.WE_UnitDiscountPercent = 20m;

				orderLine.WE_OP = data.Part1.PK;
				AssertEquals("Unit price info not updated if assigning the same product.", 5m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Unit price info not updated if assigning the same product.", 4m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Unit price info not updated if assigning the same product.", "USD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Unit price info not updated if assigning the same product.", 1m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Unit price info not updated if assigning the same product.", 20m, orderLine.WE_UnitDiscountPercent);
			}
			else
			{
				AssertPriceFieldsNotDefaulted(orderLine);
			}
		}

		public void TestWE_OP_DefaultsPriceFields_OverwritesExistingPriceInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_UnitPrice = 10m;
			data.Part1.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "AUD";
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			orderLine.WE_RecommendedUnitPrice = 5m;
			orderLine.WE_UnitPriceAfterDiscount = 4m;
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine.WE_UnitDiscountAmount = 1m;
			orderLine.WE_UnitDiscountPercent = 20m;

			orderLine.WE_OP = data.Part1.PK;
			if (DefaultPriceFieldsEnabled)
			{
				AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Product unit price info populated on product.", "AUD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountPercent);
			}
			else
			{
				AssertEquals("Product unit price info *not* defaulted on product.", 5m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Product unit price info *not* defaulted on product.", 4m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Product unit price info *not* defaulted on product.", "USD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Product unit price info *not* defaulted on product.", 1m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Product unit price info *not* defaulted on product.", 20m, orderLine.WE_UnitDiscountPercent);
			}
		}

		public void TestWE_OP_DefaultsPriceFields_ProductWithCurrencyInfoOnly_WithExistingPriceInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "AUD";
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			orderLine.WE_RecommendedUnitPrice = 5m;
			orderLine.WE_UnitPriceAfterDiscount = 4m;
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine.WE_UnitDiscountAmount = 1m;
			orderLine.WE_UnitDiscountPercent = 20m;

			orderLine.WE_OP = data.Part1.PK;

			if (DefaultPriceFieldsEnabled)
			{
				AssertEquals("Product unit price info populated on product.", 5m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Product unit price info populated on product.", 4m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Product unit price info populated on product.", "USD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Product unit price info populated on product.", 1m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Product unit price info populated on product.", 20m, orderLine.WE_UnitDiscountPercent);
			}
			else
			{
				AssertEquals("Product unit price info *not* defaulted on product.", 5m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Product unit price info *not* defaulted on product.", 4m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Product unit price info *not* defaulted on product.", "USD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Product unit price info *not* defaulted on product.", 1m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Product unit price info *not* defaulted on product.", 20m, orderLine.WE_UnitDiscountPercent);
			}
		}

		public void TestWE_OP_DefaultsPriceFields_ProductWithNoPriceInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var prodRelatedOrg = data.Part1.RelatedOrganisations[0];
			AssertEquals("Precondition: product has no price info.", 0m, prodRelatedOrg.OU_UnitPrice);
			AssertEquals("Precondition: product has no price info.", string.Empty, prodRelatedOrg.OU_RX_NKUnitPriceCurrency);

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Precondition: default price field values.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountPercent);

			orderLine.WE_OP = data.Part1.PK;

			AssertEquals("Nothing populated on orderline.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Nothing populated on orderline.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Nothing populated on orderline.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Nothing populated on orderline.", 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Nothing populated on orderline.", 0m, orderLine.WE_UnitDiscountPercent);
		}

		public void TestWE_OP_DefaultsPriceFields_ProductWithNoPriceInfo_WithExistingPriceInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var prodRelatedOrg = data.Part1.RelatedOrganisations[0];
			AssertEquals("Precondition: product has no price info.", 0m, prodRelatedOrg.OU_UnitPrice);
			AssertEquals("Precondition: product has no price info.", string.Empty, prodRelatedOrg.OU_RX_NKUnitPriceCurrency);

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			orderLine.WE_RecommendedUnitPrice = 5m;
			orderLine.WE_UnitPriceAfterDiscount = 4m;
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine.WE_UnitDiscountAmount = 1m;
			orderLine.WE_UnitDiscountPercent = 20m;

			orderLine.WE_OP = data.Part1.PK;
			AssertEquals("Existing values on product not populated on orderline.", 5m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Existing values on product not populated on orderline.", 4m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Existing values on product not populated on orderline.", "USD", orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Existing values on product not populated on orderline.", 1m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Existing values on product not populated on orderline.", 20m, orderLine.WE_UnitDiscountPercent);
		}

		public void TestWE_OP_DefaultsPriceFields_PopulatingFromUniversalDataObjectReader()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_UnitPrice = 10m;
			data.Part1.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "AUD";
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Precondition: default price field values.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountPercent);

			using (orderLine.SetIsPopulatingFromUniversalDataObjectReader())
			{
				orderLine.WE_OP = data.Part1.PK;
			}

			if (DefaultPriceFieldsEnabled)
			{
				AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Product unit price info populated on product.", "AUD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountPercent);
			}
			else
			{
				AssertPriceFieldsNotDefaulted(orderLine);
			}
		}

		public void TestWE_OP_DefaultsPriceFields_PopulatingFromUniversalDataObjectReader_WithExistingPriceInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_UnitPrice = 10m;
			data.Part1.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "AUD";
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			orderLine.WE_RecommendedUnitPrice = 5m;
			orderLine.WE_UnitPriceAfterDiscount = 4m;
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine.WE_UnitDiscountAmount = 1m;
			orderLine.WE_UnitDiscountPercent = 20m;

			using (orderLine.SetIsPopulatingFromUniversalDataObjectReader())
			{
				orderLine.WE_OP = data.Part1.PK;
			}

			AssertEquals("Product unit price info populated on product.", 5m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Product unit price info populated on product.", 4m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Product unit price info populated on product.", "USD", orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Product unit price info populated on product.", 1m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Product unit price info populated on product.", 20m, orderLine.WE_UnitDiscountPercent);
		}

		public void TestWE_OP_DefaultsPriceFields_PopulatingFromUniversalDataObjectReader_WithExistingPriceInfo_NotAllPriceInfoPopulated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_UnitPrice = 10m;
			data.Part1.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "AUD";
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			orderLine.WE_UnitDiscountAmount = 1m;
			orderLine.WE_UnitDiscountPercent = 20m;

			using (orderLine.SetIsPopulatingFromUniversalDataObjectReader())
			{
				orderLine.WE_OP = data.Part1.PK;
			}

			AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Product unit price info populated on product.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Product unit price info populated on product.", 1m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Product unit price info populated on product.", 20m, orderLine.WE_UnitDiscountPercent);
		}

		public void TestWE_OP_DefaultsPriceFields_OnlyIfIsOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_UnitPrice = 10m;
			data.Part1.RelatedOrganisations[0].OU_RX_NKUnitPriceCurrency = "AUD";
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var fields = typeof(OrgPartRelation.RelationshipTypes).GetFields(BindingFlags.Public | BindingFlags.Static);
			AssertEquals("Precondition: Please ensure that this test remains valid when adding or removing types.", 5, fields.Length);

			foreach (var relationshipType in fields.Select(f => f.GetValue(null).ToString()))
			{
				data.Part1.RelatedOrganisations[0].OU_Relationship = relationshipType;
				var orderLine = order.Lines.AddNew();

				AssertEquals("Precondition: Relationship has been set.", relationshipType, data.Part1.RelatedOrganisations[0].OU_Relationship);
				AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Precondition: default price field values.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountPercent);

				orderLine.WE_OP = data.Part1.PK;

				if (DefaultPriceFieldsEnabled && (relationshipType == OrgPartRelation.RelationshipTypes.Owner || relationshipType == OrgPartRelation.RelationshipTypes.Both))
				{
					AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_RecommendedUnitPrice);
					AssertEquals("Product unit price info populated on product.", 10m, orderLine.WE_UnitPriceAfterDiscount);
					AssertEquals("Product unit price info populated on product.", "AUD", orderLine.WE_RX_NKUnitPriceCurrency);
					AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountAmount);
					AssertEquals("Product unit price info populated on product.", 0m, orderLine.WE_UnitDiscountPercent);
				}
				else
				{
					AssertPriceFieldsNotDefaulted(orderLine);
				}
			}
		}

		public void TestWE_OP_DefaultsPriceFields_GetValueFromOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var relatedOrganisation1 = data.Part1.RelatedOrganisations[0];
			relatedOrganisation1.OU_UnitPrice = 10m;
			relatedOrganisation1.OU_RX_NKUnitPriceCurrency = "USD";
			relatedOrganisation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var relatedOrganisation2 = data.Part1.RelatedOrganisations.AddNew();
			relatedOrganisation2.OU_UnitPrice = 20m;
			relatedOrganisation2.OU_RX_NKUnitPriceCurrency = "AUD";
			relatedOrganisation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relatedOrganisation2.OU_OH = data.Org1.PK;
			var relatedOrganisation3 = data.Part1.RelatedOrganisations.AddNew();
			relatedOrganisation3.OU_UnitPrice = 30m;
			relatedOrganisation3.OU_RX_NKUnitPriceCurrency = "CNY";
			relatedOrganisation3.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			relatedOrganisation3.OU_OH = data.Org1.PK;
			Factory.Save();

			var order = GetNewWhsDocket(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var orderLine = order.Lines.AddNew();

			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("Precondition: default price field values.", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("Precondition: default price field values.", 0m, orderLine.WE_UnitDiscountPercent);

			orderLine.WE_OP = data.Part1.PK;

			if (DefaultPriceFieldsEnabled)
			{
				AssertEquals("Product unit price info populated on product from owner relation.", 20m, orderLine.WE_RecommendedUnitPrice);
				AssertEquals("Product unit price info populated on product from owner relation.", 20m, orderLine.WE_UnitPriceAfterDiscount);
				AssertEquals("Product unit price info populated on product from owner relation.", "AUD", orderLine.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Product unit price info populated on product from owner relation.", 0m, orderLine.WE_UnitDiscountAmount);
				AssertEquals("Product unit price info populated on product from owner relation.", 0m, orderLine.WE_UnitDiscountPercent);
			}
			else
			{
				AssertPriceFieldsNotDefaulted(orderLine);
			}
		}

		#endregion

		#region TestPickGroupForBinding

		public void TestPickGroupForBinding()
		{
			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var orderLine = Factory.New<WhsOrderLine>();
				AssertEquals("", orderLine.PickGroupForBinding);

				orderLine.WE_PickGroup = 1;
				AssertEquals("1 - Desc", orderLine.PickGroupForBinding);

				orderLine.PickGroupForBinding = "A";
				AssertEquals(new ZShort(0), orderLine.WE_PickGroup);

				orderLine.PickGroupForBinding = "2";
				AssertEquals(new ZShort(2), orderLine.WE_PickGroup);
				AssertEquals("2", orderLine.PickGroupForBinding);

				orderLine.PickGroupForBinding = "";
				AssertEquals(new ZShort(0), orderLine.WE_PickGroup);

				orderLine.PickGroupForBinding = "1.1";
				AssertEquals(new ZShort(11), orderLine.WE_PickGroup);

				orderLine.PickGroupForBinding = "111111";
				AssertEquals(short.MaxValue, orderLine.WE_PickGroup);
			}
		}

		#endregion

		#region TestPickGroupForBindingInfo

		public void TestPickGroupForBindingInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals(PickGroupHelper.PickGroupForBindingMaxLength, orderLine.PickGroupForBindingInfo.MaxLength);
			AssertEquals(false, orderLine.PickGroupForBindingInfo.ReadOnly);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			AssertEquals(true, orderLine.PickGroupForBindingInfo.ReadOnly);
		}

		#endregion

		#region TestForDocketWithoutClient

		[TestDate(2013, 03, 25)]
		public void TestForDocketWithoutClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine_NormalAttributes = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Factory.Save();

			var docket = orderLine_NormalAttributes.Docket;
			docket.WD_OH_Client = ZGuid.Empty;
			AssertNoExceptionThrown(() => { var cache = orderLine_NormalAttributes.WE_ShortfallQuantityCached; });
		}

		#endregion

		#region TestWE_CrossDockQuantity

		public void TestWE_CrossDockQuantity()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			AssertEquals(0m, docketLine.WE_CrossDockQuantity);

			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();

			Helper.CreateReservePickLine(docketLine, data.Line111, 5m);
			Helper.CreateReservePickLine(docketLine, data.Line112, 3m);

			AssertEquals(ExpectedCrossDockQuantity, docketLine.WE_CrossDockQuantity);
		}

		protected abstract ZDecimal ExpectedCrossDockQuantity { get; }

		#endregion

		#region TestPickLineQuantity

		public void TestPickLineQuantity()
		{
			TestPickLineQuantityCore();
		}

		protected virtual void TestPickLineQuantityCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();

			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var line = Helper.CreateWhsPickableDocketLine(docket, data.Part1, 100m);

			if (line.CanCrossDockInventory)
			{
				var pickLine = ((WhsOrderLine)line).ReserveStockIfAbleTo(data.Line111, 10m);
				AssertEquals("Reserved stock should not be registered as picked.", 0m, line.PickLineQuantity);
			}

			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(docket);

			pick.AutoAllocateItemsWithMock();
			AssertEquals(100m, line.PickLineQuantity);
		}

		#endregion

		#region TestPickedPickLineQuantity

		public void TestPickedPickLineQuantity()
		{
			TestPickedPickLineQuantityCore();
		}

		protected virtual void TestPickedPickLineQuantityCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();

			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var line = Helper.CreateWhsPickableDocketLine(docket, data.Part1, 100m);

			if (line.CanCrossDockInventory)
			{
				var pickLine = ((WhsOrderLine)line).ReserveStockIfAbleTo(data.Line111, 10m);
				AssertEquals("Reserved stock should not be registered as picked.", 0m, line.PickedPickLineQuantity);
			}

			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(docket);

			pick.AutoAllocateItemsWithMock();
			AssertEquals(0m, line.PickedPickLineQuantity);

			pick.GetAllPickLines().First().WZ_PickedDateTime = DateTimeOffset.Now;
			AssertEquals(20m, line.PickedPickLineQuantity);

			pick.GetAllPickLines().ForEach(l => l.WZ_PickedDateTime = DateTimeOffset.Now);
			AssertEquals(100m, line.PickedPickLineQuantity);
		}

		#endregion

		#region TestQuantityNotPicked

		public void TestQuantityNotPicked()
		{
			TestQuantityNotPickedCore();
		}

		protected virtual void TestQuantityNotPickedCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();

			var order = GetNewWhsDocket();
			order.WD_OH_Client = data.Org1.PK;
			order.WD_WW_Whs = data.Whs1.PK;

			var line = Helper.CreateWhsPickableDocketLine(order, data.Part1, 103m); // only 100 in stock
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			pick.AutoAllocateItemsWithMock();
			AssertEquals(3m, line.QuantityNotPicked);
		}

		#endregion

		#region TestIsDocketPicking

		public void TestIsDocketPicking()
		{
			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket(docketLine);
			AssertEquals("Precondition", false, docketLine.IsDocketPicking);

			var pick = Factory.New<WhsPick>();
			docket.WD_WP = pick.PK;
			AssertEquals(true, docketLine.IsDocketPicking);
			AssertEquals(docket.IsAttachedToPickButNotFinalised, docketLine.IsDocketPicking);

			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertEquals(false, docketLine.IsDocketPicking);
			AssertEquals(docket.IsAttachedToPickButNotFinalised, docketLine.IsDocketPicking);
		}

		#endregion

		#region TestIsPickFinalising

		public void TestIsPickFinalising()
		{
			AssertEquals("Precondition", false, DocketLine.IsPickFinalising);

			var pick = Factory.New<WhsPick>();
			Docket.WD_WP = pick.PK;

			AssertEquals("Precondition", false, DocketLine.IsPickFinalising);

			using (new SemaphoreManager(pick.FinalisePickSemaphore))
			{
				AssertEquals("Precondition", true, pick.IsFinalising);
				AssertEquals(true, DocketLine.IsPickFinalising);
			}
		}

		#endregion

		#region TestIsDocketUnpicked

		public void TestIsDocketUnpicked()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			AssertEquals("No pick attached, docketline should be unpicked.", true, docketLine.IsDocketUnpicked);

			docket.WD_WP = Factory.New<WhsPick>().PK;
			AssertEquals("Pick is attached, docketline should *not* be unpicked.", false, docketLine.IsDocketUnpicked);
			docket.WD_WP = ZGuid.Empty; // clean-up
			AssertEquals("No pick attached, docketline should be unpicked.", true, docketLine.IsDocketUnpicked);

			docket.CancelReactivateDocket();
			AssertEquals("Precondition: Docket was cancelled.", true, docket.IsCancelled);
			AssertEquals("No pick attached, but docket is cancelled, docketline should *not* be unpicked.", false, docketLine.IsDocketUnpicked);
		}

		#endregion

		#region TestWE_CalculateExtendedLinePrice

		public void TestCalculateExtendedLinePrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Now;
			pickableDocket.ConsigneePK = data.Org1.PK;

			var orderLine = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 20m);
			orderLine.WE_UnitPriceAfterDiscount = 10m;
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";

			orderLine.CalculateExtendedLinePrice();
			AssertEquals("Extended line price", pickableDocket.IsRecalculateOrderPricing ? 200m : 0m, orderLine.WE_ExtendedLinePrice);
		}

		public void TestWE_CalculateExtendedLinePrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			client.MiscServ.OM_WhsIsRecalculateOrderPricing = false;
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Now;
			pickableDocket.ConsigneePK = data.Org1.PK;

			var orderLine = (TDocketLine)Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 20m);

			SetLineOrderUnitAndPrice(orderLine, 10, 20);
			AssertEquals("Should not calculate extended line price", 0m, orderLine.WE_ExtendedLinePrice);

			var manualLinePrice = 12m;
			orderLine.WE_ExtendedLinePrice = manualLinePrice;
			SetLineOrderUnitAndPrice(orderLine, 5m, 20m); // does not change the price
			AssertEquals("Should not change extended line price", manualLinePrice, orderLine.WE_ExtendedLinePrice);

			client.MiscServ.OM_WhsIsRecalculateOrderPricing = true;

			var shouldCalcTotals = pickableDocket.IsRecalculateOrderPricing;
			var assertionMessageForSettingExtendedLinePrice = $"Should {(shouldCalcTotals ? "" : "*not*")} overwrite extended line price";

			SetLineOrderUnitAndPrice(orderLine, 10m, 20m);
			AssertEquals("Should not calculate extended line price", shouldCalcTotals ? 10m * 20m : manualLinePrice, orderLine.WE_ExtendedLinePrice);

			orderLine.WE_ExtendedLinePrice = manualLinePrice; // will be overwritten
			SetLineOrderUnitAndPrice(orderLine, 5m, 30m);
			AssertEquals(assertionMessageForSettingExtendedLinePrice, shouldCalcTotals ? 5m * 30m : manualLinePrice, orderLine.WE_ExtendedLinePrice);

			SetLineOrderUnitAndPrice(orderLine, 15m, 30m);
			AssertEquals(assertionMessageForSettingExtendedLinePrice, shouldCalcTotals ? 15m * 30m : manualLinePrice, orderLine.WE_ExtendedLinePrice);

			SetLineOrderUnitAndPrice(orderLine, 15m, 20m);
			AssertEquals(assertionMessageForSettingExtendedLinePrice, shouldCalcTotals ? 15m * 20m : manualLinePrice, orderLine.WE_ExtendedLinePrice);

			TestWE_CalculateExtendedLinePriceCore(orderLine);
		}

		protected virtual void TestWE_CalculateExtendedLinePriceCore(TDocketLine orderLine)
		{
		}

		public void TestWE_CalculateExtendedLinePrice_OnChangeClient()
		{
			var wh = Helper.CreateWarehouse("1", "A", 1, 1);
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			client1.MiscServ.OM_WhsIsRecalculateOrderPricing = false;
			client2.MiscServ.OM_WhsIsRecalculateOrderPricing = true;
			var part = Helper.CreateProduct(client1, "P1");

			var pickableDocket = GetNewWhsDocket(client1, wh);
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Now;
			pickableDocket.ConsigneePK = client1.PK;

			var orderLine = Helper.CreateWhsPickableDocketLine(pickableDocket, part, 20m);

			SetLineOrderUnitAndPrice(orderLine, 10m, 20m);
			AssertEquals("Should not calculate extended line price", 0m, orderLine.WE_ExtendedLinePrice);

			pickableDocket.WD_OH_Client = client2.PK;

			if (pickableDocket.IsRecalculateOrderPricing)
			{
				AssertEquals("Should calculate extended line price", 10m * 20m, orderLine.WE_ExtendedLinePrice);
			}
			else
			{
				AssertEquals("Should not calculate extended line price", 0m, orderLine.WE_ExtendedLinePrice);
			}
		}

		protected void SetLineOrderUnitAndPrice(WhsPickableDocketLine line, ZDecimal unit, ZDecimal unitPrice)
		{
			if (line.WE_TransactionQuantity != unit)
			{
				line.WE_TransactionQuantity = unit;
			}

			if (line.WE_UnitPriceAfterDiscount != unitPrice)
			{
				line.WE_UnitPriceAfterDiscount = unitPrice;
			}
		}

		#endregion

		#region TestMinimumShelfLife

		public void TestMinimumShelfLife_JulianBatchNumberPartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var consignee = Helper.CreateClient("CONSIGNEE");

			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;

			AssertEquals((ZShort)0, docketLine.MinimumShelfLife);

			docket.ConsigneePK = consignee.PK;
			AssertEquals((ZShort)0, docketLine.MinimumShelfLife);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertEquals((ZShort)10, docketLine.MinimumShelfLife);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			AssertEquals("Value remains as expiry date is still enabled on the product after attribute type is updated to non-julian batch number.",
				(ZShort)10, docketLine.MinimumShelfLife);
		}

		public void TestMinimumShelfLife_ExpiryDatePartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			var consignee = Helper.CreateClient("CONSIGNEE");

			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;

			AssertEquals((ZShort)0, docketLine.MinimumShelfLife);

			docket.ConsigneePK = consignee.PK;
			AssertEquals((ZShort)0, docketLine.MinimumShelfLife);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertEquals((ZShort)10, docketLine.MinimumShelfLife);
		}

		public void TestMinimumShelfLife_Fallbacks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var docket = GetNewWhsDocket();
			var docketLine = docket.Lines.AddNew();
			AssertEquals("No error when client, warehouse and product not set.", ZShort.Zero, docketLine.MinimumShelfLife);

			docket.WD_OH_Client = data.Org1.PK;
			AssertEquals("No error when warehouse and product not set.", ZShort.Zero, docketLine.MinimumShelfLife);

			docket.WD_WW_Whs = data.Whs1.PK;
			AssertEquals("No error when product not set.", ZShort.Zero, docketLine.MinimumShelfLife);

			docketLine.WE_OP = data.Part1.PK;
			AssertEquals("No error when min shelf life not set on product.", ZShort.Zero, docketLine.MinimumShelfLife);

			var orgPartRelationOwner = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			orgPartRelationOwner.OU_ConsigneeMinShelfLifeAccepted = 7;
			AssertEquals((ZShort)7, docketLine.MinimumShelfLife);

			var consignee = Helper.CreateClient("CONSIGNEE");
			docket.ConsigneePK = consignee.PK;
			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 0;
			var orgPartRelationConsignee = Helper.CreateProductClientRelationShip(consignee, data.Part1, "WCN");
			AssertEquals("Precondition", ZShort.Zero, consignee.MiscServ.OM_MinimumShelfLifeAccepted);
			AssertEquals((ZShort)7, docketLine.MinimumShelfLife);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertEquals("Consignee should override the product value.", (ZShort)10, docketLine.MinimumShelfLife);

			orgPartRelationConsignee.OU_ConsigneeMinShelfLifeAccepted = 5;
			AssertEquals("Warehouse consignee should override consignee and product value.", (ZShort)5, docketLine.MinimumShelfLife);

			orgPartRelationConsignee.OU_ConsigneeMinShelfLifeAccepted = 0;
			AssertEquals("Back to consignee value.", (ZShort)10, docketLine.MinimumShelfLife);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 0;
			AssertEquals("Back to product value.", (ZShort)7, docketLine.MinimumShelfLife);

			orgPartRelationOwner.OU_ConsigneeMinShelfLifeAccepted = 8;
			AssertEquals("Back to product value.", (ZShort)8, docketLine.MinimumShelfLife);
		}

		#endregion

		#region TestIsUpdateForOrderPickingStatusAllowed

		public void TestIsUpdateForOrderPickingStatusAllowed()
		{
			TestIsUpdateForOrderPickingStatusAllowedCore();
		}

		protected abstract void TestIsUpdateForOrderPickingStatusAllowedCore();

		#endregion

		#endregion

		#region TestConstraints

		#region TestConstraint_WE_CurrentInventoryStatus

		[ExpectNoExceptions]
		public void TestConstraint_WE_CurrentInventoryStatus_EnsureIsEmpty()
		{
			var expectedExceptionMsg = "The UPDATE statement conflicted with the CHECK constraint \"Constraint_WE_CurrentInventoryStatus";
			var pickableLine = SetupDocketLineForCurrentInventoryStatusConstraintTest();

			AssertEquals("Expected empty current inventory status", ZString.Empty, pickableLine.WE_CurrentInventoryStatus);
			Factory.Save();

			pickableLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			AssertEquals("Expected 'AVL' current inventory status.", InventoryStatus.Codes.Available, pickableLine.WE_CurrentInventoryStatus);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Exception expected from Constraint_WE_CurrentInventoryStatus");
		}

		protected override TDocketLine SetupDocketLineForCurrentInventoryStatusConstraintTest()
		{
			var docketLine = base.SetupDocketLineForCurrentInventoryStatusConstraintTest();
			docketLine.Docket.WD_RequiredDate = ZDateTimeOffset.Now;

			return docketLine;
		}

		#endregion

		#endregion

		#region Flags

		#region TestCanCrossDockInventory

		public void TestCanCrossDockInventory()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			AssertEquals(ExpectedCanCrossDockInventory, docketLine.CanCrossDockInventory);
		}

		protected abstract bool ExpectedCanCrossDockInventory { get; }

		#endregion

		#region TestCanGenerateChildWorkOrder

		public void TestCanGenerateChildWorkOrder()
		{
			TestCanGenerateChildWorkOrderCore();
		}

		protected abstract void TestCanGenerateChildWorkOrderCore();

		#endregion

		#region TestIsComponentLineOnSalesOrder

		public void TestIsComponentLineOnSalesOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Today;
			pickableDocket.ConsigneePK = Helper.CreateClient("CONSIGNEE").PK;
			var orderLine = Helper.CreateWhsPickableDocketLine(pickableDocket, mainProduct, 15m);
			if (orderLine is WhsComponentOrderLine && orderLine.ChildComponentLines.Count == 0)
			{
				var componentLine = pickableDocket.Lines.AddNew();
				componentLine.WE_OP = bomComponentProduct.PK;
				componentLine.WE_TransactionQuantity = 45m;
				componentLine.WE_WE_ParentDocketLine = orderLine.PK;
			}

			Factory.Save();

			Helper.CreatePickNew(orderLine.PickableDocket);
			var childLine = orderLine.ChildComponentLines.Single();
			AssertEquals("Is child Line for this Pickable Docket a Component Line for Sales Order.",
				IsChildOrderLineAComponentLineOnSalesOrder, childLine.IsComponentLineOnSalesOrder);
		}

		protected abstract bool IsChildOrderLineAComponentLineOnSalesOrder { get; }

		#endregion

		#region TestIsDangerousGoods

		public void TestIsDangerousGood_IsDangerousGoods()
		{
			TestIsDangerousGood_Core(true);
		}

		public void TestIsDangerousGood_NotDangerousGoods()
		{
			TestIsDangerousGood_Core(false);
		}

		public void TestIsDangerousGood_Core(bool isDangerousGood)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			if (isDangerousGood)
			{
				data.Part1.UNDGs.AddNew();
			}
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			// Assert
			AssertEquals(isDangerousGood, orderLine.IsDangerousGood);
		}

		public void TestIsDangerousGood_NullProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_OP = ZGuid.Empty;
			AssertEquals(false, orderLine.IsDangerousGood);
		}

		#endregion

		#region TestIsPickLinesLoaded

		public void TestIsPickLinesLoaded()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Today;
			pickableDocket.ConsigneePK = Helper.CreateClient("CONSIGNEE").PK;

			var orderLine1 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 15m);
			AssertEquals("PickLines should not be considered Loaded if the Collection has not been instantiated.", false, orderLine1.IsPickLinesLoaded);

			var pick = Factory.New<WhsPick>();
			pickableDocket.WD_WP = pick.PK;
			var poke1 = orderLine1.PickLines;
			AssertEquals("PickLines should be considered Loaded if the Collection has been instantiated.", true, orderLine1.IsPickLinesLoaded);

			var pickLine1 = orderLine1.PickLines.AddNew();
			pickLine1.WZ_Units = 1m;
			pickLine1.WZ_WE_TransactionLine = orderLine1.PK;

			var orderLine2 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 10m);
			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine2.WZ_Units = 1m;
			pickLine2.WZ_WE_TransactionLine = orderLine2.PK;
			AssertEquals("Precondition: PickLines Collection is not loaded.", false, orderLine2.IsPickLinesLoaded);

			var poke2 = orderLine2.PickLines;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, orderLine1.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, orderLine2.PickLines);
			AssertEquals("PickLines should be considered Loaded if the Collection has been instantiated.", true, orderLine2.IsPickLinesLoaded);
		}

		#endregion

		#region TestIsPartiallyOrFullyPickedFromPutawayLocation

		public void TestIsPartiallyOrFullyPickedFromPutawayLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 100m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_PickOption = WhsPickOption.Codes.Manual;
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Today;
			pickableDocket.ConsigneePK = Helper.CreateClient("CONSIGNEE").PK;
			var orderLine1 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 15m);
			Factory.Save();

			orderLine1.SetShortfallForTest(0);
			var pick = Helper.CreatePickNew(pickableDocket);
			AssertEquals("When order line have no pick lines attached it is not considered picked.", false, orderLine1.IsPartiallyOrFullyPickedFromPutawayLocation);

			var pickLine = Helper.CreateWhsPickLine(orderLine1, receive.Inventory[0], 15m);
			AssertEquals("When order line have not picked pick line, then it is still not considered picked.", false, orderLine1.IsPartiallyOrFullyPickedFromPutawayLocation);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("When order line have picked pick line, then it is considered picked.", true, orderLine1.IsPartiallyOrFullyPickedFromPutawayLocation);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition.", false, orderLine1.IsPartiallyOrFullyPickedFromPutawayLocation);

			pickLine.WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;
			AssertEquals("When pickableDocket have In-Transit pick line attached to its lines, then it is considered picked from Putaway Location.", true, orderLine1.IsPartiallyOrFullyPickedFromPutawayLocation);
		}

		#endregion

		#region TestIsCurrentlyBeingPickedFromPutawayLocation

		public void TestIsCurrentlyBeingPickedFromPutawayLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 100m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_PickOption = WhsPickOption.Codes.Manual;
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Today;
			pickableDocket.ConsigneePK = Helper.CreateClient("CONSIGNEE").PK;
			var orderLine1 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 15m);
			Factory.Save();

			orderLine1.SetShortfallForTest(0);
			var pick = Helper.CreatePickNew(pickableDocket);
			AssertEquals("An order line with no pick lines attached should not be considered as currently being picked.", false, orderLine1.IsCurrentlyBeingPickedFromPutawayLocation);

			var pickLine = Helper.CreateWhsPickLine(orderLine1, receive.Inventory[0], 15m);
			AssertEquals("An order line with a pick line that is not being picked should not be considered as currently being picked.", false, orderLine1.IsCurrentlyBeingPickedFromPutawayLocation);

			pickLine.WZ_GS_NKAssignedTo = "AAA";
			pickLine.WZ_IsPicking = true;
			AssertEquals("An order line with a pick line that is being picked should be considered as currently being picked.", true, orderLine1.IsCurrentlyBeingPickedFromPutawayLocation);
		}

		#endregion

		#endregion

		#region TestClearReleaseLines

		public void TestClearReleaseLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Now;
			pickableDocket.ConsigneePK = data.Org1.PK;

			var orderLine = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 20m);
			Helper.CreatePickNew(pickableDocket);
			AssertEquals("Release Lines Collection is registered child editable.", true, orderLine.IsRegisteredEditableChildObject(orderLine.ReleaseLines));

			orderLine.ClearReleaseLines();
			AssertEquals("When Release Lines Collection is cleared, it is unhooked until the collection is rebuilt.",
				false, orderLine.IsRegisteredEditableChildObject(orderLine.ReleaseLines));
		}

		#endregion

		#region TestCancelLine

		public void TestCancelLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			var pickableDocketLine1 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 10m);
			var pickableDocketLine2 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part2, 10m);
			var reservedPickLine1 = Helper.CreateReservePickLine(pickableDocketLine1, receive1.Inventory[0], 5m);
			var reservedPickLine2 = Helper.CreateReservePickLine(pickableDocketLine2, receive2.Inventory[0], 7m);
			AssertEquals("Precondition", false, pickableDocket.IsCancelled);
			AssertEquals("Precondition", false, reservedPickLine1.IsDeleted);
			AssertEquals("Precondition", false, reservedPickLine2.IsDeleted);

			pickableDocket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Should *only* delete reserved pick lines on Cancel.", false, reservedPickLine1.IsDeleted);
			AssertEquals("Should *only* delete wrong pick lines on Cancel.", false, reservedPickLine2.IsDeleted);

			pickableDocketLine1.CancelLine();
			AssertEquals("Should delete reserved pick lines on Cancel.", true, reservedPickLine1.IsDeleted);
			AssertEquals("Should not delete reserved pick lines for other docket line.", false, reservedPickLine2.IsDeleted);

			pickableDocketLine2.CancelLine();
			AssertEquals("Should now delete reserved pick lines for other docket line.", true, reservedPickLine2.IsDeleted);
		}

		#endregion

		#region Delete

		#region TestDelete_ReleaseLines

		public void TestDelete_ReleaseLines()
		{
			TestDelete_ReleaseLinesCore();
		}

		protected virtual void TestDelete_ReleaseLinesCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Now;
			pickableDocket.ConsigneePK = data.Org1.PK;

			var orderLine = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 20m);

			if (orderLine is WhsComponentOrderLine && orderLine.ChildComponentLines.Count == 0)
			{
				var childLine = pickableDocket.Lines.AddNew();
				childLine.WE_OP = data.Part2.PK;
				childLine.WE_TransactionQuantity = 20m;
				childLine.WE_WE_ParentDocketLine = orderLine.PK;
			}

			Helper.CreatePickNew(pickableDocket);

			var pickableDocketLine = orderLine.ReleaseLines.Count > 0 ? orderLine : orderLine.ChildComponentLines.ElementAt(0);
			AssertEquals("There should be one Release Line.", 1, pickableDocketLine.ReleaseLines.Count);

			pickableDocketLine.Delete();
			using (pickableDocketLine.ReleaseLines.SuspendRebuild())
			{
				AssertEquals("Release Lines should be cleared when Order Line is deleted.", 0, pickableDocketLine.ReleaseLines.Count);
			}

			AssertEquals("Release Lines should be cleared when Order Line is deleted.", 0, pickableDocketLine.ReleaseLines.Count);
		}

		#endregion

		#region TestDelete_ReservedPickLines

		public void TestDelete_ReservedPickLines()
		{
			var docketLine = DocketLine;
			var pickLine1 = Helper.CreateReservePickLine(docketLine, Factory.NewWithValidTestData<WhsInventoryView>(), 1m);
			var pickLine2 = Helper.CreateReservePickLine(docketLine, Factory.NewWithValidTestData<WhsInventoryView>(), 1m);

			docketLine.Delete();
			AssertEquals(true, pickLine1.IsDeleted);
			AssertEquals(true, pickLine2.IsDeleted);
		}

		#endregion

		#region TestDelete_Inventory

		protected override bool CanHaveInventoryAttached => false;

		#endregion

		#endregion

		#region TestUpdateWP_CriticalChangesVersionID_ExceptionFields

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_NotInExceptionList()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docketLine) => docketLine.WE_CustomAttrib1 = docketLine.WE_CustomAttrib1 = "New", true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_SystemCreateUser()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docketLine) => docketLine.WE_SystemCreateUser = "S2", false);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_SystemCreateTimeUtc()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docketLine) => docketLine.WE_SystemCreateTimeUtc = ZDateTime.Now, false);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_SystemLastEditUser()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docketLine) => docketLine.WE_SystemLastEditUser = "S2", false);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_SystemLastEditTimeUtc()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docketLine) => docketLine.WE_SystemLastEditTimeUtc = ZDateTime.Now, false);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_LineNo()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docketLine) => docketLine.WE_LineNo = 100, false);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_SubLineNo()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docketLine) => docketLine.WE_SubLineNo = 101, false);
		}

		void AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore(Action<WhsPickableDocketLine> editOrderLine, bool expectedToUpdateVersion)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();

			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var line = Helper.CreateWhsPickableDocketLine(docket, data.Part1, 100m);

			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(docket);
			Factory.Save();

			var currentPickVersionInDB = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
			editOrderLine(line);
			Factory.Save();

			var pickInNewFactory = NewFactory().Load<WhsPick>(pick.PK);
			if (expectedToUpdateVersion)
			{
				AssertNotEquals("Should update pick version.", currentPickVersionInDB, pickInNewFactory.WP_CriticalChangesVersionID);
				AssertEquals("Pick version should be valid.", true, pickInNewFactory.WP_CriticalChangesVersionID.IsValid);
			}
			else
			{
				AssertEquals("Should update pick version.", currentPickVersionInDB, pickInNewFactory.WP_CriticalChangesVersionID);
			}
		}

		#endregion

		#region Location

		protected override void TestLocationStringCore()
		{
			Assert("Location not used by Order Lines", true);
		}

		protected override void TestLocationString_FixedWidthLocationCore()
		{
			Assert("Location not used by Order Lines", true);
		}

		#endregion

		#region TestIWhsPickableDocketLine

		public void TestIWhsPickableDocketLine()
		{
			TestIWhsPickableDocketLineCore();
		}

		protected virtual void TestIWhsPickableDocketLineCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 6m);
			Factory.Save();

			var docket = GetNewWhsDocket();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var docketLine = Helper.CreateWhsPickableDocketLine(docket, data.Part1, 10m);
			AssertEquals(10m, docketLine.QuantityNotMet);
			AssertEquals(10m, ((IWhsPickableDocketLine)docketLine).QuantityNotMet);

			Helper.CreatePickNew(docket);
			AssertEquals(6m, docketLine.SumOfUnitsMet);
			AssertEquals(4m, docketLine.QuantityNotMet);
			AssertEquals(4m, ((IWhsPickableDocketLine)docketLine).QuantityNotMet);
		}

		#endregion

		#region TestGetQuantityFromComponents

		public void TestGetQuantityFromComponents()
		{
			TestGetQuantityFromComponentsCore();
		}

		protected virtual void TestGetQuantityFromComponentsCore()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();

			AssertEquals(10, kitOrderLine1.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, wheelOrderLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, wheelOrderLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
		}

		#endregion

		#region Implementation

		#region	IProcessHandlingInfoProvider Members

		protected override Type ExpectedDocketLineHandlingInfoProvider => typeof(WhsPickableDocketLineProcessHandlingInfo);

		#endregion

		protected override string ExpectedDefaultInventoryStatus => string.Empty;

		#endregion
	}
}
