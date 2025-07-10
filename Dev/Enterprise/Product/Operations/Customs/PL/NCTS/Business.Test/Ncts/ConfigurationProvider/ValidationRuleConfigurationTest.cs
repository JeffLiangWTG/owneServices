using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(ValidationRuleConfiguration))]
sealed class ValidationRuleConfigurationTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
{
	public override void TestIsRuleB1896Active() => AssertEquals(true, configuration.IsRuleB1896Active);

	public override void TestIsRuleC0236Active() => AssertEquals(false, configuration.IsRuleC0236Active);

	public override void TestIsRuleC0839Active() => AssertEquals(false, configuration.IsRuleC0839Active);

	public override void TestIsRuleG0321Active() => AssertEquals(true, configuration.IsRuleG0321Active);

	public override void TestIsRuleG0587Active() => AssertEquals(true, configuration.IsRuleG0587Active);

	public void TestIsRuleNR0032Active() => AssertEquals(true, configuration.IsRuleNR0032Active);

	public void TestIsRuleNR0033Active() => AssertEquals(true, configuration.IsRuleNR0033Active);

	public override void TestIsRuleRP16Active() => AssertEquals(true, configuration.IsRuleRP16Active);
}
