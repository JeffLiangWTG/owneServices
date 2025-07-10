using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCRuleModule))]
	sealed class USCRuleModuleTest : ZModuleBasherTest
	{
		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new USCRuleModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.USCustomsTariffRule, module.SecurityCheckpoint);
			}
		}

		public void TestModuleAllows()
		{
			using (var module = new USCRuleModule())
			{
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
				AssertEquals("module.AllowNew", false, module.AllowNew);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USCRule;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override bool HasController() => true;
	}
}
