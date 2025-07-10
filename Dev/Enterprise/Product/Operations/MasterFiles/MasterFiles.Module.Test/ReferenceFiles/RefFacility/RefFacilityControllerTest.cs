using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefFacilityController))]
	sealed class RefFacilityControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			AssertExceptionThrown<SecurityAccessDeniedException>(
				"To register a new facility with CargoWise, please raise a CR8 Compliance. Once this request has been processed the new record will be available for selection in your system.",
				() => Controller.ShowNewForm());
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals("Type of Top Level BizO should be RefFacility", typeof(RefFacility), new RefFacilityController().TypeOfTopLevelBusinessObject);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID should be RefFacilityCode", ModuleIDs.RefFacility, new RefFacilityController().ModuleID);
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new RefFacilityController();
			CombineAssertions(() =>
			{
				AssertType<RefFacilityNewSecurityCheckpoint>("For New", controller.CheckPointForNewExposedForTest);
				AssertEquals("For View", Env.Security.RefFacilityView, controller.CheckPointForViewExposedForTest);
				AssertEquals("For Edit", Env.Security.RefFacilityEdit, controller.CheckPointForEditExposedForTest);
				AssertEquals("For Delete", Env.Security.RefFacilityDelete, controller.CheckPointForDeleteExposedForTest);
			});
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefFacility;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var refFacility = Factory.NewWithValidTestData<RefFacility>();
			refFacility.RFT_Name = "TEST";
			Factory.Save();
			return refFacility;
		}

		#endregion
	}
}
