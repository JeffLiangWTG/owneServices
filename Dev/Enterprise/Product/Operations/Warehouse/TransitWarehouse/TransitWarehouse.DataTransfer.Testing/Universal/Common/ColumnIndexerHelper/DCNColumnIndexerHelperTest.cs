using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class DCNColumnIndexerHelperTest : ColumnIndexerHelperTest<WhsItemDispatchConsignment>
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
					var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
					if (type != null)
					{
						Helper.CreateAdditionalReference(dcn, number, type);
					}
					return dcn;
				},
				dcn => dcn.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(type).CE_EntryNum,
				(factory, indexer) => DCNColumnIndexerHelper.GetAdditionalReferenceByType(factory, indexer, type),
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
					var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
					Helper.CreateAdditionalReference(dcn, "S0000001", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
					Helper.CreateAdditionalReference(dcn, "WayBill1", WarehouseAdditionalReferenceTypes.Codes.WaybillNumber);
					return dcn;
				},
				dcn => dcn.AdditionalReferenceNumbers as BusinessObjectCollection,
				DCNColumnIndexerHelper.GetAdditionalReferences);
		}

		public void TestGetAdditionalReferences_NoAdditionalReference()
		{
			AssertGetRelatedIndexerCollection(
				() =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
					return dcn;
				},
				dcn => dcn.AdditionalReferenceNumbers as BusinessObjectCollection,
				DCNColumnIndexerHelper.GetAdditionalReferences);
		}

		#endregion

		#region TestGetShipmentNumber

		public void TestGetShipmentNumber_NotEmpty()
		{
			TestGetShipmentNumber("S00000001", "S00000001");
		}

		public void TestGetShipmentNumber_Empty()
		{
			TestGetShipmentNumber(null, "");
		}

		void TestGetShipmentNumber(string testShipmentNumber, string expected)
		{
			AssertGetValue(
				shipmentNumber =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
					if (shipmentNumber != null)
					{
						Helper.CreateAdditionalReference(dcn, shipmentNumber, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
					}
					return dcn;
				},
				dcn => dcn.ShipmentNumber,
				DCNColumnIndexerHelper.GetShipmentNumber,
				(input: testShipmentNumber, expected: expected));
		}

		#endregion

		public void TestGetFormattedReference()
		{
			AssertGetValue(
				data =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var dcn = Helper.CreateDispatchConsignment(data.ConsignmentId, warehouse.PK);
					dcn.WDC_JobID = data.JobId;
					return dcn;
				},
				dcn => dcn.FormattedReference,
				DCNColumnIndexerHelper.GetFormattedReference,
				(input: (ConsignmentId: "DCN1", JobId: "S0000001"), expected: "DCN1 (S0000001)"));
		}
	}
}
