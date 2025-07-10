using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefComplianceCommodityAlertController))]
	sealed class RefComplianceCommodityAlertControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals("Type of Top Level BizO should be RRefComplianceCommodityAlert", typeof(RefComplianceCommodityAlert), new RefComplianceCommodityAlertController().TypeOfTopLevelBusinessObject);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID should be RRefComplianceCommodityAlert", ModuleIDs.RefComplianceCommodityAlert, new RefComplianceCommodityAlertController().ModuleID);
		}

		public void TestCheckPoints()
		{
			var controller = new RefComplianceCommodityAlertController();
			CombineAssertions(() =>
			{
				AssertEquals("For New", Env.Security.None, controller.CheckPointForNewExposedForTest);
				AssertEquals("For View", Env.Security.RefComplianceCommodityAlertView, controller.CheckPointForViewExposedForTest);
				AssertEquals("For Edit", Env.Security.RefComplianceCommodityAlertEdit, controller.CheckPointForEditExposedForTest);
				AssertEquals("For Delete", Env.Security.None, controller.CheckPointForDeleteExposedForTest);
			});
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefComplianceCommodityAlert;
		}
	}
}
