using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class NctsBillAdditionalDocumentPhase5ValidationDecider : INctsBillAdditionalDocumentPhase5ValidationDecider
{
	public bool IsRuleE1301Active => true;

	public bool IsRuleG0321Active => true;

	public bool IsRuleR3062Active => true;

	public bool IsRuleTR0031Active => true;

	public bool IsRuleTR0032Active => true;

	public bool IsRuleTR0033Active => true;

	public bool IsRuleTR0062Active => false;
}
