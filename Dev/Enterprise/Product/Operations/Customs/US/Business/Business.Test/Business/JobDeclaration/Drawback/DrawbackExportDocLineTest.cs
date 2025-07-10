using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class DrawbackExportDocLineTest : NonPersistentBusinessObjectTestCase
	{
		protected abstract DrawbackDocLine GetNewDrawbackExportDocLine(JobComInvoiceLine invoiceLine);

		public void TestDrawbackDocLine()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var importDec = Factory.New<JobDeclaration>();
			importDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDec.US_EnableENS = true;
			importDec.US_EntryFilerCode = "XJ5";

			var importInvoice = importDec.Invoices.AddNew();

			var importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine.JI_CustomsUnitQty = "NO";
			importInvoiceLine.JI_CustomsQuantity = 2000m;
			importInvoiceLine.US_PayableMPF = 100m;
			importDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			importDec.ImportEntryNumber = "12345678";
			importInvoice.JZ_InvoiceNumber = "INV-1";
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.US_ImportEntryNo = "XJ512345678";
			invoiceLine.US_DRWImportEntryLine = 1;
			invoiceLine.US_DRWExportDate = new ZDateTime(2012, 05, 10);
			invoiceLine.US_DRWExportID = "ExportID";
			invoiceLine.US_DRWExportDest = "AU";

			var exportDocLine = GetNewDrawbackExportDocLine(invoiceLine);
			AssertEquals("Invoice No.: INV-1", exportDocLine.DrawbackDocDescription);
			AssertEquals("Invoice No.: INV-1", exportDocLine.DrawbackExpDocDescription);

			invoiceLine.JI_Description = "Description";
			invoiceLine.US_ExportTariff = "123456";
			invoiceLine.US_DRWExportAction = "D";
			invoiceLine.US_DRWExportQuantity = 10m;
			invoiceLine.US_DRWExportUQ = "KG";
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "Export Carrier";
			exporter.OH_Code = "EXP1312";
			invoiceLine.ExporterOrDestroyer.OrganisationPK = exporter.PK;

			exportDocLine = GetNewDrawbackExportDocLine(invoiceLine);

			AssertEquals(1, exportDocLine.ConstantOne);
			AssertEquals("XJ5-1234567-8 / 1", exportDocLine.FormattedImportEntryNo);
			AssertEquals(new ZDateTime(2012, 05, 10), exportDocLine.US_DRWExportDate);
			AssertEquals("AU", exportDocLine.US_DRWExportDest);
			AssertEquals("Description, Invoice No.: INV-1", exportDocLine.DrawbackDocDescription);
			AssertEquals("Description, Invoice No.: INV-1", exportDocLine.DrawbackExpDocDescription);
			AssertEquals("10 KG", exportDocLine.ExportQtyAndUnits);
			AssertEquals("123456", exportDocLine.US_ExportTariff);
			AssertEquals("D", exportDocLine.US_DRWExportAction);
			AssertEquals("Export Carrier", exportDocLine.US_DRWExporterName);

			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.DRWExportQuantity = 200m;
			invoiceLine.DRWExportUQ = "NO";
			exportDocLine = GetNewDrawbackExportDocLine(invoiceLine);
			AssertEquals("200 NO", exportDocLine.ExportQtyAndUnits);

			importInvoiceLine.JI_PartNo = "Test Product";
			Factory.Save();
			drawback = new BusinessObjectFactory().New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			invoice = drawback.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_ImportEntryNo = "XJ512345678";
			invoiceLine.US_DRWImportEntryLine = 1;
			invoiceLine.JI_Description = "Description";
			exportDocLine = GetNewDrawbackExportDocLine(invoiceLine);
			AssertEquals("Description, Invoice No.: INV-1, Part No.: Test Product", exportDocLine.DrawbackDocDescription);
			AssertEquals("Description, Invoice No.: INV-1, Part No.: Test Product", exportDocLine.DrawbackExpDocDescription);

			importInvoice.JZ_InvoiceNumber = ZString.Empty;
			Factory.Save();
			drawback = new BusinessObjectFactory().New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			invoice = drawback.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_ImportEntryNo = "XJ512345678";
			invoiceLine.US_DRWImportEntryLine = 1;
			invoiceLine.JI_Description = "Description";
			exportDocLine = GetNewDrawbackExportDocLine(invoiceLine);
			AssertEquals("Description, Part No.: Test Product", exportDocLine.DrawbackDocDescription);
			AssertEquals("Description, Part No.: Test Product", exportDocLine.DrawbackExpDocDescription);

			invoiceLine.JI_Description = ZString.Empty;
			exportDocLine = GetNewDrawbackExportDocLine(invoiceLine);
			AssertEquals("Part No.: Test Product", exportDocLine.DrawbackDocDescription);
			AssertEquals("Part No.: Test Product", exportDocLine.DrawbackExpDocDescription);
		}
	}
}
