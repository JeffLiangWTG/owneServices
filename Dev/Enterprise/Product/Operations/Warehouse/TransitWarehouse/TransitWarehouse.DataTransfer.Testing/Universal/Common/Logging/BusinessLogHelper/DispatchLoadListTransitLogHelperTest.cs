using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class DispatchLoadListTransitLogHelperTest : TransitLogTableHelperTest<WhsItemDispatchLoadList, LoadListColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();
			var loadList1 = Helper.CreateDispatchLoadList("DLL0000001", TestWarehouse.PK);
			Helper.CreateAdditionalReference(loadList1, "MB1", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			var loadList2 = Helper.CreateDispatchLoadList("DLL0000002", TestWarehouse.PK);
			Factory.Save();

			var column = tableHelper.GetColumn(new WhsItemDispatchLoadList[] { loadList1, loadList2 }, TransitLogColumnIDs.LoadListColumn.LoadList);

			AssertEquals("Load List", column.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "MB1 (DLL0000001)", "(DLL0000002)" }, column.Values);
		}

		protected override ZString GetDefaultTitle() => "Load List";
		protected override LoadListColumn GetDefaultColumn() => LoadListColumn.LoadList;

		protected override TransitLogTableHelper<WhsItemDispatchLoadList, LoadListColumn> GetTableHelper() => new DispatchLoadListTransitLogHelper();

		protected override (WhsItemDispatchLoadList BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var dllId = "DLL" + id.ToString().PadLeft(7, '0');
			var dll = Helper.CreateDispatchLoadList(dllId, TestWarehouse.PK);
			return (dll, $"({dllId})");
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestWarehouse = Helper.CreateTRWWarehouse("WH1");
		}

		WhsWarehouse TestWarehouse { get; set; }
	}
}
