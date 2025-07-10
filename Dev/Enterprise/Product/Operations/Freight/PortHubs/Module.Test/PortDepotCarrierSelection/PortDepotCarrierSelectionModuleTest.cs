using System;
using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Testing
{
	[TestedType(typeof(PortDepotCarrierSelectionModule))]
	public class PortDepotCarrierSelectionModuleTest : ZModuleBasherTest
	{
		public void TestCheckpoints()
		{
			using (HVLVDataRegistry.Instance.PortCarrierDepotSelectionModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = new PortDepotCarrierSelectionModule())
			{
				AssertEquals(Env.Security.PortDepotSelection, module.SecurityCheckpoint);
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
				AssertType<PortDepotCarrierSelectionController>(module.GetNewController());
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.PortDepotCarrierSelection;
		}
	}
}
