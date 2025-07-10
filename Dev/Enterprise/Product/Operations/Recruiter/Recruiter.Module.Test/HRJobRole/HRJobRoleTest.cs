using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobRoleModuleForTest))]
	public class HRJobRoleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.HRJobRole;
		}

		public void TestCheckpoints()
		{
			using (HRJobRoleModule module = new HRJobRoleModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.HRJobRole, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Recruiter, module.LicenceCheckPoint);
			}
		}

		#region Properties
		public void TestGetNewFilterControl()
		{
			using (HRJobRoleModuleForTest module = new HRJobRoleModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is HRJobRoleFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (HRJobRoleModuleForTest module = new HRJobRoleModuleForTest())
			{
				IBusinessObjectCollection rolesCollection = module.NewGridCollection;
				Assert("Invalid type", rolesCollection is HRJobRoleCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (HRJobRoleModuleForTest module = new HRJobRoleModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is HRJobRoleFilterBusinessObject);
			}
		}
		#endregion
	}
}
