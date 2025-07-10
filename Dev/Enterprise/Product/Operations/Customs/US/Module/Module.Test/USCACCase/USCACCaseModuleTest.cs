using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCACCaseModule))]
	sealed class USCACCaseModuleTest : ZModuleBasherTest
	{
		public void TestSecurityCheckpoint()
		{
			using (var module = new USCACCaseModule())
			{
				AssertEquals("No need to have a security right as they cannot modify", Env.Security.None, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USCACCase;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
