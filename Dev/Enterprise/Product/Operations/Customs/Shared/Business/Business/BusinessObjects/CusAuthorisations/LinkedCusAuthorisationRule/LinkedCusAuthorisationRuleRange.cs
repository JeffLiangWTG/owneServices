using CargoWise.Common;

namespace Enterprise.Customs.Business
{
	public class LinkedCusAuthorisationRuleRange
	{
		public LinkedCusAuthorisationRuleRange(string ruleType, int minRequired, int maxAllowed)
		{
			RuleType = Argument.NotNullOrEmpty(ruleType, nameof(ruleType));
			MinRequired = minRequired;
			MaxAllowed = maxAllowed;
		}

		public string RuleType { get; }

		public int MinRequired { get; }

		public int MaxAllowed { get; }
	}
}
