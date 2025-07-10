using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DerivedDutyCalculatorTest : TestCaseWithFactory
	{
		public void TestDerivedComputation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 24000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			invoice.US_UC_NKCountryOfExport = "MX";
			invoice.US_UC_NKCountryOfOrigin = "MX";

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "8211.10.00 00";
			AssertEquals("PreCondition:Derived Computation", ComputationCodeList.Codes.Derived, line1.ImportTariff.UE_DutyComputationCode);

			line1.JI_LinePrice = 0m;
			line1.JI_Weight = 1500m;
			line1.JI_WeightUQ = "KG";

			JobComInvoiceLine line2 = line1.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8211.92.90 45";
			line2.JI_LinePrice = 16000.00m;
			line2.JI_CustomsQuantity = 16000m;

			JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211.93.00 30";
			line3.JI_LinePrice = 8000.00m;
			line3.JI_CustomsQuantity = 4000m;

			line1.US_SPI = "";
			line2.US_SPI = "";
			line3.US_SPI = "";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = line1.CusEntryLine;
			CusEntryLine entryLine2 = line2.CusEntryLine;
			CusEntryLine entryLine3 = line3.CusEntryLine;

			AssertEquals("Total Duty Expected", 1896.00m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
			AssertEquals("There should be two entry lines for the entry", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals("entry line2 and 3 are the same", entryLine2, entryLine3);

			line1.US_SPI = SpecialProgramList.Codes.MX;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Total Duty Expected With Duty Free SPI", 0m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
			AssertEquals("There should be two entry lines for the entry", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals("CustomsValue for the parent line", 24000m, entryLine1.CL_CustomsValue);
			AssertEquals("CustomsValue for the secondary line", 0m, entryLine2.CL_CustomsValue);
		}

		public void TestDerivedCalculationWhenTariffChanges()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 24000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_LinePrice = 0m;
			AssertEquals("PreCondition:Derived Computation", ComputationCodeList.Codes.Derived, invoiceLine.ImportTariff.UE_DutyComputationCode);

			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8211929045"; // .4c per unit + 6.1% = 20000 *.004 + 24000 * .061 = $1544
			line2.JI_CustomsQuantity = 16000m;
			line2.JI_LinePrice = 16000m;

			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (20000 *.03) + (24000 * .054) = $1896
			line3.JI_CustomsQuantity = 4000m;
			line3.JI_LinePrice = 8000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;
			CusEntryLine entryLine2 = line2.CusEntryLine;
			CusEntryLine entryLine3 = line3.CusEntryLine;

			AssertEquals("RandomLine for entryLine3", line3, entryLine3.RandomLine);

			line3.JI_Tariff = "8211930060";//same rate as the previous one
			line3.JI_CustomsQuantity = 4000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			//When loaded freshly, it worked out RandomLine OK.
			JobDeclaration decLoaded = new CargoWise.EntityFramework.BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			JobComInvoiceLine line3Loaded = (JobComInvoiceLine)decLoaded.InvoiceLines.FindByPK(line3.PK);
			AssertEquals("RandomLine for entryLine2", line3.PK, line3Loaded.CusEntryLine.RandomLine.PK);

			//It should work out random line when merge has just happened.
			AssertEquals("RandomLine for entryLine2", line3, line3.CusEntryLine.RandomLine);
		}
	}
}
