using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(TWOrganisationDetailsController))]
	sealed class TWOrganisationDetailsControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.TW.OrganisationDetailsPlugIn;
		}

		public void TestCheckPoints()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var controller = new TWOrganisationDetailsController();
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForDelete(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForEdit(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForNew(orgHeader));
			AssertEquals(Env.Security.OrgDetailsViewCountryDefaults, controller.GetCheckPointForView(orgHeader));
		}
	}
}
