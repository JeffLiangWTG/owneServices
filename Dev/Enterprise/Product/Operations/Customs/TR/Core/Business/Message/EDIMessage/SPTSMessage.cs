using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class SPTSMessage : TRBaseMessage
	{
		public SPTSMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString GetApplicationCodeCore()
		{
			return EDIMessage.ApplicationCodes.TRCustoms;
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return ObjectFactory.GetType<Integration.Customs.TR.ICusInBondSPTSHeader>();
		}

		public override ZString EM_FormattedMessageText
		{
			get
			{
				if (formattedMessageText.IsEmpty)
				{
					if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					{
						formattedMessageText = MessageInterpretationNoteManager.Value;
					}
					else
					{
						XmlDocument xmlDocument = null;
						if (EM_MessageType == TRMessageTypes.Codes.T1P && EM_ReceiveTransmit == EDIMessage.Direction.Receive && XmlUtils.IsValidXml(EM_MessageText, out xmlDocument))
						{
							var xnm = new XmlNamespaceManager(xmlDocument.NameTable);
							xnm.AddNamespace("A", "http://schemas.xmlsoap.org/soap/envelope/");
							xnm.AddNamespace("B", "http://tempuri.org/");
							xnm.AddNamespace("C", (NoResString)"urn:schemas-microsoft-com:xml-diffgram-v1");
							var node = xmlDocument.SelectSingleNode("/A:Envelope/A:Body/B:IslemSonucGetir2Response/B:IslemSonucGetir2Result/C:diffgram/NewDataSet/Sonuc/GidenXML", xnm);
							if (node != null)
							{
								node.InnerXml = WebUtility.HtmlDecode(node.InnerXml);
							}

							formattedMessageText = MessageInterpretationGenerator.GetFormattedMessageText(xmlDocument);
						}
						else if (EM_MessageType == TRMessageTypes.Codes.TSP && EM_ReceiveTransmit == EDIMessage.Direction.Receive && XmlUtils.IsValidXml(EM_MessageText, out xmlDocument))
						{
							formattedMessageText = MessageInterpretationGenerator.GetFormattedMessageText(xmlDocument);
						}
						else
						{
							formattedMessageText = base.EM_FormattedMessageText;
						}
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
							messageInterpretation = MessageInterpretationGenerator.CreateInterpretationContentForSPTSRTx(EM_MessageType, xmlDocument);
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
