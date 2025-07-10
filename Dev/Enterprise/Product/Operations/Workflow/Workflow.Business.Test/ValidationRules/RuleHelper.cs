using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Workflow.Business.Test
{
	public class RuleHelper
	{
		public RuleHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public BusinessObjectFactory Factory { get; }

		public UniversalValidationRuleSet CreateRuleSet(DataContextType context)
		{
			var ruleSet = Factory.NewWithValidTestData<UniversalValidationRuleSet>();
			ruleSet.VRS_Code = ZString.Empty;
			ruleSet.VRS_DataContext = context.ToString();
			return ruleSet;
		}

		public UniversalValidationRule AddRule(UniversalValidationRuleSet ruleSet, string macro)
		{
			var rule = ruleSet.Rules.AddNew();
			rule.VR_Status = rule.Lookups.StatusList[0].Code;
			rule.VR_BusinessRule = macro;
			rule.VR_Sequence = (short)ruleSet.Rules.Count;
			return rule;
		}
	}
}
