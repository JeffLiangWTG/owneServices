using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public sealed class GoodsShipmentProvider_CC515_CC513(CusEntryHeader entryHeader) : AESGoodsShipmentProvider(entryHeader)
{
	protected override IDeliveryTerms GetDeliveryTerms() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? null
		: base.GetDeliveryTerms();

	protected override IReadOnlyCollection<IDocument> GetAdditionalReferences() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? []
		: base.GetAdditionalReferences();

	protected override string GetNatureOfTransaction()
		=> !AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		&& GetRuleR0201_NatureOfTransactionOnGoodsShipmentLevel()
		? base.GetNatureOfTransaction()
		: null;

	protected override IAESConsignment GetConsignment() => new ConsignmentProvider_CC515_CC513(EntryHeader);

	protected override IGoodsItem GetNewGoodsItem(CusEntryLine entryLine, int index) => new GoodsItemProvider_CC515_CC513(entryLine, index + 1, this);

	bool GetRuleR0201_NatureOfTransactionOnGoodsShipmentLevel()
	{
		if (EntryHeader.InvoiceHeaders.Length <= 1)
		{
			return true;
		}

		var firstValue = EntryHeader.InvoiceHeaders[0].JZ_ValuationCode;
		return EntryHeader.InvoiceHeaders.Skip(1).All(header => header.JZ_ValuationCode == firstValue);
	}
}
