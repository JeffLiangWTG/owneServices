using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferWorkflowDescriptor))]
	public class WhsTransferWorkflowDescriptorTest : WhsDocketWorkflowDescriptorTest<WhsTransferWorkflowDescriptor>
	{
		#region Flags

		public void TestSupportSetFiledTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		public void TestSupportsRecognizeRevenue()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsRecognizeRevenue);
		}

		public void TestSupportsSupportsPostAllRevenue()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsPostAllRevenue);
		}

		public void TestSupportsCreateJobInvoiceHeader()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsCreateJobInvoiceHeader);
		}

		public void TestSupportsPostAllSisterCompanyCharges()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsPostAllSisterCompanyCharges);
		}

		public void TestSupportsPostLocalSisterCompanyChargesOnly()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsPostLocalSisterCompanyChargesOnly);
		}

		public void TestSupportsPostAllCosts()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsPostAllCosts);
		}

		#endregion

		#region Workflow Triggers

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
			=> WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value
			? [new CodeDescriptionPair(ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning, ActionTypes.Descriptions.SetTaskPlanningStatusToReadyForPlanning)]
			: Array.Empty<CodeDescriptionPair>();

		#endregion

		#region TestCanSendUniversalXMLToWarehouse

		protected override bool ShouldRunTestCanSendUniversalXMLToWarehouse() => false;

		#endregion

		#region Implementation

		protected override ZString GetExpectedCode() => WorkflowDescriptors.WhsTransferWorkflowDescriptorCode;

		protected override ZString GetExpectedDescription() => "Warehouse Transfer";

		protected override WhsDocket GetNewDocket()
		{
			var transfer = Helper.CreateWhsTransfer(ClientOrg, Warehouse);
			Warehouse.WarehouseAddress.OA_Code = "Address";
			Warehouse.WarehouseAddress.OA_OH = WarehouseOrg.PK;

			return transfer;
		}

		#endregion
	}
}
