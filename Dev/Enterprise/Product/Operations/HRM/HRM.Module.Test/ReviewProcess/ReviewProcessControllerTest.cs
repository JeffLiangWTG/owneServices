using Enterprise.Environment;
using Enterprise.HRM.Common;
using Enterprise.HRM.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(ReviewProcessController))]
	public class ReviewProcessControllerTest : ZControllerBasherTest
	{
		public void TestControllerID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.ReviewProcess, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.ReviewProcess, controller.ModuleID);
		}

		public void TestSecurity()
		{
			AssertEquals(Env.Security.None, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.None, Controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.None, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.None, Controller.GetCheckPointForView(null));
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(typeof(ReviewProcess), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.ReviewProcess;
	}
}
