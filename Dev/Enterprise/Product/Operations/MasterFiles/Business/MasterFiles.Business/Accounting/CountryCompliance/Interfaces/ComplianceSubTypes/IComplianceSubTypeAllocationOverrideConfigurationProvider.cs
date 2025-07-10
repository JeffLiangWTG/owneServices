using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Accounting.CountryCompliance
{
	public interface IComplianceSubTypeAllocationOverrideConfigurationProvider
	{
		void GetDefaults(ComplianceSubTypeAllocationOverrideConfigurationCollection collection);
	}
}
