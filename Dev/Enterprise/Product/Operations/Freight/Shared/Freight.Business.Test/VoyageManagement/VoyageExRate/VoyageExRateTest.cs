using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.Testing.OrgCompanyDataTest;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageExRateTest : BaseFreightTest
	{
		public void TestIExchangeRate()
		{
			VoyageExRate rate = Factory.New<VoyageExRate>();
			rate.E8_RX_NKExCurrency = "AUD";
			rate.E8_VoyageExchangeRate = 11.1m;

			IExchangeRate iRate = rate;
			AssertEquals("CurrencyCode should be AUD", "AUD", iRate.CurrencyCode);
			AssertEquals("ExchangeRate should be 11.1", 11.1m, iRate.Rate);

			rate.E8_RX_NKExCurrency = ZString.Empty;
			// should never happen in a real situation but best not to throw exceptions.
			AssertEquals("CurrencyCode should be empty", "", iRate.CurrencyCode);
		}

		#region UniqueIndexFailureHandler

		public void TestUniqueIndexFailureHandler()
		{
			var notification = new NotificationHandlerForTest();

			var voyage1 = Factory.NewWithValidTestData<JobVoyage>();
			voyage1.JV_VoyageFlight = "TEST300";

			// Generic voyage exchange rate
			var rate1 = voyage1.ExRates.AddNew();
			rate1.E8_RX_NKExCurrency = "USD";
			rate1.E8_VoyageExchangeRate = 0.73m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var voyage2 = newFactory.Load<JobVoyage>(voyage1.PK);
			AssertNotNull(voyage2);
			var rate2 = voyage2.ExRates.AddNew();
			rate2.E8_RX_NKExCurrency = "USD";
			rate2.E8_VoyageExchangeRate = 0.74m;

			var failureHandler = ((IBusinessObjectInternals)rate2).UniqueIndexFailureHandlers.Single();
			AssertNotNull(failureHandler);

			AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), newFactory.Save);

			notification.Reset();
			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single());
			AssertContains(string.Format(CultureInfo.InvariantCulture, @"While you were working, another user has created a duplicate Voyage Exchange Rate.
Voyage: TEST300, currency: USD, Port: , Company: {0}.
Please cancel your changes and reload the form.", GlbCompany.CurrentCompany.CompanyName), notification.Message);
			AssertContains("-Duplicate Voyage Exchange Rate", notification.Message);
			Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);

			// Voyage exchange rate for specific port
			rate1.E8_RL_NKPort = "HKHKG";
			Factory.Save();

			rate2.E8_RL_NKPort = "HKHKG";
			AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), newFactory.Save);

			notification.Reset();
			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single());
			AssertContains(string.Format(CultureInfo.InvariantCulture, @"While you were working, another user has created a duplicate Voyage Exchange Rate.
Voyage: TEST300, currency: USD, Port: HKHKG, Company: {0}.
Please cancel your changes and reload the form.", GlbCompany.CurrentCompany.CompanyName), notification.Message);
			AssertContains("-Duplicate Voyage Exchange Rate", notification.Message);
			Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);
		}

		#endregion
	}
}
