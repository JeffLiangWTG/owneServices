using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.HelperClasses;
using Enterprise.Freight.Forwarding.Orders.Business;
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
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using IScheduleB3MessageSupporter = Enterprise.Integration.Customs.CA.IScheduleB3MessageSupporter;
using ISubmitAVSQuerySupporter = Enterprise.Integration.Customs.CA.ISubmitAVSQuerySupporter;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingShipmentWorkflowDescriptor : WorkflowDescriptor, IWorkflowParentWithLines
	{
		public ForwardingShipmentWorkflowDescriptor()
			: base()
		{
		}

		#region ID / Description / ControllerID

		public override string Code
		{
			get { return WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Freight|ForwardingShipmentWorkflowDescriptor|Description", "Shipment"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.JobShipment; }
		}

		public override ZString MilestoneTemplateHintCaption
		{
			get { return Res.GetString("588942ba-82a6-4503-89c8-14047fb3eb62", "Consolidation milestones are combined with the Shipment milestones below to produce the complete list."); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("1e0b688c-ffa8-400b-858c-6d9a89b1176b", "Transport Mode"), TransportModeList));
				list.Add(new ProcessTemplateSubType(Res.GetString("01a139c3-736c-4c4a-b3cb-3f51a896ac36", "Direction"), DirectionList));
				return list.ToArray();
			}
		}

		protected CodeDescriptionPairList TransportModeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("8791d77c-b25a-43d4-b6c5-70a09b387605", "All"));
				result.AddRange(new CodeDescriptionPairList(OLookUpEditType.TransportType));
				return result;
			}
		}

		protected CodeDescriptionPairList DirectionList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("9b145309-ee4c-499b-933c-ee9e5062d700", "All"));
				result.AddPair(DirectionsContext.Import, Res.GetString("4c588e76-3377-4707-99b5-0e609a553be2", "Import"));
				result.AddPair(DirectionsContext.Export, Res.GetString("ff6b2b51-cee3-4989-b196-f131e946cfd5", "Export"));
				result.AddPair(DirectionsContext.Domestic, Res.GetString("705f2db9-4d12-4ab7-9016-36631d22def4", "Domestic"));
				result.AddPair(DirectionsContext.CrossTrade, Res.GetString("e3216545-cccd-4fed-a5d6-ebfad0c1f432", "Cross Trade"));
				return result;
			}
		}

		#endregion

		#region Port Names

		public override ZString Port1Name
		{
			get { return Res.GetString("e342cc59-d708-4671-89c5-ba47ffde2103", "Origin"); }
		}

		public override ZString Port2Name
		{
			get { return Res.GetString("96580b40-0459-4331-9565-e8ec7b9febb3", "Destination"); }
		}

		#endregion

		#region Requires Client / Ports

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
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return true; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		#endregion

		#region Conditions

		public override CodeDescriptionPairList GetConditionList1(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new JobShipmentWorkflowCondition1CodeList();
		}

		public override CodeDescriptionPairList GetConditionList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new JobShipmentWorkflowCondition2CodeList();
		}

		#endregion

		#region Condition Values

		public override TemplateConditionValueStyle GetCondition2ValueStyle(ITemplateConditionalWorkflowItem workflowItem)
		{
			switch (workflowItem.TemplateCondition2)
			{
				case JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType:
					return TemplateConditionValueStyle.DropDown;

				default:
					return base.GetCondition2ValueStyle(workflowItem);
			}
		}

		public override CodeDescriptionPairList GetConditionValueList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			if (workflowItem != null && workflowItem.TemplateCondition2 == JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType)
			{
				var types = FreightDataRegistry.Instance.ReleaseTypes.GetValue(workflowItem);
				return types.GetCodeDescriptionPairList();
			}

			return base.GetConditionValueList2(workflowItem);
		}

		public override Type GetAdditionalRootType(TemplateConditionsViewModel templateConditions, TriggerConditionsViewModel triggerConditions)
		{
			Type additionalRootType = null;

			if (templateConditions != null
				&& templateConditions.TemplateCondition1 == JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached
				&& templateConditions.TemplateCondition2 == ProcessTasksLookups.UserDefinedCondition
				&& triggerConditions != null
				&& triggerConditions.TriggerContextCode == TriggerUserContextList.Codes.Specified)
			{
				var triggerCompany = triggerConditions.TriggerCompanyBizo;
				additionalRootType = triggerCompany != null
					? ((CountrySpecificTypeDecider)TypeDecider.GetTypeDeciderFromType(DeclarationType)).GetTypeForCountryCode(triggerCompany.GC_RN_NKCountryCode)
					: DeclarationType;
			}

			return additionalRootType;
		}

		public Type DeclarationType => ObjectFactory.GetType<IBaseJobDeclaration>();

		#endregion

		#region EstimateDefaultedFromList

		public override CodeDescriptionPairList EstimateDefaultedFromList
		{
			get { return new ForwardingShipmentEstimateDefaultedFromList(); }
		}

		#endregion

		#region Workflow Triggers

		#region GetWorkflowTriggerFieldColumns()

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			var columns = new SchemaColumn[]
				{
					JobConsolTransportSchema.JW_RL_NKLoadPort,
					JobConsolTransportSchema.JW_RL_NKDiscPort,
					JobConsolTransportSchema.JW_Vessel,
					JobConsolTransportSchema.JW_VoyageFlight,
					JobConsolTransportSchema.JW_ETD,
					JobConsolTransportSchema.JW_ETA,
					JobConsolTransportSchema.JW_ATD,
					JobConsolTransportSchema.JW_ATA,
					JobShipmentSchema.JS_HouseBill,
					JobShipmentSchema.JS_E_ARV,
					JobShipmentSchema.JS_E_DEP,
					JobConsolSchema.JK_MasterBillNum,
					JobDocsAndCartageSchema.JP_EstimatedDelivery,
					JobDocsAndCartageSchema.JP_EstimatedPickup
				};
			if ((parent is ForwardingShipment shipment && shipment.Declarations.Length > 0) || parent is ProcessTaskTemplate)
			{
				columns = columns.Append(new SchemaColumn[]
				{
					JobDeclarationSchema.JE_EntryAuthorisationDate,
					JobDeclarationSchema.JE_HouseBill,
					JobDeclarationSchema.JE_VesselName,
					JobDeclarationSchema.JE_VoyageFlightNo,
					JobDeclarationSchema.JE_DateAtOrigin,
					JobDeclarationSchema.JE_DateAtFinalDestination,
					JobDeclarationSchema.JE_ExportDate,
					JobDeclarationSchema.JE_DateOfArrival,
					JobDeclarationSchema.JE_MasterBill,
					JobDeclarationSchema.JE_EntrySubmittedDate,
					JobDeclarationSchema.JE_WarehouseReleaseDate,
					JobDeclarationSchema.JE_DateOfFirstArrival,
					JobDeclarationSchema.JE_EntryDate,
					JobDeclarationSchema.JE_RL_NKPortOfLoading,
					JobDeclarationSchema.JE_RL_NKPortOfFirstArrival,
					JobDeclarationSchema.JE_RL_NKPortOfArrival,
					JobDeclarationSchema.JE_RL_NKFinalDestination,
					JobDeclarationSchema.JE_LandedPieces,
					JobDeclarationSchema.JE_TotalNoOfPacks,
					CusEntryHeaderSchema.CH_BondAcquittedDate,
					CusEntryHeaderSchema.CH_BondValidToDate
				}).ToArray();
			}

			return columns;
		}

		#endregion

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.Shipment }; }
		}

		#region ForwardingShipmentWorkflowTriggerActionTypeList

		public static class ForwardingShipmentWorkflowTriggerActionTypeConstants
		{
			public static class Codes
			{
				public const string SendBrokerageXMLDocument = "XBK";
				public const string SendSterlingFlatFileWithJobFallback = "SFF";
				public const string SendCargoIMPPhase2Document = "CI2";
				public const string CINExportNotification = WorkflowTriggerActionTypeConstants.Codes.CINExportNotification;
			}

			public static class Descriptions
			{
				public static string SendBrokerageXMLDocument
				{
					get { return Res.GetString("fb27f52c-8e64-4531-8069-9ce1c093a8cc", "Send Brokerage XML Document"); }
				}
				public static string SendSterlingFlatFileWithJobFallback
				{
					get { return Res.GetString("89e1f77a-0989-4bcc-96d2-cc6e023ec0b7", "Send Sterling Flat File with Job Fallback"); }
				}
				public static string SendCargoIMPPhase2Document
				{
					get { return Res.GetString("d1d5a136-f684-481a-abda-fdc6fd81f9ed", "Send CargoIMP Phase 2 File"); }
				}

				public static string CINExportNotification
				{
					get { return WorkflowTriggerActionTypeConstants.Descriptions.CINExportNotification; }
				}
			}
		}

		#endregion

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return true; }
		}

		public override bool SupportsWorkflowTriggerActionXMLWithJobFallback
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

		public override bool SupportsHVLVPreScreening => true;

		/// <summary>
		/// Provides action types that are specific to ForwardingShipmentWorkflowDescriptor and available for any ProcessTask
		/// </summary>
		/// <param name="task">The ProcessTask is ignored</param>
		/// <returns>The list of available action types</returns>
		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness business)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendBrokerageXMLDocument, ForwardingShipmentWorkflowTriggerActionTypeConstants.Descriptions.SendBrokerageXMLDocument);
			result.AddPair(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendSterlingFlatFileWithJobFallback, ForwardingShipmentWorkflowTriggerActionTypeConstants.Descriptions.SendSterlingFlatFileWithJobFallback);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml, WorkflowTriggerActionTypeConstants.Descriptions.SendDescartesXml);
			result.AddPair(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document, ForwardingShipmentWorkflowTriggerActionTypeConstants.Descriptions.SendCargoIMPPhase2Document);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage, WorkflowTriggerActionTypeConstants.Descriptions.ValidateAndSendAirCargoReportMessage);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateBrokerageOnShipment, WorkflowTriggerActionTypeConstants.Descriptions.CreateBrokerageOnShipment);

			if (countriesSupportedAutoSendingMessage.Any(country => trigger.IsForCountry((BusinessObject)business, country)))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message, WorkflowTriggerActionTypeConstants.Descriptions.ScheduleB3Message);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery, WorkflowTriggerActionTypeConstants.Descriptions.SubmitAVSQuery);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendEntryDeclarationMessage);
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendReleaseMessage);
			}

			if (business is ProcessTaskTemplate template)
			{
				if (!template.P0_LoadPortCountry.StartsWith(CountryCodes.UnitedStates) && template.P0_DischargePortCountry.StartsWith(CountryCodes.UnitedStates))
				{
					if (template.P0_SubType1 == TransportModes.Sea)
					{
						AddUSSeaTriggerActionTypes(result);
					}
					else if (template.P0_SubType1 == TransportModes.Air)
					{
						AddUSAirTriggerActionTypes(result);
					}
					else if (template.P0_SubType1 == TransportModes.Road)
					{
						AddHVLVUSTruckEManifestTriggerActionType(result);
					}
				}
				else if (ShouldAddH7TriggerActionType(template.P0_LoadPortCountry, template.P0_DischargePortCountry))
				{
					AddH7TriggerActionType(result);
				}

				if (template.P0_SubType1 == TransportModes.Air)
				{
					if (template.P0_DischargePortCountry.StartsWith(CountryCodes.Australia))
					{
						AddAUAirTriggerActionTypes(result);
					}
					else if (template.P0_LoadPortCountry.StartsWith(CountryCodes.NewZealand) || template.P0_DischargePortCountry.StartsWith(CountryCodes.NewZealand))
					{
						AddNZAirTriggerActionTypes(result);
					}
				}
				else if (template.P0_SubType1 == TransportModes.Sea)
				{
					if (template.P0_DischargePortCountry.StartsWith(CountryCodes.Australia))
					{
						AddAUSeaTriggerActionTypes(result);
					}
					else if (template.P0_LoadPortCountry.StartsWith(CountryCodes.NewZealand) || template.P0_DischargePortCountry.StartsWith(CountryCodes.NewZealand))
					{
						AddNZSeaTriggerActionTypes(result);
					}
				}
			}
			else if (business is ForwardingShipment shipment)
			{
				if ((shipment.Destination?.Country?.Code.Equals(CountryCodes.UnitedStates) ?? false))
				{
					if (shipment.IsSea)
					{
						AddUSSeaTriggerActionTypes(result);
					}
					else if (shipment.IsAir)
					{
						AddUSAirTriggerActionTypes(result);
					}
					else if (shipment.IsRoad && shipment.JobDirection.Equals(Directions.Import))
					{
						AddHVLVUSTruckEManifestTriggerActionType(result);
					}
				}
				else if ((shipment.Destination?.Country?.Code.Equals(CountryCodes.Australia) ?? false))
				{
					if (shipment.IsSea)
					{
						AddAUSeaTriggerActionTypes(result);
					}
					else if (shipment.IsAir)
					{
						AddAUAirTriggerActionTypes(result);
					}
				}
				else if (ShouldAddH7TriggerActionType(shipment.Origin?.Country?.Code ?? ZString.Empty, shipment.Destination?.Country?.Code ?? ZString.Empty))
				{
					AddH7TriggerActionType(result);
				}

				if ((shipment.Destination?.Country?.Code.Equals(CountryCodes.NewZealand) ?? false) || (shipment.Origin?.Country?.Code.Equals(CountryCodes.NewZealand) ?? false))
				{
					if (shipment.IsSea)
					{
						AddNZSeaTriggerActionTypes(result);
					}
					else if (shipment.IsAir)
					{
						AddNZAirTriggerActionTypes(result);
					}
				}
			}

			if (PortMessagingRegistry.Instance.AllowToSendExportNotification.Value
				&& trigger.IsForCountry((BusinessObject)business, CountryCodes.France))
			{
				result.AddPair(ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification, ForwardingShipmentWorkflowTriggerActionTypeConstants.Descriptions.CINExportNotification);
			}
			return result;
		}

		void AddAUSeaTriggerActionTypes(CodeDescriptionPairList actionTypes)
		{
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport, WorkflowTriggerActionTypeConstants.Descriptions.AUSeaCargoReport);
		}

		void AddAUAirTriggerActionTypes(CodeDescriptionPairList actionTypes)
		{
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport, WorkflowTriggerActionTypeConstants.Descriptions.AUAirCargoReport);
		}

		void AddNZSeaTriggerActionTypes(CodeDescriptionPairList actionTypes)
		{
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport, WorkflowTriggerActionTypeConstants.Descriptions.NZSeaCargoReport);
		}

		void AddNZAirTriggerActionTypes(CodeDescriptionPairList actionTypes)
		{
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport, WorkflowTriggerActionTypeConstants.Descriptions.NZAirCargoReport);
		}

		void AddUSSeaTriggerActionTypes(CodeDescriptionPairList actionTypes)
		{
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling, WorkflowTriggerActionTypeConstants.Descriptions.CreateHVLVImporterSecurityFiling);
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS, WorkflowTriggerActionTypeConstants.Descriptions.CreateHVLVSeaFreightAMS);
		}

		void AddUSAirTriggerActionTypes(CodeDescriptionPairList actionTypes)
		{
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS, WorkflowTriggerActionTypeConstants.Descriptions.CreateHVLVAirFreightAMS);
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport, WorkflowTriggerActionTypeConstants.Descriptions.HVLVSendAcknowledgementACASReport);
		}

		void AddH7TriggerActionType(CodeDescriptionPairList actionTypes)
		{
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration, WorkflowTriggerActionTypeConstants.Descriptions.CreateH7Declaration);
		}

		bool ShouldAddH7TriggerActionType(ZString originPortCountry, ZString destinationPortCountry)
		{
			var originCountryCode = originPortCountry.Left(2);
			var destinationCountryCode = destinationPortCountry.Left(2);

			return originCountryCode != destinationCountryCode
				&& ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(CountryCodes.GetCustomsCountryOfJurisdiction(destinationCountryCode));
		}

		void AddHVLVUSTruckEManifestTriggerActionType(CodeDescriptionPairList actionTypes)
		{
			actionTypes.AddPair(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest, WorkflowTriggerActionTypeConstants.Descriptions.CreateHVLVUSTruckEManifest);
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return
				base.IsMessagingOrEmailNotificationTriggerAction(triggerAction) ||
				triggerAction == ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendBrokerageXMLDocument ||
				triggerAction == ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendSterlingFlatFileWithJobFallback;
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			var result = MessageRecipientPartyType.ArrivalCarrier |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.Broker |
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.ControllingAgent |
				MessageRecipientPartyType.ControllingCustomer |
				MessageRecipientPartyType.DeConsolidator |
				MessageRecipientPartyType.DeliveryAgent |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.DeliveryToParty |
				MessageRecipientPartyType.DepartureCarrier |
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.ExportBroker |
				MessageRecipientPartyType.ImportBroker |
				MessageRecipientPartyType.NotifyParty |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.PickupAgent |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.PickupParty |
				MessageRecipientPartyType.ReceivingAgent |
				MessageRecipientPartyType.SendingAgent |
				MessageRecipientPartyType.DepartureTransitWarehouse |
				MessageRecipientPartyType.ArrivalTransitWarehouse |
				MessageRecipientPartyType.HVLVAirClearanceAgent |
				MessageRecipientPartyType.HVLVSeaClearanceAgent |
				MessageRecipientPartyType.WarehouseInwards;

			return result;
		}

		protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse:
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive, ServiceCodesList.Codes.TransitWarehouseDispatch, ServiceCodesList.Codes.TransitWarehouseReceiveAndDispatch, ServiceCodesList.Codes.TransitWarehousePrepareDispatch };
				default:
					return base.SupportedTriggerPartyServicesCore(recipient);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a service task log")]
		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var result = base.GetWorkflowTriggerActionCore(source, queuedLog);
			if (result != null)
			{
				return result;
			}
			var action = source.Action;

			var shipmentTriggeredByEvents = !queuedLog.ChangeLogs.Any() ? new EventsWithSourceType(EventsWithSourceType.SourceType.Shipment, action, source.Job) : EventsWithSourceType.Empty;

			var shipment = (ForwardingShipment)source.Job;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType) || WorkflowTriggerActionTypeConstants.IsXmlWithJobFallback(action.PQ_TriggerType))
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				return GetWorkflowTriggerActionForXmlActionType(shipment, shipment, xmlModes, action.PQ_TriggerType, shipmentTriggeredByEvents, action);
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml)
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.DXL);
				return GetWorkflowTriggerActionForXmlActionType(shipment, shipment, xmlModes, action.PQ_TriggerType, shipmentTriggeredByEvents, action);
			}
			else if (action.PQ_TriggerType == ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendBrokerageXMLDocument)
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				var declaration = (IBaseJobDeclaration)shipment.GetDeclaration();

				if (declaration != null)
				{
					return new XmlMessageDeliver(xmlModes, (BusinessObject)declaration, () =>
					{
						var exporter = (IDeclarationWithConsolShipmentDetailExporter)Activator.CreateInstance(ObjectFactory.GetType<IDeclarationWithConsolShipmentDetailExporter>());
						return (Xsd.XmlInterchange)exporter.ExportDeclarationWithRelatedConsolShipmentDetails(declaration, shipmentTriggeredByEvents, action);
					}, action);
				}
				else
				{
					return new LogAction((NoResString)"No declaration was found, so could not send the XML document.");
				}
			}
			else if (action.PQ_TriggerType == ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendSterlingFlatFileWithJobFallback)
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.SterlingCommerceFlatFile);
				var bizObjToExport = (shipment.Consols.Count > 0) ? shipment.Consols[0] : (BusinessObject)shipment;
				IValueObjectDataAdapter dataAdapter;
				var orderToExportFilter = (BusinessObject)shipment as Order;

				if (shipment.Consols.Count > 0)
				{
					dataAdapter = new ForwardingConsolWithShipmentValueObjectDataAdapter(shipment, shipmentTriggeredByEvents);
				}
				else
				{
					dataAdapter = new ForwardingShipmentValueObjectDataAdapter(orderToExportFilter, shipmentTriggeredByEvents);
				}
				return new SterlingCommerceMessageDelivery(xmlModes, bizObjToExport, shipment, dataAdapter, action);
			}
			else if (WorkflowTriggerActionTypeConstants.IsDebtorBalanceXml(action.PQ_TriggerType))
			{
				var job = shipment.Job;

				if (job != null)
				{
					var factory = shipment.Factory;
					var debtorAddress = factory.Load<OrgAddress>(shipment.Job.JH_OA_LocalChargesAddr);
					var debtor = debtorAddress != null ? factory.Load<OrgHeader>(debtorAddress.OA_OH) : null;

					if (debtor != null)
					{
						var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XMB);
						return new XmlMessageDeliver(xmlModes, new DebtorBalanceRecordForExport(debtor, factory), shipment, new DebtorBalanceValueObjectDataAdapter(), action);
					}
					else
					{
						return new LogAction((NoResString)"No debtor information was found, so could not send the XML document.");
					}
				}
				else
				{
					return new LogAction((NoResString)"No job was found for the shipment, so could not send the XML document.");
				}
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message)
			{
				var supporter = GetScheduleB3MessageSupporter(source.Job);
				if (supporter != null)
				{
					return supporter.CreateStmProcessQueueProcessor(null, action.PQ_TriggerType);
				}
				else
				{
					return new LogAction((NoResString)"No declaration was found for the shipment, so could not create the Schedule CAD message.");
				}
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery)
			{
				var supporter = GetSubmitAVSQuerySupporter(source.Job);

				if (supporter != null)
				{
					return supporter.CreateSubmitAVSQueryProcessor();
				}
				else
				{
					return new LogAction((NoResString)"No declaration was found for the shipment, so could not submit AVS query.");
				}
			}
			else if (action.PQ_TriggerType == ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document)
			{
				return new ShipmentCargoImpPhase2MessageDelivery(shipment, action, queuedLog);
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.CreateBrokerageOnShipment)
			{
				return ObjectFactory.Get<IProcessor>("Shared.ICreateBrokerageOnShipmentProcessor", shipment);
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling)
			{
				return ObjectFactory.Get<IProcessor>("ForwardingShipmentToISFProcessor", shipment);
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.RunHVLVPreScreening)
			{
				if (shipment.IsHighVolumeLowValue)
				{
					return ObjectFactory.Get<IProcessor>("HVLVPreScreeningProcessor", shipment);
				}
				else
				{
					return new LogAction("Trigger Action Type PRE is only allowed for HighVolumeLowValue shipments.");
				}
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport ||
					action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport ||
					action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport ||
					action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport ||
					action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS ||
					action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS ||
					action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration ||
					action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest)
			{
				return GetHVLVConvertShipmentToCustomsJobProcessor(shipment, action.PQ_TriggerType);
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport)
			{
				if (shipment.IsHighVolumeLowValue)
				{
					return ObjectFactory.Get<IProcessor>("HVLVValidateAndSendAcknowledgementACASReportProcessor", shipment);
				}
				else
				{
					return new LogAction("Trigger Action Type HAK is only allowed for HighVolumeLowValue shipments.");
				}
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage || action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage)
			{
				var declaration = shipment.GetDeclaration();
				if (declaration is IJobDeclarationAutoSendingMessageSupporter supporter)
				{
					return supporter.CreateStmProcessQueueProcessor(declaration, action.PQ_TriggerType);
				}
				else
				{
					return new LogAction($"No matching declaration could be found for company '{GlbCompany.CurrentCompany.GC_Code}'.");
				}
			}

			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage)
			{
				return GetSendAirCargoReportMessageProcessor(shipment);
			}

			if (action.PQ_TriggerType == ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.CINExportNotification)
			{
				return new ExportNotificationMessageProcessor(shipment, new CINShipmentReportSendingProvider(shipment.Factory));
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a service task log")]
		IProcessor GetHVLVConvertShipmentToCustomsJobProcessor(ForwardingShipment shipment, string triggerCode)
		{
			if (shipment.IsHighVolumeLowValue)
			{
				if ((triggerCode == WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport ||
					 triggerCode == WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport) &&
					!shipment.IsSea)
				{
					return new LogAction(string.Format("Trigger Action Type {0} is only supported by Sea shipments.", triggerCode));
				}
				else if ((triggerCode == WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport ||
						  triggerCode == WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport) &&
						 !shipment.IsAir)
				{
					return new LogAction(string.Format("Trigger Action Type {0} is only supported by Air shipments.", triggerCode));
				}

				return ObjectFactory.Get<IProcessor>("HVLVConvertShipmentToCustomsJobProcessor", shipment, triggerCode);
			}
			else
			{
				return new LogAction(string.Format("Trigger Action Type {0} is only allowed for HighVolumeLowValue shipments.", triggerCode));
			}
		}

		internal IMessageProcessor GetWorkflowTriggerActionForXmlActionType(ForwardingShipment shipment, BusinessObject triggerParent, MessageProcessorCommunicationModesResult xmlModes, ZString triggerActionType, EventsWithSourceType triggeredByEvents, ProcessTaskNotification action)
		{
			var bizObjToExport = (BusinessObject)shipment;
			var orderToExportFilter = triggerParent as Order;
			IValueObjectDataAdapter dataAdapter = null;

			if (WorkflowTriggerActionTypeConstants.IsXmlWithJobFallback(triggerActionType) && shipment.LocalConsol != null)
			{
				bizObjToExport = shipment.LocalConsol;
				dataAdapter = (orderToExportFilter == null) ? new ForwardingConsolValueObjectDataAdapter(shipment, triggeredByEvents) : new ForwardingConsolValueObjectDataAdapter(orderToExportFilter, triggeredByEvents);
			}
			else
			{
				dataAdapter = new ForwardingShipmentValueObjectDataAdapter(shipment.LocalConsol, orderToExportFilter, triggeredByEvents);
			}

			return new XmlMessageDeliver(xmlModes, bizObjToExport, triggerParent as IJobNumber, dataAdapter, action);
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			var shipment = (ForwardingShipment)bizObj;
			AddAllShipmentPartiesToMessageRecipientPartyList(messageRecipientParties, shipment, partyType);
		}

		internal void AddAllShipmentPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, ForwardingShipment shipment, ZString partyType)
		{
			AddShipmentDirectPartiesToMessageRecipientPartyList(messageRecipientParties, shipment, partyType);
			AddLinkedConsolPartiesToMessageRecipientPartyList(messageRecipientParties, shipment, partyType);
			AddLinkedJobHeaderPartiesToMessageRecipientPartyList(messageRecipientParties, shipment, partyType);

			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.HVLVAirClearanceAgent:
				case MessageRecipientPartyTypeList.Codes.HVLVSeaClearanceAgent:
					{
						messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(GlbCompany.CurrentCompany.OrgProxy, ZString.Empty));
					}
					break;
				case MessageRecipientPartyTypeList.Codes.DeConsolidator:
					{
						var hawb = shipment.AUCusHAWB;
						var mawb = hawb == null ? null : hawb.MAWB;
						messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb != null ? mawb.DeConsolidatorOrgHeader as OrgHeader : null, ZString.Empty));
					}
					break;
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					{
						var hawb = shipment.AUCusHAWB;
						var mawb = hawb == null ? null : hawb.MAWB;
						messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb != null ? mawb.EffectiveResponsiblePartyOrgHeader as OrgHeader : null, ZString.Empty));
					}
					break;

				case MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ExportReceivingDepot));
					break;

				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ImportReleaseDepot));
					break;
				case MessageRecipientPartyTypeList.Codes.WarehouseInwards:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.DocAddresses.FindByDocAddressType(DocAddressType.Warehouse)));
					break;
			}
		}

		internal void AddShipmentDirectPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, ForwardingShipment shipment, ZString partyType)
		{
			if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsigneeDocumentaryAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsignorDocumentaryAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Broker)
			{
				if (shipment.IsExport())
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ExportBroker, ZString.Empty));
				}
				else
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ImportBroker, ZString.Empty));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.ExportBroker)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ExportBroker, ZString.Empty));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.ImportBroker)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ImportBroker, ZString.Empty));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.PickupCartage && !shipment.IsDeleted)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.DocsAndCartage.PickupCartageCoAddr));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.DeliveryCartage && !shipment.IsDeleted)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.DocsAndCartage.DeliveryCartageCoAddr));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.NotifyParty)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.NotifyPartyDocumentaryAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.PickupParty)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsignorPickupAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.DeliveryToParty)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsigneeDeliveryAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.ControllingAgent)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ControllingAgentDocumentaryAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.ControllingCustomer)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ControllingCustomerAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.PickupAgent)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.PickupAgentDocumentaryAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.DeliveryAgent)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.DeliveryAgent, ZString.Empty));
			}
		}

		internal void AddShipmentLinkedJobHeaderPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, ForwardingShipment shipment, ZString partyType)
		{
			AddLinkedJobHeaderPartiesToMessageRecipientPartyList(messageRecipientParties, shipment, partyType);
		}

		void AddLinkedConsolPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, ForwardingShipment shipment, ZString partyType)
		{
			if (partyType == MessageRecipientPartyTypeList.Codes.ReceivingAgent)
			{
				if (shipment.ArrivalConsol != null)
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ArrivalConsol.ReceivingForwarderAddress));
				}
				else if (shipment.Consols.Count == 1)
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Consols[0].ReceivingForwarderAddress));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.SendingAgent)
			{
				if (shipment.DepartureConsol != null)
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.DepartureConsol.SendingForwarderAddress));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.ArrivalCarrier)
			{
				if (shipment.Consols.Count == 1)
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Consols[0].ShippingLineAddress));
				}
				else if (shipment.ArrivalConsol != null)
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ArrivalConsol.ShippingLineAddress));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.DepartureCarrier)
			{
				if (shipment.Consols.Count == 1)
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Consols[0].ShippingLineAddress));
				}
				else if (shipment.DepartureConsol != null)
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.DepartureConsol.ShippingLineAddress));
				}
			}
		}

		IProcessor GetSendAirCargoReportMessageProcessor(ForwardingShipment shipment)
		{
			if (shipment.JS_RL_NKDestination.Left(2) == Core.Constants.CountryCodes.UnitedStates)
			{
				if (shipment.IsHighVolumeLowValue)
				{
					return ObjectFactory.Get<IProcessor>("HVLVValidateAndSendOriginalACASReportProcessor", shipment);
				}

				return new SendAirCargoReprotMessageProcessor(shipment, new ACASShipmentReportSendingProvider(shipment.Factory), Core.Constants.CountryCodes.UnitedStates);
			}

			if (shipment.JS_RL_NKDestination.Left(2) == Core.Constants.CountryCodes.Brazil)
			{
				return new SendAirCargoReprotMessageProcessor(shipment, new CCTShipmentReportSendingProvider(shipment.Factory), Core.Constants.CountryCodes.Brazil);
			}

			return new LogAction((NoResString)"Shipment is not arriving in US/BR."); // This is for logging only.
		}

		IScheduleB3MessageSupporter GetScheduleB3MessageSupporter(IBusiness parent)
		{
			var shipment = parent as ForwardingShipment;
			var declaration = shipment == null ? null : shipment.GetDeclaration() as CA.IJobDeclaration;
			return declaration == null ? null : ObjectFactory.New<IScheduleB3MessageSupporter>(declaration);
		}

		ISubmitAVSQuerySupporter GetSubmitAVSQuerySupporter(IBusiness parent)
		{
			var shipment = parent as ForwardingShipment;
			var declaration = shipment == null ? null : shipment.GetDeclaration() as CA.IJobDeclaration;
			return declaration == null ? null : ObjectFactory.New<ISubmitAVSQuerySupporter>(declaration);
		}

		#endregion

		#region Form Customisation

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new ForwardingShipmentFormCustomisationSettingsProvider();
		}

		#endregion

		#region Event Context

		protected override IEnumerable<WorkflowEventContextPair> GetFollowingContextStepsCore(IEnumerable<WorkflowEventContextPair> currentContextPath)
		{
			var masterClassifiers = new ForwardingShipmentWorkflowEventContextMasterClassifiers();
			var masterTypes = new ForwardingShipmentWorkflowEventContextMasterTypes();

			var lastContextPair = currentContextPath != null ? currentContextPath.LastOrDefault() : null;
			if (lastContextPair == null || lastContextPair.MasterType.Code == ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment)
			{
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]);
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]);
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]);
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]);

				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol]);
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol]);
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol]);
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol]);

				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment]);
			}
			else if (lastContextPair.MasterType.Code == ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol)
			{
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]);
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]);
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]);
				yield return new WorkflowEventContextPair(masterClassifiers[ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every], masterTypes[ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg]);
			}
			else if (lastContextPair.MasterType.Code == ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg)
			{
				// No subelements on Leg
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
		protected override bool IsRelatedEntityInContextCore(BusinessObject entity, BusinessObject relatedEntity, IEnumerable<WorkflowEventContextPair> contextPath)
		{
			if (entity == null || relatedEntity == null)
			{
				return false;
			}
			if (contextPath == null || !contextPath.Any())
			{
				return entity.PK == relatedEntity.PK;
			}

			var firstContext = contextPath.First();
			var tailContextPath = contextPath.Skip(1);
			var shipment = entity as CommonShipment;

			if (firstContext.MasterType.Code == ForwardingShipmentWorkflowEventContextMasterTypes.Codes.MasterShipment)
			{
				return shipment != null && IsRelatedEntityInContextCore(shipment.CoLoadMasterShipment, relatedEntity, tailContextPath);
			}
			else if (firstContext.MasterType.Code == ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Consol)
			{
				if (shipment != null)
				{
					var sortedConsols = shipment.Consols.ToArray<CommonConsol>();
					MovementLegComparer.SortMovementLegsByPorts(sortedConsols);

					if (firstContext.MasterClassifier == null || string.IsNullOrEmpty(firstContext.MasterClassifier.Code)
						|| firstContext.MasterClassifier.Code == ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every)
					{
						return sortedConsols.Any(consol => IsRelatedEntityInContextCore(consol, relatedEntity, tailContextPath));
					}
					else if (firstContext.MasterClassifier.Code == ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure)
					{
						return IsRelatedEntityInContextCore(sortedConsols.FirstOrDefault(), relatedEntity, tailContextPath);
					}
					else if (firstContext.MasterClassifier.Code == ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival)
					{
						return IsRelatedEntityInContextCore(sortedConsols.LastOrDefault(), relatedEntity, tailContextPath);
					}
					else if (firstContext.MasterClassifier.Code == ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship)
					{
						return sortedConsols.Skip(1).Take(sortedConsols.Length - 2).Any(consol => IsRelatedEntityInContextCore(consol, relatedEntity, tailContextPath));
					}
				}
			}
			else if (firstContext.MasterType.Code == ForwardingShipmentWorkflowEventContextMasterTypes.Codes.Leg)
			{
				Transport[] sortedTransports = null;
				CommonConsol consol;

				if (shipment != null)
				{
					sortedTransports = shipment.TransportsIncludingRelated.ToArray<Transport>();
					MovementLegComparer.SortMovementLegsByPorts(sortedTransports);
				}
				else if ((consol = entity as CommonConsol) != null)
				{
					sortedTransports = consol.Transports.ToArray<Transport>();
					MovementLegComparer.SortMovementLegsByPorts(sortedTransports);
				}

				if (sortedTransports != null)
				{
					if (firstContext.MasterClassifier == null || string.IsNullOrEmpty(firstContext.MasterClassifier.Code)
						|| firstContext.MasterClassifier.Code == ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Every)
					{
						return sortedTransports.Any(transport => IsRelatedEntityInContextCore(transport, relatedEntity, tailContextPath));
					}
					else if (firstContext.MasterClassifier.Code == ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Departure)
					{
						return IsRelatedEntityInContextCore(sortedTransports.FirstOrDefault(), relatedEntity, tailContextPath);
					}
					else if (firstContext.MasterClassifier.Code == ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Arrival)
					{
						return IsRelatedEntityInContextCore(sortedTransports.LastOrDefault(), relatedEntity, tailContextPath);
					}
					else if (firstContext.MasterClassifier.Code == ForwardingShipmentWorkflowEventContextMasterClassifiers.Codes.Transship)
					{
						return sortedTransports.Skip(1).Take(sortedTransports.Length - 2).Any(transport => IsRelatedEntityInContextCore(transport, relatedEntity, tailContextPath));
					}
				}
			}

			return false;
		}

		#endregion

		#region Default Date

		protected override bool GetLogIsValidForDateDefaultingCore(IStmALog ilog)
		{
			var log = (StmALog)ilog;
			return (log.SL_SE_NKEvent != Events.Departure.Code && log.SL_SE_NKEvent != Events.Arrival.Code)
				|| !log.SL_IsEstimate
				|| log.SL_ReferenceForBinding.IsEmpty
				|| log.SL_ReferenceForBinding.StartsWith((NoResString)"From: ", StringComparison.OrdinalIgnoreCase) // Non-translatable text
				|| log.SL_ReferenceForBinding.StartsWith((NoResString)"To: ", StringComparison.OrdinalIgnoreCase);  // Non-translatable text
		}

		protected override (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType)
		{
			if (!dateTimeSourceType.IsEmpty && defaultedFromDateProvider.Parent is ForwardingShipment concreteParent && !concreteParent.IsDeleted)
			{
				if (dateTimeSourceType == ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD)
				{
					return (concreteParent.JS_E_DEP, concreteParent.Origin);
				}
				else if (dateTimeSourceType == ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA)
				{
					return (concreteParent.JS_E_ARV, concreteParent.Destination);
				}
				else if (dateTimeSourceType == ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolLoadingETD)
				{
					var departureTransport = concreteParent.DepartureConsol?.Transports.DepartureTransport;
					var result = departureTransport?.JW_ETD ?? ZDateTime.Empty;

					return (result, departureTransport?.LoadPort);
				}
				else
				{
					var arrivalTransport = concreteParent.ArrivalConsol?.Transports.ArrivalTransport;
					var localDateTime = ZDateTime.Empty;

					if (dateTimeSourceType == ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolDischargeETA && arrivalTransport != null)
					{
						localDateTime = arrivalTransport.JW_ETA;
					}
					else if (dateTimeSourceType == ForwardingShipmentEstimateDefaultedFromList.Codes.FCLAvailable)
					{
						localDateTime = concreteParent.DocsAndCartage.JP_FCLAvailable;
					}
					else if (dateTimeSourceType == ForwardingShipmentEstimateDefaultedFromList.Codes.FCLStorage)
					{
						localDateTime = concreteParent.DocsAndCartage.JP_FCLStorageCommences;
					}
					else if (dateTimeSourceType == ForwardingShipmentEstimateDefaultedFromList.Codes.LCLAvailable)
					{
						localDateTime = concreteParent.DocsAndCartage.JP_LCLAvailable;
					}
					else if (dateTimeSourceType == ForwardingShipmentEstimateDefaultedFromList.Codes.LCLStorage)
					{
						localDateTime = concreteParent.DocsAndCartage.JP_LCLStorageCommences;
					}

					return (localDateTime, arrivalTransport?.DiscPort);
				}
			}
			return (ZDateTime.Empty, null);
		}

		#endregion

		#region Validation Rules

		protected override ValidationToolSettings GetValidationToolSettings() => new ForwardingShipmentValidationToolSettings(this);

		#endregion

		protected override void CheckWorkflowTriggerRecipientPartyCore(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo recipientPartyInfo)
		{
			base.CheckWorkflowTriggerRecipientPartyCore(trigger, parent, recipientPartyInfo);
			var shipment = parent as ForwardingShipment;
			var template = parent as ProcessTaskTemplate;
			switch (recipientPartyInfo.Value.ToString())
			{
				case MessageRecipientPartyTypeList.Codes.DeConsolidator:
					if ((shipment != null && shipment.JS_TransportMode != Enterprise.Core.Constants.TransportModes.Air) ||
						(template != null && template.P0_SubType1 != Enterprise.Core.Constants.TransportModes.Air))
					{
						recipientPartyInfo.AddWarning(Res.GetString("9588F3B5-9F1A-4D0F-AB99-21086F80491D", "This recipient is valid only for AIR shipment."));
					}
					if (shipment != null && shipment.AUCusHAWB == null)
					{
						recipientPartyInfo.AddWarning(Res.GetString("B565C56E-A9FC-4BFE-91D5-8AC476309C79", "This recipient is valid only if there is an Australian Air Cargo job attached to this Shipment, but none exists."));
					}
					break;
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					if ((shipment != null && shipment.JS_TransportMode != Enterprise.Core.Constants.TransportModes.Air) ||
						(template != null && template.P0_SubType1 != Enterprise.Core.Constants.TransportModes.Air))
					{
						recipientPartyInfo.AddWarning(Res.GetString("9588F3B5-9F1A-4D0F-AB99-21086F80491D", "This recipient is valid only for AIR shipment."));
					}
					if (shipment != null)
					{
						var hawb = shipment.AUCusHAWB;
						var mawb = hawb == null ? null : hawb.MAWB;
						if (mawb == null)
						{
							recipientPartyInfo.AddWarning(Res.GetString("B565C56E-A9FC-4BFE-91D5-8AC476309C79", "This recipient is valid only if there is an Australian Air Cargo job attached to this Shipment, but none exists."));
						}
						else if (mawb.EffectiveResponsiblePartyOrgHeader == null)
						{
							recipientPartyInfo.AddWarning(mawb.ReasonEffectiveResponsiblePartyIsUnavailable);
						}
					}
					break;
			}
		}

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			if (countriesSupportedAutoSendingMessage.Any(country => trigger.IsForCountry((BusinessObject)parent, country)))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			}
		}

		readonly ZString[] countriesSupportedAutoSendingMessage = new ZString[]
		{
			Core.Constants.CountryCodes.Canada,
			Core.Constants.CountryCodes.SouthAfrica,
			Core.Constants.CountryCodes.UnitedStates,
			Core.Constants.CountryCodes.UnitedKingdom,
			Core.Constants.CountryCodes.France,
			Core.Constants.CountryCodes.Spain
		};

		public override Type WorkflowProviderType
		{
			get { return typeof(ForwardingShipment); }
		}

		protected override AutoGenCustomColumnDefinitionValidation GetAdditionalCustomColumnDefinitionValidation(GenCustomColumnDefinition customColumnDefinition)
		{
			return new PhaseSecurityGenCustomColumnDefinitionValidation(customColumnDefinition);
		}

		protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return new ForwardingShipmentEventDataModel((ForwardingShipment)businessObject);
		}

		#region Macro Values

		protected override IEnumerable<Type> MacroTypesCore(ProcessTaskNotification action)
		{
			var list = base.MacroTypesCore(action).ToList();
			var declaration = GetRelevantDeclaration(action);
			if (declaration != null)
			{
				list.Add(declaration.GetType());
			}
			return list;
		}

		protected override IEnumerable<Type> MacroTypesCore(ProcessTask processTask)
		{
			var list = base.MacroTypesCore(processTask).ToList();
			var declaration = GetRelevantDeclaration(processTask);
			if (declaration != null)
			{
				list.Add(declaration.GetType());
			}
			return list;
		}

		protected override IEnumerable<BusinessObject> MacroRootsCore(ProcessTaskNotification action)
		{
			var declaration = GetRelevantDeclaration(action);
			if (declaration != null)
			{
				var list = new List<BusinessObject>(base.MacroRootsCore(action));
				list.Add(declaration);
				return list;
			}
			else
			{
				return base.MacroRootsCore(action);
			}
		}

		static BusinessObject GetRelevantDeclaration(ProcessTaskNotification action)
		{
			return GetRelevantDeclaration(action.Parent);
		}

		static BusinessObject GetRelevantDeclaration(IBaseTrigger parent)
		{
			BusinessObject result = null;
			if (parent != null)
			{
				var shipment = parent.GetJob() as ForwardingShipment;
				if (shipment != null)
				{
					var notifyingCompanyPK = parent.CompanyPK;
					foreach (var declaration in shipment.Declarations)
					{
						var branch = parent.Factory.Load<GlbBranch>(declaration.JE_GB);
						if (branch != null && branch.GB_GC == notifyingCompanyPK)
						{
							result = (BusinessObject)declaration;
							break;
						}
					}
				}
			}
			return result;
		}

		protected override BusinessObject[] GetUDFMacroDataContextCore(ITriggerConditions item, BusinessObject parent)
		{
			if (parent is ForwardingShipment shipment)
			{
				return shipment.Factory.GetCachedValue(("GetUDFMacroDataContextCore", shipment.PK), () => new[] { item as BusinessObject, shipment.GetDeclaration() });
			}
			else
			{
				return base.GetUDFMacroDataContextCore(item, parent);
			}
		}

		#endregion

		public IEnumerable<string> SupportedTriggerLineTypes
		{
			get
			{
				yield return TriggerLineTypes.Codes.CusExitReport;
				yield return TriggerLineTypes.Codes.CusEntryHeader;
			}
		}
	}
}
