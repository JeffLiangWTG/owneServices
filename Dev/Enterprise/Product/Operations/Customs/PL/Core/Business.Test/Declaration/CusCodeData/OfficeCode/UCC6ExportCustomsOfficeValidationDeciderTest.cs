using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class UCC6ExportCustomsOfficeValidationDeciderTest : TestCaseWithFactory
{
	public void TestIsRuleR0676Active() => AssertEquals(true, validationDecider.IsRuleR0676Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new UCC6ExportCustomsOfficeValidationDecider();
	}

	ICustomsOfficeValidationDecider validationDecider;
}
