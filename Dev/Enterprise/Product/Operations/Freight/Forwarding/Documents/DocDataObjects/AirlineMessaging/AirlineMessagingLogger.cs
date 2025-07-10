using System.Collections.Generic;
using System.Linq;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.InterchangeBuilder;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging
{
	public class AirlineMessagingLogger
	{
		public void LogRequest(ForwardingConsol consol, string content)
		{
			var logDataExport = consol.Logs.AddNew(AutoEvents.DataExport, CreateEventArgs(consol.JK_MasterBillNum, string.Empty).ToArray());
			CreateInterchangeAndMessage(logDataExport, MessageDirection.Transmit, content);

			var logMessageSent = consol.Logs.AddNew(AutoEvents.MessageSent, CreateEventArgs(consol.JK_MasterBillNum, string.Empty).ToArray());
			CreateInterchangeAndMessage(logMessageSent, MessageDirection.Transmit, content);
		}

		public void LogResponse(ForwardingConsol consol, IDictionary<int, string> originalXmlMessages, IDictionary<int, UniversalEvent> parsedEvents)
		{
			if (parsedEvents == null)
			{
				return;
			}
			foreach (var keyValuePair in parsedEvents)
			{
				var parsedEvent = keyValuePair.Value;
				var eventId = keyValuePair.Key;
				var logMessageReceived = consol.Logs.AddNew(parsedEvent.EventType.Value == AutoEvents.InterchangeSentCode ? AutoEvents.InterchangeSent : AutoEvents.InterchangeRejected, CreateEventArgs(consol.JK_MasterBillNum, parsedEvent.EventParameters.Reason).ToArray());
				CreateInterchangeAndMessage(logMessageReceived, MessageDirection.Receive, originalXmlMessages[eventId], parsedEvent);
			}
		}

		public void LogFailedMessageValidation(ForwardingConsol consol, string validationFailureReason)
		{
			consol.Logs.AddNew(AutoEvents.MessageValidationFailed, CreateEventArgs(consol.JK_MasterBillNum, validationFailureReason).ToArray());
		}

		string GetFwbFhlMessageType(string messageContent)
		{
			if (string.IsNullOrEmpty(messageContent))
			{
				return null;
			}

			return messageContent.Contains("FWB/")
				? EDIMessageTypeList.Codes.FWB
				: EDIMessageTypeList.Codes.FHL;
		}

		void CreateInterchangeAndMessage(StmALog log, MessageDirection messageDirection, string messageContents, UniversalEvent parsedEvent = null)
		{
			var factory = log.Factory;
			var interchange = factory.New<IXmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;

			if (messageDirection == MessageDirection.Receive)
			{
				interchange.EI_From = Constants.MessageTarget;
				interchange.EI_To = Env.CurrentCompany.GetLicenceCode();
				interchange.EI_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
			}
			else
			{
				interchange.EI_From = Env.CurrentCompany.GetLicenceCode();
				interchange.EI_To = Constants.MessageTarget;
				interchange.EI_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			}

			interchange.EI_InterchangeType = EDIMessageTypeList.Codes.XMS;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;

			var message = interchange.AddNeweHubMessage();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageText = messageContents;

			if (messageDirection == MessageDirection.Receive)
			{
				if (parsedEvent != null)
				{
					message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
					message.EM_Status = parsedEvent.EventType.Value == AutoEvents.InterchangeSentCode ? EDIMessageStatusList.Codes.ProcessedOK : EDIMessageStatusList.Codes.Error;
					var messageType = GetFwbFhlMessageType(parsedEvent.ContextCollection.FirstOrDefault(pe => pe.Type == "OriginalFWBFHLMessage")?.Value);
					if (messageType == EDIMessageTypeList.Codes.FHL || messageType == EDIMessageTypeList.Codes.FWB)
					{
						var mawbInfo = parsedEvent.ContextCollection.First(pe => pe.Type == "MAWBNumber").Value;
						message.EM_MessageNum = mawbInfo.Value;
						message.EM_MessageType = messageType;
					}
				}
			}
			else
			{
				message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				message.EM_Status = EDIMessageStatusList.Codes.Sent;
			}

			var messageLogPivot = factory.New<IGenPivot>();
			messageLogPivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
			messageLogPivot.XX_Relation1ID = log.PK;
			messageLogPivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
			messageLogPivot.XX_Relation2ID = message.PK;
			messageLogPivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
		}

		List<KeyValuePair<string, string>> CreateEventArgs(string refNumber, string reason)
		{
			var args = new List<KeyValuePair<string, string>>
			{
				new(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.MessageTarget),
				new(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, Constants.MessageDepartment),
			};

			if (!string.IsNullOrWhiteSpace(refNumber))
			{
				args.Add(new(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, refNumber));
			}

			if (!string.IsNullOrWhiteSpace(reason))
			{
				args.Add(new(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason));
			}
			return args;
		}
	}
}
