using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CargoWise.eServices.Encryption.Client.Encryptor;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.PL.Business.Constants;

[assembly: UniversalCustomsEDIMessagePacker(EDIMessage.ApplicationCodes.PLCustoms, typeof(Enterprise.Customs.PL.Business.PLMessagePacker))]
[assembly: UniversalCustomsEDIMessagePacker(EDIMessage.ApplicationCodes.PLCustomsNCTS, typeof(Enterprise.Customs.PL.Business.PLMessagePacker))]
[assembly: UniversalCustomsEDIMessagePacker(EDIMessage.ApplicationCodes.PLCustomsExitControl, typeof(Enterprise.Customs.PL.Business.PLMessagePacker))]

namespace Enterprise.Customs.PL.Business;

public class PLMessagePacker : IUniversalCustomsEDIMessagePacker
{
	public bool AllowEmptyMessageBody => false;

	public ZString Pack(Messaging.Business.EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		var errorBuilder = new ZStringBuilder();

		if (message.EM_MessageText.IsEmpty)
		{
			errorBuilder.Append(NoMessageTextErrorMessage);
			message.Notes.AddNew(true, ProcessingLogDescription, NoMessageTextErrorMessage);
		}
		if (message.EM_LinkedObject == null)
		{
			errorBuilder.Append(NoLinkedObjectErrorMessage);
			message.Notes.AddNew(true, ProcessingLogDescription, NoLinkedObjectErrorMessage);
		}
		if (!errorBuilder.IsEmpty)
		{
			message.EM_Status = EDIMessageStatusList.Codes.Failed;
			return errorBuilder.ToString();
		}

		interchange.EI_ApplicationCode = message.EM_ApplicationCode;
		interchange.EI_InterchangeType = message.EM_MessageType;
		interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
		interchange.EI_To = message.EM_IsTestMessage ? CustomsDestinationCodes.PlCustomsTest : CustomsDestinationCodes.PlCustoms;
		interchange.EI_GB = message.EM_GB;
		interchange.EI_GP = message.EM_GP;
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		interchange.EI_IsActive = true;
		interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
		interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
		interchange.EI_Priority = EDIInterchangePriorityList.Codes.High;

		if (message.EM_ApplicationCode == ApplicationCodeList.Codes.PLCustomsPUESCEmailSystem)
		{
			var fromEmailAddress = (NoResString)PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.Value;
			var destinationEmailAddress = (NoResString)PLCustomsDataRegistry.Instance.EAttachmentsEmailChannel.Value;
			var puescEmailAddress = (NoResString)PLCustomsDataRegistry.Instance.PUESCEmailChannel.Value;
			interchange.SetHeaderTextWithAttributeDictionary(new Dictionary<string, string>
			{
				[InterchangeHeaderAttributes.FromMailBox] = fromEmailAddress,
				[InterchangeHeaderAttributes.DestinationMailBox] = destinationEmailAddress.IfEmptyUse(() => puescEmailAddress)
			});
		}
		else
		{
			if (message.ExternalPassword is GlbExternalPassword credential)
			{
				var encryptedPassword = credential.CurrentDecryptedPassword.IsEmpty
					? string.Empty
					: EhubClientEncryptor.Encrypt(credential.CurrentDecryptedPassword);
				interchange.SetHeaderTextWithAttributeDictionary(new Dictionary<string, string>
				{
					[InterchangeHeaderAttributes.User] = credential.GP_MailBoxID,
					[InterchangeHeaderAttributes.EncryptedPassword] = encryptedPassword
				});
			}
		}

		string methodName;
		string requestBody;
		if (message.EM_MessageType == EdiMessageMessageType.CusPollingTransaction
			&& message.EM_MessageSubType == EDIMessageSubType.CusPollingTransaction)
		{
			methodName = Constants.PUESC.DocumentHandlingPort.XmlNodes.GetDocumentsMethod.Name;
			requestBody = message.EM_MessageText;
		}
		else
		{
			methodName = Constants.PUESC.DocumentHandlingPort.XmlNodes.AcceptDocumentMethod.Name;
			var content = Encoding.UTF8.GetBytes(message.EM_MessageText);
			var base64Content = Convert.ToBase64String(content);
			requestBody = WrapRequestBody(message.EM_MessageNum, base64Content);
		}
		interchange.EI_BodyText = WrapMessageTextToSoapEnvelope(methodName, interchange.EI_SessionGUID.ToString(), requestBody);

		return errorBuilder.ToString();
	}

	[SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "XML strings")]
	static string WrapRequestBody(string messageNum, string content) => $"""
		<AcceptDocumentRequest xmlns="http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0">
			<document xmlns="http://www.mf.gov.pl/schematy/SISC/WsChannel/2014/01_v2_0">
				<content filename="{messageNum}" mime="application/xml">{content}</content>
			</document>
		</AcceptDocumentRequest>
		""";

	[SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "XML strings")]
	static string WrapMessageTextToSoapEnvelope(string methodName, string uuid, string body) => $"""
		<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/" xmlns:a="http://schemas.xmlsoap.org/ws/2004/08/addressing">
			<s:Header>
				<a:Action s:mustUnderstand="1">{methodName}</a:Action>
				<a:MessageID>urn:uuid:{uuid}</a:MessageID>
				<a:ReplyTo><a:Address>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</a:Address></a:ReplyTo>
			</s:Header>
		<s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">{body}</s:Body>
		</s:Envelope>
		""";

	static ZString NoLinkedObjectErrorMessage => Res.GetString("14E1C4AB-0F38-4F91-AB64-72E0AF4D1620", "Message's Linked Object is null so can't continue with processing.") + " " + MessageSetToFailedErrorMessage;

	static ZString NoMessageTextErrorMessage => Res.GetString("00BE720A-0EFD-4B2B-B0B9-E5C057EE6166", "Message's Message Text is empty so can't continue with processing.") + " " + MessageSetToFailedErrorMessage;

	static ZString MessageSetToFailedErrorMessage => Res.GetString("EE11474C-5AC7-451E-B8DC-C482F63338E1", "The message's status has been set to 'Failed'.");

	[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Description")]
	ZString ProcessingLogDescription => "Packing Log";
}
