using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Drawback7551DocLinePageCollection))]
	public class Drawback7551DocLinePageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<Drawback7551DocLinePageCollection>
	{
		public void TestDrawbackDocImportInvoiceLinePages()
		{
			var exportingCarrier = Factory.NewWithValidTestData<OrgHeader>();
			exportingCarrier.OH_FullName = "EXPORTER NAME";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.US_DRWIsForImportSection = true;
			line1.US_DRWIsForExportSection = false;
			line1.US_ImportEntryNo = "12345678901";
			line1.US_DRWPort = "4599";
			line1.US_DRWEntryDate = new ZDateTime(2008, 12, 2);
			line1.US_DRWCMCDIndicator = "D";
			line1.JI_Tariff = "654321";
			line1.JI_PartNo = "PARTNUMBER";
			line1.JI_InvoiceUQ = "L";
			line1.JI_InvoiceQuantity = 2.3456m;
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 1.2345m;
			line1.US_DRWClaimAmountOverriden_New = true;
			line1.DRWImportQuantity = 50;
			line1.DRWExportQuantity = 50;
			line1.DRWImportUQ = "KG";
			line1.DeclaredVFD = 100;
			line1.LineDuty = line1.Claims.DutyClaim.DeclaredAmount * 0.725m;

			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.US_DRWIsForImportSection = true;
			line2.US_DRWIsForExportSection = false;
			line2.US_ImportEntryNo = "";
			line2.US_DRWCertOfManufacture = "CM123456";
			line2.US_DRWCMCDIndicator = "M";
			line2.US_DRWDateRcvFrom = new ZDateTime(2008, 12, 3);
			line2.US_DRWDateUsedFrom = new ZDateTime(2008, 12, 4);
			line2.JI_Description = "LINE DESCRIPTION";
			line2.JI_PartNo = "LINE2 PARTNO.";
			line2.JI_InvoiceUQ = "KG";
			line2.JI_InvoiceQuantity = 2.5m;
			line2.US_DRWImportUQ = "NO";
			line2.US_DRWImportQuantity = 5m;
			line2.US_DRWClaimAmountOverriden_New = true;
			line2.DRWImportQuantity = 50;
			line2.DRWExportQuantity = 50;
			line2.DeclaredVFD = 200;
			line2.LineDuty = line2.Claims.DutyClaim.DeclaredAmount * 0.1m;

			JobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_LineNo = 3;
			line3.US_DRWIsForImportSection = true;
			line3.US_DRWIsForExportSection = true;
			line3.US_DRWCertOfManufacture = "CM666666";
			line3.JI_Description = "LINE1";
			line3.JI_InvoiceUQ = "KG";
			line3.JI_InvoiceQuantity = 2.5m;
			line3.JI_CustomsUnitQty = "NO";
			line3.JI_CustomsQuantity = 5m;
			line3.US_DRWExportDate = new ZDateTime(2008, 12, 9);
			line3.US_DRWExportAction = "D";
			line3.US_DRWExportID = "EXPINV1";
			line3.ExporterOrDestroyer.OrganisationPK = exportingCarrier.PK;
			line3.US_DRWExportDest = "AU";
			line3.US_ExportTariff = "234567";
			line3.US_DRWClaimAmountOverriden_New = true;
			line3.DRWExportQuantity = 2.5m;
			line3.DRWExportUQ = "KG";

			JobComInvoiceLine line4 = header.JobComInvoiceLines.AddNew();
			line4.JI_LineNo = 4;
			line4.US_DRWIsForImportSection = false;
			line4.US_DRWIsForExportSection = true;
			line4.JI_Description = "LINE1\r\nLINE2\r\nLINE3\r\nLINE4\r\nLINE5\r\nLINE6\r\nLINE7\r\nLINE8\r\nLINE9\r\n";

			JobComInvoiceLine line5 = header.JobComInvoiceLines.AddNew();
			line5.JI_LineNo = 5;
			line5.US_DRWIsForImportSection = false;
			line5.US_DRWIsForExportSection = true;
			line5.JI_Description = "LAST EXPORT LINE";

			Factory.Save();

			var supporter = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);

			AssertEquals("3 line", 3, supporter.DrawbackImportSectionLines.Count);
			AssertEquals("line 1, CMorImportEntryNo", "123-4567890-1", supporter.DrawbackImportSectionLines[0].CMorImportEntryNo);
			AssertEquals("line 1, US_DRWPort", "4599", supporter.DrawbackImportSectionLines[0].US_DRWPort);
			AssertEquals("line 1, US_DRWEntryDate", "12/02/2008", supporter.DrawbackImportSectionLines[0].US_DRWEntryDate);
			AssertEquals("line 1, CDIndicator", "Y", supporter.DrawbackImportSectionLines[0].CDIndicator);
			AssertEquals("line 1, ReceivedDates", "", supporter.DrawbackImportSectionLines[0].ReceivedDates);
			AssertEquals("line 1, UsedDates", "", supporter.DrawbackImportSectionLines[0].UsedDates);
			AssertEquals("line 1, JI_Tariff", "654321", supporter.DrawbackImportSectionLines[0].JI_Tariff);
			AssertEquals("line 1, DrawbackDocDescription", "Part No.: PARTNUMBER", supporter.DrawbackImportSectionLines[0].DrawbackDocDescription);
			AssertEquals("line 1, ImportQtyAndUnit", "50 KG", supporter.DrawbackImportSectionLines[0].ImportQtyAndUnit);
			AssertEquals("line 1, US_DRWValuePer", "2", supporter.DrawbackImportSectionLines[0].US_DRWValuePer);
			AssertEquals("line 1, US_DRWDutyRate", "72.500 %", supporter.DrawbackImportSectionLines[0].US_DRWDutyRateInPercentage);
			AssertEquals("line 1, US_DRWClaimDuty", "71.77", supporter.DrawbackImportSectionLines[0].US_DRWClaimDuty);

			AssertEquals("line 2, CMorImportEntryNo", "CM123456", supporter.DrawbackImportSectionLines[1].CMorImportEntryNo);
			AssertEquals("line 2, CDIndicator", "", supporter.DrawbackImportSectionLines[1].CDIndicator);
			AssertEquals("line 2, ReceivedDates", "120308", supporter.DrawbackImportSectionLines[1].ReceivedDates);
			AssertEquals("line 2, UsedDates", "120408", supporter.DrawbackImportSectionLines[1].UsedDates);
			AssertEquals("line 2, DrawbackDocDescription", "LINE DESCRIPTION, Part No.: LINE2 PARTNO.", supporter.DrawbackImportSectionLines[1].DrawbackDocDescription);
			AssertEquals("line 2, ImportQtyAndUnit", "50 NO", supporter.DrawbackImportSectionLines[1].ImportQtyAndUnit);
			AssertEquals("line 2, US_DRWValuePer", "4", supporter.DrawbackImportSectionLines[1].US_DRWValuePer);
			AssertEquals("line 2, US_DRWDutyRate", "10.000 %", supporter.DrawbackImportSectionLines[1].US_DRWDutyRateInPercentage);
			AssertEquals("line 2, US_DRWClaimDuty", "19.80", supporter.DrawbackImportSectionLines[1].US_DRWClaimDuty);

			AssertEquals("3 lines", 3, supporter.DrawbackExportSectionLines.Count);
			AssertEquals("export line 2, DrawbackDocDescription", "LINE1", supporter.DrawbackExportSectionLines[1].DrawbackDocDescription);
			AssertEquals("export line 2, InvoiceQtyAndUnits", "2.5 KG", supporter.DrawbackExportSectionLines[1].ExportQtyAndUnit);
			AssertEquals("export line 2, US_DRWExportAction", new ZDateTime(2008, 12, 09), supporter.DrawbackExportSectionLines[1].US_DRWExportDate);
			AssertEquals("export line 2, US_DRWExportAction", "D", supporter.DrawbackExportSectionLines[1].US_DRWExportAction);
			AssertEquals("export line 2, US_DRWExportID", "EXPINV1", supporter.DrawbackExportSectionLines[1].US_DRWExportID);
			AssertEquals("export line 2, US_DRWExporterName", "EXPORTER NAME", supporter.DrawbackExportSectionLines[1].US_DRWExporterName);
			AssertEquals("export line 2, US_DRWExportDest", "AU", supporter.DrawbackExportSectionLines[1].US_DRWExportDest);
			AssertEquals("export line 2, US_ExportTariff", "234567", supporter.DrawbackExportSectionLines[1].US_ExportTariff);
		}

		public void TestSortByDateAndExportID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var header = declaration.Invoices.AddNew();

			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.US_DRWIsForExportSection = true;
			line1.JI_Description = "TEST LINE";
			line1.US_DRWExportDate = ZDateTime.Today.AddDays(-10);
			line1.US_DRWExportAction = "D";
			line1.US_DRWExportID = "EXPINV1";
			line1.US_ExportTariff = "234567";
			line1.DRWExportQuantity = 10m;
			line1.DRWExportUQ = "KG";

			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.US_DRWIsForExportSection = true;
			line1.JI_Description = "SOMETHING FOR EXPORT";
			line1.US_DRWExportDate = ZDateTime.Today.AddDays(-10);
			line1.US_DRWExportAction = "D";
			line1.US_DRWExportID = "EXPINV1";
			line1.US_ExportTariff = "234567";
			line1.DRWExportQuantity = 10m;
			line1.DRWExportUQ = "KG";

			var line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_LineNo = 3;
			line3.US_DRWIsForExportSection = true;
			line3.JI_Description = "LINE1";
			line3.US_DRWExportDate = ZDateTime.Today.AddDays(-10);
			line3.US_DRWExportAction = "D";
			line3.US_DRWExportID = "EXPINV1";
			line3.US_ExportTariff = "234567";
			line3.DRWExportQuantity = 2.5m;
			line3.DRWExportUQ = "KG";

			var line4 = header.JobComInvoiceLines.AddNew();
			line4.JI_LineNo = 4;
			line4.US_DRWIsForExportSection = true;
			line4.JI_Description = "LINE4";
			line4.US_DRWExportDate = ZDateTime.Today.AddDays(-10);
			line4.US_DRWExportAction = "D";
			line4.US_DRWExportID = "1001245632";
			line4.US_ExportTariff = "234567";
			line4.DRWExportQuantity = 8m;
			line4.DRWExportUQ = "NO";

			var line5 = header.JobComInvoiceLines.AddNew();
			line5.JI_LineNo = 5;
			line5.US_DRWIsForExportSection = true;
			line5.JI_Description = "LINE5";
			line5.US_DRWExportDate = ZDateTime.Today.AddDays(-10);
			line5.US_DRWExportAction = "D";
			line5.US_DRWExportID = "1001245632";
			line5.US_ExportTariff = "60064563";
			line5.DRWExportQuantity = 20m;
			line5.DRWExportUQ = "NO";

			var line6 = header.JobComInvoiceLines.AddNew();
			line6.JI_LineNo = 6;
			line6.US_DRWIsForExportSection = true;
			line6.JI_Description = "LINE6";
			line6.US_DRWExportDate = ZDateTime.Today.AddDays(-5);
			line6.US_DRWExportAction = "D";
			line6.US_DRWExportID = "A00100232";
			line6.US_ExportTariff = "60064563";
			line6.DRWExportQuantity = 20m;
			line6.DRWExportUQ = "NO";

			var line7 = header.JobComInvoiceLines.AddNew();
			line7.JI_LineNo = 7;
			line7.US_DRWIsForExportSection = true;
			line7.JI_Description = "LINE 7";
			line7.US_DRWExportDate = ZDateTime.Today.AddDays(-5);
			line7.US_DRWExportAction = "D";
			line7.US_DRWExportID = "A00100232";
			line7.US_ExportTariff = "60064563";
			line7.DRWExportQuantity = 20m;
			line7.DRWExportUQ = "NO";

			var line8 = header.JobComInvoiceLines.AddNew();
			line8.JI_LineNo = 8;
			line8.US_DRWIsForExportSection = true;
			line8.JI_Description = "LINE 8";
			line8.US_DRWExportDate = ZDateTime.Today.AddDays(-5);
			line8.US_DRWExportAction = "D";
			line8.US_DRWExportID = "3214568878";
			line8.US_ExportTariff = "60064563";
			line8.DRWExportQuantity = 3.6m;
			line8.DRWExportUQ = "NO";

			Factory.Save();

			var supporter = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);

			AssertEquals("8 lines", 8, supporter.DrawbackExportSectionLines.Count);
			AssertEquals(ZDateTime.Today.AddDays(-10), supporter.DrawbackExportSectionLines[0].US_DRWExportDate);
			AssertEquals("1001245632", supporter.DrawbackExportSectionLines[0].US_DRWExportID);

			AssertEquals(ZDateTime.Today.AddDays(-10), supporter.DrawbackExportSectionLines[1].US_DRWExportDate);
			AssertEquals("1001245632", supporter.DrawbackExportSectionLines[1].US_DRWExportID);

			AssertEquals(ZDateTime.Today.AddDays(-10), supporter.DrawbackExportSectionLines[2].US_DRWExportDate);
			AssertEquals("EXPINV1", supporter.DrawbackExportSectionLines[2].US_DRWExportID);

			AssertEquals(ZDateTime.Today.AddDays(-10), supporter.DrawbackExportSectionLines[3].US_DRWExportDate);
			AssertEquals("EXPINV1", supporter.DrawbackExportSectionLines[3].US_DRWExportID);

			AssertEquals(ZDateTime.Today.AddDays(-10), supporter.DrawbackExportSectionLines[4].US_DRWExportDate);
			AssertEquals("EXPINV1", supporter.DrawbackExportSectionLines[4].US_DRWExportID);

			AssertEquals(ZDateTime.Today.AddDays(-5), supporter.DrawbackExportSectionLines[5].US_DRWExportDate);
			AssertEquals("3214568878", supporter.DrawbackExportSectionLines[5].US_DRWExportID);

			AssertEquals(ZDateTime.Today.AddDays(-5), supporter.DrawbackExportSectionLines[6].US_DRWExportDate);
			AssertEquals("A00100232", supporter.DrawbackExportSectionLines[6].US_DRWExportID);

			AssertEquals(ZDateTime.Today.AddDays(-5), supporter.DrawbackExportSectionLines[7].US_DRWExportDate);
			AssertEquals("A00100232", supporter.DrawbackExportSectionLines[7].US_DRWExportID);
		}

		protected override void SetUp()
		{
			Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			base.SetUp();
		}

		JobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration fDeclaration;

		protected override Drawback7551DocLinePageCollection GetCollectionToTest()
		{
			return new Drawback7551DocLinePageCollection(Declaration.InvoiceLines, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoiceLine = Declaration.FilteredInvoiceLines.AddNew();
			return new Drawback7551DocLine(invoiceLine);
		}
	}
}
