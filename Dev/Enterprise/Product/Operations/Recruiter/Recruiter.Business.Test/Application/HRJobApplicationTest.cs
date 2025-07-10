using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplication))]
	sealed class HRJobApplicationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetCurrentTask()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			var t1 = application.WorkflowItems.Tasks.AddNew();
			t1.FillWithValidTestData();
			t1.P9_Description = "T1";
			t1.P9_Status = "CLS";

			var t2 = application.WorkflowItems.Tasks.AddNew();
			t2.FillWithValidTestData();
			t2.P9_Description = "T2";
			t2.P9_Status = "CAN";

			var t3 = application.WorkflowItems.Tasks.AddNew();
			t3.FillWithValidTestData();
			t3.P9_Description = "T3";
			t3.P9_Status = "ASN";

			var t4 = application.WorkflowItems.Tasks.AddNew();
			t4.FillWithValidTestData();
			t4.P9_Description = "T4";
			t4.P9_Status = "ASN";

			_ = (new[] { t1, t2, t3, t4 }).Select((t, i) => t.P9_Sequence = i).ToList();

			AssertEquals("T3", application.CurrentTask.P9_Description);
		}

		public void TestSetDefaultCountryForApplicant()
		{
			var application = Factory.New<HRJobApplication>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RL_NKRelatedPortCode = "UAIEV";
			var otherAddress = org.Addresses.AddNew();
			otherAddress.OA_RL_NKRelatedPortCode = "GBLON";

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "Batman";
			opening.HV_CampaignStartDate = new ZDateTime(2019, 2, 1); //email date is 13.03.2019
			opening.HV_CampaignEndDate = new ZDateTime(2019, 3, 1);
			opening.HV_HJ_JobRole = jobRole.PK;
			opening.HV_OH_ClientAccount = org.PK;

			var applicant = Factory.New<HRJobApplicant>();

			application.HP_HV = opening.PK;
			application.HP_HA = applicant.PK;
			AssertEquals("UA", applicant.HA_RN_NKCountry);

			opening.HV_OA_ClientAddress = otherAddress.PK;
			applicant.HA_RN_NKCountry = "";
			application.HP_HV = ZGuid.Empty;
			application.HP_HV = opening.PK;
			AssertEquals("GB", applicant.HA_RN_NKCountry);
		}

		public void TestApplicantCollection()
		{
			var application = Factory.New<HRJobApplication>();
			var applicant1 = Factory.New<HRJobApplicant>();
			var applicant2 = Factory.New<HRJobApplicant>();

			AssertNotNull(application.ApplicantCollection);
			AssertEquals(0, application.ApplicantCollection.Count);

			application.HP_HA = applicant1.PK;
			AssertEquals(1, application.ApplicantCollection.Count);
			AssertEquals(applicant1, application.ApplicantCollection[0]);
			AssertEquals(applicant1, application.Applicant);

			application.HP_HA = applicant2.PK;
			AssertEquals(1, application.ApplicantCollection.Count);
			AssertEquals(applicant2, application.ApplicantCollection[0]);
			AssertEquals(applicant2, application.Applicant);
		}

		public void TestInterviews()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			HRJobApplicationInterview interview = Factory.New<HRJobApplicationInterview>();

			interview.HI_HP = application.PK;
			AssertCollectionContains("Interview collection on Application should contain Interview", interview, application.Interviews);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			HRJobApplication application = applicant.Applications.AddNew();

			AssertEquals("Should be 0 business objects with related logs", 0, application.BusinessObjectsWithRelatedEvents.Length);
			HRJobApplicationInterview interview = application.Interviews.AddNew();
			AssertEquals("Should be 1 business object with related logs", 1, application.BusinessObjectsWithRelatedEvents.Length);
		}

		public void TestDelete_Interview()
		{
			var applicant = Factory.New<HRJobApplicant>();
			var application = applicant.Applications.AddNew();
			var interview = application.Interviews.AddNew();
			application.Delete();

			Assert("Interview should be deleted", interview.IsDeleted);
		}

		public void TestDelete_ParsingQueue()
		{
			var applicant = Factory.New<HRJobApplicant>();
			var application = applicant.Applications.AddNew();
			var queue = Factory.NewWithValidTestData<HRJobApplicationParsingQueue>();
			queue.HPQ_HP = application.PK;
			application.Delete();

			Assert("Parsing queue should be deleted", queue.IsDeleted);
		}

		public void TestIsAutologged()
		{
			HRJobApplication application = Factory.NewWithValidTestData<HRJobApplication>();
			Factory.Save();
			Assert("Application should contain logs", application.Logs.GetAllLogs().Count > 0);
		}

		public void TestFillWithValidTestDataCore()
		{
			HRJobApplication application = Factory.NewWithValidTestData<HRJobApplication>();
			Assert("Application should be for a valid campaign", application.HP_HV.IsValid);
			Assert("Application should be from a valid applicant", application.HP_HA.IsValid);
		}

		public void TestCampaign()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			AssertNull("Campaign should not be set", application.JobOpening);

			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			application.HP_HV = campaign.PK;

			AssertEquals("Campaign should be set", campaign, application.JobOpening);
		}

		public void TestApplicant()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			AssertNull("Applicant should not be set", application.Applicant);

			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			application.HP_HA = applicant.PK;

			AssertEquals("Applicant should be set", applicant, application.Applicant);
		}

		public void TestOnGetCustomLogReference()
		{
			HRJobApplicant applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Name";

			HRRecruitmentJobCampaign campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign1.HV_AdTitle = "Campaign1";

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "Staff";

			HRJobApplication application = Factory.New<HRJobApplication>();

			application.HP_HA = applicant1.PK;
			application.HP_HV = campaign1.PK;
			application.HP_GS_NKAssignedTo = staff1.GS_Code;

			Factory.Save();

			AssertEquals("There should be one log", 1, application.Logs.GetAllLogs().Count);
			ZString expectedLogText = "Application from " + applicant1.HA_FullName + " for Campaign "
				+ campaign1.HV_AdTitle + " Assigned To " + staff1.GS_FullName;
			AssertEquals(expectedLogText, application.Logs.GetAllLogs()[0].SL_Reference);

			application.HP_JobExperienceYears = 9;

			Factory.Save();

			AssertEquals("There should be two logs", 2, application.Logs.GetAllLogs().Count);
			expectedLogText = "Application from " + applicant1.HA_FullName + " for Campaign "
				+ campaign1.HV_AdTitle;
			AssertEquals(expectedLogText, application.Logs.GetAllLogs()[1].SL_Reference);
		}

		public void TestImplementsIDocManagerSupport()
		{
			var application = Factory.New<HRJobApplication>();
			Assert("it is an IDocManagerSupport", typeof(IDocManagerSupport).IsAssignableFrom(typeof(HRJobApplication)));
			AssertEquals("HRRecruitmentJobCampaign DocManagerCode = CAM", Core.Constants.DocManagerCodes.JobApplication, application.DocManagerInfo.DocManagerCode);
			AssertEquals("HRJobApplication.DocManagerInfo is HRJobApplicationDocManagerInfo", typeof(HRJobApplicationDocManagerInfo), application.DocManagerInfo.GetType());
		}

		public void TestJobCampaignTitle()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			Assert("If no campaign title is empty", application.JobCampaignTitle.IsEmpty);

			HRRecruitmentJobCampaign jobCampaign = Factory.New<HRRecruitmentJobCampaign>();
			jobCampaign.HV_AdTitle = "Smart Developer";
			application.HP_HV = jobCampaign.PK;
			AssertEquals("JobCampaignTitle", "Smart Developer", application.JobCampaignTitle);
		}

		[TestDate(2013, 1, 15, 6, 20, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestAppliedDate()
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = applicant.Applications.AddNew();
			application.HP_HV = campaign.PK;
			Factory.Save();

			AssertEquals(new ZDateTime(2013, 1, 15, 17, 20, 0), application.AppliedDate);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RL_NKRelatedPortCode = "HKHKG";
			campaign.HV_OA_ClientAddress = org.MainAddress.PK;
			Factory.Save();

			//Env.Time.GetUnlocoTimeFromUtc() method has a test only block that relies on TestUtcOffset attribute and ignores the unloco passed in
			AssertEquals(new ZDateTime(2013, 1, 15, 17, 20, 0), application.AppliedDate);
		}

		[TestDate(2015, 11, 9, 10, 20, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestSubmissionTime()
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = applicant.Applications.AddNew();
			application.HP_HV = campaign.PK;
			Factory.Save();

			AssertEquals(new ZDateTime(2015, 11, 9, 21, 20, 0), application.SubmissionTimeLocal);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RL_NKRelatedPortCode = "HKHKG";
			campaign.HV_OA_ClientAddress = org.MainAddress.PK;
			Factory.Save();

			AssertEquals(new ZDateTime(2015, 11, 9, 21, 20, 0), application.SubmissionTimeLocal);

			application.SubmissionTimeLocal = ZDateTime.Now.AddHours(1);
			AssertEquals(new ZDateTime(2015, 11, 9, 11, 20, 0), application.HP_SubmissionTimeUtc);
		}

		public void TestCreateFromTemplateWorkflowTasks()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "HRA";
			template1.P0_SubType1 = "AU";
			var templateTask1 = template1.WorkflowItems.AddNew();
			templateTask1.P9_Description = "Task One for AU";

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "HRA";
			template2.P0_SubType1 = "NZ";
			var templateTask2 = template2.WorkflowItems.AddNew();
			templateTask2.P9_Description = "Task One for NZ";

			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application = Factory.New<HRJobApplication>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "name";
			applicant.HA_RN_NKCountry = "AU";
			application.HP_HA = applicant.PK;
			application.HP_HV = campaign.PK;

			Factory.Save();

			//AssertEquals(GlbTrainingCourseLookups.LocaleConstants.OnSite, trainingCourse.G2_Locale);
			AssertEquals(0, application.WorkflowItems.Count);

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, application.WorkflowItems.Count);
		}

		public void TestApplicationOverallRatingDescription()
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application = Factory.New<HRJobApplication>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			application.HP_HA = applicant.PK;
			application.HP_HV = campaign.PK;
			Factory.Save();

			AssertEquals("-1", application.HP_ApplicationOverallRating);
			AssertEquals("Unrated", application.ApplicationOverallRatingDescription);

			application.HP_ApplicationOverallRating = "0";
			AssertEquals("0", application.HP_ApplicationOverallRating);
			AssertEquals("", application.ApplicationOverallRatingDescription);

			application.HP_ApplicationOverallRating = "1";
			AssertEquals("1", application.HP_ApplicationOverallRating);
			AssertEquals("Suitable", application.ApplicationOverallRatingDescription);

			application.HP_ApplicationOverallRating = "5";
			AssertEquals("5", application.HP_ApplicationOverallRating);
			AssertEquals("", application.ApplicationOverallRatingDescription);

			application.HP_ApplicationOverallRating = "6";
			AssertEquals("6", application.HP_ApplicationOverallRating);
			AssertEquals("", application.ApplicationOverallRatingDescription);

			//

			application.ApplicationOverallRatingDescription = "";
			AssertEquals("", application.HP_ApplicationOverallRating);
			AssertEquals("", application.ApplicationOverallRatingDescription);

			application.ApplicationOverallRatingDescription = "1";
			AssertEquals("1", application.HP_ApplicationOverallRating);
			AssertEquals("Suitable", application.ApplicationOverallRatingDescription);

			application.ApplicationOverallRatingDescription = "Suitable";
			AssertEquals("1", application.HP_ApplicationOverallRating);
			AssertEquals("Suitable", application.ApplicationOverallRatingDescription);

			application.ApplicationOverallRatingDescription = "Potential";
			AssertEquals("2", application.HP_ApplicationOverallRating);
			AssertEquals("Potential", application.ApplicationOverallRatingDescription);

			application.ApplicationOverallRatingDescription = "Unsuitable";
			AssertEquals("3", application.HP_ApplicationOverallRating);
			AssertEquals("Unsuitable", application.ApplicationOverallRatingDescription);

			application.ApplicationOverallRatingDescription = "!@#";
			AssertEquals("", application.HP_ApplicationOverallRating);
			AssertEquals("!@#", application.ApplicationOverallRatingDescription);
		}

		public void TestHP_SourceType()
		{
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var application = Factory.New<HRJobApplication>();
			var org = Factory.New<OrgHeader>();
			var person = Factory.New<GlbPerson>();
			AssertEquals(ReferringSourcesTypes.Codes.NoReferrerRegistered, application.HP_SourceType);
			AssertEquals("", application.ReferringStaffCode);
			AssertEquals(ZGuid.Empty, application.HP_OH_ReferringOrganisation);
			AssertEquals(ZGuid.Empty, application.HP_PER_ReferringPerson);

			application.HP_OH_ReferringOrganisation = org.PK;
			application.HP_PER_ReferringPerson = person.PK;
			application.HP_SourceType = ReferringSourcesTypes.Codes.Website;
			AssertEquals("", application.ReferringStaffCode);
			AssertEquals(ZGuid.Empty, application.HP_OH_ReferringOrganisation);
			AssertEquals(person.PK, application.HP_PER_ReferringPerson);

			application.HP_SourceType = ReferringSourcesTypes.Codes.NoReferrerRegistered;
			application.HP_OH_ReferringOrganisation = org.PK;
			application.HP_PER_ReferringPerson = person.PK;
			application.HP_SourceType = ReferringSourcesTypes.Codes.StaffReferral;
			AssertEquals("", application.ReferringStaffCode);
			AssertEquals(ZGuid.Empty, application.HP_OH_ReferringOrganisation);
			AssertEquals(ZGuid.Empty, application.HP_PER_ReferringPerson);
		}

		public void TestReferringStaffCode()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST9";
			Factory.Save();

			application.HP_SourceType = ReferringSourcesTypes.Codes.StaffReferral;
			application.ReferringStaffCode = "ST9";
			AssertEquals("ST9", application.ReferringStaffCode);
			AssertEquals(staff.GS_PER, application.HP_PER_ReferringPerson);

			application.ReferringStaffCode = "ST0";
			AssertEquals("ST0", application.ReferringStaffCode);
			AssertEquals(ZGuid.Empty, application.HP_PER_ReferringPerson);

			application.ReferringStaffCode = "";
			AssertEquals("", application.ReferringStaffCode);
			AssertEquals(ZGuid.Empty, application.HP_PER_ReferringPerson);

			application.HP_PER_ReferringPerson = staff.GS_PER;
			AssertEquals("ST9", application.ReferringStaffCode);
			AssertEquals(staff.GS_PER, application.HP_PER_ReferringPerson);

			application.HP_SourceType = ReferringSourcesTypes.Codes.NoReferrerRegistered;
			application.HP_PER_ReferringPerson = staff.GS_PER;
			AssertEquals("", application.ReferringStaffCode);
			AssertEquals(true, application.ReferringStaffCode_ReadOnly);
			AssertEquals(false, application.IsReferringStaffApplicable);
			AssertEquals(staff.GS_PER, application.HP_PER_ReferringPerson);
		}

		public void TestHelperMethods()
		{
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_SourceType = ReferringSourcesTypes.Codes.NoReferrerRegistered;

			AssertEquals(true, application.IsReferringOrganisationApplicable);
			AssertEquals(false, application.HP_OH_ReferringOrganisation_ReadOnly);
			AssertEquals(false, application.IsReferringOrganisationMandatory);
			AssertEquals(true, application.IsReferringPersonApplicable);
			AssertEquals(false, application.HP_PER_ReferringPerson_ReadOnly);
			AssertEquals(false, application.IsReferringStaffApplicable);
			AssertEquals(true, application.ReferringStaffCode_ReadOnly);

			application.HP_SourceType = ReferringSourcesTypes.Codes.RecruitmentAgent;
			AssertEquals(true, application.IsReferringOrganisationApplicable);
			AssertEquals(false, application.HP_OH_ReferringOrganisation_ReadOnly);
			AssertEquals(true, application.IsReferringOrganisationMandatory);
			AssertEquals(true, application.IsReferringPersonApplicable);
			AssertEquals(false, application.HP_PER_ReferringPerson_ReadOnly);
			AssertEquals(false, application.IsReferringStaffApplicable);
			AssertEquals(true, application.ReferringStaffCode_ReadOnly);

			application.HP_SourceType = ReferringSourcesTypes.Codes.StaffReferral;
			AssertEquals(false, application.IsReferringOrganisationApplicable);
			AssertEquals(true, application.HP_OH_ReferringOrganisation_ReadOnly);
			AssertEquals(false, application.IsReferringOrganisationMandatory);
			AssertEquals(false, application.IsReferringPersonApplicable);
			AssertEquals(true, application.HP_PER_ReferringPerson_ReadOnly);
			AssertEquals(true, application.IsReferringStaffApplicable);
			AssertEquals(false, application.ReferringStaffCode_ReadOnly);
		}

		public void TestSetDefaultValues()
		{
			var application = Factory.New<HRJobApplication>();
			AssertEquals(ReferringSourcesTypes.Codes.NoReferrerRegistered, application.HP_SourceType);
		}

		public void TestSetupReferringSource()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "RC1";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.GS_EmailAddress = "Recruiter@cw1.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			staff2.GS_EmailAddress = "2@cw1.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG_TEST1";

			group.Staff.Add(staff1);
			Factory.Save();

			RecruiterDataRegistry.Instance.ReferringStaffMembersToIgnoreGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			var configs = new ReferringPartyConfigurationCollection();
			var config1 = configs.AddNew();
			config1.Domain = "2@cw1.com";
			config1.ReferringParty = "GS";
			config1.DefaultReferringSource = "STF";

			var config2 = configs.AddNew();
			config2.Domain = "@org.com";
			config2.ReferringParty = "OH";
			config2.DefaultReferringSource = "AGT";
			config2.OrganizationPK = org.PK;

			RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configs);

			var application = Factory.New<HRJobApplication>();
			application.HP_SourceType = "";
			application.SetupReferringSource("Recruiter@cw1.com");
			AssertEquals("", application.HP_SourceType);

			application.SetupReferringSource("2@cw1.com");
			AssertEquals("STF", application.HP_SourceType);
			AssertEquals(staff2.GS_PER, application.HP_PER_ReferringPerson);

			application.SetupReferringSource("user@org.com");
			AssertEquals("AGT", application.HP_SourceType);
			AssertEquals(org.PK, application.HP_OH_ReferringOrganisation);
		}

		public void TestIsReferringOrganisationDropDown()
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

			application.HP_SourceType = ReferringSourcesTypes.Codes.StaffReferral;
			AssertEquals(false, application.IsReferringOrganisationDropDown);

			application.HP_SourceType = ReferringSourcesTypes.Codes.RecruitmentAgent;
			AssertEquals(true, application.IsReferringOrganisationDropDown);
		}

		public void TestApplicationNumber()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Name";

			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			campaign1.HV_AdTitle = "Campaign1";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "Staff";

			var application = Factory.New<HRJobApplication>();

			application.HP_HA = applicant1.PK;
			application.HP_HV = campaign1.PK;
			application.HP_GS_NKAssignedTo = staff1.GS_Code;

			AssertEquals("", application.HP_ApplicationNumber);

			Factory.Save();

			Assert(application.HP_ApplicationNumber.StartsWith("JA00"));
		}

		public void TestCodeProperty()
		{
			var codeProperty = typeof(HRJobApplication).GetCustomAttributes(typeof(CodePropertyAttribute), true).OfType<CodePropertyAttribute>().First();
			AssertEquals("Should be 'HP_ApplicationNumber' for Allocate eDocs", AutoHRJobApplication.Schema.HP_ApplicationNumber, codeProperty.PropertyName);
		}

		public void TestHumanReadableShortcutNameCore()
		{
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Applicant Name";

			var campaign = Factory.New<HRRecruitmentJobCampaign>();
			campaign.HV_AdTitle = "Campaign Title";

			var application = Factory.New<HRJobApplication>();
			application.HP_ApplicationNumber = "JA0000001";

			AssertEquals("", application.HumanReadableShortcutName);

			application.HP_HA = applicant.PK;
			AssertEquals("Applicant Name", application.HumanReadableShortcutName);

			application.HP_HV = campaign.PK;
			AssertEquals("Applicant Name - Campaign Title", application.HumanReadableShortcutName);
		}

		#region IHaveRequiredDocuments

		public void TestAddAndDeleteRequiredDocuments()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			var doc = application.RequiredDocuments.AddNew();
			application.RequiredDocuments.AddNew();
			application.RequiredDocuments.AddNew();
			Factory.Save();

			AssertEquals("Should have 3 required documents", 3, application.RequiredDocuments.Count);
			AssertEquals("Required documents of the object should exist", 3, new BusinessObjectFactory().Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, application.PK)).Length);
			AssertNotNull("Specific required document should exist", new BusinessObjectFactory().Load<JobRequiredDocument>(doc.PK));

			application.RequiredDocuments.RemoveAndDelete(doc);
			Factory.Save();
			AssertEquals("Should have 2 required documents", 2, application.RequiredDocuments.Count);
			AssertNull("Deleted required document should not exist", new BusinessObjectFactory().Load<JobRequiredDocument>(doc.PK));

			application.Delete();
			Factory.Save();

			AssertEquals("Required documents of deleted object should not exist", 0, new BusinessObjectFactory().Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, application.PK)).Length);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<HRJobApplication>();
		}

		#endregion

		public void TestHRJobApplicationLogEvents_QueueAndCancelRejectionEmail()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.New<HRJobApplication>();
			application.HP_HA = applicant.PK;

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			application.HP_ApplicationOverallRating = "3";

			Factory.Save();

			var expectedWarningMessage = @"Would you like to send an automated rejection email? Yes/no";
			AssertEquals("Should ask user whether or not to send a rejection email.",
				expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Message type is Question.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			Assert("Response is not defaultable.", !UnitTestUserNotification.Instance.LastMessage.WasDefaultable);

			var logs = application.Logs.Find(l => l.SL_Parent == application.PK);
			var rejectionLogs = logs.Where(l => l.Event.SE_Code == AutoEvents.RejectionEmailQueued.Code);

			AssertEquals(1, rejectionLogs.Count());

			var log = rejectionLogs.First();
			AssertEquals(false, log.IsCancelled);

			application.HP_ApplicationOverallRating = "0";

			Factory.Save();

			logs = application.Logs.Find(l => l.SL_Parent == application.PK);
			rejectionLogs = logs.Where(l => l.Event.SE_Code == AutoEvents.RejectionEmailQueued.Code);

			AssertEquals(true, log.IsCancelled);
			AssertEquals(1, rejectionLogs.Count());
		}
	}
}
