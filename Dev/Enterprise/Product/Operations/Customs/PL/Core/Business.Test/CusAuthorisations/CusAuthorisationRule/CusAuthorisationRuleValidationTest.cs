using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCPR_ValueFrom()
	{
		const string messageError = "Only alphanumeric up to 17 characters are allowed";
		cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		CombineAssertions(() =>
		{
			cusAuthorisationRule.CPR_ValueFrom = "desperatesunsets";
			AssertNoErrorContaining("If the value is alphanumeric", cusAuthorisationRule.CPR_ValueFromInfo, messageError);

			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			cusAuthorisationRule.CPR_ValueFrom = "despera!t";
			AssertHasError("If the value contains strange characters", cusAuthorisationRule.CPR_ValueFromInfo, messageError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
	}
	CusAuthorisationRule cusAuthorisationRule;
}
