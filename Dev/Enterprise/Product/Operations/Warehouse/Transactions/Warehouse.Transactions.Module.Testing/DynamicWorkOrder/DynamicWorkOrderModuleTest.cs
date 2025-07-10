using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(DynamicWorkOrderModule))]
	class DynamicWorkOrderModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (var module = new DynamicWorkOrderModule())
			{
				AssertEquals(ModuleIDs.WhsDynamicWorkOrder, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestModuleID()
		{
			using (var module = (DynamicWorkOrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsDynamicWorkOrder, module.ID);
			}
		}

		public void TestBusinessContexts()
		{
			using (var module = new DynamicWorkOrderModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("WhsDynamicWorkOrder business context should be returned", BusinessContext.WhsDynamicWorkOrder, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (DynamicWorkOrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (DynamicWorkOrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsDynamicWorkOrder, module.SecurityCheckpoint);
			}
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsDynamicWorkOrder;
		}
	}
}
