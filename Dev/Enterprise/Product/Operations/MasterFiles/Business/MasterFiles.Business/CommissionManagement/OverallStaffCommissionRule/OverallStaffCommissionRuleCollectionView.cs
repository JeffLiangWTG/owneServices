using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OverallStaffCommissionRuleCollectionView : OverallStaffCommissionRuleCollection
	{
		public OverallStaffCommissionRuleCollectionView(GlbStaff salesRep)
			: base(salesRep)
		{
		}

		protected override ZBool ShouldInclude(OverallStaffCommissionRule rule)
		{
			return
				((IncludeExpiredRules || rule.EndDate.IsEmpty || rule.EndDate >= ZDateTime.Today) &&
				(IncludeDisabledRules || rule.RuleDisable == null));
		}

		#region Filters

		public ZBool IncludeExpiredRules
		{
			get { return includeExpiredRules; }
			set
			{
				includeExpiredRules = value;
				LoadRules();
			}
		}
		ZBool includeExpiredRules;

		public ZBool IncludeDisabledRules
		{
			get { return includeDisabledRules; }
			set
			{
				includeDisabledRules = value;
				LoadRules();
			}
		}
		ZBool includeDisabledRules;

		#endregion
	}
}
