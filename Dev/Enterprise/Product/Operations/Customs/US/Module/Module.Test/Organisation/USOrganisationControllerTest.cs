using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USOrganisationController))]
	sealed class USOrganisationControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugIn()
		{
			var controller = new USOrganisationController();
			using (var plugIn = controller.GetPlugInInternal(Factory.New<OrgHeader>()))
			{
				AssertEquals(typeof(GUI.OrganisationPlugIn), plugIn.GetType());
			}
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new USOrganisationController();
			AssertEquals(controller.CheckPointForDeleteExposedForTest, Env.Security.QueryMessages);
			AssertEquals(controller.CheckPointForEditExposedForTest, Env.Security.QueryMessages);
			AssertEquals(controller.CheckPointForNewExposedForTest, Env.Security.QueryMessages);
			AssertEquals(controller.CheckPointForViewExposedForTest, Env.Security.QueryMessagesView);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.OrganisationCustomsMessaging;
	}
}
