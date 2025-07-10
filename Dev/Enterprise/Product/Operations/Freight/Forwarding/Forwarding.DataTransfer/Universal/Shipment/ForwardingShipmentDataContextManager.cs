using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingShipmentDataContextManager : BaseShipmentDataContextManager<ForwardingShipment>, IEventTransformer, IDataContextCoordinator
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.ForwardingShipment; }
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.ShipmentExportDirectory.Value; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			var shipment = writeManager.Action?.ParentBO as ForwardingShipment;

			if (shipment != null && writeManager.ContentFilterManager?.EDIMessageContentFilter?.GetUniversalShipmentPrimaryDataSource() == EDIMessageContentPrimaryDataSource.Codes.Brokerage)
			{
				var declarationDataContextManager = ObjectFactory.New<IJobDeclarationDataContextManager>();
				var declaration = shipment.GetDeclaration();
				if (declaration != null)
				{
					declarationDataContextManager.Init(declaration);
					return declarationDataContextManager.GetShipmentDataObjectWriter(writeManager);
				}
			}

			return new ShipmentDataObjectWriter(writeManager, checkSubShipments: true, checkForParent: true);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (universalShipment.GetMatchingDataTarget(DataContextType.ForwardingConsol) != null)
			{
				return null; // Should let the consol context manager process this universal shipment and hence stopping the system from processing this twice.
			}
			else
			{
				bool isNVOCC = universalShipment.IsNVOCC();
				if (universalShipment.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master && !isNVOCC)
				{
					return new ConsolDataObjectReader(universalShipment, logger, factory, Helper);
				}
				else
				{
					return new ShipmentDataObjectReader(universalShipment, logger, factory, null, null, Helper);
				}
			}
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(o => o.Code == RecipientRoleType.FOR
							|| o.Code == RecipientRoleType.RAG
							|| o.Code == RecipientRoleType.SAG
							|| o.Code == RecipientRoleType.DAG
							|| o.Code == RecipientRoleType.PAG
							|| (o.Code == RecipientRoleType.NVO && o.ServiceCode == ServiceCodeType.SIN))
				&& (dataSources == null || !dataSources.Any(o => o.Type.GetValueOrDefault().EqualsIgnoringCase(nameof(DataContextType.ForwardingConsol))));
		}

		protected override IUniversalFreightHelper GetNewHelper()
		{
			return new UniversalForwardingHelper();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ForwardingShipmentEventParentFinder(factory, this, logger);
		}

		protected override bool CanUpdateLogParentFromEventCore(BusinessObject logParent, IXmlEventValueObject xmlEvent, out ZString failureReason)
		{
			if (logParent is ForwardingShipment shipment
				&& xmlEvent != null
				&& !xmlEvent.IsCancelled
				&& !xmlEvent.IsEstimate)
			{
				if (xmlEvent.EventType == Events.PickupCartageCompleteFinalisedCode
					&& shipment.DocsAndCartage.Validation.IsActualPickupCartageCompletedInTheFuture(xmlEvent.EventTime.ToZDateTime()))
				{
					failureReason = Res.GetString("55292df2-9ac8-4649-ada3-656b22e14c6f", "The Actual PCF date {0} is detected in the future, and cannot be saved.", xmlEvent.EventTime);
					return false;
				}
				else if (xmlEvent.EventType == Events.DeliveryCartageCompleteFinalisedCode
					&& shipment.DocsAndCartage.Validation.IsActualDeliveryCartageCompletedInTheFuture(xmlEvent.EventTime.ToZDateTime()))
				{
					failureReason = Res.GetString("2dd905ab-58bf-49f1-b119-145ecd74ab27", "The Actual DCF date {0} is detected in the future, and cannot be saved.", xmlEvent.EventTime);
					return false;
				}
				else if (xmlEvent.EventType == Events.BillStatusUpdatedCode && FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration)
				{
					if (!GetAutoRejectionReasonForBoleroEvents(xmlEvent, shipment, out failureReason))
					{
						return false;
					}
				}
			}

			return base.CanUpdateLogParentFromEventCore(logParent, xmlEvent, out failureReason);
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);
			eventAdded.OnCO2eRejectionEvent(ParentBO);

			if (eventAdded.EventType.Value == Events.BillStatusUpdated.Code
				&& FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration)
			{
				ParentBO.OnEventsRelatedToOriginalBillNotes(eventAdded);
			}
		}

		protected override bool BeforeLinkToExistingBusinessObject(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipmentBO)
		{
			if (universalShipment.IsNVOCC()
				&& universalShipment.IsShippingInstructionMessage()
				&& universalShipment.IsLinkOnly()
				&& GetShipmentDataObjectReader(universalShipment, logger, factory) is ShipmentDataObjectReader boReader)
			{
				var rejectReason = boReader.GetReasonForNotAbleToUpdate(shipmentBO);
				if (!rejectReason.IsEmpty)
				{
					logger.Log(LogType.Error, Res.GetString("94d798e9-2dee-4f7c-897d-30bb9c5cd27b", "Cannot link {0} because:{1}{2}", nameof(ForwardingShipment), System.Environment.NewLine, rejectReason));

					return false;
				}
				else if (base.BeforeLinkToExistingBusinessObject(universalShipment, logger, factory, shipmentBO))
				{
					AddAdditionalEHubInterchangeReference(universalShipment, shipmentBO);

					var oldShipmentStatus = shipmentBO.JS_ShipmentStatus;

					ISupportDataImporting supportDataImporting = shipmentBO;
					supportDataImporting.IsImportingData = true;

					try
					{
						boReader.TurnBookingIntoShipment(shipmentBO);
						boReader.PopulateShipmentStatus(shipmentBO);
						AddAttachedDocumentCollection(universalShipment, logger, shipmentBO);
					}
					finally
					{
						supportDataImporting.IsImportingData = false;
					}

					logger.Log(LogType.Information, Res.GetString("2bbed519-49b7-4c23-8116-d005fb5b5f69", "Shipment status of Shipment {0} has been updated.", shipmentBO.JS_UniqueConsignRef));
					boReader.LogSTUEventForShipmentStatusChange(shipmentBO, oldShipmentStatus);

					return true;
				}
				else
				{
					return false;
				}
			}

			return base.BeforeLinkToExistingBusinessObject(universalShipment, logger, factory, shipmentBO);
		}

		void AddAdditionalEHubInterchangeReference(UniversalShipment universalShipment, ForwardingShipment shipmentBO)
		{
			var hir = universalShipment.AdditionalReferenceCollection?.FirstOrDefault(x => (x.Type?.Code ?? ZString.Empty) == CustomsReferenceNumberType.eHubInterchangeReference.HIR);
			if (hir != null && !string.IsNullOrEmpty(hir.ReferenceNumber))
			{
				var bookingConfirmationReference = shipmentBO.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR);
				if (bookingConfirmationReference == null)
				{
					bookingConfirmationReference = shipmentBO.Numbers.AddNew();
					bookingConfirmationReference.CE_RN_NKCountryCode = shipmentBO.CurrentCountryCode;
					bookingConfirmationReference.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
				}
				bookingConfirmationReference.CE_EntryNum = hir.ReferenceNumber.GetValueOrDefault();
			}
		}

		void AddAttachedDocumentCollection(UniversalShipment universalShipment, IXmlImportLogger logger, ForwardingShipment shipmentBO)
		{
			var attachedDocuments = universalShipment.AttachedDocumentCollection;

			if (attachedDocuments != null && attachedDocuments.Count > 0 && shipmentBO is IDocManagerSupport docManagerSupport)
			{
				var attachedDocumentDataObjectReader = ObjectFactory.New<IAttachedDocumentDataObjectReader>();
				foreach (var attachedDocument in attachedDocuments)
				{
					if (attachedDocumentDataObjectReader.TryAddAttachedDocument(attachedDocument, logger, docManagerSupport, out IeDoc eDoc))
					{
						shipmentBO.Logs.AddNew(AutoEvents.DocumentImported, eDoc.CreateReference());
					}
				}
			}
		}

		public EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, IStmALogParent logParent)
		{
			if (sourceEventValue.Code == AutoEvents.SubscriptionRequestedCode && sourceUniversalEvent != null)
			{
				return SubscriptionRequesteEventTransformer.Transform(sourceEventValue, sourceUniversalEvent);
			}

			var logParentContainer = logParent as ForwardingContainer;

			return logParentContainer != null
				? ForwardingContainerEventTransformer.Transform(sourceEventValue, sourceUniversalEvent, logParentContainer)
				: sourceEventValue;
		}

		bool GetAutoRejectionReasonForBoleroEvents(IXmlEventValueObject xmlEvent, ForwardingShipment shipment, out ZString failureReason)
		{
			failureReason = ZString.Empty;
			var universalEvent = xmlEvent as UniversalEvent;

			if (universalEvent == null || universalEvent.EventParameters == null || universalEvent.EventParameters.Department?.ToString() != ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry)
			{
				return true;
			}

			var eventType = universalEvent.EventParameters.Type?.ToString();

			if (eventType == Constants.BillStatusUpdatedTypes.Surrendered)
			{
				if (shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper)
				{
					failureReason = Res.GetString("200535d6-f87f-4463-915e-45c586342ee0", "Error Message: The Electronic Bill of Lading was switched to paper and Electronic Surrender Request is no longer accepted. Message rejected.");
				}
				else if (shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.Surrendered)
				{
					failureReason = Res.GetString("2322ffd2-164a-4f72-a2d3-e6b741326574", "Error Message: The Electronic Bill of Lading was already surrendered. Message rejected.");
				}
				else if (shipment.JS_ElectronicBillOfLadingStatus.IsEmpty)
				{
					failureReason = Res.GetString("640a4ad7-4c00-4a9d-b7a7-88cfc9eed53b", "Error Message: The Electronic Bill of Lading was not issued yet. Message rejected.");
				}
				else if (shipment.JS_ElectronicBillOfLadingReference != universalEvent.EventParameters.ReferenceNumber.GetValueOrDefault())
				{
					failureReason = Res.GetString("9c440701-fe07-4fce-8f35-b845f38b6474", "Error Message: The electronic Bill Identifier does not match with Reference Number. Message rejected.");
				}
			}
			else if (eventType == Constants.BillStatusUpdatedTypes.AmendmentRequested)
			{
				if (shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper)
				{
					failureReason = Res.GetString("1b62db7c-ae79-4f57-b75f-01c0716c783e", "Error Message: The Electronic Bill of Lading was switched to paper and amendments are no longer accepted. Message rejected.");
				}
				else if (shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.Surrendered)
				{
					failureReason = Res.GetString("3888b7be-051a-49da-902d-fc5b6528826f", "Error Message: The Electronic Bill of Lading was surrendered, and amendments are no longer accepted. Message rejected.");
				}
				else if (shipment.JS_ElectronicBillOfLadingStatus.IsEmpty)
				{
					failureReason = Res.GetString("ffb1d7d1-6d2c-45d9-a922-4cd59d9fbf9d", "Error Message: The Electronic Bill of Lading was not issued yet. Message rejected.");
				}
				else if (shipment.JS_ElectronicBillOfLadingReference != universalEvent.EventParameters.ReferenceNumber.GetValueOrDefault())
				{
					failureReason = Res.GetString("5ac554be-6c3d-4526-af1c-f40a37deb7bf", "Error Message: The electronic Bill Identifier does not match with Reference Number. Message rejected.");
				}
				else
				{
					var latestAmendmentRequestedEventLog = shipment.GetLatestAmendmentRequestedEHBLStatusEventLog();
					if (latestAmendmentRequestedEventLog != null && shipment.Logs.MostRecentLogByPostedTime(Events.MessageSent, log => log.SL_PostedTimeUtc > latestAmendmentRequestedEventLog.SL_PostedTimeUtc
								&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType)
								&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, out var department)
								&& department == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry && (eventType == Constants.BillStatusUpdatedTypes.OriginalBillSentForPublication || eventType == Constants.BillStatusUpdatedTypes.AmendmentDenied)) == null)
					{
						failureReason = Res.GetString("8977563e-dbab-4a1d-929f-9cded752e459", "Error Message: A previous Amendment Request was not processed yet. Message rejected.");
					}
				}
			}
			else if (eventType == Constants.BillStatusUpdatedTypes.SwitchedToPaper)
			{
				if (shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper)
				{
					failureReason = Res.GetString("c4d546f9-22ea-467a-96e0-c3e5d377ccf6", "Error Message: The Electronic Bill of Lading was already switched to paper. Message rejected.");
				}
				else if (shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.Surrendered)
				{
					failureReason = Res.GetString("7cbb2d55-1298-42bc-aaa3-778832f79bfa", "Error Message: The Electronic Bill of Lading was already surrendered. Message rejected.");
				}
				else if (shipment.JS_ElectronicBillOfLadingStatus.IsEmpty)
				{
					failureReason = Res.GetString("19a5cb2a-a485-423f-b7e3-b0971258e2e7", "Error Message: The Electronic Bill of Lading was not issued yet. Message rejected.");
				}
				else if (shipment.JS_ElectronicBillOfLadingReference != universalEvent.EventParameters.ReferenceNumber.GetValueOrDefault())
				{
					failureReason = Res.GetString("a634478d-2845-43ed-ae35-690d79f3abc9", "Error Message: The Electronic Bill Identifier does not match with Reference Number. Message rejected.");
				}
			}
			else if (eventType == Constants.BillStatusUpdatedTypes.OriginalBillPublished)
			{
				if (!shipment.JS_ElectronicBillOfLadingReference.IsEmpty && shipment.JS_ElectronicBillOfLadingReference == universalEvent.EventParameters.ReferenceNumber.GetValueOrDefault())
				{
					failureReason = Res.GetString("5a94295c-9d8a-45ce-a9ef-c63205265014", "Error Message: A previous 'Original Bill Published' Notification was already received and processed. Message Rejected.");
				}
			}

			return failureReason.IsEmpty;
		}

		public string GetUniqueContextIdentifier(IXmlEventValueObject xmlEvent)
		{
			return xmlEvent.EventType == AutoEvents.SubscriptionRequested.Code ? JobShipmentSchema.Constants.TableName : string.Empty;
		}
	}
}
