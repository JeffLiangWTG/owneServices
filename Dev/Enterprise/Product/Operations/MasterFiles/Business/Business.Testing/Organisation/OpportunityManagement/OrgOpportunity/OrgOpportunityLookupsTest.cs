using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgOpportunityLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAssignedOfficeContacts()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Zubin";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "ABC";

			var opp = Factory.NewWithValidTestData<OrgOpportunity>();

			AssertEquals("no org or assigned office, so no contacts", 0, opp.Lookups.AssignedOfficeContacts.Count);

			opp.AssignedOrgPK = org1.PK;
			Factory.Save();
			opp.Lookups.AssignedOfficeContacts.Load();

			AssertEquals("1 contact", 1, opp.Lookups.AssignedOfficeContacts.Count);

			opp.P8_OA_AssignedOffice = org1.Addresses[0].PK;
			AssertEquals("1 contact - Zubin because assigned office has now been chosen", "Zubin", opp.Lookups.AssignedOfficeContacts[0].OC_ContactName);

			opp.AssignedOrgPK = org2.PK;
			opp.P8_OA_AssignedOffice = org2.Addresses[0].PK;
			Factory.Save();
			opp.Lookups.AssignedOfficeContacts.Load();

			AssertEquals("1 contact - ABC", "ABC", opp.Lookups.AssignedOfficeContacts[0].OC_ContactName);

			opp.AssignedOrgPK = ZGuid.Empty;
			Factory.Save();
			opp.Lookups.AssignedOfficeContacts.Load();

			AssertEquals("no org, so no contacts", 0, opp.Lookups.AssignedOfficeContacts.Count);
		}

		public void TestContacts()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Adam One";

			var opportunity = org.SalesOpportunities.AddNew();
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new List<OrgContact> { contact }, opportunity.Lookups.ActiveContacts);

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Adam Two";
			Factory.Save();

			opportunity.P8_OC = contact2.PK;
			AssertContainsExactElementsInAnyOrder(new List<OrgContact> { contact, contact2 }, opportunity.Lookups.ActiveContacts);
		}

		public void TestInactiveContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			var activeContact1 = org.Contacts.AddNew();
			activeContact1.OC_ContactName = "Active Contact 1";
			activeContact1.OC_IsActive = true;
			Factory.Save();

			AssertEquals("Should return 1 active contact.", 1, opportunity.Lookups.ActiveContacts.Count);

			var inactiveContact1 = org.Contacts.AddNew();
			inactiveContact1.OC_ContactName = "Inactive Contact 1";
			inactiveContact1.OC_IsActive = false;
			Factory.Save();

			AssertEquals("Should return 1 active contact.", 1, opportunity.Lookups.ActiveContacts.Count);

			var activeContact2 = org.Contacts.AddNew();
			activeContact2.OC_ContactName = "Active Contact 2";
			activeContact2.OC_IsActive = true;

			var inactiveContact2 = org.Contacts.AddNew();
			inactiveContact2.OC_ContactName = "Inactive Contact 2";
			inactiveContact2.OC_IsActive = false;
			Factory.Save();

			AssertEquals("Should return 2 active contacts.", 2, opportunity.Lookups.ActiveContacts.Count);
		}

		public void TestInactiveContactSavedOnOpportunity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			var activeContact = org.Contacts.AddNew();
			activeContact.OC_ContactName = "Active Contact";
			activeContact.OC_IsActive = true;

			var inactiveContact1 = org.Contacts.AddNew();
			inactiveContact1.OC_ContactName = "Inactive Contact 1";
			inactiveContact1.OC_IsActive = false;

			var inactiveContact2 = org.Contacts.AddNew();
			inactiveContact2.OC_ContactName = "Inactive Contact 2";
			inactiveContact2.OC_IsActive = false;

			opportunity.P8_OC = inactiveContact2.PK;
			Factory.Save();

			AssertEquals("Should return 1 active contact and the inactive contact saved on the opportunity.", 2, opportunity.Lookups.ActiveContacts.Count);
		}

		public void TestInactiveClientsShouldNotBeVisible()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "ChuckNorris";
			contact1.OC_IsActive = false;
			OrgContact contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Rambo";
			contact2.OC_IsActive = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();

			opp.AssignedOrgPK = org1.PK;
			opp.P8_OA_AssignedOffice = org1.Addresses[0].PK;

			Factory.Save();
			opp.Lookups.AssignedOfficeContacts.Load();

			AssertEquals("1 Contact Should be visible", 1, opp.Lookups.AssignedOfficeContacts.Count);
			AssertEquals("1 contact - Rambo", "Rambo", opp.Lookups.AssignedOfficeContacts[0].OC_ContactName);

			contact1.OC_IsActive = true;
			Factory.Save();
			opp.Lookups.AssignedOfficeContacts.Load();

			AssertEquals("2 Contacts Should be visible", 2, opp.Lookups.AssignedOfficeContacts.Count);
		}

		public void TestActiveSources_OrderbyDescription()
		{
			var registryValue = new CodeDescriptionBoolRelatedItemCollection()
			{
				{ "CD1", (NoResString)"Des 2", true },
				{ "CD2", (NoResString)"Des 3", true },
				{ "CD3", (NoResString)"Des 1", true },
			};

			using (OrganisationsDataRegistry.Instance.OpportunitySource.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
			{
				var org = Factory.New<OrgHeader>();
				var opportunity = org.SalesOpportunities.AddNew();
				var sources = opportunity.Lookups.ActiveSources;

				AssertEquals("sources has 3 items", 3, sources.Count);
				AssertContainsExactElementsInExactOrder("ActiveSources is ordered by Description", new string[] { "CD3", "CD1", "CD2" }, sources.Cast<CodeDescriptionPair>().Select(s => s.Code));
			}
		}

		public void TestAddresses()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address = org.Addresses.AddNew();

			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			AssertEquals("2 addresses", 2, opportunity.Lookups.Addresses.Count);

			org.Addresses.AddNew();
			AssertEquals("3 addresses", 3, opportunity.Lookups.Addresses.Count);
		}

		public void TestReturnValues()
		{
			var opportunities = new OpportunityStatusCollection();
			opportunities.Add("CRT", (NoResString)"Current", false, false, true, "");
			opportunities.Add("ARB", (NoResString)"Arbitrary", false, false, false, "");
			opportunities.Add("TST", (NoResString)"Test", false, false, true, "");

			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, opportunities);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Count", 2, opportunity.Lookups.ActiveStatuses.Count);
		}

		public void TestStatuses()
		{
			var collection = new OpportunityStatusCollection();
			collection.Add("ABC", (NoResString)"ABC Description");
			collection.Add("XYZ", (NoResString)"XYZ Description");

			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Count", 2, opportunity.Lookups.Statuses.Count);
			AssertEquals("GetDescriptionFromCode(\"ABC\"", "ABC Description", opportunity.Lookups.Statuses.GetDescriptionFromCode("ABC"));
			AssertEquals("GetDescriptionFromCode(\"XYZ\"", "XYZ Description", opportunity.Lookups.Statuses.GetDescriptionFromCode("XYZ"));
		}

		public void TestOutcomes()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add("ABC", (NoResString)"ABC Description", true);
			collection.Add("XYZ", (NoResString)"XYZ Description", false);
			OrganisationsDataRegistry.Instance.OpportunityOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var opp = Factory.New<OrgOpportunity>();

			AssertEquals(2, opp.Lookups.Outcomes.Count);
			AssertEquals("ABC Description", opp.Lookups.Outcomes.GetDescriptionFromCode("ABC"));
			AssertEquals("XYZ Description", opp.Lookups.Outcomes.GetDescriptionFromCode("XYZ"));

			AssertEquals(1, opp.Lookups.ActiveOutcomes.Count);
			AssertEquals("ABC Description", opp.Lookups.ActiveOutcomes.GetDescriptionFromCode("ABC"));
		}

		public void TestCloseReasons()
		{
			var collection = new OpportunityClosedReasonsCollection();
			collection.Add("ABC", (NoResString)"ABC Description", true);
			collection.Add("XYZ", (NoResString)"XYZ Description", false);
			OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var opp = Factory.New<OrgOpportunity>();

			AssertEquals(2, opp.Lookups.CloseReasons.Count);
			AssertEquals("ABC Description", opp.Lookups.CloseReasons.GetDescriptionFromCode("ABC"));
			AssertEquals("XYZ Description", opp.Lookups.CloseReasons.GetDescriptionFromCode("XYZ"));

			AssertEquals(1, opp.Lookups.ActiveCloseReasons.Count);
			AssertEquals("ABC Description", opp.Lookups.ActiveCloseReasons.GetDescriptionFromCode("ABC"));
		}

		public void TestClosedReasonsByStatus()
		{
			var collection = new OpportunityClosedReasonsCollection();
			collection.Add("ABC", (NoResString)"ABC Description", true);
			collection.Add("XYZ", (NoResString)"XYZ Description", false);
			collection.Add("R01", (NoResString)"R01 Description").StatusRules.AddNew().Code = "WON";
			collection.Add("R02", (NoResString)"R02 Description").StatusRules.AddNew().Code = "ABA";
			OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var opp = Factory.New<OrgOpportunity>();

			AssertEquals(4, opp.Lookups.CloseReasons.Count);
			AssertEquals("ABC Description", opp.Lookups.CloseReasons.GetDescriptionFromCode("ABC"));
			AssertEquals("XYZ Description", opp.Lookups.CloseReasons.GetDescriptionFromCode("XYZ"));
			AssertEquals("R01 Description", opp.Lookups.CloseReasons.GetDescriptionFromCode("R01"));
			AssertEquals("R02 Description", opp.Lookups.CloseReasons.GetDescriptionFromCode("R02"));

			opp.P8_Status = "LOS";
			AssertEquals(0, opp.Lookups.ActiveCloseReasonsByStatus.Count);

			opp.P8_Status = "";
			AssertEquals(0, opp.Lookups.ActiveCloseReasonsByStatus.Count);

			opp.P8_Status = "WON";
			AssertEquals(1, opp.Lookups.ActiveCloseReasonsByStatus.Count);
			AssertEquals("R01 Description", opp.Lookups.ActiveCloseReasonsByStatus.GetDescriptionFromCode("R01"));

			opp.P8_Status = "ABA";
			AssertEquals(1, opp.Lookups.ActiveCloseReasonsByStatus.Count);
			AssertEquals("R02 Description", opp.Lookups.ActiveCloseReasonsByStatus.GetDescriptionFromCode("R02"));
		}

		public void TestOpportunityTypes()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add("AAA", (NoResString)"AAA Type", true);
			collection.Add("BBB", (NoResString)"BBB Type", false);
			collection.Add("CCC", (NoResString)"CCC Type", true);
			OrganisationsDataRegistry.Instance.OpportunitySalesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(3, collection.Count);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Count", 3, opportunity.Lookups.Types.Count);
			AssertEquals("GetDescriptionFromCode(\"AAA\")", "AAA Type", opportunity.Lookups.Types.GetDescriptionFromCode("AAA"));
			AssertEquals("GetDescriptionFromCode(\"BBB\")", "BBB Type", opportunity.Lookups.Types.GetDescriptionFromCode("BBB"));
			AssertEquals("GetDescriptionFromCode(\"CCC\")", "CCC Type", opportunity.Lookups.Types.GetDescriptionFromCode("CCC"));
		}

		public void TestAssignedOffices()
		{
			var org = Factory.New<OrgHeader>();
			org.Addresses.AddNew();
			org.Addresses.AddNew();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.AssignedOrgPK = org.PK;

			AssertEquals("3 Addresses", 3, opportunity.Lookups.AssignedOffices.Count);

			opportunity.AssignedOrgPK = ZGuid.Empty;
			AssertEquals("0 Addresses", 0, opportunity.Lookups.AssignedOffices.Count);
		}

		public void TestSourceDetails()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("MRK", (NoResString)"Marketing Campaign", true, "CL2"); //exists in hardcoded list
			collection.Add("OTH", (NoResString)"Other Campaign", true, "XXX");
			collection.Add("MRI", (NoResString)"Old Marketing Campaign", false, "CL2"); //exists in hardcoded list but not active
			collection.Add("OTN", (NoResString)"Other Campaign no list", true);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var collection2 = new CodeDescriptionBoolCollection();
			collection2.Add("CP1", (NoResString)"Campaign catagory 1", true);
			collection2.Add("CP2", (NoResString)"Old Campaign catagory 2", false);
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();

			opp.P8_Source = "MRK";
			AssertEquals(1, opp.Lookups.ActiveSourceDetails.Count);
			AssertEquals("should match secondary list for Marketing Campaign", OrganisationsDataRegistry.Instance.CampaignCategory2List.Value[0].Code, opp.Lookups.ActiveSourceDetails[0].Code);
			AssertEquals("should match secondary list for Marketing Campaign", OrganisationsDataRegistry.Instance.CampaignCategory2List.Value[0].Description, opp.Lookups.ActiveSourceDetails[0].Description);

			opp.P8_Source = "OTH";
			AssertEquals(0, opp.Lookups.ActiveSourceDetails.Count);

			opp.P8_Source = "MRI";
			AssertEquals(1, opp.Lookups.ActiveSourceDetails.Count);
			AssertEquals("should match secondary list for Marketing Campaign", OrganisationsDataRegistry.Instance.CampaignCategory2List.Value[0].Code, opp.Lookups.ActiveSourceDetails[0].Code);
			AssertEquals("should match secondary list for Marketing Campaign", OrganisationsDataRegistry.Instance.CampaignCategory2List.Value[0].Description, opp.Lookups.ActiveSourceDetails[0].Description);

			opp.P8_Source = "OTN";
			AssertEquals(0, opp.Lookups.ActiveSourceDetails.Count);
		}

		public void TestContactsOfReferringOrg()
		{
			OrgHeader referringOrg = Factory.NewWithValidTestData<OrgHeader>();
			referringOrg.Contacts.AddNew().OC_ContactName = "One";

			OrgOpportunity opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH_ReferringOrganisation = referringOrg.PK;
			Factory.Save();

			opportunity.Lookups.ContactsOfReferringOrg.Load();
			AssertEquals("1 referring contact", 1, opportunity.Lookups.ContactsOfReferringOrg.Count);

			referringOrg.Contacts.AddNew().OC_ContactName = "Two";
			Factory.Save();

			opportunity.Lookups.ContactsOfReferringOrg.Load();
			AssertEquals("2 referring contacts", 2, opportunity.Lookups.ContactsOfReferringOrg.Count);
		}

		public void TestOpportunityActiveTypes()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add("A01", (NoResString)"CCC ActiveType", true);
			collection.Add("A02", (NoResString)"BBB ActiveType", false);
			collection.Add("A03", (NoResString)"AAA ActiveType", true);
			collection.Add("A04", (NoResString)"ABB ActiveType", true);
			collection.Add("A05", (NoResString)"ABC ActiveType", true);
			OrganisationsDataRegistry.Instance.OpportunitySalesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(5, collection.Count);

			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Count", 4, opportunity.Lookups.ActiveTypes.Count);
			AssertEquals("GetDescriptionFromCode(\"A03\")", "AAA ActiveType", opportunity.Lookups.ActiveTypes.GetDescriptionFromCode("A03"));
			AssertEquals("GetDescriptionFromCode(\"A04\")", "ABB ActiveType", opportunity.Lookups.ActiveTypes.GetDescriptionFromCode("A04"));
			AssertEquals("GetDescriptionFromCode(\"A05\")", "ABC ActiveType", opportunity.Lookups.ActiveTypes.GetDescriptionFromCode("A05"));
			AssertEquals("GetDescriptionFromCode(\"A01\")", "CCC ActiveType", opportunity.Lookups.ActiveTypes.GetDescriptionFromCode("A01"));

			var expectedCollection = collection.GetActiveCodeDescriptionPairList();
			expectedCollection.SortByDescription();
			AssertSortedDescription(expectedCollection, opportunity.Lookups.ActiveTypes);
		}

		public void TestOpportunityActiveExtraCategories()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add("A01", (NoResString)"CCC ActiveExtraCategory", true);
			collection.Add("A02", (NoResString)"BBB ActiveExtraCategory", false);
			collection.Add("A03", (NoResString)"AAA ActiveExtraCategory", true);
			collection.Add("A04", (NoResString)"ABB ActiveExtraCategory", true);
			collection.Add("A05", (NoResString)"ABC ActiveExtraCategory", true);
			OrganisationsDataRegistry.Instance.ProductTypeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(5, collection.Count);

			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Count", 4, opportunity.Lookups.ActiveExtraCategories.Count);
			AssertEquals("GetDescriptionFromCode(\"A03\")", "AAA ActiveExtraCategory", opportunity.Lookups.ActiveExtraCategories.GetDescriptionFromCode("A03"));
			AssertEquals("GetDescriptionFromCode(\"A04\")", "ABB ActiveExtraCategory", opportunity.Lookups.ActiveExtraCategories.GetDescriptionFromCode("A04"));
			AssertEquals("GetDescriptionFromCode(\"A05\")", "ABC ActiveExtraCategory", opportunity.Lookups.ActiveExtraCategories.GetDescriptionFromCode("A05"));
			AssertEquals("GetDescriptionFromCode(\"A01\")", "CCC ActiveExtraCategory", opportunity.Lookups.ActiveExtraCategories.GetDescriptionFromCode("A01"));

			var expectedCollection = collection.GetActiveCodeDescriptionPairList();
			expectedCollection.SortByDescription();
			AssertSortedDescription(expectedCollection, opportunity.Lookups.ActiveExtraCategories);
		}

		void AssertSortedDescription(CodeDescriptionPairList expectedCollection, ReadOnlyCodeDescriptionPairList actualCollection)
		{
			AssertEquals(expectedCollection.Count, actualCollection.Count);
			for (int i = 0; i < actualCollection.Count; i++)
			{
				AssertEquals(expectedCollection[i].Code, actualCollection[i].Code);
				AssertEquals(expectedCollection[i].Description, actualCollection[i].Description);
			}
		}
	}
}
