using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SalesEnquiryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLocations()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			AssertEquals(false, enquiry.Lookups.Locations.AllowZones);
		}

		public void TestGetAllEnquiryTypes()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("CCR", (NoResString)"ignored");
			list.Add("AAA", (NoResString)"A desc");
			list.Add("BBB", (NoResString)"B desc");
			OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Should contain the new list", 3, SalesEnquiryLookups.GetAllEnquiryTypes().Count);
		}

		public void TestGetAllLeadInterests()
		{
			AssertNotNull("Lead Interests list should not be null", SalesEnquiryLookups.GetAllLeadInterests());
		}

		public void TestContacts()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			var contact2 = org1.Contacts.AddNew();
			var contact3 = org2.Contacts.AddNew();
			var contact4 = org2.Contacts.AddNew();
			contact1.OC_ContactName = "One";
			contact2.OC_ContactName = "Two";
			contact3.OC_ContactName = "Three";
			contact4.OC_ContactName = "Four";
			contact2.OC_IsActive = false;
			contact4.OC_IsActive = false;
			Factory.Save();

			var enquiry = Factory.New<SalesEnquiry>();
			AssertEquals("Precondition: Org is not set", ZGuid.Empty, enquiry.OrgPk);
			AssertEquals("Should have no contacts initially", 0, enquiry.Lookups.Contacts.Count);

			enquiry.OrgPk = org1.PK;
			AssertEquals("Should only have one contact when org is set", 1, enquiry.Lookups.Contacts.Count);
			AssertEquals("Contact should be 'One'", "One", enquiry.Lookups.Contacts[0].OC_ContactName);

			enquiry.Lookups.Contacts.Load();
			AssertEquals("Should only have one contact on load", 1, enquiry.Lookups.Contacts.Count);
			AssertEquals("Contact should be 'One'", "One", enquiry.Lookups.Contacts[0].OC_ContactName);

			enquiry.OrgPk = org2.PK;
			AssertEquals("Should only have one contact on org change", 1, enquiry.Lookups.Contacts.Count);
			AssertEquals("Contact should be 'Three'", "Three", enquiry.Lookups.Contacts[0].OC_ContactName);
		}

		public void TestContactsOfReferringOrg()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			var contact2 = org1.Contacts.AddNew();
			var contact3 = org2.Contacts.AddNew();
			var contact4 = org2.Contacts.AddNew();
			contact1.OC_ContactName = "One";
			contact2.OC_ContactName = "Two";
			contact3.OC_ContactName = "Three";
			contact4.OC_ContactName = "Four";
			contact1.OC_IsActive = false;
			contact3.OC_IsActive = false;
			Factory.Save();

			var enquiry = Factory.New<SalesEnquiry>();
			AssertEquals("Precondition: Referring Org is not set", ZGuid.Empty, enquiry.O1_OH_SourceOfLead);
			AssertEquals("Should have no contacts initially", 0, enquiry.Lookups.ContactsOfReferringOrg.Count);

			enquiry.O1_OH_SourceOfLead = org1.PK;
			AssertEquals("Should only have one referring contact when org is set", 1, enquiry.Lookups.ContactsOfReferringOrg.Count);
			AssertEquals("Referring contact should be 'Two'", "Two", enquiry.Lookups.ContactsOfReferringOrg[0].OC_ContactName);

			enquiry.Lookups.ContactsOfReferringOrg.Load();
			AssertEquals("Should only have one referring contact on load", 1, enquiry.Lookups.ContactsOfReferringOrg.Count);
			AssertEquals("Referring contact should be 'Two'", "Two", enquiry.Lookups.ContactsOfReferringOrg[0].OC_ContactName);

			enquiry.O1_OH_SourceOfLead = org2.PK;
			AssertEquals("Should only have one referring contact on org change", 1, enquiry.Lookups.ContactsOfReferringOrg.Count);
			AssertEquals("Referring contact should be 'Four'", "Four", enquiry.Lookups.ContactsOfReferringOrg[0].OC_ContactName);
		}

		public void TestContactsOfReferToOrg()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			var contact2 = org1.Contacts.AddNew();
			var contact3 = org2.Contacts.AddNew();
			var contact4 = org2.Contacts.AddNew();
			contact1.OC_ContactName = "One";
			contact2.OC_ContactName = "Two";
			contact3.OC_ContactName = "Three";
			contact4.OC_ContactName = "Four";
			contact1.OC_IsActive = false;
			contact3.OC_IsActive = false;
			Factory.Save();

			var enquiry = Factory.New<SalesEnquiry>();
			AssertEquals("Precondition: Refer To Org is not set", ZGuid.Empty, enquiry.O1_OH_ReferTo);
			AssertEquals("Should have no contacts initially", 0, enquiry.Lookups.ContactsOfReferToOrg.Count);

			enquiry.O1_OH_ReferTo = org1.PK;
			AssertEquals("Should only have one refer to contact when org is set", 1, enquiry.Lookups.ContactsOfReferToOrg.Count);
			AssertEquals("Refer to contact should be 'Two'", "Two", enquiry.Lookups.ContactsOfReferToOrg[0].OC_ContactName);

			enquiry.Lookups.ContactsOfReferToOrg.Load();
			AssertEquals("Should only have one refer to contact on load", 1, enquiry.Lookups.ContactsOfReferToOrg.Count);
			AssertEquals("Refer to contact should be 'Two'", "Two", enquiry.Lookups.ContactsOfReferToOrg[0].OC_ContactName);

			enquiry.O1_OH_ReferTo = org2.PK;
			AssertEquals("Should only have one refer to contact on org change", 1, enquiry.Lookups.ContactsOfReferToOrg.Count);
			AssertEquals("Refer to contact should be 'Four'", "Four", enquiry.Lookups.ContactsOfReferToOrg[0].OC_ContactName);
		}

		public void TestJobCategories()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			AssertNotNull("Active job Categories not null", enquiry.Lookups.JobCategory_List);
		}

		public void TestLeadInterests_OrderbyDescription()
		{
			var registryValue = new CodeDescriptionBoolCollection()
			{
				{ "CD1", (NoResString)"Des 2" },
				{ "CD2", (NoResString)"Des 3" },
				{ "CD3", (NoResString)"Des 1" },
			};

			using (OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var enquiry = Factory.New<SalesEnquiry>();
				AssertNotNull("Lead Interests list not null", enquiry.Lookups.LeadInterest_List);
				AssertNotNull("Active lead interests list not null", enquiry.Lookups.LeadInterest_ActiveList);

				var interests = enquiry.Lookups.LeadInterest_ActiveList;

				AssertEquals("sources has 3 items", 3, interests.Count);
				AssertContainsExactElementsInExactOrder("ActiveSources is ordered by Description", new string[] { "CD3", "CD1", "CD2" }, interests.Cast<CodeDescriptionPair>().Select(s => s.Code));
			}
		}

		public void TestSources_OrderbyDescription()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			AssertNotNull("Source list not null", enquiry.Lookups.Source_List);
			AssertNotNull("Active source list not null", enquiry.Lookups.Source_ActiveList);

			var registryValue = new CodeDescriptionBoolRelatedItemCollection()
			{
				{ "CD1", (NoResString)"Des 2", true },
				{ "CD2", (NoResString)"Des 3", true },
				{ "CD3", (NoResString)"Des 1", true },
			};

			using (OrganisationsDataRegistry.Instance.OpportunitySource.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
			{
				var sources = enquiry.Lookups.Source_ActiveList;

				AssertEquals("sources has 3 items", 3, sources.Count);
				AssertContainsExactElementsInExactOrder("ActiveSources is ordered by Description", new string[] { "CD3", "CD1", "CD2" }, sources.Cast<CodeDescriptionPair>().Select(s => s.Code));
			}
		}

		public void TestEnquiryTypeList()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			var expected = new CodeDescriptionPairList();
			expected.AddRange(OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.Value);
			AssertArrayEqualsByElements(expected.ToArray(), enquiry.Lookups.AllEnquiryTypes.ToArray());

			var reg = new CodeDescriptionBoolCollection();
			reg.Add("FOO", (NoResString)"FOO");
			reg.Add("CCR", (NoResString)"CCR");
			reg.Add(SalesEnquiry.Codes.SalesEnquiry, (NoResString)"INQ");
			reg.Add("BAR", (NoResString)"BAR");
			OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);
			expected = new CodeDescriptionPairList();
			expected.AddPair("FOO", "FOO");
			expected.AddPair("CCR", "CCR");
			expected.AddPair(SalesEnquiry.Codes.SalesEnquiry, "INQ");
			expected.AddPair("BAR", "BAR");
			AssertArrayEqualsByElements(expected.ToArray(), enquiry.Lookups.AllEnquiryTypes.ToArray());
		}

		public void TestEnquiryTypeActiveList_OrderbyDescription()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			var list = new CodeDescriptionBoolCollection();
			list.Add("AAA", (NoResString)"A desc");
			list.Add("BBB", (NoResString)"B desc", false);
			list.Add("CCC", (NoResString)"000 C desc");
			OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var expectedAll = new CodeDescriptionPairList();
			expectedAll.AddPair("AAA", "A desc");
			expectedAll.AddPair("BBB", "B desc");
			expectedAll.AddPair("CCC", "000 C desc");

			var expectedActive = new CodeDescriptionPairList();
			expectedActive.AddPair("CCC", "000 C desc");
			expectedActive.AddPair("AAA", "A desc");

			AssertArrayEqualsByElements(expectedAll.ToArray(), enquiry.Lookups.AllEnquiryTypes.ToArray());
			AssertArrayEqualsByElements(expectedActive.ToArray(), enquiry.Lookups.ActiveEnquiryTypes.ToArray());
		}
	}
}
