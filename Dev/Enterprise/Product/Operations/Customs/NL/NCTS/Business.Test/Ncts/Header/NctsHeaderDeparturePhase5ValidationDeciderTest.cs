using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NctsHeaderDeparturePhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1823Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1823Active);
	}

	public void TestIsRuleC0001Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0001Active);
	}

	public void TestIsRuleC0001_1()
	{
		AssertEquals(false, validationDecider.IsRuleC0001_1Active);
	}

	public void TestIsRuleC0001_4Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0001_4Active);
	}

	public void TestIsRuleC0001_6Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0001_6Active);
	}

	public void TestIsRuleG0001_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleG0001_1Active);
	}

	public void TestIsRuleNR0068Active()
	{
		AssertEquals(true, validationDecider.IsRuleNR0068Active);
	}
	
	public void TestIsRuleNR0069Active()
	{
		AssertEquals(true, validationDecider.IsRuleNR0069Active);
	}

	public void TestIsRuleNR0071Active()
	{
		AssertEquals(true, validationDecider.IsRuleNR0071Active);
	}

	public void TestIsRuleNR0074Active()
	{
		AssertEquals(true, validationDecider.IsRuleNR0074Active);
	}

	public void TestIsRuleTR0079Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0079Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsHeaderDeparturePhase5ValidationDecider();
	}

	INctsHeaderDeparturePhase5ValidationDecider validationDecider;
}
