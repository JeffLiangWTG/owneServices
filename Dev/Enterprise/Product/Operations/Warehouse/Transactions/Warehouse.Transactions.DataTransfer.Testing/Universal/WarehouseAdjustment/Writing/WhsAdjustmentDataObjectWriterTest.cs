using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsAdjustmentDataObjectWriterTest : WhsDocketDataObjectWriterTest<WhsAdjustment, WhsAdjustmentDataObjectWriter>
	{
		#region TestBasicAdjustmentLevelFieldMappings

		public void TestBasicAdjustmentLevelFieldMappings()
		{
			var helper = new WhsTestHelperFunctions(Factory.BOFactory);
			var warehouse = helper.CreateWarehouse("Coolhouse", "WHS", "A");
			var client = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var adjustmentBO = helper.CreateWhsAdjustment(client, warehouse);
			adjustmentBO.WD_ExternalReference = "ADJUSTME";
			adjustmentBO.WD_DocketStatus = "ENT";

			var writer = new WhsAdjustmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, adjustmentBO)));
			var adjustmentDataObject = writer.GetDataObject(adjustmentBO);

			AssertNotNull("adjustmentDataObject", adjustmentDataObject);

			CombineAssertions(delegate
			{
				var orderDataObject = adjustmentDataObject.Order;
				AssertEquals("orderDataObject.OrderNumber", "ADJUSTME", orderDataObject.OrderNumber);
				AssertEquals("orderDataObject.Status.Code", "ENT", orderDataObject.Status.Code);
				AssertEquals("orderDataObject.Status.Description", "Entered (Saved)", orderDataObject.Status.Description);
				AssertEquals("orderDataObject.Warehouse.Code", "WHS", orderDataObject.Warehouse.Code);
				AssertEquals("orderDataObject.Warehouse.Name", "Coolhouse", orderDataObject.Warehouse.Name);
			});

			AssertOrganizationBO_CRAHOLSYD("ConsignorDocumentaryAddress", adjustmentDataObject.OrganizationAddressCollection[0], "ConsignorDocumentaryAddress");
		}

		#endregion

		#region TestTotalUnitsExceedsMaximumIntQuantity

		public void TestTotalUnitsExceedsMaximumIntQuantity()
		{
			var whsDocketBO = GetNewDocket();
			whsDocketBO.WD_TotalUnits = int.MaxValue + 5000m;

			var notifications = new TestNotificationBuffer();

			AssertNoExceptionThrown(() => GetNewDataObjectWriter(whsDocketBO, notifications).GetDataObject(whsDocketBO));

			AssertEquals(false, notifications.HasWarnings);
			AssertEquals(false, notifications.HasErrors);
		}

		#endregion

		#region TestAdjustmentLines

		public void TestAdjustmentLines()
		{
			var adjustment = WhsAdjustmentLineDataObjectWriterTest.SetupAdjustmentLine(Factory.BOFactory).Docket;
			var writer = new WhsAdjustmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, adjustment)));
			var adjustmentDataObject = writer.GetDataObject(adjustment);

			AssertNotNull("adjustmentDataObject", adjustmentDataObject);
			AssertEquals("adjustmentDataObject.Order.OrderLineCollection.Count", 1, adjustmentDataObject.Order.OrderLineCollection.Count);

			CombineAssertions(delegate
			{
				var adjustmentLineDataObject = adjustmentDataObject.Order.OrderLineCollection[0];
				WhsAdjustmentLineDataObjectWriterTest.AssertContents(adjustmentLineDataObject);
			});
		}

		#endregion

		#region Implementation

		protected override WhsAdjustment GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsAdjustment>();
		}

		protected override WhsAdjustmentDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO, INotifications notifications)
			=> new WhsAdjustmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO) { Notifications = notifications }));

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		#endregion
	}
}
