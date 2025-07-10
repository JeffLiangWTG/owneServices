using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContactWebWarehouseEligibility))]
	sealed class OrgContactWebWarehouseEligibilityTest : NonPersistentBusinessObjectTestCase
	{
		#region TestConstructors

		public void TestConstructor_NonNullWarehouse()
		{
			var contact = Factory.New<OrgContact>();
			AssertExceptionThrown<ArgumentNullException>(() => new OrgContactWebWarehouseEligibility(Factory, null, contact, null));
		}

		public void TestConstructor_NonNullContact()
		{
			var warehouse = Factory.New<IWhsWarehouse>();
			AssertExceptionThrown<ArgumentNullException>(() => new OrgContactWebWarehouseEligibility(Factory, warehouse, null, null));
		}

		#endregion

		#region TestWarehouseName

		public void TestWarehouseName()
		{
			var contact = Factory.New<OrgContact>();
			var warehouseWithJustCode = Factory.New<IWhsWarehouse>();
			warehouseWithJustCode.WW_WarehouseCode = "XXX";

			var item1 = new OrgContactWebWarehouseEligibility(Factory, warehouseWithJustCode, contact, null);
			AssertEquals("XXX", item1.WarehouseName);

			var warehouseWithCodeAndName = Factory.New<IWhsWarehouse>();
			warehouseWithCodeAndName.WW_WarehouseCode = "YYY";
			warehouseWithCodeAndName.WW_WarehouseName = "Some name";
			var item2 = new OrgContactWebWarehouseEligibility(Factory, warehouseWithCodeAndName, contact, null);
			AssertEquals("YYY (Some name)", item2.WarehouseName);
		}

		#endregion

		#region TestWarehousePK

		public void TestWarehousePK()
		{
			var contact = Factory.New<OrgContact>();
			var warehouse = Factory.New<IWhsWarehouse>();

			var item = new OrgContactWebWarehouseEligibility(Factory, warehouse, contact, null);
			AssertEquals(warehouse.PK, item.WarehousePK);
		}

		#endregion

		#region TestIsGranted

		public void TestIsGranted()
		{
			var contact = Factory.New<OrgContact>();
			var warehouse = Factory.New<IWhsWarehouse>();

			var item = new OrgContactWebWarehouseEligibility(Factory, warehouse, contact, null);
			AssertEquals(true, item.IsGranted);

			item.IsGranted = false;
			AssertEquals(false, item.IsGranted);

			item.IsGranted = true;
			AssertEquals(true, item.IsGranted);

			// Pivot creation / deletion logic is tested in OrgContactWebWarehouseEligibilityCollectionTest TestPivotsCreatedOrDeletedWhenAccessChanges
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var contact = Factory.New<OrgContact>();
			var warehouse = Factory.New<IWhsWarehouse>();
			return new OrgContactWebWarehouseEligibility(Factory, warehouse, contact, null);
		}

		#endregion
	}
}
