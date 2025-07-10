using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffHolidayController))]
	sealed class GlbStaffHolidayControllerTest : ZControllerBasherTest
	{
		public void TestMakeUrlsOnlyOpenableForCurrentCompanyIsTrue() => AssertEquals(Controller.MakeUrlsOnlyOpenableForCurrentCompany, true);

		public void TestModuleID() => AssertEquals(Controller.ModuleID, ModuleIDs.GlbStaffHoliday);

		public void TestID() => AssertEquals(Controller.ID, ControllerIDs.GlbStaffHoliday);

		public void TestType() => AssertEquals(Controller.TypeOfTopLevelBusinessObject, typeof(GlbStaffHoliday));

		public void TestCheckpoints()
		{
			AssertEquals(Controller.CheckPointForDeleteExposedForTest, Env.Security.WorkItemEditModifyStaffAssignment);
			AssertEquals(Controller.CheckPointForEditExposedForTest, Env.Security.WorkItemEditModifyStaffAssignment);
			AssertEquals(Controller.CheckPointForNewExposedForTest, Env.Security.WorkItemEditModifyStaffAssignment);
			AssertEquals(Controller.CheckPointForViewExposedForTest, Env.Security.WorkItemEditModifyStaffAssignment);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var staffHoliday = Factory.New<GlbStaffHoliday>();
			staffHoliday.GA_StartTime = DateTime.Now;
			staffHoliday.GA_GS = staff.PK;
			Factory.Save();
			return staffHoliday;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.GlbStaffHoliday;
	}
}
