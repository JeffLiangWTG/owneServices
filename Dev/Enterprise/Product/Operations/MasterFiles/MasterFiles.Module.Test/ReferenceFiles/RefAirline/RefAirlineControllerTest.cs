using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefAirlineController))]
	sealed class RefAirlineControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject testObject = Factory.New(typeof(RefAirline));
			Factory.Save();
			return testObject;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefAirline;
		}

		public void TestSecurityCheckpoints()
		{
			RefAirlineController controller = new RefAirlineController();

			AssertEquals("CheckPointForEdit", Env.Security.RefAirlineModify, controller.CheckPointForEditForTesting);
			AssertEquals("CheckPointForDelete", Env.Security.RefAirlineModify, controller.CheckPointForDeleteForTesting);
			AssertEquals("CheckPointForNew", Env.Security.RefAirlineModify, controller.CheckPointForNewForTesting);
			AssertEquals("CheckPointForView", Env.Security.RefAirline, controller.CheckPointForViewForTesting);
		}
	}
}
