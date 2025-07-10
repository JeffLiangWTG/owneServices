using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class ScheduleItemDataLoader
	{
		public ScheduleItemDataLoader(GlbCompanyCampaignItemSchedule itemSchedule)
		{
			ItemSchedule = itemSchedule;
		}

		readonly GlbCompanyCampaignItemSchedule ItemSchedule;

		public static class Schema
		{
			public const string SendTimeLocal = "Send Time (Local)";
			public const string SendTimeUtc = "Send Time (UTC)";
			public const string SentStatus = "SNT";
			public const string UtcOffsetIdentifier = "UTC";
		}

#if DEBUG
		public
#endif
		ITimeZone TimeZoneInfo;

		RefTimeZone GetLocationInfo(string unloco, DateTime utcDateTime)
		{
			var timeZoneSet = GetLocationTimeZoneSet(unloco);

			if (timeZoneSet != null)
			{
				if (!Globals.IsTest)
				{
					TimeZoneInfo = timeZoneSet.GetCalculationTimeZone();
				}

				if (TimeZoneInfo != null && TimeZoneInfo.IsDaylightSavingBasedOnUtc(utcDateTime))
				{
					return timeZoneSet.DaylightSavingZone;
				}
				return timeZoneSet.StandardZone;
			}
			return null;
		}

		ZDateTime CampaignItemScheduleDateTimeToUse(GlbCompanyCampaignItem campaignItem, ZDateTime scheduleDateTimeLocal, bool isEditing)
		{
			if ((isEditing && campaignItem.ScheduleStatus == TrackingStatusCodes.Codes.QUE) || !campaignItem.G8_ScheduleTimeUtc.IsValid)
			{
				return scheduleDateTimeLocal;
			}
			return campaignItem.G8_ScheduleTimeUtc;
		}

		LinkedHashSet<ScheduleCampaignItems> LoadScheduleItemsFromCampaignItems(ZDateTime scheduleDateTimeLocal, bool isEditing)
		{
			var query = new ZDBOnlyQuery(typeof(CampaignContact));
			var subQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID);
			subQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, ItemSchedule.Parent.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			query.ReLoadExistingRows = true;
			ItemSchedule.Factory.AddFetchHint(typeof(CampaignContact), query);

			var currentBranchUNLOCO = Env.CurrentBranch.NKUNLOCO;
			RefTimeZoneSet timeZoneSet = GetLocationTimeZoneSet(currentBranchUNLOCO);

			var items = new LinkedHashSet<ScheduleCampaignItems>();
			var contactsWithoutTimeZone = ItemSchedule.SelectedScheduleItems.Cast<GlbCompanyCampaignItem>().Where(s => s.RecipientFromView != null && s.RecipientFromView.VCC_CivilianZone.IsEmpty && s.RecipientFromView.VCC_TimeZoneSetName.IsEmpty);
			ItemSchedule.ContactsWithoutTimeZone = contactsWithoutTimeZone.Count();

			foreach (var campaignItem in contactsWithoutTimeZone)
			{
				using (campaignItem.RecipientFromView.SuspendSettingHasChanges())
				{
					campaignItem.RecipientFromView.VCC_OffsetMinutesFromUtc = OffsetFromUtcTimeZone(currentBranchUNLOCO, GetUtcFromUnlocoTime(currentBranchUNLOCO, CampaignItemScheduleDateTimeToUse(campaignItem, scheduleDateTimeLocal, isEditing)));
					campaignItem.RecipientFromView.VCC_TimeZoneSetName = timeZoneSet.R3_TimeZoneSetName;
					campaignItem.RecipientFromView.VCC_CivilianZone = CivilianTimeZoneCodeTimeZone(currentBranchUNLOCO, GetUtcFromUnlocoTime(currentBranchUNLOCO, CampaignItemScheduleDateTimeToUse(campaignItem, scheduleDateTimeLocal, isEditing)));
					campaignItem.RecipientFromView.RelatedPortCodeForScheduling = currentBranchUNLOCO;
				}
			}

			ScheduleCampaignItems item = new ScheduleCampaignItems(ItemSchedule.Parent);

			using (item.SuspendSettingHasChanges())
			{
				using (item.SkipAdjustingTimeOffsets())
				{
					var result = from campaignItem in ItemSchedule.SelectedScheduleItems.Cast<GlbCompanyCampaignItem>()
								 where campaignItem.RecipientFromView != null
								 group campaignItem by new
								 {
									 OffsetFromUtcTimeZone = OffsetFromUtcTimeZone(campaignItem.RecipientFromView.RelatedPortCodeForScheduling, GetUtcFromUnlocoTime(campaignItem.RecipientFromView.RelatedPortCodeForScheduling, CampaignItemScheduleDateTimeToUse(campaignItem, scheduleDateTimeLocal, isEditing))),
									 CivilianTimeZoneCodeTimeZone = CivilianTimeZoneCodeTimeZone(campaignItem.RecipientFromView.RelatedPortCodeForScheduling, GetUtcFromUnlocoTime(campaignItem.RecipientFromView.RelatedPortCodeForScheduling, CampaignItemScheduleDateTimeToUse(campaignItem, scheduleDateTimeLocal, isEditing))),
									 campaignItem.ScheduleStatus,
									 G8_ScheduleTimeUtc = !isEditing ? campaignItem.G8_ScheduleTimeUtc : GetUtcFromUnlocoTime(campaignItem.RecipientFromView.RelatedPortCodeForScheduling, CampaignItemScheduleDateTimeToUse(campaignItem, scheduleDateTimeLocal, isEditing))
								 } into grp
								 select new ScheduleCampaignItems(ItemSchedule.Parent)
								 {
									 UtcOffset = grp.Key.OffsetFromUtcTimeZone,
									 ContactsCount = grp.Count(),
									 Status = grp.Key.ScheduleStatus,
									 ScheduleSendTimeUTC = grp.Key.G8_ScheduleTimeUtc,
									 TimeZone = string.Join(", ", grp.Select(i => i.RecipientFromView.VCC_TimeZoneSetName).Distinct().OrderBy(s => s)),
									 StandardTimeZoneCode = grp.Key.CivilianTimeZoneCodeTimeZone
								 };

					items.UnionWith(result);
				}
			}

			return items;
		}

		LinkedHashSet<ScheduleCampaignItems> LoadScheduleItemsFromCampaignContact(ZDateTime scheduleDateTimeLocal)
		{
			var currentBranchUNLOCO = Env.CurrentBranch.NKUNLOCO;
			RefTimeZoneSet timeZoneSet = GetLocationTimeZoneSet(currentBranchUNLOCO);

			var items = new LinkedHashSet<ScheduleCampaignItems>();
			var contactsWithoutTimeZone = ItemSchedule.SelectedScheduleItems.Cast<CampaignContact>().Where(s => s.VCC_TimeZoneSetName.IsEmpty && s.VCC_CivilianZone.IsEmpty);
			ItemSchedule.ContactsWithoutTimeZone = contactsWithoutTimeZone.Count();

			foreach (var contact in contactsWithoutTimeZone)
			{
				using (contact.SuspendSettingHasChanges())
				{
					contact.VCC_OffsetMinutesFromUtc = OffsetFromUtcTimeZone(currentBranchUNLOCO, GetUtcFromUnlocoTime(currentBranchUNLOCO, scheduleDateTimeLocal));
					contact.VCC_TimeZoneSetName = timeZoneSet.R3_TimeZoneSetName;
					contact.VCC_CivilianZone = CivilianTimeZoneCodeTimeZone(currentBranchUNLOCO, GetUtcFromUnlocoTime(currentBranchUNLOCO, scheduleDateTimeLocal));
					contact.RelatedPortCodeForScheduling = currentBranchUNLOCO;
				}
			}

			ScheduleCampaignItems item = new ScheduleCampaignItems(ItemSchedule.Parent);
			using (item.SuspendSettingHasChanges())
			{
				using (item.SkipAdjustingTimeOffsets())
				{
					var result = from contact in ItemSchedule.SelectedScheduleItems.Cast<CampaignContact>()
								 group contact by new
								 {
									 OffsetFromUtcTimeZone = OffsetFromUtcTimeZone(contact.RelatedPortCodeForScheduling, GetUtcFromUnlocoTime(contact.RelatedPortCodeForScheduling, scheduleDateTimeLocal)),
									 CivilianTimeZoneCodeTimeZone = CivilianTimeZoneCodeTimeZone(contact.RelatedPortCodeForScheduling, GetUtcFromUnlocoTime(contact.RelatedPortCodeForScheduling, scheduleDateTimeLocal)),
								 } into grp
								 select new ScheduleCampaignItems(ItemSchedule.Parent)
								 {
									 UtcOffset = grp.Key.OffsetFromUtcTimeZone,
									 ContactsCount = grp.Count(),
									 ScheduleSendTimeLocal = scheduleDateTimeLocal,
									 Status = TrackingStatusCodes.Codes.QUE,
									 TimeZone = string.Join(", ", grp.Select(i => i.VCC_TimeZoneSetName).Distinct().OrderBy(s => s)),
									 StandardTimeZoneCode = grp.Key.CivilianTimeZoneCodeTimeZone
								 };

					items.UnionWith(result);
				}
			}

			return items;
		}

		public LinkedHashSet<ScheduleCampaignItems> LoadScheduleItems(ZDateTime scheduleDateTimeLocal, bool isEditing = false)
		{
			var scheduleItem = ItemSchedule.SelectedScheduleItems.FirstOrDefault();

			if (scheduleItem != null)
			{
				return scheduleItem.TableCode == GlbCompanyCampaignItemSchema.Constants.Prefix
					? LoadScheduleItemsFromCampaignItems(scheduleDateTimeLocal, isEditing)
					: LoadScheduleItemsFromCampaignContact(scheduleDateTimeLocal);
			}

			return new LinkedHashSet<ScheduleCampaignItems>();
		}

		public RefTimeZoneSet GetLocationTimeZoneSet(string unloco)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RefTimeZoneSet));
			ZDBOnlySubQuery unlocoQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_R3);
			unlocoQuery.AddToFilter(RefUNLOCOSchema.RL_Code, unloco);
			query.AddSubQuery(unlocoQuery, JoinCondition.And);
			return ItemSchedule.Parent.Factory.LoadTop1<RefTimeZoneSet>(query);
		}

		public static string AddUTCPrefix(short utcOffset)
		{
			TimeSpan span = TimeSpan.FromMinutes(utcOffset);
			return Schema.UtcOffsetIdentifier + (utcOffset >= 0 ? "+" : "") + span.ToHoursAndMinutesString();
		}

		public ZString CivilianTimeZoneCodeTimeZone(string unloco, DateTime utcDateTime)
		{
			var timeZoneSet = GetLocationInfo(unloco, utcDateTime);
			if (timeZoneSet != null)
			{
				return timeZoneSet.R2_CivilianTimeZoneCode;
			}
			return ZString.Empty;
		}

		public ZShort OffsetFromUtcTimeZone(string unloco, DateTime utcDateTime)
		{
			var timeZoneSet = GetLocationInfo(unloco, utcDateTime);
			if (timeZoneSet != null)
			{
				return timeZoneSet.R2_OffsetMinutesFromUTC;
			}
			return ZShort.Zero;
		}

		public DateTime GetUtcFromUnlocoTime(string unloco, ZDateTime localDateTime)
		{
			return Env.Time.GetUtcFromUnlocoTime(unloco, localDateTime.ToDateTime());
		}
	}
}
