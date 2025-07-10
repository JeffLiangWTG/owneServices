using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public sealed class GoodsItemSupportingDocumentProvider_CC515_CC513 : AESGoodsItemSupportingDocumentProvider
{
	readonly bool isAesTransitionPeriod;

	public GoodsItemSupportingDocumentProvider_CC515_CC513(CusSupportingInfo cusSupportingInfo, Func<ZDecimal> getAmount, Func<ZDecimal> getQuantity, bool isAesTransitionPeriod)
		: base(cusSupportingInfo, getAmount, getQuantity)
	{
		this.isAesTransitionPeriod = isAesTransitionPeriod;
	}

	protected override string GetDescriptionCore() => AesRuleHelper.ApplyE1104Rule(base.GetDescriptionCore(), isAesTransitionPeriod);
}
