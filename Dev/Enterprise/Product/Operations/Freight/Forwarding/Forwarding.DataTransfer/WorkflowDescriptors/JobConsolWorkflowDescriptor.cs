using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.HelperClasses;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class JobConsolWorkflowDescriptor : WorkflowDescriptor
	{
		public JobConsolWorkflowDescriptor()
			: base()
		{
		}

		#region ID / Description

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.Consol.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.Consol.MultilingualDescription; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("1889ceb6-7fcc-41a8-908a-b3f0463a663a", "Transport Mode"), TransportModeList));
				list.Add(new ProcessTemplateSubType(Res.GetString("5becb76d-1192-4509-b605-375a4faecaaa", "Direction"), DirectionList));
				return list.ToArray();
			}
		}

		CodeDescriptionPairList TransportModeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("fdaf58d7-fc5d-4b55-9a65-6c773fe02d91", "All"));
				result.AddRange(new ConsolTransportModeCodeDescriptionPairList());
				return result;
			}
		}

		protected CodeDescriptionPairList DirectionList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("9b145309-ee4c-499b-933c-ee9e5062d700", "All"));
				result.AddPair(DirectionsContext.Import, Res.GetString("02bfea64-2bee-481d-b94d-cb2dd68b64da", "Import"));
				result.AddPair(DirectionsContext.Export, Res.GetString("0ae4e075-ed56-46a9-a6e4-7f0073737ba6", "Export"));
				result.AddPair(DirectionsContext.Domestic, Res.GetString("2fc16954-1dd7-493f-a9c8-bc3bd736e452", "Domestic"));
				result.AddPair(DirectionsContext.CrossTrade, Res.GetString("ee0d8a5a-a5f9-406a-8311-d1ab72aaec3d", "Cross Trade"));
				return result;
			}
		}

		#endregion

		#region Requires Client / Ports

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override ZString ClientName
		{
			get { return Res.GetString("366e62b2-c01d-4b81-a3ee-dfbaaedeefd7", "Carrier"); }
		}

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return false; }
		}

		public override bool RequiresDepartment
		{
			get { return false; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			if (trigger.IsForCountryOrTemplateDischargePortCountry((BusinessObject)parent, Core.Constants.CountryCodes.Australia))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging;

				if ((parent is ProcessTaskTemplate template && template.IsAir()) ||
					(parent is ForwardingConsol forwardingConsol && forwardingConsol.IsAir))
				{
					yield return WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn;
				}
			}
		}

		protected override IEnumerable<string> GetScheduleDeferredMessageSendTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			if (trigger.IsForCountryOrTemplateDischargePortCountry((BusinessObject)parent, Core.Constants.CountryCodes.Australia))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage;

				if ((parent is ProcessTaskTemplate template && template.IsAir()) ||
					(parent is ForwardingConsol forwardingConsol && forwardingConsol.IsAir))
				{
					yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage;
				}
			}
		}

		#endregion

		#region Conditions

		public override CodeDescriptionPairList GetConditionList1(ITemplateConditionalWorkflowItem workflowItem)
		{
			var result = new CodeDescriptionPairList(new JobConsolWorkflowCondition1CodeList());
			var trigger = workflowItem as IWorkflowTrigger;

			if (trigger != null &&
				trigger.TriggerEventCode != Events.Departure.Code &&
				trigger.TriggerEventCode != Events.Arrival.Code &&
				trigger.TriggerEventCode != Events.CutOffDate.Code &&
				trigger.TriggerEventCode != Events.CargoAvailable.Code &&
				trigger.TriggerEventCode != Events.StorageCommenced.Code &&
				trigger.TriggerEventCode != Events.ReceiptCommenced.Code &&
				!trigger.TriggerFieldName.StartsWith(JobConsolTransportSchema.Constants.Prefix + "_", StringComparison.OrdinalIgnoreCase))
			{
				result.RemoveCode(JobConsolWorkflowCondition1CodeList.Codes.MainTransport);
			}

			return result;
		}

		public override CodeDescriptionPairList GetConditionList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new JobConsolWorkflowCondition2CodeList();
		}

		#endregion

		#region ConditionValues

		public override TemplateConditionValueStyle GetCondition2ValueStyle(ITemplateConditionalWorkflowItem workflowItem)
		{
			switch (workflowItem.TemplateCondition2)
			{
				case JobConsolWorkflowCondition2CodeList.Codes.ReleaseType:
					return TemplateConditionValueStyle.DropDown;

				default:
					return base.GetCondition2ValueStyle(workflowItem);
			}
		}

		public override CodeDescriptionPairList GetConditionValueList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			if (workflowItem != null && workflowItem.TemplateCondition2 == JobConsolWorkflowCondition2CodeList.Codes.ReleaseType)
			{
				var types = FreightDataRegistry.Instance.ReleaseTypes.GetValue(workflowItem);
				return types.GetCodeDescriptionPairList();
			}

			return base.GetConditionValueList2(workflowItem);
		}

		#endregion

		#region EstimateDefaultedFromList

		public override CodeDescriptionPairList EstimateDefaultedFromList
		{
			get { return new ForwardingConsolEstimateDefaultedFromList(); }
		}

		#endregion

		#region Workflow Triggers

		/// <summary>
		/// Provides additional (specific to Consol) workflow trigger action types
		/// </summary>
		public static class JobConsolWorkflowDescriptorActionTypeConstants
		{
			public static class Codes
			{
				public const string ISACTraxonMessage = "ISC";
				public const string CreateCargoReportRecord = "CCR";
				public const string CreateUSAMSData = "CAM";
			}

			public static class Descriptions
			{
				public static string ISACTraxonMessage
				{
					get { return Res.GetString("1f53c972-89cb-4f97-9db0-6fff581e9cb0", "Send ISAC (Traxon) Message"); }
				}

				public static string CreateCargoReportRecord
				{
					get { return Res.GetString("e66cd251-865d-43b9-b220-7884d74e7e84", "Create AU Cargo Report Record"); }
				}

				public static string CreateUSAMSData
				{
					get { return Res.GetString("D58073DE-1915-445F-8B5F-2CA40822BC75", "Create US AMS"); }
				}
			}
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				JobConsolTransportSchema.JW_RL_NKLoadPort,
				JobConsolTransportSchema.JW_RL_NKDiscPort,
				JobConsolTransportSchema.JW_Vessel,
				JobConsolTransportSchema.JW_VoyageFlight,
				JobConsolTransportSchema.JW_ETD,
				JobConsolTransportSchema.JW_ETA,
				JobConsolTransportSchema.JW_ATD,
				JobConsolTransportSchema.JW_ATA,
				JobConsolSchema.JK_MasterBillNum,
			};
		}

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return true; }
		}

		public override bool SupportsWorkflowTriggerActionXMLWithAWB
		{
			get { return true; }
		}

		public override bool SupportsCreateTransportBooking
		{
			get { return true; }
		}

		/// <summary>
		/// Provides action types that are specific to JobConsolWorkflowDescriptor and ForwardingConsol containing the ProcessTask if specified;
		/// otherwise action types available for any ProcessTask
		/// </summary>
		/// <param name="task">The ProcessTask to find available action types</param>
		/// <returns>The list of available action types</returns>
		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml, WorkflowTriggerActionTypeConstants.Descriptions.SendDescartesXml);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.StartDestinationPortClearanceProcess, WorkflowTriggerActionTypeConstants.Descriptions.StartDestinationPortClearanceProcess);
			result.AddPair(ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document, ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Descriptions.SendCargoIMPPhase2Document);

			var consol = parent as ForwardingConsol;
			var template = parent as ProcessTaskTemplate;
			if (consol.IsAir() || template.IsAir())
			{
				result.AddPair(
					JobConsolWorkflowDescriptorActionTypeConstants.Codes.ISACTraxonMessage,
					JobConsolWorkflowDescriptorActionTypeConstants.Descriptions.ISACTraxonMessage);
			}
			if (consol.IsAir() || consol.IsSea() || template.IsAir() || template.IsSea())
			{
				result.AddPair(
					JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateCargoReportRecord,
					JobConsolWorkflowDescriptorActionTypeConstants.Descriptions.CreateCargoReportRecord);
			}

			if (consol.IsAir() || consol.IsSea() || consol.IsRail() || template.IsEmpty() || template.IsAir() || template.IsSea() || template.IsRail())
			{
				result.AddPair(
					JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData,
					JobConsolWorkflowDescriptorActionTypeConstants.Descriptions.CreateUSAMSData);
			}

			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendEmanifestCloseMessage,
				WorkflowTriggerActionTypeConstants.Descriptions.SendEmanifestCloseMessage);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendAllEmanifestHouseBills,
				WorkflowTriggerActionTypeConstants.Descriptions.SendAllEmanifestHouseBills);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML,
				WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalShipmentManifestXML);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalManifestEventXML,
				WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalManifestEventXML);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage,
				WorkflowTriggerActionTypeConstants.Descriptions.ValidateAndSendAirCargoReportMessage);

			if (PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.Value
					&& trigger.IsForCountry((BusinessObject)parent, Constants.CountryCodes.Netherlands))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification, WorkflowTriggerActionTypeConstants.Descriptions.CGNExportNotification);
			}

			return result;
		}

		/// <summary>
		/// Performs validation of the action type for the specified ProcessTask
		/// </summary>
		/// <param name="task">The ProcessTask to validate action type</param>
		/// <param name="actionTypeInfo">The info of the property containing action type</param>
		protected override void CheckWorkflowTriggerActionTypeCore(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo actionTypeInfo)
		{
			base.CheckWorkflowTriggerActionTypeCore(trigger, parent, actionTypeInfo);
			var errors = new List<string>();
			var warnings = new List<string>();
			var isValid = true;
			switch (actionTypeInfo.Value.ToString())
			{
				case WorkflowTriggerActionTypeConstants.Codes.StartDestinationPortClearanceProcess:
					isValid = IsValidForStartDestinationPortClearanceProcess(parent, warnings);
					break;
				case JobConsolWorkflowDescriptorActionTypeConstants.Codes.ISACTraxonMessage:
					isValid = IsValidForISACTraxonMessage(parent, errors);
					break;
				case JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateCargoReportRecord:
					isValid = IsValidForCreateCargoReportRecord(parent, Core.Constants.CountryCodes.Australia, warnings);
					break;
				case WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage:
					isValid = IsValidForScheduledDeferredMessageSend(parent, warnings);
					break;
				case WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging:
					isValid = IsValidForValidateForCustomsMessaging(parent, warnings);
					break;
			}
			if (!isValid)
			{
				errors.ForEach(error => actionTypeInfo.AddError(error));
				warnings.ForEach(warning => actionTypeInfo.AddWarning(warning));
			}
		}

		/// <summary>
		/// Performs validation of the recipient party for the specified ProcessTask
		/// </summary>
		/// <param name="task">The ProcessTask to validate recipient party</param>
		/// <param name="recipientPartyInfo">The info of the property containing recipient party</param>
		protected override void CheckWorkflowTriggerRecipientPartyCore(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo recipientPartyInfo)
		{
			base.CheckWorkflowTriggerRecipientPartyCore(trigger, parent, recipientPartyInfo);
			var consol = parent as ForwardingConsol;
			var template = parent as ProcessTaskTemplate;
			switch (recipientPartyInfo.Value.ToString())
			{
				case MessageRecipientPartyTypeList.Codes.DeConsolidator:
					if ((consol != null && consol.JK_TransportMode != Constants.TransportModes.Air) ||
						(template != null && template.P0_SubType1 != Constants.TransportModes.Air))
					{
						recipientPartyInfo.AddWarning(Res.GetString("5b3af5af-705b-4093-ae8a-6d301fd8e6ca", "This recipient is valid only for AIR consol."));
					}
					if (consol != null && consol.AUCusMAWB == null)
					{
						recipientPartyInfo.AddWarning(Res.GetString("bb2e5ece-80ad-47fc-910b-ec228f959711", "This recipient is valid only if there is an Australian Air Cargo job attached to this Consol, but none exists."));
					}
					break;
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					if ((consol != null && consol.JK_TransportMode != Constants.TransportModes.Air) ||
						(template != null && template.P0_SubType1 != Constants.TransportModes.Air))
					{
						recipientPartyInfo.AddWarning(Res.GetString("5b3af5af-705b-4093-ae8a-6d301fd8e6ca", "This recipient is valid only for AIR consol."));
					}
					if (consol != null)
					{
						var mawb = consol.AUCusMAWB;
						if (mawb == null)
						{
							recipientPartyInfo.AddWarning(Res.GetString("bb2e5ece-80ad-47fc-910b-ec228f959711", "This recipient is valid only if there is an Australian Air Cargo job attached to this Consol, but none exists."));
						}
						else if (mawb.EffectiveResponsiblePartyOrgHeader == null)
						{
							recipientPartyInfo.AddWarning(mawb.ReasonEffectiveResponsiblePartyIsUnavailable);
						}
					}
					break;
			}
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.Broker |
				MessageRecipientPartyType.ImportBroker |
				MessageRecipientPartyType.ExportBroker |
				MessageRecipientPartyType.ArrivalCTO |
				MessageRecipientPartyType.DepartureCTO |
				MessageRecipientPartyType.ArrivalContainerYard |
				MessageRecipientPartyType.DepartureContainerYard |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.ReceivingAgent |
				MessageRecipientPartyType.SendingAgent |
				MessageRecipientPartyType.ControllingAgent |
				MessageRecipientPartyType.ControllingCustomer |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.Carrier |
				MessageRecipientPartyType.DepartureCFS |
				MessageRecipientPartyType.ArrivalCFS |
				MessageRecipientPartyType.DeConsolidator |
				MessageRecipientPartyType.DepartureTransitWarehouse |
				MessageRecipientPartyType.ArrivalTransitWarehouse;
		}

		protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse:
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive, ServiceCodesList.Codes.TransitWarehouseDispatch, ServiceCodesList.Codes.TransitWarehouseReceiveAndDispatch };
				default:
					return base.SupportedTriggerPartyServicesCore(recipient);
			}
		}

		public override MessageRecipientPartyType SupportedManifestMessageRecipientParties(IBaseTrigger trigger)
		{
			return MessageRecipientPartyType.USAirAMS;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var result = base.GetWorkflowTriggerActionCore(source, queuedLog);
			if (result != null)
			{
				return result;
			}

			var action = source.Action;
			var consol = (ForwardingConsol)source.Job;
			var consolTriggeredByEvents = !queuedLog.ChangeLogs.Any() ? new EventsWithSourceType(EventsWithSourceType.SourceType.Consol, action, consol) : EventsWithSourceType.Empty;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType) || action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml)
			{
				ZString fileFormat = (WorkflowTriggerActionTypeConstants.IsSendXMLWithAWB(action.PQ_TriggerType) || WorkflowTriggerActionTypeConstants.IsSimplifiedXml(action.PQ_TriggerType)) ? ((ZString)EDICommunicationsModeFileFormatList.Codes.XML) : action.PQ_TriggerType;
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, fileFormat);
				result = new XmlMessageDeliver(xmlModes, consol, new ForwardingConsolValueObjectDataAdapter(consolTriggeredByEvents), action);
			}
			else
			{
				switch (action.PQ_TriggerType)
				{
					case WorkflowTriggerActionTypeConstants.Codes.StartDestinationPortClearanceProcess:
						result = IsValidForStartDestinationPortClearanceProcess(consol)
							? new SendCargoMessageProcessor(consol)
							: new LogAction((NoResString)"Cargo is not valid for port clearance."); // This is for logging only.
						break;
					case JobConsolWorkflowDescriptorActionTypeConstants.Codes.ISACTraxonMessage:
						result = (IProcessor)ObjectFactory.New<Enterprise.Integration.Customs.HK.IISCProcessor>(consol);
						break;
					case WorkflowTriggerActionTypeConstants.Codes.SendEmanifestCloseMessage:
						result = consol.CusCAeMHMaster != null
							? (IProcessor)ObjectFactory.New<Enterprise.Integration.Customs.CA.ISendCAeManifestCloseMessageProcessor>(consol.CusCAeMHMaster)
							: new LogAction((NoResString)"No CusCaeMHMaster, so could not send eManifest Close"); // This is for logging only.
						break;
					case WorkflowTriggerActionTypeConstants.Codes.SendAllEmanifestHouseBills:
						result = consol.CusCAeMHMaster != null
							? (IProcessor)ObjectFactory.New<Enterprise.Integration.Customs.CA.ISendAlleManifestHouseBillsMessageProcessor>(consol.CusCAeMHMaster)
							: new LogAction((NoResString)"No CusCaeMHMaster, so could not send eManifest House Bill"); // This is for logging only.
						break;
					case JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateCargoReportRecord:
						result = IsValidForCreateCargoReportRecord(consol, Core.Constants.CountryCodes.Australia)
							? (IProcessor)ObjectFactory.New<Enterprise.Integration.Customs.Shared.ICCRProcessor>(consol, Core.Constants.CountryCodes.Australia)
							: new LogAction((NoResString)"Consol is not valid of Cargo Report"); // This is for logging only.
						break;
					case ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document:
						result = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
						break;
					case WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage:
						result = GetSendAirCargoReportMessageProcessor(consol);
						break;
					case JobConsolWorkflowDescriptorActionTypeConstants.Codes.CreateUSAMSData:
						result = (IProcessor)ObjectFactory.New<Enterprise.Integration.Customs.US.ICAMProcessor>(consol);
						break;
					case WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification:
						result = new ExportNotificationMessageProcessor(consol, new CGNConsolReportSendingProvider(consol.Factory));
						break;
					default:
						// An unexpected error condition has occured
						result = null;
						break;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var consol = (ForwardingConsol)bizObj;

			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.ReceivingAgent:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.ReceivingForwarderAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.SendingAgent:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.SendingForwarderAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.Carrier:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.ShippingLineAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.DepartureCFS:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.PackDepotAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.ArrivalCFS:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.UnpackDepotAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.DepartureCTO:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.DepartureCTOAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.ArrivalCTO:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.ArrivalCTOAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.DepartureContainerYard:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.ContainerYardEmptyPickupAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.ArrivalContainerYard:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.ContainerYardEmptyReturnAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.PackDepotAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.UnpackDepotAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.DeConsolidator:
					{
						var mawb = consol.AUCusMAWB;
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb != null ? mawb.DeConsolidatorOrgHeader as OrgHeader : null, ZString.Empty));
					}
					break;
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					{
						var mawb = consol.AUCusMAWB;
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb != null ? mawb.EffectiveResponsiblePartyOrgHeader as OrgHeader : null, ZString.Empty));
					}
					break;
			}

			var shipmentWorkflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				shipmentWorkflowDescriptor.AddShipmentDirectPartiesToMessageRecipientPartyList(messageTriggerParties, shipment, partyType);
				shipmentWorkflowDescriptor.AddShipmentLinkedJobHeaderPartiesToMessageRecipientPartyList(messageTriggerParties, shipment, partyType);
			}
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.Consol }; }
		}

		IProcessor GetSendAirCargoReportMessageProcessor(ForwardingConsol consol)
		{
			if (consol.JK_RL_NKDischargePort.Left(2) == Core.Constants.CountryCodes.UnitedStates)
			{
				return new SendAirCargoReprotMessageProcessor(consol, new ACASConsolReportSendingProvider(consol.Factory), Core.Constants.CountryCodes.UnitedStates);
			}

			if (consol.JK_RL_NKDischargePort.Left(2) == Core.Constants.CountryCodes.Brazil)
			{
				return new SendAirCargoReprotMessageProcessor(consol, new CCTConsolReportSendingProvider(consol.Factory), Core.Constants.CountryCodes.Brazil);
			}

			return new LogAction((NoResString)"Consol is not arriving in US/BR."); // This is for logging only.
		}

		#endregion

		#region Form Customisation

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new ForwardingConsolFormCustomisationSettingsProvider();
		}

		#endregion

		#region Default Date

		protected override (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType)
		{
			var date = ZDateTime.Empty;
			RefUNLOCO locationForTimeZone = null;
			if (defaultedFromDateProvider.Parent is ForwardingConsol concreteParent)
			{
				if (dateTimeSourceType == ForwardingConsolEstimateDefaultedFromList.Codes.LoadingETD)
				{
					var departureTransport = concreteParent?.Transports.DepartureTransport;

					if (departureTransport != null)
					{
						(date, locationForTimeZone) = (departureTransport.JW_ETD, departureTransport.LoadPort);
					}
				}
				else if (dateTimeSourceType == ForwardingConsolEstimateDefaultedFromList.Codes.DischargeETA)
				{
					(date, locationForTimeZone) = GetArrivalTransportDate(Transport.Schema.JW_ETA, concreteParent);
				}
				else if (dateTimeSourceType == ForwardingConsolEstimateDefaultedFromList.Codes.FCLAvailable)
				{
					(date, locationForTimeZone) = GetArrivalTransportDate(Transport.Schema.JW_TerminalAvailabilityDate, concreteParent);
				}
				else if (dateTimeSourceType == ForwardingConsolEstimateDefaultedFromList.Codes.FCLStorage)
				{
					(date, locationForTimeZone) = GetArrivalTransportDate(Transport.Schema.JW_TerminalStorageDate, concreteParent);
				}
				else if (dateTimeSourceType == ForwardingConsolEstimateDefaultedFromList.Codes.LCLAvailable)
				{
					(date, locationForTimeZone) = GetArrivalTransportDate(Transport.Schema.JW_DepotAvailabilityDate, concreteParent);
				}
				else if (dateTimeSourceType == ForwardingConsolEstimateDefaultedFromList.Codes.LCLStorage)
				{
					(date, locationForTimeZone) = GetArrivalTransportDate(Transport.Schema.JW_DepotStorageDate, concreteParent);
				}
			}

			return (date, locationForTimeZone);
		}

		(ZDateTime, RefUNLOCO) GetArrivalTransportDate(ZString dateProperty, ForwardingConsol concreteParent)
		{
			var transport = concreteParent?.Transports.ArrivalTransport;

			if (transport != null)
			{
				var dateTime = (ZDateTime)transport[dateProperty];

				return (dateTime, transport.DiscPort);
			}

			return (ZDateTime.Empty, null);
		}

		#endregion

		#region Default

		protected override void SetDefaultTriggerConditionsCore(IMilestoneDateDefaultable defaultable, BusinessObject parent)
		{
			if (defaultable.TriggerCondition.IsEmpty && parent is ForwardingConsol consol)
			{
				if (defaultable.TriggerEventCode == Events.DepartureCode
					|| defaultable.TriggerEventCode == Events.ArrivalCode
					|| defaultable.TriggerEventCode == Events.CutOffDateCode
					|| defaultable.TriggerEventCode == Events.StorageCommencedCode
					|| defaultable.TriggerEventCode == Events.ReceiptCommencedCode)
				{
					string leg = JobConsolConditionProvider.GetLegIdentifier(defaultable, consol);
					if (leg != null)
					{
						defaultable.AddTriggerCondition(JobConsolConditionProvider.IsDepartureRelatedProcessTask(defaultable),
							leg,
							defaultable.TriggerEventCode == Events.DepartureCode
								|| defaultable.TriggerEventCode == Events.ArrivalCode
								|| defaultable.TriggerEventCode == Events.StorageCommencedCode
								|| defaultable.TriggerEventCode == Events.ReceiptCommencedCode);
					}
				}
			}
		}

		#endregion

		public override Type WorkflowProviderType
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override AutoGenCustomColumnDefinitionValidation GetAdditionalCustomColumnDefinitionValidation(GenCustomColumnDefinition customColumnDefinition)
		{
			return new PhaseSecurityGenCustomColumnDefinitionValidation(customColumnDefinition);
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.JobConsol; }
		}

		protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return new ConsolEventDataModel((CommonConsol)businessObject);
		}

		#region Validation Rules

		protected override ValidationToolSettings GetValidationToolSettings() => new JobConsolValidationToolSettings(this);

		#endregion

		#region Implementation

		static bool IsImportConsolIntoAustralia(IBusiness parent, List<string> warnings)
		{
			bool result = true;
			var consol = parent as ForwardingConsol;
			var template = parent as ProcessTaskTemplate;
			if (!consol.IsImportTo(Constants.CountryCodes.Australia) &&
				!template.IsImportTo(Constants.CountryCodes.Australia))
			{
				if (warnings != null)
				{
					warnings.Add(Res.GetString("90c7f3ba-efec-4822-8275-ac8c809b5b55",
						"This is not an import consol into Australia and therefore no trigger action will be taken."));
				}
				result = false;
			}
			return result;
		}

		static bool IsValidForStartDestinationPortClearanceProcess(IBusiness job, List<string> warnings = null)
		{
			return IsImportConsolIntoAustralia(job, warnings);
		}

		static bool IsValidForScheduledDeferredMessageSend(IBusiness job, List<string> warnings = null)
		{
			return IsImportConsolIntoAustralia(job, warnings);
		}

		static bool IsValidForValidateForCustomsMessaging(IBusiness job, List<string> warnings = null)
		{
			return IsImportConsolIntoAustralia(job, warnings);
		}

		static bool IsValidForISACTraxonMessage(IBusiness parent, List<string> errors = null)
		{
			bool result = true;
			var consol = parent as ForwardingConsol;
			if (consol != null)
			{
				if (consol.GetImportTransport(Constants.CountryCodes.HongKong) != null)
				{
					if (consol.ReceivingForwarder == null)
					{
						if (errors != null)
						{
							errors.Add(Res.GetString("55aebd03-d61b-405a-b00b-df8b3944dc54",
								"Receiving Agent is required for automated Traxon messages to be sent."));
						}
						result = false;
					}
					else
					{
						var branchLoader = new GlbBranch.Loader(consol.Factory);
						var matchingBranch = branchLoader.LoadActiveMatchingBranchInThisCountry(consol.ReceivingForwarder,
																								Constants.CountryCodes.HongKong);
						if (matchingBranch == null)
						{
							if (errors != null)
							{
								errors.Add(Res.GetString("3db7ea90-418d-43ab-8946-64a897d07eb9",
									"Receiving Agent entered does not match any Organization of active Hong Kong companies or its branches."));
							}
							result = false;
						}
					}
				}
				else if (consol.GetExportTransport(Constants.CountryCodes.HongKong) != null)
				{
					if (consol.SendingForwarder == null)
					{
						if (errors != null)
						{
							errors.Add(Res.GetString("895fb69c-2342-484b-8268-69c49ef6b634",
								"Sending Agent is required for automated Traxon messages to be sent."));
						}
						result = false;
					}
					else
					{
						var branchLoader = new GlbBranch.Loader(consol.Factory);
						var matchingBranch = branchLoader.LoadActiveMatchingBranchInThisCountry(consol.SendingForwarder,
																								Constants.CountryCodes.HongKong);
						if (matchingBranch == null)
						{
							if (errors != null)
							{
								errors.Add(Res.GetString("13a1fd76-fc45-4b95-9157-7021b5c55fd9",
									"Sending Agent entered does not match any Organization of active Hong Kong companies or its branches."));
							}
							result = false;
						}
					}
				}
			}
			return result;
		}

		static bool IsValidForCreateCargoReportRecord(IBusiness parent, string countryCode, List<string> warnings = null)
		{
			bool result = true;
			var consol = parent as ForwardingConsol;
			var template = parent as ProcessTaskTemplate;
			if (!consol.IsImportTo(countryCode) &&
				!template.IsImportTo(countryCode))
			{
				if (warnings != null)
				{
					warnings.Add(Res.GetString("94029543-9f25-4666-ba62-dd85912c247d",
						"Automated creation of {0} Customs Cargo Record is available for {0} import consolidations only.", countryCode));
				}
				result = false;
			}
			else if (consol != null)
			{
				var receivingAgent = consol.ReceivingForwarder;
				if (receivingAgent == null)
				{
					if (warnings != null)
					{
						warnings.Add(Res.GetString("7362ae35-628d-4b33-911e-3a0b25cd5dbc",
							"Receiving Agent is required for automated creation of {0} Customs Cargo Record.", countryCode));
					}
					result = false;
				}
				else
				{
					var branchLoader = new GlbBranch.Loader(consol.Factory);
					var matchingBranch = branchLoader.LoadActiveMatchingBranchInThisCountry(
						receivingAgent, countryCode);
					if (matchingBranch == null)
					{
						if (warnings != null)
						{
							warnings.Add(Res.GetString("9d1efa8c-a13c-48ca-88b7-3dd8c637c607",
								"Receiving Agent entered does not match any Organization of active {0} companies or its branches.", countryCode));
						}
						result = false;
					}
				}
			}
			return result;
		}

		#endregion // Implementation
	}
}

