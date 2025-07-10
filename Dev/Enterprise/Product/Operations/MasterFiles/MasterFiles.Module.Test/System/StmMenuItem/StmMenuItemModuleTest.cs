using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StmMenuItemModule))]
	sealed class StmMenuItemModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.StmMenuItem;
		}

		protected override bool HasController()
		{
			return false;
		}

		public void TestHasActions()
		{
			using (StmMenuItemModule module = new StmMenuItemModule())
			{
				AssertEquals(module.HasActions, false);
			}
		}

		public void TestCheckpoints()
		{
			using (StmMenuItemModule module = new StmMenuItemModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.Organisation, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (StmMenuItemModuleForTest module = new StmMenuItemModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is StmMenuItemFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (StmMenuItemModuleForTest module = new StmMenuItemModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is StmMenuItemCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (StmMenuItemModuleForTest module = new StmMenuItemModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is StmMenuItemFilterBusinessObject);
			}
		}

		public void TestGetNewController()
		{
			using (StmMenuItemModuleForTest module = new StmMenuItemModuleForTest())
			{
				AssertEquals("Do not need a controller as there is no form associated with this module. It is only used for a find box.", module.NewController(new StmMenuItemFilterBusinessObject()), null);
			}
		}

		sealed class StmMenuItemModuleForTest : StmMenuItemModule
		{
			public StmMenuItemModuleForTest()
			{
			}

			public IFilterControl NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get { return GetNewGridCollection(); }
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}

			public ZController NewController(BusinessObject selectedBusinessObject)
			{
				return GetNewController(selectedBusinessObject);
			}
		}
	}
}
