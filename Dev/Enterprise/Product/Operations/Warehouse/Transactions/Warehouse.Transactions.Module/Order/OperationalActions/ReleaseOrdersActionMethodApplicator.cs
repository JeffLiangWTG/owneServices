using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReleaseOrdersActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public ReleaseOrdersActionMethodApplicator()
			: base(Res.GetString("439623BE-05DC-476C-9903-6A44E1A45066", "Release Orders"))
		{
		}

		#region Release Orders

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] orders)
		{
			log.SetSectionProgressMax(orders.Length);

			AddFetchHints(orders);
			foreach (WhsOrder order in orders)
			{
				if (!order.IsFinalised)
				{
					LogUnReleasableOrderDueToNonFinalizedStatus(order, log);
				}
				else if (order.WorkflowItems.Find(milestone => milestone.P9_SE_NKMilestoneEvent == AutoEvents.PickedUpCode && !milestone.P9_ActualDate.IsEmpty).Any())
				{
					LogOrderAlreadyReleased(order, log);
				}
				else
				{
					ReleaserOrder(order);
				}

				log.BumpSectionProgress();
			}
		}

		#region AddFetchHints

		void AddFetchHints(BusinessObject[] orders)
		{
			AddFetchHintForOrders(orders);
			AddFetchHintForProcessTaskNotification(orders);
		}

		void AddFetchHintForOrders(BusinessObject[] orders)
		{
			var picks = new HashSet<ZGuid>();

			foreach (WhsOrder order in orders)
			{
				if (picks.Add(order.WD_WP))
				{
					order.Factory.AddFetchHint(WhsPickSchema.PK, order.WD_WP);
					order.Factory.AddFetchHint(StmALogSchema.SL_Parent, order.WD_WP);
					order.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.WD_WP);
					order.Factory.AddFetchHint(WhsDocketSchema.WD_WP, order.WD_WP);
				}

				order.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
				order.Factory.AddFetchHint(StmALogSchema.SL_Parent, order.PK);
			}
		}

		void AddFetchHintForProcessTaskNotification(BusinessObject[] orders)
		{
			foreach (WhsOrder order in orders)
			{
				var workFlows = order.WorkflowItems;
				foreach (var workFlow in workFlows)
				{
					order.Factory.AddFetchHint(ProcessTaskNotificationSchema.PQ_P9, workFlow.PK);
				}
			}
		}

		#endregion

		void ReleaserOrder(WhsOrder order)
		{
			order.Logs.AddNew(Events.PickedUp, ZDateTimeOffset.Now);
		}

		void LogOrderAlreadyReleased(WhsOrder order, IOperationalActionSectionLog log)
		{
			var docketLink = GetDocketIdLink(order);

			var message = Res.GetString("BB710B99-7317-46F8-A06B-97816DABB22C", "{0} {1} has already been released.", order.Description, "{0}");

			log.NotifyFormat(OperationalActionLogErrorLevel.Error, message, docketLink);
		}

		void LogUnReleasableOrderDueToNonFinalizedStatus(WhsOrder order, IOperationalActionSectionLog log)
		{
			var docketLink = GetDocketIdLink(order);

			var message = Res.GetString("6BDBB288-C094-42DB-B683-6D7C7918E1CE", "{0} {1} has a status of {2} and cannot be released until it is finalized.", order.Description, "{0}", order.WD_DocketStatusDescription);

			log.NotifyFormat(OperationalActionLogErrorLevel.Error, message, docketLink);
		}

		#endregion
	}
}
