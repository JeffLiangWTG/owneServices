using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffChangeRequestController))]
	public class GlbStaffChangeRequestControllerTest : ZControllerBasherTest
	{
		public void TestMakeUrlsOnlyOpenableForCurrentCompanyIsTrue() => AssertEquals(Controller.MakeUrlsOnlyOpenableForCurrentCompany, true);

		public void TestControllerID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.GlbStaffChangeRequest, controller.ID);
		}

		public void TestModuleId()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.GlbStaffChangeRequest, controller.ModuleID);
		}

		public void TestSecurity()
		{
			AssertEquals(Env.Security.None, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.None, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.None, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.GlbStaffChangeRequestView, Controller.GetCheckPointForView(null));
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(GlbStaffChangeRequest), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.GlbStaffChangeRequest;
	}
}
