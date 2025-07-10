using Enterprise.Environment;
using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Module.Test
{
	[TestedType(typeof(ReviewProcessNodeController))]
	public class ReviewProcessNodeControllerTest : ZControllerBasherTest
	{
		public void TestControllerID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ControllerIDs.ReviewProcessNode, controller.ID);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.ReviewProcessNode, controller.ModuleID);
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
			AssertEquals(typeof(ReviewProcessNode), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.ReviewProcessNode;
	}
}
