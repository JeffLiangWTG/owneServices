using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class BoleroDTOValidatorTest : TestCaseWithFactory
	{
		public void TestDTOValidationWithMissingMandatoryFields()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsConsignee = true;
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = ZString.Empty;
			org.MainAddress.OA_RN_NKCountryCode = ZString.Empty;

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = ZString.Empty;
			registrationKey.ServerCodeForTest = ZString.Empty;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = ZString.Empty;
			var boleroInvitationDetails = new BoleroInvitationDetails(org)
			{
				SelectedContactPK = orgContact.PK,
			};

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				var onboardingRequestDTO = new OnboardingRequestDTO(boleroInvitationDetails);
				BoleroDTOValidator.ValidateBoleroDTOFields(onboardingRequestDTO, out var errorMessages);

				AssertCollectionContains("FirstName, Email, LegalCompanyName, AddressLine1, City, CountryCode, CompanyIdentifications, InviterEnterpriseId, InviterServerId, InviterFirstName, InviterEmail, InviterMessage are required to Enroll for Electronic Bills of Lading.", errorMessages);
			}
		}

		public void TestDTOValidationWithOrgIsNotConsigneeOrConsignor()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsConsignee = false;
			org.OH_IsConsignor = false;
			var orgContact = org.Contacts.AddNew();

			var boleroInvitationDetails = new BoleroInvitationDetails(org)
			{
				SelectedContactPK = orgContact.PK,
			};

			var onboardingRequestDTO = new OnboardingRequestDTO(boleroInvitationDetails);
			BoleroDTOValidator.ValidateBoleroDTOFields(onboardingRequestDTO, out var errorMessages);

			AssertCollectionContains("Please ensure the organization type is either consignee or consignor.", errorMessages);
		}
	}
}
