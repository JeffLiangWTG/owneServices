using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using UEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class AirBookingResponseProcessor
	{
		public AirBookingResponseProcessor(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		public ResponseProcessResult Process(string responseContent, AirBookingRequestProcessor requestProcessor = null)
		{
			if (string.IsNullOrEmpty(responseContent))
			{
				return new ResponseProcessResult(ResponseType.Empty);
			}

			if (!TryParseResponseContent(responseContent, out var interchange))
			{
				var userMessage = Res.GetString("f5de1d37-5ee3-4efc-9446-cc6435e7bc48", "Message received from ABE could not be processed. The content of the message can be found in the EDI Interchange module.");
				return new ResponseProcessResult(ResponseType.Invalid, userMessage, false);
			}

			var messages = GetUXmlMessagesInLocalFactory(interchange);
			return ProcessAllMessages(messages, requestProcessor);
		}

		#region Parse ResponseContent

		bool TryParseResponseContent(string responseContent, out IXmlEDIInterchange interchange)
		{
			var interchangeFactory = factory.CreateNewFactory();
			var interchangeBuilder = new InterchangeBuilder(interchangeFactory);

			var isParseSuccessfully = interchangeBuilder.TryParseUniversalXml(responseContent, InterchangeBuilder.MessageDirection.Receive, out interchange);

			if (!isParseSuccessfully)
			{
				interchange = interchange ?? interchangeBuilder.CreateInterchange(responseContent, InterchangeBuilder.MessageDirection.Receive);
				interchange.EI_Status = EDIInterchangeStatusList.Codes.SyntaxRejected;
			}

			interchangeFactory.Save();

			return isParseSuccessfully;
		}

		#endregion

		#region Get Messages

		IEDIMessage[] GetUXmlMessagesInLocalFactory(IXmlEDIInterchange interchange)
		{
			var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);

			return factory.Load<IEDIMessage>(messageQuery)
				.OrderBy(m => m.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment ? -1 : 0)
				.ToArray();
		}

		#endregion

		#region Process Messages

		ResponseProcessResult ProcessAllMessages(IEDIMessage[] messages, AirBookingRequestProcessor requestProcessor)
		{
			var logger = new Logger();
			var universalFactory = new UniversalObjectFactory(factory);

			var eventResponseType = ResponseType.Empty;
			string eventMessage = null;

			var legs = new Dictionary<string, TransportLeg>();
			var eventXmls = new List<UEvent>();

			var importedUniversalShipment = false;
			string universalShipmentProcessResult = null;

			foreach (var message in messages)
			{
				IXmlSessionTracker tracker = new XmlSessionTracker(logger);

				if (message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment)
				{
					var usxml = message.GetEM_MessageTextReader().Parse<UShipment>();

					var isRatesMessage = usxml?.SubShipmentCollection?.Count > 0;

					if (isRatesMessage)
					{
						return ResponseProcessResult.ResponseWithRates(usxml);
					}

					universalShipmentProcessResult = ProcessUniversalShipment(usxml, message, tracker, universalFactory);
					usxml.TransportLegCollection.ForEach(leg => legs[leg.VoyageFlightNo] = leg);

					importedUniversalShipment = true;
				}
				else if (message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent)
				{
					message.ProcessUniversalMessage(universalFactory, logger);

					var uexml = message.GetEM_MessageTextReader().Parse<UEvent>();

					if (eventResponseType == ResponseType.Empty)
					{
						eventResponseType = GetResponseTypeFromEvent(uexml);
					}

					if (eventResponseType == ResponseType.InterchangeRejected && requestProcessor?.Shipment != null)
					{
						var notificationMessageID = GetNotificationMessageID(uexml);
						if (!notificationMessageID.IsEmpty)
						{
							if (requestProcessor.Shipment.AddInfoCollection == null)
							{
								requestProcessor.Shipment.SetAddInfoCollection(() => new List<AddInfo> { AddInfo.New("UserAction", notificationMessageID) });
							}
							else
							{
								requestProcessor.Shipment.AddInfoCollection.Add(new AddInfo { Key = "UserAction", Value = notificationMessageID });
							}
							return AirBookingCommand.ProcessRequestAndResponse(requestProcessor, factory);
						}
					}

					eventMessage = eventMessage ?? GetResponseMessageFromEvent(uexml);
					eventXmls.Add(uexml);
				}
				else
				{
					continue;
				}

				var error = tracker
					.Logs
					.FirstOrDefault(log => log.Type == LogType.Error);

				if (error != null)
				{
					return new ResponseProcessResult(ResponseType.Invalid, error.Message);
				}

				logger.Clear();
			}

			if (eventResponseType == ResponseType.Empty)
			{
				return new ResponseProcessResult(ResponseType.Empty);
			}

			eventMessage = UpdateEventMessage(eventXmls, legs) ?? eventMessage;
			var userMessage = string.Concat(eventMessage, "\r\n\r\n", universalShipmentProcessResult).Trim();

			return new ResponseProcessResult(eventResponseType, userMessage, importedUniversalShipment);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Notification type value")]
		static ZString GetNotificationMessageID(UEvent uexml)
		{
			var contextNotificationTypeValue = GetContextValueByType(uexml, UEvent.ContextTypes.NotificationType);
			if (contextNotificationTypeValue == "Warning")
			{
				var contextNotificationTextValue = GetContextValueByType(uexml, UEvent.ContextTypes.NotificationText);
				if (!contextNotificationTextValue.IsEmpty)
				{
					var result = Globals.Message.Show(contextNotificationTextValue, Res.GetString("d045e2f2-ca8d-4d3a-ad61-fbc895565132", "Warning"), ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Warning, ZDialogResult.Cancel);
					if (result == ZDialogResult.OK)
					{
						return GetContextValueByType(uexml, UEvent.ContextTypes.NotificationMessageID);
					}
				}
			}
			return ZString.Empty;
		}

		static ZString GetContextValueByType(UEvent uexml, UEvent.ContextTypes type)
		{
			return uexml.ContextCollection.FirstOrDefault(context => string.Equals(context.Type, type.ToString(), StringComparison.OrdinalIgnoreCase))?.Value ?? ZString.Empty;
		}

		string ProcessUniversalShipment(UShipment usxml, IEDIMessage message, IXmlImportLogger logger, UniversalObjectFactory universalFactory)
		{
			var reader = new AirBookingResponseDataObjectReader(usxml, logger, universalFactory);
			reader.ReadIntoBusinessObject();

			if (reader.Consol is IStmALogParent parent)
			{
				var dataImportLog = parent.Logs.AddNew(Events.DataImport);
				message.AddUniversalDataLink(dataImportLog);
			}

			if (message is IStmNoteParent noteParent)
			{
				var logMessage = string.Join("\r\n", logger.Logs);
				noteParent.Notes.AddNew(false, PredefinedNoteTypes.Instance.DataImportLogNote.Description, logMessage);
			}

			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

			return string.Join("\r\n", reader.UserMessages);
		}

		#endregion

		#region Response Details From UniversalEvent

		ResponseType GetResponseTypeFromEvent(UEvent uexml)
		{
			switch (uexml.EventType.Value)
			{
				case Events.InterchangeSentCode:
					return ResponseType.InterchangeSent;

				case Events.InterchangeReceiptAcknowledgedCode:
					return ResponseType.InterchangeAccepted;

				case Events.InterchangeRejectedCode:
					return ResponseType.InterchangeRejected;

				case Events.BookingConfirmedCode:
					return ResponseType.BookingConfirmed;

				case Events.BookingPendingCode:
					return ResponseType.BookingPending;

				case Events.BookingRejectedCode:
					return ResponseType.BookingRejected;

				case Events.BookingCancelledCode:
					return ResponseType.BookingCancelled;

				default:
					return ResponseType.Empty;
			}
		}

		string GetResponseMessageFromEvent(UEvent uexml)
		{
			switch (uexml.EventType.Value)
			{
				case Events.InterchangeSentCode:
				case Events.InterchangeReceiptAcknowledgedCode:
					return Res.GetString("2f335298-b713-480f-acad-cf2f9ab9f12f", "Booking request has been sent to the Airline. The Consol will be updated automatically when a pending response is received from the Airline. In the meantime, please check the Events Log for more information.");

				case Events.InterchangeRejectedCode:
					var contextFailureReason = uexml.ContextCollection.FirstOrDefault(context => string.Equals(context.Type, nameof(UEvent.ContextTypes.FailureReason), StringComparison.OrdinalIgnoreCase));
					var reason = string.IsNullOrEmpty(contextFailureReason?.Value.GetValueOrDefault()) ? uexml.EventParameters?.Reason : contextFailureReason.Value;
					return reason ?? Res.GetString("ff22d095-47c9-40ba-b02d-fe16668c927f", "Booking request has been rejected. Please check the Events Log for more information.");

				default:
					return null;
			}
		}

		string GetResponseStatusFromEvent(UEvent uexml)
		{
			switch (uexml.EventType.Value)
			{
				case Events.BookingConfirmedCode:
					return Res.GetString("8e9d84c9-0354-452f-98f4-4628bfa98e43", "Confirmed");

				case Events.BookingPendingCode:
					return Res.GetString("0bdbba11-fb20-4ad3-b089-d6fac05ab8cf", "Queued");

				default:
					return string.Empty;
			}
		}

		#endregion

		#region User Message

		string UpdateEventMessage(List<UEvent> eventXmls, Dictionary<string, TransportLeg> legs)
		{
			if (legs.Count > 0 && eventXmls.Count > 0)
			{
				if (eventXmls.Any(eventXml => eventXml.EventType.HasValue && eventXml.EventType.Value.EqualsIgnoringCase(Events.BookingRejectedCode)))
				{
					return Res.GetString("a4bfd9e5-8214-4419-bd78-37ebf5786274", "Booking request has been rejected. Please check the Events Log for more information.");
				}
				else if (eventXmls.Any(eventXml => eventXml.EventType.HasValue && eventXml.EventType.Value.EqualsIgnoringCase(Events.BookingCancelledCode)))
				{
					return Res.GetString("29bc5148-ced9-40e0-bec6-bfaa0239026c", "Booking has been canceled with the Airline. Please check the Events Log for more information.");
				}
				else
				{
					return CreateAirlineBookingResponseMessage(eventXmls, legs);
				}
			}

			return null;
		}

		string CreateAirlineBookingResponseMessage(List<UEvent> eventXmls, Dictionary<string, TransportLeg> legs)
		{
			var result = new StringBuilder();
			result.Append(Res.GetString("bc7e2489-abb8-4e98-bea4-878211229675", "Airline Booking Response:"));
			result.Append("\r\n");

			foreach (var eventXml in eventXmls)
			{
				result.Append(CreateLegStatusMessage(eventXml, legs));
			}

			result.Append("\r\n");

			if (eventXmls.All(eventXml => eventXml.EventType.Equals(Events.BookingConfirmedCode)))
			{
				result.Append(Res.GetString("c7be4a3e-6476-4eef-b252-7d7d480a0303",
					"The Consol will be updated automatically if any additional response(s) is received from the Airline at a later time. In the meantime, please check the Events Log for more information."));
			}
			else
			{
				result.Append(Res.GetString("f3f5a9a9-f011-4285-a549-33427d0fa447",
					@"The Consol will be updated automatically when a pending response is received from the Airline. In the meantime, please check the Events Log for more information."));
			}

			return result.ToString();
		}

		string CreateLegStatusMessage(UEvent eventXml, Dictionary<string, TransportLeg> legs)
		{
			var result = string.Empty;
			if (legs.TryGetValue(eventXml?.EventParameters?.VoyageFlightNumber, out var leg))
			{
				if (leg != null)
				{
					result = $"{leg.PortOfLoading?.Code.Value}-{leg.PortOfDischarge?.Code.Value} "; // programmatic constant
				}
			}

			result += $"{eventXml.EventParameters?.VoyageFlightNumber}/{eventXml.EventParameters?.FlightDate.Value.ToString("ddMMM", new System.Globalization.DateTimeFormatInfo())} - {GetResponseStatusFromEvent(eventXml)}\r\n"; // programmatic constant
			return result;
		}

		#endregion

		#region Nested Types

		sealed class Logger : ISimpleLogger
		{
			public IEnumerable<ISimpleLog> Logs => logs;

			readonly List<ISimpleLog> logs = new List<ISimpleLog>();

			public void Clear() => logs.Clear();

			public void Log(LogType type, string message)
			{
				if (!string.IsNullOrWhiteSpace(message))
				{
					logs.Add(new SimpleLog(type, message));
				}
			}
		}

		#endregion
	}
}
