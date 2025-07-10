using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsReleaseLine_ReducePickedStockTest<T> : TestCaseWithFactory where T : class, IPickedStockAdjuster
	{
		#region TestReducePickedStock_MoreThanPickedReturnsError

		public void TestReducePickedStock_MoreThanPickedReturnsError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			MakePickLinesPicked(orderLine.PickLines);
			Factory.Save();

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 11m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals("Quantity must not be greater that Quantity Met.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_ToZeroOrNegativeReturnsError

		public void TestReducePickedStock_ToZeroOrNegativeReturnsError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			MakePickLinesPicked(orderLine.PickLines);
			Factory.Save();

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 0m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals("Quantity must be a positive value.", result.ErrorMessage);

			result = ReducePickedStock(orderLine.ReleaseLines[0], -1m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals("Quantity must be a positive value.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_OnUnpickedPickLinesReturnsError

		public void TestReducePickedStock_OnUnpickedPickLinesReturnsError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			Factory.Save();

			AssertEquals("Precondition: Pickline can't be lost (as it is not picked)", false, ((IReducibleItem)orderLine.PickLines.Single()).CanBeReduced);

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 10m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals($"Stock must be picked or not packed to '{FunctionDescription}'.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_TooMuchOnPartiallyPickedPickLineReturnsError

		public void TestReducePickedStock_TooMuchOnPartiallyPickedPickLineReturnsError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			MakePickLinesPicked(orderLine.PickLines.Where(pl => pl.WZ_Units == 6m));
			Factory.Save();

			var result = ReducePickedStock(releaseLine, 7m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals("Quantity (7) cannot be greater than Quantity Picked (6).", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_ThrowsErrorIfPickIsFinalised

		public void TestReducePickedStock_ThrowsErrorIfPickIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			MakePickLinesPicked(orderLine.PickLines);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			Factory.Save();

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 7m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals($"Cannot '{FunctionDescription}' on a finalized Pick.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_ThrowsErrorIfOrderStatusIsLDG

		public void TestReducePickedStock_ThrowsErrorIfOrderStatusIsLDG()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			Factory.Save();

			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loading, GetOrderStatusForOrderPK(order.PK));

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 7m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals($"Order is Loading, Loaded or Departed, cannot '{FunctionDescription}'.", result.ErrorMessage);
		}

		string GetOrderStatusForOrderPK(ZGuid orderPK, bool newFactory = false)
		{
			var query = new ZQuery(WhsOrderStatusViewSchema.PK, orderPK);
			var factory = newFactory ? new BusinessObjectFactory() : Factory;
			var whsOrderStatusView = factory.Load<WhsOrderStatusView>(query).Single();
			return whsOrderStatusView.WOS_OrderStatus;
		}

		#endregion

		#region TestReducePickedStock_ThrowsErrorIfOrderStatusIsLOA

		public void TestReducePickedStock_ThrowsErrorIfOrderStatusIsLOA()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loaded, GetOrderStatusForOrderPK(order.PK));

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 7m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals($"Order is Loading, Loaded or Departed, cannot '{FunctionDescription}'.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_ThrowsErrorIfOrderStatusIsDEP

		public void TestReducePickedStock_ThrowsErrorIfOrderStatusIsDEP()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Factory.Save();

			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Departed, GetOrderStatusForOrderPK(order.PK));

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 7m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals($"Order is Loading, Loaded or Departed, cannot '{FunctionDescription}'.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_ThrowsErrorIfPickIsFinalised_NewFactory

		public void TestReducePickedStock_ThrowsErrorIfPickIsFinalised_NewFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			MakePickLinesPicked(orderLine.PickLines);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			pickInNewFactory.FinaliseAllOrders();
			pickInNewFactory.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pickInNewFactory);
			newFactory.Save();

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 7m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals($"Another user has finalized the Pick, cannot '{FunctionDescription}'.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_ThrowsErrorIfRelatedDataHasChanged_NewFactory

		public void TestReducePickedStock_ThrowsErrorIfRelatedDataHasChanged_NewFactory_Returned()
		{
			TestReducePickedStock_ThrowsErrorIfRelatedDataHasChanged_NewFactory(ReduceStockReason.Returned);
		}
		public void TestReducePickedStock_ThrowsErrorIfRelatedDataHasChanged_NewFactory_Lost()
		{
			TestReducePickedStock_ThrowsErrorIfRelatedDataHasChanged_NewFactory(ReduceStockReason.Lost);
		}

		void TestReducePickedStock_ThrowsErrorIfRelatedDataHasChanged_NewFactory(ReduceStockReason reduceStockReason)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			MakePickLinesPicked(orderLine.PickLines);
			Factory.Save();

			var currentPickVersionID = pick.WP_CriticalChangesVersionID;
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var packageJob = orderInNewFactory.PackageJob;
			var package = packageJob.Packages.AddNew("BOX");
			package.Pack((WhsReleaseLine)orderInNewFactory.Lines[0].ReleaseLines.Single(), 10m);
			newFactory.Save();

			using (MockAdjustmentCreation(ActionResult.Failure("fail")))
			{
				var pickedStockAdjustersFactory = ObjectFactory.Get<IPickedStockAdjustersFactory>();
				var pickedStockAdjuster = pickedStockAdjustersFactory.GetNewAdjuster(order.Warehouse, order.Client, reduceStockReason);
				pickedStockAdjuster.LinkToDocket(order);
				var reason = pickedStockAdjustersFactory.GetDescription(reduceStockReason);
				var factory = new BusinessObjectFactory();
				var result = WhsReleaseLine.AdjustOutInventoryAndSaveInOtherFactory(orderLine.PK, WhsReleaseLineCollection.GetKey(releaseLine), 7m, pickedStockAdjuster, reason, factory);
				AssertEquals("Should report a failure", false, result.IsSuccess);
				AssertStartsWith("Error should start with this message", $"Another user has modified related data, cannot", result.ErrorMessage);
				var (adjustResult, pickCriticalChangesVersionID) = ReleaseLineReductionManager.AdjusterSave(pickedStockAdjuster, order, factory);
				AssertNull("Should not return pick version id to update.", pickCriticalChangesVersionID);
			}
		}

		#endregion

		#region TestReducePickedStock_PickCriticalChangesVersionID

		public void TestReducePickedStock_PickCriticalChangesVersionID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			Factory.Save();
			var pickVersion = pick.WP_CriticalChangesVersionID;

			using (MockAdjustmentCreation(ActionResult.Failure("fail")))
			{
				AssertEquals("Precondition:", pickVersion, NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);
				ReducePickedStock(releaseLine, 7m);

				var pickVersionIDInDB = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
				AssertEquals("Should *not* update pick version!", pickVersion, pickVersionIDInDB);
				AssertEquals("Should *not* update pick version!", pick.WP_CriticalChangesVersionID, pickVersionIDInDB);
				AssertEquals("Should *not* update pick version!", false, pick.WP_CriticalChangesVersionIDInfo.HasChanges);
			}

			using (MockAdjustmentCreation())
			{
				AssertEquals("Precondition:", pickVersion, NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);
				ReducePickedStock(releaseLine, 7m);

				var pickVersionIDInDB = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
				AssertNotEquals("Should update pick version!", pickVersion, pickVersionIDInDB);
				AssertEquals("Should update pick version in local factory!", pick.WP_CriticalChangesVersionID, pickVersionIDInDB);
				AssertEquals("Should update pick version in local factory as original value!", false, pick.WP_CriticalChangesVersionIDInfo.HasChanges);
			}
		}

		#endregion

		#region TestReducePickedStock_ThrowsErrorIfChangesAreNotSaved

		public void TestReducePickedStock_ThrowsErrorIfChangesAreNotSaved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			Factory.Save();

			MakePickLinesPicked(orderLine.PickLines);

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 7m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals($"Save pick before attempting to '{FunctionDescription}'.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_ThrowsErrorIfReleaseLineDoesNotExistInOtherFactory

		public void TestReducePickedStock_ThrowsErrorIfReleaseLineDoesNotExistInOtherFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			Factory.Save();

			MakePickLinesPicked(orderLine.PickLines);

			// this test is simply to test when the release line does not exist in the other factory,
			// we don't care how the pickline gets picked.
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine.PK).PickLines.Single();
			using (((IWhsPickLineInternals)pickLineInOtherFactory).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				pickLineInOtherFactory.InventoryLine.WE_StockOnHand += pickLineInOtherFactory.WZ_Units;
				pickLineInOtherFactory.Delete();
				otherFactory.Save();
			}

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 7m);
			AssertEquals("Should report a failure.", false, result.IsSuccess);
			AssertEquals($"Another user has modified the Allocated Stock on this Order.\r\nClose and re-open the form, then attempt to '{FunctionDescription}' again.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_ThrowsErrorWhenSentUnitsIsLessThanPacked

		public void TestReducePickedStock_ThrowsErrorWhenSentUnitsIsLessThanPacked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();

			var package = order.PackageJob.Packages.AddNew("BOX");
			package.Pack(releaseLine, 7m);
			Factory.Save();

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 4m);
			AssertEquals("Should report a failure", false, result.IsSuccess);
			AssertEquals("Not enough units are unpacked. Unpack goods first.", result.ErrorMessage);
		}

		#endregion

		#region TestReducePickedStock_SuccessWhenSentUnitsIsEqualToUnPacked

		public void TestReducePickedStock_SuccessWhenSentUnitsIsEqualToUnPacked()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now, data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", now, data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();

			var package = order.PackageJob.Packages.AddNew("BOX");
			package.Pack(releaseLine, 1m);
			Factory.Save();

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 1m);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}.", true, result.IsSuccess);
			AssertEquals(1m, order.PackableItemParents.Typed.Sum(p => p.GetPackedQty()));
		}

		#endregion

		#region TestReducePickedStock_SuccessWhenSentUnitsIsEqualToUnPacked_WithRCAPacked

		public void TestReducePickedStock_SuccessWhenSentUnitsIsEqualToUnPacked_WithRCAPacked()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now, data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", now, data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Helper.CreatePickNew(order);

			var releaseLine1 = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			releaseLine1.Quantity = 1m;
			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.SerialNumber = "SN00001";
			releaseLine2.Quantity = 1m;

			var package = order.PackageJob.Packages.AddNew("BOX");
			package.Pack(releaseLine2, 1m);

			MakePickLinesPicked(orderLine.PickLines);

			Factory.Save();

			AssertEquals("Reduce Picked Stock should not work for Packed Release Line.", false, ReducePickedStock(releaseLine2, 1m).IsSuccess);

			var result = ReducePickedStock(releaseLine1, 1m);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}.", true, result.IsSuccess);
			AssertEquals(1m, order.PackableItemParents.Typed.Sum(p => p.GetPackedQty()));
		}

		#endregion

		#region TestReducePickedStock_SuccessWhenSentUnitsIsEqualToUnPacked_WithNonRCAPacked

		public void TestReducePickedStock_SuccessWhenSentUnitsIsEqualToUnPacked_WithNonRCAPacked()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now, data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", now, data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Helper.CreatePickNew(order);

			var releaseLine1 = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			releaseLine1.Quantity = 1m;
			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.SerialNumber = "SN00001";
			releaseLine2.Quantity = 1m;

			var package = order.PackageJob.Packages.AddNew("BOX");
			package.Pack(releaseLine1, 1m);

			MakePickLinesPicked(orderLine.PickLines);

			Factory.Save();

			AssertEquals("Reduce Picked Stock should not work for Packed Release Line.", false, ReducePickedStock(releaseLine1, 1m).IsSuccess);

			var result = ReducePickedStock(releaseLine2, 1m);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}.", true, result.IsSuccess);
			AssertEquals(1m, order.PackableItemParents.Typed.Sum(p => p.GetPackedQty()));
		}

		#endregion

		#region TestReducePickedStock_DoesWorkForBOMKits

		public void TestReducePickedStock_DoesWorkForBOMKits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 2m, Constants.PkgUnit.Unit);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 6m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Should be 1 Release Line representing Built Kits + Already made Kits.", 1, orderLine.ReleaseLines.Count);
			MakePickLinesPicked(pick.GetAllPickLines().Where(l => !l.IsPickByBOMKitPickLine()));
			Factory.Save();

			var result = ReducePickedStock(orderLine.ReleaseLines[0], 1m);
			orderLine.BuildReleaseLines();
			AssertEquals("Should report a success", true, result.IsSuccess);
			AssertEquals("Should have successfully Reduced Stock.", 9m, orderLine.ReleaseLines[0].Quantity);
		}

		#endregion

		#region TestReducePickedStock_UpdatesQuantityMet

		public void TestReducePickedStock_UpdatesQuantityMet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			Factory.Save();

			using (MockAdjustmentCreation())
			{
				AssertEquals("Precondition:", 10m, releaseLine.Quantity);
				ReducePickedStock(releaseLine, 7m);

				releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
				AssertEquals("Quantity Met should be reduced.", 3m, releaseLine.Quantity);
			}
		}

		#endregion

		#region TestReducePickedStock_UpdatesQuantityMet_InTransit

		public void TestReducePickedStock_UpdatesQuantityMet_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var originalPickLine = orderLine.PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(originalPickLine, ZDateTimeOffset.Now);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: PickLine is In-Transit.", true, pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid);
			Factory.Save();

			using (MockAdjustmentCreation())
			{
				AssertEquals("Precondition:", 10m, releaseLine.Quantity);
				var result = ReducePickedStock(releaseLine, 7m);
				AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}.", true, result.IsSuccess);

				releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
				AssertEquals("Quantity Met should be reduced.", 3m, releaseLine.Quantity);
			}
		}

		#endregion

		#region TestReducePickedStock_UpdatesPickLine

		public void TestReducePickedStock_UpdatesPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var pickLine = orderLine.PickLines.Single();
			Factory.Save();

			using (MockAdjustmentCreation())
			{
				AssertEquals("Precondition:", 10m, pickLine.WZ_Units);
				ReducePickedStock(releaseLine, 7m);
				AssertEquals("Pickline quantity should be changed.", 3m, pickLine.WZ_Units);
				AssertEquals("Pickline should be saved immediately.", false, pickLine.HasChanges);
			}
		}

		#endregion

		#region TestReducePickedStock_UpdatesSentValuesOnOrder

		public void TestReducePickedStock_UpdatesSentValuesOnOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			Helper.SetProductWeightAndVolume(data.Part1, 4m, "KG", 1.1m, "M3");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_TotalCubicUnit = "M3";
			order.WD_TotalWeightUnit = "KG";

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			Factory.Save();

			var releaseLine = orderLine.ReleaseLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Precondition:", 3m, releaseLine.Quantity);
				AssertEquals("Precondition:", 3m, order.WD_UnitsSent);
				AssertEquals("Precondition:", 3.3m, order.WD_CubicSent);
				AssertEquals("Precondition:", 12m, order.WD_WeightSent);
				AssertEquals("Precondition:", 12m, order.WD_WeightSentUserEntered);
			});

			using (MockAdjustmentCreation())
			{
				ReducePickedStock(releaseLine, 1m);
			}
			CombineAssertions(() =>
			{
				AssertEquals("Fields on the order should be updated.", 2m, order.WD_UnitsSent);
				AssertEquals("Fields on the order should be updated.", 2.2m, order.WD_CubicSent);
				AssertEquals("Fields on the order should be updated.", 8m, order.WD_WeightSent);
				AssertEquals("Fields on the order should be updated.", 8m, order.WD_WeightSentUserEntered);
			});
		}

		#endregion

		#region TestReducePickedStock_UpdatesRCAandPickLine

		public void TestReducePickedStock_UpdatesRCAandPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLineRed = orderLine.ReleaseLines[0];
			releaseLineRed.PartAttribute1 = "RED";
			releaseLineRed.Quantity = 4m;
			var releaseLineBlue = orderLine.ReleaseLines.AddNew();
			releaseLineBlue.PartAttribute1 = "BLUE";
			releaseLineBlue.Quantity = 6m;

			MakePickLinesPicked(orderLine.PickLines);
			Factory.Save();

			using (MockAdjustmentCreation())
			{
				AssertEquals("Precondition:", 10m, orderLine.PickLines.Sum(l => l.WZ_Units));
				AssertEquals("Precondition:", 2, orderLine.PickLines.Count);
				ReducePickedStock(releaseLineRed, 3m);
				AssertEquals("Pickline quantity should be changed.", 7m, orderLine.PickLines.Sum(l => l.WZ_Units));
				var redPickLine = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED");
				var bluePickLine = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE");
				AssertEquals("RCA-RED quantity should be changed.", 1m, redPickLine.WZ_Units);
				AssertEquals("RCA-BLUE quantity should NOT be changed.", 6m, bluePickLine.WZ_Units);
				AssertEquals("Pickline should be saved immediately.", false, redPickLine.HasChanges);
				AssertEquals("Pickline should be saved immediately.", false, bluePickLine.HasChanges);
			}
		}

		#endregion

		#region TestReducePickedStock_MixRCAandNonRCA

		public void TestReducePickedStock_MixRCAandNonRCA()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, use: true, setReleaseCaptured: true);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false, "Size");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, use: true, setReleaseCaptured: false);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.Lines[0].WE_PartAttrib2 = "M";
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLineRed = orderLine.ReleaseLines[0];
			releaseLineRed.PartAttribute1 = "RED";
			releaseLineRed.Quantity = 4m;
			var releaseLineBlue = orderLine.ReleaseLines.AddNew();
			releaseLineBlue.PartAttribute1 = "BLUE";
			releaseLineBlue.PartAttribute2 = "M";
			releaseLineBlue.Quantity = 6m;

			MakePickLinesPicked(orderLine.PickLines);
			Factory.Save();

			var lastAllocatedTime = ZDateTime.UtcNow.AddDays(-1);
			var location = receive.Lines[0].Location;
			location.WLV_LastAllocatedOrChangedDateUtc = lastAllocatedTime;
			Factory.Save();

			AssertEquals("Precondition:", 10m, orderLine.PickLines.Sum(l => l.WZ_Units));
			AssertEquals("Precondition:", 2, orderLine.PickLines.Count);

			ReducePickedStock(releaseLineRed, 3m);
			AssertEquals("Pickline quantity should be changed.", 7m, orderLine.PickLines.Sum(l => l.WZ_Units));
			var redPickLine = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED");
			var bluePickLine = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE");
			AssertEquals("RCA-RED quantity should be changed.", 1m, redPickLine.WZ_Units);
			AssertEquals("RCA-BLUE quantity should NOT be changed.", 6m, bluePickLine.WZ_Units);
			AssertEquals("Pickline should be saved immediately.", false, redPickLine.HasChanges);
			AssertEquals("Pickline should be saved immediately.", false, bluePickLine.HasChanges);

			var docket = (WhsDocket)order.RelatedJobs.SingleOrDefault();
			var docketLine = docket.Lines.Single();
			AssertEquals("Attrib1 was release Captured so it should NOT be on the new line.", "", docketLine.WE_PartAttrib1);
			AssertEquals("Attrib2 was Not Release captured, so it should be on the new line.", "M", docketLine.WE_PartAttrib2);
			AssertEquals("OriginalOrderLine should get cloned.", orderLine.PK, docketLine.PickLines.Single().InventoryLine.PickLines.Single().WZ_WE_OriginalOrderLine);

			if (OnlyTouchesOutboundTransferLocation)
			{
				AssertEquals("LastAllocatedOrChanged date should not be affected when reducing picked stock.", lastAllocatedTime, location.WLV_LastAllocatedOrChangedDateUtc);
			}
		}

		#endregion

		#region TestReducePickedStock_UpdatesMultiplePickLines

		public void TestReducePickedStock_UpdatesMultiplePickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var inventory1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m).Inventory.Single();
			var inventory2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m).Inventory.Single();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			Factory.Save();

			using (MockAdjustmentCreation())
			{
				AssertEquals("Precondition:", 10m, orderLine.PickLines.Sum(pl => pl.WZ_Units));
				ReducePickedStock(releaseLine, 7m);
				AssertEquals("Picklines quantity should be changed.", 3m, orderLine.PickLines.Sum(pl => pl.WZ_Units));
			}
		}

		#endregion

		#region TestReducePickedStock_UpdatesCorrespondingPickLineAndCreatesAdjustmentOut_EndToEnd

		public void TestReducePickedStock_UpdatesCorrespondingPickLineAndCreatesAdjustmentOut_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var pickLine = orderLine.PickLines.Single();

			// this is an end to end test for directly Picked Pick Lines
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition:", 10m, pickLine.WZ_Units);
			AssertEquals("Precondition: Stock is reduced immediately.", 0m, pickLine.InventoryLine.WE_StockOnHand);
			ReducePickedStock(releaseLine, 7m);
			AssertEquals("Pickline quantity should be changed.", 3m, pickLine.WZ_Units);
			AssertEquals("Pickline should be saved immediately.", false, pickLine.HasChanges);

			var docket = (WhsDocket)order.RelatedJobs.SingleOrDefault();
			AssertNotNull("Created adjustment/transfer must be linked to the order.", docket);
			AssertDocketCreated(docket, 1, 7);
			AssertEquals("Stock is taken from originally committed inventory.", pickLine.InventoryLine, docket.Lines.Single().PickLines.Single().InventoryLine);
			AssertEquals("Stock should remain the same.", 0m, pickLine.InventoryLine.WE_StockOnHand);
		}

		#endregion

		#region TestReducePickedStock_UpdatesCorrespondingPickLineAndCreatesAdjustmentOut_InTransit_EndToEnd

		public void TestReducePickedStock_UpdatesCorrespondingPickLineAndCreatesAdjustmentOut_InTransit_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var transferLine = Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var pickLine = orderLine.PickLines.Single();
			var newPickLine = transferLine.PickLines.Single();
			AssertEquals("Precondition: PickLine is In-Transit.", true, pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid);
			AssertEquals("Precondition:", 10m, pickLine.WZ_Units);
			AssertEquals("Precondition: Stock is reduced immediately.", 0m, newPickLine.InventoryLine.WE_StockOnHand);
			Factory.Save();

			var lastAllocatedTime = ZDateTime.UtcNow.AddDays(-1);
			var location = newPickLine.InventoryLine.Location;
			location.WLV_LastAllocatedOrChangedDateUtc = lastAllocatedTime;
			Factory.Save();

			var result = ReducePickedStock(releaseLine, 7m);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true, result.IsSuccess);
			AssertEquals("Pickline quantity should be changed.", 3m, pickLine.WZ_Units);
			AssertEquals("Pickline should be saved immediately.", false, pickLine.HasChanges);

			var docket = (WhsDocket)order.RelatedJobs.SingleOrDefault();
			AssertNotNull("Created adjustment/transfer must be linked to the order.", docket);
			AssertDocketCreated(docket, 1, 7m, transferLine.WE_WL_TransferFrom);

			var splitPickLine = docket.Lines.Single().PickLines.Single();
			var inventoryLine = splitPickLine.InventoryLine;
			AssertNotEquals("Stock should be taken from cloned transfer line.", transferLine, inventoryLine);
			AssertEquals("Stock should be taken from cloned transfer line.", transferLine.WE_WD, inventoryLine.WE_WD);
			AssertEquals("Transaction Qty should be the Adjusted amount.", 7m, inventoryLine.WE_TransactionQuantity);
			AssertEquals("There should be no Stock on hand for the cloned transfer line.", 0m, inventoryLine.WE_StockOnHand);
			AssertEquals("Cloned Transfer Line should be finalised.", true, inventoryLine.IsFinalised);

			AssertEquals("Stock should be reduced.", 3m, transferLine.WE_StockOnHand);
			AssertEquals("Transaction Qty should be split.", 3m, transferLine.WE_TransactionQuantity);
			AssertEquals("Original transfer line's inventory status should be unchanged.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Original transfer line's inventory status should be unchanged.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("OriginalOrderLine should get cloned.", orderLine.PK, splitPickLine.InventoryLine.PickLines.Single().WZ_WE_OriginalOrderLine);

			if (OnlyTouchesOutboundTransferLocation)
			{
				AssertEquals("LastAllocatedOrChanged date should not be affected when reducing picked stock.", lastAllocatedTime, location.WLV_LastAllocatedOrChangedDateUtc);
			}
		}

		#endregion

		#region TestReducePickedStock_InTransitTransferFailsFinalisation

		public void TestReducePickedStock_InTransitTransferFailsFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var transferLine = Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: PickLine is In-Transit.", true, pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid);
			Factory.Save();

			var newPickLine = transferLine.PickLines.Single();
			AssertEquals("Precondition:", 10m, pickLine.WZ_Units);
			AssertEquals("Precondition: Stock is reduced immediately.", 0m, newPickLine.InventoryLine.WE_StockOnHand);

			var factoryStub = new Mock<IPickedStockAdjustersFactory>();
			var mockRecorder = new Mock<IPickedStockAdjuster>();
			factoryStub.Setup(s => s.GetNewAdjuster(It.IsAny<WhsWarehouse>(), It.IsAny<OrgHeader>(), It.IsAny<ReduceStockReason>())).Returns(mockRecorder.Object);
			mockRecorder.Setup(s => s.PrepareForSaving()).Returns(ActionResult.Success()).Verifiable();
			mockRecorder.Setup(s => s.LinkToDocket(It.IsAny<WhsPickableDocket>())).Callback<WhsPickableDocket>(m =>
			{
				var orderInOtherFactory = (WhsOrder)m;
				var transferLineInOtherFactory = (WhsTransferLine)orderInOtherFactory.Lines.Single().PickLines.Single().InventoryLine;

				// hack to make transfer line finalisation fail
				EventHandler testHook = null;
				testHook = (sender, e) =>
				{
					transferLineInOtherFactory.AddRowError("Test");
					transferLineInOtherFactory.WE_CurrentInventoryStatusInfo.ValueChanged -= testHook;
				};

				transferLineInOtherFactory.WE_CurrentInventoryStatusInfo.ValueChanged += testHook;
			}).Verifiable();

			using (ObjectFactory.Substitute(factoryStub.Object))
			{
				var result = ReducePickedStock(releaseLine, 10m);
				AssertEquals("Reduce Picked Stock should fail.", false, result.IsSuccess);
				AssertEquals("Reduce Picked Stock should have correct Error.", "Error - Docket Line: Test\n" +
"Error - Docket Line: Error occurred during finalization. Close the form without saving and try again.", result.ErrorMessage);
				mockRecorder.Verify(m => m.PrepareForSaving(), Times.Never);
				mockRecorder.Verify(m => m.AdjustOutInventory(It.IsAny<WhsInventoryView>(), It.IsAny<ZGuid>(), It.IsAny<ZDecimal>()), Times.Never);
			}
		}

		#endregion

		#region TestReducePickedStock_MayDeletePickLine

		public void TestReducePickedStock_MayDeletePickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var pickLine1 = orderLine.PickLines.Single(pl => pl.WZ_Units == 4m);
			var pickLine2 = orderLine.PickLines.Single(pl => pl.WZ_Units == 6m);
			Factory.Save();

			var location = pickLine1.InventoryLineForAvailableInventory.Location;
			var lastAllocatedTime = ZDateTime.UtcNow.AddDays(-1);
			location.WLV_LastAllocatedOrChangedDateUtc = lastAllocatedTime;

			ReducePickedStock(releaseLine, 7m);
			Assert("One of the picklines must become deleted.", pickLine1.IsDeleted ^ pickLine2.IsDeleted);
			AssertEquals("Picklines should be saved immediately.", false, pickLine1.HasChanges || pickLine2.HasChanges);
			var remainingPickline = pickLine1.IsDeleted ? pickLine2 : pickLine1;
			AssertEquals("3 units must remain allocated.", 3m, remainingPickline.WZ_Units);

			var dockets = LoadNewDocketsCreated();
			AssertEquals("A single docket must be created.", 1, dockets.Length);
			AssertDocketCreated(dockets.Single(), 2, 7);

			if (OnlyTouchesOutboundTransferLocation)
			{
				AssertEquals("LastAllocatedOrChanged date should not be affected when reducing picked stock.", lastAllocatedTime, location.WLV_LastAllocatedOrChangedDateUtc);
			}
		}

		#endregion

		#region TestReducePickedStock_MayDeletePicklineAndReleaseLine_EndToEnd

		public void TestReducePickedStock_MayDeletePicklineAndReleaseLine_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();

			var releaseLineRed = orderLine.ReleaseLines[0];
			releaseLineRed.PartAttribute1 = "RED";
			releaseLineRed.Quantity = 4m;
			var releaseLineBlue = orderLine.ReleaseLines.AddNew();
			releaseLineBlue.PartAttribute1 = "BLUE";
			releaseLineBlue.Quantity = 6m;
			var redPickLine = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED");
			var bluePickLine = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "BLUE");

			MakePickLinesPicked(orderLine.PickLines);
			Factory.Save();

			ReducePickedStock(releaseLineRed, 4m);
			var firstDocket = LoadNewDocketsCreated().Single();

			releaseLineBlue = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(rl => rl.PartAttribute1 == "BLUE");
			ReducePickedStock(releaseLineBlue, 6m);
			var secondDocket = LoadNewDocketsCreated().Single(d => d.PK != firstDocket.PK);

			Assert("redPickLine must be deleted", redPickLine.IsDeleted);
			Assert("bluePickLine must be deleted", bluePickLine.IsDeleted);
			AssertEquals("Release lines must be deleted", 0, orderLine.ReleaseLines.Count);
			AssertDocketCreated(firstDocket, 1, 4);
			AssertDocketCreated(secondDocket, 1, 6);
			AssertEquals("All stock must be reduced.", 0m, receive.Lines[0].WE_StockOnHand);
			AssertContainsExactElementsInAnyOrder(new[] { firstDocket, secondDocket }, order.RelatedJobs);
		}

		#endregion

		#region TestReducePickedStock_PartiallyReduceRCA_EndToEnd

		public void TestReducePickedStock_PartiallyReduceRCA_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLineRed = orderLine.ReleaseLines[0];
			releaseLineRed.PartAttribute1 = "RED";
			releaseLineRed.Quantity = 10m;
			Factory.Save();
			AssertEquals("Precondition: Stock must be reduced.", 0m, receive.Inventory[0].WI_TotalUnits);

			ReducePickedStock(releaseLineRed, 4m);

			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Release line quantity must be updated.", 6m, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single().Quantity);
			AssertEquals("RCA quantity must be updated.", 6m, pickLine.WZ_Units);
			AssertEquals("pickLine is still RED.", "RED", pickLine.WZ_ReleaseCapturedPartAttrib1);

			var dockets = LoadNewDocketsCreated();
			AssertEquals("1 docket must be created.", 1, dockets.Length);
			AssertDocketCreated(dockets.Single(), 1, 4);
			AssertEquals("Stock must not change.", 0m, receive.Inventory[0].WI_TotalUnits);
			AssertContainsExactElementsInAnyOrder(dockets, order.RelatedJobs);
		}

		#endregion

		#region TestReducePickedStock_RefreshesAvailableInventory

		public void TestReducePickedStock_RefreshesAvailableInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			Factory.Save();

			var availInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 4m, availInventory.PickLineQuantity);

			using (MockAdjustmentCreation())
			{
				ReducePickedStock(releaseLine, 1m);
				availInventory = pick.OrderedInventories[0].AvailableInventories[0];
				AssertEquals("PickLineQuantity must be updated.", 3m, availInventory.PickLineQuantity);
			}
		}

		#endregion

		#region TestReducePickedStock_RefreshesAvailableInventory_WhenDeleted

		public void TestReducePickedStock_RefreshesAvailableInventory_WhenDeleted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			Factory.Save();

			var availInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 4m, availInventory.PickLineQuantity);

			using (MockAdjustmentCreation())
			{
				ReducePickedStock(releaseLine, 4m);
				AssertEquals("PickLineQuantity must be updated.", 0m, availInventory.PickLineQuantity);
				AssertEquals("PickLines must be updated.", 0, availInventory.PickLines.Count());
			}
		}

		#endregion

		#region TestReducePickedStock_RefreshesCollectionSumUnitsMet

		public void TestReducePickedStock_RefreshesCollectionSumUnitsMet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.FindLocation("A-2"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			AssertEquals("Precondition:", 10m, releaseLine.ParentCollection.SumOfUnitsMet);

			var availInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availInventory2 = pick.OrderedInventories[0].AvailableInventories[1];
			AssertEquals("Precondition", 10m, availInventory1.PickLineQuantity);
			Factory.Save();

			using (MockAdjustmentCreation())
			{
				ReducePickedStock(releaseLine, 1m);

				availInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
				availInventory2 = pick.OrderedInventories[0].AvailableInventories[1];

				releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
				AssertEquals("PickLineQuantity.", 9m, availInventory1.PickLineQuantity);
				AssertEquals("ReleaseLine Quantity.", 9m, releaseLine.Quantity);
				AssertEquals("UnreleasedQty Quantity.", 0m, releaseLine.UnreleasedQty);
				AssertEquals("SumOfUnitsMet on collection must be updated.", 9m, releaseLine.ParentCollection.SumOfUnitsMet);

				availInventory2.PickLineQuantity = 1m;

				AssertEquals("availInventory1 PickLineQuantity", 9m, availInventory1.PickLineQuantity);
				AssertEquals("availInventory2 PickLineQuantity", 1m, availInventory2.PickLineQuantity);
				// releaseLines gets cleared when PickLineQuantity is set. So we have to reload it.
				releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();

				AssertEquals("ReleaseLine Quantity", 10m, releaseLine.Quantity);
				AssertEquals("UnreleasedQty Quantity", 0m, releaseLine.UnreleasedQty);
				AssertEquals("SumOfUnitsMet on collection must be updated.", 10m, releaseLine.ParentCollection.SumOfUnitsMet);
				AssertNoErrors(releaseLine);
			}
		}

		#endregion

		#region TestReducePickedStock_DoesNotHaveChangesInCurrentFactoryWhenThingsGoWrong

		public void TestReducePickedStock_DoesNotHaveChangesInCurrentFactoryWhenThingsGoWrong()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			Factory.Save();

			using (MockAdjustmentCreation(ActionResult.Failure("Some validation error!")))
			{
				ReducePickedStock(releaseLine, 3m);

				AssertEquals("Should not have changes in pick.", false, pick.HasChanges);
				AssertEquals("PickLine is not reduced.", 10m, orderLine.PickLines[0].WZ_Units);
			}
		}

		#endregion

		#region TestReducePickedStock_PackItemWhenReduceStock

		public void TestReducePickedStock_PackItemWhenReduceStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			MakePickLinesPicked(orderLine.PickLines);
			Factory.Save();

			using (MockAdjustmentCreation())
			{
				var newFactory1 = new BusinessObjectFactory() { RefreshEnabled = false };
				var orderInNewFactory1 = newFactory1.Load<WhsOrder>(order.PK);
				var orderLineInNewFactory1 = newFactory1.Load<WhsOrderLine>(orderLine.PK);
				var releaseLine = (WhsReleaseLine)orderLineInNewFactory1.ReleaseLines.Single();

				var packageJob = orderInNewFactory1.PackageJob; // Add fetch hint for Package, see PkgPackageJob.GetAllPackagesOnJob: Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, PK));
				var pickInNewFactory = newFactory1.Load<WhsPick>(pick.PK);
				var isCall = pickInNewFactory.AnyPrintedPackageLabels; // simulate open the release form
				AssertNoExceptionThrown("Should not throw exception", () => ReducePickedStock(releaseLine, 1m)); // simulate first adjust out

				var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var orderInNewFactory2 = newFactory2.Load<WhsOrder>(order.PK);
				var orderLineInNewFactory2 = newFactory2.Load<WhsOrderLine>(orderLine.PK);
				var package = orderInNewFactory2.PackageJob.Packages.AddNew("BOX");
				package.Pack((WhsReleaseLine)orderLineInNewFactory2.ReleaseLines.Single(), 10m);
				newFactory2.Save();

				var releaseLineNew = (WhsReleaseLine)orderLineInNewFactory1.ReleaseLines.Single();
				AssertNoExceptionThrown("Should not throw exception", () => ReducePickedStock(releaseLineNew, 1m)); // simulate the second adjust out, the issue should occurred here.
			}
		}

		#endregion

		#region MockAdjustmentCreation

		static IDisposable MockAdjustmentCreation()
		{
			return MockAdjustmentCreation(ActionResult.Success());
		}

		static IDisposable MockAdjustmentCreation(ActionResult actionResult)
		{
			var factoryStub = new Mock<IPickedStockAdjustersFactory>();
			var mockRecorder = new Mock<IPickedStockAdjuster>();
			factoryStub.Setup(s => s.GetNewAdjuster(It.IsAny<WhsWarehouse>(), It.IsAny<OrgHeader>(), It.IsAny<ReduceStockReason>())).Returns(mockRecorder.Object);
			mockRecorder.Setup(s => s.PrepareForSaving()).Returns(actionResult);
			return ObjectFactory.Substitute(factoryStub.Object);
		}

		static void MakePickLinesPicked(IEnumerable<WhsPickLine> pickLines)
		{
			pickLines.ForEach(pl => pl.WZ_GS_NKAssignedTo = "E");
			pickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
		}

		WhsDocket[] LoadNewDocketsCreated()
		{
			var query = new ZQuery();
			query.AddToFilter(WhsDocketSchema.WD_DocketType, ExpectedDocketTypeCreated);
			query.AddToFilter(GetAdditionalFilterOnDocketsCreated());
			return Factory.Load<WhsDocket>(query);
		}

		protected virtual ZQuery GetAdditionalFilterOnDocketsCreated() => new ZQuery();

		#endregion

		#region Implementation

		protected abstract bool OnlyTouchesOutboundTransferLocation { get; }

		protected abstract ActionResult ReducePickedStock(WhsReleaseLine releaseLine, decimal qtyToReduce);

		protected abstract ZString FunctionDescription { get; }

		protected abstract ZString ExpectedDocketTypeCreated { get; }

		protected abstract void AssertDocketCreated(WhsDocket docket, int expectedLineCount, decimal expectedUnitSum, ZGuid? originalLocation = null);

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
