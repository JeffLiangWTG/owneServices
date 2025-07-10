using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USOrganisationDetailsPlugInController))]
	sealed class USOrganisationDetailsPlugInControllerTest : ZControllerBasherTest
	{
		public void TestPlugIn()
		{
			var controller = new USOrganisationDetailsPlugInController();
			using (var plugIn = controller.GetPlugInInternal(Factory.New<OrgHeader>()))
			{
				AssertEquals(typeof(GUI.OrganisationDetailsPlugIn), plugIn.GetType());
			}
		}

		public void TestCheckPoints()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var controller = new USOrganisationDetailsPlugInController();
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForDelete(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForEdit(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForNew(orgHeader));
			AssertEquals(Env.Security.OrgDetailsViewCountryDefaults, controller.GetCheckPointForView(orgHeader));
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.OrganisationDetailsPlugIn;
	}
}
