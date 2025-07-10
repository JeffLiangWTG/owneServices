using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(UCC6ImportInvoiceHeaderValidationDecider))]
sealed class UCC6ImportInvoiceHeaderValidationDeciderTest : InvoiceHeaderValidationDeciderTest<UCC6ImportInvoiceHeaderValidationDecider>
{
	protected override bool ExpectedIsRuleC0002Active => false;

	protected override bool ExpectedIsRuleC0624Active => true;

	protected override bool ExpectedIsRuleC0627Active => true;

	protected override bool ExpectedIsRuleC0729Active => true;

	protected override bool ExpectedIsRuleC0728Active => true;

	protected override bool ExpectedIsRuleR0012Active => true;

	protected override bool ExpectedIsRuleC0738Active => true;
}
