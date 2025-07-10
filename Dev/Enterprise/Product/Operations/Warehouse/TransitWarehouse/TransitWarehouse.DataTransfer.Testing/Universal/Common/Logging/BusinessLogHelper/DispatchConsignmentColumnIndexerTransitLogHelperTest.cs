using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class DispatchConsignmentColumnIndexerTransitLogHelperTest : TransitLogTableHelperTest<IColumnIndexer, DCNIndexerColumn>
	{
		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();

			var dcn = Helper.CreateDispatchConsignment("S0000001", TestWarehouse.PK, jobID: "DC0000001");

			Factory.Save();

			IColumnIndexer[] indexers = { GetIndexer(dcn) };

			var columnDCN = tableHelper.GetColumn(indexers, DCNIndexerColumn.DCN);

			AssertEquals("DCN", columnDCN.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "S0000001 (DC0000001)" }, columnDCN.Values);
		}

		protected override (IColumnIndexer BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var dcnId = "S" + id.ToString().PadLeft(7, '0');
			var jobId = "DC" + id.ToString().PadLeft(7, '0');
			var dcn = Helper.CreateDispatchConsignment(dcnId, TestWarehouse.PK, jobID: jobId);
			dcn.WDC_JobID = jobId;
			Factory.Save();
			return (GetIndexer(dcn), $"{dcnId} ({jobId})");
		}

		protected override DCNIndexerColumn GetDefaultColumn() => DCNIndexerColumn.DCN;

		protected override TransitLogTableHelper<IColumnIndexer, DCNIndexerColumn> GetTableHelper() => new DispatchConsignmentColumnIndexerTransitLogHelper(UniversalFactory);

		protected override void SetUp()
		{
			base.SetUp();
			UniversalFactory = new UniversalObjectFactory();
			TestWarehouse = Helper.CreateTRWWarehouse("WH1");
		}

		protected IColumnIndexer GetIndexer(BusinessObject bo)
		{
			return UniversalFactory.RowFactory.LoadFromPK(bo.TableName, bo.PK) as IColumnIndexer;
		}

		WhsWarehouse TestWarehouse { get; set; }

		protected UniversalObjectFactory UniversalFactory { get; set; }
	}
}
