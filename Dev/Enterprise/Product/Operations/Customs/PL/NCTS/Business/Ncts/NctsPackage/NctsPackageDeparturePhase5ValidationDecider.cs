using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class NctsPackageDeparturePhase5ValidationDecider : INctsPackageDeparturePhase5ValidationDecider
{
	public bool IsRuleB1819Active => true;
	public bool IsRuleB1919Active => true;
	public bool IsRuleC0060Active => true;
	public bool IsRuleC0060_1Active => false;
	public bool IsRuleC0060_2Active => false;
	public bool IsRuleC0060_3Active => false;
	public bool IsRuleC0670Active => true;
	public bool IsRuleE1111Active => true;
	public bool IsRuleNR0003Active => false;
	public bool IsRuleNR0027Active => false;
	public bool IsRuleR0219Active => true;
	public bool IsRuleR0220Active => true;
	public bool IsRuleR0364_1Active => false;
	public bool IsRuleR0364_2Active => false;
	public bool IsRuleR0364_3Active => false;
	public bool IsRuleTR0066Active => true;
	public bool IsRuleTR0083Active => true;
}
