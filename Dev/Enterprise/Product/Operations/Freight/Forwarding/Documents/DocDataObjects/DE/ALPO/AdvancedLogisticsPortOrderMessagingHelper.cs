using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE
{
	class AdvancedLogisticsPortOrderMessagingHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Event type name")]
		public static bool MessageSendDisabled(ForwardingConsol consol)
		{
			var logs = GetAllLogs(consol);
			var lastLog = logs?.LastOrDefault();

			if (logs != null && lastLog != null && consol != null)
			{
				if (logs.Any(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode) &&
						consol.Numbers.Cast<CusEntryNumber>().Any(c => c.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber &&
																													 c.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Germany &&
																													 !string.IsNullOrEmpty(c.CE_EntryNum)))
				{
					return true;
				}

				var lastEvent = lastLog?.SL_SE_NKEvent;
				var lastEventType = string.Empty;
				if (lastLog?.Parameters != null)
				{
					lastLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out string type);
					lastEventType = string.IsNullOrEmpty(type) ? string.Empty : type;
				}
				if (!string.IsNullOrEmpty(lastEvent) &&
						(lastEvent.ToString() == Events.MessageSentCode ||
						lastEvent.ToString() == Events.MessageWithdrawCancelRequestCode ||
						lastEvent.ToString() == Events.InterchangeReceiptAcknowledgedCode ||
						lastEvent.ToString() == Events.InterchangeSentCode ||
						(lastEvent.ToString() == Events.StatusUpdatedCode && lastEventType != "Reset To Original")))
				{
					return true;
				}
			}

			return false;
		}

		public static bool MessageWithdrawDisabled(ForwardingConsol consol)
		{
			var szbNumber = consol.Numbers.Cast<CusEntryNumber>().Where(c => c.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber
																																		&& c.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Germany);
			if (!szbNumber.Any() || szbNumber.Any(c => string.IsNullOrEmpty(c.CE_EntryNum)))
			{
				return true;
			}

			return false;
		}

		public static bool ResetToOriginalDisabled(ForwardingConsol consol)
		{
			var logs = GetAllLogs(consol);
			var lastEvent = logs?.LastOrDefault()?.SL_SE_NKEvent;
			if (!string.IsNullOrEmpty(lastEvent) && (lastEvent.ToString() == Events.InterchangeReceiptAcknowledgedCode || lastEvent.ToString() == Events.InterchangeSentCode))
			{
				return false;
			}

			return true;
		}

		static List<StmALog> GetAllLogs(ForwardingConsol consol)
		{
			var logParent = (GetDocumentData(consol) as IStmALogParent);
			if (logParent == null)
			{
				return null;
			}

			var messageStatusEventCodes = new HashSet<string>(MessageEventCodes.MessageStatusEventCodes);

			return logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => !log.SL_IsCancelled
											&& messageStatusEventCodes.Contains(log.SL_SE_NKEvent)
											&& log.MatchesDocumentName(GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder))
				.OrderBy(log => log.SL_PostedTimeUtc)
				.ToList();
		}

		static IVisualizerDocumentData GetDocumentData(ForwardingConsol consol)
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			return documentDataLoader.Load(consol, ConsolDocumentDataStoreNames.DEAdvancedLogisticsPortOrder);
		}
	}
}
