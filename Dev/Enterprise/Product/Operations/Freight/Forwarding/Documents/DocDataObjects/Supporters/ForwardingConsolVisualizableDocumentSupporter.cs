using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ForwardingConsolVisualizableDocumentSupporter : ForwardingVisualizableDocumentSupporter<ForwardingConsol>, IPrintEventsProcessorProvider
	{
		public ForwardingConsolVisualizableDocumentSupporter(ForwardingConsol consol)
			: base(consol)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.MaintainConsolCustomiseForms;

		public IPrintEventsProcessor GetPrintEventsProcessor(IDocument document)
		{
			var dataContext = document?.DataContext;
			var isSendMessageAsPDF = IsSendMessageAsPDF(dataContext);

			switch (dataContext)
			{
				case DataContext.BookingRequest:
					return new BookingRequestEventProcessor(parent, document, isSendMessageAsPDF);

				case DataContext.ShippingInstruction:
					return new ShippingInstructionEventProcessor(parent, document, isSendMessageAsPDF);

				case DataContext.ShippingOrder:
					return new ShippingOrderMessageEventProcessor(parent, document, isSendMessageAsPDF);
			}

			return null;
		}

		#region GetMessageEventsProcessor

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable document name")]
		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document)
		{
			var dataContext = document?.DataContext;
			var isSendMessageAsPDF = IsSendMessageAsPDF(dataContext);

			switch (dataContext)
			{
				case DataContext.VerifiedGrossMass:
					return new VGMMessageEventProcessor(parent, document);

				case DataContext.UXML when string.Compare(document.Name, "Verified Gross Container Weight", System.StringComparison.OrdinalIgnoreCase) == 0:
					return new LegacyVGMMessageEventProcessor(parent, document);

				case DataContext.BookingRequest:
					return new BookingRequestEventProcessor(parent, document, isSendMessageAsPDF);

				case DataContext.ShippingInstruction:
					return new ShippingInstructionEventProcessor(parent, document, isSendMessageAsPDF);

				case DataContext.ShippingOrder:
					return new ShippingOrderMessageEventProcessor(parent, document, isSendMessageAsPDF);

				case DataContext.BECertifiedPickup:
					return new BE.CertifiedPickupMessageEventsProcessor(parent, document);

				case DataContext.TMiningSecureContainerRelease:
					return new SecureContainerReleaseMessageEventsProcessor();

				case DataContext.ExportPreAdviceNotification:
					return new ExportPreAdviceNotificationMessageEventProcessor(parent, document);
			}

			return null;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters)
		{
			switch (documentName)
			{
				case ConsolDocumentNames.BookingRequest:
				case ConsolDocumentNames.ShippingInstruction:
					return ConsolDocumentDataStoreNames.SeaBookingRequest2;

				case ConsolDocumentNames.FormOfUndertakingForInterRAAWBHandling:
				case ConsolDocumentNames.DeclarationOfExportConsignmentBulk:
				case ConsolDocumentNames.RegulatedAgentAviationSecurityDeclaration:
				case ConsolDocumentNames.DeclarationOfExportConsignmentPrepackedUnit:
					return ConsolDocumentDataStoreNames.CargoSecurityDeclaration;

				case ConsolDocumentNames.ExportNotification:
					return ConsolDocumentDataStoreNames.PortbaseExportNotification;

				case ConsolDocumentNames.ImportNotification:
					return ConsolDocumentDataStoreNames.PortbaseImportNotification;

				case ConsolDocumentNames.ContainerLoadPlan:
					return ConsolDocumentDataStoreNames.ContainerLoadPlan;

				case ConsolDocumentNames.ShippingOrder:
					return ConsolDocumentDataStoreNames.ShippingOrder;

				case ConsolDocumentNames.ETerminalReleaseManifest:
					return ConsolDocumentDataStoreNames.ETerminalReleaseManifest;

				case ConsolDocumentNames.VerifiedGrossContainerWeight:
					return ConsolDocumentDataStoreNames.ContainerGrossWeightVerification;

				case ConsolDocumentNames.AdvancedManifest:
					if (eventParameters?.Location.HasValue ?? false)
					{
						var location = eventParameters.Location.Value.SubstringSafe(0, 2);

						switch (location)
						{
							case Core.Constants.CountryCodes.Brazil:
								return ConsolDocumentDataStoreNames.AdvancedManifestBR;

							case Core.Constants.CountryCodes.UnitedStates:
								return ConsolDocumentDataStoreNames.AdvancedManifestUS;
						}
					}

					return string.Empty;

				case DocumentNames.CargoDuesImport:
				case DocumentNames.CargoDuesImportQuotation:
					return ConsolDocumentDataStoreNames.CargoDuesImport;
				case DocumentNames.CargoDuesExport:
				case DocumentNames.CargoDuesExportQuotation:
					return ConsolDocumentDataStoreNames.CargoDuesExport;
				case DocumentNames.CargoDuesLoadCoastwise:
				case DocumentNames.CargoDuesLoadCoastwiseQuotation:
					return ConsolDocumentDataStoreNames.CargoDuesLoadCoastwise;
				case DocumentNames.CargoDuesDischargeCoastwise:
				case DocumentNames.CargoDuesDischargeCoastwiseQuotation:
					return ConsolDocumentDataStoreNames.CargoDuesDischargeCoastwise;

				case FR.FrenchPortsConstants.DocumentNames.ProvisionalUnpackingListLPD:
					return ConsolDocumentDataStoreNames.ProvisionalUnpackingListLPD;
				case FR.FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ:
					return ConsolDocumentDataStoreNames.PortsContainerAdviceToBookingAMQ;
				case FR.FrenchPortsConstants.DocumentNames.OutturnReportCDM:
					return ConsolDocumentDataStoreNames.OutturnReportCDM;
				case FR.FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE:
					return ConsolDocumentDataStoreNames.FinalContainerManifestLDE;
				case FR.FrenchPortsConstants.DocumentNames.DTI:
					return ConsolDocumentDataStoreNames.DTI;
				case FR.FrenchPortsConstants.DocumentNames.DTE:
					return ConsolDocumentDataStoreNames.DTE;
				case FR.FrenchPortsConstants.DocumentNames.DOSImport:
					return ConsolDocumentDataStoreNames.DossierImport;
				case FR.FrenchPortsConstants.DocumentNames.DOSExport:
					return ConsolDocumentDataStoreNames.DossierExport;

				case BE.BelgianPortsConstants.DocumentNames.ExportNotification:
					return ConsolDocumentDataStoreNames.BEEDeskExportNotification;
				case BE.BelgianPortsConstants.DocumentNames.IFTDGNExport:
					return ConsolDocumentDataStoreNames.DangerousGoodsNotificationExport;
				case BE.BelgianPortsConstants.DocumentNames.IFTDGNImport:
					return ConsolDocumentDataStoreNames.DangerousGoodsNotificationImport;
				case BE.BelgianPortsConstants.DocumentNames.CPuReleaseRightAcceptDecline:
				case BE.BelgianPortsConstants.DocumentNames.CPuReleaseRightTransfer:
				case BE.BelgianPortsConstants.DocumentNames.CPuReleaseRightRevoke:
					return ConsolDocumentDataStoreNames.BECertifiedPickup;
				case DE.GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder:
					return ConsolDocumentDataStoreNames.DEAdvancedLogisticsPortOrder;

				case TMiningConstants.DocumentNames.TMiningSecureContainerReleaseTransfer:
				case TMiningConstants.DocumentNames.TMiningSecureContainerReleaseRevoke:
					return ConsolDocumentDataStoreNames.TMiningSecureContainerRelease;

				case ConsolDocumentNames.CGNExportNotification:
					return ConsolDocumentDataStoreNames.CGNExportNotification;

				case ConsolDocumentNames.ILGatePassMovement:
					return ConsolDocumentDataStoreNames.ILGatePassMovement;
			}

			return documentName;
		}

		public override IMessageLogCreator GetMessageLogCreator(IDocument document)
		{
			switch (document.DataContext)
			{
				case DataContext.ACASHouseChecklist:
					return new AdvancedReportMessageLogCreator(DocumentNames.AdvancedManifest, Core.Constants.CountryCodes.UnitedStates);

				case DataContext.CargoControlAndTransitHouseManifest:
					return new AdvancedReportMessageLogCreator(DocumentNames.AdvancedManifest, Core.Constants.CountryCodes.Brazil);

				case DataContext.FRPortsTrackingRequestTRC:
					return new FR.DemandeDeTracingMessageLogCreator(new FR.ForwardingConsolTRCDetailsProvider(parent));

				default:
					return null;
			}
		}

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions)
		{
			switch (document.DataContext)
			{
				case DataContext.ContainerLoadPlan:
					return new CN.ContainerLoadPlanMessagingExtensions(document);

				case DataContext.VerifiedGrossMass:
					return new VGMMessagingExtensions(document, parent, messageInstructions);

				case DataContext.ACASHouseChecklist:
					return new US.ACASHouseChecklistMessagingExtensions(document);

				case DataContext.CargoControlAndTransitHouseManifest:
					return new CargoControlAndTransitHouseManifestMessagingExtensions(parent);

				case DataContext.NLPortbaseImportNotification:
					return new NL.PortbaseImportNotificationMessagingExtensions(parent, messageInstructions);

				case DataContext.NLPortbaseExportNotification:
					return new NL.PortbaseExportNotificationMessagingExtensions(document, parent);

				case DataContext.BookingRequest:
				case DataContext.ShippingInstruction:
					return new CarrierMessagingExtensions(document, parent, messageInstructions);

				case DataContext.ShippingOrder:
					return new CN.ShippingOrderMessagingExtensions(document, parent, messageInstructions);

				case DataContext.BECertifiedPickup:
					return new BE.CertifiedPickupMessagingExtensions(document, parent);

				case DataContext.BEDangerousGoodsNotification:
					return new BE.DangerousGoodsNotificationMessagingExtensions(document, parent);

				case DataContext.FRFinalContainerManifestLDE:
					return new FR.FinalManifestMessagingExtensions(document, parent, messageInstructions);

				case DataContext.FRPortsIntegrationDossier:
					return new FR.DossierMessagingExtensions(document, parent, messageInstructions);

				case DataContext.FRPortsContainerAdviceToBookingAMQ:
					return new FR.ContainerAdviceToBookingMessagingExtensions(document, parent, messageInstructions);

				case DataContext.FRImportManifestLPD:
					return new FR.ImportManifestMessagingExtensions(document, parent, messageInstructions);

				case DataContext.FRContainerOutturnReportCDM:
					return new FR.OutturnReportMessagingExtensions(document, parent, messageInstructions);

				case DataContext.DEAdvancedLogisticsPortOrder:
					return new DE.AdvancedLogisticsPortOrderMessagingExtensions(document, parent);

				case DataContext.CargoDues:
					return new ZA.CargoDuesMessagingExtensions(document, parent);

				case DataContext.TMiningSecureContainerRelease:
					return new SecureContainerReleaseMessagingExtensions(document, parent);

				case DataContext.ExportPreAdviceNotification:
					return new ExportPreAdviceNotificationExtensions(document, parent);

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
					break;
			}
		}

		public override IEnumerable<ICommand> GetCustomCommands(string dataContext)
		{
			var isSendMessageAsPDF = IsSendMessageAsPDF(dataContext);

			switch (dataContext)
			{
				case DataContext.AirBookingRequest:
					return new ICommand[]
					{
						new SendAirBookingCommand(),
						new CancelAirBookingCommand(),
						new ResetToOriginalAirBookingCommand()
					};
				case DataContext.BookingRequest:
				case DataContext.ShippingInstruction:
					{
						var commands = new List<ICommand>();

						if (isSendMessageAsPDF)
						{
							commands.Add(new DeliveryOceanCarrierMessagingAsPDFCommand(parent, dataContext));
						}

						commands.Add(new CarrierMessageDeliverDocumentCommand(parent, isSendMessageAsPDF));

						if (ConsolCarrierShipperReferenceNumberCalculator.IsCRSNumberOverLimit(parent)
								&& (dataContext == DataContext.BookingRequest || dataContext == DataContext.ShippingOrder || !IsBookingSent()))
						{
							commands.Add(DisabledCommand.ResetToOriginal);
						}

						return commands.Count > 0
							? commands.ToArray()
							: base.GetCustomCommands(dataContext);
					}
				case DataContext.ShippingOrder:
					{
						var commands = new List<ICommand>();
						var carrier = parent.ShippingLine;

						if (isSendMessageAsPDF)
						{
							commands.Add(new DeliveryOceanCarrierMessagingAsPDFCommand(parent, dataContext));
						}

						commands.Add(new CarrierMessageDeliverDocumentCommand(parent, isSendMessageAsPDF));

						return commands.Count > 0
							? commands.ToArray()
							: base.GetCustomCommands(dataContext);
					}
				case DataContext.VerifiedGrossMass:
					{
						var commands = new List<ICommand>();

						if (isSendMessageAsPDF)
						{
							commands.Add(new DeliveryOceanCarrierMessagingAsPDFCommand(parent, dataContext));
						}

						commands.Add(new CarrierMessageDeliverDocumentCommand(parent, isSendMessageAsPDF));

						return commands.Count > 0
							? commands.ToArray()
							: base.GetCustomCommands(dataContext);
					}
				case DataContext.BookingConfirmation:
					return new ICommand[]
					{
						new UpdateJobCommand()
					};
				case DataContext.DEAdvancedLogisticsPortOrder:
					{
						var commands = new List<ICommand>();
						if (DE.AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(parent))
						{
							commands.Add(DisabledCommand.SendMessage);
						}
						if (DE.AdvancedLogisticsPortOrderMessagingHelper.MessageWithdrawDisabled(parent))
						{
							commands.Add(DisabledCommand.SendWithdrawal);
						}
						if (DE.AdvancedLogisticsPortOrderMessagingHelper.ResetToOriginalDisabled(parent))
						{
							commands.Add(DisabledCommand.ResetToOriginal);
						}

						return commands.Count > 0
							? commands.ToArray()
							: base.GetCustomCommands(dataContext);
					}

				case DataContext.BECertifiedPickup:
					return new ICommand[]
					{
						new BE.CertifiedPickupSendMessageCommand()
					};

				case DataContext.TMiningSecureContainerRelease:
					return new ICommand[]
					{
						new SecureContainerReleaseSendMessageCommand()
					};

				case DataContext.CargoControlAndTransitHouseManifest:
					return new ICommand[]
					{
						new SendWithdrawCargoControlAndTransitCommand()
					};

				case DataContext.ILGatePassMovement:
					var gatepassMovementProvider = parent.GatePassMovementProvider;
					return new ICommand[]
					{
						new ILGPMSendMessageCommand(gatepassMovementProvider), new ILGPMSendWithdrawalMessageCommand(gatepassMovementProvider), new ILGPMResetToOriginalMessageCommand(gatepassMovementProvider)
					};

				default:
					return base.GetCustomCommands(dataContext);
			}
		}

		bool IsBookingSent()
		{
			return parent.HasSentDocument(ConsolDocumentNames.BookingRequest) || parent.HasSentDocument(ConsolDocumentNames.ShippingOrder);
		}

		#region GetAdditionalData

		public override Either<string, object> GetAdditionalData(object obj, IStmMenuItem menuItem)
		{
			var contextTypes = GetContextTypes(menuItem);
			if (contextTypes == null)
			{
				return (object)null;
			}

			if (contextTypes.Contains(DataContext.VerifiedGrossMass))
			{
				var receivingDocumentMessage = GetReceivingDocumentsMessage(DataContext.VerifiedGrossMass);
				if (receivingDocumentMessage != null)
				{
					return receivingDocumentMessage;
				}

				return GetAdditionalData(ContainerSelectorMode.VGM);
			}
			else if (contextTypes.Contains(DataContext.ContainerLoadPlan))
			{
				return GetAdditionalData(ContainerSelectorMode.ContainerLoadPlan);
			}
			else if (contextTypes.Contains(DataContext.ACASHouseChecklist) && !parent.Shipments.Any())
			{
				return Res.GetString("8f140bb3-8053-4f95-8b6d-2753a65111a2", "The Consol has no Shipments.");
			}

			if (contextTypes.Contains(DataContext.MultimodalDangerousGoodsDeclaration))
			{
				var containers = parent.Containers.Cast<ForwardingContainer>()
					.Where(c => c.PackLines.Cast<ForwardingPackLine>().Any(p => p.UNDGs.Any())).ToArray();

				if (containers.Length == 0)
				{
					return NoPackLineUsingTheContainers;
				}

				var selector = ObjectFactory.Get<IContainerSelector>();
				var selectedContainers = selector.SelectContainers(containers.ToArray(), ContainerSelectorMode.PrintSingle);

				if (selectedContainers.IsLeft)
				{
					return selectedContainers.Left;
				}

				return selectedContainers
					.Right
					.OfType<ForwardingContainer>()
					.First();
			}

			if (contextTypes.Contains(DataContext.FRPortsContainerAdviceToBookingAMQ))
			{
				return GetAdditionalData(ContainerSelectorMode.AMQ);
			}

			if (contextTypes.Contains(DataContext.FRContainerOutturnReportCDM))
			{
				return GetAdditionalData(ContainerSelectorMode.CDM);
			}

			if (contextTypes.Contains(DataContext.FRFinalContainerManifestLDE))
			{
				return GetAdditionalData(ContainerSelectorMode.LDE);
			}

			if (contextTypes.Contains(DataContext.FRImportManifestLPD))
			{
				return GetAdditionalData(ContainerSelectorMode.LPD);
			}

			if (contextTypes.Contains(DataContext.ExportPreAdviceNotification))
			{
				return GetAdditionalData(ContainerSelectorMode.ExportPreAdviceNotification);
			}

			if (contextTypes.Contains(DataContext.BECertifiedPickup))
			{
				ContainerSelectorMode? containerSelectorMode = null;

				if (menuItem.SU_MenuName == BE.BelgianPortsConstants.CertifiedPickupMenuItemName.AcceptDecline)
				{
					containerSelectorMode = ContainerSelectorMode.CertifiedPickupAcceptDecline;
				}
				else if (menuItem.SU_MenuName == BE.BelgianPortsConstants.CertifiedPickupMenuItemName.Transfer)
				{
					containerSelectorMode = ContainerSelectorMode.CertifiedPickupTransfer;
				}
				else if (menuItem.SU_MenuName == BE.BelgianPortsConstants.CertifiedPickupMenuItemName.Revoke)
				{
					containerSelectorMode = ContainerSelectorMode.CertifiedPickupRevoke;
				}

				if (containerSelectorMode.HasValue)
				{
					return GetAdditionalData(containerSelectorMode.Value, container =>
					{
						var currentContainerStatus = Business.CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(container);

						switch (containerSelectorMode)
						{
							case ContainerSelectorMode.CertifiedPickupAcceptDecline:
								return Business.CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(currentContainerStatus);
							case ContainerSelectorMode.CertifiedPickupTransfer:
								return Business.CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(currentContainerStatus);
							case ContainerSelectorMode.CertifiedPickupRevoke:
								return Business.CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(currentContainerStatus);
							default:
								return false;
						}
					});
				}
			}

			if (contextTypes.Contains(DataContext.BookingConfirmation))
			{
				return GetBookingConfirmationAdditionalData();
			}

			if (contextTypes.Contains(DataContext.BookingRequest))
			{
				DisplayEnablePackageGroupingMessage(DataContext.BookingRequest);
				return GetBookingRequestAdditionalData(DataContext.BookingRequest);
			}

			if (contextTypes.Contains(DataContext.ShippingOrder))
			{
				DisplayEnablePackageGroupingMessage(DataContext.ShippingOrder);

				if (!parent.IsCoLoad)
				{
					var receivingDocumentMessage = GetReceivingDocumentsMessage(DataContext.ShippingOrder);
					if (receivingDocumentMessage != null)
					{
						return receivingDocumentMessage;
					}
				}
			}

			if (contextTypes.Contains(DataContext.ShippingInstruction))
			{
				DisplayEnablePackageGroupingMessage(DataContext.ShippingInstruction);

				var receivingDocumentMessage = GetReceivingDocumentsMessage(DataContext.ShippingInstruction);
				if (receivingDocumentMessage != null)
				{
					return receivingDocumentMessage;
				}
			}

			if (contextTypes.Contains(DataContext.TMiningSecureContainerRelease))
			{
				ContainerSelectorMode? containerSelectorMode = null;

				if (menuItem.SU_MenuName == TMiningConstants.SecureContainerReleaseMenuItemName.Transfer)
				{
					containerSelectorMode = ContainerSelectorMode.TMiningSecureContainerReleaseTransfer;
				}
				else if (menuItem.SU_MenuName == TMiningConstants.SecureContainerReleaseMenuItemName.Revoke)
				{
					containerSelectorMode = ContainerSelectorMode.TMiningSecureContainerReleaseRevoke;
				}

				if (containerSelectorMode.HasValue)
				{
					return GetAdditionalData(containerSelectorMode.Value, container =>
					{
						var currentContainerStatus = Business.SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(container);

						switch (containerSelectorMode)
						{
							case ContainerSelectorMode.TMiningSecureContainerReleaseTransfer:
								return Business.SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(currentContainerStatus);
							case ContainerSelectorMode.TMiningSecureContainerReleaseRevoke:
								return Business.SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(currentContainerStatus);
							default:
								return false;
						}
					});
				}
			}

			if (contextTypes.Contains(DataContext.DraftHouseBill))
			{
				return GetDraftBillOfLadingAdditionalData();
			}

			if (contextTypes.Contains(DataContext.CargoControlAndTransitHouseManifest))
			{
				return GetCargoControlAndTransitHouseManifestAdditionalData();
			}

			if (contextTypes.Contains(DataContext.CMRConsignmentNote))
			{
				return GetCMRConsignmentNoteAdditionalData();
			}

			if (IsILGatePassMovement(menuItem))
			{
				if (!IsReceivingAgentVATValid(parent, CountryCodes.Israel))
				{
					return Res.GetString("4511BEEF-A900-4DBC-927F-669D37BB9045", "Consol Receiving Agent VAT # is not configured for country IL, update Organization – Config tab");
				}

				if (parent.JK_ConsolMode != ContainerModes.Groupage)
				{
					return Res.GetString("E700FF29-C7EE-469A-A143-F83735E614AC", "Consol Container Mode doesn't match, message must be sent from the relevant Shipment");
				}
			}

			return (object)null;
		}

		Either<string, object> GetAdditionalData(ContainerSelectorMode containerSelectorMode, Func<ForwardingContainer, bool> predicate = null)
		{
			var containers = parent
				.Containers
				.OfType<ForwardingContainer>()
				.Where(predicate ?? (_ => true))
				.ToArray();

			if (!containers.Any())
			{
				return NoApplicableContainersMessage;
			}

			var selector = ObjectFactory.Get<IContainerSelector>();
			var selectedContainers = selector.SelectContainers(containers, containerSelectorMode);

			if (selectedContainers.IsLeft)
			{
				return selectedContainers.Left;
			}

			if (!selectedContainers.Right.Any())
			{
				return NoApplicableContainersMessage;
			}

			return selectedContainers
				.Right
				.OfType<ForwardingContainer>()
				.ToArray();
		}

		Either<string, object> GetBookingConfirmationAdditionalData()
		{
			if (parent.GetBookingConfirmationUniversalXml() is UniversalShipment usxml)
			{
				return usxml;
			}

			return Res.GetString("decc4650-8fc7-403c-b138-b603fa4c08fc", "Data required for Booking Confirmation not yet received from the Carrier.");
		}

		Either<string, object> GetBookingRequestAdditionalData(string dataContext)
		{
			var shipmentsNeedConfirmMWR = parent.Shipments.OfType<ForwardingShipment>().Where(shipment => shipment.IsWaitingWithdrawConfirmation(ShipmentDocumentNames.BookingRequest)).ToArray();
			if (shipmentsNeedConfirmMWR.Any())
			{
				var shippmentNumbers = string.Join(", ", shipmentsNeedConfirmMWR.Select(s => s.JS_UniqueConsignRef));
				return Res.GetString("378b2772-1ec3-4572-bb9f-968bcf71812f", @"A confirmation for the Withdrawal/Cancellation of the Booking Request for Shipment {0} has not yet been received. The Consol Booking Request can be generated after the confirmation has been received.", shippmentNumbers);
			}

			var shipmentsHadSentBR = parent.Shipments.OfType<ForwardingShipment>().Where(shipment => shipment.MessageHasBeenSentAndNoWithdrawAcceptedOrResetToOriginal(ShipmentDocumentNames.BookingRequest)).ToArray();
			if (shipmentsHadSentBR.Any())
			{
				var shippmentNumbers = string.Join(", ", shipmentsHadSentBR.Select(s => s.JS_UniqueConsignRef));
				return Res.GetString("97a1415e-67c2-4c64-bab5-cd88d9a062ef", @"A Booking Request has been sent from Shipment {0}. To send a Booking Request from this Consol, either:
	1. Withdraw/Cancel the Booking Request/s from the Shipment/s or;
	2. Reset Shipment/s Booking Request to Original and advise NVOCC accordingly.", shippmentNumbers);
			}

			var receivingDocumentMessage = GetReceivingDocumentsMessage(dataContext);
			if (receivingDocumentMessage != null)
			{
				return receivingDocumentMessage;
			}

			return (object)null;
		}

		Either<string, object> GetDraftBillOfLadingAdditionalData()
		{
			if (parent.GetDraftBillOfLadingUniversalXml() is UniversalShipment usxml)
			{
				return usxml;
			}

			return Res.GetString("E968A96A-78F1-4E78-AE5F-02934E8FA793", "Data required for Draft Bill of Lading not yet received from the Carrier.");
		}

		Either<string, object> GetCargoControlAndTransitHouseManifestAdditionalData()
		{
			if (parent.IsDirect)
			{
				return Res.GetString("63E8CA7B-1EC6-42E1-8E15-1390C9EDB2AD", "Advanced Manifest is not available from Consolidations with type DRT.");
			}

			return (object)null;
		}

		Either<string, object> GetCMRConsignmentNoteAdditionalData()
		{
			if(IsConsolEmpty(parent))
			{
				return Res.GetString("f52c02e0-ef0c-11ef-a042-cca18d9feb45", "Consol does not contain any shipments or containers.");
			}

			var containers = parent
				.Containers
				.OfType<ForwardingContainer>()
				.ToArray();

			if (containers.Any())
			{
				if (containers.Length == 1)
				{
					return containers.First();
				}

				var selector = ObjectFactory.Get<IContainerSelector>();
				var selectedContainers = selector.SelectContainers(containers, ContainerSelectorMode.CMRConsignmentNote);

				if (selectedContainers.IsLeft)
				{
					return selectedContainers.Left;
				}

				if (!selectedContainers.Right.Any())
				{
					return NoApplicableContainersMessage;
				}

				return selectedContainers
					.Right
					.OfType<ForwardingContainer>()
					.ToArray().FirstOrDefault();
			}

			return (object)null;

			bool IsConsolEmpty(ForwardingConsol parent)
			{
				return !parent.Shipments.Any() && !parent.Containers.Any();
			}
		}

		object GetReceivingDocumentsMessage(string dataContext)
		{
			if (IsSendMessageAsPDF(dataContext))
			{
				var menuName = GetMenuName(dataContext);
				var result = Globals.Message.Show(Res.GetString("c8e97415-884c-4164-9f83-fe9992c0e6bc",
					"This {0} does not support electronic messaging. Do you want to send this {1} as a PDF via email?", parent.IsCoLoad ? "Co-loader" : Res.GetString("0566c0ec-b2e3-49ce-b1c3-6debab3b3b85", "Carrier"), menuName),
					$"No electronic {menuName}", ZMessageBoxButtons.OKCancel, ZDialogResult.Cancel);

				if (result != ZDialogResult.OK)
				{
					return string.Empty;
				}
			}

			return null;
		}

		void DisplayEnablePackageGroupingMessage(string dataContext)
		{
			if (!FreightDataRegistry.Instance.EnablePackageGrouping.Value)
			{
				var menuName = GetMenuName(dataContext);
				var messageBody = Res.GetString("c842aa1e-21fb-457d-ac71-2a88de8a641a", @"You are currently using the older version of the {0} form which will be phased out on the 31/01/2025. Please enable
					'Registry > Freight > Consolidations > Ocean Carrier Messaging > Enable Package Grouping' which will activate the new form and
					ensure electronic messages are submitted to carriers successfully.
					Please refer to 'How-to Configure Package Grouping for Ocean Carrier Messaging forms' with the hyperlink for more information.", menuName);
				var messageLink = "https://wisetechacademy.com/search?quickstart=0dc90ff2-bb23-4776-b0fc-f51fcc1322cb";

				using (var form = ObjectFactory.Get<IInformationHyerLinkForm>(nameof(IInformationHyerLinkForm), messageBody, messageLink))
				{
					form.ShowDialogAndGetResult();
				}
			}
		}

		string GetMenuName(string dataContext)
		{
			switch (dataContext)
			{
				case DataContext.BookingRequest:
					return Res.GetString("06b0c166-12b0-4191-8e04-b1bbaed6ccc1", "Booking Request");
				case DataContext.ShippingOrder:
					return Res.GetString("c15c9eda-4d8c-461b-968f-2d8c1ac872df", "Shipping Order");
				case DataContext.VerifiedGrossMass:
					return Res.GetString("1ec67956-1bc4-4ba8-a292-0038a1911f2c", "Verified Gross Container Weight");
				case DataContext.ShippingInstruction:
					return Res.GetString("771242b7-e2fb-4277-9334-ef1baf270145", "Shipping Instruction");
				default:
					return string.Empty;
			}
		}

		bool IsSendMessageAsPDF(string menuName)
		{
			switch (menuName)
			{
				case DataContext.BookingRequest:
				case DataContext.ShippingInstruction:
				case DataContext.VerifiedGrossMass:
					var carrier = parent.IsCoLoad ? parent.Creditor : parent.ShippingLine;
					return (carrier?.ShippingLine) != null && !IsShippingLineRelevantMessageOptionEnabled(menuName, carrier);

				case DataContext.ShippingOrder:
					carrier = parent.CarrierBookingAgent ?? parent.ShippingLine;
					return !IsShippingLineRelevantMessageOptionEnabled(menuName, carrier) && !parent.IsCoLoad;
			}

			return false;
		}

		bool IsShippingLineRelevantMessageOptionEnabled(string menuName, OrgHeader carrier)
		{
			if ((carrier?.ShippingLine) != null)
			{
				var shippingLine = carrier.ShippingLine;
				switch (menuName)
				{
					case DataContext.BookingRequest:
						return shippingLine.RSL_BookingRequestAvailable;
					case DataContext.ShippingInstruction:
						return shippingLine.RSL_ShippingInstructionAvailable;
					case DataContext.ShippingOrder:
						return shippingLine.RSL_ShippingOrderAvailable;
					case DataContext.VerifiedGrossMass:
						return shippingLine.RSL_VerifiedGrossContainerWeightAvailable;
				}
			}
			return false;
		}

		bool IsILGatePassMovement(IStmMenuItem menuItem) => HasMenuItemAnyDataContextOfGivenList(menuItem, DataContext.ILGatePassMovement);

		bool HasMenuItemAnyDataContextOfGivenList(IStmMenuItem menuItem, params ZString[] dataContextList)
		{
			return menuItem
				?.Documents
				.OfType<IStmMenuTemplatePivot>()
				.Select(p => p.Template)
				.Any(t => dataContextList.Contains(t.SO_DataContext))
				?? false;
		}

		bool IsReceivingAgentVATValid(ForwardingConsol consol, string countryCode)
			=> consol.ReceivingForwarder?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, countryCode) is ZString customsRegNo && !customsRegNo.IsEmpty;

		string NoApplicableContainersMessage => Res.GetString("3fc859cb-01a3-4a10-bb31-cb044f4080c6", "There are no applicable containers.");
		string NoPackLineUsingTheContainers => Res.GetString("e08f19af-30bb-4a62-8f91-3ab6336f5156", "There are no pack lines using the containers.");

		ZString[] GetContextTypes(IStmMenuItem menuItem)
		{
			return menuItem
						?.Documents
						.OfType<IStmMenuTemplatePivot>()
						.Select(p => p.Template.SO_DataContext)
						.ToArray();
		}

		#endregion
	}
}
