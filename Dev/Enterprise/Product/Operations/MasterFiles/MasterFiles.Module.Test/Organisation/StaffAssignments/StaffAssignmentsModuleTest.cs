using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StaffAssignmentsModule))]
	sealed class StaffAssignmentsModuleTest : ZModuleBasherTest
	{
		public void TestAllowPropertiesAreDisabled()
		{
			Assert("AllowNew", !module.AllowNew);
			Assert("AllowDelete", !module.AllowDelete);
		}

		public void TestCheckpoints()
		{
			AssertEquals("LicenceCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			AssertEquals("SecurityCheckpoint", Env.Security.OrgDetailsViewCompanysStaffAssignments, module.SecurityCheckpoint);
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			using (var control = module.GetNewFilterControlForGrid())
			{
				AssertType<StaffAssignmentsFilterControl>(control);
			}
		}

		public void TestFilterBusinessObject()
		{
			AssertType<StaffAssignmentsFilterBusinessObject>(module.FilterBusinessObject);
		}

		public void TestGridCollection()
		{
			AssertType<ActiveBusinessObjectCollection<OrgStaffAssignments>>(module.GridCollection);
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new StaffAssignmentsModule();
		}

		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.StaffAssignments;
		}

		StaffAssignmentsModule module;
	}
}

