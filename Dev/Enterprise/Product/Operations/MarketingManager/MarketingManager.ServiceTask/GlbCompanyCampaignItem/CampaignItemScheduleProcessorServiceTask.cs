using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.MarketingManager.ServiceTask.CampaignItemScheduleProcessorServiceTask.Code,
	"Scheduled Campaign Item Email Processor",
	"SAL",
	typeof(Enterprise.MarketingManager.ServiceTask.CampaignItemScheduleProcessorServiceTask),
	MinimumPeriod = "5minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.MarketingManager.ServiceTask
{
	[NeedsDataRefresh]
	public class CampaignItemScheduleProcessorServiceTask : ServiceProviderImpl
	{
		public const string Code = "SCH";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not needed in logging")]
		public override void RunTask(CancellationToken token)
		{
			Log(LogType.Information, "Scheduled Campaign Item Email Processor task started");
			new ScheduleTaskRunner().Process(GlbCompanyCampaignSendSettingsSchema.Constants.Prefix, false, ServiceLogger.GetTaskNotificationSubscriber(), token);

			try
			{
				hasTransitionScheduleWarning = false;
				ProcessQueue(token);
				if (hasTransitionScheduleWarning)
				{
					Log(LogType.Warning, "Scheduled Campaign Item Email Processor task completed with warnings");
				}
				else
				{
					Log(LogType.Information, "Scheduled Campaign Item Email Processor task completed");
				}
			}
			catch (UriFormatException ex)
			{
				NotifyUsersOfUriFormatException(ex);
				Log(LogType.Error, "Error occurred while processing batch: " + ex.Message);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				Log(LogType.Error, "Error occurred while processing batch: " + ex.Message);
				throw;
			}
		}

		protected virtual void ProcessQueue(CancellationToken token)
		{
			new DbOnlyBusinessObjectQueue<GlbCompanyCampaignItem>(GetSentCampaignItemQueryToTransition()).ProcessBatch(ProcessSentItemsQueue, MaxBatchSize, token);
			new DbOnlyBusinessObjectQueue<GlbCompanyCampaignItem>(GetCampaignItemQueryToReTransfer()).ProcessBatch(ProcessRetransferQueueAction, MaxBatchSize, token);
			new DbOnlyBusinessObjectQueue<GlbCompanyCampaignItem>(GetCampaignItemQueryToSend()).ProcessBatch(ProcessSendingQueueAction, MaxBatchSize, token);
			new DbOnlyBusinessObjectQueue<GlbCompanyCampaignItem>(GetCampaignItemQueryFromOpportunityTemplate()).ProcessBatch(ProcessOpportunityTemplate, MaxBatchSize, token);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not needed in logging")]
		void ProcessSentItemsQueue(GlbCompanyCampaignItem[] campaignItems, CancelEventArgs e)
		{
			if (e.Cancel)
			{
				return;
			}

			Log(LogType.Debug, "{0}: processing sent items queue for transition", campaignItems.Length);

			var reTransferPlan = new Dictionary<ZGuid, List<ZGuid>>();

			foreach (var item in campaignItems)
			{
				if (!reTransferPlan.ContainsKey(item.G8_G0))
				{
					reTransferPlan.Add(item.G8_G0, new List<ZGuid>());
				}
				reTransferPlan[item.G8_G0].Add(item.PK);
			}

			Lazy<BusinessObjectFactory> transitionsFactory = GetTransitionsFactory();
			int success = 0;

			foreach (var entry in reTransferPlan)
			{
				var campaign = transitionsFactory.Value.Load<GlbCompanyCampaign>(entry.Key);
				using (DisposableEnvironment.ForCompany(campaign.Company.GC_Code))
				{
					var results = GetTransitionResults(campaign, entry.Value);
					success += results.Count(r => r.Item2 == TransitionStatus.Success);
					entry.Value.Where(p => !results.Select(r => r.Item1).Contains(p)).ToList().ForEach(p => SetLastFailedTransition(transitionsFactory, p));

					transitionsFactory.Value.Save();
				}
			}

			if (success > 0)
			{
				Log(LogType.Debug, "{0} campaign items were successfuly transferred", success);
			}
		}

		protected virtual Lazy<BusinessObjectFactory> GetTransitionsFactory()
		{
			return new Lazy<BusinessObjectFactory>(() => new BusinessObjectFactory());
		}

		void SetLastFailedTransition(Lazy<BusinessObjectFactory> transitionsFactory, ZGuid pK)
		{
			var item = transitionsFactory.Value.LoadTop1<GlbCompanyCampaignItem>(new ZQuery(GlbCompanyCampaignItemSchema.PK, pK));

			if (item != null)
			{
				item.G8_LastFailedTransitionUtc = ZDateTime.UtcNow;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not needed in logging")]
		void ProcessRetransferQueueAction(GlbCompanyCampaignItem[] campaignItems, CancelEventArgs e)
		{
			if (e.Cancel)
			{
				return;
			}

			Log(LogType.Debug, "{0}: processing re-transfer queue", campaignItems.Length);

			var transitionStatuses = VerifyTransitions(campaignItems);
			if (transitionStatuses.Any(s => s.Value == TransitionStatus.Fail))
			{
				campaignItems.FirstOrDefault()?.Factory.Save();
			}

			var transitionsFactory = GetTransitionsFactory();
			var transitionStatusesKeys = transitionStatuses.Where(i => i.Value == TransitionStatus.ReTransfer || i.Value == TransitionStatus.Fail).Select(t => t.Key);
			campaignItems.Where(c => !transitionStatusesKeys.Contains(c.PK)).ToList().ForEach(p => SetLastFailedTransition(transitionsFactory, p.PK));
			transitionsFactory.Value.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not needed in logging")]
		void ProcessSendingQueueAction(GlbCompanyCampaignItem[] campaignItems, CancelEventArgs e)
		{
			if (e.Cancel)
			{
				return;
			}

			Log(LogType.Debug, "{0}: processing send queue", campaignItems.Length);

			var transitionStatuses = VerifyTransitions(campaignItems);

			var sentEmails = 0;
			var failure = 0;
			var hasSentAnyEmails = false;

			foreach (var item in campaignItems.Where(i => !transitionStatuses.ContainsKey(i.PK)))
			{
				var campaignSender = new GlbCompanyCampaignSender(item.CompanyCampaign);
				var isSent = campaignSender.SendScheduledEmailToContact(item);
				if (isSent)
				{
					hasSentAnyEmails = true;
					item.ResetTrackingInfo(false);
					item.G8_LastFailedTransitionUtc = ZDateTime.Empty;
					sentEmails++;
				}
				else
				{
					failure++;

					var stringBuilder = new ZStringBuilder(string.Format("Campaign: <{0}> failed to be sent to contact: {1}", item.CompanyCampaign.CampaignID, item.ContactName));

					if (item.CompanyCampaign.HasErrors)
					{
						stringBuilder.Append(campaignSender.GetErrorMessagesToDisplayOnCampaignSending());
					}

					Log(LogType.Debug, stringBuilder.ToStringWithNewLineBetweenAppends());
				}
			}

			if (hasSentAnyEmails || transitionStatuses.Any(s => s.Value == TransitionStatus.Fail))
			{
				var grouped = campaignItems.Where(ci => ci.IsPendingTransition).GroupBy(ci => ci.CompanyCampaign).ToArray();

				campaignItems.FirstOrDefault()?.Factory.Save();

				foreach (var grp in grouped)
				{
					using (DisposableEnvironment.ForCompany(grp.Key.Company.GC_Code))
					{
						GetTransitionResults(grp.Key, grp.Select(g => g.PK));
					}
				}
			}
			if (failure > 0)
			{
				hasTransitionScheduleWarning = true;
			}

			Log(LogType.Debug, "{0} emails successfully sent", sentEmails);
			Log(LogType.Debug, "{0} emails failed to be sent", failure);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not needed in logging")]
		void ProcessOpportunityTemplate(GlbCompanyCampaignItem[] campaignItems, CancelEventArgs e)
		{
			if (e.Cancel)
			{
				return;
			}

			Log(LogType.Debug, "{0} processing opportunities creation templates queued", campaignItems.Length);

			var itemsGroupedByCompany = campaignItems.GroupBy(item => item.CompanyCampaign.Company.GC_Code);
			foreach (var grp in itemsGroupedByCompany)
			{
				ProcessOpportunityTemplateForCompany(grp.Key, grp);
			}

			Log(LogType.Debug, "{0} opportunities creation templates were successful created", campaignItems.Length);
		}

		void ProcessOpportunityTemplateForCompany(string companyCode, IEnumerable<GlbCompanyCampaignItem> campaignItems)
		{
			using (DisposableEnvironment.ForCompany(companyCode))
			{
				var transitionsFactory = GetTransitionsFactory();
				var itemsGroupedByCampaign = campaignItems.GroupBy(item => item.G8_G0);
				foreach (var grp in itemsGroupedByCampaign)
				{
					var campaign = transitionsFactory.Value.Load<GlbCompanyCampaign>(grp.Key);
					ProcessOpportunityTemplateForCampaign(campaign, grp, transitionsFactory);
				}
				transitionsFactory.Value.Save();
			}
		}

		void ProcessOpportunityTemplateForCampaign(GlbCompanyCampaign campaign, IEnumerable<GlbCompanyCampaignItem> campaignItems, Lazy<BusinessObjectFactory> transitionsFactory)
		{
			var poolAssignmentItems = new List<GlbCompanyCampaignItem>();
			var opportunityCreationTemplate = campaign.OpportunityCreationTemplate;

			foreach (var item in campaignItems)
			{
				var orgOpportunity = transitionsFactory.Value.New<OrgOpportunity>();
				orgOpportunity.P8_OH = item.OrgPK;
				orgOpportunity.P8_OC = item.G8_RecipientID;
				orgOpportunity.P8_G0 = item.G8_G0;
				orgOpportunity.P8_GC = campaign.G0_GC;
				orgOpportunity.P8_Status = opportunityCreationTemplate.OpportunityStatus;
				orgOpportunity.P8_Stage = opportunityCreationTemplate.OpportunityStage;
				orgOpportunity.P8_OpportunityDescription = opportunityCreationTemplate.OpportunityDescription;
				orgOpportunity.P8_OpportunityNotes = opportunityCreationTemplate.OpportunityNotes;
				orgOpportunity.P8_PackageType = opportunityCreationTemplate.PackageType;
				orgOpportunity.P8_OpportunityType = opportunityCreationTemplate.OpportunityType;
				orgOpportunity.P8_Source = opportunityCreationTemplate.Source;
				orgOpportunity.P8_SourceDetails = opportunityCreationTemplate.ActiveSourceDetails;
				orgOpportunity.RelatedParentActivityPivotCollection.AddNewPivot(campaign);

				var loadedItem = transitionsFactory.Value.Load<GlbCompanyCampaignItem>(item.PK);
				loadedItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPC;

				if (opportunityCreationTemplate.OpportunityAssignment == OpportunityAssignmentList.Codes.IndividualSalesPerson)
				{
					orgOpportunity.P8_GS_NKPrimarySalesPerson = opportunityCreationTemplate.SalesPerson;
				}
				else if (opportunityCreationTemplate.OpportunityAssignment == OpportunityAssignmentList.Codes.StaffAssignment)
				{
					orgOpportunity.P8_GS_NKPrimarySalesPerson = item.GetAssignedStaff(opportunityCreationTemplate.StaffAssignment)?.GS_Code ?? ZString.Empty;
				}
				else if (opportunityCreationTemplate.OpportunityAssignment == OpportunityAssignmentList.Codes.MatchParentTouchSender)
				{
					var previousTransitionItem = item.GetPreviousTransitionCampaignItem();
					orgOpportunity.P8_GS_NKPrimarySalesPerson = previousTransitionItem?.GetEmailSenderStaff()?.GS_Code ?? ZString.Empty;
				}
				else if (opportunityCreationTemplate.OpportunityAssignment == OpportunityAssignmentList.Codes.StaffPoolAssignments)
				{
					poolAssignmentItems.Add(loadedItem);
				}
			}

			if (poolAssignmentItems.Any())
			{
				var opportunityCreationAssignment = new OpportunityCreationAssignment(campaign);
				opportunityCreationAssignment.SetStaffPoolAssignments(poolAssignmentItems);
			}

			var itemPKs = campaignItems.Select(i => i.PK);
			_ = GetTransitionResults(campaign, itemPKs);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not needed in logging")]
		Dictionary<ZGuid, TransitionStatus> VerifyTransitions(GlbCompanyCampaignItem[] campaignItems)
		{
			var transitions = CampaignSummaryStats.LoadPreviousTransitions(campaignItems);
			var transitionStatuses = GetTransitionStatuses(campaignItems, transitions);

			int reTransferCount = transitionStatuses.Count(i => i.Value == TransitionStatus.ReTransfer);

			if (reTransferCount > 0)
			{
				Log(LogType.Debug, "{0} campaign items re-transferred to another touch", reTransferCount);
			}

			int deleted = 0;
			foreach (var toDelete in transitionStatuses.Where(s => s.Value == TransitionStatus.Fail))
			{
				campaignItems.FirstOrDefault(i => i.PK == toDelete.Key && i.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE)?.Delete();
				deleted++;
			}

			if (deleted > 0)
			{
				Log(LogType.Debug, "{0} campaign items deleted", deleted);
			}

			return transitionStatuses;
		}

		Dictionary<ZGuid, TransitionStatus> GetTransitionStatuses(GlbCompanyCampaignItem[] campaignItems, Dictionary<ZGuid, CampaignTransitionResults> prevTransitions)
		{
			var reTransferPlan = GetRetransferPlan(campaignItems, prevTransitions);

			var result = new Dictionary<ZGuid, TransitionStatus>();
			var transitionsFactory = GetTransitionsFactory();

			foreach (var entry in reTransferPlan)
			{
				var campaign = transitionsFactory.Value.Load<GlbCompanyCampaign>(entry.Key);
				using (DisposableEnvironment.ForCompany(campaign.Company.GC_Code))
				{
					var transitionResults = GetTransitionResults(campaign, entry.Value.Select(e => e.PrevItemPK));

					foreach (var retransfer in transitionResults.Where(r => r.Item2 == TransitionStatus.ReTransfer).Select(r => r.Item1))
					{
						result.Add(retransfer, TransitionStatus.ReTransfer);
					}

					foreach (var item in GetFailedPKs(entry.Value, transitionResults))
					{
						result.Add(item, TransitionStatus.Fail);
					}
				}
			}
			return result;
		}

		static IEnumerable<ZGuid> GetFailedPKs(List<TransferPlanItem> planItems, List<Tuple<ZGuid, TransitionStatus>> transitionResults)
		{
			var failedPrevPKs = planItems.Select(e => e.PrevItemPK).Except(transitionResults.Select(r => r.Item1));
			return planItems.Where(e => failedPrevPKs.Contains(e.PrevItemPK)).Select(i => i.CurrentItem.PK);
		}

		static Dictionary<ZGuid, List<TransferPlanItem>> GetRetransferPlan(GlbCompanyCampaignItem[] campaignItems, Dictionary<ZGuid, CampaignTransitionResults> prevTransitions)
		{
			var reTransferPlan = new Dictionary<ZGuid, List<TransferPlanItem>>();

			foreach (var item in campaignItems)
			{
				CampaignTransitionResults prevTransition;
				prevTransitions.TryGetValue(item.PK, out prevTransition);

				if (prevTransition == null)
				{
					continue;
				}

				var prevCampaignPK = prevTransition.CampaignId;
				var prevItemPK = prevTransition.CampaignItemId;

				if (!reTransferPlan.ContainsKey(prevCampaignPK))
				{
					reTransferPlan.Add(prevCampaignPK, new List<TransferPlanItem>());
				}
				reTransferPlan[prevCampaignPK].Add(new TransferPlanItem(item, prevItemPK));
			}
			return reTransferPlan;
		}

		protected List<Tuple<ZGuid, TransitionStatus>> GetTransitionResults(GlbCompanyCampaign campaign, IEnumerable<ZGuid> campaignItemPks)
		{
			var errorList = new List<string>();
			var transitionResults = campaign.TransitionAndSchedule(campaignItemPks, errorList);
			if (errorList.Any())
			{
				hasTransitionScheduleWarning = true;
				foreach (var message in errorList)
				{
					Log(LogType.Debug, message);
				}
			}
			return transitionResults;
		}

		class TransferPlanItem
		{
			public TransferPlanItem(GlbCompanyCampaignItem currentItem, ZGuid prevItemPK)
			{
				CurrentItem = currentItem;
				PrevItemPK = prevItemPK;
			}

			public GlbCompanyCampaignItem CurrentItem { get; private set; }
			public ZGuid PrevItemPK { get; private set; }
		}

		static ZQuery GetCampaignItemQueryToSend()
		{
			var query = new ZQuery(GlbCompanyCampaignItemSchema.G8_TrackingStatus, TrackingStatusCodes.Codes.QUE);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_IsSuspended, SQLComparisonOperator.Equal, ZBool.False);
			return query;
		}

		static ZQuery GetCampaignItemQueryToReTransfer()
		{
			var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, TrackingStatusCodes.Codes.QUE);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_IsCheckTransitionRequired, ZBool.True);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_IsSuspended, SQLComparisonOperator.Equal, ZBool.False);

			var campaignSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaign), GlbCompanyCampaignItemSchema.G8_G0);

			var sqlParams = new ZSqlParameterCollection();
			var sql = @"G0_G0_Master IN
						(
							SELECT G0_G0_Master
							FROM dbo.GlbCompanyCampaign
							WHERE G0_G0_Master IS NOT NULL
							GROUP BY G0_G0_Master
							HAVING G8_LastFailedTransitionUtc IS NULL OR MAX(G0_SystemLastEditTimeUtc) >= G8_LastFailedTransitionUtc
						)";

			campaignSubQuery.AddFilterAndZSQLParameterCollection(sql, sqlParams);
			query.AddSubQuery(campaignSubQuery, JoinCondition.And);
			return query;
		}

		static ZQuery GetSentCampaignItemQueryToTransition()
		{
			var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, SQLComparisonOperator.NotEqual, TrackingStatusCodes.Codes.QUE);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, SQLComparisonOperator.NotEqual, TrackingStatusCodes.Codes.NDR);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, SQLComparisonOperator.NotEqual, TrackingStatusCodes.Codes.OPQ);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, SQLComparisonOperator.NotEqual, TrackingStatusCodes.Codes.OPC);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_IsCheckTransitionRequired, ZBool.True);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_IsSuspended, SQLComparisonOperator.Equal, ZBool.False);

			var campaignSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaign), GlbCompanyCampaignItemSchema.G8_G0);

			var sqlParams = new ZSqlParameterCollection();
			var sql = @"G0_G0_Master IN
						(
							SELECT G0_G0_Master
							FROM dbo.GlbCompanyCampaign
							WHERE G0_G0_Master IS NOT NULL
							GROUP BY G0_G0_Master
							HAVING G8_LastFailedTransitionUtc IS NULL OR MAX(G0_SystemLastEditTimeUtc) >= G8_LastFailedTransitionUtc
						)";

			campaignSubQuery.AddFilterAndZSQLParameterCollection(sql, sqlParams);
			query.AddSubQuery(campaignSubQuery, JoinCondition.And);

			return query;
		}

		static ZQuery GetCampaignItemQueryFromOpportunityTemplate()
		{
			var query = new ZQuery(GlbCompanyCampaignItemSchema.G8_TrackingStatus, TrackingStatusCodes.Codes.OPQ);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_IsSuspended, SQLComparisonOperator.Equal, ZBool.False);
			return query;
		}

		protected void Log(LogType logType, string format, params object[] args)
		{
			ServiceLogger.Log(logType, string.Format(CultureInfo.CurrentCulture, format, args));
		}

		protected void Log(LogType logType, string message)
		{
			ServiceLogger.Log(logType, message);
		}

		void NotifyUsersOfUriFormatException(UriFormatException ex)
		{
			var groupPK = NotificationDataRegistry.Instance.SystemServiceTasksNotificationGroup.Value;

			var emailUtility = new EmailGroupUtility();
			var groupEmails = emailUtility.GetGroupEmailCollection(groupPK, false);

			var emailDef = new EmailDef();
			emailDef.AddRecipientForUserCommunication(groupEmails);

			emailDef.FromDisplayName = Env.Registry.MailboxDisplayName;
			emailDef.Subject = Res.GetString("1a2d6bf9-8ef1-48c4-8200-09422339b8f3", "Web Campaign URL Format Exception Notification");
			emailDef.Body = ex.Message;

			Env.OutgoingMailManager.CreateAndSave(emailDef);
		}

		internal const int MaxBatchSize = 500;
		bool hasTransitionScheduleWarning;
	}
}
