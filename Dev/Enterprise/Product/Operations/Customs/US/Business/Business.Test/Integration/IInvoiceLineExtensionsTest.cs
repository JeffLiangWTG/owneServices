using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IInvoiceLineExtensionsTest : TestCaseWithFactory
	{
		public void TestMatchesTariff()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_FormattedTariff = "1111.11.1111";
			invoiceLine.SupTariffFormatted = "2222.22.2222";
			invoiceLine.SupFormattedAdditionalTariff1 = "3333.33.3333";
			invoiceLine.SupFormattedAdditionalTariff2 = "4444.44.4444";
			invoiceLine.SupFormattedAdditionalTariff3 = "5555.55.5555";
			invoiceLine.SupFormattedAdditionalTariff4 = "6666.66.6666";
			invoiceLine.SupFormattedAdditionalTariff5 = "7777.77.7777";

			var iInvoiceLine = (IInvoiceLine)invoiceLine;
			Assert("Tariff not match the regular tariff", !iInvoiceLine.MatchesTariff(false, false, false, false, false, false, "0000000000"));
			Assert("Tariff matches the regular tariff", iInvoiceLine.MatchesTariff(false, false, false, false, false, false, "1111111111"));
			Assert("Tariff not match the sup tariff", !iInvoiceLine.MatchesTariff(true, false, false, false, false, false, "0000000000"));
			Assert("Tariff matches the sup tariff", iInvoiceLine.MatchesTariff(true, false, false, false, false, false, "2222222222"));
			Assert("Tariff not match the sup additional tariff 1", !iInvoiceLine.MatchesTariff(false, true, false, false, false, false, "0000000000"));
			Assert("Tariff matches the sup additional tariff 1", iInvoiceLine.MatchesTariff(false, true, false, false, false, false, "3333333333"));
			Assert("Tariff not match the sup additional tariff 2", !iInvoiceLine.MatchesTariff(false, false, true, false, false, false, "0000000000"));
			Assert("Tariff matches the sup additional tariff 2", iInvoiceLine.MatchesTariff(false, false, true, false, false, false, "4444444444"));
			Assert("Tariff not match the sup additional tariff 3", !iInvoiceLine.MatchesTariff(false, false, false, true, false, false, "0000000000"));
			Assert("Tariff matches the sup additional tariff 3", iInvoiceLine.MatchesTariff(false, false, false, true, false, false, "5555555555"));
			Assert("Tariff not match the sup additional tariff 4", !iInvoiceLine.MatchesTariff(false, false, false, false, true, false, "0000000000"));
			Assert("Tariff matches the sup additional tariff 4", iInvoiceLine.MatchesTariff(false, false, false, false, true, false, "6666666666"));
			Assert("Tariff not match the sup additional tariff 5", !iInvoiceLine.MatchesTariff(false, false, false, false, false, true, "0000000000"));
			Assert("Tariff matches the sup additional tariff 5", iInvoiceLine.MatchesTariff(false, false, false, false, false, true, "7777777777"));
		}
	}
}
