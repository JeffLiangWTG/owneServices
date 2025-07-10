using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignSummaryStats
	{
		readonly ZGuid campaignId;
		readonly ZInt campaignHorizontalId;
		readonly List<CampaignDeliverySummaryItem> deliverySummaryItems;
		readonly Dictionary<ZInt, List<CampaignTransitionResults>> transitionResults;
		readonly int maxHorizontal;

		#region Load / Constructor

		public static CampaignSummaryStats Load(ZGuid masterCampaignPK, int maxHorizontal)
		{
			var deliveryItems = LoadDeliverySummary(masterCampaignPK);
			var transitionResults = LoadTransitionResults(masterCampaignPK);

			return new CampaignSummaryStats(deliveryItems, transitionResults, maxHorizontal);
		}

		protected CampaignSummaryStats(List<CampaignDeliverySummaryItem> deliverySummaryItems,
			IReadOnlyCollection<CampaignTransitionResults> transitionResults, int maxHorizontal)
			: this(deliverySummaryItems,
				transitionResults.GroupBy(item => item.HorizontalId).ToDictionary(group => group.Key, group => group.ToList()),
				ZGuid.Empty, int.MaxValue - 1, maxHorizontal)
		{
		}

		CampaignSummaryStats(List<CampaignDeliverySummaryItem> deliverySummaryItems,
			Dictionary<ZInt, List<CampaignTransitionResults>> transitionResults,
			ZGuid campaignId, ZInt campaignHorizontalId,
			int maxHorizontal)
		{
			this.campaignId = campaignId;
			this.campaignHorizontalId = campaignHorizontalId;
			this.deliverySummaryItems = campaignId == ZGuid.Empty
				? deliverySummaryItems
				: deliverySummaryItems.Where(s => s.CampaignId == campaignId).ToList();
			this.transitionResults = transitionResults;

			this.maxHorizontal = maxHorizontal;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static List<CampaignDeliverySummaryItem> LoadDeliverySummary(ZGuid masterCampaignPK)
		{
			var items = new List<CampaignDeliverySummaryItem>();

			const string sql = @"
SELECT CampaignPK, TrackingStatus, StatusCount
FROM dbo.GetDripCampaignDeliveryStatistics(@MasterCampaignPk)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@MasterCampaignPk", System.Data.SqlDbType.UniqueIdentifier, masterCampaignPK.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var campaignId = reader.GetGuid(0);
						var trackingStatus = reader.GetString(1);
						var statusCount = reader.GetInt32(2);

						items.Add(new CampaignDeliverySummaryItem()
						{
							CampaignId = campaignId,
							TrackingStatus = trackingStatus,
							StatusCount = statusCount
						});
					}
				}
			}

			return items;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static IReadOnlyCollection<CampaignTransitionResults> LoadTransitionResults(ZGuid masterCampaignPK)
		{
			var items = new List<CampaignTransitionResults>();

			const string sql = @"
SELECT CampaignItemID, RecipientID, HorizontalID, CampaignID, TrackingStatus, IsSuspended, IsBlocked, IsUnsubscribed
FROM dbo.GetDripCampaignProgressFlow(@MasterCampaignPk)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@MasterCampaignPk", System.Data.SqlDbType.UniqueIdentifier, masterCampaignPK.ToGuid());
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var campaignItemId = reader.GetGuid(0);
						var recipiendId = reader.GetGuid(1);
						var horizontalId = reader.GetInt32(2);
						var campaignId = reader.GetGuid(3);
						var trackingStatus = reader.GetString(4);
						var isSuspended = reader.GetBoolean(5);
						var isBlocked = reader.GetBoolean(6);
						var isIsUnsubscribed = reader.GetBoolean(7);

						items.Add(new CampaignTransitionResults()
						{
							CampaignItemId = campaignItemId,
							RecipientId = recipiendId,
							HorizontalId = horizontalId,
							CampaignId = campaignId,
							TrackingStatus = trackingStatus,
							IsSuspended = isSuspended,
							IsBlocked = isBlocked,
							IsUnsubscribed = isIsUnsubscribed
						});
					}
				}
			}
			return items;
		}

		/// <summary>
		/// Load previous transition for given campaign items.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static Dictionary<ZGuid, CampaignTransitionResults> LoadPreviousTransitions(GlbCompanyCampaignItem[] items)
		{
			var result = new Dictionary<ZGuid, CampaignTransitionResults>(items.Length);

			const string sql =
				"select ItemId, PrevItemId, RecipientId, PrevHorizontalId, PrevCampaignId, PrevTrackingStatus, MasterId " +
				"from dbo.GetDripCampaignPreviousItems(@CampaignItemIds)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddTableValuedParameter("@CampaignItemIds", GlbCompanyCampaignItemSchema.PK, items.Select(x => x.PK.ToGuid()));

				using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						int i = 0;
						var laterItemId = reader.GetGuid(i++);
						var campaignItemId = reader.GetGuid(i++);
						var recipientId = reader.GetGuid(i++);
						var horizontalId = reader.GetInt32(i++);
						var campaignId = reader.GetGuid(i++);
						var trackingStatus = reader.GetString(i++);
						var masterId = reader.GetGuid(i++);

						result.Add(laterItemId, new CampaignTransitionResults
						{
							CampaignItemId = campaignItemId,
							RecipientId = recipientId,
							HorizontalId = horizontalId,
							CampaignId = campaignId,
							TrackingStatus = trackingStatus,
							MasterId = masterId
						});
					}
				}
			}

			return result;
		}

		public static List<CampaignTransitionResults> LoadTransitions(GlbCompanyCampaignItem item)
		{
			return LoadTransitions(item.G8_RecipientID, item.CompanyCampaign.G0_G0_Master);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static List<CampaignTransitionResults> LoadTransitions(ZGuid recipientID, ZGuid masterCampaign)
		{
			var items = new List<CampaignTransitionResults>();

			const string sql =
				"select CampaignItemID, HorizontalID, CampaignID, TrackingStatus, MasterID " +
				"from dbo.GetDripCampaignProgressFlowByCampaignItem(@RecipientID, @MasterCampaignPk) ";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@RecipientID", SqlDbType.UniqueIdentifier, recipientID.ToGuid());
				cmd.AddParameter("@MasterCampaignPk", SqlDbType.UniqueIdentifier, masterCampaign.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						int i = 0;
						var campaignItemId = reader.GetGuid(i++);
						var horizontalId = reader.GetInt32(i++);
						var campaignId = reader.GetGuid(i++);
						var trackingStatus = reader.GetString(i++);
						var masterId = reader.GetGuid(i++);

						items.Add(new CampaignTransitionResults
						{
							CampaignItemId = campaignItemId,
							RecipientId = recipientID,
							HorizontalId = horizontalId,
							CampaignId = campaignId,
							TrackingStatus = trackingStatus,
							MasterId = masterId
						});
					}
				}
			}
			return items;
		}

		#endregion

		public IEnumerable<CampaignTransitionResults> GetTransitionResultsForRecipient(Guid recipientId)
		{
			return transitionResults.SelectMany(pair => pair.Value.Where(t => t.RecipientId == recipientId));
		}

		#region Delivery Summary

		public ZInt VerifiedCount
		{
			get { return deliverySummaryItems.Where(i => i.TrackingStatus == TrackingStatusCodes.Codes.VER).Sum(i => i.StatusCount); }
		}

		public ZInt UnverifiedCount
		{
			get { return deliverySummaryItems.Where(i => i.TrackingStatus == TrackingStatusCodes.Codes.UNV).Sum(i => i.StatusCount); }
		}

		public ZInt NonDeliveredCount
		{
			get { return deliverySummaryItems.Where(i => i.TrackingStatus == TrackingStatusCodes.Codes.NDR).Sum(i => i.StatusCount); }
		}

		public ZInt QueuedCount
		{
			get { return deliverySummaryItems.Where(i => i.TrackingStatus == TrackingStatusCodes.Codes.QUE).Sum(i => i.StatusCount); }
		}

		public ZInt ScheduledCount
		{
			get { return deliverySummaryItems.Where(i => i.TrackingStatus == TrackingSummaryConstants.Codes.SCH).Sum(i => i.StatusCount); }
		}

		public ZInt UnScheduledCount
		{
			get { return QueuedCount - ScheduledCount; }
		}

		public ZInt BlockedQueuedCount
		{
			get { return deliverySummaryItems.Where(i => i.TrackingStatus == TrackingStatusCodes.Codes.QUE).Sum(i => i.StatusCount); }
		}

		public ZInt SentCount
		{
			get { return VerifiedCount + UnverifiedCount + NonDeliveredCount; }
		}

		public ZInt TotalCount => SentCount + QueuedCount + OpportunityQueuedCount + OpportunityCreatedCount;

		public ZInt OpportunityQueuedCount => deliverySummaryItems.Where(t => t.TrackingStatus == TrackingStatusCodes.Codes.OPQ).Sum(i => i.StatusCount);

		public ZInt OpportunityCreatedCount => deliverySummaryItems.Where(t => t.TrackingStatus == TrackingStatusCodes.Codes.OPC).Sum(i => i.StatusCount);

		#endregion

		#region Transition Progress

		bool IsOpportunityCreationTouch => OpportunityQueuedCount > 0 || OpportunityCreatedCount > 0;

		public ZInt TouchSummaryVerifiedCount => IsOpportunityCreationTouch ? OpportunityCreatedCount : VerifiedCount;

		public ZInt TouchSummaryUnverifiedCount => IsOpportunityCreationTouch ? OpportunityQueuedCount : UnverifiedCount;

		public ZInt UnsubscribedCount
		{
			get { return deliverySummaryItems.Where(i => i.TrackingStatus == TrackingSummaryConstants.Codes.UNS).Sum(i => i.StatusCount); }
		}

		public ZInt FailedToTransitionCount
		{
			get { return !IsLastHorizontal ? SentCount - TransitionedToNextTouchCount : 0; }
		}

		public ZInt TransitionedToNextTouchCount
		{
			get { return TransitionedToNextTouches.Count(); }
		}

		public ZInt NextTouchQueued
		{
			get { return TransitionedToNextTouches.Count(t => t.TrackingStatus == TrackingStatusCodes.Codes.QUE); }
		}

		public ZInt NextTouchSent
		{
			get { return TransitionedToNextTouches.Count(t => t.TrackingStatus != TrackingStatusCodes.Codes.QUE); }
		}

		IEnumerable<CampaignTransitionResults> TransitionedToNextTouches
		{
			get
			{
				return CurrentHorizontalRecipients
					.Where(r => r.CampaignId == campaignId)
					.Where(r => HasNextHorizontalsRecipients(r.RecipientId))
					.ToList();
			}
		}

		public IEnumerable<CampaignTransitionResults> FailedToTransition
		{
			get
			{
				return CurrentHorizontalRecipients
						.Where(r => r.CampaignId == campaignId)
						.Where(r => r.TrackingStatus != TrackingStatusCodes.Codes.QUE)
						.Where(r => r.TrackingStatus != TrackingStatusCodes.Codes.OPQ)
						.Where(r => r.TrackingStatus != TrackingStatusCodes.Codes.OPC)
						.Where(r => !IsLastHorizontal || r.IsUnsubscribed || r.TrackingStatus == TrackingStatusCodes.Codes.NDR)
						.Where(r => !HasNextHorizontalsRecipients(r.RecipientId))
						.ToList();
			}
		}

		public bool IsLastHorizontal => campaignHorizontalId == maxHorizontal;

		IReadOnlyList<CampaignTransitionResults> CurrentHorizontalRecipients
		{
			get { return transitionResults.ContainsKey(campaignHorizontalId) ? transitionResults[campaignHorizontalId] : new List<CampaignTransitionResults>(); }
		}

		public bool HasHorizontalsRecipients(ZGuid recipientId, ZByte horizontalId)
		{
			return transitionResults.Where(pair => pair.Key == horizontalId).SelectMany(pair => pair.Value).Any(recipient => recipient.RecipientId == recipientId);
		}

		public bool HasTouchRecipients(ZGuid recipientId, ZByte horizontalId, ZGuid touchId)
		{
			return transitionResults.Where(pair => pair.Key == horizontalId)
				.SelectMany(pair => pair.Value)
				.Any(recipient =>
					recipient.CampaignId == touchId
					&& recipient.RecipientId == recipientId);
		}

		bool HasNextHorizontalsRecipients(ZGuid recipientId)
		{
			return transitionResults.Where(pair => pair.Key > campaignHorizontalId).SelectMany(pair => pair.Value).Any(recipient => recipient.RecipientId == recipientId);
		}

		#endregion

		public CampaignSummaryStats GetSubSummary(GlbCompanyCampaign subCampaign)
		{
			return new CampaignSummaryStats(deliverySummaryItems, transitionResults, subCampaign.PK, subCampaign.G0_HorizontalId, maxHorizontal);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public delegate void CampaignItemChangedHandler(IEnumerable<GlbCompanyCampaignItem> updated);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event CampaignItemChangedHandler OnStatsChanged = delegate { };

		public void AddCampaignItem(GlbCompanyCampaignItem item)
		{
			if (!transitionResults.ContainsKey(item.CompanyCampaign.G0_HorizontalId))
			{
				transitionResults[item.CompanyCampaign.G0_HorizontalId] = new List<CampaignTransitionResults>();
			}
			transitionResults[item.CompanyCampaign.G0_HorizontalId].Add(new CampaignTransitionResults()
			{
				CampaignId = item.G8_G0,
				HorizontalId = item.CompanyCampaign.G0_HorizontalId,
				RecipientId = item.G8_RecipientID,
				TrackingStatus = item.G8_TrackingStatus,
				CampaignItemId = item.PK,
				MasterId = item.CompanyCampaign.MasterCampaign.PK
			});

			var found = deliverySummaryItems.Find(i => i.CampaignId == item.G8_G0 && i.TrackingStatus == item.G8_TrackingStatus);
			if (found != null)
			{
				found.StatusCount++;
			}
			else
			{
				deliverySummaryItems.Add(new CampaignDeliverySummaryItem()
				{
					TrackingStatus = item.G8_TrackingStatus,
					CampaignId = item.G8_G0,
					StatusCount = 1
				});
			}

			OnStatsChanged(new[] { item });
		}

		public void UpdateTransitions(IEnumerable<GlbCompanyCampaignItem> updated)
		{
			foreach (var item in updated)
			{
				var horizontalId = item.CompanyCampaign?.G0_HorizontalId;
				if (!horizontalId.HasValue || !transitionResults.ContainsKey(horizontalId.Value))
				{
					continue;
				}

				var found = transitionResults[horizontalId.Value].Find(t => t.CampaignItemId == item.PK);
				if (found != null)
				{
					found.IsBlocked = item.G8_IsBlocked;
					found.IsSuspended = item.G8_IsSuspended;
					found.CampaignId = item.G8_G0;
					found.IsUnsubscribed = item.IsUnsubscribed;
				}
			}

			OnStatsChanged(updated);
		}
	}

	public class CampaignDeliverySummaryItem
	{
		public ZGuid CampaignId { get; set; }
		public ZString TrackingStatus { get; set; }
		public ZInt StatusCount { get; set; }
	}

	public class CampaignTransitionResults
	{
		public ZGuid CampaignItemId { get; set; }
		public ZGuid RecipientId { get; set; }
		public ZInt HorizontalId { get; set; }
		public ZGuid CampaignId { get; set; }
		public ZString TrackingStatus { get; set; }
		public ZGuid MasterId { get; set; }
		public bool IsSuspended { get; set; }
		public bool IsBlocked { get; set; }
		public bool IsUnsubscribed { get; set; }
	}
}
