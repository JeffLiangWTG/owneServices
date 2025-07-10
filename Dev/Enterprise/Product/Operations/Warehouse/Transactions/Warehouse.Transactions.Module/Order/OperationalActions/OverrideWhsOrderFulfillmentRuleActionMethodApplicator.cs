using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OverrideWhsOrderFulfillmentRuleActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public OverrideWhsOrderFulfillmentRuleActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("791D2960-DE47-490A-B172-E8F611ACFD02", "Override Order Fulfillment Rule"), factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);

			AddFetchHints(targets);

			var pickPKsToUpdate = new HashSet<ZGuid>();
			foreach (WhsOrder order in targets)
			{
				if (TryToOverrideFulfillmentRule(order, log))
				{
					pickPKsToUpdate.Add(order.WD_WP);
				}
				log.BumpSectionProgress();
			}

			if (pickPKsToUpdate.Count > 0)
			{
				var picks = targets[0].Factory.Load<WhsPick>(new ZQuery(WhsPickSchema.PK, pickPKsToUpdate));
				AddFetchHintsForPicks(picks);
				foreach (var pick in picks)
				{
					pick.UpdatePickStatusIfRequired();
				}
			}
		}

		void AddFetchHints(BusinessObject[] targets)
		{
			foreach (WhsOrder order in targets)
			{
				var processTaskquery = new ZQuery();
				processTaskquery.AddToFilter(ProcessTasksSchema.P9_ParentID, order.PK);
				processTaskquery.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, Events.EditedARecordCode);
				processTaskquery.AddToFilter(ProcessTasksSchema.P9_Type, new[] { Constants.Workflow.MilestoneType, Constants.Workflow.WorkflowTriggerType });
				processTaskquery.AddToFilter(ProcessTasksSchema.P9_LineTriggerType, ZString.Empty);
				order.Factory.AddFetchHint(ProcessTasksSchema.Instance, processTaskquery);

				order.Factory.AddFetchHint(typeof(JobDocAddress), FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, order.PK));
			}
		}

		void AddFetchHintsForPicks(WhsPick[] picks)
		{
			foreach (var pick in picks)
			{
				var pickQuery = new ZQuery();
				pickQuery.AddToFilter(WhsDocketSchema.WD_DocketType, new[] { DocketType.Codes.Order, DocketType.Codes.WorkOrder, DocketType.Codes.DynamicWorkOrder });
				pickQuery.AddToFilter(WhsDocketSchema.WD_WP, pick.PK);
				pick.Factory.AddFetchHint(WhsDocketSchema.Instance, pickQuery);
			}
		}

		bool TryToOverrideFulfillmentRule(WhsOrder order, IOperationalActionSectionLog log)
		{
			var targetLink = GetDocketIdLink(order);
			var result = CanTryToOverrideFulfillmentRule(order, log, targetLink);
			if (result)
			{
				OverrideFulfillmentRule(order, log, targetLink);
			}
			return result;
		}

		bool CanTryToOverrideFulfillmentRule(WhsOrder order, IOperationalActionSectionLog log, LogControllerLink targetLink)
		{
			var result = true;
			if (!order.IsAttachedToPick)
			{
				result = false;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					OutputTextFormat,
					order.Description,
					targetLink,
					Res.GetString("1EFEDEC8-A8DD-4B47-9617-B2C29F64F03E", "is not picked so cannot override."));
			}
			else if (order.IsFinalised)
			{
				result = false;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					OutputTextFormat,
					order.Description,
					targetLink,
					Res.GetString("CCE3C6D5-6D45-45CE-99E2-585A556BCCA2", "is already finalized so cannot override."));
			}
			else if (order.WD_WhsOrderFulfillmentRule == WhsOrderFulfillmentRuleList.Codes.None)
			{
				result = false;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					OutputTextFormat,
					order.Description,
					targetLink,
					Res.GetString("A2BCA5F0-43CF-4FC0-97AE-A47FCDA3B76E", "fulfillment rule is already NONE."));
			}
			return result;
		}

		void OverrideFulfillmentRule(WhsOrder order, IOperationalActionSectionLog log, LogControllerLink targetLink)
		{
			var notify = new NotificationBuffer();
			order.OverrideFulfillmentRuleAndCreateLog(OverrideReason, notify);
			if (notify.HasErrors)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, OutputTextFormat, order.HumanReadableName, targetLink, notify.AsString);
			}
			else if (notify.HasWarnings)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, targetLink, notify.AsString);
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, OutputTextFormat, order.HumanReadableName, targetLink, Res.GetString("6950757E-AAD0-4C58-A4DF-64A06B95018B", "fulfillment rule was successfully overridden."));
			}
		}

		const string OutputTextFormat = "{0} {1} - {2}";

		public ZString OverrideReason
		{
			get => overrideReason;
			set => SetNonPersistentPropertyValue(OverrideReasonInfo, ref overrideReason, value);
		}
		ZString overrideReason;

		public abstract class Schema
		{
			public const string OverrideReason = nameof(OverrideReason);
		}

		public ZPropertyInfo OverrideReasonInfo => GetZPropertyInfo(
			Schema.OverrideReason,
			Res.GetString("2FDAAFCF-560A-42B1-BEDC-B7B5BA567D90", "Override Reason"));
	}
}
