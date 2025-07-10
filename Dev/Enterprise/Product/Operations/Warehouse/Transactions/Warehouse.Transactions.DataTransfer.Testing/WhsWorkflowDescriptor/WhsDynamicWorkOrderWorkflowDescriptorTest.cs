using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrderWorkflowDescriptor))]
	public class WhsDynamicWorkOrderWorkflowDescriptorTest : WhsDocketWorkflowDescriptorTest<WhsDynamicWorkOrderWorkflowDescriptor>
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

		public override void TestSupportsCustomFields()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsCustomFields);
		}

		#endregion

		#region PropertiesThatAffectWorkflowCore

		protected override void TestPropertiesThatAffectWorkflowCore()
		{
			AssertNull(WorkflowDescriptor.FormCustomisationSettings);
		}

		#endregion

		#region Workflow Triggers

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes => Array.Empty<CodeDescriptionPair>();

		#endregion

		#region Implementation

		protected override ZString GetExpectedCode() => WorkflowDescriptors.WhsDynamicWorkOrderWorkflowDescriptorCode;

		protected override ZString GetExpectedDescription() => "Warehouse Dynamic Work Order";

		protected override WhsDocket GetNewDocket()
		{
			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(ClientOrg, Warehouse, "DW1");
			Warehouse.WarehouseAddress.OA_Code = "Address";
			Warehouse.WarehouseAddress.OA_OH = WarehouseOrg.PK;

			return dynamicWorkOrder;
		}

		protected override bool ShouldRunTestCanSendUniversalXMLToWarehouse() => false;

		#endregion
	}
}
