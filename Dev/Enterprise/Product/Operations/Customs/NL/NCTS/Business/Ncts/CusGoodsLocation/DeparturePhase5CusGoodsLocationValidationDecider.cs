namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class DeparturePhase5CusGoodsLocationValidationDecider : EU.NCTS.Business.IDeparturePhase5CusGoodsLocationValidationDecider
{
	public bool IsRuleNR0013Active => false;

	public bool IsRuleNR0023Active => false;

	public bool IsRuleNR0050Active => false;

	public bool IsRuleNR0063Active => true;

	public bool IsRuleC0394Active => true;

	public bool IsRuleC0382Active => true;

	public bool IsRuleTR0061Active => true;
}
