using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionLookupsWithOverridableShouldShowServicesAndSubModulesForTest : CommissionLookups
	{
		public CommissionLookupsWithOverridableShouldShowServicesAndSubModulesForTest(BusinessObjectFactory factory, bool shouldShowServicesAndSubModules)
			: base(factory)
		{
			this.shouldShowServicesAndSubModules = shouldShowServicesAndSubModules;
		}

		public override bool GetShouldShowServicesAndSubModules()
		{
			return shouldShowServicesAndSubModules;
		}

		readonly bool shouldShowServicesAndSubModules;
	}
}
