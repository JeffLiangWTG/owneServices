using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OverallStaffCommissionRuleCollection : NonPersistentBusinessObjectCollection<OverallStaffCommissionRule>
	{
		public OverallStaffCommissionRuleCollection(GlbStaff salesRep)
			: base(salesRep.Factory)
		{
			this.master = salesRep;
		}

		readonly GlbStaff master;

		public void LoadRules()
		{
			using (SuspendListChanged())
			{
				RemoveAll();

				var salesTeamsSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GG);
				salesTeamsSubQuery.AddToFilter(GlbGroupLinkSchema.GK_GS, master.PK);
				var inheritedRulesFilter = new ZDBOnlyQuery(typeof(AccCommissionRule));
				inheritedRulesFilter.AddSubQuery(AccCommissionRuleSchema.ACM_GG, salesTeamsSubQuery, JoinCondition.And);
				inheritedRulesFilter.AddToFilter(AccCommissionRuleSchema.ACM_GS_NKStaff, "");

				var staffRulesQuery = new ZQuery(AccCommissionRuleSchema.ACM_GS_NKStaff, master.GS_Code);

				var allRules = Factory.Load<AccGroupCommissionRule>(inheritedRulesFilter).Select(inheritedTeamRule => new OverallStaffCommissionRule(master, inheritedTeamRule))
					.Union(Factory.Load<AccStaffCommissionRule>(staffRulesQuery).Select(staffRule => new OverallStaffCommissionRule(staffRule)));

				foreach (var rule in allRules)
				{
					if (ShouldInclude(rule))
					{
						Add(rule);
					}
				}
			}
		}

		protected virtual ZBool ShouldInclude(OverallStaffCommissionRule rule)
		{
			return true;
		}

		#region New Child

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var staffRule = master.CommissionRules.AddNew();
			return new OverallStaffCommissionRule(staffRule);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OverallStaffCommissionRule)child).CompanyPk = GlbCompany.CurrentCompany.PK;
		}

		#endregion
	}
}
