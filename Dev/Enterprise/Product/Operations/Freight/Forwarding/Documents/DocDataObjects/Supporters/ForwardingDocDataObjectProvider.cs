using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ForwardingDocDataObjectProvider : IForwardingDocDataObjectProvider
	{
		public object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.English))
			{
				if (parent is ForwardingShipment shipment)
				{
					return GetFromShipment(shipment, dataContext, parameters);
				}

				if (parent is ForwardingConsol consol)
				{
					return GetFromConsol(consol, dataContext, parameters);
				}

				if (parent is QuotedBooking quotedBooking)
				{
					return GetFromQuotedBooking(quotedBooking, dataContext, parameters);
				}

				if (parent is WhsItemReceiveConsignment receiveConsignment)
				{
					return GetFromTransitReceive(receiveConsignment, dataContext, parameters);
				}

				if (parent is WhsItemDispatchConsignment dispatchConsignment)
				{
					return GetFromTransitDispatch(dispatchConsignment, dataContext, parameters);
				}
			}

			return null;
		}

		object GetFromShipment(ForwardingShipment shipment, string dataContext, IDocDataObjectParameters parameters)
		{
			shipment.OnShowMessageOnGUI += (o, e) => Globals.Message.ShowInformation(e.Message, e.Title);
			switch (dataContext)
			{
				case DataContext.HouseBill:
					if (parameters.DataStoreName == ShipmentDocumentDataStoreNames.CarrierBillOfLading)
					{
						return new CarrierHouseBillBuilder(shipment, parameters).Build();
					}

					return new HouseBillBuilder(shipment, parameters).Build();

				case DataContext.ShippersDangerousGoodsDeclaration:
					return new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();

				case DataContext.EManifest:
					return new CN.EManifestBuilder(shipment, parameters).Build();

				case DataContext.AirCargoAdvanceScreening:
					return new US.AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

				case DataContext.CargoControlAndTransit:
					return new BR.CargoControlAndTransitBuilder(shipment).Build();

				case DataContext.FRPortsCustomsCheckCAED:
					return new FR.CustomsDeclarationCheckBuilder(shipment, parameters).Build();

				case DataContext.FRPortsGoodsReceivedCRESA:
					return new FR.CresaBuilder(shipment).Build();

				case DataContext.FRPortsIntegrationDossier:
					return new FR.ShipmentDossierBuilder(shipment, parameters).Build();

				case DataContext.CertificateOfOriginChAFTA:
					return new ChaftaBuilder(shipment).Build();
				case DataContext.BookingRequest:
					return new SeaShipmentBookingRequestBuilder(shipment, parameters).Build();

				case DataContext.MXHouseAirwayBill:
					return new MX.HouseAirwayBillBuilder(shipment).Build();

				case DataContext.CartaDePorte:
					return new CPT.CartaDePorteBuilder(shipment, parameters).Build();

				case DataContext.ConsolidationAdvice:
					return new ConsolidationAdviceBuilder(shipment).Build();

				case DataContext.CertificateOfOriginAANZFTA:
					return new AanzftaBuilder(shipment).Build();

				case DataContext.CertificateOfOriginJAEPA:
					return new JAEPABuilder(shipment).Build();

				case DataContext.CertificateOfOriginRCEP:
					return new RCEPBuilder(shipment).Build();

				case DataContext.ForwardersCargoReceiptSummary:
					return new ForwardersCargoReceiptSummaryBuilder(shipment, parameters).Build();

				case DataContext.ForwardersCargoReceiptDetail:
					return new ForwardersCargoReceiptDetailBuilder(shipment, parameters).Build();

				case DataContext.CertificateOfOriginKAFTA:
					return new KAFTABuilder(shipment).Build();

				case DataContext.CertificateOfOriginCPTPP:
					return new CPTPPBuilder(shipment).Build();

				case DataContext.CertificateOfOriginIACEPA:
					return new IACEPABuilder(shipment).Build();

				case DataContext.CertificateOfOriginNZCFTA:
					return new NZCFTABuilder(shipment).Build();

				case DataContext.CertificateOfOriginCOOUS:
					return new COOUSBuilder(shipment).Build();

				case DataContext.CertificateOfOriginNZ:
					return new COONZBuilder(shipment).Build();

				case DataContext.ShippersSecurityEndorsement:
					return new US.ShippersSecurityEndorsementBuilder(shipment).Build();

				case DataContext.CINExportNotification:
					return new FR.CINExportNotificationBuilder(shipment).Build();

				case DataContext.ILDeliveryOrder:
					return new IL.DeliveryOrderBuilder(shipment).Build();

				case DataContext.ILGatePassMovement:
					return new IL.GatePassMovementBuilder(shipment.GatePassMovementProvider).Build();

				case DataContext.CertificateOfOriginTAFTA:
					return new TAFTABuilder(shipment).Build();

				case DataContext.CertificateOfOriginPAFTA:
					return new PAFTABuilder(shipment).Build();

				case DataContext.CertificateOfOriginAUKFTA:
					return new AUKFTABuilder(shipment).Build();

				case DataContext.CertificateOfOriginIAECTA:
					return new IAECTABuilder(shipment).Build();

				case DataContext.CertificateOfOriginAUCONP:
					return new AUCONPBuilder(shipment).Build();

				case DataContext.CMRConsignmentNote:
					return new ShipmentCMRConsignmentNoteBuilder(shipment).Build();

				case DocumentVisualizer.Integration.DataContext.USATF6A:
					if (shipment.DeclarationForDocuments != null)
					{
						var builder = ObjectFactory.Get<Enterprise.Integration.Customs.US.IATF6ADataBuilder>();
						return builder.Build(shipment.DeclarationForDocuments, parameters);
					}
					return null;

				case DataContext.DA306Document:
					if (shipment.DeclarationForDocuments != null)
					{
						return ObjectFactory.Get<object>("ZA.DA306DocumentWrapper", shipment.DeclarationForDocuments);
					}
					return null;
			}

			return null;
		}

		object GetFromConsol(ForwardingConsol consol, string dataContext, IDocDataObjectParameters parameters)
		{
			if (ConsolDocDataObjectMapping.ContainsKey(dataContext))
			{
				return ConsolDocDataObjectMapping[dataContext](consol, parameters);
			}

			return null;
		}

		object GetFromTransitReceive(WhsItemReceiveConsignment consignment, string dataContext, IDocDataObjectParameters parameters)
		{
			switch (dataContext)
			{
				case DataContext.FRPortsGoodsReceivedCRESA:
					return new FR.CresaBuilderForTransitReceiveConsignment(consignment).Build();
			}

			return null;
		}

		object GetFromTransitDispatch(WhsItemDispatchConsignment consignment, string dataContext, IDocDataObjectParameters parameters)
		{
			switch (dataContext)
			{
				case DataContext.FRPortsGoodsReceivedCRESA:
					return new FR.CresaBuilderForTransitDispatchConsignment(consignment).Build();
			}

			return null;
		}

		IReadOnlyDictionary<string, Func<ForwardingConsol, IDocDataObjectParameters, object>> consolDocDataObjectMapping;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		IReadOnlyDictionary<string, Func<ForwardingConsol, IDocDataObjectParameters, object>> ConsolDocDataObjectMapping
		{
			get
			{
				return consolDocDataObjectMapping
					?? (consolDocDataObjectMapping = new Dictionary<string, Func<ForwardingConsol, IDocDataObjectParameters, object>>
					{
						[DataContext.ShippingInstruction] = (consol, parameters) => new ShippingInstructionBuilder(consol).Build(),
						[DataContext.BookingRequest] = (consol, parameters) => new BookingRequestBuilder(consol, parameters).Build(),
						[DataContext.BookingConfirmation] = (consol, parameters) => new BookingConfirmationBuilder(consol, parameters).Build(),
						[DataContext.ShippingOrder] = (consol, parameters) => new CN.ShippingOrderBuilder(consol).Build(),
						[DataContext.ContainerLoadPlan] = (consol, parameters) => new CN.ContainerLoadPlanBuilder(consol, parameters).Build(),
						[DataContext.VerifiedGrossMass] = (consol, parameters) => new VGMBuilder(consol, parameters).Build(),
						[DataContext.ETerminalReleaseManifest] = (consol, parameters) => new CN.ETerminalReleaseManifestBuilder(consol).Build(),
						[DataContext.ACASHouseChecklist] = (consol, parameters) => new US.ACASHouseChecklistBuilder(consol).Build(),
						[DataContext.CargoControlAndTransitHouseManifest] = (consol, parameters) => new BR.CargoControlAndTransitHouseManifestBuilder(consol).Build(),
						[DataContext.NLPortbaseImportNotification] = (consol, parameters) => new NL.PortbaseImportNotificationBuilder(consol).Build(),
						[DataContext.NLPortbaseExportNotification] = (consol, parameters) => new NL.PortbaseExportNotificationBuilder(consol).Build(),
						[DataContext.CargoDues] = (consol, parameters) => new CargoDuesBuilder(consol, parameters).Build(),
						[DataContext.AirBookingRequest] = (consol, parameters) => new AirBookingRequestBuilder(consol).Build(),
						[DataContext.MultimodalDangerousGoodsDeclaration] = (consol, parameters) => new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build(),
						[DataContext.FRPortsContainerAdviceToBookingAMQ] = (consol, parameters) => new FR.ContainerAdviceToBookingBuilder(consol, parameters).Build(),
						[DataContext.FRImportManifestLPD] = (consol, parameters) => new FR.ImportManifestBuilder(consol, parameters).Build(),
						[DataContext.FRUnderbondMovementRequest] = (consol, parameters) => new FR.UnderbondMovementRequestBuilder(consol, parameters).Build(),
						[DataContext.FRFinalContainerManifestLDE] = (consol, parameters) => new FR.FinalManifestBuilder(consol, parameters).Build(),
						[DataContext.FRContainerOutturnReportCDM] = (consol, parameters) => new FR.OutturnReportBuilder(consol, parameters).Build(),
						[DataContext.FRPortsIntegrationDossier] = (consol, parameters) => new FR.ConsolDossierBuilder(consol, parameters).Build(),
						[DataContext.BEEDeskExportNotification] = (consol, parameters) => new BE.ExportNotificationBuilder(consol).Build(),
						[DataContext.BEDangerousGoodsNotification] = (consol, parameters) => new BE.DangerousGoodsNotificationBuilder(consol, parameters).Build(),
						[DataContext.BECertifiedPickup] = (consol, parameters) => new BE.CertifiedPickupBuilder(consol, parameters).Build(),
						[DataContext.DEAdvancedLogisticsPortOrder] = (consol, parameters) => new DE.AdvancedLogisticsPortOrderBuilder(consol).Build(),
						[DataContext.TMiningSecureContainerRelease] = (consol, parameters) => new SecureContainerReleaseBuilder(consol, parameters).Build(),
						[DataContext.CGNExportNotification] = (consol, parameters) => new NL.CGNExportNotificationBuilder(consol).Build(),
						[DataContext.DraftHouseBill] = (consol, parameters) => new DraftHouseBillBuilder(consol, parameters).Build(),
						[DataContext.ExportPreAdviceNotification] = (consol, parameters) => new ExportPreAdviceNotificationBuilder(consol, parameters).Build(),
						[DataContext.ILGatePassMovement] = (consol, parameters) => new IL.GatePassMovementBuilder(consol.GatePassMovementProvider).Build(),
						[DataContext.CMRConsignmentNote] = (consol, parameters) => new ConsolCMRConsignmentNoteBuilder(consol, parameters).Build()
					});
			}
		}

		object GetFromQuotedBooking(QuotedBooking quotedBooking, string dataContext, IDocDataObjectParameters parameters)
		{
			switch (dataContext)
			{
				case DataContext.HouseBill:
					return quotedBooking.Booking != null
						? new HouseBillBuilder(quotedBooking.Booking, parameters).Build()
						: null;
			}

			return null;
		}
	}
}
