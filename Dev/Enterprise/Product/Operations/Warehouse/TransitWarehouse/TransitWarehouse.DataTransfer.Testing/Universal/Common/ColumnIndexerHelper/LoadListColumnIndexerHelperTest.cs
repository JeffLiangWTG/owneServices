using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class LoadListColumnIndexerHelperTest : ColumnIndexerHelperTest<WhsItemDispatchLoadList>
	{
		#region TestGetAdditionalReferenceByType

		public void TestGetAdditionalReferenceByType_ForwardingShipmentNumber()
		{
			TestGetAdditionalReferenceByType(WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
		}

		public void TestGetAdditionalReferenceByType_MasterBill()
		{
			TestGetAdditionalReferenceByType(WarehouseAdditionalReferenceTypes.Codes.MasterBill);
		}

		public void TestGetAdditionalReferenceByType_Empty()
		{
			TestGetAdditionalReferenceByType(null);
		}

		void TestGetAdditionalReferenceByType(string type)
		{
			AssertGetValue(
				number =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var ddl = Helper.CreateDispatchLoadList("ddl1", warehouse.PK);
					if (type != null)
					{
						Helper.CreateAdditionalReference(ddl, number, type);
					}
					return ddl;
				},
				ddl => ddl.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(type).CE_EntryNum,
				(factory, indexer) => LoadListColumnIndexerHelper.GetAdditionalReferenceByType(factory, indexer, type),
				(input: "S0000001", expected: type != null ? "S0000001" : ""));
		}

		#endregion

		#region TestGetAdditionalReferences

		public void TestGetAdditionalReferences()
		{
			AssertGetRelatedIndexerCollection(
				() =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var ddl = Helper.CreateDispatchLoadList("ddl1", warehouse.PK);
					Helper.CreateAdditionalReference(ddl, "S0000001", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
					Helper.CreateAdditionalReference(ddl, "WayBill1", WarehouseAdditionalReferenceTypes.Codes.WaybillNumber);
					return ddl;
				},
				ddl => ddl.AdditionalReferenceNumbers as BusinessObjectCollection,
				LoadListColumnIndexerHelper.GetAdditionalReferences);
		}

		public void TestGetAdditionalReferences_NoAdditionalReference()
		{
			AssertGetRelatedIndexerCollection(
				() =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var ddl = Helper.CreateDispatchLoadList("ddl1", warehouse.PK);
					return ddl;
				},
				ddl => ddl.AdditionalReferenceNumbers as BusinessObjectCollection,
				LoadListColumnIndexerHelper.GetAdditionalReferences);
		}

		#endregion

		public void TestGetFormattedReference()
		{
			AssertGetValue(
				input =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var ddl = Helper.CreateDispatchLoadList(input.Reference, warehouse.PK);
					Helper.CreateAdditionalReference(ddl, input.MasterBill, WarehouseAdditionalReferenceTypes.Codes.MasterBill);
					return ddl;
				},
				ddl => ddl.FormattedReference,
				LoadListColumnIndexerHelper.GetFormattedReference,
				(input: (MasterBill: "S0001", Reference: "DDL0000001"), expected: "S0001 (DDL0000001)"));
		}

		public void TestGetFormattedReference_NoMasterBill()
		{
			AssertGetValue(
				reference =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var ddl = Helper.CreateDispatchLoadList(reference, warehouse.PK);
					return ddl;
				},
				ddl => ddl.FormattedReference,
				LoadListColumnIndexerHelper.GetFormattedReference,
				(input: "DDL0000001", expected: "(DDL0000001)"));
		}
	}
}
