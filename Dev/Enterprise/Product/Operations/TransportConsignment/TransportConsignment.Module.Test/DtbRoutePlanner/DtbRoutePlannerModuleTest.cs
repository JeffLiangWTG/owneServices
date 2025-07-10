using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbRoutePlannerModule))]
	public class DtbRoutePlannerModuleTest : ZPopupModuleBasherTest
	{
		#region TestLicenseCheckpoint

		public void TestLicenseCheckpoint()
		{
			using (var module = new DtbRoutePlannerModule())
			{
				AssertEquals(module.LicenceCheckPoint, Env.Licence.LandTransport);
			}
		}

		#endregion

		#region TestSecurityCheckpoint

		public void TestSecurityCheckpoint()
		{
			using (var module = new DtbRoutePlannerModule())
			{
				AssertEquals(module.SecurityCheckpoint, Env.Security.DtbRoutePlanner);
			}
		}

		#endregion

		public void TestModuleID()
		{
			using (var module = new DtbRoutePlannerModule())
			{
				AssertEquals(ModuleIDs.DtbRoutePlanner, module.ID);
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DtbRoutePlanner;
		}

		#endregion
	}
}
