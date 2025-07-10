using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCTariffRuleCollection : ActiveBusinessObjectCollection<USCTariffRule>
	{
		public USCTariffRuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public USCTariffRuleCollection(USCRule rule)
			: base(rule.Factory, new ZQuery(USCTariffRuleSchema.U1_RuleCode, rule.U0_Code))
		{
			this.rule = rule;
		}

		readonly USCRule rule;

		protected override void SetDefaultsForNewElementCore(USCTariffRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (rule != null)
			{
				newElement.U1_RuleCode = rule.U0_Code;

				USCTariffRule previousRule = null;
				if (Count > 0)
				{
					previousRule = this[Count - 1];
				}

				if (previousRule != null)
				{
					newElement.U1_DateFrom = previousRule.U1_DateFrom;
					newElement.U1_DateTo = previousRule.U1_DateTo;
				}
			}
		}
	}
}
