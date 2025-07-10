namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsBondedWarehouseAttributeValidationForComponentOrdersTest : WhsBusinessObjectValidationTestCase
	{
		public void TestEntryNumberNotMandatoryForWorkOrders()
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			workOrder.WD_IsInwardsProcessingJob = true;

			var orderLine = workOrder.Lines.AddNew();
			var attrib = Factory.New<WhsBondedWarehouseAttribute>();
			attrib.SetParent(orderLine);
			AssertType<WhsBondedWarehouseAttributeValidationForComponentOrders>(attrib.Validation);

			attrib.Validation.ValidateWB_EntryKey();
			AssertNoErrors(attrib.WB_EntryKeyInfo);
		}

		public void TestEntryNumberNotMandatoryForDynamicWorkOrders()
		{
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			var orderLine = workOrder.Lines.AddNew();
			var attrib = Factory.New<WhsBondedWarehouseAttribute>();
			attrib.SetParent(orderLine);
			AssertType<WhsBondedWarehouseAttributeValidationForDynamicWorkOrders>(attrib.Validation);

			attrib.Validation.ValidateWB_EntryKey();
			AssertNoErrors(attrib.WB_EntryKeyInfo);
		}
	}
}
