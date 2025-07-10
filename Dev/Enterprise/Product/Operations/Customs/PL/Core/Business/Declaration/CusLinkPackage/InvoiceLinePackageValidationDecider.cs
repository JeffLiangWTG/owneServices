using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public sealed class InvoiceLinePackageValidationDecider : IInvoiceLinePackageValidationDecider
{
	public bool IsRuleR0219Active => false;

	public bool IsRuleR0220Active => false;

	public bool IsRuleR0364Active => true;
}
