using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class NctsBillArrivalPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1964Active()
	{
		AssertEquals(expected: true, validationDecider.IsRuleB1964Active);
	}

	public void TestIsRuleC0909Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0909Active);
	}

	public void TestIsRuleNR0062Active()
	{
		AssertEquals(expected: false, validationDecider.IsRuleNR0062Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsBillArrivalPhase5ValidationDecider();
	}

	NctsBillArrivalPhase5ValidationDecider validationDecider;
}
