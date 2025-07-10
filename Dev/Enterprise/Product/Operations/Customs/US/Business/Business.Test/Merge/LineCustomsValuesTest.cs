using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class LineCustomsValuesTest : TestCaseWithFactory
	{
		public void TestRound()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500.32m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200.18m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("US_CustomsValue rounded", 501m, invoiceLine.US_CustomsValue);
			AssertEquals("US_CustomsValue rounded", 200m, invoiceLine2.US_CustomsValue);
			AssertEquals("CustomsValue should match", 701m, invoiceLine.CusEntryLine.CL_CustomsValue);

			new LineCustomsValues().CalculateOnMergedLines(declaration);
			AssertEquals("US_CustomsValue rounded", 501m, invoiceLine.US_CustomsValue);
			AssertEquals("US_CustomsValue rounded", 200m, invoiceLine2.US_CustomsValue);
			AssertEquals("CustomsValue should match", 701m, invoiceLine.CusEntryLine.CL_CustomsValue);
		}

		public void TestRoundForCargoReleaseEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500.32m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200.18m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("US_CustomsValue rounded", 501m, invoiceLine.US_CustomsValue);
			AssertEquals("US_CustomsValue rounded", 200m, invoiceLine2.US_CustomsValue);
			AssertEquals("CustomsValue should match", 701m, invoiceLine.CusEntryLine.CL_CustomsValue);

			new LineCustomsValues().CalculateOnMergedLines(declaration);
			AssertEquals("US_CustomsValue rounded", 501m, invoiceLine.US_CustomsValue);
			AssertEquals("US_CustomsValue rounded", 200m, invoiceLine2.US_CustomsValue);
			AssertEquals("CustomsValue should match", 701m, invoiceLine.CusEntryLine.CL_CustomsValue);
		}

		public void TestWhenValueIsDeclaredAgainstSupTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99041711";
			invoiceLine.JI_LinePrice = 500m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(500m, invoiceLine.US_CustomsValue);
		}

		public void TestInvoiceLineCVForDerivedComputation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 24000.5m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "8211.10.00 00";
			AssertEquals("PreCondition:Derived Computation", ComputationCodeList.Codes.Derived, line1.ImportTariff.UE_DutyComputationCode);

			line1.JI_LinePrice = 0m;

			var line2 = line1.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8211.92.90 45";
			line2.JI_LinePrice = 16000.38m;
			line2.JI_CustomsQuantity = 16000m;

			var line3 = line1.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211.93.00 30";
			line3.JI_LinePrice = 8000.12m;
			line3.JI_CustomsQuantity = 4000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("US_CustomsValue", 0m, line1.US_CustomsValue);
			AssertEquals("US_CustomsValue", 16001m, line2.US_CustomsValue);
			AssertEquals("US_CustomsValue", 8000m, line3.US_CustomsValue);
			AssertEquals("CustomsValue should match", declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue, line2.US_CustomsValue + line3.US_CustomsValue);

			//Run by service task online data transformation
			new LineCustomsValues().CalculateOnMergedLines(declaration);
			AssertEquals("US_CustomsValue", 0m, line1.US_CustomsValue);
			AssertEquals("US_CustomsValue", 16001m, line2.US_CustomsValue);
			AssertEquals("US_CustomsValue", 8000m, line3.US_CustomsValue);
			AssertEquals("CustomsValue should match", declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEnteredValue, line2.US_CustomsValue + line3.US_CustomsValue);
		}

		public void TestInvoiceLineCVForSupAdditionalTariffOnXVVLines()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038815", "7", 0.075m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "4818900080", "7", 0m, "KG");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "2853909090", "7", 0.028m, "KG");

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var xParentLine = invoice.JobComInvoiceLines.AddNew();
			xParentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			xParentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			xParentLine.JI_FormattedTariff = "4818.90.0080";
			xParentLine.SupTariffFormatted = "9903.88.15";
			xParentLine.SupFormattedAdditionalTariff1 = "9903.01.24";
			xParentLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;

			var vParentLine = xParentLine.AddSecondaryInvoiceLine();
			vParentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			vParentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			vParentLine.JI_FormattedTariff = "4818.90.0080";
			vParentLine.SupTariffFormatted = "9903.88.15";
			vParentLine.SupFormattedAdditionalTariff1 = "9903.01.24";
			vParentLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			vParentLine.JI_LinePrice = 2000m;

			var vChildLine = xParentLine.AddSecondaryInvoiceLine();
			vChildLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			vChildLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			vChildLine.JI_FormattedTariff = "2853.90.9090";
			vChildLine.SupTariffFormatted = "9903.01.24";
			vChildLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			vChildLine.JI_LinePrice = 3000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_CustomsValue for X line", 5000m, xParentLine.US_CustomsValue);
			AssertEquals("US_CustomsValue for V parent line", 2000m, vParentLine.US_CustomsValue);
			AssertEquals("US_CustomsValue for V child line", 3000m, vChildLine.US_CustomsValue);
		}
	}
}
