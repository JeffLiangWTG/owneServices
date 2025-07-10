using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobApplicationFilterBusinessObject))]
	public class HRJobApplicationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters
		public void TestHasDocumentFilter()
		{
			var document = Factory.New<HRJobApplicationDocument>();
			document.HPD_HP = Application1.PK;
			document.HPD_Type = "RES";
			document.HPD_Content = "<xml/>";
			Factory.Save();
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var filter = (ModuleFlagsFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.HasApplicationDocuments];
			filter.IsActive = true;
			filter.Property0 = true;
			Collection.AdditionalFilter = filterBizO.Filter;
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Application1, Collection);
			AssertCollectionNotContains(Application2, Collection);
			filter.Property0 = false;
			Collection.AdditionalFilter = filterBizO.Filter;
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Application2, Collection);
			AssertCollectionNotContains(Application1, Collection);
		}

		public void TestSubmissionDateFilter()
		{
			Application1.HP_SubmissionTimeUtc = new ZDateTime(2018, 12, 31);
			Application2.HP_SubmissionTimeUtc = new ZDateTime(2017, 12, 31);
			Factory.Save();
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var filter = (ModuleDateFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.SubmissionDate];
			filter.IsActive = true;
			filter.Property1 = Application1.HP_SubmissionTimeUtc.AddMonths(-1);
			filter.Property2 = Application1.HP_SubmissionTimeUtc.AddMonths(1);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(2, Collection.Count);
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
		}

		public void TestApplicantFilter()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			applicant1.HA_FullName = "AAA";
			applicant2.HA_FullName = "BBB";
			Application1.HP_HA = applicant1.PK;
			Application2.HP_HA = applicant2.PK;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.ApplicantFullName];
			nameFilter.IsActive = true;
			nameFilter.Property = "AAA";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
			nameFilter.IsActive = false;
			var pkFilter = (ModuleGuidFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.Applicant];
			pkFilter.IsActive = true;
			pkFilter.Property = applicant2.PK;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application1, Collection);
		}

		public void TestApplicantEmailFilter()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			applicant1.HA_EmailAddress = "test@test.com";
			applicant2.HA_EmailAddress = "sample@test.com";
			Application1.HP_HA = applicant1.PK;
			Application2.HP_HA = applicant2.PK;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var emailFilter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.ApplicantEmail];
			emailFilter.IsActive = true;
			emailFilter.Property = "test@test.com";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
		}

		public void TestApplicantMobilePhoneFilter()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			applicant1.HA_MobilePhone = "0412345678";
			applicant2.HA_MobilePhone = "0498765432";
			Application1.HP_HA = applicant1.PK;
			Application2.HP_HA = applicant2.PK;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var mobilePhoneFilter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.ApplicantMobilePhone];
			mobilePhoneFilter.IsActive = true;
			mobilePhoneFilter.Property = "0412345678";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
		}

		public void TestAdFilter()
		{
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign1.HV_AdTitle = "AAA";
			campaign2.HV_AdTitle = "BBB";
			Application1.HP_HV = campaign1.PK;
			Application2.HP_HV = campaign2.PK;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.AdTitle];
			nameFilter.IsActive = true;
			nameFilter.Property = "AAA";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
		}

		public void TestAdFilter_NullHP_HVOrHV_AdTitle()
		{
			var role = Factory.NewWithValidTestData<HRJobRole>();
			role.HJ_JobTitle = "AAA";
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign.HV_HJ_JobRole = ZGuid.Empty;
			Application1.HP_HV = campaign.PK;
			Application2.HP_HV = ZGuid.Empty;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var pkFilter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.AdTitle];
			pkFilter.IsActive = true;
			pkFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(2, Collection.Count);

			campaign.HV_AdTitle = "test";
			Factory.Save();
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application1, Collection);
		}

		public void TestJobFilter()
		{
			var role1 = Factory.NewWithValidTestData<HRJobRole>();
			var role2 = Factory.NewWithValidTestData<HRJobRole>();
			role1.HJ_JobTitle = "AAA";
			role2.HJ_JobTitle = "BBB";
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign1.HV_HJ_JobRole = role1.PK;
			campaign2.HV_HJ_JobRole = role2.PK;
			Application1.HP_HV = campaign1.PK;
			Application2.HP_HV = campaign2.PK;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.JobTitle];
			nameFilter.IsActive = true;
			nameFilter.Property = "AAA";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
			nameFilter.IsActive = false;
			var pkFilter = (ModuleGuidFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.JobRole];
			pkFilter.IsActive = true;
			pkFilter.Property = role2.PK;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application1, Collection);
		}

		public void TestJobFilter_NullHP_HVOrHV_HJ()
		{
			var role = Factory.NewWithValidTestData<HRJobRole>();
			role.HJ_JobTitle = "AAA";
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign.HV_HJ_JobRole = ZGuid.Empty;
			Application1.HP_HV = campaign.PK;
			Application2.HP_HV = ZGuid.Empty;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var pkFilter = (ModuleGuidFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.JobRole];
			pkFilter.IsActive = true;
			pkFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(2, Collection.Count);

			campaign.HV_HJ_JobRole = role.PK;
			Factory.Save();
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application1, Collection);
		}

		public void TestContactFilter()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "AAA";
			contact2.OC_ContactName = "BBB";
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign1.HV_OC_ClientContact = contact1.PK;
			campaign2.HV_OC_ClientContact = contact2.PK;
			Application1.HP_HV = campaign1.PK;
			Application2.HP_HV = campaign2.PK;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.ContactName];
			nameFilter.IsActive = true;
			nameFilter.Property = "AAA";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
			nameFilter.IsActive = false;
			var pkFilter = (ModuleGuidFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.Contact];
			pkFilter.IsActive = true;
			pkFilter.Property = contact2.PK;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application1, Collection);
		}

		public void TestCurrentStatusFilter()
		{
			ApplicationStatusCollection list = new ApplicationStatusCollection();
			list.AddPair("***", "Application Status");
			RecruiterDataRegistry.Instance.ApplicationStatuses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			Application1.HP_CurrentStatus = "***";
			Application2.HP_CurrentStatus = "aaa";
			Factory.Save();
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var filter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.CurrentStatus];
			filter.IsActive = true;
			filter.Property = "***";
			AssertEquals(2, Collection.Count);
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
		}

		public void TestOverallRatingFilter()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			Application1.HP_HA = applicant1.PK;
			Application2.HP_HA = applicant2.PK;
			Application1.HP_ApplicationOverallRating = "-1";
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var overallRatingFilter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.OverallRating];
			overallRatingFilter.IsActive = true;
			overallRatingFilter.Property = "-1";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(2, Collection.Count);

			Application1.HP_ApplicationOverallRating = "2";
			Application2.HP_ApplicationOverallRating = "3";
			Factory.Save();
			overallRatingFilter.Property = "2";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
		}

		public void TestLocationFilter()
		{
			var location1 = Factory.NewWithValidTestData<OrgHeader>();
			var location2 = Factory.NewWithValidTestData<OrgHeader>();
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign1.HV_OH_ClientAccount = location1.PK;
			campaign2.HV_OH_ClientAccount = location2.PK;
			Application1.HP_HV = campaign1.PK;
			Application2.HP_HV = campaign2.PK;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleGuidFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.OfficeLocation];
			nameFilter.IsActive = true;
			nameFilter.Property = location1.PK;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
		}

		public void TestCoordinatorFilter()
		{
			var coordinator1 = Factory.NewWithValidTestData<GlbStaff>();
			var coordinator2 = Factory.NewWithValidTestData<GlbStaff>();
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign1.HV_GS_NKControlledBy = coordinator1.GS_Code;
			campaign2.HV_GS_NKControlledBy = coordinator2.GS_Code;
			Application1.HP_HV = campaign1.PK;
			Application2.HP_HV = campaign2.PK;
			Factory.Save();
			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleNkFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.RecruitmentCoordinator];
			nameFilter.IsActive = true;
			nameFilter.Property = coordinator1.GS_Code;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
		}

		public void TestResponsibleStaff_JobApplication()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			Application1.HP_GS_NKAssignedTo = staff1.GS_Code;
			Application2.HP_GS_NKAssignedTo = staff2.GS_Code;

			Factory.Save();

			AssertEquals(2, Collection.Count);
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var nameFilter = (ModuleNkFilter)filterBizO["Responsible Recruiter (Job Application)"];
			nameFilter.IsActive = true;
			nameFilter.Property = staff1.GS_Code;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(1, Collection.Count);
			AssertCollectionNotContains(Application2, Collection);
		}

		public void TestReferringPartyFilters()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TEST_ORG1";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			Factory.Save();
			var application1 = Factory.NewWithValidTestData<HRJobApplication>();
			application1.HP_SourceType = "TP0";
			var application2 = Factory.NewWithValidTestData<HRJobApplication>();
			application2.HP_SourceDetails = "HP_SourceDetails1";
			var application3 = Factory.NewWithValidTestData<HRJobApplication>();
			application3.HP_SourceType = "MAN";
			application3.HP_OH_ReferringOrganisation = org.PK;
			var application4 = Factory.NewWithValidTestData<HRJobApplication>();
			application4.HP_SourceType = "MAN";
			application4.HP_PER_ReferringPerson = staff1.GS_PER;
			var application5 = Factory.NewWithValidTestData<HRJobApplication>();
			application5.HP_SourceType = "STF";
			application5.ReferringStaffCode = "ST2";
			Factory.Save();
			var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
			var sourceFilter = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.Source];
			sourceFilter.IsActive = true;
			sourceFilter.Property = "TP0";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(application1, Collection.Single());
			sourceFilter.IsActive = false;
			var sourceDetails = (ModuleTextFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.SourceDetails];
			sourceDetails.IsActive = true;
			sourceDetails.Property = "HP_SourceDetails1";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(application2, Collection.Single());
			sourceDetails.IsActive = false;
			var referringOrganisation = (ModuleGuidFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.ReferringOrganisation];
			referringOrganisation.IsActive = true;
			referringOrganisation.Property = org.PK;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(application3, Collection.Single());
			referringOrganisation.IsActive = false;
			var referringPerson = (ModuleGuidFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.ReferringPerson];
			referringPerson.IsActive = true;
			referringPerson.Property = staff1.GS_PER;
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(application4, Collection.Single());
			referringPerson.IsActive = false;
			var referringStaff = (ModuleNkFilter)filterBizO[HRJobApplicationFilterProvider.FilterDescription.ReferringStaff];
			referringStaff.IsActive = true;
			referringStaff.Property = "ST2";
			Collection.AdditionalFilter = filterBizO.Filter;
			Collection.RefreshFromDb();
			AssertEquals(application5, Collection.Single());
			referringStaff.IsActive = false;
		}

		public void TestResumeKeywordsFilter()
		{
			using (GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.SetTemporaryValue(default, default, default, false))
			{
				var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
				var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
				var applicant3 = Factory.NewWithValidTestData<HRJobApplicant>();
				var applicant4 = Factory.NewWithValidTestData<HRJobApplicant>();
				var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
				Application1.HP_HA = applicant.PK;
				var doc1 = Application1.Documents.AddNew();
				doc1.HPD_Content = "<Resume><NonXMLResume><TextResume>KW1<span></span><span>KW2</span><span>KW3</span><span>KW4 KW5</span></TextResume></NonXMLResume></Resume>";
				Application2.HP_HA = applicant2.PK;
				var doc2 = Application2.Documents.AddNew();
				doc2.HPD_Content = "<Resume><NonXMLResume><TextResume><span></span>Keyword1<span></span>Keyword2<span>Keyword3</span><span>Keyword4 Keyword5</span></TextResume></NonXMLResume></Resume>";
				var application3 = campaign.Applications.AddNew();
				application3.HP_HA = applicant3.PK;
				var doc3 = application3.Documents.AddNew();
				doc3.HPD_Content = "<Resume><NonXMLResume><TextResume><span>Keyword3</span>KW3<span></span><span>KW3 Keyword3</span><span>He said, \"Don't quote me.\"</span></TextResume></NonXMLResume></Resume>";
				var application4 = campaign.Applications.AddNew();
				application4.HP_HA = applicant4.PK;
				var doc4 = application4.Documents.AddNew();
				doc4.HPD_Content = "<ResumeXYZ>ABC</ResumeXYZ>";
				Factory.Save();
				var filterBizO = (HRJobApplicationFilterBusinessObject)CachedBusinessObject;
				var filter = filterBizO[HRJobApplicationFilterProvider.FilterDescription.ResumeKeywords] as ModuleTextFilter;
				filter.IsActive = true;
				filter.Property = "";
				Collection.AdditionalFilter = filterBizO.Filter;
				Collection.RefreshFromDb();
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Application1, Application2, application3, application4 });
				filter.Property = "@@@@@";
				Collection.AdditionalFilter = filterBizO.Filter;
				Collection.RefreshFromDb();
				AssertEquals(0, Collection.Count);
				filter.Property = "ABC KW3 DEF";
				Collection.AdditionalFilter = filterBizO.Filter;
				Collection.RefreshFromDb();
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Application1, application3 });
				filter.Property = "XYZ Keyword3 TTT";
				Collection.AdditionalFilter = filterBizO.Filter;
				Collection.RefreshFromDb();
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Application2, application3 });
				filter.Property = "AAA \"CC OO\" DDD \"KW3 Keyword3\" CCC";
				Collection.AdditionalFilter = filterBizO.Filter;
				Collection.RefreshFromDb();
				AssertContainsExactElementsInAnyOrder(Collection, new[] { application3 });
				filter.Property = "KW1 Keyword1";
				Collection.AdditionalFilter = filterBizO.Filter;
				Collection.RefreshFromDb();
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Application1, Application2 });
				filter.Property = "KW3 Keyword3";
				Collection.AdditionalFilter = filterBizO.Filter;
				Collection.RefreshFromDb();
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Application1, Application2, application3 });
				filter.Property = "\"DON'T QuOtE ME.\"";
				Collection.AdditionalFilter = filterBizO.Filter;
				Collection.RefreshFromDb();
				AssertContainsExactElementsInAnyOrder(Collection, new[] { application3 });
			}
		}

		#endregion
		#region Implementation
		protected HRJobApplicationCollection Collection;
		protected HRJobApplication Application1;
		protected HRJobApplication Application2;
		protected override void SetUp()
		{
			base.SetUp();
			Collection = new HRJobApplicationCollection(Factory);
			Application1 = Factory.NewWithValidTestData<HRJobApplication>();
			Application2 = Factory.NewWithValidTestData<HRJobApplication>();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new HRJobApplicationFilterBusinessObject();
		}
		#endregion
	}
}
