using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsSupportingDocumentDeparturePhase5ValidationDecider))]
sealed class NctsSupportingDocumentDeparturePhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleE1301Active() => AssertEquals(true, validationDecider.IsRuleE1301Active);

	public void TestIsRuleG0321Active() => AssertEquals(true, validationDecider.IsRuleG0321Active);

	public void TestIsRuleNR0006Active() => AssertEquals(false, validationDecider.IsRuleNR0006Active);

	public void TestIsRuleRP30Active() => AssertEquals(true, validationDecider.IsRuleRP30Active);

	protected override void SetUp()
	{
		validationDecider = new();
	}

	NctsSupportingDocumentDeparturePhase5ValidationDecider validationDecider;
}
