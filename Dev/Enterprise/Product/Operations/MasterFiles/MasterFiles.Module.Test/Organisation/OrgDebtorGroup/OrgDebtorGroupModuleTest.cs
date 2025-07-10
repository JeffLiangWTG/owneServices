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
	[TestedType(typeof(OrgDebtorGroupModule))]
	sealed class OrgDebtorGroupModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgDebtorGroup;
		}

		public void TestCheckpoints()
		{
			using (OrgDebtorGroupModule module = new OrgDebtorGroupModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.DebtorGroups, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (OrgDebtorGroupModuleForTest module = new OrgDebtorGroupModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is OrgDebtorGroupFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (OrgDebtorGroupModuleForTest module = new OrgDebtorGroupModuleForTest())
			{
				IBusinessObjectCollection debtorsCollection = module.NewGridCollection;
				Assert("Invalid type", debtorsCollection is OrgDebtorGroupCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (OrgDebtorGroupModuleForTest module = new OrgDebtorGroupModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is OrgDebtorGroupFilterBusinessObject);
			}
		}

		#endregion
	}
}
