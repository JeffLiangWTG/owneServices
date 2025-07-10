using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ForwardingShipmentVisualizableDocumentSupporter : ForwardingVisualizableDocumentSupporter<ForwardingShipment>
	{
		public ForwardingShipmentVisualizableDocumentSupporter(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj)
		{
			var result = base.GetBusinessObjectInAnotherFactory(factory, bizObj);

			if (result is ForwardingShipment shipment)
			{
				shipment.IsEditingElectronicBOL = (bizObj as ForwardingShipment)?.IsEditingElectronicBOL ?? false;
			}

			return result;
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.MaintainShipmentCustomiseForms;
		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document)
		{
			if (document?.Data?.Value is HouseBill houseBill)
			{
				if (houseBill.IsDraft)
				{
					return new CarrierHouseBillMessageEventProcessor(parent);
				}
			}

			switch (document?.DataContext)
			{
				case DataContext.FRPortsIntegrationDossier:
					return new FR.ShipmentDossierEventProcessor(parent, document);

				case DataContext.ExportPreAdviceNotification:
					return new FR.ShipmentDossierEventProcessor(parent, document);
			}

			return null;
		}

		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters)
		{
			switch (documentName)
			{
				case ShipmentDocumentNames.BookingRequest:
					return ShipmentDocumentDataStoreNames.BookingRequest;
				case ShipmentDocumentNames.ShippersDeclarationForDangerousGoods:
					return ShipmentDocumentDataStoreNames.ShippersDangerousGoodsDeclaration;

				case ShipmentDocumentNames.AdvancedCargoReport:
					if (eventParameters?.Location.HasValue ?? false)
					{
						var location = eventParameters.Location.Value.SubstringSafe(0, 2);

						switch (location)
						{
							case Core.Constants.CountryCodes.Brazil:
								return ShipmentDocumentDataStoreNames.AdvancedCargoReportBR;

							case Core.Constants.CountryCodes.UnitedStates:
								return ShipmentDocumentDataStoreNames.AdvancedCargoReportUS;
						}
					}

					return string.Empty;

				case FR.FrenchPortsConstants.DocumentNames.DOSImport:
					return ShipmentDocumentDataStoreNames.DossierImport;

				case FR.FrenchPortsConstants.DocumentNames.DOSExport:
					return ShipmentDocumentDataStoreNames.DossierExport;

				case FR.FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA:
					return ShipmentDocumentDataStoreNames.GoodsReceivedCRESA;

				case FR.FrenchPortsConstants.DocumentNames.CAEDImport:
					return ShipmentDocumentDataStoreNames.CustomsClearanceCheckImport;

				case FR.FrenchPortsConstants.DocumentNames.CAEDExport:
					return ShipmentDocumentDataStoreNames.CustomsClearanceCheckExport;

				case MX.MexicanPortsConstants.DocumentNames.HouseAirWaybill:
					return ShipmentDocumentDataStoreNames.MexicanHouseAirWaybill;

				case ShipmentDocumentNames.BillOfLading:
					return ShipmentDocumentDataStoreNames.BillOfLading;

				case ShipmentDocumentNames.ChaftaCertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.ChaftaCertificateOfOrigin;

				case ShipmentDocumentNames.AANZFTACertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.AANZFTACertificateOfOrigin;

				case ShipmentDocumentNames.KAFTACertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.KAFTACertificateOfOrigin;

				case ShipmentDocumentNames.PAFTACertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.PAFTACertificateOfOrigin;

				case ShipmentDocumentNames.NZCFTACertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.NZCFTACertificateOfOrigin;

				case ShipmentDocumentNames.COOUSCertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.COOUSCertificateOfOrigin;

				case ShipmentDocumentNames.CPTPPCertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.CPTPPCertificateOfOrigin;

				case ShipmentDocumentNames.ConsolidationAdvice:
					return ShipmentDocumentDataStoreNames.ConsolidationAdvice;

				case ShipmentDocumentNames.CargoReceiptAdvice:
					return ShipmentDocumentDataStoreNames.CargoReceiptAdvice;

				case ShipmentDocumentNames.JAEPACertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.JAEPACertificateOfOrigin;

				case ShipmentDocumentNames.RCEPCertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.RCEPCertificateOfOrigin;

				case FR.FrenchPortsConstants.DocumentNames.CINExportNotification:
					return ShipmentDocumentDataStoreNames.CINExportNotification;

				case ShipmentDocumentNames.NZCertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.NZCertificateOfOrigin;

				case ShipmentDocumentNames.ILDeliveryOrder:
					return ShipmentDocumentDataStoreNames.ILDeliveryOrder;

				case ShipmentDocumentNames.ILGatePassMovement:
					return ShipmentDocumentDataStoreNames.ILGatePassMovement;

				case ShipmentDocumentNames.TAFTACertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.TAFTACertificateOfOrigin;

				case ShipmentDocumentNames.IACEPACertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.IACEPACertificateOfOrigin;

				case ShipmentDocumentNames.AUKFTACertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.AUKFTACertificateOfOrigin;

				case ShipmentDocumentNames.AUCONPCertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.AUCONPCertificateOfOrigin;

				case ShipmentDocumentNames.IAECTACertificateOfOrigin:
					return ShipmentDocumentDataStoreNames.IAECTACertificateOfOrigin;

				default:
					return documentName;
			}
		}

		public override IMessageLogCreator GetMessageLogCreator(IDocument document)
		{
			switch (document.DataContext)
			{
				case DataContext.EManifest:
					return new CN.EManifestLogsCreator();

				case DataContext.AirCargoAdvanceScreening:
					return new US.AcasMessageLogCreator();

				case DataContext.CargoControlAndTransit:
					return new AdvancedReportMessageLogCreator(DocumentNames.AdvancedCargoReport, Core.Constants.CountryCodes.Brazil);

				default:
					return null;
			}
		}

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions)
		{
			switch (document.DataContext)
			{
				case DataContext.EManifest:
					return new CN.EManifestMessagingExtensions(document, parent, messageInstructions);

				case DataContext.AirCargoAdvanceScreening:
					return new US.AcasMessagingExtensions(document);

				case DataContext.CargoControlAndTransit:
					return new CargoControlAndTransitMessagingExtensions(document);

				case DataContext.FRPortsIntegrationDossier:
					return new FR.DossierMessagingExtensions(document, parent, messageInstructions);

				case DataContext.FRPortsGoodsReceivedCRESA:
					return new FR.CresaMessagingExtensions(document, parent, messageInstructions);

				case DataContext.HouseBill:
					return HouseBillMessagingExtensionCreator.New(parent, document, messageInstructions);

				case DataContext.BookingRequest:
					return new SeaShipmentBookingRequestMessagingExtensions(parent);

				case DataContext.CINExportNotification:
					return new FR.CINExportNotificationExtensions(document, parent, messageInstructions);

				case string context when CertificateOfOriginConstants.DataContexts.Contains(context):
					return new CertificateOfOriginMessagingExtensions(parent, messageInstructions.DocumentName);

				case DataContext.ILDeliveryOrder:
					return new DeliveryOrderMessagingExtensions(parent.DeliveryOrderProvider);

				case DataContext.ILGatePassMovement:
					return new GatePassMovementMessagingExtensions(parent.GatePassMovementProvider);

				default:
					return null;
			}
		}

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext)
		{
			switch (dataContext)
			{
				case DataContext.UXML:
					yield return new FreightLibrary();
					yield return new ForwardingLibrary();
					yield return new ForwardingShipmentLibrary(parent);
					break;

				case DataContext.HouseBill:
					yield return (IMacroLibrary)ObjectFactory.Get<IHouseBillMacroLibrary>();
					break;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override Either<string, object> GetAdditionalData(object obj, IStmMenuItem menuItem)
		{
			var shipment = obj as ForwardingShipment;

			if (IsDataContextHouseBill(menuItem) && shipment is not null)
			{
				if (shipment.AWBOrHBLPrintingShouldBeConfirmed() && !QueryProvider.ConfirmBOLPrinting(shipment))
				{
					return string.Empty;
				}

				if (IsElectronicBillOfLadingMenuItem(menuItem) && !shipment.IsElectronicShippingInstructionReceived
					&& Globals.Message.Show(Res.GetString("c8e97415-884c-4164-9f83-fe9992c0e5bc", "There is no electronic Shipping Instruction received from the Booking Party. Select OK to generate the Draft Bill of Lading or Cancel."),
						Res.GetString("df62ae28-e43b-4c43-967d-9bee5be80734", "No electronic Shipping Instruction"),
						ZMessageBoxButtons.OKCancel, ZDialogResult.Cancel) != ZDialogResult.OK)
				{
					return string.Empty;
				}

				return (object)null;
			}

			if ((IsCargoAndTransitMenuItem(menuItem) || IsACASMenuItem(menuItem)))
			{
				return GetCargoAndTransitAdditionalData(menuItem);
			}

			if (IsEManifestMenuItem(menuItem) && !IsShipmentValidForEManifest())
			{
				return Res.GetString("3b323ad6-ab58-4c4e-b3d4-e9d429bafbec", "The Shipment is not attached to a Consolidation with a transport leg loading in China. The form will display once this Shipment is attached to a Consolidation with a relevant transport leg.");
			}

			if (IsFrenchPortCRESAMenuItem(menuItem))
			{
				if (!IsShipmentValidForFrenchPortCRESAAndDOS())
				{
					return Res.GetString("e6c2e5ef-8d12-4b77-94ac-9658b7bbb2f2", "Goods Received (CRESA) Reporting is only available from Shipments with types STD, BCN or HVL.");
				}

				if (IsPortAuthorityControlled())
				{
					return Res.GetString("8672d73c-290a-4ec3-b808-15ed538118d9",
						"The Goods Received (CRESA) message cannot be sent from Shipment for the CFS/Transit Warehouse [{0}, {1}].\r\n" +
						"This CFS/Transit Warehouse is configured to send Port Authority Messages themselves, and as such CRESA message should be sent from the Transit Warehouse > Received Consignment Module.",
						new string[] { parent.ExportReceivingDepot?.Header?.NameAndCode, parent.ExportReceivingDepot?.Address1 });
				}
			}

			if (IsFrenchPortDOSMenuItem(menuItem) && !IsShipmentValidForFrenchPortCRESAAndDOS())
			{
				return Res.GetString("a9cde48d-2d97-44f5-a88b-efb4b599645d", "File Creation Request (DOS) Reporting is only available from Shipments with types STD, BCN or HVL.");
			}

			if (IsFrenchPortDOSMenuItem(menuItem))
			{
				return GetFRPortsIntegrationDossierAdditionalData(obj);
			}

			if (IsForwardersCargoReceiptMenuItem(menuItem) && !ShipmentIsValidForFCR())
			{
				return Res.GetString("ff09d4e7-f243-2387-42ec-9cbc799a353b", "The Shipment must at least contain a pack line with a linked Supplier Booking to generate a Forwarders Cargo Receipt");
			}

			if (IsCertificateOfOriginMenuItem(menuItem))
			{
				if (!ShipmentHasPackLines() && !ShipmentHasInvoiceLines())
				{
					return Res.GetString("1cdbedea-0229-4a69-8a46-ab35508d7628", "To create a Certificate of Origin, this Shipment requires at least one pack line entry in the Packing tab or one invoice line entry in the Brokerage tab.");
				}

				if (HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.CertificateOfOriginNZCFTA) && ShipmentHasTooManyPackLines())
				{
					return Res.GetString("c7c2f4fc-d58c-445a-8a02-333489a1190e", "An NZCFTA Certificate can only contain a maximum of 20 pack lines. Please edit in the Packing tab to be able to create the Certificate.");
				}

				if (!CheckGoodsDescriptionLineLimit(shipment))
				{
					return string.Empty;
				}
			}

			if (IsForwardersCargoReceiptDetailMenuItem(menuItem) && !ShipmentIsValidForFCR())
			{
				return Res.GetString("2197e285-d9a8-4a77-8585-5badad85085d", "The Shipment must at least contain a pack line with a linked Supplier Booking to generate a Forwarders Cargo Receipt Detail");
			}

			if (IsILDeliveryOrderMenuItem(menuItem) && shipment is not null)
			{
				if (!IsReceivingAgentVATExist(shipment, CountryCodes.Israel))
				{
					return Res.GetString("8074EA6C-01DE-48D8-B2E9-2A2FA581C631", "Consol Receiving Agent VAT # is not configured");
				}

				if (!IsImportBrokerVATExist(shipment, CountryCodes.Israel))
				{
					return Res.GetString("F5707112-C01C-41DC-858A-1B7500C29902", "Import Broker VAT # is not configured");
				}
			}

			if (IsILGatePassMovement(menuItem) && shipment is not null)
			{
				if (!IsReceivingAgentVATValid(shipment, CountryCodes.Israel))
				{
					return Res.GetString("0633C761-3220-4DEA-B112-84FC4955C4D2", "Consol Receiving Agent VAT # is not configured for country IL, update Organization – Config tab");
				}

				if (shipment.JS_PackingMode == Core.Constants.ContainerModes.LCL || ArrivalConsolContainerModeIsGroupage(shipment))
				{
					return Res.GetString("0C5522D9-5456-4BC7-9CED-1E467AD8757F", "Shipment's Container Mode doesn't match, please sent the request from the related Consol");
				}

				if (shipment.ImportReleaseDepot == null)
				{
					return Res.GetString("84CC20AC-0C1F-46A9-B757-B4119DFE677A", "You must update the Transit Warehouse for sending Gatepass Movement request");
				}

				if (!IsTransitWarehouseCCPValid(shipment, CountryCodes.Israel))
				{
					return Res.GetString("D95AAE0F-67DB-4D91-9D19-E7D5DDC7C3E8", "Customs Controlled Premises Code (CCP) is not configured for country IL, update Organization – Config tab");
				}
			}

			if (IsUSATF6AFormMenuItem(menuItem))
			{
				var selector = ObjectFactory.Get<Enterprise.Integration.Customs.US.IATF6AFormPermitNumbersSelector>();
				var selectedPermitNumbers = selector.SelectPermitNumbers(shipment.DeclarationForDocuments);

				if (selectedPermitNumbers.IsLeft)
				{
					return selectedPermitNumbers.Left;
				}

				return selectedPermitNumbers.Right;
			}

			return (object)null;
		}

		ZBool CheckGoodsDescriptionLineLimit(ForwardingShipment shipment)
		{
			var invoices = shipment.Declarations.Cast<BaseJobDeclaration>().SelectMany(d => d.Invoices);

			if (invoices.Any())
			{
				foreach (var invoiceHeader in invoices)
				{
					foreach (BaseJobComInvoiceLine invoiceLineBO in invoiceHeader.InvoiceLines)
					{
						if (!string.IsNullOrWhiteSpace(invoiceLineBO.JI_Description) && invoiceLineBO.JI_Description.Split(new[] { "\r\n", "\n", "\r" }).Length > 4)
						{
							return ShowGoodsDescriptionWarning();
						}
					}
				}
			}
			else
			{
				foreach (ForwardingPackLine packLine in shipment.OuterPackLines)
				{
					if (!string.IsNullOrWhiteSpace(packLine.JL_Description) && packLine.JL_Description.Split(new[] { "\r\n", "\n", "\r" }).Length > 4)
					{
						return ShowGoodsDescriptionWarning();
					}
				}
			}

			return true;
		}
		ZBool ShowGoodsDescriptionWarning()
		{
			var result = Globals.Message.Show(
				Res.GetString("C51F03C4-D751-4BE4-BABC-8686225BA56F",
					"A maximum number of four lines for Goods Description is allowed for each line item. Only the first four lines of the Goods Description will be included on the Certificate of Origin. Click OK to proceed or press Cancel to go back and amend the goods description."),
				Res.GetString("73ECC4DB-1014-48DF-BA97-BDBF7B589797", "Goods Description Line Limit"),
				ZMessageBoxButtons.OKCancel,
				ZDialogResult.Cancel);

			return result == ZDialogResult.OK;
		}

		public override IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.English))
			{
				if (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration && parent.IsEditingElectronicBOL && parent.JS_HouseBillOfLadingType != HouseBillOfLadingTypes.Code.FIATAHBL && document?.Data?.Value is HouseBill houseBill)
				{
					var docDataParameters = new DocumentVisualizer.DocDataObjects.DocDataObjectParameters(houseBill.IsOriginal ? (NoResString)"Original" : (NoResString)"Copy", ShipmentDocumentDataStoreNames.CarrierBillOfLading);
					var carrierHouseBill = new CarrierHouseBillBuilder(parent, docDataParameters).Build();

					var additionalDoc = new EmptyDocument(ShipmentDocumentNames.DraftBill, DataContext.HouseBill, DocumentVisualizer.DocDataObjects.DocDataObjectExtensions.MakeDocDataDynamic(carrierHouseBill));

					var container = new ServiceContainer();
					container.Register<IEventBroker>(new EventBroker());

					var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
					var documentData = documentDataLoader.Load(parent, ShipmentDocumentDataStoreNames.BillOfLading);

					if (documentData != null)
					{
						var documentOverrideMerger = new DocumentOverrideMerger(container, documentData);
						documentOverrideMerger.ApplyOverride(additionalDoc);
					}

					return new[]
					{
						additionalDoc
					};
				}

				return base.GetAdditionalDocuments(document, messageInstructions);
			}
		}

		#region FrenchPort AdditionalData

		Either<string, object> GetFRPortsIntegrationDossierAdditionalData(object obj)
		{
			if (obj is ForwardingShipment shipment && Core.Constants.CountryCodes.IsFranceOrTerritory(shipment.ExportReceivingDepot?.OA_RN_NKCountryCode))
			{
				if (!shipment.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode && !log.SL_IsCancelled && log.MatchesDocumentName(FR.FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA)) && !AllPackingLinesHavePANAndERC(shipment))
				{
					if (ShouldReportError(shipment))
					{
						ErrorReporter.ReportOnce("Log for CS01641584 - BOLLOGPAR1 - ERROR MESSAGE TO PUSH \"DOS\" TO SONE CCS", GenerateErrorReportInfo(shipment));
					}

					return Res.GetString("D0E2E5E6-97F6-45D4-9DCE-A79AC3C9B9CF", "Cargo belongs to all pack lines not received into the warehouse and CRESA is not finalized with CCS. You are not allowed to send File Creation Request (DOS) message until finalized the CRESA.");
				}

				var cfsCountryCode = shipment.ExportReceivingDepot.OA_RN_NKCountryCode;

				var rcnsInCurrentShipment = shipment.OuterPackLines.Cast<ForwardingPackLine>()
				.SelectMany(p => p.AdditionalReferenceNumbers
					.Cast<CusEntryNumber>()
					.Where(r => r.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC && !r.CE_EntryNum.IsEmpty && r.CE_RN_NKCountryCode == cfsCountryCode)
					.Select(a => a.CE_EntryNum))
				.Distinct();

				if (rcnsInCurrentShipment.Any())
				{
					var alreadySendRCNsWithShipments = FR.FRExtensions.GetShipmentNumbersWithSpecifiedRCNSent(shipment, rcnsInCurrentShipment);
					var alreadySendRCNs = alreadySendRCNsWithShipments.SelectMany(x => x.Value).Distinct();
					var rcnNeedBeSent = rcnsInCurrentShipment.Except(alreadySendRCNs).ToList();

					if (!rcnNeedBeSent.Any())
					{
						return Res.GetString("e3fa8950-704c-405f-8515-3be72ecbebc4", "File Creation Request (DOS) being sent from Shipment {0} for all received consignment(s). You are not allowed to send File Creation Request message from this Shipment.", string.Join(", ", alreadySendRCNsWithShipments.Keys));
					}
					else if (rcnNeedBeSent.Count != rcnsInCurrentShipment.Count())
					{
						Globals.Message.ShowWarning(Res.GetString("c0b0dc0c-713f-4947-b5f7-1cb6829b8c0f", "File Creation Request (DOS) being sent from Shipment {0} for received consignment(s) {1}. The File Creation Request (DOS) message from this Shipment will send with remaining received consignment(s).", string.Join(", ", alreadySendRCNsWithShipments.Where(x => x.Value.Any(c => alreadySendRCNs.Contains(c))).Select(d => d.Key).Distinct()), string.Join(", ", alreadySendRCNs)));
					}

					return rcnNeedBeSent;
				}
			}

			return (object)null;
		}

		bool AllPackingLinesHavePANAndERC(ForwardingShipment shipment)
		{
			var cfsCountryCode = shipment.ExportReceivingDepot?.OA_RN_NKCountryCode ?? Core.Constants.CountryCodes.France;

			return shipment.OuterPackLines.Cast<ForwardingPackLine>().All(p =>
				p.PortReferences.Cast<CusEntryNumber>().Any(r => !r.CE_EntryNum.IsEmpty && r.CE_RN_NKCountryCode == cfsCountryCode && r.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN) &&
				p.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Any(a => !a.CE_EntryNum.IsEmpty && a.CE_RN_NKCountryCode == cfsCountryCode && a.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC));
		}

		bool ShouldReportError(ForwardingShipment shipment)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;

			return enterpriseCode == "B52" && serverCode == "PRO" && shipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => IsPANCodeAndERCCodeBothInPackLineCusEntryNums(p.CusEntryNums.Cast<CusEntryNumber>()));
		}

		bool IsPANCodeAndERCCodeBothInPackLineCusEntryNums(IEnumerable<CusEntryNumber> cusEntryNums)
		{
			return cusEntryNums.Any(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN) && cusEntryNums.Any(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC);
		}

		ZString GenerateErrorReportInfo(ForwardingShipment shipment)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"log with SL_SE_NKEvent == {Events.MessageAcceptedCode} and matches document name {FR.FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA} was not found in shipment {shipment.JS_UniqueConsignRef} logs.");
			stringBuilder.AppendLine($"AllPackingLinesHavePANAndERC return false.");
			stringBuilder.AppendLine($"cfsCountryCode: {shipment.ExportReceivingDepot?.OA_RN_NKCountryCode ?? Core.Constants.CountryCodes.France}");

			foreach (var packline in shipment.OuterPackLines.Cast<ForwardingPackLine>())
			{
				stringBuilder.AppendLine($"packLine ({packline.PK}): {packline.JL_PackLineId} has {packline.PortReferences.Count} PortReference(s) and {packline.AdditionalReferenceNumbers.Count} AdditionalReferenceNumber(s).");

				stringBuilder.AppendLine($"cusEntryNumber in PortReferences:");
				foreach (var cusEntryNumber in packline.PortReferences.Cast<CusEntryNumber>())
				{
					stringBuilder.AppendLine(GenerateCusEntryNumberInfo(cusEntryNumber));
				}

				stringBuilder.AppendLine($"cusEntryNumber in AdditionalReferenceNumbers:");
				foreach (var cusEntryNumber in packline.AdditionalReferenceNumbers.Cast<CusEntryNumber>())
				{
					stringBuilder.AppendLine(GenerateCusEntryNumberInfo(cusEntryNumber));
				}

				stringBuilder.AppendLine($"cusEntryNumber in CusEntryNums:");
				foreach (var cusEntryNumber in packline.CusEntryNums.OrderBy(x => x.CE_Category))
				{
					stringBuilder.AppendLine(GenerateCusEntryNumberInfo(cusEntryNumber));
				}
			}

			return stringBuilder.ToString();
		}

		ZString GenerateCusEntryNumberInfo(CusEntryNumber cusEntryNumber) =>
