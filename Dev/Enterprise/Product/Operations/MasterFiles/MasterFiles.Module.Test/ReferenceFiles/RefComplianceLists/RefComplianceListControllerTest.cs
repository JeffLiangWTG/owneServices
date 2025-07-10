using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefComplianceListController))]
	sealed class RefComplianceListControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals("Type of Top Level BizO should be RefComplianceList", typeof(RefComplianceList), new RefComplianceListController().TypeOfTopLevelBusinessObject);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID should be RefComplianceList", ModuleIDs.RefComplianceList, new RefComplianceListController().ModuleID);
		}

		public void TestCheckPoints()
		{
			var controller = new RefComplianceListController();
			CombineAssertions(() =>
			{
				AssertEquals("For New", Env.Security.None, controller.CheckPointForNewExposedForTest);
				AssertEquals("For View", Env.Security.RefComplianceListView, controller.CheckPointForViewExposedForTest);
				AssertEquals("For Edit", Env.Security.RefComplianceListEdit, controller.CheckPointForEditExposedForTest);
				AssertEquals("For Delete", Env.Security.None, controller.CheckPointForDeleteExposedForTest);
			});
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefComplianceList;
		}
	}
}
