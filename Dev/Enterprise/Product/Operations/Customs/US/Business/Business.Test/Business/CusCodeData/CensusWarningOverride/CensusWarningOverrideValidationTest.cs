using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CensusWarningOverrideValidationTest : TestCaseWithFactory
	{
		public void TestCheckCY_Data()
		{
			var censusOverride = Factory.New<CensusWarningOverride>();
			censusOverride.CY_Code = CensusWarningCodeList.Codes.MaximumChargeExceeded;
			censusOverride.CY_Data = ZString.Empty;
			AssertHasMessageError(censusOverride.CY_DataInfo, CensusWarningOverrideValidation.OvercodeRequired);
			censusOverride.CY_Data = "05";
			AssertNoMessageError(censusOverride.CY_DataInfo, CensusWarningOverrideValidation.OvercodeRequired);
		}
	}
}
