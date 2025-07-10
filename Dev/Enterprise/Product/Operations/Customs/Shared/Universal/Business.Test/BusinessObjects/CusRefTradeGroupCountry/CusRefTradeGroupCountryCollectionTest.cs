using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroupCountryCollection))]
	public class CusRefTradeGroupCountryCollectionTest : ActiveBusinessObjectCollectionTestCase<CusRefTradeGroupCountryCollection>
	{
		protected override CusRefTradeGroupCountryCollection GetCollectionToTest()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			return new CusRefTradeGroupCountryCollection(tradeGroup);
		}

		public void TestDefaultDate()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			tradeGroup.CR9_StartDate = new ZDate(2020, 11, 17);
			tradeGroup.CR9_EndDate = new ZDate(2020, 11, 27);
			var tradeGroupCountry = tradeGroup.TradeGroupCountries.AddNew();
			AssertEquals(tradeGroup.CR9_StartDate, tradeGroupCountry.CRA_StartDate);
			AssertEquals(tradeGroup.CR9_EndDate, tradeGroupCountry.CRA_EndDate);
			var tradeGroupCountry2 = Factory.New<CusRefTradeGroupCountry>();
			AssertEquals(ZDate.Empty, tradeGroupCountry2.CRA_StartDate);
			AssertEquals(ZDate.Empty, tradeGroupCountry2.CRA_EndDate);
		}
	}
}
