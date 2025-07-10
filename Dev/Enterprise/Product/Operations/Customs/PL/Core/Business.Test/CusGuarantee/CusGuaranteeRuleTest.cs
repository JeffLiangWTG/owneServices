using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using PermitRuleCodeList = Enterprise.Customs.EU.Business.PermitRuleCodeList;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CusGuaranteeRule))]
sealed class CusGuaranteeRuleTest : EnterpriseBusinessObjectTestCase
{
	public void TestCPR_ValueFrom_MaxLength() => CombineAssertions(() =>
	{
		var cusGuaranteeRuleTest = Factory.New<CusGuaranteeRule>();

		AssertCPR_ValueFrom_MaxLength("AccessCode - DPN", PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin, AutoCusPermitHeader.Schema.CPH_AccessCodeOrPasswordMaxLength);
		AssertCPR_ValueFrom_MaxLength("AccessCode - PIN", PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber, AutoCusPermitHeader.Schema.CPH_AccessCodeOrPasswordMaxLength);
		AssertCPR_ValueFrom_MaxLength("Customs Office", PermitRuleCodeList.Codes.CUS, 8);
		AssertCPR_ValueFrom_MaxLength("Default", string.Empty, AutoCusPermitRule.Schema.CPR_ValueFromMaxLength);

		void AssertCPR_ValueFrom_MaxLength(string message, string type, int expectedMaxLength)
		{
			cusGuaranteeRuleTest.CPR_RuleCode = type;
			AssertEquals(message, expectedMaxLength, cusGuaranteeRuleTest.CPR_ValueFromInfo.MaxLength);
		}
	});
}
