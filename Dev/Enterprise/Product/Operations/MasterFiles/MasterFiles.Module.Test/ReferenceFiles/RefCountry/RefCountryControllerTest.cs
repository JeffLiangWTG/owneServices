using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCountryController))]
	sealed class RefCountryControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Factory.LoadTop1<RefCountry>(new ZQuery());
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefCountry;
		}

		public void TestSecurityCheckpoints()
		{
			TestRefCountryController controller = new TestRefCountryController();
			AssertEquals("For New", Env.Security.CountriesNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.CountriesView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.CountriesModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.CountriesDelete, controller.CheckPointForDelete);
		}
	}
}
