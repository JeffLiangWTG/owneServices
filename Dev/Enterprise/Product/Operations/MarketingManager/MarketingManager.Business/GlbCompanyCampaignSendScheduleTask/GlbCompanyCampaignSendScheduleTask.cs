using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSendScheduleTask : StmScheduleTask, IGlbCompanyCampaignSendScheduleTask
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int backDateValidMinutes = 15;
		public GlbCompanyCampaignSendScheduleTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S5_ParentTableCode = GlbCompanyCampaignSendSettingsSchema.Constants.Prefix;
			S5_GB = ZGuid.Empty;
		}

		#endregion

		#region Run

		protected override void RunCore(INotifications notifications, CancellationToken token)
		{
			if (!S5_NextScheduledPrintRunTimeUtc.IsValid)
			{
				notifications.AddError(Res.GetString("0a7c4ac6-fa6c-4bdd-a4b5-8aa722a5b6a3", "Schedule Time not valid"));
				return;
			}

			var sendSettings = SendSettings;
			if (sendSettings == null)
			{
				notifications.AddError(Res.GetString("6ecfb90e-14eb-4093-aeeb-8cca04b2b250", "Campaign Batch Recurrence Send Settings not found. Settings ID = {0}", S5_ParentID));
				return;
			}

			var targetTouches = new List<GlbCompanyCampaign>();
			if (sendSettings.OwnerCampaign == null && sendSettings.OwnerCampaignGroup == null)
			{
				notifications.AddError(Res.GetString("f52f79b8-61d5-496e-b139-279ba75a2b43", "Send Setting Owner not found. Campaign ID = {0} / Campaign Group = {1}", sendSettings.GSC_G0_Campaign, sendSettings.GSC_GCG_Group));
				return;
			}
			if (sendSettings.OwnerCampaign != null)
			{
				targetTouches.Add(sendSettings.OwnerCampaign);
			}
			else
			{
				targetTouches.AddRange(sendSettings.OwnerCampaignGroup.Touches);
			}

			var existingCache = new Dictionary<string, bool>();
			foreach (var touch in targetTouches)
			{
				ResetQueuedItems(touch.PK);

				var batchNumber = touch.G0_LastSentBatchNumber + 1;
				var zLocalTime = GetLocalTime();
				var localTime = zLocalTime.ToDateTime();
				var campaignItemsQuery = GetCampaignItemToScheduleQuery(touch, sendSettings);
				var reader = new DbOnlyBusinessObjectQueue<GlbCompanyCampaignItem>(campaignItemsQuery);
				var itemsProcessed = new List<GlbCompanyCampaignItem>();
				var campaignSender = new GlbCompanyCampaignSender(touch);

				reader.ProcessBatch((items, cancelEventArgs) =>
				{
					var countProcessed = 0;
					var factory = items[0].Factory;

					if (sendSettings.GSC_IsRecipientLocalTime)
					{
						foreach (var item in items)
						{
							factory.AddFetchHint(ViewCampaignContactSchema.PK, item.G8_RecipientID);
						}
					}

					foreach (var item in items)
					{
						var proposedDate = sendSettings.CalculateScheduleTimeUtc(item.RecipientFromView, localTime, item.SenderUNLOCO);
						if (proposedDate <= ZDateTime.UtcNow.AddMinutes(-backDateValidMinutes))
						{
							var newLocalTime = GetLocalDate(Recurrence.CalculateNextScheduleDate()).ToDateTime();
							proposedDate = sendSettings.CalculateScheduleTimeUtc(item.RecipientFromView, newLocalTime, item.SenderUNLOCO);
						}

						var key = item.G8_G0.ToString() + proposedDate.SqlFormat + batchNumber;
						if (!existingCache.TryGetValue(key, out var exists))
						{
							var query = GetExistingItemsQuery(item.G8_G0, proposedDate, batchNumber);
							exists = factory.ExistsInDatabase(GlbCompanyCampaignItem.Schema.TableName, query);
							existingCache.Add(key, exists);
						}

						if (exists)
						{
							continue;
						}

						item.G8_ScheduleTimeUtc = proposedDate;
						item.G8_BatchNumber = batchNumber;

						if (countProcessed == 0)
						{
							items[0].CompanyCampaign.G0_LastSentBatchNumber = batchNumber;
							items[0].CompanyCampaign.G0_LastSentBatchLocalDate = localTime;
						}

						countProcessed++;
						itemsProcessed.Add(item);
					}

					campaignSender.SetCampaignItemsSender(itemsProcessed);
					NotifyAndSave(notifications, touch.PK, countProcessed, factory);
				}, 1000, token);

				S5_IsActive = CampaignHasQueuedForScheduleItem(touch.PK);
				Factory.Save();
			}
		}

		void ResetQueuedItems(ZGuid campaignPk)
		{
			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, campaignPk);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, new List<string> { TrackingStatusCodes.Codes.QUE, TrackingStatusCodes.Codes.OPQ });

			var items = newFactory.Load<GlbCompanyCampaignItem>(query);
			if (items.Any())
			{
				foreach (var item in items)
				{
					item.G8_ScheduleTimeUtc = ZDateTime.Empty;
					item.G8_GS_NKSender = ZString.Empty;
					item.G8_EmailSenderName = ZString.Empty;
					item.G8_SenderEmailAddress = ZString.Empty;
				}

				newFactory.Save();
			}
		}

		protected virtual void NotifyAndSave(INotifications notifications, ZGuid touchPK, int countProcessed, BusinessObjectFactory factory)
		{
			if (countProcessed > 0)
			{
				notifications.Add(
					CargoWise.ComponentModel.NotificationType.Information,
					Res.GetString("C7CC8762-C532-496E-B2BD-B790475FD76F", "Scheduled {0} records for touch {1}.", countProcessed, touchPK));

				factory.Save();
			}
		}

		ZQuery GetExistingItemsQuery(ZGuid campaignPK, ZDateTime scheduleDate, ZInt batchNumber)
		{
			var query = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, campaignPK);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, new List<string> { TrackingStatusCodes.Codes.QUE, TrackingStatusCodes.Codes.OPQ });
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, scheduleDate);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_BatchNumber, SQLComparisonOperator.NotEqual, batchNumber);

			return query;
		}

		ZDateTime GetLocalTime()
		{
			var zLocalTime = Recurrence.NextScheduledPrintRunTimeLocal;
			if (!zLocalTime.IsValid)
			{
				return zLocalTime;
			}

			if (zLocalTime < ZDateTime.Now)
			{
				UpdateNextScheduledDate();
			}

			return Recurrence.NextScheduledPrintRunTimeLocal;
		}

		bool CampaignHasQueuedForScheduleItem(ZGuid campaignPk)
		{
			var queuedForScheduleQuery = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, campaignPk);
			queuedForScheduleQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, new List<string> { TrackingStatusCodes.Codes.QUE, TrackingStatusCodes.Codes.OPQ });
			queuedForScheduleQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, null);
			return Factory.LoadTop1<GlbCompanyCampaignItem>(queuedForScheduleQuery) != null;
		}

		ZNonPersistentDataQuery GetCampaignItemToScheduleQuery(GlbCompanyCampaign targetCampaign, GlbCompanyCampaignSendSettings sendSettings)
		{
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@TouchPk", targetCampaign.PK, GlbCompanyCampaignSchema.PK);
			parameters.Add("@LimitThisBatch", sendSettings.GSC_ContactLimitEachBatch, GlbCompanyCampaignSendSettingsSchema.GSC_ContactLimitEachBatch);
			parameters.Add("@LimitPerOrganizationThisBatch", sendSettings.GSC_ContactLimitPerOrganizationEachBatch, GlbCompanyCampaignSendSettingsSchema.GSC_ContactLimitPerOrganizationEachBatch);

			parameters.Add("@LimitPerOrganizationInHorizontal", sendSettings.GSC_ContactLimitPerOrganizationInHorizontal, GlbCompanyCampaignSendSettingsSchema.GSC_ContactLimitPerOrganizationInHorizontal);
			parameters.Add("@LimitPerOrganizationInHorizontalPeriodStartDateUtc",
				sendSettings.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed ? S5_NextScheduledPrintRunTimeUtc.AddDays(-sendSettings.GSC_ContactLimitPerOrgInHorizontalPeriodInDays) : null,
				CargoWise.Schema.Schema.GenericDateTimeColumn);

			parameters.Add("@LimitPerOrganizationInTouch", sendSettings.GSC_ContactLimitPerOrganizationInTouch, GlbCompanyCampaignSendSettingsSchema.GSC_ContactLimitPerOrganizationInTouch);
			parameters.Add("@LimitPerOrganizationInTouchPeriodStartDateUtc",
				sendSettings.IsContactLimitPerOrganizationInTouchPeriodInDaysUsed ? S5_NextScheduledPrintRunTimeUtc.AddDays(-sendSettings.GSC_ContactLimitPerOrganizationInTouchPeriodInDays) : null,
				CargoWise.Schema.Schema.GenericDateTimeColumn);

			var sql = @"
SELECT G8_PK
FROM GetNextCampaignItemScheduleBatch(
	@TouchPk,
	@LimitThisBatch,
	@LimitPerOrganizationThisBatch,
	@LimitPerOrganizationInHorizontal,
	@LimitPerOrganizationInHorizontalPeriodStartDateUtc,
	@LimitPerOrganizationInTouch,
	@LimitPerOrganizationInTouchPeriodStartDateUtc)";

			return new ZNonPersistentDataQuery(sql, parameters);
		}

		#endregion

		#region Properties

		protected override bool ShouldUpdateNextScheduleDate
		{
			get { return S5_NextScheduledPrintRunTimeUtc < UtcNow; }
		}

		public override TimeSpan? UtcOffsetOverride
		{
			get { return new TimeSpan(14, 0, 0); }
		}

		#endregion

		#region Related Business Objects

		GlbCompanyCampaignSendSettings SendSettings
		{
			get { return Factory.Load<GlbCompanyCampaignSendSettings>(S5_ParentID); }
		}

		#endregion

		#region Saving

		protected override void OnFactorySaving()
		{
			if (Recurrence.NextScheduledPrintRunTimeLocal.IsValid)
			{
				S5_DailyStartTime = Recurrence.NextScheduledPrintRunTimeLocal.TimeOfDay;
			}

			var nextRunTimeOldValue = S5_NextScheduledPrintRunTimeUtc;

			base.OnFactorySaving();

			if (!ShouldUpdateNextScheduleDate && ShouldPreventNextRunTimeBounceBack
				&& nextRunTimeOldValue.IsValid && S5_NextScheduledPrintRunTimeUtc.IsValid
				&& S5_NextScheduledPrintRunTimeUtc < nextRunTimeOldValue)
			{
				S5_NextScheduledPrintRunTimeUtc = nextRunTimeOldValue;
			}
		}

		public bool ShouldPreventNextRunTimeBounceBack { get; set; }

		internal bool ParentHasQueuedForScheduleItem()
		{
			var sendSettings = SendSettings;
			if (sendSettings == null)
			{
				return false;
			}

			return CampaignHasQueuedForScheduleItem(sendSettings.GSC_G0_Campaign);
		}

		#endregion

		#region Validation

		protected override StmScheduleTaskValidation GetNewValidation()
		{
			return new GlbCompanyCampaignSendScheduleTaskValidation(this, SendSettings);
		}

		#endregion

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			S5_ParentTableCode = GlbCompanyCampaignSendSettingsSchema.Constants.Prefix;
			S5_ParentID = Guid.NewGuid();

			HasChanges = false;
		}
#endif
		#endregion
	}
}
