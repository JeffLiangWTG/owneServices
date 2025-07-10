using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderAndPickAutoFinalisationProcessorTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsOrderAndPickAutoFinalisationProcessor(null));
		}

		#endregion

		#region TestProcess

		public void TestProcess_PickWithOrdersOfMixedShortfalls()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var orderWithoutShortfall = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderWithShortfall = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20m);
			Assert("Precondition - Shortfall does not exist in orderWithoutShortfall.", !orderWithoutShortfall.ShortfallExists);
			Assert("Precondition - Shortfall exists in orderWithShortfall.", orderWithShortfall.ShortfallExists);

			var pick = Helper.CreatePickNew(orderWithoutShortfall, orderWithShortfall);
			orderWithoutShortfall.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			orderWithShortfall.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			IProcessor processor = new WhsOrderAndPickAutoFinalisationProcessor(pick);
			processor.Process(Notify);

			Assert("The order that has shortfall should not be finalised.", !orderWithShortfall.IsFinalised);
			Assert("The order that has no shortfall should be finalised.", orderWithoutShortfall.IsFinalised);
			Assert("The pick with unfinalised orders should not be finalised", !pick.IsFinalised);
			AssertEquals("Should not have saved the factory.", true, pick.HasChanges);
		}

		public void TestProcess_PickWithOrdersOfMixedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var orderWithoutUncompletedPickLines = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderWithUncompletedPickLines = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 5m);
			Assert("Precondition - Shortfall does not exist in orderWithoutUncompletedPickLines.", !orderWithoutUncompletedPickLines.ShortfallExists);
			Assert("Precondition - Shortfall does not exist in orderWithoutUncompletedPickLines.", !orderWithUncompletedPickLines.ShortfallExists);

			var pick = Helper.CreatePickNew(orderWithoutUncompletedPickLines, orderWithUncompletedPickLines);
			orderWithoutUncompletedPickLines.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			IProcessor processor = new WhsOrderAndPickAutoFinalisationProcessor(pick);
			processor.Process(Notify);

			Assert("The order that has uncompleted pick lines should not be finalised.", !orderWithUncompletedPickLines.IsFinalised);
			Assert("The order that has no uncompleted pick lines should be finalised.", orderWithoutUncompletedPickLines.IsFinalised);
			Assert("The pick with unfinalised orders should not be finalised", !pick.IsFinalised);
			AssertEquals("Should not have saved the factory.", true, pick.HasChanges);
		}

		public void TestProcess_PickWithOrdersOfMixedPickLines_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var orderWithoutUncompletedPickLines = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderWithUncompletedPickLines = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 5m);
			Assert("Precondition - Shortfall does not exist in orderWithoutUncompletedPickLines.", !orderWithoutUncompletedPickLines.ShortfallExists);
			Assert("Precondition - Shortfall does not exist in orderWithoutUncompletedPickLines.", !orderWithUncompletedPickLines.ShortfallExists);

			var pick = Helper.CreatePickNew(orderWithoutUncompletedPickLines, orderWithUncompletedPickLines);
			var pickLine = orderWithoutUncompletedPickLines.Lines[0].PickLines[0];
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Today);
			Factory.Save();

			IProcessor processor = new WhsOrderAndPickAutoFinalisationProcessor(pick);
			processor.Process(Notify);
			Assert("The order that has uncompleted pick lines should not be finalised.", !orderWithUncompletedPickLines.IsFinalised);
			Assert("The order that has no uncompleted pick lines should be finalised.", orderWithoutUncompletedPickLines.IsFinalised);
			Assert("The pick with unfinalised orders should not be finalised", !pick.IsFinalised);
			AssertEquals("Should not have saved the factory.", true, pick.HasChanges);
		}

		public void TestProcess_PickAndOrdersAreAllFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			order.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			IProcessor processor = new WhsOrderAndPickAutoFinalisationProcessor(pick);
			processor.Process(Notify);

			Assert("The order should be finalised.", order.IsFinalised);
			Assert("The pick with all orders finalised should be finalised", pick.IsFinalised);
			AssertEquals("Should not have saved the factory.", true, pick.HasChanges);
		}

		public void TestProcess_PickWithWorkOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();

			IProcessor processor = new WhsOrderAndPickAutoFinalisationProcessor(pick);
			processor.Process(Notify);

			Assert("The order should be finalised.", workOrder.IsFinalised);
			Assert("The pick with all orders finalised should be finalised", pick.IsFinalised);
			AssertEquals("Should not have saved the factory.", true, pick.HasChanges);
		}

		public void TestProcess_PickWithWorkOrderShortfall()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 20m);
			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();

			Assert("Precondition - Shortfall exists in workOrder.", workOrder.ShortfallExists);

			IProcessor processor = new WhsOrderAndPickAutoFinalisationProcessor(pick);
			processor.Process(Notify);

			Assert("The work order that has shortfall should not be finalised.", !workOrder.IsFinalised);
			Assert("The pick with unfinalised work orders should not be finalised", !pick.IsFinalised);
		}

		public void TestProcess_PickWithOrderWithUnfinishedTotes_AllowPickFinaliseWithUnpackedTote()
		{
			TestProcess_PickWithOrderWithUnfinishedTotesCore(allowPickFinaliseWithUnpackedTote: true);
		}

		public void TestProcess_PickWithOrderWithUnfinishedTotes_DisallowPickFinaliseWithUnpackedTote()
		{
			TestProcess_PickWithOrderWithUnfinishedTotesCore(allowPickFinaliseWithUnpackedTote: false);
		}

		void TestProcess_PickWithOrderWithUnfinishedTotesCore(bool allowPickFinaliseWithUnpackedTote)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var pickPackParams = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParams.WPP_AllowPickFinalizationWithUnpackedTotes = allowPickFinaliseWithUnpackedTote;
			Factory.Save();

			var orderWithoutTote = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderWithTote = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20m);

			var pick = Helper.CreatePickNew(orderWithoutTote, orderWithTote);
			var package = orderWithTote.PackageJob.Packages.AddNew();
			package.SetIsTote(true);

			orderWithoutTote.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			orderWithTote.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			Assert("Precondition", !orderWithoutTote.PackageJob.Packages.Any(p => p.GetIsTote()));
			Assert("Precondition", orderWithTote.PackageJob.Packages.Any(p => p.GetIsTote()));
			Assert("Precondition", !orderWithTote.ShortfallExists && orderWithTote.Lines.All(ol => ol.PickLines.All(pl => pl.IsPickedFromPutawayLocation)));

			Notify.Clear();
			IProcessor processor = new WhsOrderAndPickAutoFinalisationProcessor(pick);
			processor.Process(Notify);

			Assert("The order that has Tote can be finalised.", orderWithTote.IsFinalised);
			Assert("The order that has no Tote should be finalised.", orderWithoutTote.IsFinalised);
			AssertEquals("The pick with unfinalised orders should have no error if the organisation's warehouse WPP_AllowPickFinalizationWithUnpackedTotes pick pack param has been enabled.",
				!allowPickFinaliseWithUnpackedTote ? "Failed to Finalize Pick.\r\nError Message: Cannot Finalize this pick as the following Orders have not finished Packing Totes:\r\nO2\r\n" : string.Empty,
				Notify.AsString);
			AssertEquals("The pick with unfinalised orders should be finalised if the organisation's warehouse WPP_AllowPickFinalizationWithUnpackedTotes pick pack param has been enabled.",
				allowPickFinaliseWithUnpackedTote,
				pick.IsFinalised);
		}

		public void TestProcess_PickWithOrderUsingConsolidationNonStaged()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertEquals("Precondition: Receive Line Location.", firstLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var transferLine = pick.Transfers[0].Lines[0];
			var transferPickLine = pick.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine.WE_WL);

			transferLine.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine.WE_WL);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg, pickLine);

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, pkg, handlingUnitPackage);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.CreateDockDoorTransfer(handlingUnitPackage.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals("New Transfer Line to Dock Door created.", 2, transfer.Lines.Count);

			Assert("Precondition: UseDirectedPackingConsolidation", order.WD_UseDirectedPackingConsolidation);
			AssertNotEquals("Precondition: WarehouseOrderStatus", WhsOrderStatus.Codes.Staged, order.WarehouseOrderStatus);
			Assert("Precondition: Shortfall", !order.ShortfallExists && order.Lines.All(ol => ol.PickLines.All(pl => pl.IsPickedFromPutawayLocation)));
			AssertResults(expectedMessage: "Failed to Finalize Pick.\r\nError Message: Cannot Finalize this pick as the following Orders are using directed Consolidation and are not yet Staged:\r\nO1\r\n", expectedToBeFinalised: false);

			errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK);
			AssertEquals(true, errorMessage.IsNullOrEmpty());
			Factory.Save();

			AssertEquals("Precondition: WarehouseOrderStatus", WhsOrderStatus.Codes.Staged, new BusinessObjectFactory().Load<WhsOrder>(order.PK).WarehouseOrderStatus);

			AssertResults(expectedMessage: string.Empty, expectedToBeFinalised: true);

			void AssertResults(string expectedMessage, bool expectedToBeFinalised)
			{
				Notify.Clear();
				var pickInDB = new BusinessObjectFactory().Load<WhsPick>(pick.PK);
				IProcessor processor = new WhsOrderAndPickAutoFinalisationProcessor(pickInDB);
				processor.Process(Notify);
				AssertEquals("The order that has Consolidation and staged should be finalised othrwise not.", true, pickInDB.Orders[0].IsFinalised);
				AssertEquals("Should not have saved the factory.", true, pickInDB.HasChanges);
				AssertEquals("Should not have saved the factory.", expectedMessage, Notify.AsString);
				AssertEquals("The pick with unfinalised orders should not be finalised", expectedToBeFinalised, pickInDB.IsFinalised);
			}
		}

		public void TestProcess_SecurityError()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();

			try
			{
				Env.Security.WhsReleaseFinalise.IsAllowed = false;

				Notify.Clear();
				IProcessor processor = new WhsOrderAndPickAutoFinalisationProcessor(pick);
				processor.Process(Notify);
				AssertEquals("Pick should not be finalized.", false, pick.IsFinalised);
				AssertEquals("Should show a user friendly message.",
@"Failed to Finalize Pick.
Login user is not allowed to finalize Warehouse Pick. Please check user's security rights settings for Warehouse Release Finalize
", Notify.AsString);
			}
			finally
			{
				Env.Security.WhsReleaseFinalise.IsAllowed = true;
			}
		}

		#endregion
	}
}
