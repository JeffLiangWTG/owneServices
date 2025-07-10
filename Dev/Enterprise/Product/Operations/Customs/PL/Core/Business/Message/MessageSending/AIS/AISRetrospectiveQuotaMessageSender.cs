using System.Collections.Generic;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public class AISRetrospectiveQuotaMessageSender : CusEntryHeaderMessageSender
{
	public AISRetrospectiveQuotaMessageSender(BusinessObjectFactory factory, BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent sendingObjectParent,
		IEnumerable<IJobDeclarationMessageSendingEntryLine> entryLines, ZString referenceNumber, ZString entryNumber)
		: base(factory, sendingObject, sendingObjectParent)
	{
		Argument.NotNull(entryLines, nameof(entryLines));
	}

	protected override IXmlMessageBuilder GetXmlMessageBuilder()
	{
		return null;
	}

	protected override ZString GetMessageSubType() => AISMessageCodes.Descriptions.ZCX05;
}
