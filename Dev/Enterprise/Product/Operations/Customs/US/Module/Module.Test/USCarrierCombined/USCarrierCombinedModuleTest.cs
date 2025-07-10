using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCarrierCombinedModule))]
	sealed class USCarrierCombinedModuleTest : ZModuleBasherTest
	{
		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new USCarrierCombinedModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.Broker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.USCustomsCarrier, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.Carrier;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override bool HasController() => true;
	}
}
