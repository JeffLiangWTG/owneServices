namespace Enterprise.ReportTesting.Warehouse
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using DocumentEngine.DataProviders;
	using DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Warehouse.Integration;
	using Enterprise.Warehouse.Transactions.Business;
	using Enterprise.Warehouse.Transactions.Business.Testing;
	using Enterprise.Warehouse.Transactions.Module;
	using Packing.Business;
	using Packing.Business.Testing;
	using ReportTesting;
	using ZArchitecture.Schema;

	[TemplateName("Whs Audit Variance Report")]
	public class TestWhsAuditVarianceReport : WhsTemplateTestCase
	{
		#region TestReportFilters

		#region TestReportFilter_Warehouse

		public void TestReportFilter_Warehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var whsForFilter = Helper.CreateWarehouse("W99", "Whs99");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, whsForFilter, data.Part1, auditor, auditor, BrettsBirthdayOffset.AddDays(2), "PakWhs", 10, 13);

			CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);
			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Warehouse"]).ValueAsStringForSerialisation = whsForFilter.PK.ToString();
			RunReport();
			AssertSingleFilteredRow("Error in Warehouse filter", audit, "Whs99");
		}

		public void TestReportFilter_Warehouse_EmptyReport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var warehouse = Helper.CreateWarehouse("W99", "Whs99");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Warehouse"]).ValueAsStringForSerialisation = warehouse.PK.ToString();
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Report should be empty.", 0, rowSource.RowCount);
		}

		#endregion

		#region TestReportFilter_Client

		public void TestReportFilter_Client()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var orgForFilter = Helper.CreateClient("C99", "Client99");
			var partForFilter = Helper.CreateProduct(orgForFilter, "PART99");
			var audit = CreateOrderAndPackageAuditForFilterTest(orgForFilter, data.Whs1, partForFilter, auditor, auditor, BrettsBirthdayOffset.AddDays(2), "PakCli", 11, 14);

			CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);
			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = orgForFilter.PK.ToString();
			RunReport();
			AssertSingleFilteredRow("Error in Client filter", audit, "A");
		}

		public void TestReportFilter_Client_EmptyReport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var client = Helper.CreateClient("C99", "Client99");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = client.PK.ToString();
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Report should be empty.", 0, rowSource.RowCount);
		}

		#endregion

		#region TestReportFilter_Product

		public void TestReportFilter_Product()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var partOwner = Helper.CreateClient("C99", "Client99");
			var product = Helper.CreateProduct(partOwner, "PART99");
			var audit = CreateOrderAndPackageAuditForFilterTest(partOwner, data.Whs1, product, auditor, auditor, BrettsBirthdayOffset.AddDays(2), "PakPrd", 18, 22);

			CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);
			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Product"]).ValueAsStringForSerialisation = product.PK.ToString();
			RunReport();
			AssertSingleFilteredRow("Error in Product filter", audit, "A");
		}

		public void TestReportFilter_Product_EmptyReport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var product = Helper.CreateProduct(data.Org1, "PART99");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Product"]).ValueAsStringForSerialisation = product.PK.ToString();
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Report should be empty.", 0, rowSource.RowCount);
		}

		#endregion

		#region TestReportFilter_Package

		public void TestReportFilter_Package()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor, auditor, BrettsBirthdayOffset.AddDays(2), "PakPkg", 18, 22);

			CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);
			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((TextField)Report.FilterCollection["Package"]).Value = "PakPkg";
			RunReport();
			AssertSingleFilteredRow("Error in Package filter", audit, "A");
		}

		public void TestReportFilter_Package_EmptyReport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((TextField)Report.FilterCollection["Package"]).Value = "PakPkg";
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Report should be empty.", 0, rowSource.RowCount);
		}

		#endregion

		#region TestReportFilter_Picker

		public void TestReportFilter_Picker()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var picker1 = Helper.CreateGlbStaff("PI3", "Picker");
			var picker2 = Helper.CreateGlbStaff("BLR", "Blair");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor, picker2, BrettsBirthdayOffset.AddDays(2), "PakPic", 18, 22);

			CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, picker1, BrettsBirthdayOffset, "Pkg1", 20, 22);
			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Picker"]).ValueAsStringForSerialisation = picker2.PK.ToString();
			RunReport();
			AssertSingleFilteredRow("Error in Picker filter", audit, "A");
		}

		public void TestReportFilter_Picker_EmptyReport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var picker1 = Helper.CreateGlbStaff("PI3", "Picker");
			var picker2 = Helper.CreateGlbStaff("BLR", "Blair");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, picker1, BrettsBirthdayOffset, "Pkg1", 20, 22);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Picker"]).ValueAsStringForSerialisation = picker2.PK.ToString();
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Report should be empty.", 0, rowSource.RowCount);
		}

		#endregion

		#region TestReportFilter_Auditor

		public void TestReportFilter_Auditor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor1 = Helper.CreateGlbStaff("AUD", "Auditor");
			var auditor2 = Helper.CreateGlbStaff("BLA", "Blair");
			var picker = Helper.CreateGlbStaff("PI3", "Picker");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor2, picker, BrettsBirthdayOffset.AddDays(2), "PakAud", 18, 22);
			CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor1, picker, BrettsBirthdayOffset, "Pkg1", 20, 22);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Auditor"]).ValueAsStringForSerialisation = auditor2.PK.ToString();
			RunReport();
			AssertSingleFilteredRow("Error in Auditor filter", audit, "A");
		}

		public void TestReportFilter_Auditor_EmptyReport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor1 = Helper.CreateGlbStaff("AUD", "Auditor");
			var auditor2 = Helper.CreateGlbStaff("BLA", "Blair");
			var picker = Helper.CreateGlbStaff("PI3", "Picker");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor1, picker, BrettsBirthdayOffset, "Pkg1", 20, 22);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((LookupField)Report.FilterCollection["Auditor"]).ValueAsStringForSerialisation = auditor2.PK.ToString();
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Report should be empty.", 0, rowSource.RowCount);
		}

		#endregion

		#region TestReportFilter_DateRange

		public void TestReportFilter_DateRange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor, auditor, BrettsBirthdayOffset.AddDays(50), "PakDat", 18, 22);

			CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);
			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((DateRangeField)Report.FilterCollection["Date Range"]).ValueLow = ZDateTime.BrettsBirthday.AddDays(45);
			((DateRangeField)Report.FilterCollection["Date Range"]).ValueHigh = ZDateTime.BrettsBirthday.AddDays(55);
			RunReport();
			AssertSingleFilteredRow("Error in Date Range filter", audit, "A");
		}

		public void TestReportFilter_DateRange_EmptyReport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((DateRangeField)Report.FilterCollection["Date Range"]).ValueLow = ZDateTime.BrettsBirthday.AddDays(1);
			((DateRangeField)Report.FilterCollection["Date Range"]).ValueHigh = ZDateTime.BrettsBirthday.AddDays(5);
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Report should be empty.", 0, rowSource.RowCount);
		}

		#endregion

		#region TestReportFilter_PackageIsHeld

		public void TestReportFilter_PackageIsHeld_HeldPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor, auditor, BrettsBirthdayOffset.AddDays(2), "PakVar", 30, 46);

			CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset, "Pkg1", 20, 22, holdPackage: false);
			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((MultipleChoice)Report.FilterCollection["Package Status"]).ValueAsStringForSerialisation = "Held";
			RunReport();
			AssertSingleFilteredRow("Error in Package Hold Status filter for held packages.", audit, "A");
		}

		public void TestReportFilter_PackageIsHeld_NotHeldPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var auditHeld = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor, auditor, BrettsBirthdayOffset, "PakVar", 30, 46);
			var auditNotHeld = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset.AddDays(2), "Pkg1", 20, 22, holdPackage: false);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, auditNotHeld.Order.Lines.Count);
			AssertEquals("Precondition", 1, auditHeld.Order.Lines.Count);

			((MultipleChoice)Report.FilterCollection["Package Status"]).ValueAsStringForSerialisation = "Not Held";
			RunReport();
			AssertSingleFilteredRow("Error in Package Hold Status filter for not held packages.", auditNotHeld, "A");
		}

		public void TestReportFilter_PackageIsHeld_SuccessfulPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var auditHeld = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor, auditor, BrettsBirthdayOffset, "PakVar", 30, 46);
			var auditNotHeld = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset.AddDays(2), "Pkg1", 20, 22, holdPackage: false);
			var auditSuccessful = CreateOrderAndSuccessfulPackageAuditForFilterTest(data.Org1, data.Whs1, part3, auditor, auditor, BrettsBirthdayOffset, "Pkg2", 20, 20, holdPackage: false);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, auditNotHeld.Order.Lines.Count);
			AssertEquals("Precondition", 1, auditHeld.Order.Lines.Count);
			AssertEquals("Precondition", 1, auditSuccessful.Order.Lines.Count);

			((MultipleChoice)Report.FilterCollection["Package Status"]).ValueAsStringForSerialisation = "Successful";
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Show only 1 report for successful package audit", 1, rowSource.RowCount);
			var itemArray = rowSource.RowByIndex(0).ItemArray;
			CombineAssertions("Error in Package Hold Status filter for successful packages.", () =>
			{
				AssertNullOrEmpty("ProductCode", itemArray[ReportColumns["ProductCode"]].ToString());
				AssertEquals("WarehouseName is incorrect.", auditSuccessful.Order.Warehouse.WW_WarehouseName, itemArray[ReportColumns["WarehouseName"]]);
				AssertEquals("ClientFullName is incorrect.", auditSuccessful.Order.Client.OH_FullName, itemArray[ReportColumns["ClientFullName"]]);
				AssertEquals("AuditorLoginName is incorrect.", auditSuccessful.Auditor.GS_LoginName, itemArray[ReportColumns["AuditorLoginName"]]);
				AssertEquals("AuditTime is incorrect.", auditSuccessful.WPA_AuditCompleteTime.ToDateTimeOffset(), itemArray[ReportColumns["AuditTime"]]);
				AssertEquals("PackageID is incorrect.", auditSuccessful.WPA_PackageID, itemArray[ReportColumns["PackageID"]]);
				AssertEquals("PackageHeldStatus is incorrect.", "Successful", itemArray[ReportColumns["PackageHeldStatus"]]);
				AssertEquals("DocketID is incorrect.", auditSuccessful.Order.WD_DocketID, itemArray[ReportColumns["DocketID"]]);
				AssertEquals("OrderReference is incorrect.", auditSuccessful.Order.WD_ExternalReference, itemArray[ReportColumns["OrderReference"]]);
			});
		}

		public void TestReportFilter_PackageIsHeld_BothHeldAndNotHeldPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var auditHeld = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor, auditor, BrettsBirthdayOffset, "PakVar", 30, 46);
			var auditNotHeld = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part1, auditor, auditor, BrettsBirthdayOffset.AddDays(2), "Pkg1", 20, 22, holdPackage: false);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, auditNotHeld.Order.Lines.Count);
			AssertEquals("Precondition", 1, auditHeld.Order.Lines.Count);

			((MultipleChoice)Report.FilterCollection["Package Status"]).ValueAsStringForSerialisation = "";
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Filtered report should show 2 data rows when using empty Package Hold Status filter.", 2, rowSource.RowCount);
			AssertFilteredRow("Error in Package Hold Status filter for empty value - Row 1", rowSource.RowByIndex(0).ItemArray, auditHeld, "A");
			AssertFilteredRow("Error in Package Hold Status filter for empty value - Row 2", rowSource.RowByIndex(1).ItemArray, auditNotHeld, "A");
		}

		public void TestReportFilter_PackageIsHeld_EmptyReportHeldPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");
			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor, auditor, BrettsBirthdayOffset, "PakVar", 30, 46, holdPackage: false);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((MultipleChoice)Report.FilterCollection["Package Status"]).ValueAsStringForSerialisation = "Held";
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Report should be empty.", 0, rowSource.RowCount);
		}

		public void TestReportFilter_PackageIsHeld_EmptyReportNotHeldPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var auditor = Helper.CreateGlbStaff("AUD", "Auditor");

			var audit = CreateOrderAndPackageAuditForFilterTest(data.Org1, data.Whs1, data.Part2, auditor, auditor, BrettsBirthdayOffset, "PakVar", 30, 46);

			PrepareReportForRender();
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals("Precondition", 1, audit.PackageAuditFailureLines.Count);

			((MultipleChoice)Report.FilterCollection["Package Status"]).ValueAsStringForSerialisation = "Not Held";
			RunReport();

			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Report should be empty.", 0, rowSource.RowCount);
		}

		#endregion

		#endregion

		#region TestAssertions

		void AssertSingleFilteredRow(ZString errorMessage, WhsPackageAudit audit, string locationName)
		{
			var rowSource = (ReportDataSource)Report.DataProvider.GetDataRowSource("ReportData");
			AssertEquals("Precondition", 1, audit.Order.Lines.Count);
			AssertEquals(errorMessage + ": Filtered report should show only 1 data row.", 1, rowSource.RowCount);

			var itemArray = rowSource.RowByIndex(0).ItemArray;
			AssertFilteredRow(errorMessage, itemArray, audit, locationName);
		}

		void AssertFilteredRow(ZString errorMessage, object[] reportRow, WhsPackageAudit audit, string locationName)
		{
			var auditLine = audit.PackageAuditFailureLines.Single();
			var pickLineWithPickTime = audit.Order.Lines.Single().PickLines.Single().InventoryLine.PickLines.Single();
			var package = audit.Order.PackageJob.Packages.Single();

			var expectedVarianceQuantity = auditLine.WPF_AuditedQty - auditLine.WPF_ExpectedQty;
			var expectedPackageHeldStatus = package.KP_IsHeld ? "Held" : "Not Held";

			CombineAssertions(errorMessage, () =>
			{
				AssertEquals("WarehouseName is incorrect.", audit.Order.Warehouse.WW_WarehouseName, reportRow[ReportColumns["WarehouseName"]]);
				AssertEquals("ClientFullName is incorrect.", audit.Order.Client.OH_FullName, reportRow[ReportColumns["ClientFullName"]]);
				AssertEquals("AuditorLoginName is incorrect.", audit.Auditor.GS_LoginName, reportRow[ReportColumns["AuditorLoginName"]]);
				AssertEquals("PickerLoginName is incorrect.", pickLineWithPickTime.AssignedTo.GS_LoginName, reportRow[ReportColumns["PickerLoginName"]]);
				AssertEquals("AuditedQuantity is incorrect.", auditLine.WPF_AuditedQty, reportRow[ReportColumns["AuditedQuantity"]]);
				AssertEquals("PickedQuantity is incorrect.", pickLineWithPickTime.WZ_Units, reportRow[ReportColumns["PickedQuantity"]]);
				AssertEquals("ProductCode is incorrect.", auditLine.SupplierPart.OP_PartNum, reportRow[ReportColumns["ProductCode"]]);
				AssertEquals("AuditTime is incorrect.", audit.WPA_AuditCompleteTime.ToDateTimeOffset(), reportRow[ReportColumns["AuditTime"]]);
				AssertEquals("PickedTime is incorrect.", pickLineWithPickTime.WZ_PickedDateTime, reportRow[ReportColumns["PickedTime"]]);
				AssertEquals("PickedLocation is incorrect.", locationName, reportRow[ReportColumns["PickedLocation"]]);
				AssertEquals("PackageID is incorrect.", audit.WPA_PackageID, reportRow[ReportColumns["PackageID"]]);
				AssertEquals("PackageHeldStatus is incorrect.", expectedPackageHeldStatus, reportRow[ReportColumns["PackageHeldStatus"]]);
				AssertEquals("DocketID is incorrect.", audit.Order.WD_DocketID, reportRow[ReportColumns["DocketID"]]);
				AssertEquals("OrderReference is incorrect.", audit.Order.WD_ExternalReference, reportRow[ReportColumns["OrderReference"]]);
				AssertEquals("VarianceQuantity is incorrect.", expectedVarianceQuantity, reportRow[ReportColumns["VarianceQuantity"]]);
			});
		}

		#endregion

		#region TestDataSetup

		WhsPackageAudit CreateOrderAndPackageAuditForFilterTest(OrgHeader client, IWhsWarehouse warehouse, OrgSupplierPart part, GlbStaff auditor, GlbStaff picker, ZDateTimeOffset completeTime, string packageNum, decimal expectedQty, decimal auditedQty, bool holdPackage = true)
		{
			Helper.CreateWhsReceiveWithInventory(client, warehouse, packageNum, part, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, packageNum, part, expectedQty);
			Helper.CreatePickNew(order);

			AddPackageToOrderAndPick(order, packageNum, "PLT", picker, completeTime, holdPackage);

			var audit = Helper.CreateWhsPackageAuditWithLineFailure(order, packageNum, part, expectedQty, auditedQty, completeTime);
			audit.WPA_GS_NKAuditor = auditor.GS_Code;
			Factory.Save();

			return audit;
		}

		WhsPackageAudit CreateOrderAndSuccessfulPackageAuditForFilterTest(OrgHeader client, IWhsWarehouse warehouse, OrgSupplierPart part, GlbStaff auditor, GlbStaff picker, ZDateTimeOffset completeTime, string packageNum, decimal expectedQty, decimal auditedQty, bool holdPackage = true)
		{
			Helper.CreateWhsReceiveWithInventory(client, warehouse, packageNum, part, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, packageNum, part, expectedQty);
			Helper.CreatePickNew(order);

			AddPackageToOrderAndPick(order, packageNum, "PLT", picker, completeTime, holdPackage);

			var audit = Helper.CreateWhsPackageAudit(order, packageNum, completeTime);
			audit.WPA_GS_NKAuditor = auditor.GS_Code;
			Factory.Save();

			return audit;
		}

		PkgPackage AddPackageToOrderAndPick(WhsOrder order, ZString packageId, ZString packageType, GlbStaff picker, ZDateTimeOffset pickedTime, bool holdPackage)
		{
			var packageJob = LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, packageId, 1, packageType);
			foreach (var orderLine in order.Lines)
			{
				var pickLine = orderLine.PickLines.Single();
				var divot = package.PackedItemDivots.AddNew();
				pickLine.WZ_GS_NKAssignedTo = picker.GS_Code;
				pickLine.WZ_PickedDateTime = pickedTime;
				divot.KI_ParentID = pickLine.PK;
				divot.KI_ParentTableCode = pickLine.TablePrefix;
				divot.KI_PackedQty = pickLine.WZ_Units;
			}

			package.KP_IsHeld = holdPackage;
			return package;
		}

		PkgPackageJob LoadOrCreatePackageJob(WhsOrder order)
		{
			var packageJob = order.Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, order.PK));

			if (packageJob == null)
			{
				packageJob = order.Factory.New<PkgPackageJob>();
				packageJob.KJ_ParentID = order.PK;
				packageJob.KJ_ParentTableCode = order.TablePrefix;
				packageJob.KJ_IsFinalized = order.IsFinalised;
			}

			return packageJob;
		}

		#endregion

		#region Implementation

		ZDateTimeOffset BrettsBirthdayOffset => new ZDateTimeOffset(1971, 9, 18);

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		Dictionary<string, int> ReportColumns
		{
			get
			{
				return reportColumns ?? (reportColumns = new Dictionary<string, int>()
				{
					{ "WarehouseName", 0 },
					{ "ClientFullName", 1 },
					{ "PickerLoginName", 2 },
					{ "AuditorLoginName", 3 },
					{ "PickedQuantity", 4 },
					{ "PickedTime", 5 },
					{ "PickedLocation", 6 },
					{ "AuditedQuantity", 7 },
					{ "VarianceQuantity", 8 },
					{ "ProductCode", 9 },
					{ "ProductDescription", 10 },
					{ "AuditTime", 11 },
					{ "PackageID", 12 },
					{ "PackageHeldStatus", 13 },
					{ "DocketID", 14 },
					{ "OrderReference", 15 }
				});
			}
		}

		Dictionary<string, int> reportColumns;

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion
	}

	public class TestWhsAuditVarianceReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Audit Variance Report"; }
		}

		public override string Hint
		{
			get
			{
				return
					@"This report shows all the variances found as a result of auditing warehouse package contents. Only audits with package variances will be considered.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsAuditVarianceReport();
		}
	}
}
