using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	interface IComplianceSubTypeDependencyConfigurationProvider
	{
		void GetDefaults(ComplianceSubTypeDependencyConfigurationCollection collection);
	}
}