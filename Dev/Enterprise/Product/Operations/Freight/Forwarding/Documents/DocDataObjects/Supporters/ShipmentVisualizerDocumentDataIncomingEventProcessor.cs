using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents
{
	public sealed class ShipmentVisualizerDocumentDataIncomingEventProcessor : IVisualizerDocumentDataIncomingEventProcessor
	{
		public void Process(IVisualizerDocumentData visualizerDocumentData, IStmALog log)
		{
			var shipment = visualizerDocumentData?.Parent as ForwardingShipment;
			if (shipment == null
				|| log == null
				|| log.SL_IsCancelled)
			{
				return;
			}

			if (log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out var eventMessageType))
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageAcceptedCode:
						{
							ProcessMessageAcceptedCode(shipment, log, eventMessageType);
							break;
						}
					case Events.InterchangeSentCode:
						{
							ProcessInterchangeSentCode(shipment, log, eventMessageType);
							break;
						}
				}
			}
		}

		void ProcessMessageAcceptedCode(ForwardingShipment shipment, IStmALog log, string eventMessageType)
		{
			if (eventMessageType == FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, out var customsReferenceNumberMAA))
			{
				var ecvEntryNumber = shipment.Numbers.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ExportConventional, Core.Constants.CountryCodes.France);
				if (ecvEntryNumber == null)
				{
					ecvEntryNumber = shipment.Numbers.AddNew();
					ecvEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
					ecvEntryNumber.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
				}
				ecvEntryNumber.CE_EntryNum = customsReferenceNumberMAA;

				shipment.OuterPackLines.Cast<PackLine>().Where(p => !p.JL_LastKnownTransitWarehouseStatus.IsEmpty).ForEach(p => p.JL_ExportRefNumber = customsReferenceNumberMAA);
			}
		}

		void ProcessInterchangeSentCode(ForwardingShipment shipment, IStmALog log, string eventMessageType)
		{
			if (eventMessageType == ShipmentDocumentNames.BookingRequest
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumberISN))
			{
				ShipmentAdditionalReferenceNumberHelper.PopulateCarrierMessageReferenceNumber(shipment, referenceNumberISN);
			}
		}
	}
}
