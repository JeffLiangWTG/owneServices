using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageExRateDependentCollectionTest : BaseFreightTest
	{
		public void TestGetRateForCurrency()
		{
			RefCurrency gbpCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "GBP"));
			RefCurrency eurCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			RefCurrency usdCurrency = Factory.Load<RefCurrency>(ZArchitecture.Core.Utilities.CurrencyUSD);

			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageExRate usdRate = voyage.ExRates.AddNew();
			usdRate.E8_RX_NKExCurrency = "USD";
			usdRate.E8_VoyageExchangeRate = 0.7863m;

			VoyageExRate eurRate = voyage.ExRates.AddNew();
			eurRate.E8_RX_NKExCurrency = "EUR";
			eurRate.E8_VoyageExchangeRate = 0.5437m;

			AssertEquals(eurRate, voyage.ExRates.GetRateForCurrency("EUR"));
			AssertEquals(usdRate, voyage.ExRates.GetRateForCurrency("USD"));
			AssertNull(voyage.ExRates.GetRateForCurrency("GBP"));
			AssertNull(voyage.ExRates.GetRateForCurrency("XXX"));
		}

		public void TestGetRateForCurrencyWithSpecificPort()
		{
			var gbpCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "GBP"));
			AssertNotNull(gbpCurrency);
			var eurCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			AssertNotNull(eurCurrency);
			var usdCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			AssertNotNull(usdCurrency);
			var hkdCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "HKD"));
			AssertNotNull(hkdCurrency);

			var voyage = Factory.New<JobVoyage>();

			var usdRate1 = voyage.ExRates.AddNew();
			usdRate1.E8_RX_NKExCurrency = "USD";
			usdRate1.E8_VoyageExchangeRate = 0.7863m;

			var usdRate2 = voyage.ExRates.AddNew();
			usdRate2.E8_RX_NKExCurrency = "USD";
			usdRate2.E8_VoyageExchangeRate = 0.79m;
			usdRate2.E8_RL_NKPort = "USLAX";

			var eurRate1 = voyage.ExRates.AddNew();
			eurRate1.E8_RX_NKExCurrency = "EUR";
			eurRate1.E8_VoyageExchangeRate = 0.5437m;

			var eurRate2 = voyage.ExRates.AddNew();
			eurRate2.E8_RX_NKExCurrency = "EUR";
			eurRate2.E8_VoyageExchangeRate = 0.55m;
			eurRate2.E8_RL_NKPort = "ITNAP";

			var hkdRate1 = voyage.ExRates.AddNew();
			hkdRate1.E8_RX_NKExCurrency = "HKD";
			hkdRate1.E8_VoyageExchangeRate = 0.6m;
			hkdRate1.E8_RL_NKPort = "ITMIL";

			AssertEquals(eurRate2, voyage.ExRates.GetRateForCurrency("EUR", "ITNAP"));
			AssertEquals(eurRate1, voyage.ExRates.GetRateForCurrency("EUR", "AUSYD"));
			AssertEquals(eurRate1, voyage.ExRates.GetRateForCurrency("EUR"));

			AssertEquals(usdRate2, voyage.ExRates.GetRateForCurrency("USD", "USLAX"));
			AssertEquals(usdRate1, voyage.ExRates.GetRateForCurrency("USD", "AUSYD"));
			AssertEquals(usdRate1, voyage.ExRates.GetRateForCurrency("USD"));

			AssertNull(voyage.ExRates.GetRateForCurrency("HKD"));
			AssertEquals(hkdRate1, voyage.ExRates.GetRateForCurrency("HKD", "ITMIL"));
			AssertNull(voyage.ExRates.GetRateForCurrency("HKD", "AUSYD"));

			AssertNull(voyage.ExRates.GetRateForCurrency("GBP"));
			AssertNull(voyage.ExRates.GetRateForCurrency("XXX"));
		}

		#region TestSetRelationships

		public void TestSetRelationships()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageExRate rate = Factory.New<VoyageExRate>();

			AssertEquals("precondition:", ZGuid.Empty, rate.E8_GC);

			voyage.ExRates.Add(rate);
			AssertEquals("Add", GlbCompany.CurrentCompany.PK, rate.E8_GC);

			VoyageExRate rate2 = voyage.ExRates.AddNew();
			AssertEquals("AddNew", GlbCompany.CurrentCompany.PK, rate2.E8_GC);
		}

		#endregion

		#region TestRelationshipFilter

		public void TestRelationshipFilter()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageExRate rate1 = voyage.ExRates.AddNew();
			VoyageExRate rate2 = voyage.ExRates.AddNew();
			rate2.E8_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

			voyage.ExRates.Load();
			AssertEquals("should only have one exchange rate on the voyage for this company.", 1, voyage.ExRates.Count);
			AssertEquals("should be the correct exchange rate.", rate1, voyage.ExRates[0]);
		}

		#endregion
	}
}
