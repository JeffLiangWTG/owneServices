using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	[TestedType(typeof(JobShipmentPreplanningModule))]
	public class JobShipmentPreplanningModule_Test : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (JobShipmentPreplanningModule module = new JobShipmentPreplanningModule())
			{
				AssertEquals(ModuleIDs.JobShipmentPreplanning, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobShipmentPreplanning;
		}
	}
}
