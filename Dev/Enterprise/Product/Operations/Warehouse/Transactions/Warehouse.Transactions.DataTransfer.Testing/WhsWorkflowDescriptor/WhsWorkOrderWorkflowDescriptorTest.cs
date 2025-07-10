using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderWorkflowDescriptor))]
	public class WhsWorkOrderWorkflowDescriptorTest : WhsDocketWorkflowDescriptorTest<WhsWorkOrderWorkflowDescriptor>
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

		protected override ZString GetExpectedCode() => WorkflowDescriptors.WhsWorkOrderWorkflowDescriptorCode;

		protected override ZString GetExpectedDescription() => "Warehouse Work Order";

		protected override WhsDocket GetNewDocket()
		{
			var transfer = Helper.CreateWhsWorkOrder(ClientOrg, Warehouse);
			Warehouse.WarehouseAddress.OA_Code = "Address";
			Warehouse.WarehouseAddress.OA_OH = WarehouseOrg.PK;

			return transfer;
		}

		#endregion
	}
}
