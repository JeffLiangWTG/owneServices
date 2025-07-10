using System.Diagnostics.CodeAnalysis;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	public sealed class ForwardingDocDataObjectUXmlWriter : IForwardingDocDataObjectUXmlWriter
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy writerStrategy, IDocument document, MessageType messageType)
		{
			var manager = new DataWritingManager(writerStrategy);
			var docDataObject = document?.Data?.Value;
			var dataContext = document?.DataContext ?? string.Empty;
			switch (dataContext)
			{
				case DataContext.HouseBill:
					if (docDataObject is HouseBill houseBill)
					{
						if (houseBill.IsElectronicBOL && !houseBill.IsDraft)
						{
							return new ElectronicHouseBillDataObjectWriter(manager, document).GetDataObject(houseBill);
						}

						var writer = new HouseBillDataObjectWriter(manager, document);

						return writer.GetDataObject(houseBill);
					}
					break;

				case DataContext.ShippingOrder:
					if (docDataObject is ShippingOrder shippingOrder)
					{
						var writer = new CN.ShippingOrderDataObjectWriter(manager, document);

						return writer.GetDataObject(shippingOrder);
					}
					break;

				case DataContext.EManifest:
					if (docDataObject is EManifest eManifest)
					{
						var writer = new CN.EManifestDataObjectWriter(manager, messageType, document);

						return writer.GetDataObject(eManifest);
					}
					break;

				case DataContext.ETerminalReleaseManifest:
					if (docDataObject is ETerminalReleaseManifest eTerminalReleaseManifest)
					{
						var writer = new CN.ETerminalReleaseManifestDataObjectWriter(manager);

						return writer.GetDataObject(eTerminalReleaseManifest);
					}
					break;

				case DataContext.ContainerLoadPlan:
					if (docDataObject is ContainerLoadPlan containerLoadPlan)
					{
						var writer = new CN.ContainerLoadPlanDataObjectWriter(manager);

						return writer.GetDataObject(containerLoadPlan);
					}
					break;

				case DataContext.ShippingInstruction:
				case DataContext.BookingRequest:
					if (docDataObject is CarrierMessageData carrierData)
					{
						var writer = new CarrierMessageDataObjectWriter(manager, dataContext, document);

						return writer.GetDataObject(carrierData);
					}
					else if (docDataObject is SeaShipmentBookingRequest seaShipmentBookingRequest)
					{
						var writer = new SeaShipmentBookingRequestDataObjectWriter(manager, document);

						return writer.GetDataObject(seaShipmentBookingRequest);
					}
					break;

				case DataContext.VerifiedGrossMass:
					if (docDataObject is VerifiedGrossMass vgm)
					{
						var writer = new VerifiedGrossMassDataObjectWriter(manager, document);

						return writer.GetDataObject(vgm);
					}
					break;

				case DataContext.AirBookingRequest:
					if (docDataObject is AirBookingRequest bookingRequest)
					{
						var writer = new AirBookingRequestDataObjectWriter(manager, messageType);

						return writer.GetDataObject(bookingRequest);
					}
					break;

				case DataContext.AirCargoAdvanceScreening:
					if (docDataObject is AirCargoAdvanceScreening acas)
					{
						var writer = new US.AcasDataObjectWriter(manager);

						return writer.GetDataObject(acas);
					}
					break;

				case DataContext.ACASHouseChecklist:
					if (docDataObject is ACASHouseChecklist acasHouseChecklist)
					{
						var writer = new US.ACASHouseChecklistDataObjectWriter(manager);

						return writer.GetDataObject(acasHouseChecklist);
					}
					break;

				case DataContext.CargoControlAndTransit:
					if (docDataObject is CargoControlAndTransit cct)
					{
						var writer = new BR.CargoControlAndTransitDataObjectWriter(manager);

						return writer.GetDataObject(cct);
					}
					break;

				case DataContext.CargoControlAndTransitHouseManifest:
					if (docDataObject is CargoControlAndTransitHouseManifest cctHouseManifest)
					{
						var writer = new BR.CargoControlAndTransitHouseManifestDataObjectWriter(manager);

						return writer.GetDataObject(cctHouseManifest);
					}
					break;

				case DataContext.NLPortbaseImportNotification:
					if (docDataObject is PortbaseImportNotification portbaseImport)
					{
						var writer = new NL.PortbaseImportNotificationDataObjectWriter(manager);

						return writer.GetDataObject(portbaseImport);
					}
					break;

				case DataContext.NLPortbaseExportNotification:
					if (docDataObject is PortbaseExportNotification portbaseExport)
					{
						var writer = new NL.PortbaseExportNotificationDataObjectWriter(manager);

						return writer.GetDataObject(portbaseExport);
					}
					break;
				case DataContext.FRPortsCustomsCheckCAED:
					if (docDataObject is CustomsDeclarationCheck customsDeclarationCheck)
					{
						var writer = new FR.CustomsDeclarationCheckDataObjectWriter(manager);

						return writer.GetDataObject(customsDeclarationCheck);
					}
					break;

				case DataContext.FRPortsIntegrationDossier:
					if (docDataObject is Dossier dossier)
					{
						var writer = new FR.DossierDataObjectWriter(manager);

						return writer.GetDataObject(dossier);
					}
					break;
				case DataContext.FRPortsContainerAdviceToBookingAMQ:
					if (docDataObject is ContainerAdviceToBooking containerAdviceToBooking)
					{
						var writer = new FR.ContainerAdviceToBookingWriter(manager);

						return writer.GetDataObject(containerAdviceToBooking);
					}
					break;
				case DataContext.FRImportManifestLPD:
					if (docDataObject is ImportManifest importManifest)
					{
						var writer = new FR.ImportManifestWriter(manager);

						return writer.GetDataObject(importManifest);
					}
					break;
				case DataContext.FRFinalContainerManifestLDE:
					if (docDataObject is FinalManifest finalManifest)
					{
						var writer = new FR.FinalManifestWriter(manager);

						return writer.GetDataObject(finalManifest);
					}
					break;
				case DataContext.FRContainerOutturnReportCDM:
					if (docDataObject is OutturnReport outturnReport)
					{
						var writer = new FR.OutturnReportWriter(manager);

						return writer.GetDataObject(outturnReport);
					}
					break;

				case DataContext.FRPortsGoodsReceivedCRESA:
					if (docDataObject is Cresa cresa)
					{
						var writer = new FR.CresaDataObjectWriter(manager);

						return writer.GetDataObject(cresa);
					}
					break;

				case DataContext.CargoDues:
				case DataContext.CargoDuesBrokerage:
					if (docDataObject is CargoDues cargoDues)
					{
						var writer = new ZA.CargoDuesDataObjectWriter(manager);

						return writer.GetDataObject(cargoDues);
					}
					break;
				case DataContext.FRUnderbondMovementRequest:
					if (docDataObject is UnderbondMovementRequest request)
					{
						var writer = new FR.UnderbondMovementRequestDataObjectWriter(manager);

						return writer.GetDataObject(request);
					}
					break;
				case DataContext.FRPortsTrackingRequestTRC:
					if (docDataObject is DemandeDeTracing demandeDeTracing)
					{
						var writer = new FR.DemandeDeTracingDataObjectWriter(manager);

						return writer.GetDataObject(demandeDeTracing);
					}
					break;
				case DataContext.BEDangerousGoodsNotification:
					if (docDataObject is DangerousGoodsNotification dangerousGoodsNotification)
					{
						var writer = new BE.DangerousGoodsNotificationObjectWriter(manager);

						return writer.GetDataObject(dangerousGoodsNotification);
					}
					break;
				case DataContext.BEEDeskExportNotification:
					if (docDataObject is ExportNotification exportNotification)
					{
						var writer = new BE.ExportNotificationWriter(manager);

						return writer.GetDataObject(exportNotification);
					}
					break;
				case DataContext.BECertifiedPickup:
					if (docDataObject is CertifiedPickup certifiedPickup)
					{
						var writer = new BE.CertifiedPickupDataObjectWriter(manager);

						return writer.GetDataObject(certifiedPickup);
					}
					break;
				case DataContext.TMiningSecureContainerRelease:
					if (docDataObject is SecureContainerRelease secureContainerRelease)
					{
						var writer = new SecureContainerReleaseDataObjectWriter(manager);

						return writer.GetDataObject(secureContainerRelease);
					}
					break;
				case DataContext.CertificateOfOriginChAFTA:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.Chafta chafta)
					{
						var writer = new CertificateOfOrigin.ChaftaDataObjectWriter(manager, document);

						return writer.GetDataObject(chafta);
					}
					break;
				case DataContext.CertificateOfOriginKAFTA:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.KAFTA kafta)
					{
						var writer = new CertificateOfOrigin.KAFTADataObjectWriter(manager, document);

						return writer.GetDataObject(kafta);
					}
					break;
				case DataContext.CertificateOfOriginNZCFTA:
					if (docDataObject is NZCFTA nzcfta)
					{
						var writer = new NZCFTADataObjectWriter(manager, document);

						return writer.GetDataObject(nzcfta);
					}
					break;
				case DataContext.DEAdvancedLogisticsPortOrder:
					if (docDataObject is AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
					{
						var writer = new DE.AdvancedLogisticsPortOrderDataObjectWriter(manager);

						return writer.GetDataObject(advancedLogisticsPortOrder);
					}
					break;
				case DataContext.MXHouseAirwayBill:
					if (docDataObject is HouseAirwayBill hawb)
					{
						var writer = new MX.HouseAirwayBillDataObjectWriter(manager);

						return writer.GetDataObject(hawb);
					}
					break;
				case DataContext.CargoReceiptAdvice:
					if (docDataObject is CargoReceiptAdvice cargoReceiptAdvice)
					{
						var writer = new CargoReceiptAdviceDataObjectWriter(manager);

						return writer.GetDataObject(cargoReceiptAdvice);
					}
					break;
				case DataContext.CertificateOfOriginAANZFTA:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.Aanzfta aanzfta)
					{
						var writer = new CertificateOfOrigin.AanzftaDataObjectWriter(manager, document);

						return writer.GetDataObject(aanzfta);
					}
					break;
				case DataContext.ConsolidationAdvice:
					if (docDataObject is ConsolidationAdvice consolidationAdvice)
					{
						var writer = new ConsolidationAdviceDataObjectWriter(manager, document);

						return writer.GetDataObject(consolidationAdvice);
					}
					break;
				case DataContext.CertificateOfOriginJAEPA:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.JAEPA japea)
					{
						var writer = new CertificateOfOrigin.JAEPADataObjectWriter(manager, document);

						return writer.GetDataObject(japea);
					}
					break;
				case DataContext.CertificateOfOriginRCEP:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.RCEP rcep)
					{
						var writer = new CertificateOfOrigin.RCEPDataObjectWriter(manager, document);

						return writer.GetDataObject(rcep);
					}
					break;
				case DataContext.CertificateOfOriginCPTPP:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.CPTPP cptpp)
					{
						var writer = new CertificateOfOrigin.CPTPPDataObjectWriter(manager, document);

						return writer.GetDataObject(cptpp);
					}
					break;
				case DataContext.CertificateOfOriginIACEPA:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.IACEPA iacepa)
					{
						var writer = new CertificateOfOrigin.IACEPADataObjectWriter(manager, document);

						return writer.GetDataObject(iacepa);
					}
					break;
				case DataContext.CertificateOfOriginNZ:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.COONZ coonz)
					{
						var writer = new CertificateOfOrigin.COONZDataObjectWriter(manager, document);

						return writer.GetDataObject(coonz);
					}
					break;
				case DataContext.CertificateOfOriginTAFTA:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.TAFTA tafta)
					{
						var writer = new CertificateOfOrigin.TAFTADataObjectWriter(manager, document);

						return writer.GetDataObject(tafta);
					}
					break;
				case DataContext.CertificateOfOriginPAFTA:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.PAFTA pafta)
					{
						var writer = new CertificateOfOrigin.PAFTADataObjectWriter(manager, document);
						return writer.GetDataObject(pafta);
					}
					break;
				case DataContext.CertificateOfOriginAUCONP:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.AUCONP auconp)
					{
						var writer = new CertificateOfOrigin.AUCONPDataObjectWriter(manager, document);
						return writer.GetDataObject(auconp);
					}
					break;
				case DataContext.CertificateOfOriginAUKFTA:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.AUKFTA aukfta)
					{
						var writer = new CertificateOfOrigin.AUKFTADataObjectWriter(manager, document);
						return writer.GetDataObject(aukfta);
					}
					break;
				case DataContext.CertificateOfOriginIAECTA:
					if (docDataObject is DocDataObjects.CertificateOfOrigin.IAECTA iaecta)
					{
						var writer = new CertificateOfOrigin.IAECTADataObjectWriter(manager, document);
						return writer.GetDataObject(iaecta);
					}
					break;

				case DataContext.CINExportNotification:
					if (docDataObject is CINExportNotification cinExportNotification)
					{
						var writer = new FR.CINExportNotificationWriter(manager);

						return writer.GetDataObject(cinExportNotification);
					}
					break;

				case DataContext.ExportPreAdviceNotification:
					if (docDataObject is ExportPreAdviceNotification notification)
					{
						var writer = new NZ.ExportPreAdviceNotificationDataObjectWriter(manager);

						return writer.GetDataObject(notification);
					}
					break;

				case DataContext.CGNExportNotification:
					if (docDataObject is CGNExportNotification cgnExportNotification)
					{
						var writer = new NL.CGNExportNotificationDataObjectWriter(manager);

						return writer.GetDataObject(cgnExportNotification);
					}
					break;
			}
			return null;
		}
	}
}
