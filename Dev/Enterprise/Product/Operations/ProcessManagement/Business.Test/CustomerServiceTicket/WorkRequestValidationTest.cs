using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business.Test
{
	class WorkRequestValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSummary_ShouldBeMandatory()
		{
			var request = Factory.New<WorkRequest>();

			request.RunPreSaveValidation();

			AssertMandatoryValidationError(request.WKR_SummaryInfo, true);

			request.WKR_Summary = "Oooh wee!";
			AssertNoErrors(request.WKR_SummaryInfo);
		}

		public void TestOrganisationPK()
		{
			var workRequest = Factory.New<WorkRequest>();
			workRequest.OrganisationPK = ZGuid.NewZGuid();

			workRequest.Validation.ValidateOrganisationPK();
			AssertHasError("GIVEN organisation PK is not exist, WHEN validating THEN should error", workRequest.OrganisationPKInfo, "Enter a valid Organization.");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			workRequest.OrganisationPK = org.PK;

			workRequest.Validation.ValidateOrganisationPK();
			AssertNoErrors("GIVEN organisation PK exist, WHEN validating THEN should not error", workRequest.OrganisationPKInfo);
		}

		public void TestClient()
		{
			var workRequest = Factory.New<WorkRequest>();

			var contact1 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, organisationName: "Organisation 1", contactName: "Contact 1");
			var org1 = contact1.ParentOrg;

			workRequest.OrganisationPK = org1.PK;
			workRequest.WKR_OC_Client = contact1.PK;

			workRequest.Validation.ValidateWKR_OC_Client();
			AssertNoErrors("Precondition", workRequest.WKR_OC_ClientInfo);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			workRequest.OrganisationPK = org2.PK;

			CombineAssertions("GIVEN organisation PK is modified other than client's organisation, WHEN validating THEN should error", () =>
			{
				AssertNoErrors("Organisation", workRequest.OrganisationPKInfo);
				AssertHasError(
					"Client",
					workRequest.WKR_OC_ClientInfo,
					"The client's organization 'XVBQP68SIYXQ' is different from the selected organization 'H5ZX52PAMCOI'.");
			});

			workRequest.OrganisationPK = ZGuid.NewZGuid();
			CombineAssertions("GIVEN organisation PK does not exist, WHEN validating THEN should error", () =>
			{
				AssertHasError("Organisation", workRequest.OrganisationPKInfo, "Enter a valid Organization.");
				AssertHasError(
					"Client",
					workRequest.WKR_OC_ClientInfo,
					"Cannot select Client from invalid Organization.");
			});

			workRequest.OrganisationPK = ZGuid.Empty;
			CombineAssertions("GIVEN organisation PK is empty, WHEN validating THEN should not error", () =>
			{
				AssertNoErrors("Organisation", workRequest.OrganisationPKInfo);
				AssertNoErrors("Client", workRequest.WKR_OC_ClientInfo);
			});
		}

		public void TestClient_ShouldBeMandatoryAndValid()
		{
			var request = Factory.New<WorkRequest>();

			request.RunPreSaveValidation();

			AssertMandatoryValidationError(request.WKR_OC_ClientInfo, true);

			request.WKR_OC_Client = ZGuid.Invalid;
			AssertListValidationInvalidCodeError(request.WKR_OC_ClientInfo, true);

			var client = Factory.NewWithValidTestData<OrgContact>();
			client.OC_Email = "filler@test.pass";
			request.WKR_OC_Client = client.PK;

			AssertNoErrors(request.WKR_OC_ClientInfo);
		}

		public void TestClient_WhenContactHasInvalidEmailAddress_ShouldShowError()
		{
			var request = Factory.New<WorkRequest>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();
			request.WKR_OC_Client = contact.PK;

			contact.OC_Email = string.Empty;
			request.Validation.ValidateAll();
			AssertHasError(request.WKR_OC_ClientInfo, "The selected Client does not have an email address, so cannot take part in eConversation. Please choose a Client with an email address.");

			contact.OC_Email = "KEEP SUMMER SAFE";
			request.Validation.ValidateAll();
			AssertHasError(request.WKR_OC_ClientInfo, "The selected Client does not have an email address, so cannot take part in eConversation. Please choose a Client with an email address.");

			contact.OC_Email = "keep@summer.safe";
			request.Validation.ValidateAll();
			AssertNoNotifications(request.WKR_OC_ClientInfo);
		}

		public void TestSelectionCriteriaValidation_ShouldEnforceRegistryConfig()
		{
			ProcessMgmtTestHelper.SetDummySelectionCriteriaValues();

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			AssertValidationAppliesCorrectly(ticket.WKR_SelectionCriteria1Info, "ENT", "69");
			AssertValidationAppliesCorrectly(ticket.WKR_SelectionCriteria2Info, "PAV", "69");
			AssertValidationAppliesCorrectly(ticket.WKR_SelectionCriteria3Info, "BUF", "69");
			AssertValidationAppliesCorrectly(ticket.WKR_SelectionCriteria4Info, "PRD", "69");
			AssertValidationAppliesCorrectly(ticket.WKR_SelectionCriteria5Info, "ALP", "69");
		}

		// Note: this test is here instead of OrgAddressValidation because we can't access WorkRequest in MasterFiles yet without circular dependencies.
		public void TestErrorOnChangingEmailAddress_WhenViolatesJobConversation_ShouldListError()
		{
			var request = Factory.New<WorkRequest>();
			request.FillWithValidTestData();
			var org_conversing = Factory.NewWithValidTestData<OrgHeader>();
			var org_no_conversing = Factory.NewWithValidTestData<OrgHeader>();

			var first_contact = org_conversing.Contacts.AddNew();
			first_contact.FillWithValidTestData();
			first_contact.OC_ContactName = "John Smith";
			first_contact.OC_OH = org_conversing.PK;
			first_contact.OC_Email = "some@body.once";
			request.WKR_OC_Client = first_contact.PK;

			var second_boring_contact = org_no_conversing.Contacts.AddNew();
			second_boring_contact = Factory.NewWithValidTestData<OrgContact>();
			second_boring_contact.OC_ContactName = "Sohn Jmith";
			second_boring_contact.OC_OH = org_no_conversing.PK;
			second_boring_contact.OC_Email = "body@some.once";

			Factory.Save();

			AssertEquals("PRE: should have added our first_contact to the annals of history and yet...", first_contact, request.Client);

			Factory.Save();

			first_contact.Validation.ValidateOC_Email();
			AssertNoNotifications("We should have no warnings OR errors yet, but instead...", first_contact.OC_EmailInfo);

			first_contact.OC_Email = "";
			first_contact.Validation.ValidateOC_Email();
			AssertHasError("We should be cheesed this is occuring, but yet...", first_contact.OC_EmailInfo, "Setting an empty email on this client will prevent them from receiving eConversation notifications for Customer Service Tickets on which they are selected as the Client.");

			second_boring_contact.Validation.ValidateOC_Email();
			AssertNoNotifications("We should have no warnings OR errors yet, but instead...", second_boring_contact.OC_EmailInfo);

			second_boring_contact.OC_Email = "";
			second_boring_contact.Validation.ValidateOC_Email();
			AssertNoNotifications("We should have no warnings OR errors yet, but instead...", second_boring_contact.OC_EmailInfo);
		}

		static void AssertValidationAppliesCorrectly(ZPropertyInfo propertyInfo, string validValue, string invalidValue)
		{
			propertyInfo.Value = (ZString)validValue;
			AssertNoErrors(propertyInfo);

			propertyInfo.Value = (ZString)invalidValue;
			AssertListValidationInvalidCodeError(propertyInfo, isExpectingError: true);
		}

		public void TestCountryValidation_ShouldValidateCountries_ShouldUseRefCountryTable()
		{
			var countryCountry = Factory.New<RefCountry>();
			countryCountry.FillWithValidTestData();
			countryCountry.Code = "FU";

			var request = Factory.New<WorkRequest>();
			request.FillWithValidTestData();

			request.WKR_RN_NKCountry = "";
			request.Validation.ValidateWKR_RN_NKCountry();

			AssertNoNotifications(request.WKR_RN_NKCountryInfo);

			request.WKR_RN_NKCountry = "FU";
			request.Validation.ValidateWKR_RN_NKCountry();

			AssertNoNotifications(request.WKR_RN_NKCountryInfo);

			var yyCountries = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "YY"));

			AssertEquals("Yy Country doesn't exist", 0, yyCountries.Length);

			request.WKR_RN_NKCountry = "YY";
			request.Validation.ValidateWKR_RN_NKCountry();

			AssertHasError("Ye Country doesn't exist, so it isn't a valid workRequest country", request.WKR_RN_NKCountryInfo, "The country code 'YY' does not represent a valid Country. Please choose a valid Country, or create a new Country in the Maintain -> Locations -> Countries Module");
		}

		public void TestCountryValidation_ShouldValidateInvalidLetterCodes()
		{
			var request = Factory.New<WorkRequest>();
			request.FillWithValidTestData();

			request.WKR_RN_NKCountry = "r";
			request.Validation.ValidateAll();

			AssertHasError("1-letter countries are never good.", request.WKR_RN_NKCountryInfo, "The country code 'R' does not represent a valid Country. Please choose a valid Country, or create a new Country in the Maintain -> Locations -> Countries Module");
		}
	}
}
