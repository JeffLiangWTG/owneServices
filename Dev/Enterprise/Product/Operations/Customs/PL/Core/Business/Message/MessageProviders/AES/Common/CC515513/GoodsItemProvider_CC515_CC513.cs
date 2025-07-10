using System;
using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class GoodsItemProvider_CC515_CC513(CusEntryLine entryLine, int index, AESGoodsShipmentProvider parentProvider)
	: AESGoodsItemProvider(entryLine, index, parentProvider)
{
	protected override IReadOnlyCollection<IDocument> GetTransportDocuments() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? Array.Empty<IPreviousDocumentSpecialProcedures>()
		: base.GetTransportDocuments();

	protected override IConsigneeConsignor GetConsignor() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? null
		: base.GetConsignor();

	protected override IReadOnlyCollection<IDocument> GetAdditionalReferences() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? Array.Empty<IDocument>()
		: base.GetAdditionalReferences();

	protected override ICommodity GetCommodity() => CommodityProvider_CC515_CC513.NewOrNull(EntryLine);

	protected override string GetNatureOfTransaction() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? null
		: base.GetNatureOfTransaction();

	protected override IReadOnlyCollection<IPreviousDocumentSpecialProcedures> GetPreviousDocumentSpecialProcedures() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? Array.Empty<IPreviousDocumentSpecialProcedures>()
		: base.GetPreviousDocumentSpecialProcedures();

	protected override IConsigneeConsignor GetConsigneeCore() => ConsigneeConsignorProvider_CC515_CC513.NewOrNull(invoiceLine.ConsigneeAddress, IsAESTransitionPeriod);

	protected override IPreviousDocument GetPreviousDocumentCore(Enterprise.Customs.Business.CusSupportingInfo cusSupportingInfo) =>
		new GoodsItemPreviousDocumentProvider_CC515_CC513(cusSupportingInfo, EntryInstruction.CEI_Procedure, EntryInstruction.IsAESTransitionPeriod());

	protected override ISupportingDocument GetSupportingDocumentCore(Enterprise.Customs.Business.CusSupportingInfo cusSupportingInfo, Func<ZDecimal> getAmount, Func<ZDecimal> getQuantity) =>
		new GoodsItemSupportingDocumentProvider_CC515_CC513(cusSupportingInfo, getAmount, getQuantity, EntryInstruction.IsAESTransitionPeriod());

	protected override IDocument GetAdditionalReferenceCore(Enterprise.Customs.Business.CusSupportingInfo cusSupportingInfo) =>
			new DocumentProvider_CC515_CC513(cusSupportingInfo, referenceNumberAsDescription: true, EntryInstruction.IsAESTransitionPeriod());
}
