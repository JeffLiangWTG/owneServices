using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(CourtesyNoticesOfLiquidationModule))]
	sealed class CourtesyNoticesOfLiquidationModuleTest : ZModuleBasherTest
	{
		public void TestSecurityCheckPoint()
		{
			using (var module = new CourtesyNoticesOfLiquidationModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.CourtesyNoticesOfLiquidationMessages, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.CourtesyNoticesOfLiquidation;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
