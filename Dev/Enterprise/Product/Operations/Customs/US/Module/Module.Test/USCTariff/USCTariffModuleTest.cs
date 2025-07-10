using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCTariffModule))]
	sealed class USCTariffModuleTest : ZModuleBasherTest
	{
		public void TestLicenceCheckpoint()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
			}
		}

		public void TestCustomSQLFilter()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var filterBusinessObject = module.FilterBusinessObject;
				var moduleFilters = filterBusinessObject.ModuleFilters;
				var customSqlFilterDescription = "Custom SQL Filter";
				AssertNotNull(moduleFilters[customSqlFilterDescription]);
			}
		}

		protected override bool HasController() => false;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.Tariff;

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;
	}
}
