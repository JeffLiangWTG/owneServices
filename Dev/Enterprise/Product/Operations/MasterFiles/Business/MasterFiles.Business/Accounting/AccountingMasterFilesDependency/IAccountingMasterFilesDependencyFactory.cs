using Enterprise.MasterFiles.Business.CountryCompliance.Argentina;
using Enterprise.MasterFiles.Business.CountryCompliance.Mexico;

namespace Enterprise.MasterFiles.Business.Accounting
{
	public interface IAccountingMasterFilesDependencyFactory
	{
		ITaxFrameworkConfigurationHelper GetTaxFrameworkConfigurationHelper();
		IAccOrgTaxConfigurationTemplateDataHelper GetAccOrgTaxConfigurationTemplateDataHelper();
		IArgentinaEInvoicingExtension GetArgentinaEInvoicingExtension();
		IMexicoEInvoicingExtension GetMexicoEInvoicingExtension();
		IAccountingLogHelper GetAccountingLogHelper();
		IChargeCodeOverrideRulesRanker GetChargeCodeOverrideRulesRanker();
		IChargeComplianceDescriptionPostingHelper GetChargeComplianceDescriptionPostingHelper();
		IComplianceSequencePresentationProvider GetComplianceSequencePresentationProvider();
	}
}
