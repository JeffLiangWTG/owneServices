using System.Xml;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageDefinitions.XML.Control_1p15;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using EDIInterchange = Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.NL.Business;

public class InboundMessageCreator : IInboundMessageCreator
{
	void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));

		var messageCreated = CreateMessagesFromInterchangeXml(interchange);
		if (!messageCreated)
		{
			interchange.Logs.AddNew(AutoEvents.ErrorReport, $"No message has been created for interchange {interchange.EI_InterchangeNum}");
			interchange.EI_Status = Enterprise.Messaging.Integration.EDIInterchangeStatusList.Codes.Error;
		}
	}
	static bool CanDeserialize<T>(string xml)
	{
		try
		{
			var obj = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<T>(xml);
			return obj is T;
		}
		catch
		{
			return false;
		}
	}

	static ZBool CreateMessagesFromInterchangeXml(EDIInterchange interchange)
	{
		var result = false;

		var (errorMessage, xmlDocument) = ValidateMessageXML(interchange.EI_BodyText);
		if (!errorMessage.IsEmpty)
		{
			interchange.Logs.AddNew(AutoEvents.ErrorReport, $"{errorMessage}, the Message creation failed");
		}
		else
		{
			var isExitControl = interchange.EI_InterchangeType == NLEDIMessageTypes.Codes.EXT;
			var isControl = interchange.EI_InterchangeType == NLEDIMessageTypes.Codes.DMS && CanDeserialize<XmlControl>(interchange.EI_BodyText);
			var messageBodyNode = isExitControl ?  GetMessageBody(xmlDocument,  ExitControlResponseBodyNodeXPath) : null;
			var subMessageType = isExitControl ? GetExitControlMessageSubTypeByXml(messageBodyNode) : isControl ? NLIncomingMessageSubTypeList.Codes.Control : GetMessageSubTypeByXml(interchange.EI_InterchangeType, xmlDocument);
			var applicationReference = isControl ? NLConstants.EDIMessageApplicationReferences.Control : string.Empty;

			if (!subMessageType.IsEmpty)
			{
				var newEDIMessage = interchange.ContainedMessages.AddNew(typeof(NLEDIMessage));
				newEDIMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				newEDIMessage.EM_MessageText = messageBodyNode != null ? messageBodyNode.InnerXml : interchange.EI_BodyText;
				newEDIMessage.EM_MessageType = interchange.EI_InterchangeType;
				newEDIMessage.EM_MessageSubType = subMessageType;
				newEDIMessage.EM_ApplicationReference = applicationReference;
				result = true;
			}
			else
			{
				interchange.Logs.AddNew(AutoEvents.ErrorReport, $"Interchange {interchange.EI_InterchangeNum}: Can not determine Message Sub Type for the Received Interchange");
			}
		}
		return result;
	}

	static (ZString, XmlDocument) ValidateMessageXML(ZString xml)
	{
		var errorMessage = ZString.Empty;
		XmlDocument xmlDocument = null;

		try
		{
			xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
		}
		catch (XmlException)
		{
			errorMessage = (NoResString)"The Message XML is not a valid XML";
		}

		return (errorMessage, xmlDocument);
	}

	static XmlNode GetMessageBody(XmlDocument xmlDocument, string inNodePath)
	{
		if (xmlDocument != null)
		{
			return xmlDocument.DocumentElement?.SelectSingleNode(inNodePath);
		}
		return null;
	}

	static ZString GetMessageSubTypeByXml(ZString interchangeType, XmlDocument xmlDocument)
	{
		var result = ZString.Empty;
		var typeCode = ZString.Empty;

		switch (interchangeType)
		{
			case NLEDIMessageTypes.Codes.DMS:
				typeCode = xmlDocument.SelectSingleNode(WCOTypeCodeNodeXPath)?.InnerXml;
				break;
			case NLEDIMessageTypes.Codes.NCT:
				typeCode = xmlDocument?.DocumentElement?.LocalName ?? string.Empty;
				break;
		}

		if (!typeCode.IsEmpty)
		{
			var messageSubType = typeCode.SubstringSafe(2, 3);
			if (new NLIncomingMessageSubTypeList().ContainsCode(messageSubType))
			{
				result = messageSubType;
			}
		}
		return result;
	}

	static ZString GetExitControlMessageSubTypeByXml(XmlNode xmlNode)
	{
		var result = ZString.Empty;
		if (xmlNode != null)
		{
			var innerTypeCodes = xmlNode.SelectNodes(ExitControlTypeCodeNodeXPath);
			if (innerTypeCodes != null)
			{
				var messageSubTypeList = new NLIncomingMessageSubTypeList();
				foreach (XmlNode innerTypeCode in innerTypeCodes)
				{
					if (messageSubTypeList.ContainsCode(innerTypeCode.InnerText))
					{
						result = innerTypeCode.InnerText;
						break;
					}
				}
			}
		}
		return result;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xcode path")]
	const string WCOTypeCodeNodeXPath = "//*[local-name()='MetaData']/*[local-name()='WCOTypeCode']";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xcode path")]
	const string ExitControlResponseBodyNodeXPath = "//*[local-name()='SW2B']";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xcode path")]
	const string ExitControlTypeCodeNodeXPath = "//*[local-name()='Response']/*[local-name()='TypeCode']";
}
