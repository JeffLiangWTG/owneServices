using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class TWControllingAgencyDeliveryNotificationMessageHelper : TWMessageHelper
	{
		public TWControllingAgencyDeliveryNotificationMessageHelper(TWMessage message) : base(message)
		{
			messageKeyInfomation = Message.IncomingMessageKeyInfomation;
			response = messageKeyInfomation.Result as XDocument;
		}

		readonly XDocument response;
		readonly TWIncomingMessageKeyInfomation messageKeyInfomation;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				WriteRow(table, Captions.EventTime, EventTime);
				WriteRow(table, Captions.EventType, GetEventTypeDescriptionFromCode(messageKeyInfomation.EventType));
				var errorCode = ErrorCode;
				if (!errorCode.IsEmpty)
				{
					WriteRow(table, Captions.ErrorCode, GetErrorCodeAndDescriptionFromCode(ErrorCode));
				}
				var errorDescription = ErrorDescription;
				if (!errorDescription.IsEmpty)
				{
					WriteRow(table, Captions.ErrorDescription, ErrorDescription);
				}
				WriteRow(table, Captions.FunctionalReferenceID, messageKeyInfomation.FunctionalReferenceID);
				WriteRow(table, Captions.MessageType, GetMessageTypeDescriptionFromCode(messageKeyInfomation.MessageType));
				WriteRow(table, Captions.EDIInterchangeNumber, InterchangeNumber);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xpath string")]
		ZString EventTime => response.XPathSelectElement("//*[local-name()='Event']/*[local-name()='EventTime']")?.Value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xpath string")]
		ZString ErrorCode => response.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='ErrorCode']/*[local-name()='Value']")?.Value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xpath string")]
		ZString ErrorDescription => response.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='Description']/*[local-name()='Value']")?.Value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xpath string")]
		ZString InterchangeNumber => response.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='InterchangeNumber']/*[local-name()='Value']")?.Value;

		public string GetErrorCodeAndDescriptionFromCode(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => ErrorCodeList.GetDescriptionFromCode(itemCode));
		}
		CodeDescriptionPairList ErrorCodeList => Factory.GetCachedValue<ErrorCodeList>();

		public string GetEventTypeDescriptionFromCode(string code)
		{
			return EventTypeCodeList.GetDescriptionFromCode(code);
		}
		CodeDescriptionPairList EventTypeCodeList => Factory.GetCachedValue<EventTypeCodeList>();

		public string GetMessageTypeDescriptionFromCode(ZString code)
		{
			var codeShortening = new MessageTypeCodeList().GetDescriptionFromCode(code);
			return $"{new ControllingMessageTypeList().GetDescriptionFromCode(codeShortening)} {codeShortening}";
		}
	}
}
