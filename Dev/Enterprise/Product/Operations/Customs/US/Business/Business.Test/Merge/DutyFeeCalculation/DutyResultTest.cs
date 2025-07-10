using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DutyResultTest : TestCaseWithFactory
	{
		public void TestClearIfDutyIsNotToBeCalculated()
		{
			DutyResult result = new DutyResult();
			result.PercentOfValue = 999999.00m;

			result.TotalAmount = new Money(10000000m, JobDeclaration.GetLocalCurrency());
			result.PerUnitAmount = 100m;
			result.PerUnitUQ = "KG";

			result.ClearIfDutyIsNotToBeCalculated();
			AssertEquals(0m, result.PercentOfValue);
			AssertEquals(Money.Empty, result.TotalAmount);
			AssertEquals(0m, result.PerUnitAmount);
			AssertEquals("", result.PerUnitUQ);
			AssertEquals(ZString.Empty, result.RateString);
		}

		[NUnit.Framework.TestDate(2009, 1, 1)]
		public void TestClearIfDutyIsNotToBeCalculatedWhenPerUnitAmountIsInvalidDutyRate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "CJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9910.04.66";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";

			invoiceLine.JI_Tariff = "1806.32.7000";
			invoiceLine.JI_CustomsQuantity = 150m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty should not have calculated with an invalid duty rate", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);

			invoiceLine.US_SPI = "SG";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty should not have calculated with an invalid duty rate", 262.20m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
		}
	}
}
