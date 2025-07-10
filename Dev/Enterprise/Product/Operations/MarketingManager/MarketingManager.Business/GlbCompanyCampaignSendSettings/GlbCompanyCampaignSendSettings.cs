using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSendSettings : AutoGlbCompanyCampaignSendSettings
	{
		#region Schema

		public new class Schema : AutoGlbCompanyCampaignSendSettings.Schema
		{
			public const string IsBatchSchedule = "IsBatchSchedule";
			public const string IsImmediate = "IsImmediate";
			public const string IsDelayed = "IsDelayed";
			public const string IsFixedDate = "IsFixedDate";

			public const string IsContactLimitEachBatchUsed = "IsContactLimitEachBatchUsed";
			public const string IsContactLimitPerOrganizationEachBatchUsed = "IsContactLimitPerOrganizationEachBatchUsed";
			public const string IsContactLimitPerOrganizationInHorizontalUsed = "IsContactLimitPerOrganizationInHorizontalUsed";
			public const string IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = "IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed";
			public const string IsContactLimitPerOrganizationInTouchUsed = "IsContactLimitPerOrganizationInTouchUsed";
			public const string IsContactLimitPerOrganizationInTouchPeriodInDaysUsed = "IsContactLimitPerOrganizationInTouchPeriodInDaysUsed";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public GlbCompanyCampaignSendSettings(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			if (IsDelayed)
			{
				using (SuspendSettingHasChanges())
				{
					IsUseCurrentTime = GSC_ScheduleTime.IsEmpty || !GSC_ScheduleTime.IsValid;
				}
			}
			else if (!IsFixedDate && !IsBatchSchedule)
			{
				IsImmediate = true;
			}
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IsBatchSchedule = true;
			GSC_IsRecipientLocalTime = true;
			GSC_IsSenderLocalTime = false;
			GSC_ScheduleTime = ZDateTime.Today.AddHours(14);
		}

		#endregion

		#region Properties

		#region GSC_G0_Campaign

		public GlbCompanyCampaign OwnerCampaign
		{
			get { return Factory.Load<GlbCompanyCampaign>(GSC_G0_Campaign); }
		}

		public GlbCompanyCampaignGroup OwnerCampaignGroup
		{
			get { return Factory.Load<GlbCompanyCampaignGroup>(GSC_GCG_Group); }
		}

		#endregion

		#region GSC_ScheduleType

		#region GSC_ScheduleType

		[List("Lookups.ScheduleTypes")]
		public override ZString GSC_ScheduleType
		{
			get { return base.GSC_ScheduleType; }
			set
			{
				base.GSC_ScheduleType = value;
				Validation.ValidateLimits();
			}
		}

		#endregion

		#region IsBatchSchedule

		[BusinessObjectTestExclude]
		public ZBool IsBatchSchedule
		{
			get { return GSC_ScheduleType == GlbCompanyCampaignSendSettingsLookups.Codes.BAT; }
			set
			{
				if (value)
				{
					EnsureScheduleTaskExists();
					GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.BAT;
				}
			}
		}

		public ZPropertyInfo IsBatchScheduleInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.IsBatchSchedule, (x) => GSC_ScheduleTypeInfo); }
		}

		#endregion

		#region IsImmediate

		public ZBool IsImmediate
		{
			get { return GSC_ScheduleType == GlbCompanyCampaignSendSettingsLookups.Codes.IMM; }
			set
			{
				if (!IsImmediate && value)
				{
					GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;
				}
			}
		}

		public ZPropertyInfo IsImmediateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.IsImmediate, (x) => GSC_ScheduleTypeInfo); }
		}

		#endregion

		#region IsDelayed

		public ZBool IsDelayed
		{
			get { return GSC_ScheduleType == GlbCompanyCampaignSendSettingsLookups.Codes.DEL; }
			set
			{
				if (!IsDelayed && value)
				{
					GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.DEL;
				}
			}
		}

		public ZPropertyInfo IsDelayedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.IsDelayed, (x) => GSC_ScheduleTypeInfo); }
		}

		#endregion

		#region IsFixedDate

		public ZBool IsFixedDate
		{
			get { return GSC_ScheduleType == GlbCompanyCampaignSendSettingsLookups.Codes.FIX; }
			set
			{
				if (!IsFixedDate && value)
				{
					GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.FIX;
				}
			}
		}

		public ZPropertyInfo IsFixedDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.IsFixedDate, (x) => GSC_ScheduleTypeInfo); }
		}

		#endregion

		#endregion

		#region IsUseCurrentTime

		ZBool isUseCurrentTime;
		public ZBool IsUseCurrentTime
		{
			get { return isUseCurrentTime; }
			set
			{
				SetNonPersistentPropertyValue(IsUseCurrentTimeInfo, ref isUseCurrentTime, value);

				if (isUseCurrentTime)
				{
					GSC_ScheduleTime = ZDateTime.Empty;
				}
				else
				{
					HoursOffset = 0;
				}
			}
		}

		public ZPropertyInfo IsUseCurrentTimeInfo
		{
			get { return GetZPropertyInfo(nameof(IsUseCurrentTime)); }
		}

		#endregion

		#region GSC_ContactLimitEachBatch

		public ZBool IsContactLimitEachBatchUsed
		{
			get { return isContactLimitEachBatchUsed || GSC_ContactLimitEachBatch != 0; }
			set
			{
				if (!value)
				{
					using (GetValidationSuspender())
					{
						GSC_ContactLimitEachBatch = 0;
					}
				}
				SetNonPersistentPropertyValue(IsContactLimitEachBatchUsedInfo, ref isContactLimitEachBatchUsed, value);
				Validation.ValidateLimits();
			}
		}
		ZBool isContactLimitEachBatchUsed;

		public ZPropertyInfo IsContactLimitEachBatchUsedInfo
		{
			get { return GetZPropertyInfo(Schema.IsContactLimitEachBatchUsed); }
		}

		protected bool GSC_ContactLimitEachBatch_ReadOnly
		{
			get { return !IsContactLimitEachBatchUsed; }
		}

		#endregion

		#region GSC_ContactLimitPerOrganizationEachBatch

		public ZBool IsContactLimitPerOrganizationEachBatchUsed
		{
			get { return isContactLimitPerOrganizationEachBatchUsed || GSC_ContactLimitPerOrganizationEachBatch != 0; }
			set
			{
				if (!value)
				{
					using (GetValidationSuspender())
					{
						GSC_ContactLimitPerOrganizationEachBatch = 0;
					}
				}
				SetNonPersistentPropertyValue(IsContactLimitPerOrganizationEachBatchUsedInfo, ref isContactLimitPerOrganizationEachBatchUsed, value);
				Validation.ValidateLimits();
			}
		}
		ZBool isContactLimitPerOrganizationEachBatchUsed;

		public ZPropertyInfo IsContactLimitPerOrganizationEachBatchUsedInfo
		{
			get { return GetZPropertyInfo(Schema.IsContactLimitPerOrganizationEachBatchUsed); }
		}

		protected bool GSC_ContactLimitPerOrganizationEachBatch_ReadOnly
		{
			get { return !IsContactLimitPerOrganizationEachBatchUsed; }
		}

		#endregion

		#region GSC_ContactLimitPerOrganizationInHorizontal

		public ZBool IsContactLimitPerOrganizationInHorizontalUsed
		{
			get { return isContactLimitPerOrganizationInHorizontalUsed || GSC_ContactLimitPerOrganizationInHorizontal != 0; }
			set
			{
				if (!value)
				{
					using (GetValidationSuspender())
					{
						GSC_ContactLimitPerOrganizationInHorizontal = 0;
						IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed = false;
					}
				}
				SetNonPersistentPropertyValue(IsContactLimitPerOrganizationInHorizontalUsedInfo, ref isContactLimitPerOrganizationInHorizontalUsed, value);
				Validation.ValidateLimits();
			}
		}
		ZBool isContactLimitPerOrganizationInHorizontalUsed;

		public ZPropertyInfo IsContactLimitPerOrganizationInHorizontalUsedInfo
		{
			get { return GetZPropertyInfo(Schema.IsContactLimitPerOrganizationInHorizontalUsed); }
		}

		public override ZShort GSC_ContactLimitPerOrganizationInHorizontal
		{
			get { return base.GSC_ContactLimitPerOrganizationInHorizontal; }
			set
			{
				if (base.GSC_ContactLimitPerOrganizationInHorizontal != value)
				{
					base.GSC_ContactLimitPerOrganizationInHorizontal = value;

					if (!isPropogateGSC_ContactLimitPerOrganizationInHorizontalSuspended)
					{
						foreach (var otherCampaign in FindOtherTouchesInSameHorizontal())
						{
							try
							{
								otherCampaign.SendSettings.isPropogateGSC_ContactLimitPerOrganizationInHorizontalSuspended = true;
								otherCampaign.SendSettings.GSC_ContactLimitPerOrganizationInHorizontal = value;
							}
							finally
							{
								otherCampaign.SendSettings.isPropogateGSC_ContactLimitPerOrganizationInHorizontalSuspended = false;
							}
						}
					}
				}
			}
		}

		protected bool GSC_ContactLimitPerOrganizationInHorizontal_ReadOnly
		{
			get { return !IsContactLimitPerOrganizationInHorizontalUsed; }
		}

		bool isPropogateGSC_ContactLimitPerOrganizationInHorizontalSuspended;

		IEnumerable<GlbCompanyCampaign> FindOtherTouchesInSameHorizontal()
		{
			if (OwnerCampaign != null)
			{
				return OwnerCampaign.GetOtherTouchesInSameHorizontal();
			}
			else if (OwnerCampaignGroup != null)
			{
				return OwnerCampaignGroup.Touches[0].GetOtherTouchesInSameHorizontal().Where(x => x.G0_GCG_Group != GSC_GCG_Group);
			}
			else
			{
				return Enumerable.Empty<GlbCompanyCampaign>();
			}
		}

		#endregion

		#region GSC_ContactLimitPerOrgInHorizontalPeriodInDays

		[BusinessObjectTestExclude]
		public ZBool IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed
		{
			get { return IsContactLimitPerOrganizationInHorizontalUsed && (isContactLimitPerOrganizationInHorizontalPeriodInDaysUsed || GSC_ContactLimitPerOrgInHorizontalPeriodInDays != 0); }
			set
			{
				if (!value)
				{
					using (GetValidationSuspender())
					{
						GSC_ContactLimitPerOrgInHorizontalPeriodInDays = 0;
					}
				}
				SetNonPersistentPropertyValue(IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedInfo, ref isContactLimitPerOrganizationInHorizontalPeriodInDaysUsed, value);
			}
		}
		ZBool isContactLimitPerOrganizationInHorizontalPeriodInDaysUsed;

		public ZPropertyInfo IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedInfo
		{
			get { return GetZPropertyInfo(Schema.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed); }
		}

		protected bool IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed_ReadOnly
		{
			get { return !IsContactLimitPerOrganizationInHorizontalUsed; }
		}

		public override ZShort GSC_ContactLimitPerOrgInHorizontalPeriodInDays
		{
			get { return base.GSC_ContactLimitPerOrgInHorizontalPeriodInDays; }
			set
			{
				if (base.GSC_ContactLimitPerOrgInHorizontalPeriodInDays != value)
				{
					base.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = value;

					if (!isPropogateGSC_ContactLimitPerOrgInHorizontalPeriodInDaysSuspended)
					{
						foreach (var otherCampaign in FindOtherTouchesInSameHorizontal())
						{
							try
							{
								otherCampaign.SendSettings.isPropogateGSC_ContactLimitPerOrgInHorizontalPeriodInDaysSuspended = true;
								otherCampaign.SendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays = value;
							}
							finally
							{
								otherCampaign.SendSettings.isPropogateGSC_ContactLimitPerOrgInHorizontalPeriodInDaysSuspended = false;
							}
						}
					}
				}
			}
		}

		protected bool GSC_ContactLimitPerOrgInHorizontalPeriodInDays_ReadOnly
		{
			get { return !IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed; }
		}

		bool isPropogateGSC_ContactLimitPerOrgInHorizontalPeriodInDaysSuspended;

		#endregion

		#region GSC_ContactLimitPerOrganizationInTouch

		public ZBool IsContactLimitPerOrganizationInTouchUsed
		{
			get { return isContactLimitPerOrganizationInTouchUsed || GSC_ContactLimitPerOrganizationInTouch != 0; }
			set
			{
				if (!value)
				{
					using (GetValidationSuspender())
					{
						GSC_ContactLimitPerOrganizationInTouch = 0;
						IsContactLimitPerOrganizationInTouchPeriodInDaysUsed = false;
					}
				}
				SetNonPersistentPropertyValue(IsContactLimitPerOrganizationInTouchUsedInfo, ref isContactLimitPerOrganizationInTouchUsed, value);
				Validation.ValidateLimits();
			}
		}
		ZBool isContactLimitPerOrganizationInTouchUsed;

		public ZPropertyInfo IsContactLimitPerOrganizationInTouchUsedInfo
		{
			get { return GetZPropertyInfo(Schema.IsContactLimitPerOrganizationInTouchUsed); }
		}

		protected bool GSC_ContactLimitPerOrganizationInTouch_ReadOnly
		{
			get { return !IsContactLimitPerOrganizationInTouchUsed; }
		}

		#endregion

		#region GSC_ContactLimitPerOrganizationInTouchPeriodInDays

		[BusinessObjectTestExclude]
		public ZBool IsContactLimitPerOrganizationInTouchPeriodInDaysUsed
		{
			get { return IsContactLimitPerOrganizationInTouchUsed && (isContactLimitPerOrganizationInTouchPeriodInDaysUsed || GSC_ContactLimitPerOrganizationInTouchPeriodInDays != 0); }
			set
			{
				if (!value)
				{
					using (GetValidationSuspender())
					{
						GSC_ContactLimitPerOrganizationInTouchPeriodInDays = 0;
					}
				}
				SetNonPersistentPropertyValue(IsContactLimitPerOrganizationInTouchPeriodInDaysUsedInfo, ref isContactLimitPerOrganizationInTouchPeriodInDaysUsed, value);
			}
		}
		ZBool isContactLimitPerOrganizationInTouchPeriodInDaysUsed;

		public ZPropertyInfo IsContactLimitPerOrganizationInTouchPeriodInDaysUsedInfo
		{
			get { return GetZPropertyInfo(Schema.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed); }
		}

		protected bool IsContactLimitPerOrganizationInTouchPeriodInDaysUsed_ReadOnly
		{
			get { return !IsContactLimitPerOrganizationInTouchUsed; }
		}

		protected bool GSC_ContactLimitPerOrganizationInTouchPeriodInDays_ReadOnly
		{
			get { return !IsContactLimitPerOrganizationInTouchPeriodInDaysUsed; }
		}

		#endregion

		#region Offsets
		public ZShort DaysOffset
		{
			get { return (ZShort)(GSC_HoursOffset / 24); }
			set { SetOffsetValue(value, HoursOffset); }
		}

		public ZPropertyInfo DaysOffsetInfo => GetZPropertyInfo(nameof(DaysOffset));

		public ZShort HoursOffset
		{
			get { return (ZShort)(GSC_HoursOffset % 24); }
			set { SetOffsetValue(DaysOffset, value); }
		}

		public ZPropertyInfo HoursOffsetInfo => GetZPropertyInfo(nameof(HoursOffset));

		void SetOffsetValue(ZShort days, ZShort hours)
		{
			var value = (ZShort)(24 * days + hours);
			if (GSC_HoursOffset == value)
			{
				return;
			}

			GSC_HoursOffset = value;
			DaysOffsetInfo.RefreshBinding();
			HoursOffsetInfo.RefreshBinding();
		}

		public override ZShort GSC_HoursOffset
		{
			get { return base.GSC_HoursOffset; }
			set
			{
				if (GSC_HoursOffset == value)
				{
					return;
				}

				base.GSC_HoursOffset = value;
				SetNonPersistentPropertyValue(DaysOffsetInfo, ref daysOffset, DaysOffset);
				SetNonPersistentPropertyValue(HoursOffsetInfo, ref hoursOffset, HoursOffset);
			}
		}

		ZShort daysOffset;
		ZShort hoursOffset;

		#endregion

		internal void CopyHorizontalSettings(GlbCompanyCampaignSendSettings other)
		{
			try
			{
				isPropogateGSC_ContactLimitPerOrganizationInHorizontalSuspended = true;
				isPropogateGSC_ContactLimitPerOrgInHorizontalPeriodInDaysSuspended = true;

				GSC_ContactLimitPerOrganizationInHorizontal = other.GSC_ContactLimitPerOrganizationInHorizontal;
				GSC_ContactLimitPerOrgInHorizontalPeriodInDays = other.GSC_ContactLimitPerOrgInHorizontalPeriodInDays;
			}
			finally
			{
				isPropogateGSC_ContactLimitPerOrganizationInHorizontalSuspended = false;
				isPropogateGSC_ContactLimitPerOrgInHorizontalPeriodInDaysSuspended = false;
			}
		}

		#region ReadOnly's

		public bool GSC_IsRecipientLocalTime_ReadOnly
		{
			get { return IsImmediate; }
		}

		public bool GSC_IsSenderLocalTime_ReadOnly
		{
			get { return IsImmediate; }
		}

		public bool GSC_ScheduleTime_ReadOnly
		{
			get { return (IsImmediate || IsUseCurrentTime) && !IsFixedDate; }
		}

		public bool GSC_HoursOffset_ReadOnly
		{
			get { return IsImmediate || !IsDelayed; }
		}

		public bool DaysOffset_ReadOnly => GSC_HoursOffset_ReadOnly;

		public bool HoursOffset_ReadOnly => GSC_HoursOffset_ReadOnly || !IsUseCurrentTime;

		public bool IsUseCurrentTime_ReadOnly
		{
			get { return IsImmediate || !IsDelayed; }
		}

		public bool GSC_RL_NKSenderTimeZone_ReadOnly
		{
			get { return GSC_IsRecipientLocalTime || IsImmediate || IsUseCurrentTime; }
		}

		#endregion

		#endregion

		#region Scheduling

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ScheduleData GetNextTouchScheduleData(GlbCompanyCampaign targetTouch, CampaignContact contact, Dictionary<GlbCompanyCampaign, List<ScheduleData>> pendingSends, ZString senderUnloco)
		{
			if (IsImmediate)
			{
				return new ScheduleData(contact, ZDateTime.UtcNow.AddSeconds(5));
			}

			if (IsBatchSchedule)
			{
				return
					GetBatchScheduleDataIfCanStillFit(targetTouch, contact, targetTouch.G0_LastSentBatchNumber, targetTouch.G0_LastSentBatchLocalDate, pendingSends, senderUnloco) // Try to add to current batch if still can
					?? new ScheduleData(contact, ZDateTime.Empty); // Otherwise add it to queue
			}

			ZDateTime scheduleTime;
			if (IsDelayed && IsUseCurrentTime)
			{
				scheduleTime = ZDateTime.UtcNow.AddHours(GSC_HoursOffset);
			}
			else
			{
				ZDateTime dateLocal = IsDelayed
					? ZDateTime.UtcNow.Date.AddHours(GSC_HoursOffset).AddHours(GSC_ScheduleTime.Hour).AddMinutes(GSC_ScheduleTime.Minute)
					: GSC_ScheduleTime;

				scheduleTime = dateLocal.IsValid ? CalculateScheduleTimeUtc(contact, dateLocal.ToDateTime(), senderUnloco) : dateLocal;
			}

			return new ScheduleData(contact, scheduleTime);
		}

		ScheduleData GetBatchScheduleDataIfCanStillFit(GlbCompanyCampaign targetTouch, CampaignContact contact, int batchNumber, ZDateTime zLocalTime, Dictionary<GlbCompanyCampaign, List<ScheduleData>> pendingSends, ZString senderUnloco)
		{
			if (!zLocalTime.IsValid)
			{
				return null;
			}

			var localTime = zLocalTime.ToDateTime();

			var scheduleTimeUtc = CalculateScheduleTimeUtc(contact, localTime, senderUnloco);
			if (!BatchCanFit(targetTouch, contact, batchNumber, pendingSends)
				|| IsContactLimitPerOrganizationReached(targetTouch, contact, scheduleTimeUtc, pendingSends)
				|| localTime < ZDateTime.Now
				)
			{
				return null;
			}

			return new ScheduleData(contact, scheduleTimeUtc, batchNumber);
		}

		public ZDateTime CalculateScheduleTimeUtc(CampaignContact contact, DateTime localTime, ZString senderUnloco)
		{
			if (GSC_IsSenderLocalTime)
			{
				return Env.Time.GetUtcFromUnlocoTime(senderUnloco, localTime);
			}
			else // GSC_IsRecipientLocalTime
			{
				var recipientUnloco = contact != null ? contact.VCC_RelatedPortCode : ZString.Empty;
				if (recipientUnloco.IsEmpty)
				{
					return Env.Time.GetUtcFromUnlocoTime(senderUnloco, localTime);
				}
				else
				{
					return Env.Time.GetUtcFromUnlocoTime(recipientUnloco, localTime);
				}
			}
		}

		bool BatchCanFit(GlbCompanyCampaign targetTouch, CampaignContact contact, int batchNumber, Dictionary<GlbCompanyCampaign, List<ScheduleData>> pendingSends)
		{
			if (contact == null)
			{
				return false;
			}

			if (IsContactLimitEachBatchUsed || IsContactLimitPerOrganizationEachBatchUsed)
			{
				var othersSentInBatchQuery = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, GSC_G0_Campaign);
				othersSentInBatchQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_BatchNumber, batchNumber);
				var otherItemsSentInBatch = Factory.Load<GlbCompanyCampaignItem>(othersSentInBatchQuery);
				foreach (var otherItem in otherItemsSentInBatch)
				{
					Factory.AddFetchHint(ViewCampaignContactSchema.PK, otherItem.G8_RecipientID);
				}

				var othersSentInBatch = otherItemsSentInBatch.Select(x => x.RecipientFromView).Where(i => i != null);
				var othersAboutToBeSentInBatch = pendingSends.ContainsKey(targetTouch) ? pendingSends[targetTouch].Where(x => x.BatchNumber == batchNumber).Select(x => x.Contact) : Enumerable.Empty<CampaignContact>();
				var othersSentOrAboutToBeSentInBatch = othersSentInBatch.Concat(othersAboutToBeSentInBatch).ToList();

				if (IsContactLimitEachBatchUsed && (othersSentOrAboutToBeSentInBatch.Count >= GSC_ContactLimitEachBatch))
				{
					return false;
				}

				if (IsContactLimitPerOrganizationEachBatchUsed)
				{
					var othersInSameOrganizationSentOrAboutToBeSentInThisBatch = othersSentOrAboutToBeSentInBatch.Where(o => o.VCC_OH == contact.VCC_OH);

					if (othersInSameOrganizationSentOrAboutToBeSentInThisBatch.Count() >= GSC_ContactLimitPerOrganizationEachBatch)
					{
						return false;
					}
				}
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool IsContactLimitPerOrganizationReached(GlbCompanyCampaign targetTouch, CampaignContact contact, ZDateTime targetDate, Dictionary<GlbCompanyCampaign, List<ScheduleData>> pendingSends)
		{
			if (contact == null)
			{
				return false;
			}

			if (IsContactLimitPerOrganizationInTouchUsed)
			{
				var periodStartDate = targetDate.AddDays(-GSC_ContactLimitPerOrganizationInTouchPeriodInDays);
				var periodEndDate = targetDate.AddDays(GSC_ContactLimitPerOrganizationInTouchPeriodInDays);

				var othersSentInTouchQuery = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, GSC_G0_Campaign);
				if (IsContactLimitPerOrganizationInTouchPeriodInDaysUsed)
				{
					var isSentOrQueuedAfterStartPart = new ZQuery(GlbCompanyCampaignItemSchema.G8_LastSentTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, periodStartDate);
					isSentOrQueuedAfterStartPart.AddToFilter(JoinCondition.Or, GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, periodStartDate);
					othersSentInTouchQuery.AddToFilter(isSentOrQueuedAfterStartPart);

					var isSentOrQueuedBeforEndPart = new ZQuery(GlbCompanyCampaignItemSchema.G8_LastSentTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, periodEndDate);
					isSentOrQueuedBeforEndPart.AddToFilter(JoinCondition.Or, GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, periodEndDate);
					othersSentInTouchQuery.AddToFilter(isSentOrQueuedBeforEndPart);
				}
				else
				{
					var isSentOrQueuedPart = new ZQuery(GlbCompanyCampaignItemSchema.G8_LastSentTimeUtc, SQLComparisonOperator.NotEqual, null);
					isSentOrQueuedPart.AddToFilter(JoinCondition.Or, GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, SQLComparisonOperator.NotEqual, null);
					othersSentInTouchQuery.AddToFilter(isSentOrQueuedPart);
				}

				var otherItemsSentInTouch = Factory.Load<GlbCompanyCampaignItem>(othersSentInTouchQuery);
				foreach (var otherItem in otherItemsSentInTouch)
				{
					Factory.AddFetchHint(ViewCampaignContactSchema.PK, otherItem.G8_RecipientID);
				}

				var othersSentInTouch = otherItemsSentInTouch.Select(x => x.RecipientFromView).Where(i => i != null);
				var othersAboutToBeSentInTouch = pendingSends.ContainsKey(targetTouch)
					? pendingSends[targetTouch].Where(x => !IsContactLimitPerOrganizationInTouchPeriodInDaysUsed || (x.ScheduleTimeUtc >= periodStartDate && x.ScheduleTimeUtc <= periodEndDate)).Select(x => x.Contact)
					: Enumerable.Empty<CampaignContact>();
				var othersSentOrAboutToBeSentInTouch = othersSentInTouch.Concat(othersAboutToBeSentInTouch).ToList();

				var othersInSameOrganizationSentOrAboutToBeSentInTouch = othersSentOrAboutToBeSentInTouch.Where(o => o.VCC_OH == contact.VCC_OH);
				if (othersInSameOrganizationSentOrAboutToBeSentInTouch.Count() >= GSC_ContactLimitPerOrganizationInTouch)
				{
					return true;
				}
			}

			if (IsContactLimitPerOrganizationInHorizontalUsed)
			{
				var touchesInSameHorizontalQuery = new ZQuery(GlbCompanyCampaignSchema.G0_HorizontalId, targetTouch.G0_HorizontalId);
				touchesInSameHorizontalQuery.AddToFilter(GlbCompanyCampaignSchema.G0_G0_Master, targetTouch.G0_G0_Master);
				var touchesInSameHorizontal = Factory.Load<GlbCompanyCampaign>(touchesInSameHorizontalQuery);

				var periodStartDate = targetDate.AddDays(-GSC_ContactLimitPerOrgInHorizontalPeriodInDays);
				var periodEndDate = targetDate.AddDays(GSC_ContactLimitPerOrgInHorizontalPeriodInDays);

				var othersSentInHorizontalQuery = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, touchesInSameHorizontal.Select(x => x.PK));
				if (IsContactLimitPerOrganizationInTouchPeriodInDaysUsed)
				{
					var isSentOrQueuedAfterStartPart = new ZQuery(GlbCompanyCampaignItemSchema.G8_LastSentTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, periodStartDate);
					isSentOrQueuedAfterStartPart.AddToFilter(JoinCondition.Or, GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, periodStartDate);
					othersSentInHorizontalQuery.AddToFilter(isSentOrQueuedAfterStartPart);

					var isSentOrQueuedBeforEndPart = new ZQuery(GlbCompanyCampaignItemSchema.G8_LastSentTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, periodEndDate);
					isSentOrQueuedBeforEndPart.AddToFilter(JoinCondition.Or, GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, periodEndDate);
					othersSentInHorizontalQuery.AddToFilter(isSentOrQueuedBeforEndPart);
				}
				else
				{
					var isSentOrQueuedPart = new ZQuery(GlbCompanyCampaignItemSchema.G8_LastSentTimeUtc, SQLComparisonOperator.NotEqual, null);
					isSentOrQueuedPart.AddToFilter(JoinCondition.Or, GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, SQLComparisonOperator.NotEqual, null);
					othersSentInHorizontalQuery.AddToFilter(isSentOrQueuedPart);
				}

				var otherItemsSentInHorizontal = Factory.Load<GlbCompanyCampaignItem>(othersSentInHorizontalQuery);
				foreach (var otherItem in otherItemsSentInHorizontal)
				{
					Factory.AddFetchHint(ViewCampaignContactSchema.PK, otherItem.G8_RecipientID);
				}

				var othersSentInHorizontal = otherItemsSentInHorizontal.Select(x => x.RecipientFromView).Where(i => i != null);
				var othersAboutToBeSentInHorizontal = touchesInSameHorizontal.SelectMany(t =>
						pendingSends.ContainsKey(t)
						? pendingSends[t].Where(x => !IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed || (x.ScheduleTimeUtc >= periodStartDate && x.ScheduleTimeUtc <= periodEndDate)).Select(x => x.Contact)
						: Enumerable.Empty<CampaignContact>());
				var othersSentOrAboutToBeSentInHorizontal = othersSentInHorizontal.Concat(othersAboutToBeSentInHorizontal).ToList();

				var othersInSameOrganizationSentOrAboutToBeSentInHorizontal = othersSentOrAboutToBeSentInHorizontal.Where(o => o.VCC_OH == contact.VCC_OH);
				if (othersInSameOrganizationSentOrAboutToBeSentInHorizontal.Count() >= GSC_ContactLimitPerOrganizationInHorizontal)
				{
					return true;
				}
			}

			return false;
		}

		public ZBool HasScheduledItems => GetScheduledItems().Any();

		public ZBool HasQueuedItems => GetQueuedItems().Any();

		IEnumerable<GlbCompanyCampaignItem> GetQueuedItems()
		{
			if (OwnerCampaign == null && OwnerCampaignGroup == null)
			{
				return Enumerable.Empty<GlbCompanyCampaignItem>();
			}

			var result = new List<GlbCompanyCampaignItem>(1);
			foreach (var touch in GetOwnerCampaigns())
			{
				result.AddRange(touch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Where(item =>
					item.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE ||
					item.G8_TrackingStatus == TrackingStatusCodes.Codes.OPQ));
			}

			return result;
		}

		public void SetBatchRecurrenceQueuedItems()
		{
			if (ScheduleTask != null)
			{
				var nextRun = ScheduleTask.S5_NextScheduledPrintRunTimeUtc;

				ScheduleTask.Run(CancellationToken.None);
				ScheduleTask.S5_NextScheduledPrintRunTimeUtc = nextRun;
			}
		}

		public void RecalculateAllScheduledTimes()
		{
			var itemsToReschedule = GetScheduledItems();
			if (!IsBatchSchedule)
			{
				itemsToReschedule = itemsToReschedule.Union(GetNonScheduledItems());
			}

			if (!itemsToReschedule.Any())
			{
				return;
			}

			foreach (var item in itemsToReschedule)
			{
				Factory.AddFetchHint(ViewCampaignContactSchema.PK, item.G8_RecipientID);
			}

			foreach (var item in itemsToReschedule)
			{
				ZDateTime scheduleTime;

				if (IsImmediate)
				{
					scheduleTime = ZDateTime.UtcNow.AddSeconds(5);
				}
				else if (IsBatchSchedule)
				{
					scheduleTime = CalculateScheduleTimeUtc(item.RecipientFromView, ScheduleTask.CalcNextRunTimeLocal.ToDateTime(), item.SenderUNLOCO);
				}
				else if (IsDelayed && IsUseCurrentTime)
				{
					scheduleTime = ZDateTime.UtcNow.AddHours(GSC_HoursOffset);
				}
				else
				{
					var dateLocal = IsDelayed
						? ZDateTime.UtcNow.Date.AddHours(GSC_HoursOffset).AddHours(GSC_ScheduleTime.Hour).AddMinutes(GSC_ScheduleTime.Minute)
						: GSC_ScheduleTime;

					scheduleTime = dateLocal.IsValid ? CalculateScheduleTimeUtc(item.RecipientFromView, dateLocal.ToDateTime(), item.SenderUNLOCO) : dateLocal;
				}

				if (scheduleTime.IsValid)
				{
					item.G8_ScheduleTimeUtc = scheduleTime;
				}
			}

			var campaignSender = new GlbCompanyCampaignSender(OwnerCampaign);
			campaignSender.SetCampaignItemsSender(itemsToReschedule.ToList());
		}

		IEnumerable<GlbCompanyCampaignItem> GetScheduledItems()
		{
			if (OwnerCampaign == null && OwnerCampaignGroup == null)
			{
				return Enumerable.Empty<GlbCompanyCampaignItem>();
			}

			var result = new List<GlbCompanyCampaignItem>(1);
			foreach (var touch in GetOwnerCampaigns())
			{
				result.AddRange(touch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.Where(item =>
						(item.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE || item.G8_TrackingStatus == TrackingStatusCodes.Codes.OPQ) &&
						!item.G8_ScheduleTimeUtc.IsEmpty));
			}

			return result;
		}

		IEnumerable<GlbCompanyCampaignItem> GetNonScheduledItems()
		{
			if (OwnerCampaign == null && OwnerCampaignGroup == null)
			{
				return Enumerable.Empty<GlbCompanyCampaignItem>();
			}

			var result = new List<GlbCompanyCampaignItem>(1);
			foreach (var touch in GetOwnerCampaigns())
			{
				result.AddRange(touch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.Where(item =>
						(item.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE || item.G8_TrackingStatus == TrackingStatusCodes.Codes.OPQ) &&
						item.G8_ScheduleTimeUtc.IsEmpty));
			}

			return result;
		}

		GlbCompanyCampaign[] GetOwnerCampaigns()
		{
			var touches = OwnerCampaign != null
				? new GlbCompanyCampaign[] { OwnerCampaign }
				: OwnerCampaignGroup.Touches.ToArray();
			return touches;
		}

		#endregion

		#region Related Business Objects

		void EnsureScheduleTaskExists()
		{
			if (ScheduleTask == null)
			{
				var task = Factory.New<GlbCompanyCampaignSendScheduleTask>();
				task.S5_ParentID = PK;
			}
		}

		GlbCompanyCampaignSendScheduleTask scheduleTask;

		public GlbCompanyCampaignSendScheduleTask ScheduleTask
		{
			get
			{
				if (scheduleTask == null)
				{
					var scheduleTaskQuery = new ZQuery(StmScheduleTaskSchema.S5_ParentID, PK);
					scheduleTaskQuery.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, GlbCompanyCampaignSendSettingsSchema.Constants.Prefix);

					scheduleTask = LoadScheduleTask(scheduleTaskQuery);
					RegisterEditableChildObject(scheduleTask);
				}

				return scheduleTask;
			}
		}

		protected virtual GlbCompanyCampaignSendScheduleTask LoadScheduleTask(ZQuery scheduleTaskQuery)
		{
			return Factory.LoadTop1<GlbCompanyCampaignSendScheduleTask>(scheduleTaskQuery);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (ScheduleTask != null && ScheduleTask.IsInDatabase && !ScheduleTask.HasChanges)
			{
				scheduleTask.Reload();
			}
		}

		#endregion

		#region Save

		public override void OnSaving()
		{
			base.OnSaving();

			var task = ScheduleTask;
			if (task != null)
			{
				if (OwnerCampaign != null)
				{
					OwnerCampaign.G0_LastSentBatchLocalDate = ZDateTime.Empty;
				}
				else if (OwnerCampaignGroup != null)
				{
					foreach (var touch in OwnerCampaignGroup.Touches)
					{
						touch.G0_LastSentBatchLocalDate = ZDateTime.Empty;
					}
				}

				if (!task.IsDeleted)
				{
					if (!IsBatchSchedule)
					{
						task.S5_IsActive = false;
					}
					else if (!task.IsInDatabase || GSC_ScheduleTypeInfo.HasChanges)
					{
						task.S5_IsActive = task.ParentHasQueuedForScheduleItem();
					}
				}
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			ScheduleTask?.Delete();
			base.Delete();
		}

		#endregion
	}
}
