using System.Data;
using System.Linq;
using CargoWise.Customs.NL.MessageDefinitions.DMS.AdditionalMessage_1p30;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class CancelDeclarationEDIMessage : NLEDIMessage<MetaData>
{
	public CancelDeclarationEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
	}

	protected override NLEDIMessagePrettier GetNLEDIMessagePrettierCore() => new InvalidationMessagePrettier(this);

	public string InvalidationReason => invalidationReason ?? (invalidationReason = MessageDataObject?.Declaration?.AdditionalInformation?.FirstOrDefault()?.StatementDescription?.Value);
	string invalidationReason;
}
