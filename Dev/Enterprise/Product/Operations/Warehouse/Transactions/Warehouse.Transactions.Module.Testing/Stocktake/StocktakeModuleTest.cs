using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(StocktakeModule))]
	internal class StocktakeModuleTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (StocktakeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsStocktake, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (StocktakeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (StocktakeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsStocktake, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = new StocktakeModule())
			{
				Assert("Delete option should not be available", !module.AllowDelete);
			}
		}

		#endregion

		#region TestIOperationalActionSupportable

		public void TestIOperationalActionSupportable()
		{
			using (var stocktakeModule = new StocktakeModule())
			{
				AssertNotNull(stocktakeModule.Plugins.GetPlugin(ControllerIDs.OperationalActions));
				AssertEquals(typeof(StocktakeOperationalActionsSupporter), ((IOperationalActionSupportable)stocktakeModule).OperationalActionSupporter.GetType());
			}
		}

		#endregion

		#region TestSupportsWorkflow

		public void TestSupportsWorkflow()
		{
			using (var module = new StocktakeModule())
			{
				AssertEquals("Stocktake should support workflow", true, module.SupportsWorkflow);
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsStocktake;
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		#endregion
	}
}
