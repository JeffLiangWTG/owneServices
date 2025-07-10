using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusPermitController))]
	public class CusPermitControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new CusPermitController();
			AssertEquals("For View", Env.Security.PermitsView, controller.CheckPointForViewExposedForTest);
			AssertEquals("For Edit", Env.Security.PermitsModify, controller.CheckPointForEditExposedForTest);
			AssertEquals("For New", Env.Security.PermitsNew, controller.CheckPointForNewExposedForTest);
			AssertEquals("For Delete", Env.Security.PermitsDelete, controller.CheckPointForDeleteExposedForTest);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.Permits;
	}
}
