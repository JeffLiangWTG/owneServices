using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AdministrationPanelController))]
	sealed class AdministrationPanelControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AdministrationPanel;
		}

		#region Security checkpoints

		public void TestCheckPoints()
		{
			var controller = new AdministrationPanelControllerForTest();
			AssertEquals(Env.Security.MdmAdministrationPanel, controller.CheckPointForNewForTest);
		}

		class AdministrationPanelControllerForTest : AdministrationPanelController
		{
			public SecurityCheckpoint CheckPointForNewForTest => CheckPointForNew;
		}

		#endregion
	}
}
