using Enterprise.MasterFiles.Business.CountryCompliance.Argentina;
using Enterprise.MasterFiles.Business.CountryCompliance.Mexico;

namespace Enterprise.MasterFiles.Business.Accounting
{
	public class AccountingMasterFilesDependencyFactory : IAccountingMasterFilesDependencyFactory
	{
		ITaxFrameworkConfigurationHelper IAccountingMasterFilesDependencyFactory.GetTaxFrameworkConfigurationHelper() => new TaxFrameworkConfigurationHelper();
		IAccOrgTaxConfigurationTemplateDataHelper IAccountingMasterFilesDependencyFactory.GetAccOrgTaxConfigurationTemplateDataHelper() => new AccOrgTaxConfigurationTemplateDataHelper();
		IArgentinaEInvoicingExtension IAccountingMasterFilesDependencyFactory.GetArgentinaEInvoicingExtension() => new ArgentinaEInvoicingExtension();
		IMexicoEInvoicingExtension IAccountingMasterFilesDependencyFactory.GetMexicoEInvoicingExtension() => new MexicoEInvoicingExtension();
		IAccountingLogHelper IAccountingMasterFilesDependencyFactory.GetAccountingLogHelper() => new AccountingLogHelper();
		IChargeCodeOverrideRulesRanker IAccountingMasterFilesDependencyFactory.GetChargeCodeOverrideRulesRanker() => new ChargeCodeOverrideRulesRanker();
		IChargeComplianceDescriptionPostingHelper IAccountingMasterFilesDependencyFactory.GetChargeComplianceDescriptionPostingHelper() => new ChargeComplianceDescriptionPostingHelper();
		IComplianceSequencePresentationProvider IAccountingMasterFilesDependencyFactory.GetComplianceSequencePresentationProvider() => new ComplianceSequencePresentationProvider();
	}
}
