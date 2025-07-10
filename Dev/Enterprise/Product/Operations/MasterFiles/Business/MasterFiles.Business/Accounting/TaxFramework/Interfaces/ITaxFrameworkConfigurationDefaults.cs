using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface ITaxFrameworkConfigurationDefaults
	{
		List<TaxAuthoritiesConfiguration> GetTaxAuthorities();

		List<TaxSystemsConfiguration> GetTaxSystems();
	}
}
