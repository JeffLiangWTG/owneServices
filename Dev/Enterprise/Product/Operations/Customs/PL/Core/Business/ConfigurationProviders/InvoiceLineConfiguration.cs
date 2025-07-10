using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class InvoiceLineConfiguration : EU.Business.InvoiceLineConfiguration
{
	protected override ZBool ValueIndicatorsSupportCore(BusinessObject businessObject) => ConfigurationHelper.GetValueIndicatorsSupport(businessObject, base.ValueIndicatorsSupportCore);

	protected override ZBool AuthorisationsSupportForInvoiceLineCore(JobDeclaration declaration) => !declaration?.IsImport ?? true;

	protected override ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => false;

	protected override IInvoiceLineValidationDecider GetValidationDeciderCore(JobComInvoiceLine invoiceLine)
	{
		return invoiceLine.Declaration?.IsUCC6AndIsImport ?? false
			? new Declaration.UCC6ImportInvoiceLineValidationDecider()
			: null;
	}
}
