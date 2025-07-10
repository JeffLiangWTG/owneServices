using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[Serializable]
	public sealed class OrderCopyVoyageEventsLogSubscriber : LogSubscriber
	{
		public override string Name
		{
			get { return "OrderVoyageEventsCopier"; }
		}

		public override string FriendlyName
		{
			get { return (NoResString)"Processor of Voyage events related to Orders"; } // Log subscriber names should be in English only
		}

		public override string[] TableNames
		{
			get { return new string[] { JobVoyageSchema.Constants.TableName }; }
		}

		public override string[] EventTypes
		{
			get { return new string[] { Events.EditedARecordCode }; }
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (queuedLogs != null && queuedLogs.Length > 0)
			{
				Order[] orders = FindOrdersLinkedToVoyage(queuedLogs);
				UpdateMilestoneDates(orders);
			}
		}

		static Order[] FindOrdersLinkedToVoyage(IQueuedLog[] queuedLogs)
		{
			ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originQuery.AddToFilter(JobVoyOriginSchema.JA_JV, queuedLogs.Select(log => log.SJ_ParentID).ToArray());

			ZDBOnlySubQuery sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
			sailingSubQuery.AddSubQuery(originQuery, JoinCondition.And);

			ZDBOnlySubQuery transportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportSubQuery.AddSubQuery(sailingSubQuery, JoinCondition.And);

			ZDBOnlySubQuery conShipLinkSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			conShipLinkSubQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, transportSubQuery, JoinCondition.And);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(Order));
			query.AddSubQuery(JobOrderHeaderSchema.JD_JS, conShipLinkSubQuery, JoinCondition.Or);
			query.AddSubQuery(JobOrderHeaderSchema.JD_JS, transportSubQuery, JoinCondition.Or);
			query.AddSubQuery(JobOrderHeaderSchema.JD_JE, transportSubQuery, JoinCondition.Or);

			return queuedLogs[0].Factory.Load<Order>(query);
		}

		void UpdateMilestoneDates(Order[] orders)
		{
			for (var i = 0; i < orders.Length; i++)
			{
				var order = orders[i];
				for (var j = 0; j < order.WorkflowItems.Count; j++)
				{
					OrderProcessTasks task = order.WorkflowItems[j];
					if (task.IsMilestone && (task.P9_SE_NKMilestoneEvent == Events.DepartureCode || task.P9_SE_NKMilestoneEvent == Events.ArrivalCode))
					{
						task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: true);
						task.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: true);
					}
				}
			}
		}
	}
}
