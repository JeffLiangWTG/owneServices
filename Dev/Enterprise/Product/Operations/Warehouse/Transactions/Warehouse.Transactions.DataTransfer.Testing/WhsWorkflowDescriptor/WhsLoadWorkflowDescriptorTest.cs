using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLoadWorkflowDescriptor))]
	public class WhsLoadWorkflowDescriptorTest : WorkflowDescriptorTestCase<WhsLoadWorkflowDescriptor>
	{
		#region TestID

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.WhsLoadWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Warehouse Load", WorkflowDescriptor.Description);
		}
		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region Workflow Triggers

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
			=> WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value
			? [new CodeDescriptionPair(ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning, ActionTypes.Descriptions.SetTaskPlanningStatusToReadyForPlanning),]
			: Array.Empty<CodeDescriptionPair>();

		public void TestWorkflowTriggerActionTypes_EnableTaskManagement()
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestWorkflowTriggerActionTypes();
			}
		}

		public void TestGetWorkflowTriggerAction_SetTaskPlanningStatusToReadyForPlanningProcessor()
		{
			var load = Factory.New<WhsLoad>();
			var task = load.WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning;

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertEquals("Should get correct processor.", typeof(SetTaskPlanningStatusToReadyForPlanningProcessor), processor.GetType());
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals("No sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<WhsLoad>() };
		}
	}
}
