using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public abstract class SuretyToBrokerNoticeMessageProcessor
	{
		protected SuretyToBrokerNoticeMessageProcessor(Event eventDataObject)
		{
			this.EventDataObject = eventDataObject;
		}

		public Event EventDataObject;

		public void SendAcknowledgementReport()
		{
			var emailBody = GetEmailBody(EventDataObject);
			GenerateHtmlEmailAndSendToBrokerOrGroup(emailBody);
		}

		protected abstract void GenerateHtmlEmailAndSendToBrokerOrGroup(string body);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "property name")]
		ZString GetEmailBody(Event eventDataObject)
		{
			var result = new ZStringBuilder();
			var suretyCodTable = new HtmlTableCreator();
			suretyCodTable.WriteRow("Event Time", eventDataObject.EventTime.GetValueOrDefault());
			var eventParameters = eventDataObject.EventParameters;
			if (eventParameters != null)
			{
				suretyCodTable.WriteRow("Reason", eventParameters.Reason.GetValueOrDefault());
			}
			result.Append(suretyCodTable.ToHtml());
			return result.ToStringWithNewLineBetweenAppends();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message Type")]
		public const string MessageType = "eBond Message to Surety Agent";
	}

	public static class SuretyToBrokerNoticeMessageProcessorHelper
	{
		public static bool IsSuretyToBrokerNoticeMessage(Event eventDataObject)
		{
			var result = false;
			if (eventDataObject != null)
			{
				var eventParameters = eventDataObject.EventParameters;
				result = eventDataObject.EventType.GetValueOrDefault() == Events.InterchangeRejectedCode && eventParameters != null && eventParameters.MessageType.GetValueOrDefault() == SuretyToBrokerNoticeMessageProcessor.MessageType;
			}
			return result;
		}
	}
}
