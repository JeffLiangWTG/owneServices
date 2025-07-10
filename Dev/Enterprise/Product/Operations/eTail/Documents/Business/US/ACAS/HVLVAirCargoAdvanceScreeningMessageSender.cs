using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using NotificationsHandler = Enterprise.DocumentVisualizer.Business.NotificationsHandler;
using ShipmentDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.ShipmentDocumentDataStoreNames;

namespace Enterprise.eTail.Documents.Business
{
	public class HVLVAirCargoAdvanceScreeningMessageSender : IHVLVAirCargoAdvanceScreeningMessageSender
	{
		public HVLVAirCargoAdvanceScreeningMessageSender(ForwardingShipment loadedOnShipment)
			: this(loadedOnShipment, null)
		{
		}

		public HVLVAirCargoAdvanceScreeningMessageSender(ForwardingShipment loadedOnShipment, Action<string, int> progressUpdate)
		{
			NotificationsHandler = new NotificationsHandler();
			DocDataObjectUXmlWriter = ObjectFactory.Get<IForwardingDocDataObjectUXmlWriter>();
			Parameters = new DocDataObjectParameters(ShipmentDocumentNames.AdvancedCargoReport, ShipmentDocumentDataStoreNames.AdvancedCargoReportUS);
			ProgressUpdate = progressUpdate;

			if (loadedOnShipment != null)
			{
				LoadedOnShipment = loadedOnShipment;
				DocDataObjectProvider = new HVLVConsignmentDocDataObjectProvider(loadedOnShipment);
				CommunicationsMode = GetEDICommunicationsMode(loadedOnShipment.Factory);
			}
		}

		readonly NotificationsHandler NotificationsHandler;
		readonly IForwardingDocDataObjectUXmlWriter DocDataObjectUXmlWriter;
		readonly DocDataObjectParameters Parameters;
		readonly Action<string, int> ProgressUpdate;
		readonly ForwardingShipment LoadedOnShipment;
		readonly HVLVConsignmentDocDataObjectProvider DocDataObjectProvider;
		readonly EDICommunicationsMode CommunicationsMode;

		public
#if DEBUG
		virtual
#endif
		bool TrySendACASReports(ACASReportAction reportAction, out string message)
		{
			var consignments = GetConsignmentsToSendByAction(reportAction);

			message = string.Empty;
			var result = true;

			var totalConsignmentsCount = consignments.Count();
			var processedCount = 0;
			var succceedCount = 0;
			foreach (var consignment in consignments)
			{
				var deliverResult = SendACASReportCore(consignment, reportAction);

				if (deliverResult != null && deliverResult.Succeeded)
				{
					succceedCount++;
					OnSendACASReportSucceeded(consignment, reportAction);
				}
				else
				{
					message += deliverResult?.FailureReason + "\r\n";
					result = false;
					break;
				}

				ProgressUpdate?.Invoke(Res.GetString("5E983EB8-38EF-4BB8-B550-55F1EBE844DA", "[{0} / {1}] Consignments processed",
				++processedCount,
				totalConsignmentsCount),
				(int)(processedCount * 100F / totalConsignmentsCount));
			}

			ProgressUpdate?.Invoke(Res.GetString("B8CBB457-13B0-42D7-B2A4-D012108D7FDF", "All Consignments processed, saving changes"), 100);
			message += GetReportSentNotification(succceedCount);

			LoadedOnShipment.Factory.Save();
			return result;
		}

