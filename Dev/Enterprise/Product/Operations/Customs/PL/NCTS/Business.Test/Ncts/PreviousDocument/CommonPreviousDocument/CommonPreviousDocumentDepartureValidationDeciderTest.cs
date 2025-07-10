using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

public sealed class CommonPreviousDocumentDepartureValidationDeciderTest : TestCase
{
	public void TestIsRuleG0026_1Active() => AssertEquals(false, validationDecider.IsRuleG0026_1Active);

	public void TestIsRuleNR0008Active() => AssertEquals(false, validationDecider.IsRuleNR0008Active);

	public void TestIsRuleR0416Active() => AssertEquals(true, validationDecider.IsRuleR0416Active);

	public void TestIsRuleTR0030_1Active() => AssertEquals(false, validationDecider.IsRuleTR0030_1Active);

	public void TestIsRuleE1301Active() => AssertEquals(true, validationDecider.IsRuleE1301Active);

	protected override void SetUp()
	{
		validationDecider = new();
	}

	CommonPreviousDocumentDepartureValidationDecider validationDecider;
}
