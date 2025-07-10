using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OnboardingRequestDTOTest : TestCaseWithFactory
	{
		public void TestOnboardingRequestDTOAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Organization";
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Email = "1234567@163.com";
			Factory.Save();

			var boleroInvitationDetails = new BoleroInvitationDetails(org)
			{
				SelectedContactPK = orgContact.PK,
				YourMessage = "Welcome to the Bolero."
			};

			var onboardingRequest = new OnboardingRequestDTO(boleroInvitationDetails);

			AssertNotNull(onboardingRequest.InvitationDetailsDto);
			AssertNotNull(onboardingRequest.CompanyInformationDto);
			AssertNotNull(onboardingRequest.RequestMetaDataDto);
			Assert(!onboardingRequest.RequestMetaDataDto.OverrideFlag);
		}

		public void TestOnboardingRequestDTOJsonPropertyAliases()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Organization";
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Email = "1234567@163.com";
			Factory.Save();

			var boleroInvitationDetails = new BoleroInvitationDetails(org)
			{
				SelectedContactPK = orgContact.PK,
				YourMessage = "Welcome to the Bolero."
			};
			var onboardingRequest = new OnboardingRequestDTO(boleroInvitationDetails);

			var json = JsonConvert.SerializeObject(onboardingRequest);
			var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

			Assert(dict.ContainsKey("inviterDetailsDto"));
			Assert(dict.ContainsKey("companyInformationDto"));
			Assert(dict.ContainsKey("requestMetaDataDto"));

			var inviterDetailsJson = dict["inviterDetailsDto"].ToString();
			var inviterDetailsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(inviterDetailsJson);
			Assert(inviterDetailsDict.ContainsKey("inviterEnterpriseId"));
			Assert(inviterDetailsDict.ContainsKey("inviterServerId"));
			Assert(inviterDetailsDict.ContainsKey("inviterOrgCode"));
			Assert(inviterDetailsDict.ContainsKey("inviterOrgName"));
			Assert(inviterDetailsDict.ContainsKey("inviterFirstName"));
			Assert(inviterDetailsDict.ContainsKey("inviterLastName"));
			Assert(inviterDetailsDict.ContainsKey("inviterEmail"));
			Assert(inviterDetailsDict.ContainsKey("inviterMessage"));

			var companyInfoJson = dict["companyInformationDto"].ToString();
			var companyInfoDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(companyInfoJson);
			Assert(companyInfoDict.ContainsKey("firstName"));
			Assert(companyInfoDict.ContainsKey("lastName"));
			Assert(companyInfoDict.ContainsKey("email"));
			Assert(companyInfoDict.ContainsKey("legalCompanyName"));
			Assert(companyInfoDict.ContainsKey("addressLine1"));
			Assert(companyInfoDict.ContainsKey("addressLine2"));
			Assert(companyInfoDict.ContainsKey("city"));
			Assert(companyInfoDict.ContainsKey("postCode"));
			Assert(companyInfoDict.ContainsKey("state"));
			Assert(companyInfoDict.ContainsKey("countryCode"));
			Assert(companyInfoDict.ContainsKey("companyIdentifications"));
			Assert(companyInfoDict.ContainsKey("entityTypes"));
			Assert(companyInfoDict.ContainsKey("companyOrgCode"));

			var requestMetaDataJson = dict["requestMetaDataDto"].ToString();
			var requestMetaDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestMetaDataJson);
			Assert(requestMetaDataDict.ContainsKey("overrideFlag"));
		}

		public void TestOnboardingRequestDTONullArgumentChecks()
		{
			AssertArgumentExceptionThrown("boleroInvitationdetails", () => new OnboardingRequestDTO(null));
		}
	}
}
