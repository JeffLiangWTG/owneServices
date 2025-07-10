using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class NctsHeaderMessageSendingObjectValidationDecider : INctsHeaderMessageSendingObjectValidationDecider
{
	public bool IsRuleC0220Active => true;
	public bool IsRuleC0315Active => true;
	public bool IsRuleTR0020Active => true;
	public bool IsRuleTR0021Active => true;
}
