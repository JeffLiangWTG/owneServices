using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobApplicationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestApplicants()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			AssertNotNull("Applicants should not be null", application.Lookups.Applicants);
		}

		public void TestCampaigns()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			AssertNotNull("Campaigns should not be null", application.Lookups.Campaigns);
		}

		public void TestApplicationStatuses()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			AssertNotNull("Application Statuses should not be null", application.Lookups.ApplicationStatuses);
		}

		public void TestJobRoles()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			AssertNotNull("Job Roles should not be null", application.Lookups.JobRoles);
		}

		public void TestOverallRatings()
		{
			var application = Factory.New<HRJobApplication>();
			var ratings = application.Lookups.OverallRatings;
			AssertNotNull(ratings);
			AssertEquals(4, ratings.Count);

			Assert(ratings.ContainsCode("-1"));
			AssertEquals("Unrated", ratings.GetDescriptionFromCode("-1"));
			Assert(ratings.ContainsCode("1"));
			AssertEquals("Suitable", ratings.GetDescriptionFromCode("1"));
			Assert(ratings.ContainsCode("2"));
			AssertEquals("Potential", ratings.GetDescriptionFromCode("2"));
			Assert(ratings.ContainsCode("3"));
			AssertEquals("Unsuitable", ratings.GetDescriptionFromCode("3"));
		}

		public void TestSourceTypes()
		{
			var sourcesTypes = RecruiterDataRegistry.Instance.ReferringSourcesTypes.Value;
			var type = sourcesTypes.AddNew();
			type.Code = "C01";
			type.Bool = false;
			RecruiterDataRegistry.Instance.ReferringSourcesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourcesTypes);

			var application = Factory.NewWithValidTestData<HRJobApplication>();
			AssertEquals(false, application.Lookups.SourceTypes.ContainsCode("C01"));
			AssertEquals(true, application.Lookups.AllSourceTypes.ContainsCode("C01"));
		}

		public void TestReferringStaffs()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			AssertNotNull(application.Lookups.ReferringStaffs);
		}

		public void TestReferringPersons()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG_TEST1";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact 1";
			contact1.OC_Email = "u1@cw1.com";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2_TEST1";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Contact 2";
			contact2.OC_Email = "u2@cw1.com";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST9";
			Factory.Save();

			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_SourceType = ReferringSourcesTypes.Codes.NoReferrerRegistered;
			var referringPersons = Factory.Load<GlbPerson>(new ZQuery(application.Lookups.ReferringPersons.CompleteFilter).AddToFilter(GlbPersonSchema.PK, new[] { contact1.OC_PER, contact2.OC_PER, staff.GS_PER }));
			AssertCollectionContains(contact1.Person, referringPersons);
			AssertCollectionContains(contact2.Person, referringPersons);
			AssertCollectionContains(staff.Person, referringPersons);
			AssertEquals(false, application.Lookups.ReferringPersons.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>().Any());

			application.HP_OH_ReferringOrganisation = org.PK;
			referringPersons = Factory.Load<GlbPerson>(new ZQuery(application.Lookups.ReferringPersons.CompleteFilter).AddToFilter(GlbPersonSchema.PK, new[] { contact1.OC_PER, contact2.OC_PER, staff.GS_PER }));
			AssertCollectionContains(contact1.Person, referringPersons);
			AssertCollectionNotContains(contact2.Person, referringPersons);
			AssertCollectionNotContains(staff.Person, referringPersons);
			var filterDefault = application.Lookups.ReferringPersons.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>().Single();
			AssertEquals("Related Organization", filterDefault.FilterName);
			AssertEquals("Property", filterDefault.PropertyName);
			AssertEquals(org.PK, filterDefault.Value);
			AssertEquals(false, filterDefault.IsRemovable);

			application.HP_SourceType = ReferringSourcesTypes.Codes.RecruitmentAgent;
			application.HP_OH_ReferringOrganisation = ZGuid.Empty;
			referringPersons = Factory.Load<GlbPerson>(new ZQuery(application.Lookups.ReferringPersons.CompleteFilter).AddToFilter(GlbPersonSchema.PK, new[] { contact1.OC_PER, contact2.OC_PER, staff.GS_PER }));
			AssertCollectionContains(contact1.Person, referringPersons);
			AssertCollectionContains(contact2.Person, referringPersons);
			AssertCollectionNotContains(staff.Person, referringPersons);
			filterDefault = application.Lookups.ReferringPersons.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>().Single();
			AssertEquals("Related Context", filterDefault.FilterName);
			AssertEquals("Property0", filterDefault.PropertyName);
			AssertEquals(ZBool.True, filterDefault.Value);
			AssertEquals(false, filterDefault.IsRemovable);
		}

		public void TestReferringOrganisationsList()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG_1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG_2";
			Factory.Save();

			var referringParties = new ReferringPartyConfigurationCollection();
			var party1 = referringParties.AddNew();
			party1.Domain = "wtg1@gmail.com";
			party1.ReferringParty = OrgHeaderSchema.Constants.Prefix;
			party1.OrganizationPK = org.PK;
			party1.DefaultReferringSource = ReferringSourcesTypes.Codes.RecruitmentAgent;
			var party2 = referringParties.AddNew();
			party2.Domain = "wtg2@gmail.com";
			party2.ReferringParty = GlbStaffSchema.Constants.Prefix;
			party2.DefaultReferringSource = ReferringSourcesTypes.Codes.StaffReferral;
			var party3 = referringParties.AddNew();
			party3.Domain = "wtg3@gmail.com";
			party3.ReferringParty = OrgHeaderSchema.Constants.Prefix;
			party3.OrganizationPK = org2.PK;
			party3.DefaultReferringSource = ReferringSourcesTypes.Codes.RecruitmentAgent;
			RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, referringParties);

			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_SourceType = ReferringSourcesTypes.Codes.RecruitmentAgent;
			AssertEquals(2, application.Lookups.ReferringOrganisationsList.Count);
			AssertEquals(true, application.Lookups.ReferringOrganisationsList.ContainsCode("ORG_1"));
			AssertEquals(true, application.Lookups.ReferringOrganisationsList.ContainsCode("ORG_2"));
		}
	}
}