$@"PK: {cusEntryNumber.PK}, CE_ParentID: {cusEntryNumber.CE_ParentID}, CE_ParentTable: {cusEntryNumber.CE_ParentTable}, CE_EntryNum: {cusEntryNumber.CE_EntryNum}, CE_RN_NKCountryCode: {cusEntryNumber.CE_RN_NKCountryCode}, CE_EntryType: {cusEntryNumber.CE_EntryType},
CE_EntryStatus: {cusEntryNumber.CE_EntryStatus}, CE_Category: {cusEntryNumber.CE_Category}, CE_EntryLineReference: {cusEntryNumber.CE_EntryLineReference}, CE_IssueDate: {cusEntryNumber.CE_IssueDate}, CE_ExpiryDate: {cusEntryNumber.CE_ExpiryDate}, CE_EntryIsSystemGenerated: {cusEntryNumber.CE_EntryIsSystemGenerated},
SystemLastEditTimeUTC: {cusEntryNumber.CE_SystemLastEditTimeUtc}, SystemLastEditUser: {cusEntryNumber.CE_SystemLastEditUser}, CE_SystemCreateTimeUtc: {cusEntryNumber.CE_SystemCreateTimeUtc}, CE_SystemCreateUser: {cusEntryNumber.CE_SystemCreateUser}
";

		#endregion

		ISharedGuiQueryProvider QueryProvider => queryProvider ?? (queryProvider = ObjectFactory.Get<ISharedGuiQueryProvider>());
		ISharedGuiQueryProvider queryProvider;

		bool IsDataContextHouseBill(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.HouseBill);

		bool IsElectronicBillOfLadingMenuItem(IStmMenuItem menuItem) => menuItem?.Documents.OfType<IStmMenuTemplatePivot>().Any(x => x.SI_DataStoreName == ShipmentDocumentDataStoreNames.CarrierBillOfLading) ?? false;

		#region FrenchPort AdditionalData

		Either<string, object> GetCargoAndTransitAdditionalData(IStmMenuItem menuItem)
		{
			if (IsCargoAndTransitMenuItem(menuItem) && !IsShipmentValidForCCT())
			{
				return Res.GetString("2561921a-5a9d-7d82-4e32-a82ac7e64452", "Advanced Air Cargo Reporting is only available from Shipments with types STD, BCN, CLD, ASM or HVL.");
			}
			else if (IsACASMenuItem(menuItem) && !IsShipmentValidForACAS())
			{
				return Res.GetString("22f4adbf-e4f8-4a16-89e2-13c994749ee8", "Advanced Air Cargo Reporting is only available from Shipments with types STD, BCN, CLD or HVL.");
			}
			else if (parent.CoLoadMasterShipment != null && parent.CoLoadMasterShipment.JS_ShipmentType == Core.Constants.ShipmentTypes.CoLoadMaster)
			{
				return Res.GetString("8a4409f3-5874-41b2-996b-3b1b80c4deed", "In co-load scenario, Advance Air Cargo Reporting should be done from the Co-Load Master (CLD) shipment.");
			}
			else if (parent.CoLoadMasterShipment != null && parent.CoLoadMasterShipment.JS_ShipmentType == Core.Constants.ShipmentTypes.AssemblyMaster && parent.CoLoadMasterShipment.HasSentAdvancedCargoReport)
			{
				return Res.GetString("bc8db955-6f8d-4cdc-9be2-aeaf7702fe67", "Advanced Air Cargo Reporting has been sent from Assembly master shipment. In Assembly master scenario, Advanced Air Cargo Reporting can be done from master or subs not from both. Please withdraw the message sent from Assembly master shipment prior to send from sub-Shipment.");
			}
			else
			{
				var coLoadShipmentsSentAdvancedCargoReport = parent.CoLoadShipments?.Where(sub => sub.HasSentAdvancedCargoReport);
				if (coLoadShipmentsSentAdvancedCargoReport != null && coLoadShipmentsSentAdvancedCargoReport.Any())
				{
					return Res.GetString("33af6c2a-e1ad-4f8b-ab89-a85f1a4ff922", "Advanced Air Cargo Reporting has been sent from sub-shipments < {0} >. In Assembly master scenario, Advanced Air Cargo Reporting can be done from master or subs not from both. Please withdraw the message sent from above each sub-shipment prior to send from Assembly master.", string.Join(",", coLoadShipmentsSentAdvancedCargoReport.Select(s => s.JobNumber)));
				}
			}

			return (object)null;
		}

		bool IsACASMenuItem(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.AirCargoAdvanceScreening);

		bool IsCargoAndTransitMenuItem(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.CargoControlAndTransit);

		bool IsShipmentValidForCCT()
		{
			var validShipmentTypes = new HashSet<string>()
			{
				Core.Constants.ShipmentTypes.StandardHouse,
				Core.Constants.ShipmentTypes.BuyersConsolLead,
				Core.Constants.ShipmentTypes.CoLoadMaster,
				Core.Constants.ShipmentTypes.HighVolumeLowValue,
				Core.Constants.ShipmentTypes.AssemblyMaster
			};

			if (validShipmentTypes.Contains(parent.JS_ShipmentType))
			{
				return true;
			}

			return false;
		}

		bool IsShipmentValidForACAS()
		{
			var validShipmentTypes = new HashSet<string>()
			{
				Core.Constants.ShipmentTypes.StandardHouse,
				Core.Constants.ShipmentTypes.BuyersConsolLead,
				Core.Constants.ShipmentTypes.CoLoadMaster,
				Core.Constants.ShipmentTypes.HighVolumeLowValue
			};

			if (validShipmentTypes.Contains(parent.JS_ShipmentType))
			{
				return true;
			}

			return false;
		}

		#endregion

		bool IsEManifestMenuItem(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.EManifest);

		bool IsShipmentValidForEManifest()
		{
			if (!parent.Consols.Cast<ForwardingConsol>()
				.Any(c => c.Transports.Cast<Freight.Business.Transport>()
					.Any(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea
						&& t.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase)
						&& !t.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase))))
			{
				return false;
			}

			return true;
		}

		bool IsFrenchPortCRESAMenuItem(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.FRPortsGoodsReceivedCRESA);

		bool IsFrenchPortDOSMenuItem(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.FRPortsIntegrationDossier);

		bool IsShipmentValidForFrenchPortCRESAAndDOS()
		{
			var validShipmentTypes = new HashSet<string>()
			{
				Core.Constants.ShipmentTypes.StandardHouse,
				Core.Constants.ShipmentTypes.BuyersConsolLead,
				Core.Constants.ShipmentTypes.HighVolumeLowValue
			};

			if (validShipmentTypes.Contains(parent.JS_ShipmentType))
			{
				return true;
			}

			return false;
		}

		bool IsForwardersCargoReceiptMenuItem(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.ForwardersCargoReceiptSummary);

		bool IsForwardersCargoReceiptDetailMenuItem(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.ForwardersCargoReceiptDetail);

		bool IsILDeliveryOrderMenuItem(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.ILDeliveryOrder);

		bool IsImportBrokerVATExist(ForwardingShipment shipment, ZString countryCode)
			=> shipment.ImportBroker?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, countryCode) is ZString customsRegNo
				&& !customsRegNo.IsEmpty;

		bool IsReceivingAgentVATExist(ForwardingShipment shipment, ZString countryCode)
			=> shipment.ArrivalConsol is ForwardingConsol arrivalConsol
				&& arrivalConsol?.ReceivingForwarder?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, countryCode) is ZString customsRegNo
				&& !customsRegNo.IsEmpty;

		bool IsILGatePassMovement(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.ILGatePassMovement);

		bool IsTransitWarehouseCCPValid(ForwardingShipment shipment, string countryCode) =>
			shipment.ImportReleaseDepot?.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, countryCode) is ZString customsRegNo
			&& !customsRegNo.IsEmpty;

		bool IsReceivingAgentVATValid(ForwardingShipment shipment, string countryCode) =>
			shipment.ArrivalConsol is ForwardingConsol arrivalConsol
			&& arrivalConsol.ReceivingForwarder?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, countryCode) is ZString customsRegNo
			&& !customsRegNo.IsEmpty;

		bool ArrivalConsolContainerModeIsGroupage(ForwardingShipment shipment)
			=> shipment.ArrivalConsol is ForwardingConsol arrivalConsol
			&& arrivalConsol.JK_ConsolMode == Core.Constants.ContainerModes.Groupage;

		bool IsCertificateOfOriginMenuItem(IStmMenuItem menuItem) =>
			HasMenuItemAnyDataContextOfGivenList(menuItem, CertificateOfOriginConstants.DataContexts.Select(x => new ZString(x)).ToArray());

		bool IsUSATF6AFormMenuItem(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DocumentVisualizer.Integration.DataContext.USATF6A);

		bool IsPortAuthorityControlled()
		{
			if (parent.JS_OA_ExportReceivingDepot.IsEmpty)
			{
				return false;
			}

			var warehouse = parent.Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, parent.JS_OA_ExportReceivingDepot));
			return warehouse?.WW_IsPortAuthorityControlled ?? false;
		}

		bool ShipmentIsValidForFCR()
		{
			return parent.OuterPackLines.Cast<ForwardingPackLine>().Any(packLine => packLine.Containers.Cast<ForwardingContainer>().Any(container => container.JC_JSB_SupplierBooking.IsValid));
		}

		bool ShipmentHasPackLines()
		{
			return parent.OuterPackLines.Cast<ForwardingPackLine>().Any();
		}

		bool ShipmentHasInvoiceLines()
		{
			return parent.Declarations.Cast<BaseJobDeclaration>().Any(declaration => declaration.Invoices != null && declaration.Invoices.Count > 0);
		}

		bool ShipmentHasTooManyPackLines()
		{
			return parent.OuterPackLines.Cast<ForwardingPackLine>().Count() > 20;
		}

		bool HasMenuItemAnyDataContextOfGivenList(IStmMenuItem menuItem, params ZString[] dataContextList)
		{
			return menuItem
				?.Documents
				.OfType<IStmMenuTemplatePivot>()
				.Select(p => p.Template)
				.Any(t => dataContextList.Contains(t.SO_DataContext))
				?? false;
		}

		public override IEnumerable<ICommand> GetCustomCommands(string dataContext)
		{
			return dataContext switch
			{
				DataContext.BookingRequest => GetBookingRequestCustomCommands(dataContext),
				DataContext.CargoControlAndTransit => new[] { new SendWithdrawCargoControlAndTransitCommand() },
				DataContext.HouseBill => GetHouseBillCustomCommands(dataContext),
				DataContext.ILGatePassMovement => GetGatepassMovementCommands(),
				DataContext.ILDeliveryOrder => GetDeliveryOrderCommands(),
				string context when (CertificateOfOriginConstants.DataContexts.Contains(context)) => new[] { new OpenCOOrderFormCommand() },
				_ => base.GetCustomCommands(dataContext),
			};

			CustomCommand[] GetGatepassMovementCommands()
			{
				var gatepassMovementProvider = parent.GatePassMovementProvider;
				return new CustomCommand[] { new ILGPMSendMessageCommand(gatepassMovementProvider), new ILGPMSendWithdrawalMessageCommand(gatepassMovementProvider), new ILGPMResetToOriginalMessageCommand(gatepassMovementProvider) };
			}

			CustomCommand[] GetDeliveryOrderCommands()
			{
				var deliveryOrderProvider = parent.DeliveryOrderProvider;
				return new CustomCommand[] { new ILDLOSendMessageCommand(deliveryOrderProvider), new ILDLOSendWithdrawalMessageCommand(deliveryOrderProvider), new ILDLOResetToOriginalMessageCommand(deliveryOrderProvider) };
			}
		}

		IEnumerable<ICommand> GetBookingRequestCustomCommands(string dataContext)
		{
			var result = new List<ICommand>();
			if (parent.Consols.Any(c => c.MessageHasBeenSentAndNoWithdrawAcceptedOrResetToOriginal(ConsolDocumentNames.BookingRequest)))
			{
				result.Add(DisabledCommand.SendMessage);
			}

			if (!parent.MessageHasBeenSent(ShipmentDocumentNames.BookingRequest))
			{
				result.Add(DisabledCommand.SendWithdrawal);
				result.Add(DisabledCommand.ResetToOriginal);
			}

			return result.Any() ? result : base.GetCustomCommands(dataContext);
		}

		IEnumerable<ICommand> GetHouseBillCustomCommands(string dataContext)
		{
			var result = new List<ICommand>();
			if (parent.JS_HouseBillOfLadingType != Core.Constants.HouseBillOfLadingTypes.Code.FIATAHBL
				&& parent.IsEditingElectronicBOL)
			{
				result.Add(new PublishHouseBillCommand(parent));
			}

			return result.Any() ? result : base.GetCustomCommands(dataContext);
		}

		public override string GetMessageBroker()
		{
			return parent.IsEditingElectronicBOL
				? EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface
				: EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
		}

		public override bool ShouldUseDraftWatermark(IDocument document)
		{
			return document.DataContext == DataContext.HouseBill
				&& FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration
				&& parent.IsEditingElectronicBOL;
		}
	}
}
