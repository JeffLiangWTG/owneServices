using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobApplicantFilterBusinessObject))]
	public class HRJobApplicantFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters
		public void TestCertificatesFilter()
		{
			GenRegCertAccredMaintList cert = Applicant1.Certificates.AddNew();
			cert.XZ_ParentTableCode = Applicant1.TablePrefix;
			cert.XZ_ParentID = Applicant1.PK;
			cert.XZ_Type = "CUS";
			cert.XZ_ExpiryOrDueDate = new ZDateTime(2017, 12, 31);
			cert.XZ_RefNumber = "12345";
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			ModuleTextFilter filterA = ((ModuleTextFilter)filterBizO["Certificate Number"]);
			filterA.IsActive = true;
			filterA.Property = "123";
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			filterA.IsActive = false;
			filterA.IsActive = true;
			filterA.Property = "4567";
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, Collection);
			filterA.IsActive = false;
			ModuleTextFilter filterB = ((ModuleTextFilter)filterBizO["Certificate Type"]);
			filterB.Property = "CUS";
			filterB.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			filterB.IsActive = false;
			ModuleDateFilter filterC = ((ModuleDateFilter)filterBizO["Certificate Expiry Date"]);
			filterC.Property1 = new ZDateTime(2017, 12, 10);
			filterC.Property2 = new ZDateTime(2017, 12, 30);
			filterC.IsActive = true;
			filterC.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, Collection);
			filterC.IsActive = false;
			filterC.IsActive = true;
			filterC.PropertySearch = new ZDateTime(2017, 12, 31).SqlFormat;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			filterC.IsActive = false;
		}

		public void TestCombiningCertificatesFilterShouldUseSubGroupQueries()
		{
			GenRegCertAccredMaintList cert = Applicant1.Certificates.AddNew();
			cert.XZ_ParentTableCode = Applicant1.TablePrefix;
			cert.XZ_ParentID = Applicant1.PK;
			cert.XZ_Type = "CUS";
			cert.XZ_ExpiryOrDueDate = new ZDateTime(2017, 12, 31);
			cert.XZ_RefNumber = "12345";
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			ModuleTextFilter filterA = ((ModuleTextFilter)filterBizO["Certificate Number"]);
			ModuleTextFilter filterB = ((ModuleTextFilter)filterBizO["Certificate Type"]);
			filterA.IsActive = true;
			filterB.IsActive = true;
			filterA.Property = "123";
			filterB.Property = "CUS";
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertEquals("HA_PK IN (SELECT XZ_ParentID FROM dbo.GenRegCertAccredMaintList WHERE XZ_ParentID IS NOT NULL AND XZ_Type = 'CUS' and XZ_RefNumber like '123%')", filterBizO.Filter.LiteralTextADO);
				//filterA.IsActive = false;
				//filterA.IsActive = true;
				filterA.Property = "4567";
				Collection.Load(filterBizO.Filter);
				AssertCollectionNotContains(Applicant1, Collection);
				AssertEquals("HA_PK IN (SELECT XZ_ParentID FROM dbo.GenRegCertAccredMaintList WHERE XZ_ParentID IS NOT NULL AND XZ_Type = 'CUS' and XZ_RefNumber like '4567%')", filterBizO.Filter.LiteralTextADO);
				//filterA.IsActive = false;
				ModuleDateFilter filterC = ((ModuleDateFilter)filterBizO["Certificate Expiry Date"]);
				filterC.Property1 = new ZDateTime(2017, 12, 10);
				filterC.Property2 = new ZDateTime(2017, 12, 30);
				filterC.IsActive = true;
				filterC.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				Collection.Load(filterBizO.Filter);
				AssertCollectionNotContains(Applicant1, Collection);
				AssertEquals("HA_PK IN (SELECT XZ_ParentID FROM dbo.GenRegCertAccredMaintList WHERE XZ_ParentID IS NOT NULL AND XZ_Type = 'CUS' and XZ_RefNumber like '4567%' and (XZ_ExpiryOrDueDate >= #2017-12-10 00:00:00.000# and XZ_ExpiryOrDueDate < #2017-12-31 00:00:00.000#))", filterBizO.Filter.LiteralTextADO);
				filterC.IsActive = false;
				filterC.IsActive = true;
				filterC.PropertySearch = new ZDateTime(2017, 12, 31).SqlFormat;
				Collection.Load(filterBizO.Filter);
				AssertCollectionNotContains(Applicant1, Collection);
				filterA.Property = "123";
				Collection.Load(filterBizO.Filter);
				AssertCollectionContains(Applicant1, Collection);
				filterA.IsActive = false;
				Collection.Load(filterBizO.Filter);
				AssertCollectionContains(Applicant1, Collection);
				filterA.IsActive = false;
				filterB.IsActive = false;
				filterC.IsActive = false;
			}
		}

		public void TestCertificatesIssueDateFilter()
		{
			GenRegCertAccredMaintList cert = Applicant1.Certificates.AddNew();
			cert.XZ_ParentTableCode = Applicant1.TablePrefix;
			cert.XZ_ParentID = Applicant1.PK;
			cert.XZ_IssueDate = ZDateTime.Today;
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			ModuleDateFilter dateFilter = ((ModuleDateFilter)filterBizO["Certificate Issue Date"]);
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.Future;
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, Collection);
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new ZDateTime(2016, 01, 20);
			dateFilter.Property2 = new ZDateTime(2016, 10, 01);
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, Collection);
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.NextCalendarMonth;
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, Collection);
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			dateFilter.IsActive = false;
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new ZDateTime(2016, 07, 01);
			dateFilter.Property2 = new ZDateTime(2050, 07, 01);
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			dateFilter.IsActive = false;
		}

		public void TestCampaignDetails()
		{
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicationCampaign1 = Applicant1.Applications.AddNew();
			applicationCampaign1.HP_HV = campaign1.PK;
			var applicationCampaign2 = Applicant2.Applications.AddNew();
			applicationCampaign2.HP_HV = campaign2.PK;
			Factory.Save();
			var filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			var filter = (ModuleGuidFilter)filterBizO[HRJobApplicantFilterProvider.FilterDescription.JobOpenings];
			Assert("This filter not supported on Web - yet", !filter.IsPublishedOnWeb);
			filter.Property = campaign1.PK;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			AssertCollectionNotContains(Applicant2, Collection);
			filter.Property = campaign2.PK;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, Collection);
			AssertCollectionContains(Applicant2, Collection);
			filter.Property = ZGuid.Empty;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			AssertCollectionContains(Applicant2, Collection);
			var campaign3 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			Factory.Save();
			filter.Property = campaign3.PK;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, Collection);
			AssertCollectionNotContains(Applicant2, Collection);
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
			var filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			var filter = (ModuleGuidFilter)filterBizO[HRJobApplicantFilterProvider.FilterDescription.JobRole];
			Assert("This filter not supported on Web - yet", !filter.IsPublishedOnWeb);
			filter.Property = jobRole1.PK;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			AssertCollectionNotContains(Applicant2, Collection);
			filter.Property = jobRole2.PK;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, Collection);
			AssertCollectionContains(Applicant2, Collection);
			filter.Property = ZGuid.Empty;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionContains(Applicant1, Collection);
			AssertCollectionContains(Applicant2, Collection);
			var jobRole3 = Factory.NewWithValidTestData<HRJobRole>();
			Factory.Save();
			filter.Property = jobRole3.PK;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(Applicant1, Collection);
			AssertCollectionNotContains(Applicant2, Collection);
		}

		public void TestJobRole_NullHV_HJ()
		{
			var jobRole = Factory.NewWithValidTestData<HRJobRole>();
			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign1.HV_HJ_JobRole = jobRole.PK;
			campaign2.HV_HJ_JobRole = ZGuid.Empty;
			var applicationCampaign1 = Applicant1.Applications.AddNew();
			applicationCampaign1.HP_HV = campaign1.PK;
			var applicationCampaign2 = Applicant2.Applications.AddNew();
			applicationCampaign2.HP_HV = campaign2.PK;
			Factory.Save();
			var filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			var filter = (ModuleGuidFilter)filterBizO[HRJobApplicantFilterProvider.FilterDescription.JobRole];
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.IsActive = true;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant2, Collection);
			AssertCollectionNotContains(Applicant1, Collection);
		}

		public void TestJobRole_IsBlankFilterShouldNotHaveValidationError()
		{
			var filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			var filter = (ModuleGuidFilter)filterBizO[HRJobApplicantFilterProvider.FilterDescription.JobRole];
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.IsActive = true;
			filter.Validation.ValidateComparisonOperator();

			Assert("JobRole|IsBlank filter should be accessible.", !filter.ComparisonOperatorInfo.HasErrors());
		}

		public void TestCountry()
		{
			var countries = Factory.Load<RefCountry>(new ZQuery());
			Applicant1.HA_RN_NKCountry = countries[0].Code;
			Applicant2.HA_RN_NKCountry = countries[1].Code;
			Factory.Save();
			var filter = (ModuleTextFilter)(GetNewFilterStripBusinessObject()["Country"]);
			filter.Property = countries[0].Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Applicant1, Collection);
			AssertCollectionNotContains(Applicant2, Collection);
			filter.Property = countries[1].Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Applicant1, Collection);
			AssertCollectionContains(Applicant2, Collection);
			filter.Property = ZString.Empty;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionContains(Applicant1, Collection);
			AssertCollectionContains(Applicant2, Collection);
			filter.Property = countries[2].Code;
			filter.IsActive = true;
			Collection.Load(filter.Query);
			AssertCollectionNotContains(Applicant1, Collection);
			AssertCollectionNotContains(Applicant2, Collection);
		}

		public void TestHasApplied()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = Applicant1.PK;
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			filterBizO["Has Applied"].IsActive = true;
			((ModuleFlagsFilter)filterBizO["Has Applied"]).Property0 = true;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
		}

		public void TestRegistrationMethod()
		{
			Applicant1.Logs.AddedLog.SL_GS_NKUser = User.WebUserCode;
			Applicant2.Logs.AddedLog.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			filterBizO["Registration Method"].IsActive = true;
			((ModuleFlagsFilter)filterBizO["Registration Method"])["Registered via Web"] = true;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
			((ModuleFlagsFilter)filterBizO["Registration Method"])["Registered via Web"] = false;
			((ModuleFlagsFilter)filterBizO["Registration Method"])["Registered manually"] = true;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant2, Collection);
			((ModuleFlagsFilter)filterBizO["Registration Method"])["Registered via Web"] = true;
			Collection.Load(filterBizO.Filter);
			AssertEquals(2, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
			AssertCollectionContains(Applicant2, Collection);
			((ModuleFlagsFilter)filterBizO["Registration Method"])["Registered via Web"] = false;
			((ModuleFlagsFilter)filterBizO["Registration Method"])["Registered manually"] = false;
			Collection.Load(filterBizO.Filter);
			AssertEquals(2, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
			AssertCollectionContains(Applicant2, Collection);
		}
		public void TestApplicantType()
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Applicant1.Logs.AddNew(AutoEvents.EditedARecord, "Related Contact");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			var filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			var filter = filterBizO["Applicant Type"] as ModuleFlagsFilter;
			filter.IsActive = false;
			Collection.Load(filterBizO.Filter);
			AssertEquals(2, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
			AssertCollectionContains(Applicant2, Collection);

			filter.IsActive = true;
			filter["Learning Center User"] = false;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);

			filter["Learning Center User"] = true;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant2, Collection);
		}

		public void TestHomePhone()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = Applicant1.PK;
			Applicant1.HA_HomePhone = "0249743938";
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			filterBizO["Home Phone"].IsActive = true;
			((ModuleTextFilter)filterBizO["Home Phone"]).Property = "0249743938";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
			((ModuleTextFilter)filterBizO["Home Phone"]).Property = "07777343938";
			Collection.Load(filterBizO.Filter);
			AssertEquals(0, Collection.Count);
			AssertCollectionNotContains(Applicant1, Collection);
		}

		public void TestFullNameAccentsExcluded()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_FullName = "Âbleton";
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			filterBizO["Full Name (Accents Excluded)"].IsActive = true;
			((ModuleTextFilter)filterBizO["Full Name (Accents Excluded)"]).Property = "Ableton";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(applicant, Collection);
			filterBizO["Full Name (Accents Excluded)"].IsActive = false;
			filterBizO["Full Name"].IsActive = true;
			((ModuleTextFilter)filterBizO["Full Name"]).Property = "Ableton";
			Collection.Load(filterBizO.Filter);
			AssertEquals(0, Collection.Count);
		}

		public void TestMobilePhone()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = Applicant1.PK;
			Applicant1.HA_MobilePhone = "0449743938";
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			filterBizO["Mobile Phone"].IsActive = true;
			((ModuleTextFilter)filterBizO["Mobile Phone"]).Property = "0449743938";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
			((ModuleTextFilter)filterBizO["Mobile Phone"]).Property = "07777343938";
			Collection.Load(filterBizO.Filter);
			AssertEquals(0, Collection.Count);
			AssertCollectionNotContains(Applicant1, Collection);
		}

		[TestDate(2015, 11, 12, 12, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestSubmissionTime()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = Applicant1.PK;
			application.HP_SubmissionTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			filterBizO["Submission Time"].IsActive = true;
			((ModuleDateFilter)filterBizO["Submission Time"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filterBizO["Submission Time"]).Property1 = ZDateTime.UtcNow;
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
			((ModuleDateFilter)filterBizO["Submission Time"]).Property1 = ZDateTime.UtcNow.AddDays(2);
			((ModuleDateFilter)filterBizO["Submission Time"]).Property2 = ZDateTime.UtcNow.AddDays(3);
			Collection.Load(filterBizO.Filter);
			AssertEquals(0, Collection.Count);
			AssertCollectionNotContains(Applicant1, Collection);
		}

		public void TestApplicationStatus()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = Applicant1.PK;
			application.HP_CurrentStatus = "INP";
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			filterBizO["Application Status"].IsActive = true;
			((ModuleTextFilter)filterBizO["Application Status"]).Property = "ST1";
			Collection.Load(filterBizO.Filter);
			AssertEquals(0, Collection.Count);
			AssertCollectionNotContains(Applicant1, Collection);
			((ModuleTextFilter)filterBizO["Application Status"]).Property = "INP";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
		}

		public void TestWorkPermitStatus()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = Applicant1.PK;
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			filterBizO["Work Permit Status"].IsActive = true;
			((ModuleTextFilter)filterBizO["Work Permit Status"]).Property = "SKL";
			Collection.Load(filterBizO.Filter);
			AssertEquals(0, Collection.Count);
			AssertCollectionNotContains(Applicant1, Collection);
			((ModuleTextFilter)filterBizO["Work Permit Status"]).Property = "RES";
			Collection.Load(filterBizO.Filter);
			AssertEquals(2, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
		}

		public void TestAvailabilities()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = Applicant1.PK;
			Applicant1.HA_Availability = "FUL";
			Factory.Save();
			HRJobApplicantFilterBusinessObject filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
			filterBizO["Availability"].IsActive = true;
			((ModuleTextFilter)filterBizO["Availability"]).Property = "CAS";
			Collection.Load(filterBizO.Filter);
			AssertEquals(0, Collection.Count);
			AssertCollectionNotContains(Applicant1, Collection);
			((ModuleTextFilter)filterBizO["Availability"]).Property = "FUL";
			Collection.Load(filterBizO.Filter);
			AssertEquals(1, Collection.Count);
			AssertCollectionContains(Applicant1, Collection);
		}

		public void TestResumeKeywordsFilter()
		{
			using (GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.SetTemporaryValue(default, default, default, false))
			{
				var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
				var application1 = campaign.Applications.AddNew();
				application1.HP_HA = Applicant1.PK;
				var doc1 = application1.Documents.AddNew();
				doc1.HPD_Content = "<Resume><NonXMLResume><TextResume>KW1<span></span><span>KW2</span><span>KW3</span><span>KW4 KW5</span></TextResume></NonXMLResume></Resume>";
				var application2 = campaign.Applications.AddNew();
				application2.HP_HA = Applicant2.PK;
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
				var filterBizO = (HRJobApplicantFilterBusinessObject)CachedBusinessObject;
				var filter = filterBizO[HRJobApplicantFilterProvider.FilterDescription.ResumeKeywords] as ModuleTextFilter;
				filter.IsActive = true;
				filter.Property = "";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Applicant1, Applicant2, applicant3, applicant4 });
				filter.Property = "@@@@@";
				Collection.Load(filterBizO.Filter);
				AssertEquals(0, Collection.Count);
				filter.Property = "ABC KW3 DEF";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Applicant1, applicant3 });
				filter.Property = "XYZ Keyword3 TTT";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Applicant2, applicant3 });
				filter.Property = "AAA \"CC OO\" DDD \"KW3 Keyword3\" CCC";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(Collection, new[] { applicant3 });
				filter.Property = "KW1 Keyword1";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Applicant1, Applicant2 });
				filter.Property = "KW3 Keyword3";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(Collection, new[] { Applicant1, Applicant2, applicant3 });
				filter.Property = "\"DON'T QuOtE ME.\"";
				Collection.Load(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(Collection, new[] { applicant3 });
			}
		}

		public void TestSensitiveFiltersWithSecurityRights()
		{
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				AssertSensitiveFiltersWithSecurityRights(security.HRJobApplicantViewMobilePhone, HRJobApplicantFilterBusinessObject.FilterDescription.MobilePhone);
				AssertSensitiveFiltersWithSecurityRights(security.HRJobApplicantViewHomePhone, HRJobApplicantFilterBusinessObject.FilterDescription.HomePhone);
				AssertSensitiveFiltersWithSecurityRights(security.HRJobApplicantViewName, HRJobApplicantFilterProvider.FilterDescription.FullName);
				AssertSensitiveFiltersWithSecurityRights(security.HRJobApplicantViewName, "Full Name (Accents Excluded)");
				AssertSensitiveFiltersWithSecurityRights(security.HRJobApplicantViewNationality, HRJobApplicantFilterProvider.FilterDescription.Country);
				AssertSensitiveFiltersWithSecurityRights(security.HRJobApplicantViewUserAddress, HRJobApplicantFilterProvider.FilterDescription.City);
				AssertSensitiveFiltersWithSecurityRights(security.HRJobApplicantViewUserAddress, HRJobApplicantFilterProvider.FilterDescription.State);
			}
		}

		#endregion
		#region Implementation
		protected HRJobApplicant Applicant1, Applicant2;
		protected HRJobApplicantCollection Collection;
		protected override void SetUp()
		{
			base.SetUp();
			Collection = new HRJobApplicantCollection(Factory);
			Applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			Applicant1.HA_WorkPermitStatus = Core.Constants.WorkPermitStatuses.Residence;
			Applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			Applicant2.HA_WorkPermitStatus = Core.Constants.WorkPermitStatuses.Residence;
			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new HRJobApplicantFilterBusinessObject();
		}
		#endregion
	}
}
