using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignItemSchedule : NonPersistentBusinessObject
	{
		public GlbCompanyCampaignItemSchedule(BusinessObjectFactory factory)
			: base(factory)
		{
			isRecipientsLocalTimeUsed = true;
			senderTimeZone = Env.CurrentBranch.NKUNLOCO;
		}

		public GlbCompanyCampaignItemSchedule(GlbCompanyCampaign campaign)
			: this(campaign.Factory)
		{
			this.Campaign = campaign;
		}

		readonly GlbCompanyCampaign Campaign;

		public GlbCompanyCampaign Parent
		{
			get { return Campaign; }
		}

		public bool SaveRelatedBusinessObjects()
		{
			GlbCampaignContactCollection contactCollection = new GlbCampaignContactCollection(Campaign);
			var tableCode = SelectedScheduleItems.Any() ? SelectedScheduleItems.FirstOrDefault().TableCode.ToString() : GlbCompanyCampaignItemSchema.Constants.Prefix;

			ScheduleItemDataLoader loader = new ScheduleItemDataLoader(this);

			foreach (ScheduleCampaignItems item in ScheduleItemsCollection)
			{
				if (item.Status == TrackingStatusCodes.Codes.QUE)
				{
					if (tableCode == GlbCompanyCampaignItemSchema.Constants.Prefix)
					{
						var campaignItems = SelectedScheduleItems.Cast<GlbCompanyCampaignItem>().Where(c =>
							c.RecipientFromView != null
							&& loader.OffsetFromUtcTimeZone(c.RecipientFromView.RelatedPortCodeForScheduling, loader.GetUtcFromUnlocoTime(c.RecipientFromView.RelatedPortCodeForScheduling, item.ScheduleSendTimeUTC)) == item.UtcOffset
							&& loader.CivilianTimeZoneCodeTimeZone(c.RecipientFromView.RelatedPortCodeForScheduling, loader.GetUtcFromUnlocoTime(c.RecipientFromView.RelatedPortCodeForScheduling, item.ScheduleSendTimeUTC)) == item.StandardTimeZoneCode);

						foreach (var campaignItem in campaignItems)
						{
							((GlbCompanyCampaignItem)Campaign.CampaignsItemsSent.FindByPK(campaignItem.PK)).G8_ScheduleTimeUtc = item.ScheduleSendTimeUTC;
						}
					}
					else
					{
						var contacts = SelectedScheduleItems.Cast<CampaignContact>().Where(c => loader.OffsetFromUtcTimeZone(c.RelatedPortCodeForScheduling, loader.GetUtcFromUnlocoTime(c.RelatedPortCodeForScheduling, item.ScheduleSendTimeUTC)) == item.UtcOffset &&
							loader.CivilianTimeZoneCodeTimeZone(c.RelatedPortCodeForScheduling, loader.GetUtcFromUnlocoTime(c.RelatedPortCodeForScheduling, item.ScheduleSendTimeUTC)) == item.StandardTimeZoneCode);

						foreach (var contact in contacts)
						{
							contact.ScheduleData = new ScheduleData(contact, item.ScheduleSendTimeUTC);
							contactCollection.Add(contact);
						}
					}
				}
			}

			if (contactCollection.Any())
			{
				return OnSendScheduleEvent(new Collection<CampaignContact>(contactCollection.Cast<CampaignContact>().ToList()));
			}

			return true;
		}

		void UpdateScheduleSendTime(string unloco = "")
		{
			if (scheduleSendTimeLocal.IsValid)
			{
				ZDateTime dateTimeInUse = string.IsNullOrEmpty(unloco) ? scheduleSendTimeLocal : Env.Time.GetTimeInOneZoneFromTimeInAnotherZone(unloco, scheduleSendTimeLocal.ToDateTime(), Env.CurrentBranch.NKUNLOCO);
				ScheduleItemsCollection.LoadData(this, dateTimeInUse, true);
			}
		}

		void UpdateScheduleItemSendTimeUtc()
		{
			DateTime utcTime = Env.Time.GetUtcFromUnlocoTime((!senderTimeZone.IsEmpty ? senderTimeZone.ToString() : Env.CurrentBranch.NKUNLOCO), scheduleSendTimeLocal.ToDateTime());
			foreach (ScheduleCampaignItems scheduleItem in ScheduleItemsCollection)
			{
				scheduleItem.ScheduleSendTimeUTC = utcTime;
			}
		}

		public delegate void SaveAndSendScheduleEventHandler<TEventArgs>(object sender, TEventArgs e);
		public event SaveAndSendScheduleEventHandler<SendScheduleEventArgs> SendScheduleEventHandler;

		bool OnSendScheduleEvent(Collection<CampaignContact> contacts)
		{
			if (SendScheduleEventHandler != null)
			{
				SendScheduleEventArgs arg = new SendScheduleEventArgs(contacts);
				SendScheduleEventHandler(null, arg);
				return arg.Result;
			}

			return false;
		}

		#region Properties

		public ZBool ItemsDeleted;
		public ZInt ContactsWithoutTimeZone;

		public ZDateTime ScheduleSendTimeLocal
		{
			get { return scheduleSendTimeLocal; }
			set
			{
				SetNonPersistentPropertyValue(ScheduleSendTimeLocalInfo, ref scheduleSendTimeLocal, value);
				if (IsRecipientsLocalTimeUsed)
				{
					string unloco = isRecipientsLocalTimeUsed ? "" : (!senderTimeZone.IsEmpty ? senderTimeZone.ToString() : Env.CurrentBranch.NKUNLOCO);
					UpdateScheduleSendTime(unloco);
				}
				else
				{
					UpdateScheduleItemSendTimeUtc();
				}
			}
		}
		ZDateTime scheduleSendTimeLocal;
		public ZPropertyInfo ScheduleSendTimeLocalInfo
		{
			get { return GetZPropertyInfo(nameof(ScheduleSendTimeLocal)); }
		}

		public ZString SenderTimeZone
		{
			get { return senderTimeZone; }
			set
			{
				SetNonPersistentPropertyValue(SenderTimeZoneInfo, ref senderTimeZone, value);
				Validation.ValidateScheduleSendTimeLocal();
				if (!ScheduleSendTimeLocal.IsEmpty)
				{
					UpdateScheduleItemSendTimeUtc();
				}
			}
		}
		ZString senderTimeZone;
		public ZPropertyInfo SenderTimeZoneInfo
		{
			get { return GetZPropertyInfo(nameof(SenderTimeZone)); }
		}
		public bool SenderTimeZone_ReadOnly
		{
			get { return IsRecipientsLocalTimeUsed; }
		}

		public ZString SenderTimeZoneDescription
		{
			get
			{
				ZString senderTimeZoneDescription = "";
				RefTimeZoneSet timeZoneSet = DataLoader.GetLocationTimeZoneSet(SenderTimeZone);
				if (timeZoneSet != null)
				{
					senderTimeZoneDescription = timeZoneSet.R3_TimeZoneSetName + " " + ScheduleItemDataLoader.AddUTCPrefix(DataLoader.OffsetFromUtcTimeZone(SenderTimeZone, (scheduleSendTimeLocal.IsValid ? scheduleSendTimeLocal.ToDateTime() : ZDateTime.UtcNow.ToDateTime())));
				}
				return senderTimeZoneDescription.ToUpperInvariant();
			}
		}

#if DEBUG
		public
#endif
		ScheduleItemDataLoader DataLoader
		{
			get { return dataLoader ?? (dataLoader = new ScheduleItemDataLoader(this)); }
		}
		ScheduleItemDataLoader dataLoader;

		public ZString NoTimeZoneDescription
		{
			get
			{
				ZString noTimeZoneDescription = "";
				var contactsWithoutTimeZone = ContactsWithoutTimeZone;
				if (contactsWithoutTimeZone > 0)
				{
					noTimeZoneDescription = Res.GetString("5a70578d-1c84-4123-b650-09adc35445de", "No time zone exists for {0} contacts. These have been included to the sender's local time zone.", contactsWithoutTimeZone);
				}
				return noTimeZoneDescription;
			}
		}

		public ZBool IsSendersLocalTimeUsed
		{
			get { return isSendersLocalTimeUsed; }
			set
			{
				SetNonPersistentPropertyValue(IsSendersLocalTimeUsedInfo, ref isSendersLocalTimeUsed, value);
				Validation.ValidateAll();
				ScheduleItemsCollection.SetReadOnlyIncludingChildren(isSendersLocalTimeUsed);
				if (!ScheduleSendTimeLocal.IsEmpty && isSendersLocalTimeUsed)
				{
					UpdateScheduleItemSendTimeUtc();
				}
			}
		}
		ZBool isSendersLocalTimeUsed;
		public ZPropertyInfo IsSendersLocalTimeUsedInfo
		{
			get { return GetZPropertyInfo(nameof(IsSendersLocalTimeUsed)); }
		}

		public ZBool IsRecipientsLocalTimeUsed
		{
			get { return isRecipientsLocalTimeUsed; }
			set
			{
				SetNonPersistentPropertyValue(IsRecipientsLocalTimeUsedInfo, ref isRecipientsLocalTimeUsed, value);
				Validation.ValidateScheduleSendTimeLocal();
				if (!ScheduleSendTimeLocal.IsEmpty && isRecipientsLocalTimeUsed)
				{
					UpdateScheduleSendTime();
				}
			}
		}
		ZBool isRecipientsLocalTimeUsed;
		public ZPropertyInfo IsRecipientsLocalTimeUsedInfo
		{
			get { return GetZPropertyInfo(nameof(IsRecipientsLocalTimeUsed)); }
		}

		public GlbCompanyCampaignItemScheduleItemsCollection ScheduleItemsCollection
		{
			get
			{
				if (scheduleItemsCollection == null)
				{
					scheduleItemsCollection = new GlbCompanyCampaignItemScheduleItemsCollection(Parent);
					scheduleItemsCollection.LoadData(this, ZDateTime.Now);
				}
				return scheduleItemsCollection;
			}
		}
		GlbCompanyCampaignItemScheduleItemsCollection scheduleItemsCollection;

		public LinkedHashSet<IScheduleItemsProvider> SelectedScheduleItems
		{
			get { return selectedScheduleItems ?? (selectedScheduleItems = new LinkedHashSet<IScheduleItemsProvider>()); }
		}
		LinkedHashSet<IScheduleItemsProvider> selectedScheduleItems;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			ScheduleItemsCollection.RunPreSaveValidation();
			base.RunPreSaveValidationCore();
		}

		public GlbCompanyCampaignItemScheduleValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual GlbCompanyCampaignItemScheduleValidation GetNewValidation()
		{
			return new GlbCompanyCampaignItemScheduleValidation(this);
		}

		#endregion

		#region Lookups

		public RefUNLOCOCollection UNLOCOs
		{
			get { return new RefUNLOCOCollection(new BusinessObjectFactory()); }
		}

		#endregion

		public class SendScheduleEventArgs : EventArgs
		{
			readonly Collection<CampaignContact> contacts;

			public SendScheduleEventArgs(Collection<CampaignContact> contacts)
			{
				this.contacts = contacts;
			}

			public bool Result { get; set; }

			public Collection<CampaignContact> Contacts
			{
				get { return contacts; }
			}
		}
	}

	public class ScheduleData
	{
		public ScheduleData(CampaignContact contact, ZDateTime scheduleTimeUtc, int batchNumber = 0)
		{
			Contact = contact;
			ScheduleTimeUtc = scheduleTimeUtc;
			BatchNumber = batchNumber;
		}

		public readonly CampaignContact Contact;
		public readonly ZDateTime ScheduleTimeUtc;
		public readonly ZInt BatchNumber;
	}
}
