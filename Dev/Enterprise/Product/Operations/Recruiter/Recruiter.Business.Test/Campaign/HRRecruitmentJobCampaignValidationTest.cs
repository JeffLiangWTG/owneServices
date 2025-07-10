using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRRecruitmentJobCampaignValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHV_HJ_JobRole()
		{
			HRJobRole role = Factory.New<HRJobRole>();
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			campaign.Validation.ValidateHV_HJ_JobRole();
			AssertHasErrors("Job Role is mandatory, should have errors", campaign.HV_HJ_JobRoleInfo);
			campaign.HV_HJ_JobRole = role.PK;
			AssertNoErrors("Job Role is entered, should NOT have errors", campaign.HV_HJ_JobRoleInfo);
		}

		public void TestCheckHV_CampaignStartDate()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			ZDateTime startDate = new ZDateTime(ZDateTime.Today.Year, 1, 1, 1, 1, 1);
			ZDateTime endDate = new ZDateTime(ZDateTime.Today.Year, 2, 2, 2, 2, 2);

			campaign.HV_CampaignEndDate = startDate;
			campaign.HV_CampaignStartDate = endDate;

			AssertHasErrors("Campaign Start date is after end date, should have errors", campaign.HV_CampaignStartDateInfo);

			campaign.HV_CampaignEndDate = endDate;
			campaign.HV_CampaignStartDate = startDate;

			AssertNoErrors("Dates are valid, should not have errors", campaign.HV_CampaignStartDateInfo);
		}

		public void TestCheckHV_CampaignEndDate()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			ZDateTime startDate = ZDateTime.UtcNow;
			ZDateTime endDate = ZDateTime.UtcNow.AddDays(1);

			campaign.HV_CampaignStartDate = endDate;
			campaign.HV_CampaignEndDate = startDate;

			AssertHasErrors("Campaign End date is before start date, should have errors", campaign.HV_CampaignEndDateInfo);

			campaign.HV_CampaignStartDate = startDate;
			campaign.HV_CampaignEndDate = endDate;

			AssertNoErrors("Dates are valid, should not have errors", campaign.HV_CampaignEndDateInfo);
		}

		public void TestCheckHV_GS_NKControlledBy()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "GS";
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			campaign.HV_GS_NKControlledBy = "--";
			AssertHasErrors("Staff member with code --- does not exist, should have error", campaign.HV_GS_NKControlledByInfo);
			campaign.HV_GS_NKControlledBy = staff.GS_Code;
			AssertNoErrors("Staff member with this code exists, should not have error", campaign.HV_GS_NKControlledByInfo);
			campaign.HV_GS_NKControlledBy = ZString.Empty;
			AssertHasErrors("Controlled by (Campaign Leader) should be mandatory", campaign.HV_GS_NKControlledByInfo);
		}

		public void TestCheckHV_OC_ClientContact()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contactOnOrg = org.Contacts.AddNew();

			OrgContact contactNotOnOrg = Factory.New<OrgContact>();

			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			campaign.HV_OC_ClientContact = contactOnOrg.PK;
			AssertHasErrors("Client Account not set, should have errors", campaign.HV_OC_ClientContactInfo);

			campaign.HV_OH_ClientAccount = org.PK;
			campaign.HV_OC_ClientContact = contactOnOrg.PK;
			AssertNoErrors("Client Contact is on Client Account, should not have errors", campaign.HV_OC_ClientContactInfo);

			campaign.HV_OC_ClientContact = contactNotOnOrg.PK;
			AssertHasErrors("Client Contact is not on Client Account, should have errors", campaign.HV_OC_ClientContactInfo);
		}

		public void TestCheckHV_RX_NKWageRangeCurrency()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			campaign.HV_RX_NKWageRangeCurrency = "---";
			AssertHasErrors("Currency with code --- does not exist, should have error", campaign.HV_RX_NKWageRangeCurrencyInfo);
			campaign.HV_RX_NKWageRangeCurrency = "AUD";
			AssertNoErrors("Currency with this code exists, should not have error", campaign.HV_RX_NKWageRangeCurrencyInfo);

			campaign.HV_RX_NKWageRangeCurrency = "";
			AssertMandatoryValidationError(campaign.HV_RX_NKWageRangeCurrencyInfo, false);
			campaign.HV_RX_NKWageRangeCurrency = "JPY";
			AssertNoErrors(campaign.HV_RX_NKWageRangeCurrencyInfo);
		}

		public void TestCheckWageRange()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			campaign.HV_WageLow = 200;
			campaign.HV_WageHigh = 200;
			campaign.Validation.ValidateHV_WageLow();
			AssertNoErrors(campaign.HV_WageLowInfo);
			AssertNoErrors(campaign.HV_WageHighInfo);

			campaign.HV_WageHigh = 300;
			campaign.Validation.ValidateHV_WageLow();
			AssertNoErrors(campaign.HV_WageLowInfo);
			AssertNoErrors(campaign.HV_WageHighInfo);

			campaign.HV_WageHigh = 199;
			campaign.Validation.ValidateHV_WageLow();
			AssertHasErrorContaining(campaign.HV_WageHighInfo, "greater than or equal to");
			AssertHasErrorContaining(campaign.HV_WageLowInfo, "less than or equal to");

			campaign.HV_WageLow = 100;
			campaign.Validation.ValidateHV_WageHigh();
			AssertNoErrors(campaign.HV_WageLowInfo);
			AssertNoErrors(campaign.HV_WageHighInfo);
		}

		public void TestCampaignIDValidation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress mainAddress = org.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			HRRecruitmentJobCampaign campaign1 = Factory.New<HRRecruitmentJobCampaign>();
			campaign1.HV_AdTitle = "Software Developer";
			campaign1.HV_CampaignStartDate = ZDateTime.Today;
			campaign1.HV_OA_ClientAddress = mainAddress.PK;
			campaign1.Validation.ValidateAll();
			AssertNoErrors(campaign1.CampaignIDInfo);

			Factory.Save();

			HRRecruitmentJobCampaign campaign2 = Factory.New<HRRecruitmentJobCampaign>();
			campaign2.HV_AdTitle = "Software Developer";
			campaign2.HV_CampaignStartDate = ZDateTime.Today;
			campaign2.HV_OA_ClientAddress = mainAddress.PK;
			campaign2.Validation.ValidateAll();
			AssertHasErrors(campaign2.CampaignIDInfo);

			campaign2.HV_AdTitle = "Software Developer (Sydney)";
			campaign2.Validation.ValidateAll();
			AssertNoErrors(campaign2.CampaignIDInfo);
		}

		public void TestCheckHV_CampaignStartDateLocal()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			ZDateTime startDate = ZDateTime.UtcNow;
			ZDateTime endDate = ZDateTime.UtcNow.AddDays(1);

			campaign.HV_CampaignEndDate = startDate;
			campaign.HV_CampaignStartDate = endDate;

			campaign.Validation.ValidateHV_CampaignStartDate();
			AssertHasErrors("Campaign Start date is after end date, should have errors", campaign.HV_CampaignStartDateLocalInfo);

			campaign.HV_CampaignEndDate = endDate;
			campaign.HV_CampaignStartDate = startDate;

			campaign.Validation.ValidateHV_CampaignStartDate();
			AssertNoErrors("Dates are valid, should not have errors", campaign.HV_CampaignStartDateLocalInfo);
		}

		public void TestCheckHV_CampaignEndDateLocal()
		{
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			ZDateTime startDate = ZDateTime.UtcNow;
			ZDateTime endDate = ZDateTime.UtcNow.AddDays(1);

			campaign.HV_CampaignStartDateLocal = endDate;
			campaign.HV_CampaignEndDateLocal = startDate;

			campaign.Validation.ValidateHV_CampaignEndDate();
			AssertHasErrors("Campaign End date is before start date, should have errors", campaign.HV_CampaignEndDateLocalInfo);

			campaign.HV_CampaignStartDateLocal = startDate;
			campaign.HV_CampaignEndDateLocal = endDate;

			campaign.Validation.ValidateHV_CampaignEndDate();
			AssertNoErrors("Dates are valid, should not have errors", campaign.HV_CampaignEndDateLocalInfo);
		}
	}
}
