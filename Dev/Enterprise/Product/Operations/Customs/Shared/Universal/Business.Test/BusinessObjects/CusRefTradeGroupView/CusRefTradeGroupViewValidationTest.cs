using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRefTradeGroupViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZA_ZZZ_NKDataGrouping()
		{
			var tradeGroup = Factory.New<CusRefTradeGroupView>();
			tradeGroup.ZZA_ZZZ_NKDataGrouping = "TTT";
			AssertHasError(tradeGroup.ZZA_ZZZ_NKDataGroupingInfo, "Country/Region code cannot be longer than 2 characters.");
			tradeGroup.ZZA_ZZZ_NKDataGrouping = "TT";
			AssertNoErrors(tradeGroup.ZZA_ZZZ_NKDataGroupingInfo);
			tradeGroup.ZZA_IsSystem = true;
			tradeGroup.ZZA_ZZZ_NKDataGrouping = "TTT";
			AssertNoErrors(tradeGroup.ZZA_ZZZ_NKDataGroupingInfo);
		}

		public void TestCheckZZA_TradeGroup()
		{
			var tradeGroup = Factory.New<CusRefTradeGroupView>();
			tradeGroup.ZZA_TradeGroup = "T0123456789";
			AssertHasError(tradeGroup.ZZA_TradeGroupInfo, "Non-system trade group cannot be longer than 10 characters.");
			tradeGroup.ZZA_TradeGroup = "T012345678";
			AssertNoErrors(tradeGroup.ZZA_TradeGroupInfo);
			tradeGroup.ZZA_IsSystem = true;
			tradeGroup.ZZA_TradeGroup = "T0123456789";
			AssertNoErrors(tradeGroup.ZZA_TradeGroupInfo);
		}
	}
}
