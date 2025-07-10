using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.HelperClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;
using IAUCusHAWB = Enterprise.Integration.Customs.AU.ICusHAWB;
using IAUCusMAWB = Enterprise.Integration.Customs.AU.ICusMAWB;
using ICusUnderbond = Enterprise.Integration.Customs.AU.ICusUnderbond;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public abstract class ForwardingShipmentWorkflowDescriptorTestBase<TShipment, TWorkflowDescriptor> : WorkflowDescriptorTestCase<TWorkflowDescriptor>
		where TShipment : ForwardingShipment
		where TWorkflowDescriptor : ForwardingShipmentWorkflowDescriptor, new()
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.Shipment.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", JobInvoicingConsumerTypes.Shipment.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("2 sub types", 2, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			CodeDescriptionPairList transportModeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
			AssertEquals("", transportModeList[0].Code);
			AssertEquals("All", transportModeList[0].Description);

			AssertEquals("Direction", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			CodeDescriptionPairList directionList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[1].List;
			AssertEquals("", directionList[0].Code);
			AssertEquals("All", directionList[0].Description);
			AssertEquals(DirectionsContext.Import, directionList[1].Code);
			AssertEquals("Import", directionList[1].Description);
			AssertEquals(DirectionsContext.Export, directionList[2].Code);
			AssertEquals("Export", directionList[2].Description);
			AssertEquals(DirectionsContext.Domestic, directionList[3].Code);
			AssertEquals("Domestic", directionList[3].Description);
			AssertEquals(DirectionsContext.CrossTrade, directionList[4].Code);
			AssertEquals("Cross Trade", directionList[4].Description);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestSupportsHVLVPreScreening() => AssertEquals(true, WorkflowDescriptor.SupportsHVLVPreScreening);

		#region ExpectedSupportedTriggerPartyServices

		protected override ZString[] ExpectedSupportedTriggerPartyServices(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse:
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive, ServiceCodesList.Codes.TransitWarehouseDispatch, ServiceCodesList.Codes.TransitWarehouseReceiveAndDispatch, ServiceCodesList.Codes.TransitWarehousePrepareDispatch };
				default:
					return base.ExpectedSupportedTriggerPartyServices(recipient);
			}
		}

		#endregion

		#region Workflow Trigger

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
				MessageRecipientPartyType.ArrivalCarrier |
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
			}
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				List<CodeDescriptionPair> list = new List<CodeDescriptionPair>();
				list.Add(new CodeDescriptionPair(ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendBrokerageXMLDocument, ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Descriptions.SendBrokerageXMLDocument));
				list.Add(new CodeDescriptionPair(ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendSterlingFlatFileWithJobFallback, ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Descriptions.SendSterlingFlatFileWithJobFallback));
				list.Add(new CodeDescriptionPair(ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document, ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Descriptions.SendCargoIMPPhase2Document));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml, WorkflowTriggerActionTypeConstants.Descriptions.SendDescartesXml));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage, WorkflowTriggerActionTypeConstants.Descriptions.ValidateAndSendAirCargoReportMessage));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.CreateBrokerageOnShipment, WorkflowTriggerActionTypeConstants.Descriptions.CreateBrokerageOnShipment));
				return list.ToArray();
			}
		}

		public void TestSupportsWorkflowTriggerAction()
		{
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();

			Assert(workflowDescriptor.SupportsWorkflowTriggerActionXML);
			Assert(workflowDescriptor.SupportsWorkflowTriggerActionXMLWithJobFallback);
			Assert(workflowDescriptor.SupportsWorkflowTriggerActionXMLWithAWB);
		}

		public override void TestIsMessagingOrEmailNotificationTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.SendXML));
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback));
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendBrokerageXMLDocument));
			AssertEquals(false, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.SendDocument));
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail));
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendSterlingFlatFileWithJobFallback));
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml));
			AssertEquals(false, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document));
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking));
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer));
			AssertEquals(false, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.CreateBrokerageOnShipment));
		}

		public void TestGetWorkflowTriggerActionCore()
		{
			ForwardingShipment shipment = NewShipmentWithConfiguredOrganisationParties();
			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendSterlingFlatFileWithJobFallback;

			IProcessor resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendSterlingFlatFileWithJobFallback should be SterlingCommerceMessageDelivery", resultProcessor is SterlingCommerceMessageDelivery);
			Assert(((SterlingCommerceMessageDelivery)resultProcessor).dataAdapter is ForwardingConsolWithShipmentValueObjectDataAdapter);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateBrokerageOnShipment;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for CreateBrokerageOnShipment should be CreateBrokerageOnShipmentProcessor", "CreateBrokerageOnShipmentProcessor", resultProcessor.GetType().Name);

			shipment = Factory.New<ForwardingShipment>();
			processTask = shipment.WorkflowItems.Triggers.AddNew();
			action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendSterlingFlatFileWithJobFallback;

			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendSterlingFlatFileWithJobFallback should be SterlingCommerceMessageDelivery", resultProcessor is SterlingCommerceMessageDelivery);
			Assert(((SterlingCommerceMessageDelivery)resultProcessor).dataAdapter is ForwardingShipmentValueObjectDataAdapter);

			shipment = NewShipmentWithConfiguredOrganisationParties();
			processTask = shipment.WorkflowItems.Triggers.AddNew();
			action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendSterlingFlatFileWithJobFallback should be SterlingCommerceMessageDelivery", resultProcessor is XmlMessageDeliver);

			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendCargoIMPPhase2Document should be ShipmentCargoImpPhase2MessageDelivery", resultProcessor is ShipmentCargoImpPhase2MessageDelivery);

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			processTask = shipment.WorkflowItems.Triggers.AddNew();
			action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			Factory.Save();
		}

		public void TestGetWorkflowTriggerActionCore_HVLVTriggerActions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			Factory.Save();

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling;
			var resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for CreateHVLVImporterSecurityFiling should be ForwardingShipmentToISFProcessor", "ForwardingShipmentToISFProcessor", resultProcessor.GetType().Name);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.RunHVLVPreScreening;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for RunHVLVPreScreening should be HVLVPreScreeningProcessor", "HVLVPreScreeningProcessor", resultProcessor.GetType().Name);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for SendACASReport should be HVLVValidateAndSendAcknowledgementACASReportProcessor ", "HVLVValidateAndSendAcknowledgementACASReportProcessor", resultProcessor.GetType().Name);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for CreateHVLVSeaFreightAMS should be HVLVConvertShipmentToCustomsJobProcessor", "HVLVConvertShipmentToCustomsJobProcessor", resultProcessor.GetType().Name);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for CreateHVLVAirFreightAMS should be HVLVConvertShipmentToCustomsJobProcessor ", "HVLVConvertShipmentToCustomsJobProcessor", resultProcessor.GetType().Name);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for CreateH7Declaration should be HVLVConvertShipmentToCustomsJobProcessor ", "HVLVConvertShipmentToCustomsJobProcessor", resultProcessor.GetType().Name);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for CreateHVLVUSTruckEManifest should be HVLVConvertShipmentToCustomsJobProcessor", "HVLVConvertShipmentToCustomsJobProcessor", resultProcessor.GetType().Name);

			shipment.JS_TransportMode = TransportModes.Sea;

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for AUSeaCargoReport should be HVLVConvertShipmentToCustomsJobProcessor", "HVLVConvertShipmentToCustomsJobProcessor", resultProcessor.GetType().Name);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for NZSeaCargoReport should be HVLVConvertShipmentToCustomsJobProcessor", "HVLVConvertShipmentToCustomsJobProcessor", resultProcessor.GetType().Name);

			shipment.JS_TransportMode = TransportModes.Air;

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for AUAirCargoReport should be HVLVConvertShipmentToCustomsJobProcessor", "HVLVConvertShipmentToCustomsJobProcessor", resultProcessor.GetType().Name);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for NZAirCargoReport should be HVLVConvertShipmentToCustomsJobProcessor", "HVLVConvertShipmentToCustomsJobProcessor", resultProcessor.GetType().Name);
		}

		public void TestGetWorkflowTriggerActionCore_OnlyAllowedForHVLVShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			Factory.Save();

			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.RunHVLVPreScreening);
			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport);
			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS);
			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS);
			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration);
			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest);
			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport);
			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport);
			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport);
			AssertTriggerActionIsOnlyAllowedForHVLVShipment(WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport);

			void AssertTriggerActionIsOnlyAllowedForHVLVShipment(string actionCode)
			{
				action.PQ_TriggerType = actionCode;
				var resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
				AssertEquals(string.Format("Trigger Action Type {0} is only allowed for HighVolumeLowValue shipments.", actionCode), "LogAction", resultProcessor.GetType().Name);
			}
		}

		public void TestGetWorkflowTriggerActionCore_HVLVConvertShipmentToCustomsJobTriggerActions_OnlySupportedBySeaShipments()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			Factory.Save();

			shipment.JS_TransportMode = TransportModes.Air;

			AssertTriggerActionIsOnlySupportedBySeaShipments(WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport);
			AssertTriggerActionIsOnlySupportedBySeaShipments(WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport);

			void AssertTriggerActionIsOnlySupportedBySeaShipments(string actionCode)
			{
				action.PQ_TriggerType = actionCode;
				var resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
				AssertEquals(string.Format("Trigger Action Type {0} is only supported by Sea shipments.", actionCode), "LogAction", resultProcessor.GetType().Name);
			}
		}

		public void TestGetWorkflowTriggerActionCore_HVLVConvertShipmentToCustomsJobTriggerActions_OnlySupportedByAirShipments()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			Factory.Save();

			shipment.JS_TransportMode = TransportModes.Sea;

			AssertTriggerActionIsOnlySupportedBySeaShipments(WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport);
			AssertTriggerActionIsOnlySupportedBySeaShipments(WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport);

			void AssertTriggerActionIsOnlySupportedBySeaShipments(string actionCode)
			{
				action.PQ_TriggerType = actionCode;
				var resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
				AssertEquals(string.Format("Trigger Action Type {0} is only supported by Air shipments.", actionCode), "LogAction", resultProcessor.GetType().Name);
			}
		}

		public void TestGetWorkflowTriggerActionCore_NoJob()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance;

			AssertNull(shipment.Job);
			AssertNoExceptionThrown(() => WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory)));
		}

		public void TestGetWorkflowTriggerActionCore_SendEntryDeclarationMessageAndSendReleaseMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "123";
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var caBranch = caCompany.Branches.AddNew();
			caBranch.GB_Code = "456";
			var cnCompany = Factory.NewWithValidTestData<GlbCompany>();
			cnCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			var cnBranch = cnCompany.Branches.AddNew();
			cnBranch.GB_Code = "789";

			var usDeclaration = Factory.New<IBaseJobDeclaration>();
			usDeclaration.JE_GB = usBranch.PK;
			usDeclaration.JE_JS = shipment.PK;
			usDeclaration.JE_MessageType = "IMP";

			var caDeclaration = Factory.New<IBaseJobDeclaration>();
			caDeclaration.JE_GB = caBranch.PK;
			caDeclaration.JE_JS = shipment.PK;
			caDeclaration.JE_MessageType = "EXP";
			Factory.Save();

			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			processTask.TriggerConditions.TriggerCompany = usCompany.PK;
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			Factory.Save();

			AssertStmProcessQueue(action, WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage, usDeclaration.PK, usBranch.PK);
			AssertStmProcessQueue(action, WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage, usDeclaration.PK, usBranch.PK);

			processTask.TriggerConditions.TriggerCompany = caCompany.PK;
			Factory.Save();
			AssertStmProcessQueue(action, WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage, caDeclaration.PK, caBranch.PK);
			AssertStmProcessQueue(action, WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage, caDeclaration.PK, caBranch.PK);

			processTask.TriggerConditions.TriggerCompany = cnCompany.PK;
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, cnBranch.PK.ToGuid(), Guid.Empty))
			{
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;
				var resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
				AssertEquals("LogAction", resultProcessor.GetType().Name);
				var logger = new NotificationsForTesting();
				resultProcessor.Process(logger);
				AssertContains("No matching declaration could be found for company ", logger.ToString());
			}
		}

		public void TestGetWorkflowTriggerActionCore_SendBrokerageXMLDocument_NotCreateXmlInterchange()
		{
			var processTask = ShipmentWithConfiguredOrganisationParties.WorkflowItems.Triggers.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendBrokerageXMLDocument;

			var resultProcessor = (XmlMessageDeliver)WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			var xmlInterchange = resultProcessor.GetType().GetField("xmlInterchange", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(resultProcessor);
			AssertNull("should not create xmlInterchange for processor in GetWorkflowTriggerAction()", xmlInterchange);
		}

		void AssertStmProcessQueue(ProcessTaskNotification action, ZString actionCode, ZGuid declarationPK, ZGuid branchPK)
		{
			var newFactory = new BusinessObjectFactory();
			var actionLoaded = newFactory.Load<ProcessTaskNotification>(action.PK);
			using (DisposableEnvironment.ForBranch(branchPK.ToGuid()))
			{
				actionLoaded.PQ_TriggerType = actionCode;
				var resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(actionLoaded, new QueuedLogForTesting(newFactory));
				AssertEquals("CustomsStmProcessQueueCreatorProcessor", resultProcessor.GetType().Name);
				var logger = new NotificationsForTesting();
				var stmProcessQueues = newFactory.Load<StmProcessQueue>(new ZQuery(StmProcessQueueSchema.SW_ReferenceID, declarationPK));
				AssertEquals(0, stmProcessQueues.Length);
				resultProcessor.Process(logger);
				stmProcessQueues = newFactory.Load<StmProcessQueue>(new ZQuery(StmProcessQueueSchema.SW_ReferenceID, declarationPK));
				AssertEquals(1, stmProcessQueues.Length);
				var stmProcessQueue = stmProcessQueues[0];
				AssertEquals("ASC", stmProcessQueue.SW_ApplicationCode);
				AssertEquals(actionCode, stmProcessQueue.SW_ActionCode);
				stmProcessQueue.Delete();
			}
		}

		class NotificationsForTesting : INotifications
		{
			public void Add(INotification notification)
			{
				notifications.Add(notification.Message);
			}

			readonly List<string> notifications = new List<string>();

			public override string ToString()
			{
				return string.Join("\r\n", notifications.ToArray());
			}
		}

		/// <summary>
		/// Link to some Organisation parties are removed.
		/// So These should NOT have any messages delivered, although all party types are requested.
		/// </summary>
		public void TestGetWorkflowTriggerAction_WithSomeMissingPartyLinks()
		{
			MessageRecipientPartyType partyType =
				MessageRecipientPartyType.BillToParty
				| MessageRecipientPartyType.Broker
				| MessageRecipientPartyType.ImportBroker
				| MessageRecipientPartyType.Consignee
				| MessageRecipientPartyType.Consignor
				| MessageRecipientPartyType.DeliveryCartage
				| MessageRecipientPartyType.PickupCartage
				| MessageRecipientPartyType.PickupAgent
				| MessageRecipientPartyType.DeliveryAgent
				| MessageRecipientPartyType.ReceivingAgent
				| MessageRecipientPartyType.SendingAgent
				| MessageRecipientPartyType.ControllingAgent
				| MessageRecipientPartyType.ControllingCustomer
				| MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.PickupParty
				| MessageRecipientPartyType.DeliveryToParty
				| MessageRecipientPartyType.NotifyParty
				| MessageRecipientPartyType.ArrivalCarrier
				| MessageRecipientPartyType.DepartureCarrier
				| MessageRecipientPartyType.DepartureTransitWarehouse
				| MessageRecipientPartyType.ArrivalTransitWarehouse;
			OrgHeader[] partiesWithDeliveredModes =
			{
				ReceivingAgentOrg,
				SendingAgentOrg,
				PickupCartageOrg,
				DeliveryCartageOrg,
				BillToPartyOrg,
				OrgProxyOrg,
				ImportReleaseDepotOrg,
				ExportReceivingDepotOrg,
			};

			// Remove links to some organisations. No message should be delivered for these party types.
			ShipmentWithConfiguredOrganisationParties.ConsignorPK = ZGuid.Empty;
			ShipmentWithConfiguredOrganisationParties.ConsigneePK = ZGuid.Empty;
			ShipmentWithConfiguredOrganisationParties.JS_OH_ImportBroker = ZGuid.Empty;
			ShipmentWithConfiguredOrganisationParties.ConsignorPickupAddress.OrganisationPK = ZGuid.Empty;
			ShipmentWithConfiguredOrganisationParties.ConsigneeDeliveryAddress.OrganisationPK = ZGuid.Empty;
			ShipmentWithConfiguredOrganisationParties.NotifyPartyDocumentaryAddress.OrganisationPK = ZGuid.Empty;

			RunDeliveryWorkflowTriggerAction_AndAssertXmlMessageDelivery(
				ShipmentWithConfiguredOrganisationParties,
				partyType,
				partiesWithDeliveredModes,
				WorkflowTriggerActionTypeConstants.Codes.SendXML);
		}

		public void TestGetWorkflowTriggerAction_SendBrokerageXMLDocument()
		{
			MessageRecipientPartyType partyType =
				MessageRecipientPartyType.BillToParty
				| MessageRecipientPartyType.Broker
				| MessageRecipientPartyType.Consignee
				| MessageRecipientPartyType.Consignor
				| MessageRecipientPartyType.DeliveryCartage
				| MessageRecipientPartyType.PickupCartage
				| MessageRecipientPartyType.PickupAgent
				| MessageRecipientPartyType.DeliveryAgent
				| MessageRecipientPartyType.ReceivingAgent
				| MessageRecipientPartyType.SendingAgent
				| MessageRecipientPartyType.ControllingAgent
				| MessageRecipientPartyType.ControllingCustomer
				| MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.PickupParty
				| MessageRecipientPartyType.DeliveryToParty
				| MessageRecipientPartyType.NotifyParty
				| MessageRecipientPartyType.ArrivalCarrier
				| MessageRecipientPartyType.DepartureCarrier
				| MessageRecipientPartyType.DepartureTransitWarehouse
				| MessageRecipientPartyType.ArrivalTransitWarehouse;

			OrgHeader[] partiesWithDeliveredModes =
			{
				ReceivingAgentOrg,
				SendingAgentOrg,
				PickupCartageOrg,
				DeliveryCartageOrg,
				PickupAgentOrg,
				DeliveryAgentOrg,
				BillToPartyOrg,
				ControllingAgentOrg,
				ControllingCustomerOrg,
				OrgProxyOrg,
				BrokerOrg,
				ConsignorOrg,
				ConsigneeOrg,
				PickupFromOrg,
				DeliverToOrg,
				NotifyPartyOrg,
				ArrivalCarrierOrg,
				DepartureCarrierOrg,
				ImportReleaseDepotOrg,
				ExportReceivingDepotOrg,
			};

			RunDeliveryWorkflowTriggerAction_AndAssertXmlMessageDelivery(
				ShipmentWithConfiguredOrganisationParties,
				partyType,
				partiesWithDeliveredModes,
				ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendBrokerageXMLDocument);
		}

		public void TestGetWorkflowTriggerAction_SendDescartesXML()
		{
			CargoWise.Data.Db.Connection.BeginTransaction();        // We are not saving a factory but instead performing a real transaction
			try
			{
				MessageRecipientPartyType partyType =
					MessageRecipientPartyType.BillToParty
					| MessageRecipientPartyType.Broker
					| MessageRecipientPartyType.Consignee
					| MessageRecipientPartyType.Consignor
					| MessageRecipientPartyType.DeliveryCartage
					| MessageRecipientPartyType.PickupCartage
					| MessageRecipientPartyType.PickupAgent
					| MessageRecipientPartyType.DeliveryAgent
					| MessageRecipientPartyType.ReceivingAgent
					| MessageRecipientPartyType.SendingAgent
					| MessageRecipientPartyType.ControllingAgent
					| MessageRecipientPartyType.ControllingCustomer
					| MessageRecipientPartyType.OrgProxy
					| MessageRecipientPartyType.PickupParty
					| MessageRecipientPartyType.DeliveryToParty
					| MessageRecipientPartyType.NotifyParty
					| MessageRecipientPartyType.ArrivalCarrier
					| MessageRecipientPartyType.DepartureCarrier
					| MessageRecipientPartyType.DepartureTransitWarehouse
					| MessageRecipientPartyType.ArrivalTransitWarehouse;

				OrgHeader[] partiesWithDeliveredModes =
			{
				ReceivingAgentOrg,
				SendingAgentOrg,
				PickupCartageOrg,
				DeliveryCartageOrg,
				PickupAgentOrg,
				DeliveryAgentOrg,
				BillToPartyOrg,
				ControllingAgentOrg,
				ControllingCustomerOrg,
				OrgProxyOrg,
				BrokerOrg,
				ConsignorOrg,
				ConsigneeOrg,
				PickupFromOrg,
				DeliverToOrg,
				NotifyPartyOrg,
				ArrivalCarrierOrg,
				DepartureCarrierOrg,
				ImportReleaseDepotOrg,
				ExportReceivingDepotOrg,
			};

				foreach (OrgHeader org in partiesWithDeliveredModes)
				{
					EDICommunicationsMode dxlMode = Factory.New<EDICommunicationsMode>();
					dxlMode.EK_Module = WorkflowDescriptor.Code;
					dxlMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.DXL;
					dxlMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
					dxlMode.EK_Destination = org.OH_FullName + "@notificationemail.cargowise.com";
					org.EDICommunicationsModes.Add(dxlMode);
				}

				RunDeliveryWorkflowTriggerAction_AndAssertXmlMessageDelivery(
					ShipmentWithConfiguredOrganisationParties,
					partyType,
					partiesWithDeliveredModes,
					WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml);
			}
			finally
			{
				CargoWise.Data.Db.Connection.RollbackTransaction();     // We are not saving a factory but instead performing a real transaction
			}
		}

		public void TestGetAndRunWorkflowTriggerActionForXmlMessageDeliveryWithFallback_ToConsol()
		{
			ForwardingShipment shipment = NewShipmentWithConfiguredOrganisationParties();
			shipment.JS_HouseBill = "~TheShipment~";
			shipment.JS_UniqueConsignRef = "ShipmentID";
			SetupAUCusHAWBWithUnderbond(shipment, EDICommunicationsModeFileFormatList.Codes.XML);

			// Add other shipments to the test shipment consol
			ForwardingShipment anotherShipment1 = shipment.Consols[0].Shipments.AddNew();
			anotherShipment1.JS_HouseBill = "~AnotherShipment1~";
			ForwardingShipment anotherShipment2 = shipment.Consols[0].Shipments.AddNew();
			anotherShipment2.JS_HouseBill = "~AnotherShipment2~";
			AssertEquals("[PRE-CONDITION] Consol Shipment Count", 3, shipment.Consols[0].Shipments.Count);

			var task = shipment.WorkflowItems.AddNew();
			OrgHeader[] expectedRecipientParties = GetExpectedOrganisationsForPartyType(task, WorkflowDescriptor.SupportedMessageRecipientParties(task, shipment));

			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunDeliveryWorkflowTriggerActionForParent(
				shipment,
				WorkflowDescriptor.SupportedMessageRecipientParties(task, shipment),
				WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);

			List<AttachmentDef> attachments = Env.OutgoingMailManager.EmailsCreated.SelectMany(emailDef => emailDef.Attachments.Cast<AttachmentDef>()).ToList();
			Assert(attachments.All(x => !string.IsNullOrEmpty(x.DisplayName)));

			int noOfEmailsExpected = expectedRecipientParties.Length + 3;
			// one for MessageRecipientPartyTypeList.Codes.Email and one each for the Import and Export Broker as these return the same thing as Broker but this test is not smart enough to tell that.

			AssertEquals("Number of messages created", noOfEmailsExpected, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("Delivered XML top level object should be a Consol", true, xmlText.Contains("</CONSOL>"));
			AssertEquals("Delivered XML should contain Test Shipment", true, xmlText.Contains(shipment.JS_HouseBill.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Shipment 1", false, xmlText.Contains(anotherShipment1.JS_HouseBill.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Shipment 2", false, xmlText.Contains(anotherShipment2.JS_HouseBill.ToUpper()));
			AssertEquals("(*JobNumber*) is from the Shipment", true, message.Attachments[0].DisplayName.EndsWith("ShipmentID"));
		}

		public void TestGetAndRunWorkflowTriggerActionWhereNoArrivalConsol()
		{
			var shipment = NewShipmentWithConfiguredOrganisationParties();
			var importReleaseDepotPK = shipment.JS_OA_ImportReleaseDepot;
			shipment.JS_HouseBill = "~TheShipment~";
			shipment.JS_UniqueConsignRef = "ShipmentID";
			shipment.JS_RL_NKDestination = "NZAKL"; // Not in same country as consol's destination, so shipment will have no ArrivalConsol.
			shipment.JS_OA_ImportReleaseDepot = importReleaseDepotPK; // set this after Destination as the address gets cleared out.
			SetupAUCusHAWBWithUnderbond(shipment, EDICommunicationsModeFileFormatList.Codes.XML);

			// Add other shipments to the test shipment consol
			var anotherShipment1 = shipment.Consols[0].Shipments.AddNew();
			anotherShipment1.JS_HouseBill = "~AnotherShipment1~";
			var anotherShipment2 = shipment.Consols[0].Shipments.AddNew();
			anotherShipment2.JS_HouseBill = "~AnotherShipment2~";
			AssertEquals("[PRE-CONDITION] Consol Shipment Count", 3, shipment.Consols[0].Shipments.Count);
			var task = shipment.WorkflowItems.AddNew();
			var expectedRecipientParties = GetExpectedOrganisationsForPartyType(task, WorkflowDescriptor.SupportedMessageRecipientParties(task, shipment));

			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunDeliveryWorkflowTriggerActionForParent(
				shipment,
				WorkflowDescriptor.SupportedMessageRecipientParties(task, shipment),
				WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);

			int noOfEmailsExpected = expectedRecipientParties.Length + 1;
			// one for MessageRecipientPartyTypeList.Codes.Email. This test is not smart enough to tell that.

			AssertEquals("Number of messages created", noOfEmailsExpected, Env.OutgoingMailManager.EmailsCreated.Count);

			var message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("Delivered XML top level object should be a Consol", true, xmlText.Contains("</CONSOL>"));
			AssertEquals("Delivered XML should contain Test Shipment", true, xmlText.Contains(shipment.JS_HouseBill.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Shipment 1", false, xmlText.Contains(anotherShipment1.JS_HouseBill.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Shipment 2", false, xmlText.Contains(anotherShipment2.JS_HouseBill.ToUpper()));
			AssertEquals("(*JobNumber*) is from the Shipment", true, message.Attachments[0].DisplayName.EndsWith("ShipmentID"));
		}

		public void TestTriggerActionForXmlWithFallback_ForShipmentWithNoneOrMultipleConsols()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			shipment.ConsigneePK = ConsigneeOrg.PK;
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = NotifyPartyOrg.PK;
			Factory.Save();

			AssertEquals("It is a standalone shipment", 0, shipment.Consols.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunDeliveryWorkflowTriggerActionForParent(
				shipment,
				MessageRecipientPartyType.Consignee,
				WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);

			AssertEquals("Number of messages created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			Assert("Delivered XML top level object should NOT be a Consol", !xmlText.Contains("</CONSOL>"));
			AssertEquals("Delivered XML should contain Test Shipment", true, xmlText.Contains(shipment.JS_HouseBill.ToUpper()));

			var firstConsol = shipment.Consols.AddNew();
			firstConsol.JK_RL_NKLoadPort = "AUSYD";
			firstConsol.JK_RL_NKDischargePort = "HKHKG";
			firstConsol.JK_MasterBillNum = "FIRSTCONSOL";

			var secondConsol = shipment.Consols.AddNew();
			secondConsol.JK_RL_NKLoadPort = "HKHKG";
			secondConsol.JK_RL_NKDischargePort = "USCHI";
			secondConsol.JK_MasterBillNum = "SECONDCONSOL";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunDeliveryWorkflowTriggerActionForParent(
				shipment,
				MessageRecipientPartyType.Consignee,
				WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);

			AssertEquals("Number of messages created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			message = Env.OutgoingMailManager.EmailsCreated[0];
			xmlData = message.Attachments[0].Data;
			xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("Delivered XML top level object should be a Consol", true, xmlText.Contains("</CONSOL>"));
			AssertEquals("Delivered XML should contain local consol", true, xmlText.Contains(firstConsol.JK_MasterBillNum.ToUpper()));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunDeliveryWorkflowTriggerActionForParent(
				shipment,
				MessageRecipientPartyType.Consignee,
				WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);

			AssertEquals("Number of messages created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			message = Env.OutgoingMailManager.EmailsCreated[0];
			xmlData = message.Attachments[0].Data;
			xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("Delivered XML top level object should be a Consol", true, xmlText.Contains("</CONSOL>"));
			AssertEquals("Delivered XML should contain local consol", true, xmlText.Contains(secondConsol.JK_MasterBillNum.ToUpper()));
		}

		public void TestTriggerActionForXml_MultiConsols_UsingCorrectContainerNumberForPackingLine()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			shipment.ConsigneePK = ConsigneeOrg.PK;
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = NotifyPartyOrg.PK;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "HKHKG";
			consol1.JK_RL_NKDischargePort = "AUSYD";
			consol1.JK_MasterBillNum = "FIRSTCONSOL";

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "XCV00001";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "USCHI";
			consol2.JK_MasterBillNum = "SECONDCONSOL";

			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "XCV00002";

			var packingLine = shipment.OuterPackLines.AddNew();
			packingLine.JL_F3_NKPackType = "PKG";
			packingLine.SetContainer(consol2, container2);

			AssertContainerNumber(shipment, "XCV00002", WorkflowTriggerActionTypeConstants.Codes.SendXML);
			AssertContainerNumber(shipment, "XCV00002", WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
		}

		public void TestGetAction_NoNull()
		{
			// Set valid workflow provider for descriptor under test
			var shipment = Factory.New<ForwardingShipment>();
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);

			// Set invalid trigger action type for descriptor under test
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage;
			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(wteLog, trigger));
			AssertNull(processor);
		}

		public void TestCustomsMessageActions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				var template = Factory.New<ProcessTaskTemplate>();
				var trigger = template.WorkflowItems.Triggers.AddNew();
				var testItem = new ForwardingShipmentWorkflowDescriptor();

				CombineAssertions(() =>
				{
					template.GlobalTemplate = false;
					var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
					Assert("Should contain VCM for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));
					Assert("Should contain SEM for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage));
					Assert("Should contain SRM for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage));
					Assert("Should contain SB3 for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message));
					Assert("Should contain AVS for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery));

					template.GlobalTemplate = true;
					testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
					Assert("Should NOT contain VCM for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));
					Assert("Should NOT contain SEM for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage));
					Assert("Should NOT contain SRM for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage));
					Assert("Should NOT contain SB3 for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message));
					Assert("Should NOT contain AVS for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery));
				});
			}
		}

		public void TestUSSeaTemplateActions()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var trigger = template.WorkflowItems.Triggers.AddNew();
			var testItem = new ForwardingShipmentWorkflowDescriptor();

			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = CountryCodes.Australia;
			template.P0_SubType1 = TransportModes.Air;
			var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("Not SEA template", () =>
			{
				Assert("Should NOT contain HIS", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling));
				Assert("Should NOT contain HAM", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS));
			});

			template.P0_DischargePortCountry = CountryCodes.Australia;
			template.P0_LoadPortCountry = CountryCodes.Australia;
			template.P0_SubType1 = TransportModes.Sea;
			testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("Not US Destination", () =>
			{
				Assert("Should NOT contain HIS", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling));
				Assert("Should NOT contain HAM", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS));
			});

			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = CountryCodes.UnitedStates;
			template.P0_SubType1 = TransportModes.Sea;
			testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("Not Non-US Origin", () =>
			{
				Assert("Should NOT contain HIS", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling));
				Assert("Should NOT contain HAM", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS));
			});

			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = CountryCodes.Australia;
			template.P0_SubType1 = TransportModes.Sea;
			testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("SEA template, Non-US Origin, US Destination", () =>
			{
				Assert("Should contain HIS", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling));
				Assert("Should contain HAM", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS));
			});

			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = ZString.Empty;
			template.P0_SubType1 = TransportModes.Sea;
			testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("SEA template, empty Origin, US Destination", () =>
			{
				Assert("Should contain HIS", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling));
				Assert("Should contain HAM", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS));
			});
		}

		public void TestUSSeaShipmentActions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			var testItem = new ForwardingShipmentWorkflowDescriptor();

			CombineAssertions(() =>
			{
				var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should NOT contain HIS for non US sea shipment", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling));
				Assert("Should NOT contain HAM for non US sea shipment", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS));

				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = TransportModes.Sea;

				testResult = testItem.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should contain HIS for US sea shipment", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling));
				Assert("Should contain HAM for US sea shipment", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS));
			});
		}

		public void TestUSAirTemplateActions()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var trigger = template.WorkflowItems.Triggers.AddNew();
			var testItem = new ForwardingShipmentWorkflowDescriptor();

			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = CountryCodes.Australia;
			template.P0_SubType1 = TransportModes.Sea;
			var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("Not AIR template", () =>
			{
				Assert("Should NOT contain HAS", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS));
				Assert("Should NOT contain HAK", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport));
			});

			template.P0_DischargePortCountry = CountryCodes.Australia;
			template.P0_LoadPortCountry = CountryCodes.Australia;
			template.P0_SubType1 = TransportModes.Air;
			testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("Not Non-US Origin", () =>
			{
				Assert("Should NOT contain HAS", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS));
				Assert("Should NOT contain HAK", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport));
			});

			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = CountryCodes.UnitedStates;
			template.P0_SubType1 = TransportModes.Air;
			testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("Not US Origin", () =>
			{
				Assert("Should NOT contain HAS", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS));
				Assert("Should NOT contain HAK", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport));
			});

			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = CountryCodes.Australia;
			template.P0_SubType1 = TransportModes.Air;
			testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("AIR template, Non-US Origin, US Destination", () =>
			{
				Assert("Should contain HAS", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS));
				Assert("Should contain HAK", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport));
			});

			template.P0_DischargePortCountry = CountryCodes.UnitedStates;
			template.P0_LoadPortCountry = ZString.Empty;
			template.P0_SubType1 = TransportModes.Air;
			testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
			CombineAssertions("AIR template, empty Origin, US Destination", () =>
			{
				Assert("Should contain HAS", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS));
				Assert("Should contain HAK", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport));
			});
		}

		public void TestUSAirShipmentActions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			var testItem = new ForwardingShipmentWorkflowDescriptor();

			CombineAssertions(() =>
			{
				var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should NOT contain HAS for non US air shipment", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS));
				Assert("Should NOT contain HAK for non US air shipment", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport));

				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = TransportModes.Air;

				testResult = testItem.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should contain HAS for US air shipment", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS));
				Assert("Should contain HAK for US air shipment", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport));
			});
		}

		public void TestWhenAMSISFEnabledForHVLVItemIsTrue_ThenIncludeAMSISFTriggersInWorkflowTriggerActionTypesList()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			var testItem = new ForwardingShipmentWorkflowDescriptor();
			shipment.JS_RL_NKDestination = "USCHI";

			CombineAssertions(() =>
			{
				shipment.JS_TransportMode = TransportModes.Air;
				var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should contain HAS when registry is enabled", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS));

				shipment.JS_TransportMode = TransportModes.Sea;
				testResult = testItem.GetWorkflowTriggerActionTypes(trigger, shipment);
				Assert("Should contain HAM when registry is enabled", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS));
				Assert("Should contain HIS when registry is enabled", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling));
			});
		}

		public void TestGetWorkflowTriggerAction_NoErrorWhenShipmentDestinationCountryCodeIsEmpty()
		{
			var refUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO.RL_RN_NKCountryCode = "";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = refUNLOCO.RL_Code;

			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			var testItem = new ForwardingShipmentWorkflowDescriptor();

			AssertNoExceptionThrown(() =>
			{
				var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, shipment);
			});
		}

		public void TestUpdateAndSaveShipment_WhenHISTriggerExists_DoesNotPromptError()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_TransportMode = TransportModes.Sea;

			var processTask = shipment.WorkflowItems.Triggers.AddNew();
			processTask.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling;

			shipment.Logs.AddNew(Events.Arrival);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, processTask.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];
			WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerWTELog, processTask));

			shipment.JS_RL_NKOrigin = "AUSYD";
			Factory.Save();

			AssertNoExceptionThrown(() => WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerWTELog, processTask)));
		}

		void AssertContainerNumber(ForwardingShipment shipment, ZString expectedNumber, ZString actionType)
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunDeliveryWorkflowTriggerActionForParent(
				shipment,
				MessageRecipientPartyType.Consignee,
				actionType);

			AssertEquals("Number of messages created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var message = Env.OutgoingMailManager.EmailsCreated[0];
			var xmlData = message.Attachments[0].Data;
			var doc = new XmlDocument();

			using (var xmlStream = new MemoryStream(xmlData))
			using (var xmlReader = new XmlTextReader(xmlStream))
			{
				doc.Load(xmlReader);
			}

			var containerNumber = doc.SelectSingleNode("//*[local-name()='Package'][*[local-name()='PackType']='PKG']").SelectSingleNode("//*[local-name()='ContainerNumber']");

			AssertNotNull(containerNumber);
			AssertEquals(expectedNumber, containerNumber.InnerText);
		}

		#region Shipments With Configured Organisation Parties

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				ShipmentWithConfiguredOrganisationParties,
			};
		}

		protected override IWorkflowProvider GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			return ShipmentWithConfiguredOrganisationParties;
		}

		protected ForwardingShipment ShipmentWithConfiguredOrganisationParties
		{
			get { return shipmentWithConfiguredOrganisationParties ?? (shipmentWithConfiguredOrganisationParties = NewShipmentWithConfiguredOrganisationParties()); }
		}
		ForwardingShipment shipmentWithConfiguredOrganisationParties;

		ForwardingShipment NewShipmentWithConfiguredOrganisationParties()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";

			shipment.ConsigneePK = ConsigneeOrg.PK;
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.JS_OH_ExportBroker = BrokerOrg.PK;
			shipment.JS_OH_ImportBroker = BrokerOrg.PK;

			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = NotifyPartyOrg.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = DeliverToOrg.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = PickupFromOrg.PK;
			shipment.DocAddresses.CreateWithAddressType(DocAddressType.Warehouse).OrganisationPK = WarehouseInwardsOrg.PK;
			shipment.ControllingCustomerAddress.OrganisationPK = ControllingCustomerOrg.PK;
			shipment.ControllingAgentDocumentaryAddress.OrganisationPK = ControllingAgentOrg.PK;
			shipment.JS_OA_ExportReceivingDepot = ExportReceivingDepotOrg.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = ImportReleaseDepotOrg.MainAddress.PK;

			shipment.PickupAgentDocumentaryAddress.OrganisationPK = PickupAgentOrg.PK;
			shipment.JS_OH_DeliveryAgent = DeliveryAgentOrg.PK;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_OA_ReceivingForwarderAddress = ReceivingAgentOrg.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = SendingAgentOrg.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = DepartureCarrierOrg.MainAddress.PK;
			shipment.Consols.Add(consol);

			JobDocsAndCartage shipmentDocs = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			shipmentDocs.PickupCartageCoPK = PickupCartageOrg.PK;
			shipmentDocs.DeliveryCartageCoPK = DeliveryCartageOrg.PK;

			JobHeader.Loader jobLoader = new JobHeader.Loader(shipment);
			JobHeader job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			return shipment;
		}

		#endregion

		#endregion

		#region GetWorkflowTriggerFieldColumns()

		public void TestWorkflowTriggerFieldColumns_Parent()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			workflowTriggerFieldColumnsWithBrokerage = true;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(FormattableString.Invariant($"ForwardingShipment: Field change columns need to be declared in {nameof(ExpectedWorkflowTriggerFieldColumns)}."),
					ExpectedWorkflowTriggerFieldColumns.Select(c => c.Name), WorkflowDescriptor.GetWorkflowTriggerFieldColumns(shipment).Select(c => c.Name));

				var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				AssertContainsExactElementsInAnyOrder(FormattableString.Invariant($"ProcessTaskTemplate: Field change columns need to be declared in {nameof(ExpectedWorkflowTriggerFieldColumns)}."),
					ExpectedWorkflowTriggerFieldColumns.Select(c => c.Name), WorkflowDescriptor.GetWorkflowTriggerFieldColumns(processTaskTemplate).Select(c => c.Name));
			});
		}

		bool workflowTriggerFieldColumnsWithBrokerage;

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return workflowTriggerFieldColumnsWithBrokerage ? new SchemaColumn[]
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
						JobDocsAndCartageSchema.JP_EstimatedPickup,
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
					} : new SchemaColumn[]
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
			}
		}

		#endregion

		#region EstimateDefaultedFromList

		protected override TimeSpan GetUtcOffsetForDateTimeSourceType(IWorkflowProvider workflowProvider, string dateTimeSourceType)
		{
			switch (dateTimeSourceType)
			{
				case ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolLoadingETD:
				case ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD:
					return TimeSpan.FromHours(11); // The shipment departs from AUSYD.

				case ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolDischargeETA:
				case ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA:
				case ForwardingShipmentEstimateDefaultedFromList.Codes.FCLAvailable:
				case ForwardingShipmentEstimateDefaultedFromList.Codes.FCLStorage:
				case ForwardingShipmentEstimateDefaultedFromList.Codes.LCLAvailable:
				case ForwardingShipmentEstimateDefaultedFromList.Codes.LCLStorage:
					return TimeSpan.FromHours(10); // The shipment arrives in AUBNE.

				default:
					return base.GetUtcOffsetForDateTimeSourceType(workflowProvider, dateTimeSourceType);
			}
		}

		protected override void SetDateTimeSourcePropertyValue(IWorkflowProvider workflowProvider, string dateTimeSourceType, ZDateTime localTime)
		{
			switch (dateTimeSourceType)
			{
				case ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA:
					((TShipment)workflowProvider).JS_E_ARV = localTime;
					break;

				case ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD:
					((TShipment)workflowProvider).JS_E_DEP = localTime;
					break;

				case ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolDischargeETA:
					((TShipment)workflowProvider).ArrivalConsol.Transports.ArrivalTransport.JW_ETA = localTime;
					break;

				case ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolLoadingETD:
					((TShipment)workflowProvider).ArrivalConsol.Transports.ArrivalTransport.JW_ETD = localTime;
					break;

				case ForwardingShipmentEstimateDefaultedFromList.Codes.FCLAvailable:
					((TShipment)workflowProvider).DocsAndCartage.JP_FCLAvailable = localTime;
					break;

				case ForwardingShipmentEstimateDefaultedFromList.Codes.FCLStorage:
					((TShipment)workflowProvider).DocsAndCartage.JP_FCLStorageCommences = localTime;
					break;

				case ForwardingShipmentEstimateDefaultedFromList.Codes.LCLAvailable:
					((TShipment)workflowProvider).DocsAndCartage.JP_LCLAvailable = localTime;
					break;

				case ForwardingShipmentEstimateDefaultedFromList.Codes.LCLStorage:
					((TShipment)workflowProvider).DocsAndCartage.JP_LCLStorageCommences = localTime;
					break;

				default:
					base.SetDateTimeSourcePropertyValue(workflowProvider, dateTimeSourceType, localTime);
					break;
			}
		}

		#endregion

		#region Implementation

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);
			switch (partyTypeCode)
			{
				case MessageRecipientPartyTypeList.Codes.DeConsolidator:
					SetupAUCusHAWBWithUnderbond(workflowProvider as ForwardingShipment);
					break;
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					SetupAUCusHAWBWithResponsibleParty(workflowProvider as ForwardingShipment);
					break;
			}
		}

		void SetupAUCusHAWBWithUnderbond(ForwardingShipment shipment, string fileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail)
		{
			var mawb = Factory.New<IAUCusMAWB>();
			var hawb = Factory.New<IAUCusHAWB>();
			hawb.CS_CM = mawb.PK;
			hawb.CS_JS = shipment.PK;

			var underbond = Factory.New<ICusUnderbond>();
			underbond.LinkedObject = mawb as BusinessObject;
			underbond.C4_MovementReason = "DCL";
			underbond.C4_DestinationPremiseID = "9999Z";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationMode.EK_Destination = orgHeader.OH_FullName + "@notificationemail.cargowise.com";
			communicationMode.EK_FileFormat = fileFormat;
			communicationMode.EK_Module = WorkflowDescriptor.Code;

			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			orgCusCode.OK_CustomsRegNo = "9999Z";
			orgCusCode.OK_OH = orgHeader.PK;
		}

		void SetupAUCusHAWBWithResponsibleParty(ForwardingShipment shipment, string fileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var mawb = Factory.New<IAUCusMAWB>();
			mawb.CM_OH_ResponsibleParty = orgHeader.PK;

			var hawb = Factory.New<IAUCusHAWB>();
			hawb.CS_CM = mawb.PK;
			hawb.CS_JS = shipment.PK;

			var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationMode.EK_Destination = orgHeader.OH_FullName + "@notificationemail.cargowise.com";
			communicationMode.EK_FileFormat = fileFormat;
			communicationMode.EK_Module = WorkflowDescriptor.Code;
		}

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			switch (table.TableName)
			{
				case JobShipmentSchema.Constants.TableName:
					return Factory.New<ForwardingShipment>();

				case JobConsolSchema.Constants.TableName:
					return Factory.New<ForwardingConsol>();

				default:
					return base.NewBusinessObjectInTable(table);
			}
		}

		#endregion
	}
}
