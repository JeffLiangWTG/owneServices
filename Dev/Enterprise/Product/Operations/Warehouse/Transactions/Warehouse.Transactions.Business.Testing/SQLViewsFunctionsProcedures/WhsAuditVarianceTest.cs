using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAuditVarianceTest : WhsTestCaseWithFactory
	{
		#region TestView

		public void TestView()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD1", data.Part2, 23m);
			var order_passed = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD2", data.Part1, 10m);
			var order_failed = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD3", data.Part2, 20m);
			Helper.CreatePickNew(order);
			order.WD_QualityAuditRequired = true;
			AddPackageToOrder(order, "PAK1", "PLT");

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var pick_passed = Helper.CreatePickNew(order_passed);
			var package_passed = AddPackageToOrder(order_passed, "PAK2", "PLT");
			var audit_passed = Helper.CreateWhsPackageAudit(order_passed, "PAK2", completeTimeoffset);
			audit_passed.WPA_GS_NKAuditor = auditor.GS_Code;

			var pick_failed = Helper.CreatePickNew(order_failed);
			var package_failed = AddPackageToOrder(order_failed, "PAK3", "PLT");
			var audit_failed = Helper.CreateWhsPackageAuditWithLineFailure(order_failed, "PAK3", data.Part2, 20m, 18m,
				completeTimeoffset.AddDays(1));
			audit_failed.WPA_GS_NKAuditor = auditor.GS_Code;

			// this test is testing without in-transit transfers
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var pickLine = order_failed.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			AssertEquals("Precondition: Testing picking without WZ_WE_OriginalPickedInventory set.", true,
				pickLine.WZ_WE_OriginalPickedInventoryLine.IsEmpty);

			var reportView = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain one failure and one success.", 2, reportView.Count);

			var reportViewRow = reportView.Where(x => x["PackageID"].ToString() == "PAK3").Single();
			AssertRowData(order_failed, package_failed, data.Part2, pickLine, 20m, 20m, 18m, audit_failed,
				reportViewRow);
		}

		public void TestView_InTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			var otherUser = Helper.CreateGlbStaff("LSK", "Luke Skywalker");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order_failed = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order_failed);
			order_failed.WD_QualityAuditRequired = true;

			var package_failed = AddPackageToOrder(order_failed, "P1", "PLT");
			var pickLine = order_failed.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Today);
			pickLine.WZ_GS_NKAssignedTo = otherUser.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-1);
			AssertEquals("Precondition: Testing picking with WZ_WE_OriginalPickedInventory set.", true,
				pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid);

			var audit_failed = Helper.CreateWhsPackageAuditWithLineFailure(order_failed, "P1", data.Part1, 20m, 18m,
				completeTimeoffset.AddDays(1));
			audit_failed.WPA_GS_NKAuditor = auditor.GS_Code;

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order_failed);

			// finalise Pick to prevent creation of second In-Transit Transfer Line
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var reportView = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain only one failure.", 1, reportView.Count);

			var reportViewRow = reportView.Single();
			var newPickLine = inTransitLine.PickLines.Single();
			AssertNotEquals("NOTE: Assertion below intentionally passes in newPickLine and NOT pickLine.", newPickLine,
				pickLine);
			AssertRowData(order_failed, package_failed, data.Part1, newPickLine, 20m, 20m, 18m, audit_failed,
				reportViewRow);
		}

		// This schema isn't supported as of 09/2017, but will be soon
		public void TestView_InTransitInventory_MultipleTransferSteps()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			var otherUser = Helper.CreateGlbStaff("LSK", "Luke Skywalker");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order_failed = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order_failed);
			order_failed.WD_QualityAuditRequired = true;

			var package_failed = AddPackageToOrder(order_failed, "P1", "PLT");
			var pickLineToMakeInTransit =
				order_failed.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			WhsPickLine firstPickLine = null;

			for (int i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit,
					ZDateTimeOffset.Today.AddDays(-6 + i), allowMultipleSteps: true);

				var location =
					i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				if (i > 0)
				{
					newPickLine.WZ_GS_NKAssignedTo = otherUser.GS_Code;
				}
				else
				{
					firstPickLine = newPickLine;
				}

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-1);
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = otherUser.GS_Code;

			var audit_failed = Helper.CreateWhsPackageAuditWithLineFailure(order_failed, "P1", data.Part1, 20m, 18m,
				completeTimeoffset.AddDays(1));
			audit_failed.WPA_GS_NKAuditor = auditor.GS_Code;

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order_failed);

			// finalise Pick to prevent creation of another In-Transit Transfer Line
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var reportView = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain only one failure.", 1, reportView.Count);

			var reportViewRow = reportView.Single();
			AssertRowData(order_failed, package_failed, data.Part1, firstPickLine, 20m, 20m, 18m, audit_failed,
				reportViewRow);
		}

		public void TestView_InTransitInventory_ReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			var otherUser = Helper.CreateGlbStaff("LSK", "Luke Skywalker");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order_failed = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order_failed);
			order_failed.WD_QualityAuditRequired = true;

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order_failed);
			var package_failed = PackingHelper.CreatePackage(pkgJob, "P1", 1, "BOX");
			var pickLine = order_failed.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			order_failed.Lines[0].ReleaseLines[0].PartAttribute1 = "RED";

			var packageDivot = PackingHelper.CreatePackageDivot(package_failed, pickLine);

			var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Today);
			pickLine.WZ_GS_NKAssignedTo = otherUser.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-1);
			AssertEquals("Precondition: Testing picking with WZ_WE_OriginalPickedInventory set.", true,
				pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid);

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var audit_failed = Helper.CreateWhsPackageAuditWithLineFailure(order_failed, "P1", data.Part1, 20m, 18m,
				completeTimeoffset.AddDays(1));
			audit_failed.WPA_GS_NKAuditor = auditor.GS_Code;

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order_failed);

			// finalise Pick to prevent creation of second In-Transit Transfer Line
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var reportView = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain only one failure.", 1, reportView.Count);

			var reportViewRow = reportView.Single();
			var newPickLine = inTransitLine.PickLines.Single();
			AssertNotEquals("NOTE: Assertion below intentionally passes in newPickLine and NOT pickLine.", newPickLine,
				pickLine);
			AssertRowData(order_failed, package_failed, data.Part1, newPickLine, 20m, 20m, 18m, audit_failed,
				reportViewRow);
		}

		public void TestView_InTransitInventory_ReleaseCaptured_MultipleTransferSteps()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			var otherUser = Helper.CreateGlbStaff("LSK", "Luke Skywalker");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order_failed = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order_failed);
			order_failed.WD_QualityAuditRequired = true;

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order_failed);
			var package_failed = PackingHelper.CreatePackage(pkgJob, "P1", 1, "BOX");
			var pickLineToMakeInTransit =
				order_failed.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			order_failed.Lines[0].ReleaseLines[0].PartAttribute1 = "RED";

			var packageDivot = PackingHelper.CreatePackageDivot(package_failed, pickLineToMakeInTransit);

			WhsPickLine firstPickLine = null;

			for (var i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit,
					ZDateTimeOffset.Today.AddDays(-6 + i), allowMultipleSteps: true);

				var location =
					i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				if (i > 0)
				{
					newPickLine.WZ_GS_NKAssignedTo = otherUser.GS_Code;
				}
				else
				{
					firstPickLine = newPickLine;
				}

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-1);
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = otherUser.GS_Code;
			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);

			var audit_failed = Helper.CreateWhsPackageAuditWithLineFailure(order_failed, "P1", data.Part1, 20m, 18m,
				completeTimeoffset.AddDays(1));
			audit_failed.WPA_GS_NKAuditor = auditor.GS_Code;

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order_failed);

			// finalise Pick to prevent creation of another In-Transit Transfer Line
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var reportView = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain only one failure.", 1, reportView.Count);

			var reportViewRow = reportView.Single();
			AssertRowData(order_failed, package_failed, data.Part1, firstPickLine, 20m, 20m, 18m, audit_failed,
				reportViewRow);
		}

		public void TestView_DeletedAuditedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 35m);
			var package = AddPackageToOrder(order, "PAK1", "PLT");
			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);

			var audit = Helper.CreateWhsPackageAuditWithLineFailure(order, package.KP_PackageID, data.Part1, 35m, 15m,
				completeTimeoffset);
			Factory.Save();

			AssertNull("Precondition", order.Pick);
			AssertEquals("Precondition", 0, order.Lines.Single().PickLines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			var reportView = Load_AuditVarianceReport(data.Org1);
			var reportViewRow = reportView.Single();
			AssertEquals("The view should contain only one failure.", 1, reportView.Count);
			AssertRowData(order, package, data.Part1, null, 0m, 35m, 15m, audit, reportViewRow);
		}

		public void TestView_MultipleLocationsPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "LOC01", 2, 2);
			Factory.Save();

			var location01 = data.Whs1.FindLocation("LOC01-1-1");
			var location02 = data.Whs1.FindLocation("LOC01-2-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m, location01, "PAL01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location02, "PAL02");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_RequiredDate = ZDateTimeOffset.Today;
			order.ConsigneePK = Helper.CreateClient("CONSIGNEE").PK;

			var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 35m);
			var pick = Helper.CreatePickNew(order);
			var pickLine01 = orderLine.PickLines.Single(pl => pl.WZ_Units == 25m);
			var pickLine02 = orderLine.PickLines.Single(pl => pl.WZ_Units == 10m);
			var package = AddPackageToOrder(order, "PAK1", "PLT");
			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);

			var audit = Helper.CreateWhsPackageAuditWithLineFailure(order, package.KP_PackageID, data.Part1, 35m, 15m,
				completeTimeoffset);
			audit.WPA_GS_NKAuditor = auditor.GS_Code;

			// this test is testing what happens with multiple locations, regardless of in-transit transfers
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			var viewResult = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain two lines.", 2, viewResult.Count);

			var viewRow01 = viewResult.Where(v => v["PickedLocation"].ToString() == "LOC01-1-1")
				.Single();
			var viewRow02 = viewResult.Where(v => v["PickedLocation"].ToString() == "LOC01-2-2")
				.Single();
			AssertRowData(order, package, data.Part1, pickLine01, 25m, 35m, 15m, audit, viewRow01);
			AssertRowData(order, package, data.Part1, pickLine02, 10m, 35m, 15m, audit, viewRow02);
		}

		public void TestView_MultiplePickLinesForSameProductAndLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline01 = Helper.CreateWhsOrderLine(order, data.Part1, 25m);
			var orderline02 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().First();
			var package = AddPackageToOrder(order, "PAK1", "PLT");
			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);

			var audit = Helper.CreateWhsPackageAuditWithLineFailure(order, package.KP_PackageID, data.Part1, 35m, 15m,
				completeTimeoffset);
			audit.WPA_GS_NKAuditor = auditor.GS_Code;

			// this test is testing what happens with multiple picklines for same location, regardless of in-transit transfers
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition", 2, order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			var viewResult = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain one single line.", 1, viewResult.Count);

			var viewRow = viewResult.Single();
			AssertRowData(order, package, data.Part1, pickLine, 35m, 35m, 15m, audit, viewRow);
		}

		public void TestView_MultiplePickersForSameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			var picker1 = Helper.CreateGlbStaff("PI1", "Picker1");
			var picker2 = Helper.CreateGlbStaff("PI2", "Picker2");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC2", data.Part1, 7m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 12m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines.Single().PickLines.Single(pl => pl.WZ_Units == 5m);
			var pickLine2 = order.Lines.Single().PickLines.Single(pl => pl.WZ_Units == 7m);
			var package = AddPackageToOrder(order, "PAK1", "PLT");
			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);

			var audit = Helper.CreateWhsPackageAuditWithLineFailure(order, "PAK1", data.Part1, 12m, 8m,
				completeTimeoffset);

			pickLine1.WZ_GS_NKAssignedTo = "PI1";
			pickLine2.WZ_GS_NKAssignedTo = "PI2";
			audit.WPA_GS_NKAuditor = auditor.GS_Code;

			// this test is testing what happens with multiple pickers, regardless of in-transit transfers
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition", picker1.PK, pickLine1.AssignedTo.PK);
			AssertEquals("Precondition", picker2.PK, pickLine2.AssignedTo.PK);
			AssertEquals("Precondition", 1, order.Lines.Count);
			AssertEquals("Precondition", 2, order.Lines.Single().PickLines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			var viewResult = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain two lines.", 2, viewResult.Count);

			var viewRow01 = viewResult.Where(v => v["PickerPK"].ToString() == picker1.PK.ToString()).Single();
			var viewRow02 = viewResult.Where(v => v["PickerPK"].ToString() == picker2.PK.ToString()).Single();
			AssertRowData(order, package, data.Part1, pickLine1, 5m, 12m, 8m, audit, viewRow01);
			AssertRowData(order, package, data.Part1, pickLine2, 7m, 12m, 8m, audit, viewRow02);
		}

		public void TestView_FixedWidthLocationWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZZ", 3, 2, 2);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			Helper.CreateRowAndGenerateLocations(warehouse, "LOC01", 4, 3, 2);
			Factory.Save();

			var location01 = warehouse.FindLocation("LOC010010101");
			Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", data.Part1, 25m, location01, "PAL01");
			var location02 = warehouse.FindLocation("LOC010040302");
			Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R2", data.Part1, 10m, location02, "PAL02");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, warehouse);
			order.WD_RequiredDate = ZDateTimeOffset.Today;
			order.ConsigneePK = Helper.CreateClient("CONSIGNEE").PK;

			var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 35m);
			var pick = Helper.CreatePickNew(order);
			var pickLine01 = orderLine.PickLines.Single(pl => pl.WZ_Units == 25m);
			var pickLine02 = orderLine.PickLines.Single(pl => pl.WZ_Units == 10m);
			var package = AddPackageToOrder(order, "PAK1", "PLT");
			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);

			var audit = Helper.CreateWhsPackageAuditWithLineFailure(order, package.KP_PackageID, data.Part1, 35m, 15m,
				completeTimeoffset);
			audit.WPA_GS_NKAuditor = auditor.GS_Code;

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			var viewResult = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain two lines.", 2, viewResult.Count);

			var viewRow01 = viewResult.Where(v => v["PickedLocation"].ToString() == "LOC01-001-01-01").Single();
			AssertRowData(order, package, data.Part1, pickLine01, 25m, 35m, 15m, audit, viewRow01);

			var viewRow02 = viewResult.Where(v => v["PickedLocation"].ToString() == "LOC01-004-03-02").Single();
			AssertRowData(order, package, data.Part1, pickLine02, 10m, 35m, 15m, audit, viewRow02);
		}

		public void TestView_EmptyPackageAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var package = AddPackageToOrder(order, "PAK1", "PLT");
			var audit = Helper.CreateWhsPackageAudit(order, "PAK1", completeTimeoffset);
			audit.WPA_GS_NKAuditor = auditor.GS_Code;

			// this test is testing without in-transit transfers
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var reportView = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain only one success.", 1, reportView.Count);

			var reportViewRow = reportView.Single();
			AssertRowData(order, package, data.Part1, null, 10m, 10m, 10m, audit,
				reportViewRow);
		}

		public void TestView_AuditFailureForProductNotInWhsOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 30m);
			Factory.Save();

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PAK1", "PLT");
			var audit = Helper.CreateWhsPackageAudit(order, "PAK1", completeTimeoffset);
			Helper.CreateWhsPackageAuditLineFailure(audit, data.Part1, 20m, 18m);
			Helper.CreateWhsPackageAuditLineFailure(audit, data.Part2, 30m, 27m);
			Helper.CreateWhsPackageAuditLineFailure(audit, part3, 10m, 8m);
			Helper.CreateWhsPackageAuditLineFailure(audit, part4, 15m, 12m);
			audit.WPA_GS_NKAuditor = auditor.GS_Code;

			// this test is testing without in-transit transfers
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var reportView = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain only one success.", 4, reportView.Count);

			var reportViewRow1 = reportView.Where(x => x["ProductCode"].ToString() == "P1").Single();
			AssertRowData(order, package, data.Part1, orderLine1.PickLines[0], 20m, 20m, 18m, audit,
				reportViewRow1);

			var reportViewRow2 = reportView.Where(x => x["ProductCode"].ToString() == "P2").Single();
			AssertRowData(order, package, data.Part2, orderLine2.PickLines[0], 30m, 30m, 27m, audit,
				reportViewRow2);

			var reportViewRow3 = reportView.Where(x => x["ProductCode"].ToString() == "P3").Single();
			AssertRowData(order, package, part3, null, 0m, 10m, 8m, audit,
				reportViewRow3);

			var reportViewRow4 = reportView.Where(x => x["ProductCode"].ToString() == "P4").Single();
			AssertRowData(order, package, part4, null, 0m, 15m, 12m, audit,
				reportViewRow4);
		}

		public void TestView_SuccessfulAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("HAN", "Han Solo");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order_passed = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD2", data.Part1, 10m);

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var pick_passed = Helper.CreatePickNew(order_passed);
			var pickLine_passed = order_passed.Lines[0].PickLines.Single(pl => pl.WZ_Units == 10m);
			var package_passed = AddPackageToOrder(order_passed, "PAK2", "PLT");
			var audit_passed = Helper.CreateWhsPackageAudit(order_passed, "PAK2", completeTimeoffset);
			audit_passed.WPA_GS_NKAuditor = auditor.GS_Code;

			// this test is testing without in-transit transfers
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var reportView = Load_AuditVarianceReport(data.Org1);
			AssertEquals("The view should contain only one success.", 1, reportView.Count);

			var reportViewRow = reportView.Single();
			AssertRowData(order_passed, package_passed, data.Part1, null, 10m, 10m, 10m, audit_passed,
				reportViewRow);
		}

		#endregion

		#region TestSupportMethods

		PkgPackage AddPackageToOrder(WhsOrder order, ZString packageId, ZString packageType)
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, packageId, 1, packageType);

			foreach (var pickLine in order.Lines.SelectMany(l => l.PickLines))
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
				PackingHelper.CreatePackageDivot(package, pickLine);
			}

			return package;
		}

		DynamicBusinessObjectCollection Load_AuditVarianceReport(OrgHeader client)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"
						SELECT *
						FROM dbo.WhsAuditVarianceReport
						WHERE OH_PK = @ClientPK";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ClientPK", client.PK, OrgHeaderSchema.PK);

			AssertNoExceptionThrown("Error when trying to retrieve the view: dbo.WhsAuditVarianceReport",
				() => { result.Load(sql, sqlParams); });
			return result;
		}

		void AssertRowData(WhsDocket order, PkgPackage package, OrgSupplierPart product, WhsPickLine pickline,
			ZDecimal pickedQuantity, ZDecimal expectedQuantity, ZDecimal auditedQuantity, WhsPackageAudit audit,
			DynamicBusinessObject actual)
		{
			var expectedVariance = auditedQuantity - expectedQuantity;
			var actualVariance = ZDecimal.Parse(actual["VarianceQuantity"].ToString());
			string expectedPackageStatus;

			var isSuccessfulAuditInOneTime = expectedVariance == 0;

			if (package.KP_IsHeld)
			{
			   expectedPackageStatus = "Held";
			}
			else
			{
			   expectedPackageStatus = isSuccessfulAuditInOneTime ? "Successful" : "Not Held";
			}

			CombineAssertions(() =>
			{
				AssertEquals("OrgHeaderPK", order.WD_OH_Client, actual[OrgHeaderSchema.PK]);
				AssertEquals("PackageID", package.KP_PackageID, actual["PackageID"]);
				AssertEquals("PackageStatus", expectedPackageStatus, actual["PackageStatus"]);
				AssertEquals("DocketID", order.WD_DocketID, actual["DocketID"]);
				AssertEquals("ClientCode", order.Client.OH_Code, actual["ClientCode"]);
				AssertEquals("ClientFullName", order.Client.OH_FullName, actual["ClientFullName"]);
				AssertEquals("OrderReference", order.WD_ExternalReference, actual["OrderReference"]);
				AssertEquals("AuditTime", audit.WPA_AuditCompleteTime, actual["AuditTime"]);
				AssertEquals("VarianceQuantity", expectedVariance, actualVariance);
				AssertEquals("AuditorPK", audit.Auditor.PK, actual["AuditorPK"]);
				AssertEquals("AuditorLoginName", audit.Auditor.GS_LoginName, actual["AuditorLoginName"]);
				AssertEquals("WarehousePK", order.WD_WW_Whs, actual[WhsWarehouseSchema.PK]);
				AssertEquals("WarehouseName", order.Warehouse.WW_WarehouseName, actual["WarehouseName"]);

				if (isSuccessfulAuditInOneTime)
				{
					AssertNullOrEmpty("Product", actual["ProductCode"].ToString());
					AssertNullOrEmpty("ProductDescription", actual["ProductDescription"].ToString());
					AssertEquals("PickedQuantity", 0m, actual["PickedQuantity"]);
					AssertEquals("ExpectedQuantity", 0m, actual["ExpectedQuantity"]);
					AssertEquals("AuditedQuantity", 0m, actual["AuditedQuantity"]);
					AssertEquals("OrgSupplierPartSchemaPK", ZGuid.Empty, actual[OrgSupplierPartSchema.PK]);
				}
				else
				{
					AssertEquals("Product", product.OP_PartNum, actual["ProductCode"]);
					AssertEquals("ProductDescription", product.OP_Desc, actual["ProductDescription"]);
					AssertEquals("PickedQuantity", pickedQuantity, actual["PickedQuantity"]);
					AssertEquals("ExpectedQuantity", expectedQuantity, actual["ExpectedQuantity"]);
					AssertEquals("AuditedQuantity", auditedQuantity, actual["AuditedQuantity"]);
					AssertEquals("OrgSupplierPartSchemaPK", product.PK, actual[OrgSupplierPartSchema.PK]);
				}

				if (pickline != null)
				{
					AssertEquals("PickLocation", pickline.InventoryLine.LocationString, actual["PickedLocation"]);
					AssertEquals("PickTime", pickline.WZ_PickedDateTime, actual["PickTime"]);
					AssertEquals("PickerPK", pickline.AssignedTo.PK, actual["PickerPK"]);
					AssertEquals("PickerLoginName", pickline.AssignedTo.GS_LoginName, actual["PickerLoginName"]);
				}
				else
				{
					AssertNullOrEmpty("PickLocation", actual["PickedLocation"].ToString());
					AssertNullOrEmpty("PickTime", actual["PickTime"].ToString());
					AssertEquals("PickerPK", ZGuid.Empty.ToString(), actual["PickerPK"].ToString());
					AssertNullOrEmpty("PickerLoginName", actual["PickerLoginName"].ToString());
				}
			});
		}

		#endregion

		#region Implementation

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion
	}
}
