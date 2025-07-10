using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefPostCodeController))]
	sealed class RefPostCodeControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.RefPostCode, new RefPostCodeController().ModuleID);
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new RefPostCodeControllerForTest();
			AssertEquals("For New", Env.Security.PostCodeNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.PostCodeView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.PostCodeModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.PostCodeDelete, controller.CheckPointForDelete);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var cityTown = Factory.New<RefPostCode>();
			Factory.Save();
			return cityTown;
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefPostCode;
		}

		#endregion
	}
}
