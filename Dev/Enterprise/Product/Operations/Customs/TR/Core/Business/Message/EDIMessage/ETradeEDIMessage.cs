using System.Data;
using System.Net;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeEDIMessage : TRBaseMessage
	{
		public ETradeEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString GetApplicationCodeCore()
		{
			return EDIMessage.ApplicationCodes.TRCustoms;
		}

		public override ZString EM_FormattedMessageText
		{
			get
			{
				if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
				{
					formattedMessageText = MessageInterpretationNoteManager.Value;
				}
				else
				{
					XmlDocument xmlDocument = null;
					if (EM_ReceiveTransmit == EDIMessage.Direction.Receive)
					{
						var nodeList = GetNodeList(EM_MessageType);
						if (!nodeList.IsEmpty && XmlUtils.IsValidXml(EM_MessageText, out xmlDocument))
						{
							var xnm = new XmlNamespaceManager(xmlDocument.NameTable);
							xnm.AddNamespace("A", "http://schemas.xmlsoap.org/soap/envelope/");
							xnm.AddNamespace("B", "http://tempuri.org/");
							xnm.AddNamespace("C", "http://schemas.datacontract.org/2004/07/Gov.GTB.EGEWS.Domain.ETicaret");

							var node = xmlDocument.SelectSingleNode(nodeList, xnm);
							if (node != null)
							{
								node.InnerXml = WebUtility.HtmlDecode(node.InnerXml);
							}

							formattedMessageText = MessageInterpretationGenerator.GetFormattedMessageText(xmlDocument);
						}
						else
						{
							formattedMessageText = base.EM_MessageText;
						}
					}
					else
					{
						formattedMessageText = base.EM_FormattedMessageText;
					}
				}

				return formattedMessageText;
			}
		}

		ZString formattedMessageText;

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
							messageInterpretation = MessageInterpretationGenerator.CreateInterpretationContentForETradeRTx(EM_MessageType, xmlDocument);
						}
					}
				}
				var returnValue = messageInterpretation;
				messageInterpretation = ZString.Empty;
				return returnValue;
			}
		}
		ZString messageInterpretation;

		ZString GetNodeList(ZString messageType)
		{
			var nodeList = ZString.Empty;
			switch (messageType)
			{
				case TRMessageTypes.Codes.TRI:
					nodeList = "/A:Envelope/A:Body/B:ETGBMuayeneMemuruSorgulaResponse/B:ETGBMuayeneMemuruSorgulaResult";
					break;
				case TRMessageTypes.Codes.TRB:
					nodeList = "/A:Envelope/A:Body/B:AyrilanTasimaSenediSorgulaResponse/B:AyrilanTasimaSenediSorgulaResult";
					break;
				case TRMessageTypes.Codes.T1E:
				case TRMessageTypes.Codes.T2D:
				case TRMessageTypes.Codes.T1D:
					nodeList = "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult/C:XmlData";
					break;
			}

			return nodeList;
		}

		protected override SoapMessageObject GetMessageObject()
		{
			SoapMessageObject result = default;

			switch (EM_MessageType)
			{
				case TRMessageTypes.Codes.TRI:
					result = new TRIMessageObject(EM_MessageText, this, null);
					break;
			}

			return result;
		}
	}
}

