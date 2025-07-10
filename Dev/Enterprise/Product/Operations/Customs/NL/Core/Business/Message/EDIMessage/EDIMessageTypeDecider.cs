using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.Business;

public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.NL.IEDIMessageTypeDecider
{
	public override Type GetTypeForNew() => null;

	public override Type GetTypeForBinding() => null;

	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		var direction = row[EDIMessageSchema.Constants.EM_ReceiveTransmit].ToString().Trim();
		if (direction == EDIMessage.Direction.Receive)
		{
			var messageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			switch (messageType)
			{
				case NLEDIMessageTypes.Codes.DMS:
				case NLEDIMessageTypes.Codes.NCT:
				default:
					return typeof(NLEDIMessage);
			}
		}
		else
		{
			var messageSubType = row[EDIMessageSchema.Constants.EM_MessageSubType].ToString().Trim();
			switch (messageSubType)
			{
				case CommonSendMessageTypes.Codes.CAN:
					return typeof(CancelDeclarationEDIMessage);
				case CommonSendMessageTypes.Codes.AMD:
				case ImportSendMessageTypes.Codes.CRI:
					return typeof(NLComparisonEDIMessage);
				default:
					return typeof(NLEDIMessage);
			}
		}
	}
}
