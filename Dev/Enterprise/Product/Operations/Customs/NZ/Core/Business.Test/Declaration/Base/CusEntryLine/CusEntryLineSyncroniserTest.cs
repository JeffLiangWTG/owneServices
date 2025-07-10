using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineSyncroniser))]
	public class CusEntryLineSyncroniserTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSyncroniserEndToEndTest()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "A";
			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "B";

			JobComInvoiceLine header1line1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			header1line1.JI_Tariff = "1234567890";
			header1line1.JI_Description = "DESCRIPTION";
			header1line1.JI_CustomsUnitQty = "PK";
			header1line1.JI_CustomsQuantity = 3.2m;

			JobComInvoiceLine header1line2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			header1line2.JI_Tariff = "1234567890";
			header1line2.JI_Description = "DESCRIPTION";
			header1line2.JI_CustomsUnitQty = "PK";
			header1line2.JI_CustomsQuantity = 2.1m;

			JobComInvoiceLine header1line3 = invoiceHeader1.JobComInvoiceLines.AddNew();
			header1line3.JI_Tariff = "1234567890";
			header1line3.JI_Description = "DESCRIPTION";
			header1line3.JI_CustomsUnitQty = "PK";
			header1line3.JI_CustomsQuantity = 5.5m;

			JobComInvoiceLine header2line1 = invoiceHeader2.JobComInvoiceLines.AddNew();
			header2line1.JI_Tariff = "18631863";
			header2line1.JI_Description = "NONE";
			header2line1.JI_CustomsUnitQty = "BX";
			header2line1.JI_CustomsQuantity = 7.3m;

			int invoiceLinesCount = declaration.InvoiceLines.Count; //4
			ZGuid declarationPK = declaration.PK;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			CusEntryLineSyncroniser syncroniser = new CusEntryLineSyncroniser(entryLine);
			syncroniser.InvoiceLine.PermitCodes.AddNew(PermitCodeList.Codes.CustomsDepartmentApproval, "ABC1232");
			syncroniser.SyncAllInvoiceLinesOnEntryLine();

			AssertEquals("No new invoice lines should have been created. (before factory save)", invoiceLinesCount, declaration.InvoiceLines.Count);
			Factory.Save();
			AssertEquals("No new invoice lines should have been created. (after factory save)", invoiceLinesCount, declaration.InvoiceLines.Count);

			JobDeclaration declarationFromDB = new BusinessObjectFactory().Load<JobDeclaration>(declarationPK);
			AssertEquals("No new invoice lines should have been created. (after load from db)", invoiceLinesCount, declarationFromDB.InvoiceLines.Count);
			foreach (JobComInvoiceLine invoiceLine in declarationFromDB.InvoiceLines)
			{
				if (invoiceLine.JI_Tariff == "1234.56.78.90")
				{
					AssertEquals("invoiceLine.PermitCodes.Count", 1, invoiceLine.PermitCodes.Count);
					AssertEquals("invoiceLine.PermitCodes[0].ZO_Code", PermitCodeList.Codes.CustomsDepartmentApproval, invoiceLine.PermitCodes[0].ZO_Code);
					AssertEquals("invoiceLine.PermitCodes[0].ZO_Data", "ABC1232", invoiceLine.PermitCodes[0].ZO_Data);
				}
				else if (invoiceLine.JI_Tariff == "1863.18.63")
				{
					AssertEquals("invoiceLine.PermitCodes.Count", 0, invoiceLine.PermitCodes.Count);
				}
				else
				{
					Fail("Unexpected Invoice Tariff Value: " + invoiceLine.JI_Tariff);
				}
			}
		}

		public void TestSyncAllInvoiceLinesOnEntryLine()
		{
			CusEntryLineBuilder builder = new CusEntryLineBuilder();
			CusEntryLine entryLine = builder.GetEntryLineWithTwoInvoiceLines();

			AssertEquals("The number of records in the entryLine was : " + entryLine.InvoiceLines.Count, (entryLine.InvoiceLines.Count > 1), actual: true);

			JobComInvoiceLine line1 = (JobComInvoiceLine)entryLine.InvoiceLines[0];
			JobComInvoiceLine line2 = (JobComInvoiceLine)entryLine.InvoiceLines[1];

			line1.PermitCodes.AddNew(PermitCodeList.Codes.CustomsDepartmentApproval, "Test Data");
			line1.PermitCodes.AddNew(PermitCodeList.Codes.DeptOfConservationCites, "Test Data2");

			line1.ProhibitedCodes.AddNew(ProhibitedCodeList.Codes.Antiques, "Antiques");
			line1.ProhibitedCodes.AddNew(ProhibitedCodeList.Codes.ApprovedPesticidesAndChemical, "Approved");

			line1.OtherInfos.AddNew("lkj", "laskjdf;");

			line1.JI_ConcessionCode = ";lakjds";

			CusEntryLineSyncroniser modifier = new CusEntryLineSyncroniser(entryLine);
			line2.PermitCodes.RemoveAll();
			line2.ProhibitedCodes.RemoveAll();
			line2.OtherInfos.RemoveAll();
			line2.JI_ConcessionCode = "";

			modifier.SyncAllInvoiceLinesOnEntryLine();

			AssertEquals(line1.PermitCodes[0].ZO_Code, line2.PermitCodes[0].ZO_Code);
			AssertEquals(line1.PermitCodes[1].ZO_Code, line2.PermitCodes[1].ZO_Code);

			AssertEquals(line1.ProhibitedCodes[0].ZO_Code, line2.ProhibitedCodes[0].ZO_Code);
			AssertEquals(line1.ProhibitedCodes[1].ZO_Code, line2.ProhibitedCodes[1].ZO_Code);

			AssertEquals(line1.OtherInfos.Count, line2.OtherInfos.Count);

			AssertEquals(line1.JI_ConcessionCode, line2.JI_ConcessionCode);
		}

		public void TestSyncroniserGetsCodesFromInvoiceLineProperly()
		{
			CusEntryLineBuilder builder = new CusEntryLineBuilder();
			CusEntryLine entryLine = builder.GetEntryLineWithTwoInvoiceLines();

			CusEntryLineSyncroniser testSyncroniser = new CusEntryLineSyncroniser(entryLine);

			testSyncroniser.InvoiceLine.PermitCodes.AddNew(PermitCodeList.Codes.CustomsDepartmentApproval, "TESTDAT");
			testSyncroniser.InvoiceLine.PermitCodes.AddNew(PermitCodeList.Codes.DeptOfConservationCites, "TESTDAT");
			testSyncroniser.InvoiceLine.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalRiskManagement, "TESTDAT");

			testSyncroniser.SyncAllInvoiceLinesOnEntryLine();

			AssertEquals("First Invoice Line has the right number of Permit Codes", ((JobComInvoiceLine)entryLine.InvoiceLines[0]).PermitCodes.Count, 3);
			AssertEquals("Second Invoice Line has the right number of Permit Codes", ((JobComInvoiceLine)entryLine.InvoiceLines[1]).PermitCodes.Count, 3);

			CusEntryLineSyncroniser testSyncroniser2 = new CusEntryLineSyncroniser(entryLine);
			AssertEquals("New Synchroniser picks up existing Codes", testSyncroniser2.InvoiceLine.PermitCodes.Count, 3);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			CusEntryLineBuilder builder = new CusEntryLineBuilder();
			CusEntryLine entryLine = builder.GetEntryLineWithTwoInvoiceLines();

			return new CusEntryLineSyncroniser(entryLine);
		}
		#endregion
	}

	public class CusEntryLineBuilder
	{
		readonly BusinessObjectFactory factory = new BusinessObjectFactory();

		public CusEntryLine GetEntryLineWithTwoInvoiceLines()
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "A";
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_CustomsUnitQty = "PK";
			invoiceLine1.JI_CustomsQuantity = 3.2m;

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1234567890";
			invoiceLine2.JI_Description = "DESCRIPTION";
			invoiceLine2.JI_CustomsUnitQty = "PK";
			invoiceLine2.JI_CustomsQuantity = 2.1m;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.CL_AdValoremTariff = invoiceLine1.JI_Tariff.Left(15);

			return entryLine;
		}
	}
}
