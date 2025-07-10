using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OverallStaffCommissionRuleFetchStrategy : BusinessObjectFetchStrategy
	{
		public OverallStaffCommissionRuleFetchStrategy(OverallStaffCommissionRule rule)
			: base(rule)
		{
		}

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var rule = BusinessObject as OverallStaffCommissionRule;
			if (rule != null)
			{
				if (columns.Any(col => OverrideableProperties.Contains(col.ColumnName) || col.ColumnName == OverallStaffCommissionRule.Schema.Status))
				{
					Factory.AddFetchHint(AccCommissionRuleStaffDisableSchema.Instance, rule.GetRuleDisableQuery());
					Factory.AddFetchHint(AccCommissionRuleStaffOverrideSchema.Instance, rule.GetRuleOverrideQuery());
				}
			}
		}

		HashSet<string> OverrideableProperties
		{
			get
			{
				if (overrideableProperties == null)
				{
					overrideableProperties = new HashSet<string>
					{
						OverallStaffCommissionRule.Schema.CommissionBasis,
						OverallStaffCommissionRule.Schema.CommissionTriggerType
					};
				}

				return overrideableProperties;
			}
		}
		HashSet<string> overrideableProperties;

		#endregion
	}
}
