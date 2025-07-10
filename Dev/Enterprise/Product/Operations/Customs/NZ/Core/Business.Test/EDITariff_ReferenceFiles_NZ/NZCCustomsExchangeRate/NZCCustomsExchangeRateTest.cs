using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCCustomsExchangeRate))]
	public class NZCCustomsExchangeRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetMostRecentExchangeRateEndDate()
		{
			TestCaseHelper.ClearTable(NZCCustomsExchangeRate.Schema.TableName);

			AssertEquals("NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory)", ZDateTime.Empty, NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory));

			CreateRow("NZD", new ZDateTime(2006, 1, 15), 0.89m);
			CreateRow("USD", new ZDateTime(2010, 01, 11), 1.12m);
			CreateRow("AUD", new ZDateTime(2008, 4, 22), 42.33m);

			AssertEquals("NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory)", new ZDate(2010, 01, 11), NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory));

			CreateRow("ZAR", new ZDateTime(2010, 01, 25), 12.4m);

			AssertEquals("NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory)", new ZDate(2010, 01, 25), NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory));

			CreateRow("SGD", new ZDateTime(2015, 11, 01), 12.4m);

			AssertEquals("NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory)", new ZDate(2015, 11, 01), NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory));
		}

		void CreateRow(string currencyCode, ZDateTime dateTo, decimal rate)
		{
			NZCCustomsExchangeRate exchangeRate2 = Factory.New<NZCCustomsExchangeRate>();
			exchangeRate2.U7_CurrencyCode = currencyCode;
			exchangeRate2.U7_DateActiveFrom = dateTo.AddDays(-14);
			exchangeRate2.U7_DateActiveTo = dateTo;
			exchangeRate2.U7_Rate = rate;
		}
	}
}
