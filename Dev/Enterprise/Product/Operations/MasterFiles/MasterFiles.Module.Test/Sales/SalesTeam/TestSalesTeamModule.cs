using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesTeamModule))]
	sealed class TestSalesTeamModule : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SalesTeam;
		}

		public void TestCheckpoints()
		{
			using (SalesTeamModule module = new SalesTeamModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.SalesTeams, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}
	}
}
