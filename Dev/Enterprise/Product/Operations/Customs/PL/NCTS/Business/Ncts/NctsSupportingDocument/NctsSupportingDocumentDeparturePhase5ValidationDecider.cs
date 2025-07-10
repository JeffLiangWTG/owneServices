using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsSupportingDocumentDeparturePhase5ValidationDecider : INctsSupportingDocumentDeparturePhase5ValidationDecider
{
	public bool IsRuleE1301Active => true;

	public bool IsRuleG0321Active => true;

	public bool IsRuleNR0006Active => false;

	public bool IsRuleRP30Active => true;
}
