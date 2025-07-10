using System;
using CargoWise.Definitions;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderLineWorkflowDescriptor))]
	class OrderLineWorkflowDescriptorTest : WorkflowDescriptorTestCase<OrderLineWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Code", WorkflowDescriptors.OrderLineWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Description", "Forwarding Order Line", WorkflowDescriptor.Description);
		}

		public void TestMilestoneTemplateHintCaption()
		{
			AssertEquals("Shipment milestones are combined with the Order Line milestones below to produce the complete list.", WorkflowDescriptor.MilestoneTemplateHintCaption);
		}

		public override void TestSubTypes()
		{
			AssertEquals("1 sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Transport Mode", "Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);

			CodeDescriptionPairList transportModeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
			AssertEquals(string.Empty, transportModeList[0].Code);
			AssertEquals("All", transportModeList[0].Description);

			AssertEquals(false, WorkflowDescriptor.SupportsBufferManagement);
			AssertEquals(true, WorkflowDescriptor.SupportsUniversalTemplates);
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

		public override void TestOnlySupportEventTrackingForUniversalTemplates()
		{
			AssertEquals(true, WorkflowDescriptor.OnlySupportEventTrackingForUniversalTemplates);
		}

		public override void TestSupportsTasks()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsTasks);
		}

		public override void TestSupportsScreenLayout()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsScreenLayout);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.Order, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestValidationToolSettings()
		{
			AssertType<OrderLineValidationToolSettings>(WorkflowDescriptor.ValidationToolSettings);
		}

		#region Workflow Triggers

		protected override bool ExpectingTasksToBeCompanySpecific => false;

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Consignee
					| MessageRecipientPartyType.Consignor
					| MessageRecipientPartyType.ControllingCustomer;
			}
		}

		public void TestSuppressXmlTriggersOnOrdersWithZeroQuantity()
		{
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_QtyReceived = 0m;
			var trigger = ((IWorkflowProvider)orderLine).WorkflowItems.Triggers.AddNew();
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNotNull("Typically, Xml should be delivered", processor);

			OrdersDataRegistry.Instance.SuppressXmlTriggersOnOrdersWithZeroQuantity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNull("No xml should be delivered if no order has any quantity received and the system is configured this way", processor);

			orderLine.JO_QtyReceived = 2m;
			processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNotNull("Xml should be delivered if an order has quantity received", processor);
		}

		#endregion

		#region GetParentsWithConfiguredOrganisationPartiesForTest

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				OrderLineWithOrganisationParties,
			};
		}

		protected OrderLine OrderLineWithOrganisationParties
		{
			get { return orderLineWithOrganisationParties ?? (orderLineWithOrganisationParties = GetOrderLineWithOrgInfo()); }
		}
		OrderLine orderLineWithOrganisationParties;

		OrderLine GetOrderLineWithOrgInfo()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = ConsigneeOrg.PK;
			order.SupplierPK = ConsignorOrg.PK;
			order.ControllingCustomerDocAddress.OrganisationPK = ControllingCustomerOrg.PK;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_QtyReceived = 20m;

			return orderLine;
		}

		#endregion
	}
}
