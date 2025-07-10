using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Recruitment.Testing.RecruitmentDataHelpers;

namespace Enterprise.Recruitment.Testing.Module
{
	[TestedType(typeof(CandidateFilterStripBusinessObject))]
	class CandidateFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CandidateFilterStripBusinessObject(Factory);

		#region Filters

		public void TestRatingFilter()
		{
			var suitableCandidate = CreateCandidate(Factory, "Jack");
			suitableCandidate.Rating_Suitable = true;

			var unsuitableCandidate = CreateCandidate(Factory, "Jim");
			unsuitableCandidate.Rating_Unsuitable = true;

			var potentialCandidate = CreateCandidate(Factory, "Jane");
			potentialCandidate.Rating_Potential = true;

			var unrated = CreateCandidate(Factory, "NewGuy");
			var alsoUnrated = CreateCandidate(Factory, "Old");
			alsoUnrated.Application.HP_ApplicationOverallRating = "bum";

			Factory.Save();

			var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;

			var filter = (ModuleTextFilter)filterBizO["Rating"];
			filter.IsActive = true;

			CombineAssertions(() =>
			{
				filter.Property = "3";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder("Unsuitable", new[] { unsuitableCandidate }, Collection);

				filter.Property = "2";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder("Potential", new[] { potentialCandidate }, Collection);

				filter.Property = "1";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder("Suitable", new[] { suitableCandidate }, Collection);

				filter.Property = "-1";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder("Anyone who doesn't have a valid rating should be considered unrated", new[] { unrated, alsoUnrated }, Collection);
			});
		}

		public void TestNameFilter()
		{
			var (applicant1, applicant2) = Candidates;

			Factory.Save();

			var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleTextFilter)filterBizO["Candidate Name"];
			nameFilter.IsActive = true;
			nameFilter.Property = "AAA";

			Collection.Load(filterBizO.Filter);

			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(applicant2, Collection);
			nameFilter.IsActive = false;

			var nameFilter2 = (ModuleTextFilter)filterBizO["Candidate Name"];
			nameFilter2.IsActive = true;
			nameFilter2.Property = "BBB";

