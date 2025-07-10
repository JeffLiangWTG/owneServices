using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public sealed class GoodsItemPreviousDocumentProvider_CC515_CC513 : AESGoodsItemPreviousDocumentProvider
{
	readonly bool isAesTransitionPeriod;

	public GoodsItemPreviousDocumentProvider_CC515_CC513(CusSupportingInfo cusSupportingInfo, ZString procedureCode, bool isAesTransitionPeriod)
		: base(cusSupportingInfo, procedureCode)
	{
		this.isAesTransitionPeriod = isAesTransitionPeriod;
	}

	protected override string GetDescriptionCore() => AesRuleHelper.ApplyE1104Rule(base.GetDescriptionCore(), isAesTransitionPeriod);
}
