namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class ValidationRuleConfiguration : EU.NCTS.Business.ValidationRuleConfiguration
{
	protected override bool IsRuleB1896ActiveCore() => true;

	protected override bool IsRuleC0236ActiveCore() => false;

	protected override bool IsRuleC0839ActiveCore() => false;

	protected override bool IsRuleG0321ActiveCore() => true;

	protected override bool IsRuleG0587ActiveCore() => true;

	public bool IsRuleNR0032Active => true;

	public bool IsRuleNR0033Active => true;

	protected override bool IsRuleRP16ActiveCore() => true;
}
