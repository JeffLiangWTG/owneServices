using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class InvoiceLinePackageValidationDeciderTest : TestCaseWithFactory
{
	public void TestIsRuleR0219Active() => AssertEquals(false, validationDecider.IsRuleR0219Active);

	public void TestIsRuleR0220Active() => AssertEquals(false, validationDecider.IsRuleR0220Active);

	public void TestIsRuleR0364Active() => AssertEquals(true, validationDecider.IsRuleR0364Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new InvoiceLinePackageValidationDecider();
	}

	IInvoiceLinePackageValidationDecider validationDecider;
}
