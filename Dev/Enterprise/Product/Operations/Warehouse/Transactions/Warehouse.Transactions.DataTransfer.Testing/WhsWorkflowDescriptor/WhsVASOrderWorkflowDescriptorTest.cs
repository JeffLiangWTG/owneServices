using System.Collections.Generic;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrderWorkflowDescriptor))]
	class WhsVASOrderWorkflowDescriptorTest : WorkflowDescriptorTestCase<WhsVASOrderWorkflowDescriptor>
	{
		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertContainsExactElementsInAnyOrder(new[] { BusinessContext.INVALID }, WorkflowDescriptor.DocumentBusinessContext);
		}

		#endregion

		#region TestGetWorkflowTriggerAction

		public void TestGetWorkflowTriggerAction()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var task = vasOrder.WorkflowItems.Triggers.AddNew();
			var notification1 = task.ProcessTaskNotifications.AddNew();
			var notification2 = task.ProcessTaskNotifications.AddNew();
			var notification3 = task.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = ActionTypes.Codes.CreateInitialVASOrderTransfer;
			notification2.PQ_TriggerType = ActionTypes.Codes.CreateReturnVASOrderTransfer;
			notification3.PQ_TriggerType = ActionTypes.Codes.CompleteAndFinaliseVASOrder;
			var processor1 = WorkflowDescriptor.GetWorkflowTriggerAction(notification1, new QueuedLogForTesting(Factory));
			var processor2 = WorkflowDescriptor.GetWorkflowTriggerAction(notification2, new QueuedLogForTesting(Factory));
			var processor3 = WorkflowDescriptor.GetWorkflowTriggerAction(notification3, new QueuedLogForTesting(Factory));
			AssertEquals("Should get correct processor.", typeof(CreateInitialVASOrderTransferProcessor), processor1.GetType());
			AssertEquals("Should get correct processor.", typeof(CreateReturnVASOrderTransferProcessor), processor2.GetType());
			AssertEquals("Should get correct processor.", typeof(CompleteAndFinaliseVASOrderProcessor), processor3.GetType());

			var notify = new TestNotificationBuffer();
			processor1.Process(notify);
			AssertEquals("Save all changes before creating the Transfer to Service Area.\r\n", notify.AsString);

			notify.Clear();
			processor2.Process(notify);
			AssertEquals("Save all changes before creating the Transfer out of the Service Area.\r\n", notify.AsString);

			notify.Clear();
			processor3.Process(notify);
			AssertEquals("Save all changes before Completing this VAS Order.\r\n", notify.AsString);
		}

		#endregion

		#region Implementation

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				var result = new List<CodeDescriptionPair>(base.ExpectedAdditionalWorkflowTriggerActionTypes);
				result.Add(new CodeDescriptionPair(ActionTypes.Codes.CreateInitialVASOrderTransfer, ActionTypes.Descriptions.CreateInitialVASOrderTransfer));
				result.Add(new CodeDescriptionPair(ActionTypes.Codes.CreateReturnVASOrderTransfer, ActionTypes.Descriptions.CreateReturnVASOrderTransfer));
				result.Add(new CodeDescriptionPair(ActionTypes.Codes.CompleteAndFinaliseVASOrder, ActionTypes.Descriptions.CompleteAndFinaliseVASOrder));

				return result.ToArray();
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<WhsVASOrder>() };
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.WhsVASOrderWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Warehouse VAS Order", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
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

		#region TestRequiresWarehouse

		protected override bool RequiresWarehouseExpectedResult => true;

		#endregion

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

		protected override bool ExpectingTasksToBeCompanySpecific => false;

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
