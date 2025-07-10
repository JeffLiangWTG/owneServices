using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public class TRManifestMessage : TRBaseMessage
	{
		public TRManifestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString GetApplicationCodeCore()
		{
			return EDIMessage.ApplicationCodes.TRCustoms;
		}

		string formattedMessageText;
		public override ZString EM_FormattedMessageText
		{
			get
			{
				if (formattedMessageText == null)
				{
					if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					{
						formattedMessageText = MessageInterpretationNoteManager.Value;
					}
					else if (MessageObject != null)
					{
						formattedMessageText = MessageObject.ToString();
					}
					else if (EM_MessageType == TRMessageTypes.Codes.TRO && EM_ReceiveTransmit == EDIMessage.Direction.Receive && XmlUtils.IsValidXml(EM_MessageText, out var xmlDocument))
					{
						formattedMessageText = xmlDocument.ToFormattedString();
					}
					else
					{
						formattedMessageText = base.EM_FormattedMessageText;
					}
				}
				return formattedMessageText;
			}
		}

		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (messageInterpretation.IsEmpty)
				{
					messageInterpretation = MessageInterpretationNoteManager.Value;
					if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					{
						var messageText = messageInterpretation;
						if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
						{
							messageInterpretation = MessageInterpretationGenerator.CreateInterpretationContentForManifestRTx(EM_MessageType, xmlDocument);
						}
					}
				}

				var returnValue = messageInterpretation;
				messageInterpretation = ZString.Empty;
				return returnValue;
			}
		}
		ZString messageInterpretation;

		protected override SoapMessageObject GetMessageObject()
		{
			SoapMessageObject result = default;

			switch (EM_MessageType)
			{
				case TRMessageTypes.Codes.TRO:
					result = new TROMessageObject(EM_MessageText, this, null);
					break;
				case TRMessageTypes.Codes.T1O:
					result = new T1OMessageObject(EM_MessageText, this, null);
					break;
				case TRMessageTypes.Codes.T2O:
					result = new T2OMessageObject(EM_MessageText, this, null);
					break;
				case TRMessageTypes.Codes.T3O:
					result = new T3OMessageObject(EM_MessageText, this, null);
					break;
				case TRMessageTypes.Codes.TRM:
					result = new TRMMessageObject(EM_MessageText, this, null);
					break;
			}

			return result;
		}
	}
}
