using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class UniversalValidationHelperTest : TestCaseWithFactory
{
	public void TestIsInAESTransitionPeriod_True()
	{
		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Poland, ZDate.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals("Functionality is enabled for Poland", true, UniversalValidationHelper.IsInAESTransitionPeriod);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("Functionality is enabled for EUN", true, UniversalValidationHelper.IsInAESTransitionPeriod);
			}
		});
	}

	public void TestIsInAESTransitionPeriod_False()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			AssertEquals("Functionality is disabled", false, UniversalValidationHelper.IsInAESTransitionPeriod);
		}
	}
}
