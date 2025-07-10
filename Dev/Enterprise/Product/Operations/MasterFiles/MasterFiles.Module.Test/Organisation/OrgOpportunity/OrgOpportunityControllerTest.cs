using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgOpportunityController))]
	sealed class OrgOpportunityControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			Factory.Save();

			return opportunity;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Opportunity;
		}

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<OrgOpportunity>();
			bizObjWithoutAccess.P8_GS_NKPrimarySalesPerson = "U00";
			bizObjWithoutAccess.Header.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<OrgOpportunity>.AssertController(new OrgOpportunityController(), bizObjWithoutAccess, Env.Security.OpportunityManagementCRMSecurity);
		}

		#endregion

		public void TestUrlsCanBeOpenedByAnyCompany()
		{
			Assert("Opportunity hyperlinks should not be restricted to the current company", !Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
