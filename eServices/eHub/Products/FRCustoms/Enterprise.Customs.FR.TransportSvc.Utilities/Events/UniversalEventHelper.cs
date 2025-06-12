using System;
using System.IO;
using Enterprise.Customs.FR.TransportSvc.Utilities;

namespace Enterprise.Customs.FR.TransportSvc.Messages
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypesAnalyzer")]
	public static class UniversalEventHelper
	{
		public static string GetUniversalMessageContent(string assemblyPath, string targetType, string eventType, string eventStatus, string eventAdditionalInfo, string eventDate, string recipientID, string senderID, string declarationReference, string interchangeNumber, string transactionID)
		{
			if (string.IsNullOrEmpty(targetType))
			{
				throw new Exception($"{eventStatus} notification not sent to CW1. The event would not stick to any business object in CW1");
			}

			var xmlUniversalEventMessage = File.ReadAllText(Path.Combine(assemblyPath, "Message", "Templates", "UniversalEvent.xml"));
			xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%1", senderID);
			xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%2", recipientID);
			xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%3", targetType);
			xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%4", declarationReference);
			if (eventStatus == Constants.UniversalEvent.Statuses.MessageRejectedByCustoms || eventStatus == Constants.UniversalEvent.Statuses.MessageNotDelivered || eventStatus == Constants.UniversalEvent.Statuses.MessageRejectedByMareva)
			{
				xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%5", $@"
          <Context>
            <Type>Error</Type>
            <Value>{eventAdditionalInfo}</Value>
          </Context>");
			}
			else
			{
				xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%5", string.Empty);
			}

			if (!string.IsNullOrEmpty(interchangeNumber))
			{
				xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%6", $@"
          <Context>
            <Type>InterchangeNumber</Type>
            <Value>{interchangeNumber}</Value>
          </Context>");
			}
			else
			{
				xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%6", string.Empty);
			}

			if (!string.IsNullOrEmpty(transactionID))
			{
				xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%7", $@"
          <Context>
            <Type>CorrelationID</Type>
            <Value>{transactionID}</Value>
          </Context>");
			}
			else
			{
				xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%7", string.Empty);
			}

			xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%8", eventDate);
			xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%9", eventType);
			xmlUniversalEventMessage = xmlUniversalEventMessage.Replace("%0", eventStatus);
			return xmlUniversalEventMessage;
		}

		public static string GetUniversalEventFilePath(string workingDirectory, string eventType, string trackingId) => Path.Combine(workingDirectory, trackingId) + "_" + eventType + Constants.Extensions.Xml;

		public static string GetTargetType(string schemaIdOrApplication)
		{
			var targetType = string.Empty;
			switch (schemaIdOrApplication)
			{
				case Constants.MessageSchemas.DeltaCImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaCExportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDExportDeclarationSchema:
				case Constants.MessageSchemas.DcgSchema:
				case Constants.ApplicationTypes.DeltaCG:
				case Constants.ApplicationTypes.DeltaDG:
				case Constants.ApplicationTypes.ECS:
				case Constants.ApplicationTypes.CIN:
				case Constants.MessageSchemas.CIN745Schema:
				case Constants.MessageSchemas.CIN750Schema:
				case Constants.MessageSchemas.CIN755Schema:
				case Constants.MessageSchemas.IE507Schema:
				case Constants.MessageSchemas.IE618Schema:
					targetType = Constants.UniversalEvent.TargetTypes.CustomsDeclaration;
					break;
				case Constants.MessageSchemas.IE007Schema:
				case Constants.MessageSchemas.IE013Schema:
				case Constants.MessageSchemas.IE014Schema:
				case Constants.MessageSchemas.IE015Schema:
				case Constants.MessageSchemas.IEF15Schema:
				case Constants.MessageSchemas.IE044Schema:
				case Constants.MessageSchemas.IE141Schema:
				case Constants.MessageSchemas.CC007CSchema:
				case Constants.MessageSchemas.CC013CSchema:
				case Constants.MessageSchemas.CC014CSchema:
				case Constants.MessageSchemas.CC034CSchema:
				case Constants.MessageSchemas.CC044CSchema:
				case Constants.MessageSchemas.CC141CSchema:
				case Constants.MessageSchemas.CC170CSchema:
				case Constants.ApplicationTypes.DeltaT:
					targetType = Constants.UniversalEvent.TargetTypes.NctsHeader;
					break;
				case Constants.MessageSchemas.APPLUSSchema:
					targetType = Constants.UniversalEvent.TargetTypes.NctsHeader;
					break;
				default:
					break;
			}

			return targetType;
		}

		public static string GetEventType(string schemaIdOrApplication)
		{
			var targetType = string.Empty;
			switch (schemaIdOrApplication)
			{
				case Constants.MessageSchemas.DeltaCImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaCExportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDExportDeclarationSchema:
				case Constants.MessageSchemas.DcgSchema:
				case Constants.ApplicationTypes.DeltaCG:
				case Constants.ApplicationTypes.DeltaDG:
				case Constants.ApplicationTypes.ECS:
				case Constants.ApplicationTypes.CIN:
				case Constants.MessageSchemas.CIN745Schema:
				case Constants.MessageSchemas.CIN750Schema:
				case Constants.MessageSchemas.CIN755Schema:
				case Constants.MessageSchemas.IE507Schema:
				case Constants.MessageSchemas.IE618Schema:
				case Constants.MessageSchemas.IE007Schema:
				case Constants.MessageSchemas.IE013Schema:
				case Constants.MessageSchemas.IE014Schema:
				case Constants.MessageSchemas.IE015Schema:
				case Constants.MessageSchemas.IEF15Schema:
				case Constants.MessageSchemas.IE044Schema:
				case Constants.MessageSchemas.IE141Schema:
				case Constants.ApplicationTypes.DeltaT:
					targetType = Constants.UniversalEvent.EventTypes.FrenchCustomsMessages;
					break;
				case Constants.MessageSchemas.APPLUSSchema:
					targetType = Constants.UniversalEvent.EventTypes.APPLUS;
					break;
				default:
					break;
			}

			return targetType;
		}
	}
}

