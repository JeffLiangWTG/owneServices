using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroupCountry))]
	public class CusRefTradeGroupCountryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDescription()
		{
			var tradeGroupCountry = Factory.New<CusRefTradeGroupCountry>();
			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			AssertEquals("China", tradeGroupCountry.CRA_Description);
			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "";
			AssertEquals("", tradeGroupCountry.CRA_Description);
			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "X";
			AssertEquals("", tradeGroupCountry.CRA_Description);
		}

		public void TestHumanReadableName()
		{
			var tradeGroupCountry = Factory.New<CusRefTradeGroupCountry>();
			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			AssertEquals("Trade Group Country/Region CN", tradeGroupCountry.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var tradeGroup = Factory.NewWithValidTestData<CusRefTradeGroup>();
			var tradeGroupCountry = tradeGroup.TradeGroupCountries.AddNew();
			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "CN";
			return tradeGroupCountry;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
	}
}
