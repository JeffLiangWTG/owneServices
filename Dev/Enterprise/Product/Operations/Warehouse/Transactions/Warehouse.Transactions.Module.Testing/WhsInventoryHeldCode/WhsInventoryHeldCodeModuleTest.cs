using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WhsInventoryHeldCodeModule))]
	class WhsInventoryHeldCodeModuleTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (WhsInventoryHeldCodeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsInventoryHeldCodes, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (WhsInventoryHeldCodeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (WhsInventoryHeldCodeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigInventoryHeldCode, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestShowRecentItems

		public void TestShowRecentItems()
		{
			using (var module = new WhsInventoryHeldCodeModuleForTest())
			{
				AssertEquals("InventoryHeldCode Module itself should be able to see recent items.", true, module.ShowRecentItemsExposedForTest);
			}
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = new WhsInventoryHeldCodeModule())
			{
				Assert("Delete option should  be available", module.AllowDelete);
			}
		}

		#endregion

		#region TestFilterControl

		public void TestFilterControl()
		{
			using (var module = new WhsInventoryHeldCodeModuleForTest())
			{
				var controlForTest = module.GetNewFilterControlForTest();
				Assert(controlForTest is WhsInventoryHeldCodeFilterControl);
				controlForTest.Dispose();
			}
		}

		#endregion

		#region TestFilterBusinessObject

		public void TestFilterBusinessObject()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateInventoryHeldCode("TEST1", "Blah1");
			helper.CreateInventoryHeldCode("TEST2", "Blah2");

			Factory.Save();

			using (var module = new WhsInventoryHeldCodeModule())
			{
				AssertEquals(typeof(WhsInventoryHeldCodeFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		#endregion

		#region GetIsSystemDefinedDefaultProperty

		protected override string GetIsSystemDefinedDefaultProperty()
		{
			return "All"; // Filter option
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsInventoryHeldCodes;
		}

		#endregion
	}

	#region WhsInventoryHeldCodeModuleForTest

	public class WhsInventoryHeldCodeModuleForTest : WhsInventoryHeldCodeModule
	{
		public IFilterControl GetNewFilterControlForTest()
		{
			return GetNewFilterControl();
		}

		public bool ShowRecentItemsExposedForTest => ShowRecentItems;
	}

	#endregion
}
