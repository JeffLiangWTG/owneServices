using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class AIMMessageBuilder
	{
		readonly ZString messageType;
		readonly ZString messageSubType;
		readonly BusinessObjectFactory factory;
		readonly IAIMMessageHeader messageHeader;
		readonly bool isBillMessage;

		public AIMMessageBuilder(IAIMMessageHeader aimMessageHeader, BusinessObjectFactory factory, bool isBillMessage = false)
		{
			this.messageHeader = Argument.NotNull(aimMessageHeader, nameof(aimMessageHeader));
			this.messageType = EDIMessageTypeList.Codes.FHL;
			this.messageSubType = messageHeader.MessageType;
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.isBillMessage = isBillMessage;
		}

		public AIMEDIMessage Build()
		{
			var message = factory.New<AIMEDIMessage>();
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = GetMessageText();
			message.EM_ApplicationReference = messageHeader.Reference.Left(EDIMessage.Schema.EM_ApplicationReferenceMaxLength);
			return message;
		}

		ZString GetMessageText()
		{
			var stringBuilder = new ZStringBuilder();
			var blockGenerator = GetBlockGenerator(messageHeader.MessageType);
			if (blockGenerator != null)
			{
				foreach (var messageBlock in blockGenerator.Generate())
				{
					var message = messageBlock.CreateElements().ToString();
					stringBuilder.Append(message);
				}
			}

			return stringBuilder.ToString();
		}

		AIMBlockGenerator GetBlockGenerator(ZString messageIdentifier)
		{
			AIMBlockGenerator generator = null;
			if (isBillMessage)
			{
				switch (messageIdentifier)
				{
					case AIMMessageSubTypes.FRI:
						generator = new FRIBillBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FXI:
						generator = new FXIBillBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FRC:
						generator = new FRCBillBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FXC:
						generator = new FXCBillBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FRX:
						generator = new FRXBillBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FXX:
						generator = new FXXBillBlockGenerator(messageHeader);
						break;
				}
			}
			else
			{
				switch (messageIdentifier)
				{
					case AIMMessageSubTypes.FRI:
						generator = new FRIBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FXI:
						generator = new FXIBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FRC:
						generator = new FRCBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FXC:
						generator = new FXCBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FRX:
						generator = new FRXBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FXX:
						generator = new FXXBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FDM:
						generator = new FDMBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FSN:
						generator = new FSNBlockGenerator(messageHeader);
						break;
					case AIMMessageSubTypes.FSQ:
						generator = new FSQBlockGenerator(messageHeader);
						break;
				}
			}
			return generator;
		}
	}
}
