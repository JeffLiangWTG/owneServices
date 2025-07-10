using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(BaseCusGuaranteeHeader), "CusGuaranteeRules")]
	public class CusGuaranteeRuleCollection<T> : CusGuaranteeRuleCollection
		where T : CusGuaranteeRule
	{
		public CusGuaranteeRuleCollection(BaseCusGuaranteeHeader master)
			: base(master)
		{
		}

		public CusGuaranteeRuleCollection(BaseCusGuaranteeHeader master, ZString ruleCode)
			: base(master, ruleCode)
		{
		}

		public new T this[int i] => (T)base[i];

		public new T AddNew() => (T)base.AddNew();
	}

	public abstract class CusGuaranteeRuleCollection : ActiveBusinessObjectCollection<CusGuaranteeRule>
	{
		public CusGuaranteeRuleCollection(BaseCusGuaranteeHeader master)
			: base(master.Factory, master, GetGeneralPurposeRulesFilterQuery(), CusPermitRuleSchema.CPR_CPH_PermitHeader)
		{
		}

		public CusGuaranteeRuleCollection(BaseCusGuaranteeHeader master, ZString ruleCode)
						: base(master.Factory, master, GetSingleTypedRulesFilterQuery(ruleCode), CusPermitRuleSchema.CPR_CPH_PermitHeader)
		{
			this.ruleCode = ruleCode;
		}
		readonly ZString ruleCode;

		protected override void SetDefaultsForNewElementCore(CusGuaranteeRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (!ruleCode.IsEmpty)
			{
				newElement.CPR_RuleCode = ruleCode;
			}
		}

		static ZQuery GetSingleTypedRulesFilterQuery(ZString ruleCode)
		{
			var query = new ZQuery();
			query.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, SQLComparisonOperator.Equal, ruleCode);
			return query;
		}

		static ZQuery GetGeneralPurposeRulesFilterQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, SQLComparisonOperator.NotEqual, PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber);
			query.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, SQLComparisonOperator.NotEqual, PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin);
			return query;
		}
	}
}
