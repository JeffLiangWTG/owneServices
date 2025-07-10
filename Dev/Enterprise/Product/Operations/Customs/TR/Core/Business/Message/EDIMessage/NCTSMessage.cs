using System.Data;
using System.Net;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public class NCTSMessage : TRBaseMessage
	{
		public NCTSMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
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
					if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit && EM_MessageType == TRMessageTypes.Codes.TRN)
					{
						formattedMessageText = MessageInterpretationNoteManager.Value;
					}
					else
					{
						formattedMessageText = base.EM_FormattedMessageText;
					}
				}
				return formattedMessageText;
			}
		}

		string textIndentedXml;
		public override ZString EM_MessageTextIndentedXml
		{
			get
			{
				if (textIndentedXml == null)
				{
					if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					{
						textIndentedXml = MessageInterpretationNoteManager.Value;
					}
					else if (MessageObject != null)
					{
						textIndentedXml = MessageObject.ToString();
					}
					else if (EM_ReceiveTransmit == EDIMessage.Direction.Receive && XmlUtils.IsValidXml(EM_MessageText, out var xmlDocument))
					{
						if (EM_MessageType == TRMessageTypes.Codes.T2N)
						{
							var xnm = new XmlNamespaceManager(xmlDocument.NameTable);
							xnm.AddNamespace("S", "http://schemas.xmlsoap.org/soap/envelope/");
							xnm.AddNamespace("ns0", "http://ws/");
							var node = xmlDocument.SelectSingleNode("/S:Envelope/S:Body/ns0:downloadmessagebyindexResponse/return/msgContent", xnm);
							if (node != null)
							{
								node.InnerXml = WebUtility.HtmlDecode(node.InnerXml);
							}
						}
						textIndentedXml = MessageInterpretationGenerator.GetFormattedMessageText(xmlDocument);
					}
					else
					{
						textIndentedXml = base.EM_MessageTextIndentedXml;
					}
				}
				return textIndentedXml;
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
						if (XmlUtils.IsValidXml(messageInterpretation, out var xmlDocument))
						{
							messageInterpretation = MessageInterpretationGenerator.CreateInterpretationContentForNCTSRTx(EM_MessageType, xmlDocument);
						}
					}
				}
				var returnValue = messageInterpretation;
				messageInterpretation = ZString.Empty;
				return returnValue;
			}
		}
		ZString messageInterpretation;
	}
}
