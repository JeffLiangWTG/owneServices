using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Declaration;

sealed class UCC6ImportInvoiceHeaderValidationDecider : IInvoiceHeaderValidationDecider
{
	public bool IsRuleC0002Active => false;

	public bool IsRuleR0012Active => true;

	public bool IsRuleC0624Active => true;

	public bool IsRuleC0627Active => true;

	public bool IsRuleC0729Active => true;

	public bool IsRuleC0728Active => true;

	public bool IsRuleC0738Active => true;
}