		public bool TrySendACASReport(IHVLVConsignment consignment, ACASReportAction reportAction, out string errorMessage)
		{
			var singleConsignment = consignment as HVLVConsignment;
			var result = false;
			errorMessage = string.Empty;

			var deliverResult = SendACASReportCore(singleConsignment, reportAction);
			if (deliverResult != null && deliverResult.Succeeded)
			{
				OnSendACASReportSucceeded(singleConsignment, reportAction);
				result = true;
			}
			else
			{
				errorMessage = deliverResult?.FailureReason;
			}

			LoadedOnShipment.Factory.Save();
			return result;
		}

#if DEBUG
		public virtual
#endif
		IDeliveryResult SendACASReportCore(HVLVConsignment consignment, ACASReportAction reportAction)
		{
			try
			{
				if (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSACAS.Value)
				{
					LoadedOnShipment.Factory.ServiceContainer.AddService(new NonWesternEuropeanCharactersRemovalService());
				}

				var docDataObject = GetDocDataObject(consignment, DocDataObjectProvider, reportAction);

				if (docDataObject != null)
				{
					var xmlNamespace = GetXmlNamespace(reportAction);
					var dataObjectUXmlWriter = new DocDataObjectUXmlWriter(docDataObject, xmlNamespace) as IXmlWriter;

					var streamWrapper = new DeliveryStreamWrapperUXML(EntityInfo.New(consignment), docDataObject, dataObjectUXmlWriter, xmlNamespace);
					return DeliverACASReport(consignment, CommunicationsMode, streamWrapper);
				}

				return null;
			}
			finally
			{
				LoadedOnShipment.Factory.ServiceContainer.RemoveService<NonWesternEuropeanCharactersRemovalService>();
			}
		}

#if DEBUG
		public
#endif

		ITopLevelDataObject GetDocDataObject(HVLVConsignment consignment, HVLVConsignmentDocDataObjectProvider docDataObjectProvider, ACASReportAction reportAction)
		{
			var docDataObject = docDataObjectProvider.GetDocDataObject(consignment, DocumentVisualizer.Integration.DataContext.HVLVConsignment, Parameters);
			if (docDataObject != null)
			{
				var document = new EmptyDocument(ShipmentDocumentNames.AdvancedCargoReport, Freight.Forwarding.Documents.DataContext.AirCargoAdvanceScreening, docDataObject.MakeDynamic());
				var dataObject = DocDataObjectUXmlWriter.GetDataObject(DefaultDataObjectWriterStrategy.Instance, document);

				var purposeCode = GetDocumentPurposeCode(reportAction);
				dataObject.DataContext.SetDocumentaryOverride(ACASDocumentaryName, purposeCode, null, true, 1, 1);
				return dataObject;
			}

			return null;
		}

		string GetReportSentNotification(int consignmentsCount)
		{
			return consignmentsCount == 1
						? ResString.GetMultilingualString("016561f0-2abc-4fd2-89cd-2511f90a550e", "1 report successfully sent.")
						: ResString.GetMultilingualString("718f7d0a-0639-4e7a-8674-56fbd7c3632f", "{0} reports successfully sent.", consignmentsCount);
		}

		IDeliveryResult DeliverACASReport(HVLVConsignment consignment, EDICommunicationsMode mode, DeliveryStreamWrapperUXML streamWrapper)
		{
			var delivery = new EDIMessageDelivery();

			var deliveryContext = new DeliveryContext(consignment.Factory)
			{
				ParentInfo = EntityInfo.New(consignment),
				ApplicationCode = ApplicationCodeList.Codes.AirCargoAdvanceScreening,
				MessageTypeCode = EDIMessageTypeList.Codes.XDC,
				MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				Notifications = NotificationsHandler,
			};

			return delivery.Deliver(deliveryContext, mode, streamWrapper);
		}

		EDICommunicationsMode GetEDICommunicationsMode(BusinessObjectFactory factory)
		{
			var stmTemplate = factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_DataContext, Freight.Forwarding.Documents.DataContext.AirCargoAdvanceScreening));
			var templateBizObj = factory.Load<VisualizerTemplate>(stmTemplate.PK.ToGuid());

			var res = templateBizObj?
					.GetFlexCelWorksheet()
					.CreateTemplate();

			var template = res.Value.Right;
			var eHubClientID = ((IStandardTemplate)template).GetEHubClientID();

			var mode = factory.New<EDICommunicationsMode>();
			mode.EK_Module = WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode;
			mode.EK_Destination = eHubClientID;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;

