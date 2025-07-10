using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
{
	protected override ZBool AdditionalInfosSupportCore(BusinessObject businessObject) => !(businessObject is IImportExport importExport) || importExport.IsImport() || importExport.IsExport();

	protected override ZBool SupportingDocumentsSupportCore(BusinessObject businessObject) => !(businessObject is IImportExport importExport) || importExport.IsImport() || importExport.IsExport();

	protected override ZBool PreviousDocumentsSupportCore(EU.Business.Declaration.JobDeclaration declaration) => declaration.IsImport || declaration.IsExport;

	protected override ZBool AgreedPlaceCodeSupportCore(BusinessObject businessObject) => businessObject is IImportExport importExport
																						&& (importExport.IsExport() || importExport.IsImport());

	protected override ZBool ValueIndicatorsSupportCore(BusinessObject businessObject) => ConfigurationHelper.GetValueIndicatorsSupport(businessObject, base.ValueIndicatorsSupportCore);

	protected override EU.Business.Declaration.IInvoiceHeaderValidationDecider GetValidationDeciderCore(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) =>
		invoiceHeader.JobDeclaration?.IsUCC6AndIsImport ?? false
			? new UCC6ImportInvoiceHeaderValidationDecider()
			: null;
}
