namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class NctsBillArrivalPhase5ValidationDecider : EU.NCTS.Business.INctsBillArrivalPhase5ValidationDecider
{
	public bool IsRuleB1964Active => true;

	public bool IsRuleC0909Active => false;

	public bool IsRuleNR0062Active => false;
}
