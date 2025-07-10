using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferModule))]
	public class TransferModuleTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (TransferModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsTransfer, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (TransferModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (TransferModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsTransfer, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestIOperationalActionSupportable

		public void TestIOperationalActionSupportable()
		{
			using (var transferModule = new TransferModule())
			{
				AssertNotNull(transferModule.Plugins.GetPlugin(ControllerIDs.OperationalActions));
				AssertEquals(typeof(TransferOperationalActionsSupporter), ((IOperationalActionSupportable)transferModule).OperationalActionSupporter.GetType());
			}
		}

		#endregion

		#region TestModuleIDAndSupportsWorkflow

		public void TestModuleIDAndSupportsWorkflow()
		{
			using (var module = new TransferModule())
			{
				AssertEquals(ModuleIDs.WhsTransfer, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsTransfer;
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		#endregion
	}
}
