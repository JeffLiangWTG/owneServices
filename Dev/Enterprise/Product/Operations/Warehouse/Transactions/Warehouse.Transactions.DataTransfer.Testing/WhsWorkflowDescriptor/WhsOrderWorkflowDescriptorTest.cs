using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderWorkflowDescriptor))]
	public class WhsOrderWorkflowDescriptorTest : WhsDocketWorkflowDescriptorTest<WhsOrderWorkflowDescriptor>
	{
		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.WhsOrder, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		#endregion

		#region TestIsMessagingOrEmailNotificationTriggerAction

		public override void TestIsMessagingOrEmailNotificationTriggerAction()
		{
			base.TestIsMessagingOrEmailNotificationTriggerAction();
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(ActionTypes.Codes.IFSExport));
		}

		#endregion

		#region TestGetWorkflowTriggerAction_GenerateTransportReferenceNumberProcessor

		public void TestGetWorkflowTriggerAction_GenerateTransportReferenceNumberProcessor()
		{
			var order = Factory.New<WhsOrder>();
			var task = order.WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = ActionTypes.Codes.GenerateTransportReferenceNumber;

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertEquals("Should get correct processor.", typeof(GenerateTransportReferenceNumberProcessor), processor.GetType());
		}

		#endregion

		#region TestGetWorkflowTriggerAction_WhsOrderGeneratePickProcessor

		public void TestGetWorkflowTriggerAction_WhsOrderGeneratePickProcessor()
		{
			var order = Factory.New<WhsOrder>();
			var task = order.WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = ActionTypes.Codes.GeneratePickForOrder;

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertEquals("Should get correct processor.", typeof(WhsOrderGeneratePickProcessor), processor.GetType());
		}

		#endregion

		#region TestSupportedTriggerLineTypes

		public void TestSupportedTriggerLineTypes()
		{
			AssertContainsExactElementsInAnyOrder(new[] { TriggerLineTypes.Codes.PkgPackage }, ((IWorkflowParentWithLines)WorkflowDescriptor).SupportedTriggerLineTypes);
		}

		#endregion

		#region TestGetWorkflowTriggerActionCore

		public void TestGetWorkflowTriggerActionCore()
		{
			var receive1 = GetNewDocket();
			var task1 = GetNewProcessTask(receive1, ActionTypes.Codes.Confirmation);
			var processor1 = task1.WorkflowDescriptor.GetWorkflowTriggerAction(receive1.WorkflowItems[0].ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			Assert(processor1 is XmlMessageDeliver);

			receive1.WorkflowItems[0].ProcessTaskNotifications[0].PQ_TriggerType = ActionTypes.Codes.IFSExport;
			var processor3 = task1.WorkflowDescriptor.GetWorkflowTriggerAction(receive1.WorkflowItems[0].ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			Assert(processor3 is XmlMessageDeliverIFS);

			receive1.WorkflowItems[0].ProcessTaskNotifications[0].PQ_TriggerType = ActionTypes.Codes.IFSExport;
			var processor5 = task1.WorkflowDescriptor.GetWorkflowTriggerAction(receive1.WorkflowItems[0].ProcessTaskNotifications[0], null);
			Assert(processor5 is XmlMessageDeliverIFS);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			var salesChannel1 = Factory.New<WhsSalesChannel>();
			salesChannel1.WSH_Code = "SCH";
			salesChannel1.WSH_Description = "Schoool";

			var salesChannel2 = Factory.New<WhsSalesChannel>();
			salesChannel2.WSH_Code = "ERT";
			salesChannel2.WSH_Description = "ERT Description";
			Factory.Save();

			AssertEquals("1 sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);

			var subType = WorkflowDescriptor.SubTypeInformation[0];
			AssertEquals("Sales Channel", subType.Description);

			AssertContainsExactElementsInAnyOrder(
				"SubType List should contain all Sales Channels.",
				new[] { salesChannel1.WSH_Code, salesChannel2.WSH_Code },
				subType.Collection.Cast<WhsSalesChannel>().Select(i => i.WSH_Code));
		}

		#endregion

		#region Implementation

		protected override OrgHeader[] GetExpectedOrganisationsForPartyType(ProcessTask task, MessageRecipientPartyType partyType)
		{
			if (partyType == MessageRecipientPartyType.Forwarder)
			{
				return new OrgHeader[] { Forwarder };
			}
			else if (partyType == MessageRecipientPartyType.CarrierBookingAgent)
			{
				return new OrgHeader[] { SendingAgentOrg };
			}
			else
			{
				return base.GetExpectedOrganisationsForPartyType(task, partyType);
			}
		}

		protected override ZString GetExpectedCode()
		{
			return JobInvoicingConsumerTypes.WarehouseOutwards.Code;
		}

		protected override ZString GetExpectedDescription()
		{
			return JobInvoicingConsumerTypes.WarehouseOutwards.Description;
		}

		protected override WhsDocket GetNewDocket()
		{
			var order = Helper.CreateWhsOrder(ClientOrg, Warehouse);
			order.TransportCoPK = order.Client.PK;
			order.WD_OH_Forwarder = Forwarder.PK;
			Warehouse.WarehouseAddress.OA_Code = "Address";
			Warehouse.WarehouseAddress.OA_OH = WarehouseOrg.PK;

			return order;
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
			=> new CodeDescriptionPair[]
				{
					new CodeDescriptionPair(ActionTypes.Codes.Confirmation, ActionTypes.Descriptions.Confirmation),
					new CodeDescriptionPair(ActionTypes.Codes.IFSExport, ActionTypes.Descriptions.IFSExport),
					new CodeDescriptionPair(ActionTypes.Codes.GeneratePickForOrder, ActionTypes.Descriptions.GeneratePickForOrder),
					new CodeDescriptionPair(ActionTypes.Codes.GenerateTransportReferenceNumber, ActionTypes.Descriptions.GenerateTransportReferenceNumber)
				};

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
			=> base.ExpectedSupportedMessageRecipientParties
			| MessageRecipientPartyType.Consignee
			| MessageRecipientPartyType.DeliveryCartage
			| MessageRecipientPartyType.TransportCo
			| MessageRecipientPartyType.Forwarder
			| MessageRecipientPartyType.CarrierBookingAgent
			| MessageRecipientPartyType.WarehouseInwards;

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);
			var order = (WhsOrder)workflowProvider;

			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.CarrierBookingAgent)
			{
				order.CarrierBookingAgentDocAddress.E2_OA_Address = SendingAgentOrg.MainAddress.PK;
			}
			else if (partyTypeCode == MessageRecipientPartyTypeList.Codes.WarehouseInwards)
			{
				order.DestinationWarehouseDocAddress.E2_OA_Address = Warehouse.WarehouseAddress.PK;
			}
		}

		#endregion
	}
}
