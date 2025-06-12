using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if NET48
using eServices.eHubDataModel.eHubTransactions;
#else
using eServices.eHubDataModel.eHubTransactionsCore;
#endif
using Common.Logging;
using Common.Logging.Simple;

namespace eServices.eHubRoutingRuleEngine
{
	public class SubRuleCollection : IEnumerable<Criterion>
	{
		internal SubRuleCollection(eHubRoutingRule parentRule)
		{
			this.parentRule = parentRule;
			parentRule.eHubRoutingRules_Group.Count();
			this.subRules = new List<Criterion>(parentRule.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => Criterion.LoadRuleType(r)));
		}

		public void Add(Criterion newSubRule)
		{
			AddRange(new[] { newSubRule });
		}

		public void AddRange(IEnumerable<Criterion> newSubRules)
		{
			Load(this.subRules.Concat(newSubRules));
		}

		public void Insert(int index, Criterion newSubRule)
		{
			InsertRange(index, new[] { newSubRule });
		}

		public void InsertRange(int index, IEnumerable<Criterion> newSubRules)
		{
			Load(this.subRules.Take(index).Concat(newSubRules).Concat(this.subRules.Skip(index)));
		}

		public void RemoveAt(int index)
		{
			RemoveRange(index, 1);
		}

		public void RemoveRange(int index, int count)
		{
			Load(this.subRules.Take(index).Concat(this.subRules.Skip(index + count)));
		}

		public void Load(IEnumerable<Criterion> newSubRules, ILog logger = null)
		{
			if (logger == null)
				logger = new NoOpLogger();

			if (newSubRules == null) throw new ArgumentNullException("newSubRules");
			if (newSubRules.OfType<Group>().Any()) throw new InvalidOperationException("Cannot load sub rule collection that includes group criteria.");

			if (newSubRules.GroupBy(n =>
				new
				{
					n.eHubRoutingRule.RR_Condition_Expression,
					n.eHubRoutingRule.RR_Success_SP_Provider,
					n.eHubRoutingRule.RR_Success_CC_Recipient,
					n.eHubRoutingRule.RR_Success_RR_SubRule,
					n.eHubRoutingRule.RR_Failed_ErrorCode,
					n.eHubRoutingRule.RR_Failed_ErrorDescription,
					n.eHubRoutingRule.RR_Failed_RR_SubRule,
					n.eHubRoutingRule.RR_Result_Value
				}).Any(g => g.Count() > 1))
				throw  new RoutingRuleException("Duplicate rules detected.");

			var mergedList = (from n in newSubRules
							  join o in this.subRules
								 on new
								 {
									 n.eHubRoutingRule.RR_Condition_Expression,
									 n.eHubRoutingRule.RR_Success_SP_Provider,
									 n.eHubRoutingRule.RR_Success_CC_Recipient,
									 n.eHubRoutingRule.RR_Success_RR_SubRule,
									 n.eHubRoutingRule.RR_Failed_ErrorCode,
									 n.eHubRoutingRule.RR_Failed_ErrorDescription,
									 n.eHubRoutingRule.RR_Failed_RR_SubRule,
									 n.eHubRoutingRule.RR_Result_Value
								 }
								 equals new
								 {
									 o.eHubRoutingRule.RR_Condition_Expression,
									 o.eHubRoutingRule.RR_Success_SP_Provider,
									 o.eHubRoutingRule.RR_Success_CC_Recipient,
									 o.eHubRoutingRule.RR_Success_RR_SubRule,
									 o.eHubRoutingRule.RR_Failed_ErrorCode,
									 o.eHubRoutingRule.RR_Failed_ErrorDescription,
									 o.eHubRoutingRule.RR_Failed_RR_SubRule,
									 o.eHubRoutingRule.RR_Result_Value
								 } into no
							  from m in no.DefaultIfEmpty()
							  select m ?? n).ToList();

			if (logger.IsDebugEnabled) logger.DebugFormat("Start adding new rules to the Context.");

			foreach (var item in mergedList.Except(this.subRules))
			{
				item.SetRuleContexts(this.rule, this.persistingContext);
				item.eHubRoutingRule.eHubRoutingRule_Group = this.parentRule;
				item.eHubRoutingRule.RR_Group_Ordering = null;
				this.parentRule.eHubRoutingRules_Group.Add(item.eHubRoutingRule);

				if (logger.IsDebugEnabled) logger.DebugFormat("Added Routing rule: {0}", item.GetRowInfoString());
			}

			if (logger.IsDebugEnabled) logger.DebugFormat("New rules are added to the Context.");

			EntityHelpers.ReorderEntities(mergedList.Select(r => r.eHubRoutingRule).ToList(), "RR_Group_Ordering");

			if (logger.IsDebugEnabled) logger.DebugFormat("Start deleting old rules from the Context.");

			var toDelete = this.subRules.Except(mergedList).ToList();
			foreach (var item in toDelete)
			{
				this.parentRule.eHubRoutingRules_Group.Remove(item.eHubRoutingRule);
				item.Delete();

				if (logger.IsDebugEnabled) logger.DebugFormat("Deleted Routing rule: {0}", item.GetRowInfoString());
			}
			if (logger.IsDebugEnabled) logger.DebugFormat("Old Rules are deleted from the Context");

			this.subRules = new List<Criterion>(mergedList);
		}

		public void Clear()
		{
			foreach (var item in this.subRules)
				item.Delete();
			this.subRules.Clear();
			this.parentRule.eHubRoutingRules_Group.Clear();
		}

		public int Count
		{
			get { return this.subRules.Count; }
		}

		public IEnumerator<Criterion> GetEnumerator()
		{
			return this.subRules.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.subRules.GetEnumerator();
		}

		internal void SetRuleContexts(Rule rule, eHubTransactionsContext persistingContext)
		{
			foreach (var subrule in this.subRules)
				subrule.SetRuleContexts(rule, persistingContext);
			this.persistingContext = persistingContext;
			this.rule = rule;
		}

		eHubRoutingRule parentRule;
		List<Criterion> subRules;
		eHubTransactionsContext persistingContext;
		Rule rule;
	}
}
