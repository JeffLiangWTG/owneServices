namespace Enterprise.Customs.NL.NCTS.Business;

public class GoodsItemsConfiguration : EU.NCTS.Business.GoodsItemsConfiguration
{
	protected override EU.NCTS.Business.INctsDepartureCargoDescPhase5ValidationDecider GetDeparturePhase5ValidationDecider(EU.NCTS.Business.NctsCommonCargoDesc goodsItem) => new NctsDepartureCargoDescPhase5ValidationDecider();
}
