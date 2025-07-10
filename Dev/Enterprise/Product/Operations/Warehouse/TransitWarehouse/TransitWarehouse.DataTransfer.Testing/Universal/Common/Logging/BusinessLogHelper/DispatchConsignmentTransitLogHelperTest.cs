using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class DispatchConsignmentTransitLogHelperTest : TransitLogTableHelperTest<WhsItemDispatchConsignment, DCNColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();
			var dcn1 = Helper.CreateDispatchConsignment("S1000000", TestWarehouse.PK, jobID: "DC0000001");
			Factory.Save();

			var column = tableHelper.GetColumn(new WhsItemDispatchConsignment[] { dcn1 }, DCNColumn.DCN);

			AssertEquals("DCN", column.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "S1000000 (DC0000001)" }, column.Values);
		}

		protected override DCNColumn GetDefaultColumn() => DCNColumn.DCN;

		protected override TransitLogTableHelper<WhsItemDispatchConsignment, DCNColumn> GetTableHelper() => new DispatchConsignmentTransitLogHelper();

		protected override (WhsItemDispatchConsignment BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var dcnId = "S" + id.ToString().PadLeft(7, '0');
			var jobId = "DC" + id.ToString().PadLeft(7, '0');
			var dcn = Helper.CreateDispatchConsignment(dcnId, TestWarehouse.PK, jobID: jobId);
			return (dcn, $"{dcnId} ({jobId})");
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestWarehouse = Helper.CreateTRWWarehouse("WH1");
		}

		WhsWarehouse TestWarehouse { get; set; }
	}
}
