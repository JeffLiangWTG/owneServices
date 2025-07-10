using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	class CustomerServiceTicketDataObjectReaderTest : ActivityDataObjectReaderTestCase<CustomerServiceTicketDataObjectReader, WorkRequest>
	{
		protected override void AssertDescription(WorkRequest businessObject)
		{
			AssertEquals("Lonely Gal Margarita Mix for One", businessObject.WKR_Description);
		}

		protected override void AssertBranch(WorkRequest businessObject)
		{
			AssertEquals("BRA", businessObject.Branch.GB_Code);
		}

		public void TestPopulateDataObject_BranchNameNotProvided()
		{
			var currentBranch = GlbBranch.CurrentBranch;
			var activity = CreateActivityWithContact();
			activity.Branch = new Branch { Code = currentBranch.GB_Code };
			WorkRequest ticket = null;

			AssertNoExceptionThrown("Providing XML with a company that only has a Code specified shouldn't throw an exception. SAD!", () => ticket = ReadBusinessObject(activity));
			AssertEquals(currentBranch.GB_BranchName, ticket.Branch.GB_BranchName);
		}

		protected override void AssertDepartment(WorkRequest businessObject)
		{
			AssertEquals("DEP", businessObject.Department.GE_Code);
		}

		public void TestPopulateDataObject_DepartmentNameNotProvided()
		{
			var currentDepartment = GlbDepartment.CurrentDepartment;
			var activity = CreateActivityWithContact();
			activity.Department = new Department { Code = currentDepartment.GE_Code };
			WorkRequest ticket = null;

			AssertNoExceptionThrown("Providing XML with a company that only has a Code specified shouldn't throw an exception. SAD!", () => ticket = ReadBusinessObject(activity));
			AssertEquals(currentDepartment.GE_Desc, ticket.Department.GE_Desc);
		}

		protected override void AssertCompany(WorkRequest businessObject)
		{
			Assert("Field is not used for Customer Service Tickets.", true);
		}

		protected override void AssertClient1(WorkRequest businessObject)
		{
			AssertEquals("Jan Michael Vincent", businessObject.Client.OC_ContactName);
		}

		protected override void AssertClient2(WorkRequest businessObject)
		{
			Assert("Field is not used for Customer Service Tickets.", true);
		}

		protected override void AssertCountry(WorkRequest businessObject)
		{
			AssertEquals("LB", businessObject.WKR_RN_NKCountry);
		}

		protected override void AssertProjectManager(WorkRequest businessObject)
		{
			Assert("Field is not used for Customer Service Tickets.", true);
		}

		protected override IEnumerable<BusinessObject> GetRelatedItemsForTestsCore(BusinessObjectFactory factory, OrgContact contact)
		{
			ProcessMgmtTestHelper.CreateWorkItem(factory, "This one shant be related."); // just to prove that it doesn't find all objects of the given type in the db
			yield return ProcessMgmtTestHelper.CreateWorkItem(factory, "Yeah, I'd like to order one large sofa chair with extra chair please.");
			yield return ProcessMgmtTestHelper.CreateWorkItem(factory, "High chair. No, no, no, no. Recliner. And wheelchair on half.");
		}

		protected override IEnumerable<string> ExpectedRelatedItemDescriptions => new[] { "Yeah, I'd like to order one large sofa chair with extra chair please.", "High chair. No, no, no, no. Recliner. And wheelchair on half." };
		protected override string ExpectedValidRelatedItemTypes => "[Work Item]";

		public void TestPopulateDataObject_NoBranchElement()
		{
			var activity = CreateActivityWithContact();
			var ticket = ReadBusinessObject(activity);

			AssertEquals("No branch was specified, which is fine, so this field should simply be empty. SAD!", ZGuid.Empty, ticket.WKR_GB_Branch);
		}

		public void TestPopulateDataObject_NoDepartmentElement()
		{
			var activity = CreateActivityWithContact();
			var ticket = ReadBusinessObject(activity);

			AssertEquals("No department was specified, which is fine, so this field should simply be empty. SAD!", ZGuid.Empty, ticket.WKR_GE_Department);
		}

		public void TestPopulateDataObject_NoCountryElement()
		{
			var activity = CreateActivityWithContact();
			var ticket = ReadBusinessObject(activity);

			AssertEquals("No country was specified, which is fine, so this field should simply be empty. SAD!", ZString.Empty, ticket.WKR_RN_NKCountry);
		}

		public void TestPopulateContact_WhenOrganizationHasMultipleContacts_ShouldSelectCorrectContact()
		{
			var contact = CreateContact();

			var otherContactInSameOrg = contact.Header.Contacts.AddNew();
			otherContactInSameOrg.OC_ContactName = "Art Vandelay";
			otherContactInSameOrg.OC_Email = "art@vandelay.com";

			BizoFactory.Save();

			var activity = CreateActivity(contact);
			var ticket = ReadBusinessObject(activity);

			AssertEquals("The correct contact at the correct organization should have been matched. SAD!", contact.PK, ticket.WKR_OC_Client);
		}

		public void TestPopulateContact_WhenSameContactNameExistsInMultipleOrganizations_ShouldSelectCorrectContact()
		{
			var contact = CreateContact();

			var contactWithSameNameInOtherOrg = ProcessMgmtTestHelper.CreateOrganizationAndContact(BizoFactory);
			var otherAddress = contactWithSameNameInOtherOrg.Header.MainAddress;
			otherAddress.Address1 = "123 Canada Street";
			otherAddress.Address2 = "Unit 127"; // this, plus the org name, is the only difference in address
			otherAddress.City = "Canada City";
			otherAddress.State = "Nova Scotia";
			otherAddress.Postcode = "B0V 1A0";
			otherAddress.OA_RN_NKCountryCode = "CA";
			otherAddress.OA_Email = "someoneelse@email.com";
			otherAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			BizoFactory.Save();

			var activity = CreateActivity(contact);
			var ticket = ReadBusinessObject(activity);

			AssertEquals("The correct contact at the correct organization should have been matched. SAD!", contact.PK, ticket.WKR_OC_Client);
		}

		public void TestPopulateContact_WhenNoMatchingOrgFound_ShouldLogError()
		{
			var activity = CreateActivityWithContact();
			var client = activity.OrganizationAddressCollection.Single();
			client.Address1 = "Some other street";
			client.OrganizationCode = "SQUANCH";

			var logger = new TestErrorLogger();
			var reader = GetReader(activity, logger);

			AssertExceptionThrown<DataObjectReadFailureException>("An exception should have been thrown because this object can't be saved. SAD!",
				"Could not find an organization matching the details provided. Because Client is a required field, this item cannot be imported.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateContact_WhenOrgFoundButNoMatchingContact_ShouldLogError()
		{
			var activity = CreateActivityWithContact();
			var client = activity.OrganizationAddressCollection.Single();
			client.Contact = "Bird Person";

			var logger = new TestErrorLogger();
			var reader = GetReader(activity, logger);

			AssertExceptionThrown<DataObjectReadFailureException>("An exception should have been thrown because this object can't be saved. SAD!",
				"Could not find a contact with the name 'Bird Person' in the matched organization 'Vandelay Industries'. Because Client is a required field, this item cannot be imported.",
				() => reader.ReadIntoBusinessObject());
		}

		protected override ZPropertyInfo GetStatusPropertyInfo(WorkRequest businessObject)
		{
			return businessObject.WKR_StatusInfo;
		}

		protected override ZString GetCreateUserCode(WorkRequest ticket)
		{
			return ticket.WKR_SystemCreateUser;
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.CustomerServiceTicket;
	}
}
