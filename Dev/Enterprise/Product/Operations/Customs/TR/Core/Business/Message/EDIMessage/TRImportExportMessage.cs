using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public class TRImportExportMessage : TRBaseMessage
	{
		public TRImportExportMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString GetApplicationCodeCore() => EDIMessage.ApplicationCodes.TRCustoms;

		string formattedMessageText;
		public override ZString EM_FormattedMessageText
		{
			get
			{
				if (formattedMessageText == null)
				{
					if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit && EM_MessageType == TRMessageTypes.Codes.DTE)
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
					else if ((EM_MessageType == TRMessageTypes.Codes.DKO || EM_MessageType == TRMessageTypes.Codes.DTE) &&
							  EM_ReceiveTransmit == EDIMessage.Direction.Receive && XmlUtils.IsValidXml(EM_MessageText, out var xmlDocument))
					{
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
				var messageInterpretation = MessageInterpretationNoteManager.Value;
				if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit && EM_LinkedObject != null)
				{
					if (XmlUtils.IsValidXml(messageInterpretation, out var xmlDocument) && EM_LinkedObject is Integration.Customs.TR.ICusEntryHeader cusEntryHeader)
					{
						var header = cusEntryHeader as CusEntryHeader;
						var declaration = header.Declaration;

						var entryType = declaration.JE_MessageType;
						var supplierCompanyName = !declaration.SupplierDocumentaryAddress.IsNull ? declaration.SupplierDocumentaryAddress.CompanyName : ZString.Empty;
						var importerCompanyName = !declaration.ImporterDocumentaryAddress.IsNull ? declaration.ImporterDocumentaryAddress.CompanyName : ZString.Empty;
						var portOfLoading = declaration.JE_RL_NKPortOfLoading;
						var portOfArrival = declaration.JE_RL_NKPortOfArrival;
						var voyage = declaration.JE_VoyageFlightNo;

						messageInterpretation = MessageInterpretationGenerator.CreateInterpretationContentForDeclarationRTx(EM_MessageType, xmlDocument, entryType, supplierCompanyName,
																															importerCompanyName, portOfLoading, portOfArrival, voyage);
					}
				}

				return messageInterpretation;
			}
		}

		protected override SoapMessageObject GetMessageObject()
		{
			SoapMessageObject result = default;

			switch (EM_MessageType)
			{
				case TRMessageTypes.Codes.DK1:
					result = new DK1MessageObject(EM_MessageText, this, null);
					break;
				case TRMessageTypes.Codes.DT1:
					result = new DT1MessageObject(EM_MessageText, this, null);
					break;
				case TRMessageTypes.Codes.DT2:
					result = new DT2MessageObject(EM_MessageText, this, null);
					break;
				case TRMessageTypes.Codes.DT3:
					result = new DT3MessageObject(EM_MessageText, this, null);
					break;
			}

			return result;
		}
	}
}
