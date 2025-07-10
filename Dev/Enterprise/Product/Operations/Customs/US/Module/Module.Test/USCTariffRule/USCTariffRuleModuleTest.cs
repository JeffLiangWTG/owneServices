using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCTariffRuleModule))]
	sealed class USCTariffRuleModuleTest : ZModuleBasherTest
	{
		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new USCTariffRuleModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.USCustomsTariffRule, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USCTariffRule;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override bool HasController() => true;
	}
}
