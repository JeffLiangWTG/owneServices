using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Rating.Business.Testing
{
	public class DateValidatorTest : TestCaseWithFactory
	{
		public void TestDateValidator()
		{
			var testQuote = Factory.New<Quote>();
			testQuote.TH_QuoteDate = ZDate.Empty;
			AssertEquals("Has Errors", true, testQuote.TH_QuoteDateInfo.HasErrors());

			testQuote.TH_QuoteEndDate = ZDate.Today.AddDays(10);
			testQuote.TH_QuoteDate = ZDate.Today.AddDays(11);
			AssertEquals("Has Errors", true, testQuote.TH_QuoteDateInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.StartDateAfterExpiryDate, testQuote.TH_QuoteDateInfo.GetErrors().GetFirstMessage());

			testQuote.TH_QuoteDate = ZDate.Today.AddDays(10);
			AssertEquals("Has Errors", false, testQuote.TH_QuoteDateInfo.HasErrors());

			testQuote.TH_QuoteDate = ZDate.Today.AddDays(5);
			AssertEquals("Has Errors", false, testQuote.TH_QuoteDateInfo.HasErrors());

			testQuote.TH_QuoteEndDate = ZDate.Today.AddDays(4);
			AssertEquals("Has Errors", true, testQuote.TH_QuoteEndDateInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.ExpiryBeforeStartDate, testQuote.TH_QuoteEndDateInfo.GetErrors().GetFirstMessage());

			testQuote.TH_QuoteEndDate = ZDate.Today.AddDays(5);
			AssertEquals("Has Errors", false, testQuote.TH_QuoteEndDateInfo.HasErrors());

			Env.Registry.Rating.QuoteEndDateMandatory = true;
			testQuote.TH_QuoteEndDate = ZDate.Empty;
			AssertEquals("Has Errors", true, testQuote.TH_QuoteEndDateInfo.HasErrors());

			testQuote.TH_QuoteEndDate = ZDate.Today.AddDays(15);
			AssertEquals("Has Errors", false, testQuote.TH_QuoteEndDateInfo.HasErrors());

			Env.Registry.Rating.QuoteEndDateMandatory = false;
			testQuote.TH_QuoteEndDate = ZDate.Empty;
			AssertEquals("Has Errors", false, testQuote.TH_QuoteEndDateInfo.HasErrors());

			testQuote.TH_QuoteDate = ZDate.Today.AddDays(-1);
			AssertEquals("Has Warnings", true, testQuote.TH_QuoteDateInfo.HasWarnings());
			AssertEquals("Warnings Message", ErrorMessages.StartDateInPast, testQuote.TH_QuoteDateInfo.GetWarnings().GetFirstMessage());
		}

		public void TestDateValidatorOnRateEntry()
		{
			var testRate = Factory.New<ClientRate>();
			var testEntry = testRate.AddRateEntry("DST");
			testEntry.TI_RateStartDate = ZDate.Empty;
			AssertEquals("Has Errors", true, testEntry.TI_RateStartDateInfo.HasErrors());

			testEntry.TI_RateEndDate = ZDate.Today.AddDays(10);
			testEntry.TI_RateStartDate = ZDate.Today.AddDays(11);
			AssertEquals("Has Errors", true, testEntry.TI_RateStartDateInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.StartDateAfterExpiryDate, testEntry.TI_RateStartDateInfo.GetErrors().GetFirstMessage());

			testEntry.TI_RateStartDate = ZDate.Today.AddDays(10);
			AssertEquals("Has Errors", false, testEntry.TI_RateStartDateInfo.HasErrors());

			testEntry.TI_RateStartDate = ZDate.Today.AddDays(5);
			AssertEquals("Has Errors", false, testEntry.TI_RateStartDateInfo.HasErrors());

			testEntry.TI_RateEndDate = ZDate.Today.AddDays(4);
			AssertEquals("Has Errors", true, testEntry.TI_RateEndDateInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.ExpiryBeforeStartDate, testEntry.TI_RateEndDateInfo.GetErrors().GetFirstMessage());

			testEntry.TI_RateEndDate = ZDate.Today.AddDays(5);
			AssertEquals("Has Errors", false, testEntry.TI_RateEndDateInfo.HasErrors());

			testEntry.TI_RateEndDate = ZDate.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RateEndDateInfo.HasErrors());

			testEntry.TI_RateEndDate = ZDate.Today.AddDays(15);
			AssertEquals("Has Errors", false, testEntry.TI_RateEndDateInfo.HasErrors());

			testEntry.TI_RateStartDate = ZDate.Today.AddDays(-1);
			AssertEquals("Has Warnings", true, testEntry.TI_RateStartDateInfo.HasWarnings());
			AssertEquals("Warning Message", ErrorMessages.StartDateInPast, testEntry.TI_RateStartDateInfo.GetWarnings().GetFirstMessage());

			testEntry.TI_RateStartDate = ZDate.Today.AddDays(-29);
			AssertEquals("Has Warnings", true, testEntry.TI_RateStartDateInfo.HasWarnings());
			AssertEquals("Warning Message", ErrorMessages.StartDateInPast, testEntry.TI_RateStartDateInfo.GetWarnings().GetFirstMessage());

			testEntry.TI_RateStartDate = ZDate.Today.AddDays(-30);
			AssertEquals("Has Warnings", true, testEntry.TI_RateStartDateInfo.HasWarnings());
			AssertEquals("Warning Message", ErrorMessages.StartDateInPast, testEntry.TI_RateStartDateInfo.GetWarnings().GetFirstMessage());
		}
	}
}
