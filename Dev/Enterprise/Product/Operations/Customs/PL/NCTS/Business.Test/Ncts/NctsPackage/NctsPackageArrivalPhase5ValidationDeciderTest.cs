using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class NctsPackageArrivalPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1919Active() => AssertEquals(expected: true, validationDecider.IsRuleB1919Active);

	public void TestIsRuleC0670Active() => AssertEquals(expected: true, validationDecider.IsRuleC0670Active);

	public void TestIsRuleNR0029Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0029Active);

	public void TestIsRuleNR0061Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0061Active);

	public void TestIsRuleR0220Active() => AssertEquals(expected: true, validationDecider.IsRuleR0220Active);

	public void TestIsRuleTR0097Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0097Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsPackageArrivalPhase5ValidationDecider();
	}

	INctsPackageArrivalPhase5ValidationDecider validationDecider;
}
