using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(CustomsExchangeRatesHeader))]
	sealed class CustomsExchangeRatesHeaderTestCase : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2007, 11, 6, 12, 15, 0)]
		public void TestCorrectExchangeRatesReported()
		{
			ZDateTime today = ZDateTime.Today;
			ZDateTime yesterday = today.AddDays(-1);
			ZDateTime tomorrow = today.AddDays(1);

			string cUS = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			string bUY = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			string sEL = Core.Constants.ExchangeRateTypes.Code.SellRate;

			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			RefCurrency gBP = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedKingdom);

			GlbCompany companyA = Factory.NewWithValidTestData<GlbCompany>();
			companyA.GC_RX_NKLocalCurrency = gBP.RX_Code;
			companyA.GC_Code = "AAA";

			GlbCompany companyB = Factory.NewWithValidTestData<GlbCompany>();
			companyB.GC_Code = "BBB";
			companyB.GC_RX_NKLocalCurrency = uSD.RX_Code;

			TestCaseHelper.ClearTable(RefExchangeRateSchema.Constants.TableName);

			CreateExchangeRate(companyA, uSD, bUY, yesterday, 0.79m);
			CreateExchangeRate(companyA, uSD, cUS, yesterday, 0.80m);
			CreateExchangeRate(companyA, uSD, sEL, yesterday, 0.81m);

			CreateExchangeRate(companyA, uSD, bUY, today, 0.89m);
			CreateExchangeRate(companyA, uSD, cUS, today, 0.90m);
			CreateExchangeRate(companyA, uSD, sEL, today, 0.91m);
			CreateExchangeRate(companyA, gBP, cUS, today, 0.92m);   //it shouldn't show Company's local currency

			CreateExchangeRate(companyA, uSD, bUY, tomorrow, 0.99m);
			CreateExchangeRate(companyA, uSD, cUS, tomorrow, 1.00m);
			CreateExchangeRate(companyA, uSD, sEL, tomorrow, 1.01m);

			Factory.Save();

			CustomsExchangeRatesHeader companyAHeader = new CustomsExchangeRatesHeader(Factory, companyA.GC_Code);
			AssertEquals("Should only have one currencies exchange rates", 1, companyAHeader.ExchangeRates.Count);
			AssertEquals("Exchange Rate's Currency", uSD.RX_Code, companyAHeader.ExchangeRates[0].RX_Code);
			AssertEquals("Should be todayst' Exchange Rate", 0.90m, companyAHeader.ExchangeRates[0].RE_SellRate);
			AssertEquals("Should expire today", today.Date, companyAHeader.ExchangeRates[0].RE_ExpiryDate.Date);

			CustomsExchangeRatesHeader companyBHeader = new CustomsExchangeRatesHeader(Factory, companyB.GC_Code);
			AssertEquals("Company B should not show any exchange rates", 0, companyBHeader.ExchangeRates.Count);
		}

		void CreateExchangeRate(GlbCompany company, RefCurrency currency, string rateType, ZDateTime date, decimal rate)
		{
			RefExchangeRate newRate = Factory.New<RefExchangeRate>();
			newRate.RE_GC = company.PK;
			newRate.RE_RX_NKExCurrency = currency.RX_Code;
			newRate.RE_ExRateType = rateType;
			newRate.RE_StartDate = date.Date;
			newRate.RE_ExpiryDate = newRate.RE_StartDate.AddDays(1).AddSeconds(-1);
			newRate.RE_SellRate = rate;
		}

		public void TestPropertiesAndRates()
		{
			AssertNotNull(testRatesHeader);

			AssertEquals("Company should be the CurrentCompany", GlbCompany.CurrentCompany.PK, testRatesHeader.Company.PK);
			AssertEquals("CountryName should match", GlbCompany.CurrentCompany.Country.Description, testRatesHeader.CountryName);
			AssertEquals("CurrencyCode should match", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testRatesHeader.CurrencyCode);
			AssertEquals("CurrencyDescription should match", GlbCompany.CurrentCompany.LocalCurrency.RX_Desc, testRatesHeader.CurrencyDescription);

			AssertNotNull("Rates should not be null", testRatesHeader.ExchangeRates);
			Assert("Should be some rates", testRatesHeader.ExchangeRates.Count > 0);
		}

		protected override BusinessObject GetNewBusinessObject() => testRatesHeader;

		protected override void SetUp()
		{
			base.SetUp();

			testRatesHeader = new CustomsExchangeRatesHeader(Factory, GlbCompany.CurrentCompany.GC_Code);
		}

		CustomsExchangeRatesHeader testRatesHeader;
	}
}