			Collection.Load(filterBizO.Filter);

			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(applicant1, Collection);
		}

		public void TestCityFilter()
		{
			Applicant1.HA_City = "Sydney";
			Applicant2.HA_City = "Melbourne";

			Factory.Save();

			var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleTextFilter)filterBizO["City"];
			nameFilter.IsActive = true;
			nameFilter.Property = "Sydney";

			Collection.Load(filterBizO.Filter);

			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Applicant2, Collection);
			nameFilter.IsActive = false;

			var nameFilter2 = (ModuleTextFilter)filterBizO["City"];
			nameFilter2.IsActive = true;
			nameFilter2.Property = "Melbourne";

			Collection.Load(filterBizO.Filter);

			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Applicant1, Collection);
		}

		public void TestEmailFilter()
		{
			Applicant1.HA_EmailAddress = "Email1@Zmail.com";
			Applicant2.HA_EmailAddress = "Email2@Zmail.com";

			Factory.Save();

			var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
			var emailFilter = (ModuleTextFilter)filterBizO["Email"];
			emailFilter.IsActive = true;
			emailFilter.Property = "Email2@Zmail.com";

			Collection.Load(filterBizO.Filter);

			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Applicant2, Collection);
			emailFilter.IsActive = false;

			var emailFilter2 = (ModuleTextFilter)filterBizO["Email"];
			emailFilter2.IsActive = true;
			emailFilter2.Property = "Email1@Zmail.com";

			Collection.Load(filterBizO.Filter);

			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Applicant1, Collection);
		}

		[TestDate(2015, 11, 12, 12, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestSubmissionTime()
		{
			Applicant1.Applications[0].HP_SubmissionTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			Applicant2.Applications[0].HP_SubmissionTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = Applicant1.PK;
			application.HP_SubmissionTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			CandidateFilterStripBusinessObject filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
			filterBizO["Last Application"].IsActive = true;
			((ModuleDateFilter)filterBizO["Last Application"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filterBizO["Last Application"]).Property1 = ZDateTime.UtcNow;

			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant1, CollectionApplicants);

			((ModuleDateFilter)filterBizO["Last Application"]).Property1 = ZDateTime.UtcNow.AddDays(2);
			((ModuleDateFilter)filterBizO["Last Application"]).Property2 = ZDateTime.UtcNow.AddDays(3);

			Collection.Load(filterBizO.Filter);
			AssertEquals(0, Collection.Count);
			AssertCollectionNotContains(Applicant1, CollectionApplicants);
		}

		public void TestResumeKeywordsFilter()
		{
			using (GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.SetTemporaryValue(default, default, default, false))
			{
				var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();

				var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
				var application1 = campaign.Applications.AddNew();
				application1.HP_HA = applicant1.PK;
				var doc1 = application1.Documents.AddNew();
				doc1.HPD_Content = "<Resume><NonXMLResume><TextResume>KW1<span></span><span>KW2</span><span>KW3</span><span>KW4 KW5</span></TextResume></NonXMLResume></Resume>";

				var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
				var application2 = campaign.Applications.AddNew();
				application2.HP_HA = applicant2.PK;
				var doc2 = application2.Documents.AddNew();
				doc2.HPD_Content = "<Resume><NonXMLResume><TextResume><span></span>Keyword1<span></span>Keyword2<span>Keyword3</span><span>Keyword4 Keyword5</span></TextResume></NonXMLResume></Resume>";

				var applicant3 = Factory.NewWithValidTestData<HRJobApplicant>();
				var application3 = campaign.Applications.AddNew();
				application3.HP_HA = applicant3.PK;
				var doc3 = application3.Documents.AddNew();
				doc3.HPD_Content = "<Resume><NonXMLResume><TextResume><span>Keyword3</span>KW3<span></span><span>KW3 Keyword3</span><span>He said, \"Don't quote me.\"</span></TextResume></NonXMLResume></Resume>";

				var applicant4 = Factory.NewWithValidTestData<HRJobApplicant>();
				var application4 = campaign.Applications.AddNew();
				application4.HP_HA = applicant4.PK;
				var doc4 = application4.Documents.AddNew();
				doc4.HPD_Content = "<ResumeXYZ>ABC</ResumeXYZ>";

				Factory.Save();

				var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
				var filter = filterBizO["Resume Keywords"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.Property = "";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { applicant1, applicant2, applicant3, applicant4 }, CollectionApplicants);

				filter.Property = "@@@@@";
				Collection.Load(filterBizO.Filter);
				AssertEquals(0, Collection.Count);

				filter.Property = "ABC KW3 DEF";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { applicant1, applicant3 }, CollectionApplicants);

				filter.Property = "XYZ Keyword3 TTT";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { applicant2, applicant3 }, CollectionApplicants);

				filter.Property = "AAA \"CC OO\" DDD \"KW3 Keyword3\" CCC";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { applicant3 }, CollectionApplicants);

				filter.Property = "KW1 Keyword1";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { applicant1, applicant2 }, CollectionApplicants);

				filter.Property = "KW3 Keyword3";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { applicant1, applicant2, applicant3 }, CollectionApplicants);

				filter.Property = "\"DON'T QuOtE ME.\"";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { applicant3 }, CollectionApplicants);
			}
		}

		public void TestJobRole()
		{
			var jobRole1 = Factory.NewWithValidTestData<HRJobRole>();
			var jobRole2 = Factory.NewWithValidTestData<HRJobRole>();

			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();

			campaign1.HV_HJ_JobRole = jobRole1.PK;
			campaign2.HV_HJ_JobRole = jobRole2.PK;

			var applicationCampaign1 = Applicant1.Applications.AddNew();
			applicationCampaign1.HP_HV = campaign1.PK;

			var applicationCampaign2 = Applicant2.Applications.AddNew();
			applicationCampaign2.HP_HV = campaign2.PK;

			Factory.Save();

			var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
			var filter = (ModuleGuidFilter)filterBizO["Selected Role"];
			Assert("This filter not supported on Web - yet", !filter.IsPublishedOnWeb);

			filter.Property = jobRole1.PK;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, CollectionApplicants);
			AssertCollectionNotContains(Applicant2, CollectionApplicants);

			filter.Property = jobRole2.PK;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, CollectionApplicants);
			AssertCollectionContains(Applicant2, CollectionApplicants);

			filter.Property = ZGuid.Empty;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, CollectionApplicants);
			AssertCollectionContains(Applicant2, CollectionApplicants);

			var jobRole3 = Factory.NewWithValidTestData<HRJobRole>();
			Factory.Save();

			filter.Property = jobRole3.PK;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, CollectionApplicants);
			AssertCollectionNotContains(Applicant2, CollectionApplicants);
		}

		public void TestCountry()
		{
			RefCountry[] countries = (RefCountry[])(Factory.Load(typeof(RefCountry), new ZQuery()));

			Applicant1.HA_RN_NKCountry = countries[0].Code;
			Applicant2.HA_RN_NKCountry = countries[1].Code;

			Factory.Save();

			ModuleNkFilter filter = GetModuleNkFilter("Country");

			filter.Property = countries[0].Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Applicant1, CollectionApplicants);
			AssertCollectionNotContains(Applicant2, CollectionApplicants);

			filter.Property = countries[1].Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Applicant1, CollectionApplicants);
			AssertCollectionContains(Applicant2, CollectionApplicants);

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Applicant1, CollectionApplicants);
			AssertCollectionContains(Applicant2, CollectionApplicants);

			filter.Property = countries[2].Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Applicant1, CollectionApplicants);
			AssertCollectionNotContains(Applicant2, CollectionApplicants);
		}

		public void TestResponsibleStaff_JobApplication()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant3 = Factory.NewWithValidTestData<HRJobApplicant>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var application1 = applicant1.Applications.AddNew();
			var application2 = applicant2.Applications.AddNew();
			var application3 = applicant3.Applications.AddNew();

			application1.HP_GS_NKAssignedTo = staff1.GS_Code;
			application2.HP_GS_NKAssignedTo = staff2.GS_Code;
			application3.HP_GS_NKAssignedTo = staff3.GS_Code;

			Factory.Save();

			ModuleNkFilter filter = GetModuleNkFilter("Responsible Recruiter (Job Application)");
			AssertResponsibleStaffWithCurrentUser(staff1, staff2, staff3, applicant1, applicant2, applicant3, filter);

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			AssertResponsibleStaffWithExact(staff1, staff2, staff3, applicant1, applicant2, applicant3, filter);
		}

		public void TestResponsibleStaff_JobOpenings()
		{
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign3 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant3 = Factory.NewWithValidTestData<HRJobApplicant>();

			var application1 = applicant1.Applications.AddNew();
			var application2 = applicant2.Applications.AddNew();
			var application3 = applicant3.Applications.AddNew();

			campaign1.HV_GS_NKControlledBy = staff1.GS_Code;
			campaign2.HV_GS_NKControlledBy = staff2.GS_Code;
			campaign3.HV_GS_NKControlledBy = staff3.GS_Code;

			application1.HP_HV = campaign1.PK;
			application2.HP_HV = campaign2.PK;
			application3.HP_HV = campaign3.PK;

			Factory.Save();

			ModuleNkFilter filter = GetModuleNkFilter("Responsible Recruiter (Job Openings)");
			AssertResponsibleStaffWithCurrentUser(staff1, staff2, staff3, applicant1, applicant2, applicant3, filter);

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			AssertResponsibleStaffWithExact(staff1, staff2, staff3, applicant1, applicant2, applicant3, filter);
		}

		public void TestApplicantFilter()
		{
			Applicant1.HA_EmailAddress = "AAA@test.com";
			Applicant2.HA_EmailAddress = "BBB@test.com";

			Factory.Save();

			var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
			var applicantFilter = (ModuleGuidFilter)filterBizO["Applicant"];
			applicantFilter.IsActive = true;
			applicantFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;

			var subFilter = applicantFilter.SelectedFilters.AddTextFilterStrip("Email", Applicant1.HA_EmailAddress);
			subFilter.IsActive = true;

			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, CollectionApplicants);
			AssertCollectionNotContains(Applicant2, CollectionApplicants);
		}

		public void TestApplicationFilter()
		{
			var (candidate1, candidate2) = Candidates;
			var application1 = candidate1.Application;
			var application2 = candidate1.Application;
			application1.HP_CurrentStatus = "INP";
			application2.HP_CurrentStatus = "TST";

			Factory.Save();

			var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
			var applicationFilter = (ModuleGuidFilter)filterBizO["Application"];
			applicationFilter.IsActive = true;
			applicationFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;

			var subFilter = applicationFilter.SelectedFilters.AddTextFilterStrip("Status", application1.HP_CurrentStatus);
			subFilter.IsActive = true;

			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, CollectionApplicants);
			AssertCollectionNotContains(Applicant2, CollectionApplicants);
		}

		public void TestJobOpeningsFilter()
		{
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();

			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();

			var application1 = applicant1.Applications.AddNew();
			var application2 = applicant2.Applications.AddNew();

			application1.HP_HV = campaign1.PK;
			application2.HP_HV = campaign2.PK;
			campaign1.HV_AdTitle = "Auckland";
			Factory.Save();

			var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
			var jobOpeningsFilter = (ModuleGuidFilter)filterBizO["Job Openings"];
			jobOpeningsFilter.IsActive = true;
			jobOpeningsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;

			var subFilter = jobOpeningsFilter.SelectedFilters.AddTextFilterStrip("Ad Title", campaign1.HV_AdTitle);
			subFilter.IsActive = true;

			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(applicant1, CollectionApplicants);
			AssertCollectionNotContains(applicant2, CollectionApplicants);
		}

		public void TestApplicationStatusFilter()
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();

			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = campaign.Applications.AddNew();
			application1.HP_HA = applicant1.PK;
			application1.HP_CurrentStatus = "AAA";

			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			var application2 = campaign.Applications.AddNew();
			application2.HP_HA = applicant2.PK;
			application2.HP_CurrentStatus = "AAA";

			var applicant3 = Factory.NewWithValidTestData<HRJobApplicant>();
			var application3 = campaign.Applications.AddNew();
			application3.HP_HA = applicant3.PK;
			application3.HP_CurrentStatus = "BBB";

			var filterBizO = (CandidateFilterStripBusinessObject)CachedBusinessObject;
			var filter = filterBizO["Application Status"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.Property = "";
			Collection.Load(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { applicant1, applicant2, applicant3 }, CollectionApplicants);

			filter.Property = "CCC";
			Collection.Load(filterBizO.Filter);
			AssertEquals(0, Collection.Count);

			filter.Property = "AAA";
			Collection.Load(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { applicant1, applicant2 }, CollectionApplicants);

			filter.Property = "BBB";
			Collection.Load(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { applicant3 }, CollectionApplicants);
		}

		void AssertResponsibleStaffWithCurrentUser(GlbStaff staff1, GlbStaff staff2, GlbStaff staff3, HRJobApplicant applicant1, HRJobApplicant applicant2, HRJobApplicant applicant3, ModuleNkFilter filter)
		{
			GlbStaff.CurrentUser.GS_Code = staff1.GS_Code;
			filter.Property = staff1.GS_Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(applicant1, CollectionApplicants);
			AssertCollectionNotContains(applicant2, CollectionApplicants);
			AssertCollectionNotContains(applicant3, CollectionApplicants);

			GlbStaff.CurrentUser.GS_Code = staff2.GS_Code;
			filter.Property = staff2.GS_Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(applicant1, CollectionApplicants);
			AssertCollectionContains(applicant2, CollectionApplicants);
			AssertCollectionNotContains(applicant3, CollectionApplicants);

			GlbStaff.CurrentUser.GS_Code = staff3.GS_Code;
			filter.Property = staff3.GS_Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(applicant1, CollectionApplicants);
			AssertCollectionNotContains(applicant2, CollectionApplicants);
			AssertCollectionContains(applicant3, CollectionApplicants);
		}

		void AssertResponsibleStaffWithExact(GlbStaff staff1, GlbStaff staff2, GlbStaff staff3, HRJobApplicant applicant1, HRJobApplicant applicant2, HRJobApplicant applicant3, ModuleNkFilter filter)
		{
			filter.Property = staff1.GS_Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(applicant1, CollectionApplicants);
			AssertCollectionNotContains(applicant2, CollectionApplicants);
			AssertCollectionNotContains(applicant3, CollectionApplicants);

			filter.Property = staff2.GS_Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(applicant1, CollectionApplicants);
			AssertCollectionContains(applicant2, CollectionApplicants);
			AssertCollectionNotContains(applicant3, CollectionApplicants);

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(applicant1, CollectionApplicants);
			AssertCollectionContains(applicant2, CollectionApplicants);
			AssertCollectionContains(applicant3, CollectionApplicants);

			filter.Property = staff3.GS_Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(applicant1, CollectionApplicants);
			AssertCollectionNotContains(applicant2, CollectionApplicants);
			AssertCollectionContains(applicant3, CollectionApplicants);
		}

		public void TestDefaultAsCurrentStaff_ResponsibleStaff_JobApplication()
		{
			var expectedCOList = new CodeDescriptionPairList();
			expectedCOList.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			expectedCOList.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual);
			expectedCOList.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			expectedCOList.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			expectedCOList.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.CurrentUser);

			AssertComparionOperator("Responsible Recruiter (Job Application)", "current user", expectedCOList);
		}

		public void TestDefaultAsCurrentStaff_ResponsibleStaff_JobOpenings()
		{
			var expectedCOList = new CodeDescriptionPairList();
			expectedCOList.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			expectedCOList.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.CurrentUser);

			AssertComparionOperator("Responsible Recruiter (Job Openings)", "current user", expectedCOList);
		}

		void AssertComparionOperator(string filtername, string firstComparisonOperator, CodeDescriptionPairList expectedCOList)
		{
			ModuleNkFilter filter = GetModuleNkFilter(filtername);
			AssertEquals(firstComparisonOperator, filter.ComparisonOperator);

			var filterCOList = filter.ComparisonOperator_List;
			AssertEquals(expectedCOList.Count, filterCOList.Count);

			foreach (CodeDescriptionPair expectedCO in expectedCOList)
			{
				filterCOList.Contains(expectedCO);
			}
		}

		protected ModuleNkFilter GetModuleNkFilter(ZString description)
		{
			return (ModuleNkFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		#endregion

		#region Implementation

		IEnumerable<HRJobApplicant> CollectionApplicants => Collection.Select(c => c.Applicant);

		(Candidate, Candidate) Candidates => candidates ?? (candidates = (CreateCandidate(Factory, "AAA"), CreateCandidate(Factory, "BBB"))).Value;
		(Candidate, Candidate)? candidates;

		HRJobApplicant Applicant1 => Candidates.Item1.Applicant;
		HRJobApplicant Applicant2 => Candidates.Item2.Applicant;

		protected CandidateBusinessObjectCollection Collection;

		protected override void SetUp()
		{
			base.SetUp();

			Collection = new CandidateBusinessObjectCollection(Factory);

			Factory.Save();
		}

		#endregion
	}
}
