using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class InviterDetailsDTOTest : TestCaseWithFactory
	{
		public void TestInvitationDetailsDTOConstructor()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUserPK);
			currentUser.GS_FullName = "Huang Fei Hong";
			currentUser.GS_EmailAddress = "123456@163.com";

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_FullName = "Test Organization Proxy";
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = orgProxy.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Email = "1234567@163.com";
			var boleroInvitationDetails = new BoleroInvitationDetails(org)
			{
				YourMessage = "Welcome to the Bolero."
			};

			boleroInvitationDetails.SelectedContactPK = orgContact.PK;
			Factory.Save();

			var invitationDetailsDTO = new InviterDetailsDTO(boleroInvitationDetails);

			AssertEquals("ENT", invitationDetailsDTO.InviterEnterpriseId);
			AssertEquals("SVR", invitationDetailsDTO.InviterServerId);
			AssertEquals(orgProxy.PK, invitationDetailsDTO.InviterOrgCode);
			AssertEquals("Test Organization Proxy", invitationDetailsDTO.InviterOrgName);
			AssertEquals("Huang Fei", invitationDetailsDTO.InviterFirstName);
			AssertEquals("Hong", invitationDetailsDTO.InviterLastName);
			AssertEquals("123456@163.com", invitationDetailsDTO.InviterEmail);
			AssertEquals("Welcome to the Bolero.", invitationDetailsDTO.InviterMessage);

			currentUser.GS_FullName = "  Huang   Fei  Hong  ";
			Factory.Save();
			invitationDetailsDTO = new InviterDetailsDTO(boleroInvitationDetails);
			AssertEquals("Even if login name has spaces, the first name should not contain spaces", "Huang Fei", invitationDetailsDTO.InviterFirstName);
			AssertEquals("Even if login name has spaces, the last name should not contain spaces", "Hong", invitationDetailsDTO.InviterLastName);

			currentUser.GS_FullName = "Pony";
			Factory.Save();
			invitationDetailsDTO = new InviterDetailsDTO(boleroInvitationDetails);
			AssertEquals("Pony", invitationDetailsDTO.InviterFirstName);
			AssertNullOrEmpty(invitationDetailsDTO.InviterLastName);
		}

		public void TestInvitationDetailsDTOFallbackProxyOrg()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			var fallbackOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			fallbackOrgProxy.OH_FullName = "Fallback Organization Proxy";
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = fallbackOrgProxy.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Email = "1234567@163.com";
			Factory.Save();

			var boleroInvitationDetails = new BoleroInvitationDetails(org)
			{
				YourMessage = "Welcome to the Bolero."
			};

			boleroInvitationDetails.SelectedContactPK = orgContact.PK;
			var invitationDetailsDTO = new InviterDetailsDTO(boleroInvitationDetails);
			AssertEquals(fallbackOrgProxy.PK, invitationDetailsDTO.InviterOrgCode);
			AssertEquals("Fallback Organization Proxy", invitationDetailsDTO.InviterOrgName);
		}
	}
}
