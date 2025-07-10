using System.Linq;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer
{
	class PortMessagingShipmentDataObjectWriter : ShipmentDataObjectWriter
	{
		public PortMessagingShipmentDataObjectWriter(IDataWritingManager manager, bool checkSubShipments, bool checkForParent, PortMessagingManager.MessageType messageType, string purpose)
			: base(manager, checkSubShipments, checkForParent)
		{
			Argument.NotNullOrEmpty(purpose, "purpose");

			this.purpose = purpose;
			this.messageType = messageType;
		}

		readonly string purpose;
		readonly PortMessagingManager.MessageType messageType;

		protected override void PopulateDataObject(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			base.PopulateDataObject(shipmentBO, shipmentData);

			if (PortMessagingManager.IsHDS(messageType))
			{
				var portMessaging = ShipmentPortMessaging.LoadOrCreate(shipmentBO);
				shipmentData.PortMessaging = new UniversalDataBuss.DataObjects.Universal.PortMessaging()
				{
					TypeOfDeclaration = ListHelper.GetWithDescription<CodeDescriptionPair>(portMessaging.JSM_EntryType, portMessaging.Lookups.EntryTypeList),
					MRN = portMessaging.JSM_MovementReferenceNumber,
					MRNComplete = portMessaging.JSM_MovementReferenceNumberComplete ? "Y" : "N",
					ExemptionReason = ListHelper.GetWithDescription<CodeDescriptionPair>(portMessaging.JSM_ExemptionReason, portMessaging.Lookups.ExemptionReasonList),
					ATB = portMessaging.JSM_ATBNumber,
					Annex30AType = ListHelper.GetWithDescription<CodeDescriptionPair>(portMessaging.JSM_Annex30AType, portMessaging.Lookups.Annex30ATypeList),
					Annex30AFailureProcess = portMessaging.JSM_Annex30AFailureProcess,
					ExportDeclarationNumber = portMessaging.JSM_ExportDeclarationReference,
					CustomsReleaseDate = portMessaging.JSM_CustomsReleaseDate,
					LRN = portMessaging.JSM_LocalReferenceNumber,
					LRNComplete = portMessaging.JSM_LocalReferenceNumberComplete ? "Y" : "N"
				};

				if (messageType == PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation)
				{
					shipmentData.PortMessaging.ForwardingCustomsOfficeCode = portMessaging.JSM_ForwardingCustomsOfficeCode;
				}

				var shipmentNotes = shipmentBO.Notes.FindByDescription(PredefinedNoteTypes.Instance.PortMessageRemarks.Description);
				if (shipmentNotes.Any())
				{
					shipmentData.PortMessaging.Remarks = shipmentNotes.First().ST_NoteDataAsText;
				}
			}

			AddCustomCountryCodeForNorthernIreland(shipmentBO, shipmentData);
		}

		void AddCustomCountryCodeForNorthernIreland(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			if (shipmentBO.Origin != null && shipmentBO.Origin.IsInNorthernIreland && (shipmentData.PortOfOrigin?.Code).HasValue)
			{
				shipmentData.PortOfOrigin.Code = Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes
					+ shipmentData.PortOfOrigin.Code.GetValueOrDefault().SubstringSafe(2);
			}

			if (shipmentBO.Destination != null && shipmentBO.Destination.IsInNorthernIreland && (shipmentData.PortOfDestination?.Code).HasValue)
			{
				shipmentData.PortOfDestination.Code = Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes
					+ shipmentData.PortOfDestination.Code.GetValueOrDefault().SubstringSafe(2);
			}
		}

		protected override ConsolDataObjectWriter GetConsolDataObjectWriter(DataWriterOptions dataWriterOptions)
		{
			return new PortMessagingConsolDataObjectWriter(writeManager, linkManager, dataWriterOptions, messageType, purpose);
		}

		protected override ForwardingPackingLineDataObjectWriter GetPackLineDataObjectWriter(IContainerLinkManager<ForwardingConsol> containerLinkManager, BindToLists listCache, ForwardingShipment sourceBO)
		{
			if (PortMessagingManager.IsHDS(messageType))
			{
				return new PortMessagingPackingLineDataObjectWriter(containerLinkManager, OrderLineLinkManager, PackLineLinkManager, listCache, writeManager);
			}

			return new ForwardingPackingLineDataObjectWriter(containerLinkManager, OrderLineLinkManager, PackLineLinkManager, listCache, writeManager, new LineRelatedDataWriterHelper(sourceBO));
		}

		protected override ForwardingConsol FindCurrentConsol(ForwardingShipment shipmentBO)
		{
			return new ShipmentPortMessagingManager(shipmentBO).GetCurrentConsol();
		}
	}
}
