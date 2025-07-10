using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StaffNumberRangesPlugInController))]
	sealed class StaffNumberRangesPlugInControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.StaffNumberRangesPlugIn;
		}

		public void TestSecurityCheckPoint()
		{
			var staff = Factory.New<GlbStaff>();
			var controller = new StaffNumberRangesPlugInController();
			AssertEquals(Env.Security.StaffModifyAll, controller.GetCheckPointForDelete(staff));
			AssertEquals(Env.Security.StaffModifyAll, controller.GetCheckPointForEdit(staff));
			AssertEquals(Env.Security.StaffModifyAll, controller.GetCheckPointForNew(staff));
			AssertEquals(Env.Security.StaffView, controller.GetCheckPointForView(staff));
		}

		public void TestPlugIn()
		{
			var controller = new StaffNumberRangesPlugInController();
			using (var tabControl = new ZTabControl())
			{
				var plugIns = new PlugIns(Factory.New<GlbStaff>(), tabControl);
				plugIns.Add(controller.ID);
				using (var plugIn = plugIns.GetPlugIn(controller.ID))
				{
					AssertType<GUI.StaffNumberRangesPlugIn>(plugIn);
				}
			}
		}
	}
}
