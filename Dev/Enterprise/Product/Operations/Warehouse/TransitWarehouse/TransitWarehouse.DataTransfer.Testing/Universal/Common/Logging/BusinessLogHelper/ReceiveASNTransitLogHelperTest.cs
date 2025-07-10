using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class ReceiveASNTransitLogHelperTest : TransitLogTableHelperTest<WhsItemReceiveASN, ASNColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();
			var asn1 = Helper.CreateReceiveASN("asn0000001", TestWarehouse.PK);
			AddAdditionalReference(asn1.AdditionalReferenceNumbers, WarehouseAdditionalReferenceTypes.Codes.MasterBill, "MB1");
			var asn2 = Helper.CreateReceiveASN("asn0000002", TestWarehouse.PK);
			Factory.Save();

			var column = tableHelper.GetColumn(new WhsItemReceiveASN[] { asn1, asn2 }, TransitLogColumnIDs.ASNColumn.ASN);

			AssertEquals("ASN", column.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "MB1 (asn0000001)", "(asn0000002)" }, column.Values);
		}

		protected override ASNColumn GetDefaultColumn() => ASNColumn.ASN;

		protected override TransitLogTableHelper<WhsItemReceiveASN, ASNColumn> GetTableHelper() => new ReceiveASNTransitLogHelper();

		protected override (WhsItemReceiveASN BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var asnId = "ASN" + id.ToString().PadLeft(7, '0');
			var asn = Helper.CreateReceiveASN(asnId, TestWarehouse.PK);
			return (asn, $"({asnId})");
		}

		void AddAdditionalReference(Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers, string type, string entryNum)
		{
			var additionalReference = additionalReferenceNumbers.AddNew();
			additionalReference.CE_EntryType = type;
			additionalReference.CE_EntryNum = entryNum;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestWarehouse = Helper.CreateTRWWarehouse("WH1");
		}

		WhsWarehouse TestWarehouse { get; set; }
	}
}
