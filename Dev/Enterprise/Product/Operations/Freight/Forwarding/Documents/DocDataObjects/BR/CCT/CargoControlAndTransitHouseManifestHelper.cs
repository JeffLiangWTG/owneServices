using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	public static class CargoControlAndTransitHouseManifestHelper
	{
		#region Logs

		public static StmALog[] GetAllApplicableCctOrderedLogs(ForwardingShipment shipment)
		{
			var visualizerDocumentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();

			var documentData = visualizerDocumentDataLoader.Load(shipment, ShipmentDocumentDataStoreNames.AdvancedCargoReportBR);

			if (documentData is IStmALogParent logParent)
			{
				return logParent.Logs.GetAllLogs().OfType<StmALog>()
					.OrderByDescending(x => x.SL_PostedTimeUtc).Where(LogIsApplicable).ToArray();
			}

			return null;
		}

		static bool LogIsApplicable(StmALog log)
		{
			switch (log.SL_SE_NKEvent)
			{
				case Events.MessageSentCode:
				case Events.InterchangeSentCode:
				case Events.InterchangeRejectedCode:
				case Events.MessageRejectedCode:
				case Events.MessagePendingProcessingCode:
				case Events.MessageAcceptedCode:
				case Events.MessageWithdrawCancelRequestCode:
				case Events.MessageWithdrawCancelAcceptedCode:
					var location = log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Location) ?? string.Empty;

					if (location.StartsWith(Core.Constants.CountryCodes.Brazil))
					{
						var messageType = log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType);
						return string.Compare(messageType, DocumentNames.AdvancedCargoReport, StringComparison.OrdinalIgnoreCase) == 0;
					}

					return false;

				default:
					return false;
			}
		}

		#endregion

		public static bool ShouldExcludeFromShipments(ForwardingShipment shipment)
		{
			if (shipment.JS_ShipmentType == Constants.ShipmentTypes.AssemblyMaster)
			{
				var latestLog = CargoControlAndTransitHouseManifestHelper.GetAllApplicableCctOrderedLogs(shipment)?.FirstOrDefault();
				return latestLog == null
					|| latestLog.SL_SE_NKEvent == Events.MessageRejectedCode
					|| latestLog.SL_SE_NKEvent == Events.MessageWithdrawCancelAcceptedCode
					|| latestLog.SL_SE_NKEvent == Events.InterchangeRejectedCode;
			}

			if (shipment.JS_ShipmentType == Constants.ShipmentTypes.BlindCoLoadMaster)
			{
				return true;
			}

			if (shipment.CoLoadMasterShipment?.JS_ShipmentType.ToString() == Constants.ShipmentTypes.CoLoadMaster)
			{
				return true;
			}

			if (shipment.CoLoadMasterShipment is ForwardingShipment masterShipment && masterShipment.JS_ShipmentType == Constants.ShipmentTypes.AssemblyMaster)
			{
				var latestMasterShipmentLog = CargoControlAndTransitHouseManifestHelper.GetAllApplicableCctOrderedLogs(masterShipment)?.FirstOrDefault();
				return latestMasterShipmentLog != null
					&& latestMasterShipmentLog.SL_SE_NKEvent != Events.MessageRejectedCode
					&& latestMasterShipmentLog.SL_SE_NKEvent != Events.MessageWithdrawCancelAcceptedCode
					&& latestMasterShipmentLog.SL_SE_NKEvent != Events.InterchangeRejectedCode;
			}

			return false;
		}
	}
}