			return mode;
		}

		public string ValidateACASReportBasicRequirments()
		{
			var sendersAcasCode = GetACASCode(GlbBranch.CurrentBranch.OrgProxy) ?? GetACASCode(GlbCompany.CurrentCompany.OrgProxy);
			if (sendersAcasCode == null || sendersAcasCode.OK_CustomsRegNo.IsEmpty)
			{
				return Res.GetString("6223fd48-7fe4-4e43-8043-140231618a17", "Sender's ACAS code is required for ACAS messaging. {0}", AirCargoAdvanceScreeningBuilder.ACASCodeEmptyErrorMessage);
			}

			var consol = LoadedOnShipment.ArrivalConsol;
			if ((consol?.ArrivalCTOAddress != null) && consol.ArrivalCTOAddress?.CustomsCodes?.GetOrgCusCodeObjectForCodeTypeAndCountry(OrgCusCode.USACodeTypes.FIRMSCode, CountryCodes.UnitedStates) == null)
			{
				return AirCargoAdvanceScreeningBuilder.CTOFIRMSCodeEmptyErrorMessage;
			}

			return string.Empty;
		}

		OrgCusCode GetACASCode(OrgHeader org)
		{
			return org?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.USACodeTypes.ACASOriginatorCode, CountryCodes.UnitedStates);
		}

		void OnSendACASReportSucceeded(HVLVConsignment consignment, ACASReportAction reportAction)
		{
			UpdateACASMessageStatusAfterSending(consignment);
			SetSecurityFilingUsage(consignment);
			SetLastUsageCodeForAllItemsIfRequired(consignment, reportAction);
		}

		void UpdateACASMessageStatusAfterSending(HVLVConsignment consignment)
		{
			switch (consignment.HVC_ACASMessageStatus)
			{
				case HVLVACASMessageStatusList.Codes.AmendmentRequired:
					UpdateACASMessageStatusForConsignment(consignment, HVLVACASMessageStatusList.Codes.AmendmentSent);
					break;
				case HVLVACASMessageStatusList.Codes.AcknowledgementRequired:
					UpdateACASMessageStatusForConsignment(consignment, HVLVACASMessageStatusList.Codes.AcknowledgementSent);
					break;
				default:
					UpdateACASMessageStatusForConsignment(consignment, HVLVACASMessageStatusList.Codes.OriginalSent);
					break;
			}
		}

		void UpdateACASMessageStatusForConsignment(HVLVConsignment consignment, ZString newStatus)
		{
			consignment.HVC_ACASMessageStatus = newStatus;
		}

		static void SetSecurityFilingUsage(IHVLVSecurityFilingSource source)
		{
			source.SetSecurityFilingUsageDateTimeIfNeeded();
		}

		static void SetLastUsageCodeForAllItemsIfRequired(HVLVConsignment source, ACASReportAction reportAction)
		{
			if (reportAction == ACASReportAction.SendOriginal)
			{
				source.SetLastUsageCodeForAllItems(UsageCodes.ACAS);
			}
		}

		string GetXmlNamespace(ACASReportAction reportAction)
		{
			return reportAction == ACASReportAction.SendAcknowledgement
				? "/AcknowledgementOfHold/1"
				: "/AirCargoAdvanceScreening/1";
		}

		string GetDocumentPurposeCode(ACASReportAction reportAction)
		{
			return reportAction == ACASReportAction.SendOriginal ? MessagePurposes.Codes.Original : MessagePurposes.Codes.Amendment;
		}

		IEnumerable<HVLVConsignment> GetConsignmentsToSendByAction(ACASReportAction reportAction)
		{
			var consignments = DocDataObjectProvider.GetHVLVConsignmentsWithItemLoadedOnShipment();
			switch (reportAction)
			{
				case ACASReportAction.SendOriginal:
					consignments = consignments.Where(c => c.HVC_ACASMessageStatus == ZString.Empty);
					break;
				case ACASReportAction.SendAmendment:
					consignments = consignments.Where(c => c.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.AmendmentRequired);
					break;
				case ACASReportAction.SendAcknowledgement:
					consignments = consignments.Where(c => c.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.AcknowledgementRequired);
					break;
				default:
					break;
			}

			return consignments;
		}

		public ZString ACASDocumentaryName => ShipmentDocumentDataStoreNames.HVLVAdvancedCargoReportUS;
	}
}
