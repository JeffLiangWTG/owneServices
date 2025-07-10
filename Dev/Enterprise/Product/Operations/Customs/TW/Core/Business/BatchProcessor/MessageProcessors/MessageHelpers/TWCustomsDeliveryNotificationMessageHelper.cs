using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class TWCustomsDeliveryNotificationMessageHelper : TWMessageHelper
	{
		public TWCustomsDeliveryNotificationMessageHelper(TWMessage message) : base(message)
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
				WriteRow(table, Captions.EntryNumber, messageKeyInfomation.DeclarationID);
				WriteRow(table, Captions.EntryNumberType, GetEntryNumberTypeDescriptionFromCode(messageKeyInfomation.EntryType));
				WriteRow(table, Captions.MessageType, GetMessageTypeDescriptionFromCode(messageKeyInfomation.MessageType));
				WriteRow(table, Captions.EDIInterchangeNumber, messageKeyInfomation.InterchangeNumber);
				if (!errorCode.IsEmpty)
				{
					WriteRow(table, Captions.ErrorSuggestion, GetErrorSuggestionFromCode(ErrorCode));
				}
			}
		}

		ZString EventTime => response.XPathSelectElement((NoResString)"//*[local-name()='Event']/*[local-name()='EventTime']")?.Value;

		ZString ErrorCode => response.XPathSelectElement((NoResString)"//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='ErrorCode']/*[local-name()='Value']")?.Value;

		ZString ErrorDescription => response.XPathSelectElement((NoResString)"//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='Description']/*[local-name()='Value']")?.Value;

		public string GetErrorCodeAndDescriptionFromCode(string code)
		{
			return GetDescriptionFromCode(code, (itemCode) => ErrorCodeList.GetDescriptionFromCode(itemCode));
		}
		CodeDescriptionPairList ErrorCodeList => Factory.GetCachedValue<ErrorCodeList>();

		public string GetErrorSuggestionFromCode(string code)
		{
			return ErrorSuggestionList.GetDescriptionFromCode(code);
		}
		CodeDescriptionPairList ErrorSuggestionList => Factory.GetCachedValue<ErrorSuggestionList>();

		public string GetEventTypeDescriptionFromCode(string code)
		{
			return EventTypeCodeList.GetDescriptionFromCode(code);
		}
		CodeDescriptionPairList EventTypeCodeList => Factory.GetCachedValue<EventTypeCodeList>();

		public string GetEntryNumberTypeDescriptionFromCode(ZString code)
		{
			var result = code;
			switch (code)
			{
				case SharedJobMessageTypeList.Codes.Export:
					result = EntryNumberTypeDescriptions.Export;
					break;
				case SharedJobMessageTypeList.Codes.Import:
					result = EntryNumberTypeDescriptions.Import;
					break;
			}
			return result;
		}

		public string GetMessageTypeDescriptionFromCode(ZString code)
		{
			var result = code;
			switch (code)
			{
				case MessageTypeList.Codes.ECD:
					result = MessageTypeDescriptions.ECD;
					break;
				case MessageTypeList.Codes.ICD:
					result = MessageTypeDescriptions.ICD;
					break;
				case MessageTypeList.Codes.ADM:
					result = MessageTypeDescriptions.ADM;
					break;
				case MessageTypeList.Codes.IEA:
					result = MessageTypeDescriptions.IEA;
					break;
				case MessageTypeList.Codes.FHM:
					result = MessageTypeDescriptions.FHM;
					break;
			}
			return result;
		}
	}
}
