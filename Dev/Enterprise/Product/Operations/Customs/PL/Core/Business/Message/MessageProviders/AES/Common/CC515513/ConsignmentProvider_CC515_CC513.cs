using System;
using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public sealed class ConsignmentProvider_CC515_CC513 : AESConsignmentProvider
{
	public ConsignmentProvider_CC515_CC513(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	protected override IConsigneeConsignor GetConsignor() => AesRuleHelper.CheckRuleR0089E(entryInstruction)
		? null
		: base.GetConsignor();

	protected override IConsigneeConsignor GetConsigneeCore() => ConsignmentConsigneeConsignorProvider_CC515_CC513.NewOrNull(declaration.ImporterDocumentaryAddress, entryInstruction.IsAESTransitionPeriod());

	protected override string GetCarrierIdentificationNumber() => AesRuleHelper.CheckRuleR0089E(entryInstruction)
		? null
		: base.GetCarrierIdentificationNumber();

	protected override IReadOnlyCollection<IDocument> GetTransportDocuments() => AesRuleHelper.CheckRuleR0089E(entryInstruction)
		? Array.Empty<IDocument>()
		: base.GetTransportDocuments();

	protected override IReadOnlyCollection<ICountryOfRoutingOfConsignment> GetCountryOfRoutingOfConsignments() => AesRuleHelper.CheckRuleR0089E(entryInstruction)
		? Array.Empty<ICountryOfRoutingOfConsignment>()
		: base.GetCountryOfRoutingOfConsignments();

	protected override string GetTransportChargesMethodOfPayment() => AesRuleHelper.CheckRuleR0089E(entryInstruction)
		? null
		: base.GetTransportChargesMethodOfPayment();
}
