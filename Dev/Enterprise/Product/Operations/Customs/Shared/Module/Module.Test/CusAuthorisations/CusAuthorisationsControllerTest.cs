using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusAuthorisationsController))]
	public class CusAuthorisationsControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CusAuthorisations;

		public void TestCheckPointForView()
		{
			AssertEquals(Env.Security.AuthorisationsView, Controller.CheckPointForViewExposedForTest);
		}

		public void TestCheckPointForEdit()
		{
			AssertEquals(Env.Security.AuthorisationsEdit, Controller.CheckPointForEditExposedForTest);
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.AuthorisationsNew, Controller.CheckPointForNewExposedForTest);
		}

		public void TestCheckPointForDelete()
		{
			AssertEquals(Env.Security.AuthorisationsDelete, Controller.CheckPointForDeleteExposedForTest);
		}
	}
}
