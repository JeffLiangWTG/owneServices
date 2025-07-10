using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Freight.Forwarding.Documents
{
	public sealed class ConsolVisualizerDocumentDataIncomingEventProcessor : IVisualizerDocumentDataIncomingEventProcessor
	{
		public void Process(IVisualizerDocumentData visualizerDocumentData, IStmALog log)
		{
			var consol = visualizerDocumentData?.Parent as ForwardingConsol;
			if (consol == null
				|| log == null
				|| log.SL_IsCancelled)
			{
				return;
			}

			if (log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out var eventMessageType))
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageRejectedCode:
						{
							ProcessMessageRejectedCode(consol, log, eventMessageType);
							break;
						} 
					case Events.MessageAcceptedCode:
						{
							ProcessMessageAcceptedCode(consol, log, eventMessageType);
							break;
						}
					case Events.AuthorisedCode:
						{
							ProcessAuthorisedCode(consol, log, eventMessageType);
							break;
						}
					case Events.AuthorisationWithdrawnCode:
						{
							ProcessAuthorisationWithdrawnCode(consol, log, eventMessageType);
							break;
						}
					case Events.MessageWithdrawCancelAcceptedCode:
						{
							ProcessMessageWithdrawCancelAcceptedCode(consol, log, eventMessageType);
							break;
						}
					case Events.InterchangeSentCode:
						{
							ProcessInterchangeSentCode(consol, log, eventMessageType);
							break;
						}
				}
			}

			ProcessStatusUpdatedCode(consol, log);
		}

		void ProcessMessageRejectedCode(ForwardingConsol consol, IStmALog log, string eventMessageType)
		{
			if (eventMessageType == ConsolDocumentNames.BookingRequest
				|| eventMessageType == ConsolDocumentNames.ShippingOrder
				|| (eventMessageType == ConsolDocumentNames.ShippingInstruction && !consol.HasSentDocument(ConsolDocumentNames.BookingRequest) && !consol.HasSentDocument(ConsolDocumentNames.ShippingOrder)))
			{
				using (var mutex = new ZGlobalMutex(ZArchitecture.Modules.MutexIDs.CSRNumberAllocation, consol.PK.ToString()))
				{
					if (!mutex.IsLocked && mutex.Lock())
					{
						ConsolCarrierShipperReferenceNumberCalculator.PopulateShipperReferenceNumber(consol);
					}
				}
			}
		}

		void ProcessMessageAcceptedCode(ForwardingConsol consol, IStmALog log, string eventMessageType)
		{
			if ((eventMessageType == FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE
				|| eventMessageType == FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ
				|| eventMessageType == FrenchPortsConstants.DocumentNames.ProvisionalUnpackingListLPD)
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, out var customsReferenceNumberMAA)
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, out var equipmentReferenceNumberMAA))
			{
				switch (eventMessageType)
				{
					case FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE:
						PopulateContainerDepotCustomsReference(consol, equipmentReferenceNumberMAA, customsReferenceNumberMAA?.Split('/')[0], false);
						break;
					case FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ:
						PopulateContainerDepotCustomsReference(consol, equipmentReferenceNumberMAA, customsReferenceNumberMAA, false);
						break;
					case FrenchPortsConstants.DocumentNames.ProvisionalUnpackingListLPD:
						PopulateContainerDepotCustomsReference(consol, equipmentReferenceNumberMAA, customsReferenceNumberMAA, true);
						break;
				}
			}

			if (eventMessageType == GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, out var customsReferenceNumber))
			{
				var number = consol.Numbers.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Germany && x.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber)
				?? consol.Numbers.AddNew();

				number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				number.CE_EntryNum = customsReferenceNumber;
				number.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			}
		}

		void ProcessAuthorisedCode(ForwardingConsol consol, IStmALog log, string eventMessageType)
		{
			if (eventMessageType == Business.TMiningConstants.DocumentNames.TMiningSecureContainerRelease
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out var customsReferenceNumberATH)
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, out var equipmentReferenceNumberATH))
			{
				PopulateContainerDepotCustomsReference(consol, equipmentReferenceNumberATH, customsReferenceNumberATH, true);
			}
		}

		void ProcessAuthorisationWithdrawnCode(ForwardingConsol consol, IStmALog log, string eventMessageType)
		{
			if (eventMessageType == Business.TMiningConstants.DocumentNames.TMiningSecureContainerRelease
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, out var equipmentReferenceNumberATW))
			{
				PopulateContainerDepotCustomsReference(consol, equipmentReferenceNumberATW, string.Empty, true);
			}
		}

		void ProcessMessageWithdrawCancelAcceptedCode(ForwardingConsol consol, IStmALog log, string eventMessageType)
		{
			if (eventMessageType == GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder)
			{
				var number = consol.Numbers.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Germany && x.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber);
				if (number != null)
				{
					consol.Numbers.RemoveAndDelete(number);
				}
			}
		}

		void ProcessInterchangeSentCode(ForwardingConsol consol, IStmALog log, string eventMessageType)
		{
			if (log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out var customsReferenceNumberISN))
			{
				if (eventMessageType == ConsolDocumentNames.BookingRequest
					|| eventMessageType == ConsolDocumentNames.ShippingInstruction
					|| eventMessageType == ConsolDocumentNames.ShippingOrder)
				{
					ConsolAdditionalReferenceNumberHelper.PopulateCarrierMessageReferenceNumber(consol, customsReferenceNumberISN);
				}
				else if (eventMessageType == ConsolDocumentNames.VerifiedGrossContainerWeight
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, out var equipmentReferenceNumberISN))
				{
					var container = consol.Containers.FindAnyByContainerNumber(equipmentReferenceNumberISN);
					if (container != null)
					{
						ContainerAdditionalReferenceNumberHelper.PopulateCarrierMessageReferenceNumber(container, customsReferenceNumberISN);
					}
				}
			}
		}

		void ProcessStatusUpdatedCode(ForwardingConsol consol, IStmALog log)
		{
			if (log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, out var customsReferenceNumberSTU)
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, out var equipmentReferenceNumberSTU)
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType))
			{
				switch (eventType)
				{
					case Core.Constants.EventReferenceParameterTypes.LDENotification:
						PopulateContainerDepotCustomsReference(consol, equipmentReferenceNumberSTU, customsReferenceNumberSTU, false);
						break;
					case Core.Constants.EventReferenceParameterTypes.LPDNotification:
						PopulateContainerDepotCustomsReference(consol, equipmentReferenceNumberSTU, customsReferenceNumberSTU, true);
						break;
				}
			}
		}

		void PopulateContainerDepotCustomsReference(ForwardingConsol consol, string equipmentReferenceNumber, string customsReferenceNumber, bool isImport)
		{
			var container = consol.Containers.FindAnyByContainerNumber(equipmentReferenceNumber);
			if (container != null)
			{
				if (isImport)
				{
					container.JC_ImportDepotCustomsReference = customsReferenceNumber;
				}
				else
				{
					container.JC_ExportDepotCustomsReference = customsReferenceNumber;
				}
			}
		}
	}
}
