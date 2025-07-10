namespace CargoWise.RefDbRepo.Staging.Rule
{
	public interface IRuleResult<T>
	{
		T Data { get; }
		ITariffRuleWrapper Rule { get; }
		RuleType RuleType { get; }
	}
}