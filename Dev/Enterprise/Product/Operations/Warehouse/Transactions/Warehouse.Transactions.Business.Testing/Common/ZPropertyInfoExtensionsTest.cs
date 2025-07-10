using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Common
{
	class ZPropertyInfoExtensionsTest : TestCaseWithFactory
	{
		public void TestConvertOffsetPropertyToTimeZone()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var warehouseTimeZone = warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;
			var calculationTimeZone = warehouseTimeZone.GetCalculationTimeZone();

			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var someWarehouseOffset = warehouse.GetWarehouseBranchDateTimeOffset(new ZDateTime(2024, 02, 02, 12, 30, 00));
			var originalDummyOffset = new ZDateTimeOffset(2024, 02, 02, 12, 30, 00, someWarehouseOffset.Offset.Add(TimeSpan.FromHours(3)));
			dummy.Z0_DateTimeOffset = originalDummyOffset;

			dummy.Z0_DateTimeOffsetInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone);

			AssertEquals("DateTimeOffsets should be equal.", originalDummyOffset, dummy.Z0_DateTimeOffset);
			AssertEquals("DateTimeOffsets Offsets should be equal.", someWarehouseOffset.Offset, dummy.Z0_DateTimeOffset.Offset);
		}

		public void TestConvertOffsetPropertyToTimeZone_NotInDatabase()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var warehouseTimeZone = warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;
			var calculationTimeZone = warehouseTimeZone.GetCalculationTimeZone();

			var dummy = Factory.New<DummyBusinessObject>();

			var someWarehouseOffset = warehouse.GetWarehouseBranchDateTimeOffset(new ZDateTime(2024, 02, 02, 12, 30, 00));
			var originalDummyOffset = new ZDateTimeOffset(2024, 02, 02, 12, 30, 00, someWarehouseOffset.Offset.Add(TimeSpan.FromHours(3)));
			dummy.Z0_DateTimeOffset = originalDummyOffset;

			dummy.Z0_DateTimeOffsetInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone);

			AssertEquals("DateTimeOffsets should be equal.", originalDummyOffset, dummy.Z0_DateTimeOffset);
			AssertEquals("DateTimeOffsets Offsets should be equal.", someWarehouseOffset.Offset, dummy.Z0_DateTimeOffset.Offset);
		}

		public void TestConvertOffsetPropertyToTimeZone_HasError()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var warehouseTimeZone = warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;
			var calculationTimeZone = warehouseTimeZone.GetCalculationTimeZone();

			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var someWarehouseOffset = warehouse.GetWarehouseBranchDateTimeOffset(new ZDateTime(2024, 02, 02, 12, 30, 00));
			var originalDummyOffset = new ZDateTimeOffset(2024, 02, 02, 12, 30, 00, someWarehouseOffset.Offset.Add(TimeSpan.FromHours(3)));
			dummy.Z0_DateTimeOffset = originalDummyOffset;

			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_DateTimeOffsetInfo.AddError("Something looks wrong!");
			}

			dummy.Z0_DateTimeOffsetInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone);

			AssertEquals("DateTimeOffsets should be equal.", originalDummyOffset, dummy.Z0_DateTimeOffset);
			AssertEquals("DateTimeOffsets Offsets should not be updated.", originalDummyOffset.Offset, dummy.Z0_DateTimeOffset.Offset);
		}

		public void TestConvertOffsetPropertyToTimeZone_WrongPropertyType()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var warehouseTimeZone = warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;
			var calculationTimeZone = warehouseTimeZone.GetCalculationTimeZone();

			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			AssertExceptionThrown<InvalidOperationException>(() => dummy.Z0_SmallDateTimeInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone));
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;
	}
}
