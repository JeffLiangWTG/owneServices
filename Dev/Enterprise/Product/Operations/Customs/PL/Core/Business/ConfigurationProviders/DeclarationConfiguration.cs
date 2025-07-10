using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
{
	protected override ZBool DV1DetailsSupportCore(BusinessObject businessObject) => true;

	protected override ZBool IsUCC6Core(BusinessObject businessObject) => (businessObject is JobDeclaration jobDeclaration)
																		&& (jobDeclaration.IsExport || jobDeclaration.IsImport);

	protected override EU.Business.InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

	protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

	protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

	protected override ZBool UCCAdditionalInfosSupportCore(BusinessObject businessObject) => businessObject is Declaration.JobDeclaration declaration && declaration.IsExport;

	protected override ZBool MiscAdditionalInfosSupportCore(BusinessObject businessObject) => false;

	protected override ZBool MiscPreviousDocumentsSupportCore(BusinessObject businessObject) => false;

	protected override ZBool MiscSupportingDocumentsSupportCore(BusinessObject businessObject) => false;

	protected override ZBool UseEucdmSupportingDocumentGoodsShipmentAndItemCore => true;

	protected override EU.Business.ICustomsOfficeValidationDecider GetUCC6CustomsOfficeValidationDecider(EU.Business.Declaration.JobDeclaration declaration) =>
		declaration.IsExport
			? new UCC6ExportCustomsOfficeValidationDecider()
			: base.GetUCC6CustomsOfficeValidationDecider(declaration);

	protected override ZBool IsPopulateAuthorisationsForOfficeOfPresentationEnabledCore(EU.Business.Declaration.JobDeclaration declaration) => declaration.IsExport;

	protected override EU.Business.Declaration.IDeclarationValidationDecider GetValidationDeciderCore(BusinessObject businessObject) =>
		businessObject is JobDeclaration declaration && declaration.IsUCC6AndIsImport
			? new UCC6ImportDeclarationValidationDecider()
			: base.GetValidationDeciderCore(businessObject);

	protected override EU.Business.Declaration.IInvoiceLinePackageValidationDecider GetInvoiceLinePackageValidationDeciderCore() => new InvoiceLinePackageValidationDecider();
}
