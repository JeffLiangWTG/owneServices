using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DDPDisbursementSupDutyDataTest : DDPDisbursementDutyDataBaseTest
	{
		public void TestParentLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineOne = invoice.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "8206000000";
			invoiceLineOne.US_SupTariff = "99038803";

			var chargesAndFees = new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>();
			var customsValues = new Dictionary<JobComInvoiceLine, CustomsValues>();
			var lineDutyData = (IEntryLineOrInvoiceLineDutyData)new DDPDisbursementSupDutyData(invoiceLineOne, chargesAndFees, customsValues, true);
			AssertEquals("99038803", lineDutyData.Tariff);
			AssertEquals(1, lineDutyData.SupTariffs.Count);
			Assert(lineDutyData.SupTariffs.Contains("99038803"));

			var parentLineData = lineDutyData.ParentLine;
			AssertNull(parentLineData);

			var invoiceLineTwo = invoice.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_Tariff = "8205595510";
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			lineDutyData = new DDPDisbursementLineDutyData(invoiceLineTwo, chargesAndFees, customsValues, true);
			AssertEquals("8205595510", lineDutyData.Tariff);
			AssertEquals(0, lineDutyData.SupTariffs.Count);

			parentLineData = lineDutyData.ParentLine;
			AssertNotNull(parentLineData);
			AssertEquals("8206000000", parentLineData.Tariff);
			AssertEquals(1, parentLineData.SupTariffs.Count);
			Assert(parentLineData.SupTariffs.Contains("99038803"));
		}

		public void TestMPFCalculationOnInvoiceLineWithSupTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "DDP";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8457.20.0010";
			invoiceLine.US_SupTariff = "9802.00.8068";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 319m;
			invoiceLine.JI_LinePrice = 1356m;

			var dutyData = new DDPDisbursementSupDutyData(invoiceLine, null, null, true);
			AssertEquals(2, dutyData.FeeDataProviders.Count());

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var dddAmount = invoice.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, JobDeclaration.GetLocalCurrency());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("MPF should have been included in DDP calculation", dddAmount, entry.TotalAmountPayable);
		}

		public void TestGetSupCustomsValue()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8424201000";
			invoiceLine.US_SupTariff = "99030123";

			var supDutyData = new DDPDisbursementSupDutyData(invoiceLine, null, null, true);
			AssertEquals("Sup customs value for sup tariff is zero", 0m, ((IDutyData)supDutyData).SupCustomsValue);

			invoiceLine.US_SupGoodsValue = 100m;
			supDutyData = new DDPDisbursementSupDutyData(invoiceLine, null, null, true);
			AssertEquals("Sup customs value for sup tariff is 100", 100m, ((IDutyData)supDutyData).SupCustomsValue);
		}

		internal override DDPDisbursementDutyDataBase GetDDPDisbursementDutyDataForTest(JobComInvoiceLine invoiceLine) => new DDPDisbursementSupDutyData(invoiceLine, null, null, true);
	}
}
