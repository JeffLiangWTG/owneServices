using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ClientIntelligenceController))]
	sealed class ClientIntelligenceControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			Env.Security.OrganisationDelete.IsAllowed = true;
			Env.Security.ClientIntelligenceModify.IsAllowed = true;
			Env.Security.ClientIntelligenceView.IsAllowed = true;
			Env.Security.OrganisationCRMSecurity.DisableCRMSecurityForTesting(true);

			var clientIntelligence = GetBusinessObjectThatIsInTheDatabase();
			ClientIntelligenceController controller = new ClientIntelligenceController();
			AssertEquals("For Delete, same security as org", Env.Security.OrganisationDelete, controller.GetCheckPointForDelete(clientIntelligence));
			AssertEquals("For Edit", Env.Security.ClientIntelligenceModify, controller.GetCheckPointForEdit(clientIntelligence));
			AssertEquals("For New", Env.Security.ClientIntelligenceModify, controller.GetCheckPointForNew(clientIntelligence));
			AssertEquals("For View", Env.Security.ClientIntelligenceView, controller.GetCheckPointForView(clientIntelligence));
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader org = factory.New<OrgHeader>();
			org.OH_FullName = "Name";
			org.MainAddress.OA_Address1 = "Address";
			org.OH_RL_NKClosestPort = "USLAX";
			org.OH_IsSalesLead = true;
			factory.Save();

			return org;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ClientIntelligence;
		}
	}
}
