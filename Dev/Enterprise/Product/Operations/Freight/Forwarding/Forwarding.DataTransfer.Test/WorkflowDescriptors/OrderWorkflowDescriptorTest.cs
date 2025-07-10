using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderWorkflowDescriptor))]
	class OrderWorkflowDescriptorTest : WorkflowDescriptorTestCase<OrderWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "ORD", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Forwarding Order", WorkflowDescriptor.Description);
		}

		public void TestMilestoneTemplateHintCaption()
		{
			AssertEquals("Shipment milestones are combined with the Order milestones below to produce the complete list.", WorkflowDescriptor.MilestoneTemplateHintCaption);
		}

		public override void TestSubTypes()
		{
			AssertEquals("1 sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Transport Mode", "Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);

			CodeDescriptionPairList transportModeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
			AssertEquals("", transportModeList[0].Code);
			AssertEquals("All", transportModeList[0].Description);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestConditionList1()
		{
			AssertEquals(typeof(OrderWorkflowCondition1CodeList), WorkflowDescriptor.GetConditionList1(null).GetType());
		}

		public void TestConditionList2()
		{
			AssertEquals(typeof(OrderWorkflowCondition2CodeList), WorkflowDescriptor.GetConditionList2(null).GetType());
		}

		public void TestEstimateDefaultedFromList()
		{
			AssertEquals(typeof(OrderMilestoneEstimateDefaultedFromList), WorkflowDescriptor.EstimateDefaultedFromList.GetType());
		}

		public void TestValidationToolSettings()
		{
			AssertType<OrderValidationToolSettings>(WorkflowDescriptor.ValidationToolSettings);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		#region Workflow Triggers

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					JobOrderLineSchema.JO_QtyReceived,
					JobOrderHeaderSchema.JD_BookingConfRef,
					JobOrderHeaderSchema.JD_DeliveryRequiredBy
				};
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.Consignor |
					MessageRecipientPartyType.BillToParty |
					MessageRecipientPartyType.Broker |
					MessageRecipientPartyType.PickupCartage |
					MessageRecipientPartyType.DeliveryCartage |
					MessageRecipientPartyType.ReceivingAgent |
					MessageRecipientPartyType.SendingAgent |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.WarehouseInwards;
			}
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.Order, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestOrderXmlWithJobFallbackTrigger_ExportsOnlyTriggeredOrderWhenExportingPreadvice()
		{
			JobShipmentPreplanning preplanning = Factory.New<JobShipmentPreplanning>();

			Order order1 = GetOrderWithOrgInfo();
			order1.JD_EF_ShipmentPrePlanning = preplanning.PK;
			order1.JD_OrderNumber = "111";

			Order order2 = GetOrderWithOrgInfo();
			order2.JD_OrderNumber = "222";
			order2.JD_EF_ShipmentPrePlanning = preplanning.PK;

			Order order3 = GetOrderWithOrgInfo();
			order3.JD_OrderNumber = "333";
			order3.JD_EF_ShipmentPrePlanning = preplanning.PK;

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var provider = (IWorkflowProvider)order2;
			var task = provider.WorkflowItems.AddNew();
			RunDeliveryWorkflowTriggerActionForParent(provider, WorkflowDescriptor.SupportedMessageRecipientParties(task, order2), WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
			Assert("Precondition: At least one message was created", Env.OutgoingMailManager.EmailsCreated.Count > 0);

			EmailDef message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("XML should not contain order1", false, xmlText.Contains("<ORDERNUMBER>111</ORDERNUMBER>"));
			AssertEquals("XML should contain order2", true, xmlText.Contains("<ORDERNUMBER>222</ORDERNUMBER>"));
			AssertEquals("XML should not contain order3", false, xmlText.Contains("<ORDERNUMBER>333</ORDERNUMBER>"));
		}

		public void TestGetAndRunWorkflowTriggerActionForXmlMessageDeliveryWithFallback_ToShipment()
		{
			Order testOrder = GetOrderWithOrgInfo();
			testOrder.JD_OrderNumber = "_TheOrder_";
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			testOrder.Shipment.Consols.RemoveAndDeleteAll();

			// Add other orders to the test order shipment
			Order anotherOrder1 = AddNewOrderToShipment(testOrder.Shipment.PK, "_AnotherOrder1_");
			Order anotherOrder2 = AddNewOrderToShipment(testOrder.Shipment.PK, "_AnotherOrder2_");
			AssertEquals("[PRE-CONDITION] Shipment Order Count", 3, testOrder.Shipment.AttachedOrders.Count);

			var provider = (IWorkflowProvider)testOrder;
			var task = provider.WorkflowItems.AddNew();
			OrgHeader[] expectedRecipientsParties = GetExpectedOrganisationsForPartyType(task, WorkflowDescriptor.SupportedMessageRecipientParties(task, testOrder));

			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunDeliveryWorkflowTriggerActionForParent(provider, WorkflowDescriptor.SupportedMessageRecipientParties(task, testOrder), WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
			int noOfEmailsExpected = expectedRecipientsParties.Length + 1; // one for MessageRecipientPartyTypeList.Codes.Email
			AssertEquals("Number of messages created", noOfEmailsExpected, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("Delivered XML top level object should be a Shipment", true, xmlText.Contains("</SHIPMENT>"));
			AssertEquals("Delivered XML should contain Test Order", true, xmlText.Contains(testOrder.JD_OrderNumber.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Order 1", false, xmlText.Contains(anotherOrder1.JD_OrderNumber.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Order 2", false, xmlText.Contains(anotherOrder2.JD_OrderNumber.ToUpper()));
			AssertEquals("(*JobNumber*) is from the Order", true, message.Attachments[0].DisplayName.EndsWith("_TheOrder_"));
		}

		public void TestGetAndRunWorkflowTriggerActionForXmlMessageDeliveryWithFallback_ToShipmentPreplanning()
		{
			Order order = GetOrderWithBasicOrgInfo();
			AddOrderDirectConsigneeConsignorReceivingSendingAgentInfo(order);
			order.JD_OrderNumber = "_TheOrder_";
			order.JD_JS = ZGuid.Empty;
			JobShipmentPreplanning shipmentPreplanning = Factory.New<JobShipmentPreplanning>();
			shipmentPreplanning.EF_HouseBill = "_ThePreadvice_";
			order.JD_EF_ShipmentPrePlanning = shipmentPreplanning.PK;

			// Add other orders to the test order shipment pre-planning
			Order anotherOrder1 = Factory.New<Order>();
			anotherOrder1.JD_OrderNumber = "_AnotherOrder1_";
			anotherOrder1.JD_EF_ShipmentPrePlanning = shipmentPreplanning.PK;
			Order anotherOrder2 = Factory.New<Order>();
			anotherOrder2.JD_OrderNumber = "_AnotherOrder2_";
			anotherOrder2.JD_EF_ShipmentPrePlanning = shipmentPreplanning.PK;
			AssertEquals("[PRE-CONDITION] Shipment Order Count", 3, order.PreAdvice.Orders.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunDeliveryWorkflowTriggerActionForParent(order, WorkflowDescriptor.SupportedMessageRecipientParties(((IWorkflowProvider)order).WorkflowItems.AddNew(), order), WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
			AssertEquals("Messages should have been created", true, Env.OutgoingMailManager.EmailsCreated.Count > 0);

			EmailDef message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("Delivered XML top level object should be a Consol (generated by ShipmentPreAdvice DataAdapter)", true, xmlText.Contains("</CONSOL>"));
			AssertEquals("Delivered XML should contain a SHIPMENTPLANNING tag (generated by ShipmentPreAdvice DataAdapter)", true, xmlText.Contains("</SHIPMENTPLANNING>"));
			AssertEquals("Delivered XML should contain Test Order ShipmentPreplanning", true, xmlText.Contains(order.PreAdvice.EF_HouseBill.ToUpper()));
			AssertEquals("Delivered XML should contain Test Order", true, xmlText.Contains(order.JD_OrderNumber.ToUpper()));
			AssertEquals("Delivered XML should not contain Another Order 1", false, xmlText.Contains(anotherOrder1.JD_OrderNumber.ToUpper()));
			AssertEquals("Delivered XML should not contain Another Order 2", false, xmlText.Contains(anotherOrder2.JD_OrderNumber.ToUpper()));
			AssertEquals("(*JobNumber*) is from the Order", true, message.Attachments[0].DisplayName.EndsWith("_TheOrder_"));
		}

		public void TestGetAndRunWorkflowTriggerActionForXmlMessageDeliveryWithFallback_ToConsol()
		{
			Order order = GetOrderWithOrgInfo();
			order.JD_OrderNumber = "_TheOrder_";
			order.Shipment.JS_HouseBill = "~TheShipment~";

			// Add other orders to the test order shipment
			Order anotherOrder11 = AddNewOrderToShipment(order.Shipment.PK, "_AnotherOrder11_");
			Order anotherOrder12 = AddNewOrderToShipment(order.Shipment.PK, "_AnotherOrder12_");
			AssertEquals("[PRE-CONDITION] Shipment Order Count", 3, order.Shipment.AttachedOrders.Count);

			// Add another shipment to the test order shipment consol
			ForwardingShipment anotherShipment = order.Shipment.Consols[0].Shipments.AddNew();
			anotherShipment.JS_HouseBill = "~AnotherShipment~";
			// Add other orders to the other shipment
			Order anotherOrder21 = AddNewOrderToShipment(anotherShipment.PK, "_AnotherOrder21_");
			Order anotherOrder22 = AddNewOrderToShipment(anotherShipment.PK, "_AnotherOrder22_");
			AssertEquals("[PRE-CONDITION] Another Shipment Order Count", 2, anotherShipment.AttachedOrders.Count);

			AssertEquals("[PRE-CONDITION] Consol Shipment Count", 2, order.Shipment.Consols[0].Shipments.Count);

			var provider = (IWorkflowProvider)order;
			var task = provider.WorkflowItems.AddNew();
			OrgHeader[] expectedRecipientsParties = GetExpectedOrganisationsForPartyType(task, WorkflowDescriptor.SupportedMessageRecipientParties(task, order));

			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunDeliveryWorkflowTriggerActionForParent(provider, WorkflowDescriptor.SupportedMessageRecipientParties(task, order), WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
			int noOfEmailsExpected = expectedRecipientsParties.Length + 1; // one for MessageRecipientPartyTypeList.Codes.Email
			AssertEquals("Number of messages created", noOfEmailsExpected, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("Delivered XML top level object should be a Consol", true, xmlText.Contains("</CONSOL>"));

			AssertEquals("Delivered XML should contain Test Order Shipment", true, xmlText.Contains(order.Shipment.JS_HouseBill.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Shipment", false, xmlText.Contains(anotherShipment.JS_HouseBill.ToUpper()));

			AssertEquals("Delivered XML should contain Test Order", true, xmlText.Contains(order.JD_OrderNumber.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Order 11", false, xmlText.Contains(anotherOrder11.JD_OrderNumber.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Order 12", false, xmlText.Contains(anotherOrder12.JD_OrderNumber.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Order 21", false, xmlText.Contains(anotherOrder21.JD_OrderNumber.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Order 22", false, xmlText.Contains(anotherOrder22.JD_OrderNumber.ToUpper()));
			AssertEquals("(*JobNumber*) is from the Order", true, message.Attachments[0].DisplayName.EndsWith("_TheOrder_"));
		}

		public void TestGetWorkflowTriggerAction_ConfigFromCompanyOrgProxy()
		{
			var order = GetOrderWithOrgInfo();
			order.JD_OrderNumber = "3217890";
			order.Shipment.JS_HouseBill = "3777488";

			var trigger = order.WorkflowItems.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsCommencedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var communicationsMode = orgProxy.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = "ORD";
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = "9CHARCODE";

			var logBO = order.GetLogs().AddNew(Events.CustomsCommenced, ZDateTimeOffset.UtcNow);
			Factory.Save();

			AssertNotEquals("Precondition: Trigger should now have an actual date / time recorded.", ZDateTime.Empty, trigger.P9_ActualDate);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);
			var logger = new NotificationsForTesting();
			using (Factory.AddDisposableService())
			{
				processor.Process(logger);
				Factory.Save();
			}

			AssertMultilineASCIIEquals("loggger results from processor.Process()", @"
".Trim(), logger.ToString());

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, order.PK));
			AssertEquals("EDIMessages linked to order", 1, messages.Length);
		}

		public void TestSuppressXmlTriggersOnOrdersWithZeroQuantity()
		{
			Order order = OrderWithOrganisationParties;
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_QtyReceived = 0m;
			ProcessTask trigger = ((IWorkflowProvider)order).WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			IProcessor processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNotNull("Typically, Xml should be delivered", processor);

			OrdersDataRegistry.Instance.SuppressXmlTriggersOnOrdersWithZeroQuantity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNull("No xml should be delivered if no order has any quantity received and the system is configured this way", processor);

			orderLine.JO_QtyReceived = 2m;
			processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNotNull("Xml should be delivered if an order has quantity received", processor);
		}

		public void TestSuppressXmlTriggersOnOrdersWithZeroQuantity_DontSuppressOrderConfirmedTriggerField()
		{
			Order order = OrderWithOrganisationParties;
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_QtyReceived = 0m;
			ProcessTask trigger = ((IWorkflowProvider)order).WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			IProcessor processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNotNull("Typically, Xml should be delivered", processor);

			OrdersDataRegistry.Instance.SuppressXmlTriggersOnOrdersWithZeroQuantity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNull("No xml should be delivered if no order has any quantity received and the system is configured this way", processor);

			trigger.TriggerConditions.TriggerFieldName = JobOrderHeaderSchema.JD_BookingConfRef.Name;
			processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNotNull("Xml should be delivered always for 'order confirmed' trigger field", processor);
		}

		Order AddNewOrderToShipment(ZGuid shipmentPk, string newOrderNumber)
		{
			Order newOrder = Factory.New<Order>();
			newOrder.JD_OrderNumber = newOrderNumber;
			newOrder.JD_JS = shipmentPk;
			return newOrder;
		}

		#region GetParentsWithConfiguredOrganisationPartiesForTest

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				OrderWithOrganisationParties,
				OrderWithOrganisationParties_ForDirectOrgFallbackTest,
				OrderWithOrganisationParties_ForShipmentOrgFallbackTest,
			};
		}

		protected override IWorkflowProvider GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			return OrderWithOrganisationParties;
		}

		protected Order OrderWithOrganisationParties
		{
			get { return orderWithOrganisationParties ?? (orderWithOrganisationParties = GetOrderWithOrgInfo()); }
		}
		Order orderWithOrganisationParties;

		Order OrderWithOrganisationParties_ForDirectOrgFallbackTest
		{
			get
			{
				if (orderWithOrganisationParties_ForDirectOrgFallbackTest == null)
				{
					Order tempOrder = GetOrderWithBasicOrgInfo();
					AddOrderDirectConsigneeConsignorReceivingSendingAgentInfo(tempOrder);
					AddOrderShipmentPickupDeliveryCartageInfo(tempOrder);
					orderWithOrganisationParties_ForDirectOrgFallbackTest = tempOrder;
				}

				return orderWithOrganisationParties_ForDirectOrgFallbackTest;
			}
		}
		Order orderWithOrganisationParties_ForDirectOrgFallbackTest;

		Order OrderWithOrganisationParties_ForShipmentOrgFallbackTest
		{
			get
			{
				if (orderWithOrganisationParties_ForShipmentOrgFallbackTest == null)
				{
					Order tempOrder = GetOrderWithBasicOrgInfo();
					AddOrderShipmentConsigneeConsignorReceivingSendingAgentInfo(tempOrder);
					AddOrderShipmentPickupDeliveryCartageInfo(tempOrder);
					orderWithOrganisationParties_ForShipmentOrgFallbackTest = tempOrder;
				}

				return orderWithOrganisationParties_ForShipmentOrgFallbackTest;
			}
		}
		Order orderWithOrganisationParties_ForShipmentOrgFallbackTest;

		/// <summary>
		/// Gets a Order with required test organisations attached to it
		/// </summary>
		Order GetOrderWithOrgInfo()
		{
			var jobOrder = GetOrderWithBasicOrgInfo();
			AddOrderDirectConsigneeConsignorReceivingSendingAgentInfo(jobOrder);
			AddOrderShipmentConsigneeConsignorReceivingSendingAgentInfo(jobOrder);
			AddOrderShipmentPickupDeliveryCartageInfo(jobOrder);
			AddWarehouseOrg(jobOrder);

			return jobOrder;
		}

		Order GetOrderWithBasicOrgInfo()
		{
			Order jobOrder = Factory.New<Order>();

			//
			// Linked Shipment Parties
			//
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			jobOrder.JD_JS = shipment.PK;

			// Shipment - Broker
			shipment.JS_OH_ExportBroker = BrokerOrg.PK;
			shipment.JS_OH_ImportBroker = BrokerOrg.PK;

			// Shipment - Linked JobHeader BillToParty
			JobHeader.Loader jobLoader = new JobHeader.Loader(shipment);
			JobHeader job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = "XXX";
			jobOrder.BuyerPK = org.PK;
			jobOrder.SupplierPK = org.PK;

			return jobOrder;
		}

		void AddOrderDirectConsigneeConsignorReceivingSendingAgentInfo(Order jobOrder)
		{
			// Consignee & Consignor
			jobOrder.BuyerPK = ConsigneeOrg.PK;
			jobOrder.SupplierPK = ConsignorOrg.PK;

			// Order Receiving & Sending Agent
			jobOrder.JD_OH_ReceivingAgent = ReceivingAgentOrg.PK;
			jobOrder.JD_OH_SendingAgent = SendingAgentOrg.PK;
		}

		void AddOrderShipmentConsigneeConsignorReceivingSendingAgentInfo(Order jobOrder)
		{
			ForwardingShipment shipment = jobOrder.Shipment;

			// Shipment - Consignee & Consignor
			shipment.ConsigneePK = ConsigneeOrg.PK;
			shipment.ConsignorPK = ConsignorOrg.PK;

			// Shipment - Linked Consol Receiving & Sending Agent
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_OA_ReceivingForwarderAddress = ReceivingAgentOrg.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = SendingAgentOrg.MainAddress.PK;
			shipment.Consols.Add(consol);
		}

		void AddOrderShipmentPickupDeliveryCartageInfo(Order jobOrder)
		{
			// Shipment - Pickup & Delivery Cartage
			JobDocsAndCartage shipmentDoc = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(jobOrder.Shipment);
			shipmentDoc.PickupCartageCoPK = PickupCartageOrg.PK;
			shipmentDoc.DeliveryCartageCoPK = DeliveryCartageOrg.PK;
		}

		void AddWarehouseOrg(Order jobOrder)
		{
			jobOrder.WarehouseDocAddress.E2_OA_Address = WarehouseInwardsOrg.MainAddress.PK;
		}

		#endregion

		#region Implementation

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

		#endregion

		#endregion

		#region EstimateDefaultedFromList

		protected override IWorkflowProvider GetParentForGettingDateTimeOffset()
		{
			var order = (Order)base.GetParentForGettingDateTimeOffset();

			order.OrderLines.AddNew();

			return order;
		}

		protected override void SetDateTimeSourcePropertyValue(IWorkflowProvider workflowProvider, string dateTimeSourceType, ZDateTime localTime)
		{
			var order = (Order)workflowProvider;

			switch (dateTimeSourceType)
			{
				case OrderMilestoneEstimateDefaultedFromList.Codes.OrderDate:
					order.JD_OrderDate = localTime;
					break;

				case OrderMilestoneEstimateDefaultedFromList.Codes.OrderLineRequiredDate:
					order.OrderLines.First().JO_LineDropDate = localTime;
					break;

				default:
					base.SetDateTimeSourcePropertyValue(workflowProvider, dateTimeSourceType, localTime);
					break;
			}
		}

		protected override TimeSpan GetUtcOffsetForDateTimeSourceType(IWorkflowProvider workflowProvider, string dateTimeSourceType)
		{
			return TestUtcOffsetAttribute.IsActive ? TestUtcOffsetAttribute.Time : ZDateTime.UtcNow.ToDateTimeOffset(((GlbBranch)Env.CurrentBranch).HomePort).Offset;
		}

		#endregion
	}
}
