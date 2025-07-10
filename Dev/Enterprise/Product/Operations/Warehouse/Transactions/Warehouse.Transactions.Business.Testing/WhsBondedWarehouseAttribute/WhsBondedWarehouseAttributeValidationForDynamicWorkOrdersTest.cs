using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsBondedWarehouseAttributeValidationForDynamicWorkOrders))]
	public class WhsBondedWarehouseAttributeValidationForDynamicWorkOrdersTest : WhsBusinessObjectValidationTestCase
	{
		public void TestEntryNumberNotMandatoryForWorkOrders()
		{
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_IsInwardsProcessingJob = true;

			var orderLine = workOrder.Lines.AddNew();
			var attrib = Factory.New<WhsBondedWarehouseAttribute>();
			attrib.SetParent(orderLine);
			AssertType<WhsBondedWarehouseAttributeValidationForDynamicWorkOrders>(attrib.Validation);

			attrib.Validation.ValidateWB_EntryKey();
			AssertNoErrors(attrib.WB_EntryKeyInfo);
		}

		#region TestIsMainInwardProcessedItem

		public void TestIsMainInwardProcessedItem_ShouldBeAtLeastOne()
		{
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			var workOrderLine1 = workOrder.Lines.AddNew();
			var workOrderLine2 = workOrder.Lines.AddNew();
			var customsDataWL1 = workOrderLine1.CustomsData;
			var customsDataWL2 = workOrderLine2.CustomsData;

			customsDataWL1.WB_IsMainInwardsProcessedItem = true;
			customsDataWL2.WB_IsSecondaryInwardsProcessedItem = true;
			AssertNoErrors(customsDataWL1.WB_IsMainInwardsProcessedItemInfo);
			AssertNoErrors(customsDataWL2.WB_IsMainInwardsProcessedItemInfo);

			customsDataWL1.WB_IsSecondaryInwardsProcessedItem = true;
			customsDataWL1.WB_IsMainInwardsProcessedItem = false;
			AssertNoErrors(customsDataWL1.WB_IsMainInwardsProcessedItemInfo);
			AssertNoErrors(customsDataWL2.WB_IsMainInwardsProcessedItemInfo);

			workOrder.RunPreSaveValidation();
			AssertHasError(
				customsDataWL1.WB_IsMainInwardsProcessedItemInfo,
				"Should have at least one 'Is Main Processed Item' per Dynamic Work Order.");
			AssertHasError(
				customsDataWL2.WB_IsMainInwardsProcessedItemInfo,
				"Should have at least one 'Is Main Processed Item' per Dynamic Work Order.");

			customsDataWL1.WB_IsSecondaryInwardsProcessedItem = false;
			customsDataWL1.WB_IsMainInwardsProcessedItem = true;
			workOrder.RunPreSaveValidation();
			AssertNoErrors(customsDataWL1.WB_IsMainInwardsProcessedItemInfo);
			AssertNoErrors(customsDataWL2.WB_IsMainInwardsProcessedItemInfo);
		}

		public void TestIsMainInwardProcessedItem_MultipleInvalid()
		{
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			var workOrderLine1 = workOrder.Lines.AddNew();
			var workOrderLine2 = workOrder.Lines.AddNew();
			workOrderLine1.CustomsData.WB_IsMainInwardsProcessedItem = true;
			workOrderLine2.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;
			AssertNoErrors(workOrderLine1.CustomsData.WB_IsMainInwardsProcessedItemInfo);
			AssertNoErrors(workOrderLine2.CustomsData.WB_IsMainInwardsProcessedItemInfo);

			workOrderLine2.CustomsData.WB_IsSecondaryInwardsProcessedItem = false;
			workOrderLine2.CustomsData.WB_IsMainInwardsProcessedItem = true;
			AssertNoErrors(workOrderLine1.CustomsData.WB_IsMainInwardsProcessedItemInfo);
			AssertNoErrors(workOrderLine2.CustomsData.WB_IsMainInwardsProcessedItemInfo);

			workOrder.RunPreSaveValidation();
			AssertHasError(
				workOrderLine1.CustomsData.WB_IsMainInwardsProcessedItemInfo,
				"Should have only one 'Is Main Processed Item' per Dynamic Work Order.");
			AssertHasError(
				workOrderLine2.CustomsData.WB_IsMainInwardsProcessedItemInfo,
				"Should have only one 'Is Main Processed Item' per Dynamic Work Order.");

			workOrderLine2.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;
			workOrderLine2.CustomsData.WB_IsMainInwardsProcessedItem = false;
			workOrder.RunPreSaveValidation();
			AssertNoErrors(workOrderLine1.CustomsData.WB_IsMainInwardsProcessedItemInfo);
			AssertNoErrors(workOrderLine2.CustomsData.WB_IsMainInwardsProcessedItemInfo);

			// Should be allowed after finalization, due to splitting
			workOrder.WD_FinalisedDate = ZDateTimeOffset.Now;
			workOrderLine2.CustomsData.WB_IsSecondaryInwardsProcessedItem = false;
			workOrderLine2.CustomsData.WB_IsMainInwardsProcessedItem = true;
			workOrder.RunPreSaveValidation();
			AssertNoErrors(workOrderLine1.CustomsData.WB_IsMainInwardsProcessedItemInfo);
			AssertNoErrors(workOrderLine2.CustomsData.WB_IsMainInwardsProcessedItemInfo);
		}

		public void TestIsMainInwardProcessedItem_OrIsSecondaryInwardProcessedItemSet()
		{
			var error = "Every line on Dynamic Work Order must have either 'Is Main Inward Processed Item' or 'Is Secondary Inward Processed Item' set.";
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			var workOrderLine1 = workOrder.Lines.AddNew();
			var workOrderLine2 = workOrder.Lines.AddNew();
			workOrderLine1.IsMainInwardProcessedItem = true;
			workOrderLine2.IsSecondaryInwardProcessedItem = true;
			AssertNoErrors(workOrderLine1.IsMainInwardProcessedItemInfo);
			AssertNoErrors(workOrderLine1.IsSecondaryInwardProcessedItemInfo);
			AssertNoErrors(workOrderLine2.IsMainInwardProcessedItemInfo);
			AssertNoErrors(workOrderLine2.IsSecondaryInwardProcessedItemInfo);

			workOrderLine2.IsSecondaryInwardProcessedItem = false;
			AssertNoErrors(workOrderLine1.IsMainInwardProcessedItemInfo);
			AssertNoErrors(workOrderLine1.IsSecondaryInwardProcessedItemInfo);
			AssertHasError(workOrderLine2.IsMainInwardProcessedItemInfo, error);
			AssertHasError(workOrderLine2.IsSecondaryInwardProcessedItemInfo, error);

			workOrderLine2.IsSecondaryInwardProcessedItem = true;
			workOrderLine1.IsMainInwardProcessedItem = false;
			AssertHasError(workOrderLine1.IsMainInwardProcessedItemInfo, error);
			AssertHasError(workOrderLine1.IsSecondaryInwardProcessedItemInfo, error);
			AssertNoError(workOrderLine2.IsMainInwardProcessedItemInfo, error);
			AssertNoError(workOrderLine2.IsSecondaryInwardProcessedItemInfo, error);

			workOrderLine1.IsSecondaryInwardProcessedItem = true;
			AssertNoError(workOrderLine1.IsMainInwardProcessedItemInfo, error);
			AssertNoError(workOrderLine1.IsSecondaryInwardProcessedItemInfo, error);
			AssertNoError(workOrderLine2.IsMainInwardProcessedItemInfo, error);
			AssertNoError(workOrderLine2.IsSecondaryInwardProcessedItemInfo, error);
		}

		public void TestIsMainInwardProcessedItem_AndIsSecondaryInwardProcessedItem_CantBothBeSet()
		{
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			var workOrderLine = workOrder.Lines.AddNew();
			workOrderLine.IsMainInwardProcessedItem = true;
			AssertNoErrors(workOrderLine.IsMainInwardProcessedItemInfo);
			AssertNoErrors(workOrderLine.IsSecondaryInwardProcessedItemInfo);

			workOrderLine.IsSecondaryInwardProcessedItem = true;
			AssertHasError(
				workOrderLine.IsMainInwardProcessedItemInfo,
				"Every line on Dynamic Work Order must have either 'Is Main Inward Processed Item' or 'Is Secondary Inward Processed Item' set.");
			AssertHasError(
				workOrderLine.IsSecondaryInwardProcessedItemInfo,
				"Every line on Dynamic Work Order must have either 'Is Main Inward Processed Item' or 'Is Secondary Inward Processed Item' set.");

			workOrderLine.IsSecondaryInwardProcessedItem = false;
			AssertNoErrors(workOrderLine.IsMainInwardProcessedItemInfo);
			AssertNoErrors(workOrderLine.IsSecondaryInwardProcessedItemInfo);
		}

		#endregion
	}
}
