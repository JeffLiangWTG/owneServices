using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class FIATAHouseBillMessagingExtension : BaseMessagingExtensions
	{
		public FIATAHouseBillMessagingExtension(ForwardingShipment shipment, IMessageInstructions messageInstructions)
		{
			this.shipment = shipment;
			this.messageInstructions = messageInstructions;
		}

		readonly ForwardingShipment shipment;
		readonly IMessageInstructions messageInstructions;

		public override string GetMessageStatus() => GetCurrentMessageStatusFromLogs();

		#region Implementation 

		IVisualizerDocumentData GetDocumentData()
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			return documentDataLoader.Load(shipment, ShipmentDocumentDataStoreNames.BillOfLading);
		}

		#endregion

		#region GetMessageStatus

		string GetCurrentMessageStatusFromLogs()
		{
			var logParent = GetDocumentData() as IStmALogParent;

			if (logParent?.Logs == null)
			{
				return string.Empty;
			}

			var messageEventLogs = logParent.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => IsValidMessageEvent(log));

			var orderedMessageEventLogs = (messageInstructions.OrderLogsByLocalTime ? messageEventLogs.OrderByDescending(log => log.SL_EventTime) : messageEventLogs.OrderByDescending(log => log.SL_PostedTimeUtc))
				.ThenByDescending(log => log.SL_SE_NKEvent);

			var lastLog = orderedMessageEventLogs.FirstOrDefault();

			return lastLog?.DisplayEventReference.ToString() ?? Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("e05490b4-f954-4d24-a135-74c9bf745e9b", "No {0} Messages Have Been Sent.", ShipmentDocumentNames.BillOfLading);
		}

		bool IsValidMessageEvent(StmALog log)
		{
			if (log == null)
			{
				return false;
			}

			var authorizationRelatedEventCodes = new List<string>() { Events.AuthorisedCode, Events.AuthorisationRejectedCode };
			var logEventCode = log.SL_SE_NKEvent.ToString();

			return (MessageEventCodes.MessageStatusEventCodes.Contains(logEventCode) && MatchesDocumentName(log, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType))
				|| (authorizationRelatedEventCodes.Contains(logEventCode) && MatchesDocumentName(log, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type) && MatchesDepartment(log));
		}

		bool MatchesDocumentName(StmALog log, string eventReferenceParameterCode)
		{
			return log.Parameters.TryGetValue(eventReferenceParameterCode, out var value)
				&& string.Equals(value, ShipmentDocumentNames.BillOfLading, StringComparison.OrdinalIgnoreCase);
		}

		bool MatchesDepartment(StmALog log)
		{
			return log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, out var department)
				&& string.Equals(department, DocDataConstants.Department.FIATA, StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}
