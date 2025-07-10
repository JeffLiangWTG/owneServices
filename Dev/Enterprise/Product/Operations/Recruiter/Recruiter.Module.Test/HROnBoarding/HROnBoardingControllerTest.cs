using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HROnBoardingController))]
	public class HROnBoardingControllerTest : ZControllerBasherTest
	{
		public void TestMakeUrlsOnlyOpenableForCurrentCompanyIsTrue() => AssertEquals(Controller.MakeUrlsOnlyOpenableForCurrentCompany, true);
		public void TestControllerID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.HROnBoarding, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.HROnBoarding, controller.ModuleID);
		}

		public void TestSecurity()
		{
			AssertEquals(Env.Security.WorkItemEditModifyStaffAssignment, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WorkItemEditModifyStaffAssignment, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.WorkItemEditModifyStaffAssignment, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WorkItemEditModifyStaffAssignment, Controller.GetCheckPointForView(null));
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(HROnBoarding), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.HROnBoarding;
	}
}
