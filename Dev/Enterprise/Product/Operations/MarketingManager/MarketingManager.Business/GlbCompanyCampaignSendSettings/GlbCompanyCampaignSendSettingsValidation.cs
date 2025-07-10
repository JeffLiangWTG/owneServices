//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbCompanyCampaignSendSettingsValidation
//
//    This class should be used for overriding validation in AutoGlbCompanyCampaignSendSettingsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSendSettingsValidation : AutoGlbCompanyCampaignSendSettingsValidation
	{
		public GlbCompanyCampaignSendSettingsValidation(AutoGlbCompanyCampaignSendSettings parent) : base(parent)
		{
		}

		public new GlbCompanyCampaignSendSettings Parent
		{
			get { return (GlbCompanyCampaignSendSettings)base.Parent; }
		}

		protected override void CheckGSC_ContactLimitEachBatch()
		{
			base.CheckGSC_ContactLimitEachBatch();

			MandatoryValidation.CheckNotNegative(Parent.GSC_ContactLimitEachBatchInfo);

			if (Parent.IsContactLimitEachBatchUsed)
			{
				MandatoryValidation.CheckNotZero(Parent.GSC_ContactLimitEachBatchInfo);
			}

			if (IsBatchingLimitNotSet)
			{
				Parent.GSC_ContactLimitEachBatchInfo.AddError(batchLimitMessage);
			}
		}

		protected override void CheckGSC_ContactLimitPerOrganizationEachBatch()
		{
			base.CheckGSC_ContactLimitPerOrganizationEachBatch();

			MandatoryValidation.CheckNotNegative(Parent.GSC_ContactLimitPerOrganizationEachBatchInfo);

			if (Parent.IsContactLimitPerOrganizationEachBatchUsed)
			{
				MandatoryValidation.CheckNotZero(Parent.GSC_ContactLimitPerOrganizationEachBatchInfo);

				if (Parent.IsContactLimitPerOrganizationInHorizontalUsed)
				{
					CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.GSC_ContactLimitPerOrganizationEachBatchInfo, Parent.GSC_ContactLimitPerOrganizationInHorizontalInfo);
				}
				if (Parent.IsContactLimitPerOrganizationInTouchUsed)
				{
					CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.GSC_ContactLimitPerOrganizationEachBatchInfo, Parent.GSC_ContactLimitPerOrganizationInTouchInfo);
				}
			}

			if (IsBatchingLimitNotSet)
			{
				Parent.GSC_ContactLimitPerOrganizationEachBatchInfo.AddError(batchLimitMessage);
			}
		}

		protected override void CheckGSC_ContactLimitPerOrganizationInHorizontal()
		{
			base.CheckGSC_ContactLimitPerOrganizationInHorizontal();

			MandatoryValidation.CheckNotNegative(Parent.GSC_ContactLimitPerOrganizationInHorizontalInfo);

			if (Parent.IsContactLimitPerOrganizationInHorizontalUsed)
			{
				MandatoryValidation.CheckNotZero(Parent.GSC_ContactLimitPerOrganizationInHorizontalInfo);
			}

			if (IsBatchingLimitNotSet)
			{
				Parent.GSC_ContactLimitPerOrganizationInHorizontalInfo.AddError(batchLimitMessage);
			}
		}

		protected override void CheckGSC_ContactLimitPerOrgInHorizontalPeriodInDays()
		{
			base.CheckGSC_ContactLimitPerOrgInHorizontalPeriodInDays();

			MandatoryValidation.CheckNotNegative(Parent.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysInfo);

			if (Parent.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed)
			{
				MandatoryValidation.CheckNotZero(Parent.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysInfo);
			}
		}

		protected override void CheckGSC_ContactLimitPerOrganizationInTouch()
		{
			base.CheckGSC_ContactLimitPerOrganizationInTouch();

			MandatoryValidation.CheckNotNegative(Parent.GSC_ContactLimitPerOrganizationInTouchInfo);

			if (Parent.IsContactLimitPerOrganizationInTouchUsed)
			{
				MandatoryValidation.CheckNotZero(Parent.GSC_ContactLimitPerOrganizationInTouchInfo);

				if (Parent.IsContactLimitPerOrganizationInHorizontalUsed)
				{
					if (
						!Parent.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed
						|| Parent.GSC_ContactLimitPerOrganizationInTouchPeriodInDays >= Parent.GSC_ContactLimitPerOrgInHorizontalPeriodInDays)
					{
						CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.GSC_ContactLimitPerOrganizationInTouchInfo, Parent.GSC_ContactLimitPerOrganizationInHorizontalInfo);
					}
				}
			}

			if (IsBatchingLimitNotSet)
			{
				Parent.GSC_ContactLimitPerOrganizationInTouchInfo.AddError(batchLimitMessage);
			}
		}

		public void ValidateLimits()
		{
			ValidateGSC_ContactLimitPerOrganizationInTouch();
			ValidateGSC_ContactLimitPerOrganizationInHorizontal();
			ValidateGSC_ContactLimitPerOrganizationEachBatch();
			ValidateGSC_ContactLimitEachBatch();
		}

		protected override void CheckGSC_ContactLimitPerOrganizationInTouchPeriodInDays()
		{
			base.CheckGSC_ContactLimitPerOrganizationInTouchPeriodInDays();

			MandatoryValidation.CheckNotNegative(Parent.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysInfo);

			if (Parent.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed)
			{
				MandatoryValidation.CheckNotZero(Parent.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysInfo);
			}
		}

		readonly ResourceString batchLimitMessage = ResString.GetMultilingualString("18968718-B435-47EC-BC4F-1AC919EFD09E", "Please set at least one batch limit or change your Schedule Type.");

		bool IsBatchingLimitNotSet
		{
			get
			{
				return Parent.IsBatchSchedule &&
					(!Parent.IsContactLimitEachBatchUsed &&
					!Parent.IsContactLimitPerOrganizationEachBatchUsed &&
					!Parent.IsContactLimitPerOrganizationInHorizontalUsed &&
					!Parent.IsContactLimitPerOrganizationInTouchUsed);
			}
		}

		#region Time Offsets

		protected override void CheckGSC_HoursOffset()
		{
			base.CheckGSC_HoursOffset();

			if (Parent.IsDelayed)
			{
				MandatoryValidation.CheckEntered(Parent.GSC_HoursOffsetInfo);
				MandatoryValidation.CheckValidShortGreaterOrEqualToZero(Parent.GSC_HoursOffsetInfo);
			}
		}

		public void ValidateDaysOffset() => ValidateCalculatedProperty(Parent.DaysOffsetInfo);

		protected virtual void CheckDaysOffset()
		{
			if (Parent.IsDelayed && Parent.GSC_HoursOffset <= 0)
			{
				MandatoryValidation.CheckEntered(Parent.DaysOffsetInfo);
				MandatoryValidation.CheckValidShortGreaterOrEqualToZero(Parent.DaysOffsetInfo);
			}
		}

		public void ValidateHoursOffset() => ValidateCalculatedProperty(Parent.HoursOffsetInfo);

		protected virtual void CheckHoursOffset()
		{
			if (Parent.IsDelayed && Parent.IsUseCurrentTime && Parent.GSC_HoursOffset <= 0)
			{
				MandatoryValidation.CheckEntered(Parent.HoursOffsetInfo);
				MandatoryValidation.CheckValidShortGreaterOrEqualToZero(Parent.HoursOffsetInfo);
			}
		}

		#endregion

		protected override void CheckGSC_ScheduleTime()
		{
			base.CheckGSC_ScheduleTime();

			if ((Parent.IsDelayed && !Parent.IsUseCurrentTime) || Parent.IsFixedDate || Parent.IsBatchSchedule)
			{
				MandatoryValidation.CheckEntered(Parent.GSC_ScheduleTimeInfo);
			}
		}

		protected override void CheckGSC_IsRecipientLocalTime()
		{
			base.CheckGSC_IsRecipientLocalTime();

			if (Parent.IsDelayed || Parent.IsFixedDate || Parent.IsBatchSchedule)
			{
				if (!Parent.GSC_IsSenderLocalTime)
				{
					MandatoryValidation.CheckEntered(Parent.GSC_IsRecipientLocalTimeInfo);
				}
			}
		}

		protected override void CheckGSC_IsSenderLocalTime()
		{
			base.CheckGSC_IsSenderLocalTime();

			if (Parent.IsDelayed || Parent.IsFixedDate || Parent.IsBatchSchedule)
			{
				if (!Parent.GSC_IsRecipientLocalTime)
				{
					MandatoryValidation.CheckEntered(Parent.GSC_IsSenderLocalTimeInfo);
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDaysOffset();
			ValidateHoursOffset();
			ValidateLimits();
		}
	}
}
