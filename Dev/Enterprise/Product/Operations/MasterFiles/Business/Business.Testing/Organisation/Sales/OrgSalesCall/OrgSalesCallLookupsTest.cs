using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSalesCallLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrgActiveContactsPlusCurrentContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var activeContact = org.Contacts.AddNew();
			activeContact.OC_IsActive = true;
			activeContact.OC_ContactName = "Active";
			var inactiveContact = org.Contacts.AddNew();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_ContactName = "Inactive";

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_OH = org.PK;
			var lookups = new OrgSalesCallLookups(communication);

			AssertContainsExactElementsInAnyOrder(new[] { activeContact }, lookups.OrgActiveContactsPlusExistingContact);

			communication.OQ_OC = activeContact.PK;
			AssertContainsExactElementsInAnyOrder(new[] { activeContact }, lookups.OrgActiveContactsPlusExistingContact);

			communication.OQ_OC = inactiveContact.PK;
			AssertContainsExactElementsInAnyOrder(new[] { activeContact }, lookups.OrgActiveContactsPlusExistingContact);

			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { activeContact, inactiveContact }, lookups.OrgActiveContactsPlusExistingContact);

			communication.OQ_OC = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder(new[] { activeContact }, lookups.OrgActiveContactsPlusExistingContact);
		}

		public void TestOrgActiveContactsPlusCurrentContact_Sorted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contactA = org.Contacts.AddNew();
			contactA.OC_IsActive = true;
			contactA.OC_ContactName = "Contact A";
			var contactZ = org.Contacts.AddNew();
			contactZ.OC_IsActive = true;
			contactZ.OC_ContactName = "Contact Z";
			var contactB = org.Contacts.AddNew();
			contactB.OC_IsActive = true;
			contactB.OC_ContactName = "Contact B";
			var contactInactive = org.Contacts.AddNew();
			contactInactive.OC_IsActive = false;
			contactInactive.OC_ContactName = "Contact Inactive";

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_OH = org.PK;
			communication.OQ_OC = contactInactive.PK;
			Factory.Save();

			var lookups = new OrgSalesCallLookups(communication);

			AssertArrayEqualsByElements(new[] { "Contact A", "Contact B", "Contact Inactive", "Contact Z" }, lookups.OrgActiveContactsPlusExistingContact.Cast<OrgContact>().Select(contact => contact.OC_ContactName.ToString()).ToArray());
		}

		public void TestLocationList()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address0 = org1.Addresses[0];
			address0.OA_Address1 = "Default Address";
			var address1 = org1.Addresses.AddNew();
			address1.OA_Address1 = "Address 1";
			var address2 = org1.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";
			var address3 = org1.Addresses.AddNew();
			address3.OA_Address1 = "Address 2";
			var address4 = org1.Addresses.AddNew();
			address4.OA_Address1 = "Address 2";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address5 = org2.Addresses.AddNew();
			address5.OA_Address1 = "Address 3";

			var meetingRoom1 = Factory.New<GlbStaff>();
			meetingRoom1.GS_IsResource = true;
			meetingRoom1.GS_Code = "$R1";
			meetingRoom1.GS_ResourceType = "ROM";
			meetingRoom1.GS_FullName = "Meeting Room 1";

			var meetingRoom2 = Factory.New<GlbStaff>();
			meetingRoom2.GS_IsResource = true;
			meetingRoom2.GS_Code = "$R2";
			meetingRoom2.GS_ResourceType = "ROM";
			meetingRoom2.GS_FullName = "Meeting Room 2";

			var resource1 = Factory.New<GlbStaff>();
			resource1.GS_IsResource = true;
			resource1.GS_Code = "$AA";
			resource1.GS_ResourceType = "PRJ";
			resource1.GS_FullName = "Projector";

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "SCW";
			staff1.GS_FullName = "Sam";

			var salesCall = org1.SalesCalls.AddNew();
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"DEFAULT ADDRESS",
					"ADDRESS 1",
					"ADDRESS 2",
					"ADDRESS 2 (1)",
					"ADDRESS 2 (2)",
					"Meeting Room 1",
					"Meeting Room 2"
				},
				salesCall.Lookups.LocationList.Cast<CodeDescriptionPair>().Select(pair => pair.Code));

			salesCall.OQ_OH = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"Meeting Room 1",
					"Meeting Room 2"
				},
				salesCall.Lookups.LocationList.Cast<CodeDescriptionPair>().Select(pair => pair.Code));
		}

		public void TestActiveLocationList()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address0 = org1.Addresses[0];
			address0.OA_Address1 = "Default Address";
			var address1 = org1.Addresses.AddNew();
			address1.OA_Address1 = "Address 1";
			var address2 = org1.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";
			var address3 = org1.Addresses.AddNew();
			address3.OA_Address1 = "Address 2";
			var address4 = org1.Addresses.AddNew();
			address4.OA_Address1 = "Address 2";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address5 = org2.Addresses.AddNew();
			address5.OA_Address1 = "Address 3";

			var salesCall = org1.SalesCalls.AddNew();
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"DEFAULT ADDRESS",
					"ADDRESS 1",
					"ADDRESS 2",
					"ADDRESS 2 (1)",
					"ADDRESS 2 (2)"
				},
				salesCall.Lookups.LocationList.Cast<CodeDescriptionPair>().Select(pair => pair.Code));

			address1.OA_IsActive = false;
			AssertCollectionNotContains("ADDRESS 1", salesCall.Lookups.ActiveLocationList.Cast<CodeDescriptionPair>().Select(pair => pair.Code));
		}

		public void TestOQ_Status_List()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			Assert("OQ_Status_List.Count > 0", salesCall.Lookups.OQ_Status_List.Count > 0);
		}

		public void TestOQ_Category_List()
		{
			var categoryList = Factory.New<OrgSalesCall>();
			Assert("OQ_Status_List.Count > 0", categoryList.Lookups.OQ_Category_List.Count > 0);
		}

		public void TestOQ_TypeOfCall_List()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			Assert("OQ_TypeOfCall_List.Count > 0", salesCall.Lookups.OQ_TypeOfCall_List.Count > 0);
		}
	}
}
