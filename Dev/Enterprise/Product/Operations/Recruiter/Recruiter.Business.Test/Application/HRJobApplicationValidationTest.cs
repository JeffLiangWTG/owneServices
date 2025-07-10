using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobApplicationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAssignedTo()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "GS";
			HRJobApplication application = Factory.New<HRJobApplication>();
			application.HP_GS_NKAssignedTo = "--";
			AssertHasErrors("Staff member with code --- does not exist, should have error", application.HP_GS_NKAssignedToInfo);
			application.HP_GS_NKAssignedTo = staff.GS_Code;
			AssertNoErrors("Staff member with this code exists, should not have error", application.HP_GS_NKAssignedToInfo);
		}

		public void TestCheckHP_HA()
		{
			var applicant1 = Factory.New<HRJobApplicant>();
			var applicant2 = Factory.New<HRJobApplicant>();
			var campaign = Factory.New<HRRecruitmentJobCampaign>();

			var application1 = campaign.Applications.AddNew();
			application1.HP_HA = applicant1.PK;

			AssertHasErrors("Application1 Please enter a Full Name.", application1.HP_HAInfo);
			applicant1.HA_FullName = "Name Applicant 1";
			application1.Validation.ValidateAll();

			AssertNoErrors("Application1 Applicant should not have errors", application1.HP_HAInfo);

			var application2 = campaign.Applications.AddNew();
			application2.HP_HA = applicant1.PK;

			AssertHasErrors("Application2 Applicant should have errors because an application for this Applicant already exists", application2.HP_HAInfo);

			applicant2.HA_FullName = "Name Applicant 2";
			application2.HP_HA = applicant2.PK;
			AssertNoErrors("Application1 for Applicant1 should not have errors", application1.HP_HAInfo);
			AssertNoErrors("Application2 is for Applicant2, should not have errors", application2.HP_HAInfo);
		}

		public void TestCheckHP_HA_NoOpening()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = ZGuid.Empty;

			AssertNoErrors("Application1 Applicant should not have errors", application1.HP_HAInfo);

			var application2 = applicant.Applications.AddNew();
			application2.HP_HV = ZGuid.Empty;

			AssertNoErrors("Application2 Applicant should not have errors because duplications on null campaign are allowed", application2.HP_HAInfo);
		}

		public void TestCheckHP_HV()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			HRRecruitmentJobCampaign campaign1 = Factory.New<HRRecruitmentJobCampaign>();
			HRRecruitmentJobCampaign campaign2 = Factory.New<HRRecruitmentJobCampaign>();

			HRJobApplication application1 = applicant.Applications.AddNew();
			application1.HP_HV = campaign1.PK;

			AssertNoErrors("Application1 Campaign should not have errors", application1.HP_HVInfo);

			HRJobApplication application2 = applicant.Applications.AddNew();
			application2.HP_HV = campaign1.PK;

			AssertHasErrors("Application2 Campaign should have errors because an application for this campaign already exists", application2.HP_HVInfo);

			application2.HP_HV = campaign2.PK;
			AssertNoErrors("Application1 for Campaign1 should not have errors", application1.HP_HVInfo);
			AssertNoErrors("Application2 is for Campaign2, should not have errors", application2.HP_HVInfo);
		}

		public void TestCheckHP_HV_NoOpening()
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = ZGuid.Empty;

			AssertNoErrors("Application1 with no assosciated campaign should have no errors", application1.HP_HVInfo);

			var application2 = applicant.Applications.AddNew();
			application2.HP_HV = campaign.PK;
			application2.HP_HV = ZGuid.Empty;

			AssertNoErrors("Application2 should not have errors because duplications on null campaign are allowed", application2.HP_HVInfo);
		}

		public void TestCheckHP_CurrentStatus()
		{
			ApplicationStatusCollection list = new ApplicationStatusCollection();
			list.AddPair("***", "Application Status");
			RecruiterDataRegistry.Instance.ApplicationStatuses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			HRJobApplication application = Factory.New<HRJobApplication>();
			application.HP_CurrentStatus = "***";
			AssertNoErrors("Application Status is valid, should not have errors", application.HP_CurrentStatusInfo);
			application.HP_CurrentStatus = ";;;";
			AssertHasErrors("Application Status is NOT valid, should have errors", application.HP_CurrentStatusInfo);
		}

		public void TestCheckHP_ApplicationOverallRating()
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application = Factory.New<HRJobApplication>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			application.HP_HA = applicant.PK;
			application.HP_HV = campaign.PK;

			application.HP_ApplicationOverallRating = "1";
			AssertNoErrors(application.HP_ApplicationOverallRatingInfo);
			application.HP_ApplicationOverallRating = "2";
			AssertNoErrors(application.HP_ApplicationOverallRatingInfo);
			application.HP_ApplicationOverallRating = "3";
			AssertNoErrors(application.HP_ApplicationOverallRatingInfo);
			application.HP_ApplicationOverallRating = "-1";
			AssertNoErrors(application.HP_ApplicationOverallRatingInfo);

			application.HP_ApplicationOverallRating = "";
			AssertNoErrors(application.HP_ApplicationOverallRatingInfo);
			application.HP_ApplicationOverallRating = "4";
			AssertHasError(application.HP_ApplicationOverallRatingInfo, "Enter a valid Overall Rating.");
			application.HP_ApplicationOverallRating = "@";
			AssertHasError(application.HP_ApplicationOverallRatingInfo, "Enter a valid Overall Rating.");

			Factory.Save();
			application.RunPreSaveValidation();
			AssertNoErrors(application.HP_ApplicationOverallRatingInfo);
			application.HP_ApplicationOverallRating = "1";
			AssertNoErrors(application.HP_ApplicationOverallRatingInfo);
			application.HP_ApplicationOverallRating = "@@";
			AssertHasError(application.HP_ApplicationOverallRatingInfo, "Enter a valid Overall Rating.");
		}

		public void TestCheckApplicationOverallRatingDescription()
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application = Factory.New<HRJobApplication>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			application.HP_HA = applicant.PK;
			application.HP_HV = campaign.PK;

			application.ApplicationOverallRatingDescription = "";
			AssertNoErrors(application.ApplicationOverallRatingDescriptionInfo);
			application.ApplicationOverallRatingDescription = "1";
			AssertNoErrors(application.ApplicationOverallRatingDescriptionInfo);
			application.ApplicationOverallRatingDescription = "@";
			AssertHasError(application.ApplicationOverallRatingDescriptionInfo, "Please enter a valid value");

			application.ApplicationOverallRatingDescription = "";
			application.HP_ApplicationOverallRating = "@";
			Factory.Save();
			application.RunPreSaveValidation();
			AssertNoErrors(application.ApplicationOverallRatingDescriptionInfo);
			application.ApplicationOverallRatingDescription = "1";
			AssertNoErrors(application.ApplicationOverallRatingDescriptionInfo);
			application.ApplicationOverallRatingDescription = "@";
			AssertHasError(application.ApplicationOverallRatingDescriptionInfo, "Please enter a valid value");
		}

		public void TestCheckHP_SourceType()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_SourceType = "";
			Factory.Save();

			application.RunPreSaveValidation();
			AssertNoErrors(application.HP_SourceTypeInfo);

			application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_SourceType = "@@@";
			AssertHasError(application.HP_SourceTypeInfo, "Enter a valid selection.");
			application.HP_SourceType = "";
			AssertHasError(application.HP_SourceTypeInfo, "Please enter a value.");
			application.HP_SourceType = "WEB";
			AssertNoErrors(application.HP_SourceTypeInfo);
		}

		public void TestCheckHP_OH_ReferringOrganisation()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_SourceType = "MAN";
			application.HP_OH_ReferringOrganisation = ZGuid.Empty;
			application.RunPreSaveValidation();
			AssertNoErrors(application.HP_OH_ReferringOrganisationInfo);

			application.HP_SourceType = "AGT";
			application.HP_OH_ReferringOrganisation = ZGuid.Empty;
			application.RunPreSaveValidation();
			AssertHasError(application.HP_OH_ReferringOrganisationInfo, "Please enter a value.");
		}

		public void TestCheckReferringStaffCode()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST9";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST8";
			Factory.Save();

			TestConnection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_PER = null, GS_IsSystemAccount = 1, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_Code = 'ST8';");

			application = new BusinessObjectFactory().Load<HRJobApplication>(application.PK);
			application.HP_SourceType = "STF";
			application.ReferringStaffCode = "ST9";
			AssertNoErrors(application.ReferringStaffCodeInfo);

			application.ReferringStaffCode = "ST0";
			AssertHasError(application.ReferringStaffCodeInfo, "Enter a valid selection.");

			application.ReferringStaffCode = "ST8";
			AssertHasError(application.ReferringStaffCodeInfo, "This Staff is not linked to a Person.");
		}

		public void TestCheckHP_PER_ReferringPerson()
		{
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG_TEST1";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Contact 1";
			contact1.OC_Email = "u1@cw1.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG_TEST2";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Contact 2";
			contact2.OC_Email = "u2@cw2.com";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			Factory.Save();

			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_SourceType = ReferringSourcesTypes.Codes.RecruitmentAgent;
			application.HP_PER_ReferringPerson = staff1.GS_PER;
			AssertHasError(application.HP_PER_ReferringPersonInfo, "This Person does not have an Organization Contact Context.");

			application.HP_OH_ReferringOrganisation = org1.PK;
			application.HP_PER_ReferringPerson = contact2.OC_PER;
			AssertHasError(application.HP_PER_ReferringPersonInfo, "This Person does not have a Related Organization ORG_TEST1.");

			application.HP_PER_ReferringPerson = contact1.OC_PER;
			AssertNoErrors(application.HP_PER_ReferringPersonInfo);
		}
	}
}
