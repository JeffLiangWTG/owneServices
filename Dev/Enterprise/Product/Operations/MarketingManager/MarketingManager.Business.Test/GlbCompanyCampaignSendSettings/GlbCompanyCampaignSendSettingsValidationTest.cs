using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignSendSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateGSC_ScheduleType()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsBatchSchedule = true;
			settings.Validation.ValidateAll();

			AssertLimitsErrors(settings, true);

			settings.IsContactLimitPerOrganizationEachBatchUsed = true;
			AssertLimitsErrors(settings, false);
			settings.IsContactLimitPerOrganizationEachBatchUsed = false;
			AssertLimitsErrors(settings, true);

			settings.IsContactLimitPerOrganizationInTouchUsed = true;
			AssertLimitsErrors(settings, false);
			settings.IsContactLimitPerOrganizationInTouchUsed = false;
			AssertLimitsErrors(settings, true);

			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			AssertLimitsErrors(settings, false);
			settings.IsContactLimitPerOrganizationInHorizontalUsed = false;
			AssertLimitsErrors(settings, true);

			settings.IsContactLimitEachBatchUsed = true;
			AssertLimitsErrors(settings, false);
			settings.IsContactLimitEachBatchUsed = false;
			AssertLimitsErrors(settings, true);

			settings.IsFixedDate = true;
			AssertLimitsErrors(settings, false);
		}

		void AssertLimitsErrors(GlbCompanyCampaignSendSettings settings, bool hasError)
		{
			string error = "Please set at least one batch limit or change your Schedule Type.";
			if (hasError)
			{
				AssertHasError(settings.GSC_ContactLimitPerOrganizationInTouchInfo, error);
				AssertHasError(settings.GSC_ContactLimitPerOrganizationInHorizontalInfo, error);
				AssertHasError(settings.GSC_ContactLimitPerOrganizationEachBatchInfo, error);
				AssertHasError(settings.GSC_ContactLimitEachBatchInfo, error);
			}
			else
			{
				AssertNoError(settings.GSC_ContactLimitPerOrganizationInTouchInfo, error);
				AssertNoError(settings.GSC_ContactLimitPerOrganizationInHorizontalInfo, error);
				AssertNoError(settings.GSC_ContactLimitPerOrganizationEachBatchInfo, error);
				AssertNoError(settings.GSC_ContactLimitEachBatchInfo, error);
			}
		}

		public void TestValidateGSC_ContactLimitEachBatch()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsImmediate = true;
			settings.IsContactLimitEachBatchUsed = false;
			settings.GSC_ContactLimitEachBatch = -1;
			AssertCheckNotNegativeValidationError(settings.GSC_ContactLimitEachBatchInfo, true);

			settings.GSC_ContactLimitEachBatch = 0;
			AssertNoErrors(settings.GSC_ContactLimitEachBatchInfo);

			settings.IsContactLimitEachBatchUsed = true;
			settings.Validation.ValidateGSC_ContactLimitEachBatch();
			AssertHasErrorContaining(settings.GSC_ContactLimitEachBatchInfo, MandatoryValidation.ValueCannotBeZero);

			settings.GSC_ContactLimitEachBatch = 3;
			AssertNoErrors(settings.GSC_ContactLimitEachBatchInfo);
		}

		public void TestValidateGSC_ContactLimitPerOrganizationEachBatch()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsImmediate = true;
			settings.IsContactLimitPerOrganizationEachBatchUsed = false;
			settings.GSC_ContactLimitPerOrganizationEachBatch = -1;
			AssertCheckNotNegativeValidationError(settings.GSC_ContactLimitPerOrganizationEachBatchInfo, true);

			settings.GSC_ContactLimitPerOrganizationEachBatch = 0;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationEachBatchInfo);

			settings.IsContactLimitPerOrganizationEachBatchUsed = true;
			settings.Validation.ValidateGSC_ContactLimitPerOrganizationEachBatch();
			AssertHasErrorContaining(settings.GSC_ContactLimitPerOrganizationEachBatchInfo, MandatoryValidation.ValueCannotBeZero);

			settings.GSC_ContactLimitPerOrganizationEachBatch = 3;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationEachBatchInfo);

			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			settings.GSC_ContactLimitPerOrganizationInHorizontal = 1;
			settings.Validation.ValidateGSC_ContactLimitPerOrganizationEachBatch();
			AssertHasErrors(settings.GSC_ContactLimitPerOrganizationEachBatchInfo);
		}

		public void TestValidateGSC_ContactLimitPerOrganizationInHorizontal()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsImmediate = true;
			settings.IsContactLimitPerOrganizationInHorizontalUsed = false;
			settings.GSC_ContactLimitPerOrganizationInHorizontal = -1;
			AssertCheckNotNegativeValidationError(settings.GSC_ContactLimitPerOrganizationInHorizontalInfo, true);

			settings.GSC_ContactLimitPerOrganizationInHorizontal = 0;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationInHorizontalInfo);

			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			settings.Validation.ValidateGSC_ContactLimitPerOrganizationInHorizontal();
			AssertHasErrorContaining(settings.GSC_ContactLimitPerOrganizationInHorizontalInfo, MandatoryValidation.ValueCannotBeZero);

			settings.GSC_ContactLimitPerOrganizationInHorizontal = 3;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationInHorizontalInfo);
		}

		public void TestValidateGSC_ContactLimitPerOrgInHorizontalPeriodInDays()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = false;
			settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = -1;
			AssertCheckNotNegativeValidationError(settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysInfo, true);

			settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = 0;
			AssertNoErrors(settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysInfo);

			settings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = true;
			settings.Validation.ValidateGSC_ContactLimitPerOrgInHorizontalPeriodInDays();
			AssertHasErrorContaining(settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysInfo, MandatoryValidation.ValueCannotBeZero);

			settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = 3;
			AssertNoErrors(settings.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysInfo);
		}

		public void TestValidateGSC_ContactLimitPerOrganizationInTouch()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsImmediate = true;
			settings.IsContactLimitPerOrganizationInTouchUsed = false;
			settings.GSC_ContactLimitPerOrganizationInTouch = -1;
			AssertCheckNotNegativeValidationError(settings.GSC_ContactLimitPerOrganizationInTouchInfo, true);

			settings.GSC_ContactLimitPerOrganizationInTouch = 0;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationInTouchInfo);

			settings.IsContactLimitPerOrganizationInTouchUsed = true;
			settings.Validation.ValidateGSC_ContactLimitPerOrganizationInTouch();
			AssertHasErrorContaining(settings.GSC_ContactLimitPerOrganizationInTouchInfo, MandatoryValidation.ValueCannotBeZero);

			settings.GSC_ContactLimitPerOrganizationInTouch = 3;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationInTouchInfo);

			settings.IsContactLimitPerOrganizationInHorizontalUsed = true;
			settings.GSC_ContactLimitPerOrganizationInHorizontal = 1;
			settings.Validation.ValidateGSC_ContactLimitPerOrganizationInTouch();
			AssertHasErrors(settings.GSC_ContactLimitPerOrganizationInTouchInfo);
		}

		public void TestValidateGSC_ContactLimitPerOrganizationInTouchPeriodInDays()
		{
			var settings = Factory.New<GlbCompanyCampaignSendSettings>();
			settings.IsContactLimitPerOrganizationInTouchUsed = true;
			settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed = false;
			settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDays = -1;
			AssertCheckNotNegativeValidationError(settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysInfo, true);

			settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDays = 0;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysInfo);

			settings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed = true;
			settings.Validation.ValidateGSC_ContactLimitPerOrganizationInTouchPeriodInDays();
			AssertHasErrorContaining(settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysInfo, MandatoryValidation.ValueCannotBeZero);

			settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDays = 3;
			AssertNoErrors(settings.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysInfo);
		}

		public void TestCheckGSC_HoursOffset()
		{
			var sendSetting = Factory.New<GlbCompanyCampaignSendSettings>();
			sendSetting.IsImmediate = true;
			sendSetting.IsUseCurrentTime = true;

			sendSetting.Validation.ValidateGSC_HoursOffset();
			AssertNoErrors("Should not have errors.", sendSetting);

			sendSetting.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.DEL;
			AssertShouldHaveAnError(sendSetting);

			sendSetting.GSC_HoursOffset = 1;
			AssertNoErrorsOnOffset(sendSetting);

			sendSetting.GSC_HoursOffset = -1;
			AssertShouldHaveAnError(sendSetting);

			sendSetting.GSC_HoursOffset = 1;
			AssertNoErrorsOnOffset(sendSetting);

			sendSetting.GSC_HoursOffset = 0;
			AssertShouldHaveAnError(sendSetting);

			sendSetting.GSC_HoursOffset = 1;
			AssertNoErrorsOnOffset(sendSetting);

			sendSetting.IsUseCurrentTime = false;
			sendSetting.GSC_HoursOffset = -1;
			ValidateTimeOffsets(sendSetting);
			AssertHasErrors("Should have an error.", sendSetting.GSC_HoursOffsetInfo);
			AssertHasErrors("Should have an error.", sendSetting.DaysOffsetInfo);
			AssertNoErrors("Should not have errors.", sendSetting.HoursOffsetInfo);
		}

		static void ValidateTimeOffsets(GlbCompanyCampaignSendSettings sendSetting)
		{
			sendSetting.Validation.ValidateGSC_HoursOffset();
			sendSetting.Validation.ValidateDaysOffset();
			sendSetting.Validation.ValidateHoursOffset();
		}

		static void AssertShouldHaveAnError(GlbCompanyCampaignSendSettings sendSetting)
		{
			ValidateTimeOffsets(sendSetting);
			AssertHasErrors("Should have an error.", sendSetting.GSC_HoursOffsetInfo);
			AssertHasErrors("Should have an error.", sendSetting.DaysOffsetInfo);
			AssertHasErrors("Should have an error.", sendSetting.HoursOffsetInfo);
		}

		static void AssertNoErrorsOnOffset(GlbCompanyCampaignSendSettings sendSetting)
		{
			ValidateTimeOffsets(sendSetting);
			AssertNoErrors("Should not have errors.", sendSetting);
			AssertNoErrors("Should not have errors.", sendSetting.GSC_HoursOffsetInfo);
			AssertNoErrors("Should not have errors.", sendSetting.DaysOffsetInfo);
			AssertNoErrors("Should not have errors.", sendSetting.HoursOffsetInfo);
		}
	}
}
