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
	[TestedType(typeof(OrgCreditorGroupModule))]
	sealed class OrgCreditorGroupModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgCreditorGroup;
		}

		public void TestCheckpoints()
		{
			using (OrgCreditorGroupModule module = new OrgCreditorGroupModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.CreditorGroups, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (OrgCreditorGroupModuleForTest module = new OrgCreditorGroupModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is OrgCreditorGroupFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (OrgCreditorGroupModuleForTest module = new OrgCreditorGroupModuleForTest())
			{
				IBusinessObjectCollection creditorsCollection = module.NewGridCollection;
				Assert("Invalid type", creditorsCollection is OrgCreditorGroupCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (OrgCreditorGroupModuleForTest module = new OrgCreditorGroupModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is OrgCreditorGroupFilterBusinessObject);
			}
		}

		#endregion
	}
}
