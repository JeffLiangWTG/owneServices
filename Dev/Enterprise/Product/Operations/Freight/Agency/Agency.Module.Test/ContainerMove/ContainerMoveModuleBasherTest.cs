using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerMoveModule))]
	internal class ContainerMoveModuleBasherTest : ZModuleBasherTest
	{
		public void TestSupportsWorkflow()
		{
			using (ContainerMoveModule module = new ContainerMoveModule())
			{
				Assert(module.SupportsWorkflow);
			}
		}

		public void TestAllow()
		{
			using (ContainerMoveModule module = new ContainerMoveModule())
			{
				CombineAssertions(delegate
				{
					AssertEquals("Allow Delete", false, module.AllowDelete);
					AssertEquals("Allow New", true, module.AllowNew);
					AssertEquals("Allow Edit", true, module.AllowEdit);
				});
			}
		}

		public void TestCheckpoints()
		{
			using (ContainerMoveModule moveModule = new ContainerMoveModule())
			using (ContainerManagerModule containerModule = new ContainerManagerModule())
			{
				CombineAssertions(delegate
				{
					AssertEquals("Licence", containerModule.LicenceCheckPoint, moveModule.LicenceCheckPoint);
					AssertEquals("Security", Env.Security.AgencyContainerMovements, moveModule.SecurityCheckpoint);
				});
			}
		}

		#region Implementation
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AgencyContainerMove;
		}
		#endregion
	}
}
