using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using PickTrolleyStatus = Enterprise.Warehouse.Transactions.TrolleyPicking.PickTrolleyStatus;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ReallocateShortPickedOrderLinesForTrolleyTest : WhsPickingSecureServiceTestCase
	{
		#region TestReallocateShortPickedOrderLinesForTrolley_SingleLine

		public void TestReallocateShortPickedOrderLinesForTrolley_SingleLine()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, 1);
			packingHelper.CreatePackageDivot(packageTote, pickLine);
			AssignPickLine(pickLine, staff);
			Helper.Factory.Save();

			AssertEquals("Pre-condition: packageTote has 1 divot.", 1, packageTote.PackedItemDivots.Count);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				false);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);
			AssertEquals("Pre-condition: Shorted items removed in packageTote.", 0, packageTote.PackedItemDivots.Count);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForTrolley(trolleyJob.PK.ToGuid(), packageTote.PK.ToGuid(), new[] { orderLine.PK.ToGuid() });

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Job.Lines.Count);
				AssertEquals("The reallocated PickLine has 4 units to pick.", 5m, response.Job.Lines[0].Units);
				AssertEquals("Returned PickLine linked to Slot 1.", (short)1, response.Job.Lines[0].SlotNumber);
				AssertEquals("Returned PickLine linked to package packageTote.", packageTote.KP_PackageID, response.Job.Lines[0].PackageID);
				AssertEquals("Reallocated items packed in packageTote.", 1, packageTote.PackedItemDivots.Count);
				AssertEquals("Reallocated items packed in packageTote.", 5m, packageTote.PackedItemDivots[0].KI_PackedQty);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_MultipleLines

		public void TestReallocateShortPickedOrderLinesForTrolley_MultipleLines()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 5, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var loc3 = data.Whs1.FindLocation("A-3");
			var loc4 = data.Whs1.FindLocation("A-4");
			var loc5 = data.Whs1.FindLocation("A-5");

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, loc2);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, loc3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, loc4);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 60m, loc5);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", Notify);
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			var pickLine1 = orderLine1.PickLines[0];
			var pickLine2 = orderLine2.PickLines[0];
			var pick2 = Helper.CreatePickNew(order2);
			var pickLine3 = orderLine3.PickLines[0];
			var pickLine4 = orderLine4.PickLines[0];

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, 1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageTote2 = packingHelper.CreatePackage(packageJob2, "Tote2", 1, Constants.PkgUnit.Box);
			packageTote2.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote2.PK, 2);
			packingHelper.CreatePackageDivot(packageTote, pickLine1);
			packingHelper.CreatePackageDivot(packageTote, pickLine2);
			packingHelper.CreatePackageDivot(packageTote2, pickLine3);
			packingHelper.CreatePackageDivot(packageTote2, pickLine4);
			AssignPickLine(pickLine1, staff);
			AssignPickLine(pickLine2, staff);
			AssignPickLine(pickLine3, staff);
			AssignPickLine(pickLine4, staff);
			Helper.Factory.Save();

			AssertEquals("Pre-condition: packageTote has 2 divots.", 2, packageTote.PackedItemDivots.Count);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				false);

			var divot1 = packageTote.PackedItemDivots[0];
			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick1.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 1m, order1.WD_UnitsSent);
			AssertEquals("Pre-condition: PK of orderLine1 and orderLin2 was returned.", 2, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: Shorted items removed in packageTote.", 1, packageTote.PackedItemDivots.Count);
			AssertEquals("Pre-condition: divot1 has 1 item.", 1m, divot1.KI_PackedQty);
			AssertEquals("Pre-condition: packageTote2 still has 2 items.", 2, packageTote2.PackedItemDivots.Count);
			AssertContainsExactElementsInAnyOrder("Pre-condition: PK of orderLines were returned.",
				new[] { orderLine1.PK.ToGuid(), orderLine2.PK.ToGuid() }, shortPickResponse.ShortedOrderLinePKs);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForTrolley(trolleyJob.PK.ToGuid(), packageTote.PK.ToGuid(), new[] { orderLine1.PK.ToGuid(), orderLine2.PK.ToGuid() });

				AssertEquals("2 Pick Line Group returned after reallocation.", 3, response.Job.Lines.Count);
				AssertEquals("Slot 1 Contains 1 Pick Line", 1, response.Job.Lines[0].PKs.Length);
				AssertEquals("Slot 2 Contains 1 Pick Line", 1, response.Job.Lines[1].PKs.Length);
				AssertEquals("Slot 3 Contains 2 Pick Lines", 2, response.Job.Lines[2].PKs.Length);

				var line1 = response.Job.Lines.Single(l => l.SlotNumber == 1);
				var newDivots = packageTote.PackedItemDivots.Except(new[] { divot1 }).ToArray();
				AssertEquals("The reallocated PickLine has 19 units to pick.", 19m, line1.Units);
				AssertEquals("Returned PickLine linked to package packageTote.", packageTote.KP_PackageID, line1.PackageID);
				AssertEquals("Reallocated 2 more PickLines for packageTote.", 2, newDivots.Length);
				AssertEquals("Reallocated items packed in packageTote.", 19m, newDivots[0].KI_PackedQty + newDivots[1].KI_PackedQty);

				var slotNumber2Lines = response.Job.Lines.Where(l => l.SlotNumber == 2).ToArray();
				var line2 = slotNumber2Lines[0];
				AssertEquals("Slot 2 should remain untouched.", 10m, line2.Units);
				AssertEquals("line2 linked to package packageTote2.", packageTote2.KP_PackageID, line2.PackageID);

				var line3 = slotNumber2Lines[1];
				AssertEquals("Slot 3 should remain untouched.", 10m, line3.Units);
				AssertEquals("line3 linked to package packageTote2.", packageTote2.KP_PackageID, line3.PackageID);

				AssertEquals("packageTote2 remain untouched.", 2, packageTote2.PackedItemDivots.Count);
				AssertEquals("packageTote2 remain untouched.", 20m, packageTote2.PackedItemDivots[0].KI_PackedQty + packageTote2.PackedItemDivots[1].KI_PackedQty);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_NoStockReallocated

		public void TestReallocateShortPickedOrderLinesForTrolley_NoStockReallocated()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, 1);
			packingHelper.CreatePackageDivot(packageTote, pickLine);
			AssignPickLine(pickLine, staff);
			Helper.Factory.Save();

			AssertEquals("Pre-condition: packageTote has 1 divot.", 1, packageTote.PackedItemDivots.Count);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(2, false),
				null,
				false);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 2m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);
			AssertEquals("Pre-condition: packageTote has 1 item since we picked 1 unit.", 1, packageTote.PackedItemDivots.Count);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForTrolley(trolleyJob.PK.ToGuid(), packageTote.PK.ToGuid(), new[] { orderLine.PK.ToGuid() });

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "No stock could be reallocated for the shorted products.", response.ErrorMessage);
				AssertEquals("packageTote has 1 picked line and nothing reallocated.", 1, packageTote.PackedItemDivots.Count);
				AssertEquals("The PickLine has 2 picked items.", 2m, packageTote.PackedItemDivots[0].KI_PackedQty);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_NoEnoughStockToReallocate

		public void TestReallocateShortPickedOrderLinesForTrolley_NoEnoughStockToReallocate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, loc1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, loc2);
			receive.FinaliseDocketWithoutUserConfirmation();

			var packingHelper = new PackingTestHelper(Helper.Factory);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, 1);
			packingHelper.CreatePackageDivot(packageTote, pickLine);
			AssignPickLine(pickLine, staff);
			Helper.Factory.Save();

			AssertEquals("Pre-condition: packageTote has 1 divot.", 1, packageTote.PackedItemDivots.Count);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				false);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 1m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);
			AssertEquals("Pre-condition: packageTote has 1 item because we picked 1 unit.", 1, packageTote.PackedItemDivots.Count);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForTrolley(trolleyJob.PK.ToGuid(), packageTote.PK.ToGuid(), new[] { orderLine.PK.ToGuid() });

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Job.Lines.Count);
				AssertEquals("The reallocated PickLine has 4 units to pick.", 4m, response.Job.Lines[0].Units);
				AssertEquals("Returned PickLine linked to Slot 1.", (short)1, response.Job.Lines[0].SlotNumber);
				AssertEquals("Returned PickLine linked to package packageTote.", packageTote.KP_PackageID, response.Job.Lines[0].PackageID);
				AssertEquals("Reallocated items packed in packageTote.", 2, packageTote.PackedItemDivots.Count);
				AssertEquals("Reallocated items packed in packageTote.", 5m, packageTote.PackedItemDivots[0].KI_PackedQty + packageTote.PackedItemDivots[1].KI_PackedQty);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_Failed

		public void TestReallocateShortPickedOrderLinesForTrolley_Failed()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, 1);
			packingHelper.CreatePackageDivot(packageTote, pickLine);
			AssignPickLine(pickLine, staff);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				false);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 1m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns<WhsPick, INotifications, IPickStrategy, IEnumerable<WhsPickOrderedInventory>>((p, n, ps, ordInv) => AllocationResult.ErrorOrWarning)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					n.AddError("Some allocation error.");
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForTrolley(trolleyJob.PK.ToGuid(), packageTote.PK.ToGuid(), new[] { orderLine.PK.ToGuid() });

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "Some allocation error.", response.ErrorMessage.TrimEnd());
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_TrolleyJobNotFound

		public void TestReallocateShortPickedOrderLinesForTrolley_TrolleyJobNotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.ReallocateShortPickedOrderLinesForTrolley(new Guid(), new Guid(), new[] { new Guid() });

			AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Trolley job was not found.", response.ErrorMessage);
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_PackageNotFound

		public void TestReallocateShortPickedOrderLinesForTrolley_PackageNotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.ReallocateShortPickedOrderLinesForTrolley(trolleyJob.PK.ToGuid(), new Guid(), new[] { new Guid() });

			AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package was not found.", response.ErrorMessage);
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_ShortedOrderedInventoriesNotFound

		public void TestReallocateShortPickedOrderLinesForTrolley_ShortedOrderedInventoriesNotFound()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, 1);
			packingHelper.CreatePackageDivot(packageTote, pickLine);
			AssignPickLine(pickLine, staff);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.ReallocateShortPickedOrderLinesForTrolley(trolleyJob.PK.ToGuid(), packageTote.PK.ToGuid(), new[] { new Guid() });

			AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Shorted Ordered Inventories not found.", response.ErrorMessage);
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_MultipleOrders

		public void TestReallocateShortPickedOrderLinesForTrolley_MultipleOrders()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 60m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, loc2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", Notify);
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1, order2);
			var pickLine1 = orderLine1.PickLines[0];
			var pickLine2 = orderLine2.PickLines[0];
			var pickLine3 = orderLine3.PickLines[0];
			var pickLine4 = orderLine4.PickLines[0];

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, 1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageTote2 = packingHelper.CreatePackage(packageJob2, "Tote2", 1, Constants.PkgUnit.Box);
			packageTote2.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote2.PK, 2);
			packingHelper.CreatePackageDivot(packageTote, pickLine1);
			packingHelper.CreatePackageDivot(packageTote, pickLine2);
			packingHelper.CreatePackageDivot(packageTote2, pickLine3);
			packingHelper.CreatePackageDivot(packageTote2, pickLine4);
			AssignPickLine(pickLine1, staff);
			AssignPickLine(pickLine2, staff);
			AssignPickLine(pickLine3, staff);
			AssignPickLine(pickLine4, staff);
			Helper.Factory.Save();

			AssertEquals("Pre-condition: packageTote has 2 divots.", 2, packageTote.PackedItemDivots.Count);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse1 = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				false);

			var divot1 = packageTote.PackedItemDivots[0];
			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick1.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 1m, order1.WD_UnitsSent);
			AssertEquals("Pre-condition: PK of orderLine1 and orderLin2 was returned.", 2, shortPickResponse1.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: Shorted items removed in packageTote.", 1, packageTote.PackedItemDivots.Count);
			AssertEquals("Pre-condition: divot1 has 1 item.", 1m, divot1.KI_PackedQty);
			AssertEquals("Pre-condition: packageTote2 still has 2 items.", 2, packageTote2.PackedItemDivots.Count);
			AssertContainsExactElementsInAnyOrder("Pre-condition: PK of orderLines were returned.",
				new[] { orderLine1.PK.ToGuid(), orderLine2.PK.ToGuid() }, shortPickResponse1.ShortedOrderLinePKs);

			var shortPickResponse2 = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine3.PK.ToGuid(), pickLine4.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				false);

			var divot2 = packageTote2.PackedItemDivots[0];
			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick1.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 1m, order2.WD_UnitsSent);
			AssertEquals("Pre-condition: PK of orderLine3 and orderLine4 was returned.", 2, shortPickResponse2.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: Shorted items removed in packageTote.", 1, packageTote2.PackedItemDivots.Count);
			AssertEquals("Pre-condition: divot2 has 1 item.", 1m, divot2.KI_PackedQty);
			AssertContainsExactElementsInAnyOrder("Pre-condition: PK of orderLines were returned.",
				new[] { orderLine3.PK.ToGuid(), orderLine4.PK.ToGuid() }, shortPickResponse2.ShortedOrderLinePKs);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForTrolley(trolleyJob.PK.ToGuid(), packageTote.PK.ToGuid(), new[] { orderLine1.PK.ToGuid(), orderLine2.PK.ToGuid() });

				AssertEquals("Single Pick Line Group returned after reallocation.", 1, response.Job.Lines.Count);
				AssertEquals("Pick Line Group is in Slot 1", (short)1, response.Job.Lines[0].SlotNumber);
				AssertEquals("Slot 1 Contains 2 Pick Lines", 2, response.Job.Lines[0].PKs.Length);

				var line = response.Job.Lines.Single(l => l.SlotNumber == 1);
				var newDivots = packageTote.PackedItemDivots.Except(new[] { divot1 }).ToArray();
				AssertEquals("The reallocated PickLine has 19 units to pick.", 19m, line.Units);
				AssertEquals("Returned PickLine linked to package packageTote.", packageTote.KP_PackageID, line.PackageID);
				AssertEquals("Reallocated 2 more PickLines for packageTote.", 2, newDivots.Length);
				AssertEquals("Reallocated items packed in packageTote.", 19m, newDivots[0].KI_PackedQty + newDivots[1].KI_PackedQty);

				// Below is a side effect of matching Ordered Inventories being reallocated, but unable to be packed
				AssertEquals("No PickLines new pick lines for packageTote2.", 0, packageTote2.PackedItemDivots.Except(new[] { divot2 }).Count());
				AssertEquals("All inventory reallocated, no items shorted.", false, pick1.HasShortfallItems);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_DBHits

		public void TestReallocateShortPickedOrderLinesForTrolley_DBHits()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var products = new List<OrgSupplierPart>(10);
			for (int i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct(data.Org1, "PP" + i);
				products.Add(product);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);
				Helper.CreateWhsReceiveInventoryLine(receive, product, 1m, loc1);
				Helper.CreateWhsReceiveInventoryLine(receive, product, 99m, loc2);
				receive.FinaliseDocketWithoutUserConfirmation();
			}
			Helper.Factory.Save();

			var orders = new List<WhsOrder>(100);
			var pickLines = new List<WhsPickLine>(100);
			for (int i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + i, Notify);
				for (int j = 0; j < 10; j++)
				{
					Helper.CreateWhsOrderLine(order, products[i], 1m);
				}
				orders.Add(order);
				var pick = Helper.CreatePickNew(order);
				pickLines.AddRange(pick.GetAllPickLines());
			}
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);

			for (int i = 0; i < 10; i++)
			{
				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(orders[i]);
				var packageTote = packingHelper.CreatePackage(packageJob, "Tote" + i, 1, Constants.PkgUnit.Box);
				packageTote.SetIsTote(true);
				Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, (short)(i + 1));

				for (int j = 0; j < 10; j++)
				{
					var pickLine = pickLines[i * 10 + j];
					packingHelper.CreatePackageDivot(packageTote, pickLine);
					AssignPickLine(pickLine, staff);
				}
			}

			Helper.Factory.Save();

			var firstOrder = orders[0];
			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { firstOrder.Lines[0].PickLines[0].PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				false);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 2 },
				{ OrgPartUnitSchema.Constants.TableName, 2 },
				{ OrgSupplierPartSchema.Constants.TableName, 2 },
				{ OrgSupplierPartBarcodeSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 4 },
				{ PkgPackageHeaderSchema.Constants.TableName, 3 },
				{ PkgPackageJobSchema.Constants.TableName, 3 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 3 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ ProductionRuleSchema.Constants.TableName, 1 },
				{ RefEquipmentSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				// 2 for preparing data before reallocation, 1 for Allocation, 4 for building TrolleyInfo(Receives and Orders)
				{ WhsDocketSchema.Constants.TableName, 8 },
				{ WhsDocketLineSchema.Constants.TableName, 4 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 3 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 4 },
				{ WhsPickTrolleyJobSchema.Constants.TableName, 1 },
				{ WhsPickTrolleySlotSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
			};

			var webService2 = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDbHits, webService2.Factory))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var response = webService2.ReallocateShortPickedOrderLinesForTrolley(
					trolleyJob.PK.ToGuid(),
					firstOrder.PackageJob.Packages[0].PK.ToGuid(),
					new[] { firstOrder.Lines[0].PK.ToGuid() });

				AssertEquals(ErrorTypes.None, response.Error);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForTrolley_FactoryConcurrencySaveError

		public void TestReallocateShortPickedOrderLinesForTrolley_FactoryConcurrencySaveError()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageTote = packingHelper.CreatePackage(packageJob, "Tote1", 1, Constants.PkgUnit.Box);
			packageTote.SetIsTote(true);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, 1);
			packingHelper.CreatePackageDivot(packageTote, pickLine);
			AssignPickLine(pickLine, staff);
			Helper.Factory.Save();

			AssertEquals("Pre-condition: packageTote has 1 divot.", 1, packageTote.PackedItemDivots.Count);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				false);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);
			AssertEquals("Pre-condition: Shorted items removed in packageTote.", 0, packageTote.PackedItemDivots.Count);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var innerException = new Exception();
				var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pickLine).Row, TestConnection);
				webService2.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

				var response = webService2.ReallocateShortPickedOrderLinesForTrolley(trolleyJob.PK.ToGuid(), packageTote.PK.ToGuid(), new[] { orderLine.PK.ToGuid() });
				AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has been assigned to or modified this Job. Please restart the operation and try again.", response.ErrorMessage);
			}
		}

		#endregion

		#region AssignPickLine

		void AssignPickLine(WhsPickLine pickLine, GlbStaff staff)
		{
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine.WZ_IsPicking = true;
		}

		#endregion
	}
}
