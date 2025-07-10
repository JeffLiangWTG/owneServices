using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCityTownController))]
	sealed class RefCityTownControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.RefCityTown, new RefCityTownController().ModuleID);
		}

		public void TestSecurityCheckpoints()
		{
			TestRefCityTownController controller = new TestRefCityTownController();
			AssertEquals("For New", Env.Security.CityTownNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.CityTownView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.CityTownModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.CityTownDelete, controller.CheckPointForDelete);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			RefCityTown cityTown = Factory.New<RefCityTown>();
			Factory.Save();
			return cityTown;
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefCityTown;
		}

		class TestRefCityTownController : RefCityTownController
		{
			public new SecurityCheckpoint CheckPointForNew
			{
				get { return base.CheckPointForNew; }
			}

			public new SecurityCheckpoint CheckPointForView
			{
				get { return base.CheckPointForView; }
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get { return base.CheckPointForEdit; }
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get { return base.CheckPointForDelete; }
			}
		}

		#endregion
	}
}
