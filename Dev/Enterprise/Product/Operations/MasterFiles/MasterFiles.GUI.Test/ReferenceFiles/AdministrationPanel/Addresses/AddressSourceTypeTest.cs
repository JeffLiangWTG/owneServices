using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressSourceTypeTest : TestCase
	{
		public void TestGetAddressSourceNameFromCode()
		{
			AssertEquals(0, AddressSourceType.GetAddressSourceNameFromCode("INVALID").Length);

			AssertContainsExactElementsInAnyOrder(new[] { AccPayableOrderHeaderSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.AccPayableOrderHeaderCode));
			AssertContainsExactElementsInAnyOrder(new[] { CusInBondBillSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { CusISFHeaderSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { CusCAeMHHouseSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(AddressSourceType.CusCAeMHHouseCode));
			AssertContainsExactElementsInAnyOrder(new[] { CusDecHouseBillSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(AddressSourceType.CusDecHouseBillCode));
			AssertContainsExactElementsInAnyOrder(new[] { JobOrderHeaderSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(AddressSourceType.JobOrderHeaderCode));
			AssertContainsExactElementsInAnyOrder(new[] { JobDeclarationSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { JobComInvoiceLineSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(AddressSourceType.JobComInvoiceLineCode));
			AssertContainsExactElementsInAnyOrder(new[] { JobCartageSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(JobInvoicingConsumerTypes.LocalCartage.Code));
			AssertContainsExactElementsInAnyOrder(new[] { JobConsolSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.JobConsolWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { JPAFRBillsSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { JPAFRHeaderSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.JPAFRWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { JobShipmentSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { JobContainerLegsSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.CartageLegWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { DtbBookingConsolidationSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { DtbBookingSchema.Constants.TableName, DtbBookingInstructionSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.DtbBookingWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { RatingHeaderSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.QuotationWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { RateOneOffShipmentSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(AddressSourceType.RateOneOffShipmentCode));
			AssertContainsExactElementsInAnyOrder(new[] { WhsDocketSchema.Constants.TableName }, AddressSourceType.GetAddressSourceNameFromCode(AddressSourceType.WhsDocketCode));
		}

		public void TestAddressSourcePairList()
		{
			var expectedElements = new[]
			{
				WorkflowDescriptors.JobConsolWorkflowDescriptorCode,
				WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode,
				WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode,
				WorkflowDescriptors.DtbBookingWorkflowDescriptorCode,
				WorkflowDescriptors.WhsOrderWorkflowDescriptorCode
			};

			AssertContainsExactElementsInAnyOrder(expectedElements, AddressSourceType.AddressSourcePairList.GetAllCodes());
		}
	}
}
