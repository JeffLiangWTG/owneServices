using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Testing
{
	[TestedType(typeof(PortHubSelectionModule))]
	public class PortHubSelectionModuleTest : ZPopupModuleBasherTest
	{
		public void TestCheckpoints()
		{
			using (var module = new PortHubSelectionModule())
			{
				AssertEquals(Env.Security.PortDepotSelection, module.SecurityCheckpoint);
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.PortHubSelection;
		}
	}
}
