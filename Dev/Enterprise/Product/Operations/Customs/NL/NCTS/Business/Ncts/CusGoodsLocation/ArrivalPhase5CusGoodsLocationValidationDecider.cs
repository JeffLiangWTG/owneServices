using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class ArrivalPhase5CusGoodsLocationValidationDecider : IArrivalPhase5CusGoodsLocationValidationDecider
{
	public bool IsRuleC0382Active => true;

	public bool IsRuleNR0011Active => false;

	public bool IsRuleNR0012Active => false;

	public bool IsRuleNR0013Active => false;

	public bool IsRuleNR0075Active => true;

	public bool IsRuleTR0061Active => true;

	public bool IsRuleTR0069Active => false;

	public bool IsRuleC0394Active => true;
}
