using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class ScheduleCampaignItems : NonPersistentBusinessObject
	{
		public ScheduleCampaignItems()
			: base()
		{
		}

		public ScheduleCampaignItems(GlbCompanyCampaign campaign)
			: this()
		{
			this.Campaign = campaign;
		}

		public readonly GlbCompanyCampaign Campaign;

		bool skipTimeAdjustment;

		public IDisposable SkipAdjustingTimeOffsets()
		{
			skipTimeAdjustment = true;
			return new SyncLocalAndUTCTimes(this);
		}

		class SyncLocalAndUTCTimes : IDisposable
		{
			public SyncLocalAndUTCTimes(ScheduleCampaignItems scheduled)
			{
				this.scheduled = scheduled;
			}

			readonly ScheduleCampaignItems scheduled;

			public void Dispose()
			{
				scheduled.skipTimeAdjustment = false;
			}
		}

		#region Properties

		public ZDateTime ScheduleSendTimeUTC
		{
			get { return scheduleSendTimeUTC; }
			set
			{
				SetNonPersistentPropertyValue(ScheduleSendTimeUTCInfo, ref scheduleSendTimeUTC, value);
				if (!skipTimeAdjustment && scheduleSendTimeUTC.IsValid)
				{
					scheduleSendTimeLocal = scheduleSendTimeUTC.AddMinutes(utcOffset);
				}
			}
		}
		ZDateTime scheduleSendTimeUTC;
		public ZPropertyInfo ScheduleSendTimeUTCInfo
		{
			get { return GetZPropertyInfo(nameof(ScheduleSendTimeUTC)); }
		}
		protected bool ScheduleSendTimeUTC_ReadOnly
		{
			get { return Status != TrackingStatusCodes.Codes.QUE; }
		}

		public ZShort UtcOffset
		{
			get { return utcOffset; }
			set { SetNonPersistentPropertyValue(UtcOffsetInfo, ref utcOffset, value); }
		}
		ZShort utcOffset;
		public ZPropertyInfo UtcOffsetInfo
		{
			get { return GetZPropertyInfo(nameof(UtcOffset)); }
		}
		protected bool UtcOffset_ReadOnly
		{
			get { return true; }
		}

		public ZString UtcOffsetText
		{
			get { return ScheduleItemDataLoader.AddUTCPrefix(UtcOffset); }
		}

		public ZString TimeZone
		{
			get { return timeZone; }
			set { SetNonPersistentPropertyValue(TimeZoneInfo, ref timeZone, value); }
		}
		ZString timeZone;
		public ZPropertyInfo TimeZoneInfo
		{
			get { return GetZPropertyInfo(nameof(TimeZone)); }
		}
		protected bool TimeZone_ReadOnly
		{
			get { return true; }
		}

		public ZString StandardTimeZoneCode
		{
			get { return standardTimeZoneCode; }
			set { SetNonPersistentPropertyValue(StandardTimeZoneCodeInfo, ref standardTimeZoneCode, value); }
		}
		ZString standardTimeZoneCode;
		public ZPropertyInfo StandardTimeZoneCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StandardTimeZoneCode)); }
		}
		protected bool StandardTimeZoneCode_ReadOnly
		{
			get { return true; }
		}

		public ZInt ContactsCount
		{
			get { return contactsCount; }
			set { SetNonPersistentPropertyValue(ContactsCountInfo, ref contactsCount, value); }
		}
		ZInt contactsCount;
		public ZPropertyInfo ContactsCountInfo
		{
			get { return GetZPropertyInfo(nameof(ContactsCount)); }
		}
		protected bool ContactsCount_ReadOnly
		{
			get { return true; }
		}

		public ZDateTime ScheduleSendTimeLocal
		{
			get { return scheduleSendTimeLocal; }
			set
			{
				SetNonPersistentPropertyValue(ScheduleSendTimeLocalInfo, ref scheduleSendTimeLocal, value);
				if (!skipTimeAdjustment && scheduleSendTimeLocal.IsValid)
				{
					scheduleSendTimeUTC = scheduleSendTimeLocal.AddMinutes(-utcOffset);
				}
			}
		}
		ZDateTime scheduleSendTimeLocal;
		public ZPropertyInfo ScheduleSendTimeLocalInfo
		{
			get { return GetZPropertyInfo(nameof(ScheduleSendTimeLocal)); }
		}
		protected bool ScheduleSendTimeLocal_ReadOnly
		{
			get { return Status != TrackingStatusCodes.Codes.QUE; }
		}

		public ZString Status
		{
			get { return status; }
			set { SetNonPersistentPropertyValue(StatusInfo, ref status, value); }
		}
		ZString status;
		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(nameof(Status)); }
		}
		protected bool Status_ReadOnly
		{
			get { return true; }
		}

		public ZString StatusText
		{
			get
			{
				if (Status == TrackingStatusCodes.Codes.QUE)
				{
					return ResString.GetMultilingualString("f28eeb96-9f08-4a2d-841d-32797a80c447", "Queued");
				}
				return ResString.GetMultilingualString("3d296703-1553-403f-a7cd-69fccdf808b0", "Sent");
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore(); // call RunPreSaveValidationCore() on all children then fire OnNotificationsChanged()
		}

		public ScheduleCampaignItemsValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual ScheduleCampaignItemsValidation GetNewValidation()
		{
			return new ScheduleCampaignItemsValidation(this);
		}

		#endregion
	}
}
