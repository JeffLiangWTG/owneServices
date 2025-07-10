using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveWorkflowDescriptor))]
	public class WhsReceiveWorkflowDescriptorTest : WhsDocketWorkflowDescriptorTest<WhsReceiveWorkflowDescriptor>
	{
		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.WhsInwards, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override ZString GetExpectedCode() => JobInvoicingConsumerTypes.WarehouseInwards.Code;

		protected override ZString GetExpectedDescription()
		{
			return JobInvoicingConsumerTypes.WarehouseInwards.Description;
		}

		protected override WhsDocket GetNewDocket()
		{
			var receive = Helper.CreateWhsReceive(ClientOrg, Warehouse);
			receive.TransportCoPK = CarrierOrg.PK;
			Warehouse.WarehouseAddress.OA_Code = "Address";
			Warehouse.WarehouseAddress.OA_OH = WarehouseOrg.PK;

			return receive;
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
			=> base.ExpectedSupportedMessageRecipientParties
			| MessageRecipientPartyType.TransportCo
			| MessageRecipientPartyType.PickupCartage;

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypesCore
			=> WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value
			? new CodeDescriptionPair[]
				{
					new CodeDescriptionPair(ActionTypes.Codes.GeneratePalletIdForReceive, ActionTypes.Descriptions.GeneratePalletIdForReceive),
					new CodeDescriptionPair(ActionTypes.Codes.AutoPalletizeForReceive, ActionTypes.Descriptions.AutoPalletizeForReceive),
					new CodeDescriptionPair(ActionTypes.Codes.StartedReceiving, ActionTypes.Descriptions.StartedReceiving),
					new CodeDescriptionPair(ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning, ActionTypes.Descriptions.SetTaskPlanningStatusToReadyForPlanning),
				}
			: new CodeDescriptionPair[]
				{
					new CodeDescriptionPair(ActionTypes.Codes.GeneratePalletIdForReceive, ActionTypes.Descriptions.GeneratePalletIdForReceive),
					new CodeDescriptionPair(ActionTypes.Codes.AutoPalletizeForReceive, ActionTypes.Descriptions.AutoPalletizeForReceive),
					new CodeDescriptionPair(ActionTypes.Codes.StartedReceiving, ActionTypes.Descriptions.StartedReceiving),
				};

		public void TestWorkflowTriggerActionCore_ReceiveSpecific()
		{
			var receive1 = GetNewDocket();
			var task1 = GetNewProcessTask(receive1, ActionTypes.Codes.GeneratePalletIdForReceive);
			var processor1 = task1.WorkflowDescriptor.GetWorkflowTriggerAction(receive1.WorkflowItems[0].ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			Assert(processor1 is ReceivePalletIDGenerator);

			var receive2 = GetNewDocket();
			var task2 = GetNewProcessTask(receive2, ActionTypes.Codes.AutoPalletizeForReceive);
			var processor2 = task2.WorkflowDescriptor.GetWorkflowTriggerAction(receive2.WorkflowItems[0].ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			Assert(processor2 is ReceiveAutoPalletiseProcessor);

			var receive3 = GetNewDocket();
			var task3 = GetNewProcessTask(receive3, ActionTypes.Codes.Confirmation);
			var processor3 = task3.WorkflowDescriptor.GetWorkflowTriggerAction(receive3.WorkflowItems[0].ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			Assert(processor3 is XmlMessageDeliver);

			var receive4 = GetNewDocket();
			var task4 = GetNewProcessTask(receive4, ActionTypes.Codes.StartedReceiving);
			var process4 = task4.WorkflowDescriptor.GetWorkflowTriggerAction(receive4.WorkflowItems[0].ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			Assert(process4 is StartedReceivingProcessor);

			var receive5 = GetNewDocket();
			var task5 = GetNewProcessTask(receive5, ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning);
			var process5 = task5.WorkflowDescriptor.GetWorkflowTriggerAction(receive5.WorkflowItems[0].ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			Assert(process5 is SetTaskPlanningStatusToReadyForPlanningProcessor);
		}

		public override void TestSubTypes()
		{
			var categories = new SystemDefinableCodeDescriptionBoolCollection();
			categories.Add("IMA", (NoResString)"In", true);
			categories.Add("DAN", (NoResString)"The", true);
			categories.Add("CIN", (NoResString)"Moonlight", true);
			WarehouseDataRegistry.Instance.ReceiveCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

			AssertEquals("1 sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);

			var subType = WorkflowDescriptor.SubTypeInformation[0];
			AssertEquals("Receive Category", subType.Description);

			var subTypeList = subType.List.Cast<SystemDefinableCodeDescriptionBool>();
			AssertContainsExactElementsInAnyOrder(
				"SubType List should contain all Receive Categories.",
				new[] { "IMA - In", "DAN - The", "CIN - Moonlight" },
				subTypeList.Select(i => $"{i.Code} - {i.Description}"));
		}
	}
}
