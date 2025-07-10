using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetDefaultPrintingInfoTest : WhsSecureServiceTestCase
	{
		#region TestGetDefaultPrintingInfo

		public void TestGetDefaultPrintingInfo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PRINTER");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TEST1";
			var pickPackParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1, "KEG", 3, 2, false);

			Helper.Factory.Save();

			var targetLabelMenu = Helper.Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocumentForTesting);
			pickPackParams.WPP_IsPickAndPackEnabled = true;
			var docForPackageClosePK = "a301f4ff-e6b0-4b6f-a7c4-3de29ad8f750";

			// link document to doc pack
			var stmMenuMenuPivot = Helper.Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = targetLabelMenu.PK;
			stmMenuMenuPivot.SF_SU_Inward = new ZGuid(docForPackageClosePK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfo(order.WD_DocketID, new PackageForPrintingInfo { PackageID = "TEST1", PK = package.PK.ToGuid() });

			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertNull("No Error Mesage.", response.ErrorMessage);
				AssertEquals("There should be no default Printer.", Guid.Empty, response.DefaultPrinter);
				AssertEquals("There have Correct Num of Labels to Print on Close.", 3, response.NumberOfLabelsToPrintOnClose);
				AssertEquals("There have Correct Num of Labels to Print on New.", 2, response.NumberOfLabelsToPrintOnNew);
				AssertContainsExactElementsInAnyOrder(new[] { printer.PK }, response.Printers.Select(p => p.PK));
			});
		}

		public void TestGetDefaultPrintingInfo_DocketIDDoesNotExists()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfo(receive.WD_DocketID, new PackageForPrintingInfo());

			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"No Warehouse Order found with Docket ID: {receive.WD_DocketID}.", response.ErrorMessage);
		}

		public void TestGetDefaultPrintingInfo_PackageIDDoesNotExists()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TEST1";
			var pickPackParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1, "KEG", 3, 2, false);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfo(order.WD_DocketID, new PackageForPrintingInfo { PackageID = "ABC", PK = Guid.NewGuid() });

			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"No package found with package ID: ABC.", response.ErrorMessage);
		}

		public void TestGetDefaultPrintingInfo_PickPackParamDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PRINTER");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TEST1";

			var docForPackageClosePK = "a301f4ff-e6b0-4b6f-a7c4-3de29ad8f750";
			var targetLabelMenu = Helper.Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocumentForTesting);

			// link document to doc pack
			var stmMenuMenuPivot = Helper.Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = targetLabelMenu.PK;
			stmMenuMenuPivot.SF_SU_Inward = new ZGuid(docForPackageClosePK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfo(order.WD_DocketID, new PackageForPrintingInfo { PackageID = "TEST1", PK = package.PK.ToGuid() });

			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertNull("No Error message.", response.ErrorMessage);
				AssertEquals("There should be no default Printer.", Guid.Empty, response.DefaultPrinter);
				AssertEquals("Num of Labels to Print on Close is defaulted to 0.", 0, response.NumberOfLabelsToPrintOnClose);
				AssertEquals("Num of Labels to Print on New is defaulted to 0.", 0, response.NumberOfLabelsToPrintOnNew);
				AssertContainsExactElementsInAnyOrder(new[] { printer.PK }, response.Printers.Select(p => p.PK));
			});
		}

		public void TestGetDefaultPrintingInfo_PickPackParamExists_PickPackNotEnabled()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PRINTER");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TEST1";
			var pickPackParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1, "KEG", 3, 2, false);
			var docForPackageClosePK = "a301f4ff-e6b0-4b6f-a7c4-3de29ad8f750";
			var targetLabelMenu = Helper.Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocumentForTesting);

			// link document to doc pack
			var stmMenuMenuPivot = Helper.Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = targetLabelMenu.PK;
			stmMenuMenuPivot.SF_SU_Inward = new ZGuid(docForPackageClosePK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfo(order.WD_DocketID, new PackageForPrintingInfo { PackageID = "TEST1", PK = package.PK.ToGuid() });

			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertNull("No Error message.", response.ErrorMessage);
				AssertEquals("There should be no default Printer.", Guid.Empty, response.DefaultPrinter);
				AssertEquals("Num of Labels to Print on Close is defaulted to 0.", 0, response.NumberOfLabelsToPrintOnClose);
				AssertEquals("Num of Labels to Print on New is defaulted to 0.", 0, response.NumberOfLabelsToPrintOnNew);
				AssertContainsExactElementsInAnyOrder(new[] { printer.PK }, response.Printers.Select(p => p.PK));
			});
		}

		public void TestGetDefaultPrintingInfo_PickPackParamExists_PreventAutoDelivery()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PRINTER");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TEST1";
			var pickPackParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1, "KEG", 3, 2, false);

			Helper.Factory.Save();

			var targetLabelMenu = Helper.Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocumentForTesting);
			targetLabelMenu.SU_PreventAutoDelivery = true;
			pickPackParams.WPP_IsPickAndPackEnabled = true;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfo(order.WD_DocketID, new PackageForPrintingInfo { PackageID = "TEST1", PK = package.PK.ToGuid() });

			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error is returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("No labels to print.", 0, response.NumberOfLabelsToPrintOnClose);
			AssertEquals("No labels to print.", 0, response.NumberOfLabelsToPrintOnNew);
		}

		#endregion

		#region TestGetDefaultPrintingInfo_DefaultPrinter

		public void TestGetDefaultPrintingInfo_DefaultPrinter()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer1 = Helper.CreatePrintQueue("Warehouse");
			var printer2 = Helper.CreatePrintQueue("Dock Door Area");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TEST1";
			var pickPackParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1);
			var dockDoorArea = data.Whs1.DefaultOutboundDockDoorLocation.PickingArea;
			data.Whs1.RFPickPackPrinterPK = printer1.PK;
			dockDoorArea.RFPickPackPrinterPK = printer2.PK;

			var docForPackageClosePK = "a301f4ff-e6b0-4b6f-a7c4-3de29ad8f750";
			var targetLabelMenu = Helper.Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocumentForTesting);

			// link document to doc pack
			var stmMenuMenuPivot = Helper.Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = targetLabelMenu.PK;
			stmMenuMenuPivot.SF_SU_Inward = new ZGuid(docForPackageClosePK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfo(order.WD_DocketID, new PackageForPrintingInfo { PackageID = "TEST1", PK = package.PK.ToGuid() });

			AssertSuccessfulResponse(response, webService);
			AssertEquals("Should have correct default Printer.", printer2.PK.ToGuid(), response.DefaultPrinter);
		}

		#endregion

		#region TestGetDefaultPrintingInfo_DefaultPrinter_WhenOrderHasCBA

		public void TestGetDefaultPrintingInfo_DefaultPrinter_WhenPackageSupportsCarrierLabels()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PRINTER");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1);
			var package = order.PackageJob.Packages.AddNew("PLT", "TEST1");

			var docForPackageClosePK = "a301f4ff-e6b0-4b6f-a7c4-3de29ad8f750";
			var targetLabelMenu = Helper.Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocumentForTesting);

			// link document to doc pack
			var stmMenuMenuPivot = Helper.Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = targetLabelMenu.PK;
			stmMenuMenuPivot.SF_SU_Inward = new ZGuid(docForPackageClosePK);

			Helper.CreateDefaultPrinter(new ZGuid(docForPackageClosePK), GlbStaff.CurrentUser, printer.PK, 1);
			Helper.CreateDefaultPrinter(ZGuid.Empty, GlbStaff.CurrentUser, printer.PK, 1);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var packageInfo = new PackageForPrintingInfo { PackageID = "TEST1", PK = package.PK.ToGuid(), SupportsCarrierLabelIntegration = true };
			var response = webService.GetDefaultPrintingInfo(order.WD_DocketID, packageInfo);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Should not have falled back to the default Printer, since we are using Carrier Label Integration.", Guid.Empty, response.DefaultPrinter);
			AssertContainsExactElementsInAnyOrder(new[] { printer.PK }, response.Printers.Select(p => p.PK));
		}

		#endregion

		#region TestGetDefaultPrintingInfo_WithNullPackingInfo

		public void TestGetDefaultPrintingInfo_WithNullPackingInfo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PRINTER");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfo(order.WD_DocketID, null);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Invalid Request for Printing Information.", response.ErrorMessage);
		}

		#endregion

		#region TestGetDefaultPrintingInfo_WithNoPrinters

		public void TestGetDefaultPrintingInfo_WithNoPrinters()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TEST1";
			var pickPackParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1);
			var docForPackageClosePK = "a301f4ff-e6b0-4b6f-a7c4-3de29ad8f750";
			var targetLabelMenu = Helper.Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocumentForTesting);

			// link document to doc pack
			var stmMenuMenuPivot = Helper.Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = targetLabelMenu.PK;
			stmMenuMenuPivot.SF_SU_Inward = new ZGuid(docForPackageClosePK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfo(order.WD_DocketID, new PackageForPrintingInfo { PackageID = "TEST1", PK = package.PK.ToGuid() });

			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("No printers exist in the system.", response.ErrorMessage);
		}

		#endregion
	}
}
