using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentWorkflowDescriptor))]
	public class WhsAdjustmentWorkflowDescriptorTest : WhsDocketWorkflowDescriptorTest<WhsAdjustmentWorkflowDescriptor>
	{
		public void TestWorkflowTriggerActionCore_AdjustmentSpecific()
		{
			var adjustment = GetNewDocket();
			var task = GetNewProcessTask(adjustment, ActionTypes.Codes.Confirmation);
			var processor = task.WorkflowDescriptor.GetWorkflowTriggerAction(adjustment.WorkflowItems[0].ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			Assert(processor is XmlMessageDeliver);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.WhsAdjustment, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override ZString GetExpectedCode()
		{
			return WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode;
		}

		protected override ZString GetExpectedDescription()
		{
			return "Warehouse Adjustment";
		}

		protected override WhsDocket GetNewDocket()
		{
			var adjustment = Helper.CreateWhsAdjustment(ClientOrg, Warehouse);
			Warehouse.WarehouseAddress.OA_Code = "Address";
			Warehouse.WarehouseAddress.OA_OH = WarehouseOrg.PK;

			return adjustment;
		}
	}
}
