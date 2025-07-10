using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(CusUSLVClearanceController))]
	public class CusUSLVClearanceControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.USLowValueEntries;

		public void TestOverrideProperties()
		{
			var controller = new CusUSLVClearanceController();
			CombineAssertions(() =>
			{
				AssertEquals(ControllerIDs.Customs.US.USLowValueEntries, controller.ID);
				AssertEquals(ModuleIDs.Customs.US.USLowValueEntries, controller.ModuleID);
				AssertEquals(typeof(CusUSLVClearance), controller.TypeOfTopLevelBusinessObject);
				AssertEquals(Env.Security.USLVClearanceDelete, controller.GetCheckPointForDelete(null));
				AssertEquals(Env.Security.USLVClearanceEdit, controller.GetCheckPointForEdit(null));
				AssertEquals(Env.Security.USLVClearanceNew, controller.GetCheckPointForNew(null));
				AssertEquals(Env.Security.USLVClearanceView, controller.GetCheckPointForView(null));
				Assert(controller.MakeUrlsOnlyOpenableForCurrentCompany);
				using (var form = controller.ShowNewForm())
				{
					AssertType<CusUSLVClearanceForm>(form);
				}
			});
		}
	}
}
