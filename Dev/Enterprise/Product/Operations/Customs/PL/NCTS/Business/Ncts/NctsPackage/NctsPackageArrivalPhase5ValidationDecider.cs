using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class NctsPackageArrivalPhase5ValidationDecider : INctsPackageArrivalPhase5ValidationDecider
{
	public bool IsRuleB1919Active => true;
	public bool IsRuleC0670Active => true;
	public bool IsRuleNR0029Active => false;
	public bool IsRuleNR0061Active => false;
	public bool IsRuleR0220Active => true;
	public bool IsRuleTR0097Active => true;
}
