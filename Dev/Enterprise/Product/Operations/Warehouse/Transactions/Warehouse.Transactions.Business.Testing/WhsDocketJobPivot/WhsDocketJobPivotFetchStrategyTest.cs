using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsDocketJobPivotFetchStrategyTest : TestCaseWithFactory
	{
		#region TestFetchForLoad

		public void TestFetchForLoad()
		{
			var order1 = Factory.NewWithValidTestData<WhsOrder>();
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var pivot1 = Factory.New<WhsDocketJobPivot>();
			pivot1.WV_DocketType = DocketType.Codes.Order;
			pivot1.WV_WD_Docket = order1.PK;
			pivot1.WV_ParentId = shipment.PK;
			pivot1.WV_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var order2 = Factory.NewWithValidTestData<WhsOrder>();
			var pivot2 = Factory.New<WhsDocketJobPivot>();
			pivot2.WV_DocketType = DocketType.Codes.Order;
			pivot2.WV_WD_Docket = order2.PK;
			pivot2.WV_ParentId = shipment.PK;
			pivot2.WV_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var dbHitsBeforeLoad = otherFactory.GetTableHitCount(WhsDocketSchema.Constants.TableName);
			var pivotsInOtherFactory = otherFactory.Load<WhsDocketJobPivot>(new ZQuery());
			AssertEquals(dbHitsBeforeLoad, otherFactory.GetTableHitCount(WhsDocketSchema.Constants.TableName));

			foreach (var pivot in pivotsInOtherFactory)
			{
				var orderInOtherFactory = otherFactory.Load<WhsOrder>(pivot.WV_WD_Docket);
				AssertEquals(dbHitsBeforeLoad + 1, otherFactory.GetTableHitCount(WhsDocketSchema.Constants.TableName));
			}
		}

		#endregion
	}
}
