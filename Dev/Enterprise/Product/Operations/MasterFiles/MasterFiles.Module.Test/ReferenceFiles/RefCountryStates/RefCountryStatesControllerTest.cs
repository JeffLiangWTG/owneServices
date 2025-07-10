using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCountryStatesController))]
	sealed class RefCountryStatesControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefCountryStates;
		}

		public void TestSecurityCheckpoints()
		{
			TestRefCountryStatesController controller = new TestRefCountryStatesController();
			AssertEquals("For New", Env.Security.StatesNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.StatesView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.StatesModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.StatesDelete, controller.CheckPointForDelete);
		}
	}
}
