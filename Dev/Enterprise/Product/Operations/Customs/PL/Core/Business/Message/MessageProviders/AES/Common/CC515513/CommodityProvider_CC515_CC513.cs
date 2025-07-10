using System;
using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public sealed class CommodityProvider_CC515_CC513 : AESCommodityProvider
{
	CommodityProvider_CC515_CC513(CusEntryLine entryLine) : base(entryLine)
	{
	}

	public new static CommodityProvider_CC515_CC513 NewOrNull(CusEntryLine entryLine) => entryLine is not null
		? new CommodityProvider_CC515_CC513(entryLine)
		: null;

	protected override IReadOnlyCollection<IDangerousGoods> GetDangerousGoods() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? Array.Empty<IDangerousGoods>()
		: base.GetDangerousGoods();

	protected override IReadOnlyCollection<IDutiesAndTaxesType> GetCalculationOfTaxes() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? Array.Empty<IDutiesAndTaxesType>()
		: base.GetCalculationOfTaxes();
}
