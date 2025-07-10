using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefAccessorialController))]
	class RefAccessorialControllerTest : ZControllerBasherTest
	{
		public void TestImplementation()
		{
			AssertEquals(ModuleIDs.RefAccessorial, Controller.ModuleID);
			AssertEquals(typeof(RefAccessorial), Controller.TypeOfTopLevelBusinessObject);
			AssertEquals(expected: true, Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		public void TestSecurityCheckpoints()
		{
			var refAccessorial = Factory.New<RefAccessorial>();

			CombineAssertions(() =>
			{
				AssertEquals(Controller.GetCheckPointForNew(refAccessorial), Env.Security.None);
				AssertEquals(Controller.GetCheckPointForEdit(refAccessorial), Env.Security.None);
				AssertEquals(Controller.GetCheckPointForView(refAccessorial), Env.Security.RefAccessorialView);
				AssertEquals(Controller.GetCheckPointForDelete(refAccessorial), Env.Security.None);
			});
		}

		protected override ControllerID GetControllerID()
			=> ControllerIDs.RefAccessorial;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = base.Factory.NewWithValidTestData(GetBusinessObjectType());
			base.Factory.Save();
			return result;
		}
	}
}
