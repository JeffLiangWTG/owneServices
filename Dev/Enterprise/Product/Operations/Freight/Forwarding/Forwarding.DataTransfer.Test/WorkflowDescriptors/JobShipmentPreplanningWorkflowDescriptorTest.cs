using System;
using System.Text;
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
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(JobShipmentPreplanningWorkflowDescriptor))]
	class JobShipmentPreplanningWorkflowDescriptorTest : WorkflowDescriptorTestCase<JobShipmentPreplanningWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "SPA", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Shipment Pre Advice", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("No sub types", 0, WorkflowDescriptor.SubTypeInformation.Length);
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

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		#region EstimateDefaultedFromList

		public void TestEstimateDefaultedFromList()
		{
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETD));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETA));
		}

		protected override IWorkflowProvider GetParentForGettingDateTimeOffset()
		{
			var job = (JobShipmentPreplanning)base.GetParentForGettingDateTimeOffset();

			job.EF_RL_NKPortLoad = "AUPER";
			job.EF_RL_NKPortDisch = "AUBNE";

			return job;
		}

		protected override TimeSpan GetUtcOffsetForDateTimeSourceType(IWorkflowProvider workflowProvider, string dateTimeSourceType)
		{
			switch (dateTimeSourceType)
			{
				case ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETD:
					return TimeSpan.FromHours(8); // Departs from AUPER.

				case ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETA:
					return TimeSpan.FromHours(10); // Arrives in AUBNE.

				default:
					return base.GetUtcOffsetForDateTimeSourceType(workflowProvider, dateTimeSourceType);
			}
		}

		protected override void SetDateTimeSourcePropertyValue(IWorkflowProvider workflowProvider, string dateTimeSourceType, ZDateTime localTime)
		{
			var job = (JobShipmentPreplanning)workflowProvider;

			switch (dateTimeSourceType)
			{
				case ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETD:
					{
						var transport = job.PreAdviceTransports.FindTransportByLoadPort(job.EF_RL_NKPortLoad);
						transport.JW_ETD = localTime;
						break;
					}

				case ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETA:
					{
						var transport = job.PreAdviceTransports.FindTransportByDischargePort(job.EF_RL_NKPortDisch);
						transport.JW_ETA = localTime;
						break;
					}

				default:
					base.SetDateTimeSourcePropertyValue(workflowProvider, dateTimeSourceType, localTime);
					break;
			}
		}

		#endregion

		#region Workflow Triggers

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
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
					JobShipmentSchema.JS_HouseBill,
					JobShipmentPreplanningSchema.EF_HouseBill,
					JobShipmentPreplanningSchema.EF_MasterBill,
					JobOrderLineSchema.JO_QtyReceived,
					JobShipmentPreplanningSchema.EF_JS,
					JobShipmentPreplanningSchema.EF_JE
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
					MessageRecipientPartyType.Email;
			}
		}

		public void TestGetAndRunWorkflowTriggerActionForXmlMessageDeliveryWithFallback_ToShipment()
		{
			JobShipmentPreplanning preadvice = GetShipmentPreplanningWithOrgInfo();
			preadvice.EF_HouseBill = "_ThePreadvice_";
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			preadvice.Shipment.Consols.RemoveAndDeleteAll();
			preadvice.EF_PreshipID = "PreadviceID";

			var provider = (IWorkflowProvider)preadvice;
			var task = provider.WorkflowItems.AddNew();
			OrgHeader[] expectedRecipientParties = GetExpectedOrganisationsForPartyType(task, WorkflowDescriptor.SupportedMessageRecipientParties(task, preadvice));

			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunDeliveryWorkflowTriggerActionForParent(provider, WorkflowDescriptor.SupportedMessageRecipientParties(task, preadvice), WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
			int noOfEmailsExpected = expectedRecipientParties.Length + 1; // one for MessageRecipientPartyTypeList.Codes.Email
			AssertEquals("Number of messages created", noOfEmailsExpected, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("Delivered XML top level object should be a Shipment", true, xmlText.Contains("</SHIPMENT>"));
			AssertEquals("Shipment DataAdapter does NOT export preplanning info", false, xmlText.Contains(preadvice.EF_HouseBill.ToUpper()));
			AssertEquals("(*JobNumber*) is from the Preadvice", true, message.Attachments[0].DisplayName.EndsWith("PreadviceID"));
		}

		public void TestShipmentTriggerExportAllOrders()
		{
			JobShipmentPreplanning preplanning = ShipmentPreplanningWithAllOrganisationParties;

			Order order1 = Factory.New<Order>();
			order1.JD_JS = preplanning.Shipment.PK;
			order1.JD_OrderNumber = "111";
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			order1.Shipment.Consols.RemoveAndDeleteAll();

			Order order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "222";
			order2.JD_JS = order1.Shipment.PK;

			Order order3 = Factory.New<Order>();
			order3.JD_OrderNumber = "333";
			order3.JD_JS = order1.Shipment.PK;

			AssertEquals("Precondition: Shipment Order Count", 3, order1.Shipment.AttachedOrders.Count);
			var provider = (IWorkflowProvider)preplanning;
			var task = provider.WorkflowItems.AddNew();
			OrgHeader[] expectedRecipientParties = GetExpectedOrganisationsForPartyType(task, WorkflowDescriptor.SupportedMessageRecipientParties(task, preplanning));

			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunDeliveryWorkflowTriggerActionForParent(provider, WorkflowDescriptor.SupportedMessageRecipientParties(task, preplanning), WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
			Assert("Precondition: At least one message was created", Env.OutgoingMailManager.EmailsCreated.Count > 0);

			EmailDef message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("XML should contain order1", true, xmlText.Contains("<ORDERNUMBER>111</ORDERNUMBER>"));
			AssertEquals("XML should contain order2", true, xmlText.Contains("<ORDERNUMBER>222</ORDERNUMBER>"));
			AssertEquals("XML should contain order3", true, xmlText.Contains("<ORDERNUMBER>333</ORDERNUMBER>"));
		}

		public void TestGetAndRunWorkflowTriggerActionForXmlMessageDeliveryWithFallback_ToConsol()
		{
			JobShipmentPreplanning preadvice = GetShipmentPreplanningWithOrgInfo();
			preadvice.EF_HouseBill = "_ThePreadvice_";
			preadvice.Shipment.JS_HouseBill = "~TheShipment~";
			preadvice.EF_PreshipID = "PreadviceID";

			// Add another shipment to the test order shipment consol
			ForwardingShipment anotherShipment = preadvice.Shipment.Consols[0].Shipments.AddNew();
			anotherShipment.JS_HouseBill = "~AnotherShipment~";

			AssertEquals("[PRE-CONDITION] Consol Shipment Count", 2, preadvice.Shipment.Consols[0].Shipments.Count);
			var provider = (IWorkflowProvider)preadvice;
			var task = provider.WorkflowItems.AddNew();
			OrgHeader[] expectedRecipientParties = GetExpectedOrganisationsForPartyType(task, WorkflowDescriptor.SupportedMessageRecipientParties(task, preadvice));

			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunDeliveryWorkflowTriggerActionForParent(provider, WorkflowDescriptor.SupportedMessageRecipientParties(task, preadvice), WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
			int noOfEmailsExpected = expectedRecipientParties.Length + 1; // one for MessageRecipientPartyTypeList.Codes.Email
			AssertEquals("Number of messages created", noOfEmailsExpected, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef message = Env.OutgoingMailManager.EmailsCreated[0];
			byte[] xmlData = message.Attachments[0].Data;
			string xmlText = ASCIIEncoding.ASCII.GetString(xmlData).Trim().ToUpper();

			AssertEquals("Delivered XML top level object should be a Consol", true, xmlText.Contains("</CONSOL>"));

			AssertEquals("Delivered XML should contain Test Preadvice Shipment", true, xmlText.Contains(preadvice.Shipment.JS_HouseBill.ToUpper()));
			AssertEquals("Delivered XML should NOT contain Another Shipment", false, xmlText.Contains(anotherShipment.JS_HouseBill.ToUpper()));
			AssertEquals("Shipment DataAdapter does NOT export preplanning info", false, xmlText.Contains(preadvice.EF_HouseBill.ToUpper()));
			AssertEquals("(*JobNumber*) is from the Preadvice", true, message.Attachments[0].DisplayName.EndsWith("PreadviceID"));
		}

		public void TestSuppressXmlTriggersOnOrdersWithZeroQuantity()
		{
			Order order = ShipmentPreplanningWithAllOrganisationParties.Orders.AddNew();
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_QtyReceived = 0m;
			ProcessTask trigger = ((IWorkflowProvider)ShipmentPreplanningWithAllOrganisationParties).WorkflowItems.Triggers.AddNew();
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

		public void TestGetWorkflowTriggerActionForEDTIFC()
		{
			JobShipmentPreplanning preadvice = GetShipmentPreplanningWithOrgInfo();
			preadvice.EF_Packs = 1;

			var trigger = ((IWorkflowProvider)preadvice).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = "EDT";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			notification.PQ_FieldName = JobShipmentPreplanning.Schema.EF_Packs;

			Factory.Save();

			var log = preadvice.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = Events.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
			}

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#region ShipmentPreplanning With Configured Organisation Parties

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				ShipmentPreplanningWithAllOrganisationParties,
				ShipmentPreplanningWithOrganisationParties_MissingSomeLinkedShipmentOrgs,
				ShipmentPreplanningWithOrganisationParties_MissingSomeDirectlyLinkedOrgs,
			};
		}

		protected override IWorkflowProvider GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			return ShipmentPreplanningWithAllOrganisationParties;
		}

		JobShipmentPreplanning ShipmentPreplanningWithAllOrganisationParties
		{
			get { return shipmentPreplanningWithAllOrganisationParties ?? (shipmentPreplanningWithAllOrganisationParties = GetShipmentPreplanningWithOrgInfo()); }
		}
		JobShipmentPreplanning shipmentPreplanningWithAllOrganisationParties;

		JobShipmentPreplanning ShipmentPreplanningWithOrganisationParties_MissingSomeLinkedShipmentOrgs
		{
			get
			{
				if (shipmentPreplanningWithOrganisationParties_MissingSomeLinkedShipmentOrgs == null)
				{
					JobShipmentPreplanning tempShipmentPreplanning = GetShipmentPreplanningWithBasicOrgInfo();
					AddShipmentPreplanningConsigneeReceivingSendingAgentInfo(tempShipmentPreplanning);
					AddShipmentPreplanningShipmentPickupDeliveryCartageInfo(tempShipmentPreplanning);
					shipmentPreplanningWithOrganisationParties_MissingSomeLinkedShipmentOrgs = tempShipmentPreplanning;
				}

				return shipmentPreplanningWithOrganisationParties_MissingSomeLinkedShipmentOrgs;
			}
		}
		JobShipmentPreplanning shipmentPreplanningWithOrganisationParties_MissingSomeLinkedShipmentOrgs;

		JobShipmentPreplanning ShipmentPreplanningWithOrganisationParties_MissingSomeDirectlyLinkedOrgs
		{
			get
			{
				if (shipmentPreplanningWithOrganisationParties_MissingSomeDirectlyLinkedOrgs == null)
				{
					JobShipmentPreplanning tempShipmentPreplanning = GetShipmentPreplanningWithBasicOrgInfo();
					AddShipmentPreplanningShipmentConsigneeReceivingSendingAgentInfo(tempShipmentPreplanning);
					AddShipmentPreplanningShipmentPickupDeliveryCartageInfo(tempShipmentPreplanning);
					shipmentPreplanningWithOrganisationParties_MissingSomeDirectlyLinkedOrgs = tempShipmentPreplanning;
				}

				return shipmentPreplanningWithOrganisationParties_MissingSomeDirectlyLinkedOrgs;
			}
		}
		JobShipmentPreplanning shipmentPreplanningWithOrganisationParties_MissingSomeDirectlyLinkedOrgs;

		/// <summary>
		/// Gets a ShipmentPreplanning with required test organisations attached to it
		/// </summary>
		JobShipmentPreplanning GetShipmentPreplanningWithOrgInfo()
		{
			JobShipmentPreplanning shipmentPreplanning = GetShipmentPreplanningWithBasicOrgInfo();

			AddShipmentPreplanningConsigneeReceivingSendingAgentInfo(shipmentPreplanning);
			AddShipmentPreplanningShipmentConsigneeReceivingSendingAgentInfo(shipmentPreplanning);
			AddShipmentPreplanningShipmentPickupDeliveryCartageInfo(shipmentPreplanning);

			return shipmentPreplanning;
		}

		JobShipmentPreplanning GetShipmentPreplanningWithBasicOrgInfo()
		{
			JobShipmentPreplanning shipmentPreplanning = Factory.New<JobShipmentPreplanning>();

			//
			// Linked Shipment Parties
			//
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipmentPreplanning.EF_JS = shipment.PK;

			// Shipment - Consignor & Broker
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.JS_OH_ExportBroker = BrokerOrg.PK;
			shipment.JS_OH_ImportBroker = BrokerOrg.PK;

			// Shipment - Linked JobHeader BillToParty
			JobHeader.Loader jobLoader = new JobHeader.Loader(shipment);
			JobHeader job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = "XXX";
			shipmentPreplanning.BuyerPK = org.PK;

			return shipmentPreplanning;
		}

		void AddShipmentPreplanningConsigneeReceivingSendingAgentInfo(JobShipmentPreplanning shipmentPreplanning)
		{
			// Consignee

			// Receiving & Sending Agent
			shipmentPreplanning.EF_OH_ReceivingAgent = ReceivingAgentOrg.PK;
			shipmentPreplanning.EF_OH_SendingAgent = SendingAgentOrg.PK;
		}

		void AddShipmentPreplanningShipmentConsigneeReceivingSendingAgentInfo(JobShipmentPreplanning shipmentPreplanning)
		{
			ForwardingShipment shipment = shipmentPreplanning.Shipment;

			// Shipment - Consignee
			shipment.ConsigneePK = ConsigneeOrg.PK;

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

		void AddShipmentPreplanningShipmentPickupDeliveryCartageInfo(JobShipmentPreplanning shipmentPreplanning)
		{
			// Shipment - Pickup & Delivery Cartage
			JobDocsAndCartage shipmentDoc = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentPreplanning.Shipment);
			shipmentDoc.PickupCartageCoPK = PickupCartageOrg.PK;
			shipmentDoc.DeliveryCartageCoPK = DeliveryCartageOrg.PK;
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			switch (table.TableName)
			{
				case JobShipmentSchema.Constants.TableName:
					return Factory.New<ForwardingShipment>();

				default:
					return base.NewBusinessObjectInTable(table);
			}
		}

		#endregion
	}
}
