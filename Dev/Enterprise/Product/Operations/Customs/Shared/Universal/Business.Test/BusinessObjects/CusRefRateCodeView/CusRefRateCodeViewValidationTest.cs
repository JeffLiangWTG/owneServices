using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRefRateCodeViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZY1_ZZZ_NKDataGrouping()
		{
			var rateCode = Factory.New<CusRefRateCodeView>();
			rateCode.ZY1_ZZZ_NKDataGrouping = "T";
			AssertHasError(rateCode.ZY1_ZZZ_NKDataGroupingInfo, "Country/Region code must be 2 characters.");
			rateCode.ZY1_ZZZ_NKDataGrouping = "TT";
			AssertNoErrors(rateCode.ZY1_ZZZ_NKDataGroupingInfo);
			rateCode.ZY1_ZZZ_NKDataGrouping = "TTT";
			AssertHasError(rateCode.ZY1_ZZZ_NKDataGroupingInfo, "Country/Region code must be 2 characters.");
			rateCode.ZY1_IsSystem = true;
			rateCode.ZY1_ZZZ_NKDataGrouping = "TTT";
			AssertNoErrors(rateCode.ZY1_ZZZ_NKDataGroupingInfo);
		}
	}
}
