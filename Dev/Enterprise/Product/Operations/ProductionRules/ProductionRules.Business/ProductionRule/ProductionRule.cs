using System.Data;
using CargoWise.EntityFramework;
using WTG.ProductionRules.Core;

namespace Enterprise.ProductionRules.Business
{
	public class ProductionRule : AutoProductionRule
	{
		public ProductionRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ProductionRuleSet RuleSet => Factory.Load<ProductionRuleSet>(PRL_PRS_RuleSet);

		public IProductionRule GetProductionRuleWrapper()
		{
			var ruleSet = RuleSet;
			return new ProductionRuleWrapper(
					ruleSet?.PRS_Context ?? string.Empty,
					ruleSet?.PRS_ContextSubType ?? string.Empty,
					PRL_Name, PRL_Description,
					PRL_Priority,
					PRL_RuleDefinition);
		}
	}
}
