using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class AdditionalReferenceColumnIndexerHelperTest : ColumnIndexerHelperTest<CusEntryNumber>
	{
		public void TestGetFirstAdditionalReferenceByType_Matched()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");

			var rcn = CreateRCNWithAdditionalReferenceTypes(1, warehouse.PK, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber,
				WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			var indexer = GetIndexer(rcn);
			var refs = RCNColumnIndexerHelper.GetAdditionalReferences(UniversalFactory, indexer);
			AssertEquals("MAB001", AdditionalReferenceColumnIndexerHelper.GetFirstAdditionalReferenceByType(refs, WarehouseAdditionalReferenceTypes.Codes.MasterBill));
		}

		public void TestGetFirstAdditionalReferenceByType_NotMatched()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");

			var rcn = CreateRCNWithAdditionalReferenceTypes(1, warehouse.PK, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
			var indexer = GetIndexer(rcn);
			var refs = RCNColumnIndexerHelper.GetAdditionalReferences(UniversalFactory, indexer);
			AssertEquals("", AdditionalReferenceColumnIndexerHelper.GetFirstAdditionalReferenceByType(refs, WarehouseAdditionalReferenceTypes.Codes.MasterBill));
		}

		public void TestGetAdditionalReferencesByPK()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");

			var rcn1 = CreateRCNWithAdditionalReferenceTypes(1, warehouse.PK, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber,
				WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			var indexer1 = GetIndexer(rcn1);
			var refs1 = AdditionalReferenceColumnIndexerHelper.GetAdditionalReferencesByParent(UniversalFactory, rcn1.PK);

			var rcn2 = CreateRCNWithAdditionalReferenceTypes(2, warehouse.PK, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber,
				WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber,
				WarehouseAdditionalReferenceTypes.Codes.MarksAndNumbers);
			var indexer2 = GetIndexer(rcn2);
			var refs2 = AdditionalReferenceColumnIndexerHelper.GetAdditionalReferencesByParent(UniversalFactory, rcn2.PK);

			var typeAndNumbers1 = refs1.Select(c => (Type: c.GetValue(CusEntryNumSchema.CE_EntryType), Number: c.GetValue(CusEntryNumSchema.CE_EntryNum)));
			var expected1 = new List<(ZString Type, ZString Number)> {
				(Type: WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, Number: "FSH001"),
				(Type: WarehouseAdditionalReferenceTypes.Codes.MasterBill, Number: "MAB001")
			};
			AssertContainsExactElementsInAnyOrder(expected1, typeAndNumbers1);

			var typeAndNumbers2 = refs2.Select(c => (Type: c.GetValue(CusEntryNumSchema.CE_EntryType), Number: c.GetValue(CusEntryNumSchema.CE_EntryNum)));
			var expected2 = new List<(ZString Type, ZString Number)> {
				(Type: WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, Number: "FSH002"),
				(Type: WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, Number: "INV002"),
				(Type: WarehouseAdditionalReferenceTypes.Codes.MarksAndNumbers, Number: "MAR002")
			};
			AssertContainsExactElementsInAnyOrder(expected2, typeAndNumbers2);
		}

		WhsItemReceiveConsignment CreateRCNWithAdditionalReferenceTypes(int number, ZGuid warehousePK, params string[] types)
		{
			var rcn = Helper.CreateReceiveConsignment($"RCN{number}", warehousePK);
			foreach (var type in types)
			{
				Helper.CreateAdditionalReference(rcn, type + $"00{number}", type);
			}
			Factory.Save();
			return rcn;
		}
	}
}
