using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class RCNColumnIndexerHelperTest : ColumnIndexerHelperTest<WhsItemReceiveConsignment>
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
					var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
					if (type != null)
					{
						Helper.CreateAdditionalReference(rcn, number, type);
					}
					return rcn;
				},
				rcn => rcn.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(type).CE_EntryNum,
				(factory, indexer) => RCNColumnIndexerHelper.GetAdditionalReferenceByType(factory, indexer, type),
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
					var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
					Helper.CreateAdditionalReference(rcn, "S0000001", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
					Helper.CreateAdditionalReference(rcn, "WayBill1", WarehouseAdditionalReferenceTypes.Codes.WaybillNumber);
					return rcn;
				},
				rcn => rcn.AdditionalReferenceNumbers as BusinessObjectCollection,
				RCNColumnIndexerHelper.GetAdditionalReferences);
		}

		public void TestGetAdditionalReferences_NoAdditionalReference()
		{
			AssertGetRelatedIndexerCollection(
				() =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
					return rcn;
				},
				rcn => rcn.AdditionalReferenceNumbers as BusinessObjectCollection,
				RCNColumnIndexerHelper.GetAdditionalReferences);
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
					var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
					if (shipmentNumber != null)
					{
						Helper.CreateAdditionalReference(rcn, shipmentNumber, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
					}
					return rcn;
				},
				rcn => rcn.ShipmentNumber,
				RCNColumnIndexerHelper.GetShipmentNumber,
				(input: testShipmentNumber, expected: expected));
		}

		#endregion

		public void TestGetFormattedReference()
		{
			AssertGetValue(
				data =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var rcn = Helper.CreateReceiveConsignment(data.ConsignmentId, warehouse.PK);
					rcn.WRC_JobID = data.JobId;
					return rcn;
				},
				rcn => rcn.FormattedReference,
				RCNColumnIndexerHelper.GetFormattedReference,
				(input: (ConsignmentId: "RCN1", JobId: "S0000001"), expected: "RCN1 (S0000001)"));
		}
	}
}
