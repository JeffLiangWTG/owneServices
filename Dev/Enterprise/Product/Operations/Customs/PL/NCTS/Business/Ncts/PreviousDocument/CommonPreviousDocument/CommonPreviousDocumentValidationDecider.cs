using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CommonPreviousDocumentValidationDecider : ICommonPreviousDocumentValidationDecider
{
	public bool IsRuleG0321Active => true;

	public bool IsRuleTR0030_1Active => false;

	public bool IsRuleE1301Active => true;
}
