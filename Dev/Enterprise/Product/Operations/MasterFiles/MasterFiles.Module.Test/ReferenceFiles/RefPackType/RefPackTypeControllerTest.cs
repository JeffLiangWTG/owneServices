using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefPackTypeController))]
	sealed class RefPackTypeControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject testObject = Factory.New(typeof(RefPackType));
			Factory.Save();
			return testObject;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefPackType;
		}

		public void TestSecurityCheckpoints()
		{
			RefPackTypeController controller = new RefPackTypeController();

			AssertEquals("CheckPointForEdit", Env.Security.RefPackTypeModify, controller.CheckPointForEditForTesting);
			AssertEquals("CheckPointForDelete", Env.Security.RefPackTypeModify, controller.CheckPointForDeleteForTesting);
			AssertEquals("CheckPointForNew", Env.Security.RefPackTypeModify, controller.CheckPointForNewForTesting);
			AssertEquals("CheckPointForView", Env.Security.RefPackType, controller.CheckPointForViewForTesting);
		}
	}
}
