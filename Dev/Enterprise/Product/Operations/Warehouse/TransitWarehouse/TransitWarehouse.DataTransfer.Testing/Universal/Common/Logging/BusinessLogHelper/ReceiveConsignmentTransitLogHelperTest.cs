using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class ReceiveConsignmentTransitLogHelperTest : TransitLogTableHelperTest<WhsItemReceiveConsignment, RCNColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();
			var rcn1 = Helper.CreateReceiveConsignment("S1000000", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "RC0000001");
			var rcn2 = Helper.CreateReceiveConsignment("RCN1", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "RC0000002");
			Factory.Save();

			var column = tableHelper.GetColumn(new WhsItemReceiveConsignment[] { rcn1, rcn2 }, TransitLogColumnIDs.RCNColumn.RCN);

			AssertEquals("RCN", column.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "S1000000 (RC0000001)", "RCN1 (RC0000002)" }, column.Values);
		}

		protected override RCNColumn GetDefaultColumn() => RCNColumn.RCN;

		protected override TransitLogTableHelper<WhsItemReceiveConsignment, RCNColumn> GetTableHelper() => new ReceiveConsignmentTransitLogHelper();

		protected override (WhsItemReceiveConsignment BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var rcnId = "S" + id.ToString().PadLeft(7, '0');
			var jobId = "RC" + id.ToString().PadLeft(7, '0');
			var rcn = Helper.CreateReceiveConsignment(rcnId, serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: jobId);
			return (rcn, $"{rcnId} ({jobId})");
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestWarehouse = Helper.CreateTRWWarehouse("WH1");
		}

		WhsWarehouse TestWarehouse { get; set; }
	}
}
