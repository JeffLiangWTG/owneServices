using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsPreviousDocumentDeparturePhase5ValidationDecider : INctsPreviousDocumentDeparturePhase5ValidationDecider
{
	public bool IsRuleG0321Active => true;

	public bool IsRuleC0298Active => true;

	public bool IsRuleNR0008Active => false;

	public bool IsRuleNR0046Active => false;

	public bool IsRuleG0058_1Active => false;

	public bool IsRuleTR0030_1Active => false;

	public bool IsRuleNR0066Active => false;
}
